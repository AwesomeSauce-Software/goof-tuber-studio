using NativeWebSocket;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[System.Serializable]
public class SessionInformation
{
    public SessionPayload SessionPayload;
    public List<string> VerifiedUserIDs;
    public string DiscordID;

    public SessionInformation()
    {
        VerifiedUserIDs = new List<string>();
    }
}

public class NetworkManager : MonoBehaviour
{
    public bool HasSession => sessionInfo?.SessionPayload != null || sessionInfo.SessionPayload.session_id.Length > 0;

    [SerializeField] SpriteInternalManager spriteManager;
    [SerializeField] CharacterAnimator selfCharacter;
    [SerializeField] CharacterManager characterManager;
    [Header("UI References")]
    [SerializeField] InputField userIDInputField;
    [SerializeField] InputField friendIDInputField;
    [SerializeField] InputField verifyIDInputField;
    [Header("Network Options")]
    [SerializeField] string networkFolder;
    [SerializeField] string sessionFileName;
    [SerializeField] string uri;
    [SerializeField] float updateConnectionTime;

    float updateConnectionTimer;

    WebSocket masterSocket;
    System.DateTime apiRequestTime;
    List<double> apiResponseTimes;

    string networkPath;
    string sessionFilePath;

    SessionInformation sessionInfo;
    List<VerifiedUser> verifiedUsers;

    #region Callbacks
    void InitiateSessionCallback(long result, string data)
    {
        LogEx.Log(LogTopics.NetworkGeneral, $"Verification ID result: {result} {data}");
    }

    void RquestSessionCallback(long result, VerifiedUser user)
    {
        LogEx.Log(LogTopics.NetworkGeneral, $"Request session user ID result: {result}");
        if (result == 200)
        {
            if (!sessionInfo.VerifiedUserIDs.Contains(user.UserID))
                sessionInfo.VerifiedUserIDs.Add(user.UserID);
        }
        else
        {
            Destroy(user.Character);
            verifiedUsers.RemoveAll(u => u.UserID == user.UserID);
        }
    }

    void GetSessionIDCallback(long result, string data)
    {
        LogEx.Log(LogTopics.NetworkGeneral, $"Session ID result: {result} {data}");
        if (result == 200)
        {
            sessionInfo.SessionPayload = JsonUtility.FromJson<SessionPayload>(data);
            SaveCache();
        }
    }

    void UploadAvatarsCallback(long result, string data)
    {
        LogEx.Log(LogTopics.NetworkAvatars, $"Uploading Sprites result: {result} {data}");
    }

    void GetAvatarsCallback(long result, VerifiedUser user, string data)
    {
        LogEx.Log(LogTopics.NetworkAvatars, $"Get Avatars result: {result} for ID {user.UserID}");
        if (result == 200)
        {
            var avatarPayload = JsonUtility.FromJson<AvatarPayload>(data);

            if (user.Character != null)
            {
                user.Character.LoadAvatarPayload(avatarPayload);
            }
            else
            {
                user.Character = characterManager.CreateExtCharacter(user.UserID, avatarPayload);
            }

#if UNITY_EDITOR
            foreach (var serializedAvatar in avatarPayload.avatars)
            {
                LogEx.Log(LogTopics.NetworkAvatars, serializedAvatar.filename);
            }
#endif
        }
    }

    void ValidateSessionCallback(long result, string data)
    {
        LogEx.Log(LogTopics.NetworkGeneral, $"Validate Session result: {result}");
        if (result == 200)
        {
            InitializeWebsocket();
        }
        else
        {
            sessionInfo.SessionPayload = null;
            sessionInfo.VerifiedUserIDs.Clear(); 
            RemoveVerifiedUsers();
            SaveCache();
        }
    }

    void PingAPICallback(long result, string data)
    {
        var apiResponseTime = System.DateTime.Now - apiRequestTime;
        LogEx.Log(LogTopics.NetworkGeneral, $"API Ping: {result} {apiResponseTime.TotalMilliseconds}ms");
        apiResponseTimes.Add(apiResponseTime.TotalMilliseconds);
    }
    #endregion

    #region Requests
    public void InitiateSession()
    {
        StartCoroutine(NetworkHelper.GetRequest(InitiateSessionCallback, uri, $"verify/{userIDInputField.text}"));
    }

    public void RequestSession()
    {
        if (HasSession)
        {
            var newUser = AddUserID(friendIDInputField.text);
            StartCoroutine(NetworkHelper.SessionRequest(RquestSessionCallback, newUser, uri, $"request-session/{sessionInfo.SessionPayload.session_id}/{friendIDInputField.text}"));
        }
    }

    public void GetSessionID()
    {
        StartCoroutine(NetworkHelper.GetRequest(GetSessionIDCallback, uri, $"verify/{userIDInputField.text}/{verifyIDInputField.text}"));
    }

    public void GetAvatars()
    {
        if (HasSession && verifiedUsers.Count > 0)
        {
            foreach (var verifiedUser in verifiedUsers)
            {
                GetAvatar(verifiedUser);
            }
        }
    }

    public void GetAvatar(VerifiedUser user)
    {
        LogEx.Log(LogTopics.NetworkAvatars, $"Attempting to get avatars from {user.UserID}");
        StartCoroutine(NetworkHelper.GetUserData(GetAvatarsCallback, user, uri, $"get-avatars/{sessionInfo.SessionPayload.session_id}/{user.UserID}"));
    }

    public void UploadAvatars()
    {
        if (HasSession)
        {
            var cachedSpritePaths = spriteManager.CachedSpritePaths;
            var avatarPayload = new AvatarPayload();

            avatarPayload.avatars = new AvatarInternalPayload[cachedSpritePaths.Count];
            for (int i = 0; i < avatarPayload.avatars.Length; ++i)
            {
                avatarPayload.avatars[i] = new AvatarInternalPayload();

                avatarPayload.avatars[i].base64 = System.Convert.ToBase64String(File.ReadAllBytes(cachedSpritePaths[i]));
                avatarPayload.avatars[i].filename = Path.GetFileName(cachedSpritePaths[i]);
            }

            string uploadJSON = JsonUtility.ToJson(avatarPayload);

            StartCoroutine(NetworkHelper.PostRequestRaw(UploadAvatarsCallback, uri, $"upload-avatar/{sessionInfo.SessionPayload.session_id}", uploadJSON));
        }
    }

    public void ValidateSession()
    {
        if (HasSession)
        {
            StartCoroutine(NetworkHelper.GetRequest(ValidateSessionCallback, uri, $"validsession/{sessionInfo.SessionPayload.session_id}"));
        }
    }

    public void PingAPI()
    {
        apiRequestTime = System.DateTime.Now;
        StartCoroutine(NetworkHelper.GetRequest(PingAPICallback, uri, "ping"));
    }
    #endregion

    #region Websockets
    public async void InitializeWebsocket()
    {
        List<string> userIDs = new List<string>();
        foreach (var verifiedUser in verifiedUsers)
            userIDs.Add(verifiedUser.UserID);
        string url = "ws://" + uri + "/websocket/" + sessionInfo.SessionPayload.session_id + '/' + string.Join(",", userIDs);
        LogEx.Log(LogTopics.NetworkWebsocketGeneral, $"Initializing Websocket: {url}");

        masterSocket = new WebSocket(url);

        masterSocket.OnOpen += () =>
        {
            LogEx.Log(LogTopics.NetworkWebsocketGeneral, "Websocket Opened");
        };
        masterSocket.OnError += (err) =>
        {
            LogEx.Error(LogTopics.NetworkWebsocketGeneral, $"Websocket Error: {err}");
        };
        masterSocket.OnMessage += ReceiveWebsocketData;

        InvokeRepeating("SendWebsocketData", 0.0f, 0.01f);

        await masterSocket.Connect();
    }

    void ReceiveWebsocketData(byte[] data)
    {
        string serializedData = System.Text.Encoding.UTF8.GetString(data);
        LogEx.Log(LogTopics.NetworkWebsocketReceiving, $"Received Websocket Data: {serializedData}");

        // json utility is ass (AwesomeSauce Software) :)
        if (serializedData.Length > 1 && serializedData.StartsWith("{"))
        {
            var activityPayload = JsonUtility.FromJson<ReceiveActivitiyPayload>(serializedData);
            if (activityPayload.data.Length > 0)
            {
                LogEx.Log(LogTopics.NetworkWebsocketReceiving, $"Activity Payload: {activityPayload.data[0].userid} {activityPayload.data[0].activity.voice_activity} {activityPayload.data[0].activity.action}");

                var verifiedUser = verifiedUsers.Find(u => u.UserID == activityPayload.data[0].userid);
                if (verifiedUser != null && verifiedUser.Character != null)
                {
                    verifiedUser.Character.MeanVolume = activityPayload.data[0].activity.voice_activity;
                }
            }
        }
    }

    async void SendWebsocketData()
    {
        if (masterSocket.State == WebSocketState.Open)
        {
            var activityPayload = new ActivityPayload();
            activityPayload.voice_activity = selfCharacter.MeanVolume;
            activityPayload.action = selfCharacter.CurrentExpressionName;
            LogEx.Log(LogTopics.NetworkWebsocketSending, $"Sending Websocket Data: {activityPayload.voice_activity}");

            await masterSocket.SendText("SEND " + JsonUtility.ToJson(activityPayload));
        }
        else if (masterSocket.State == WebSocketState.Closed)
        {
            CancelInvoke("PollWebsocketData");
        }
    }
    #endregion

    public VerifiedUser AddUserID(string userID)
    {
        var user = new VerifiedUser(userID);
        user.Character = characterManager.CreateExtCharacter(userID);
        verifiedUsers.Add(user);

        return user;
    }

    public void SaveCache()
    {
        List<string> userIDs = new List<string>();
        foreach (var verifiedUser in verifiedUsers)
            userIDs.Add(verifiedUser.UserID);
        sessionInfo.VerifiedUserIDs = userIDs;

        File.WriteAllText(sessionFilePath, JsonUtility.ToJson(sessionInfo));
    }

    void RemoveVerifiedUsers()
    {
        foreach (var verifiedUser in verifiedUsers)
        {
            if (verifiedUser.Character != null)
                Destroy(verifiedUser.Character);
        }
        verifiedUsers.Clear();
    }

    void UpdateConnections()
    {
        updateConnectionTimer += Time.deltaTime;
        if (updateConnectionTimer > updateConnectionTime)
        {
            updateConnectionTimer = 0.0f;

            GetAvatars();
        }
    }

    void LoadCache()
    {
        networkPath = DataSystem.CreateSpace(networkFolder);
        sessionFilePath = networkPath + sessionFileName;

        sessionInfo = new SessionInformation();
        verifiedUsers = new List<VerifiedUser>();
        if (File.Exists(sessionFilePath))
        {
            var serializedSessionPayload = File.ReadAllText(sessionFilePath);
            sessionInfo = JsonUtility.FromJson<SessionInformation>(serializedSessionPayload);

            if (HasSession)
            {
                foreach (var verifiedUserID in sessionInfo.VerifiedUserIDs)
                    AddUserID(verifiedUserID);

                ValidateSession();
            }
        }
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (masterSocket != null)
            masterSocket.DispatchMessageQueue();
#endif
        UpdateConnections();
    }

    void Start()
    {
        LoadCache();
    }

    void Awake()
    {
        updateConnectionTimer = updateConnectionTime;
        apiResponseTimes = new List<double>();
    }
}
