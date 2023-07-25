using NativeWebSocket;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    public bool HasSession => sessionInfo?.SessionPayload != null && sessionInfo.SessionPayload.session_id.Length > 0;

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

    WebSocket websocketSender;
    System.DateTime apiRequestTime;
    System.DateTime apiResponseTime;

    string networkPath;
    string sessionFilePath;

    SessionInformation sessionInfo;
    List<VerifiedUser> verifiedUsers;

    #region Callbacks
    void InitiateSessionCallback(long result, string data)
    {
        LogEx.Log(LogTopics.Networking, $"Verification ID result: {result} {data}");
    }

    void RquestSessionCallback(long result, VerifiedUser user)
    {
        LogEx.Log(LogTopics.Networking, $"Request session user ID result: {result}");
        if (result == 200)
        {
            AddWebsocketReceiver(user, "ws://" + uri + "/receive-data/" + sessionInfo.SessionPayload.session_id + '/' + user.UserID);
            if (!sessionInfo.VerifiedUserIDs.Contains(user.UserID))
                sessionInfo.VerifiedUserIDs.Add(user.UserID);
        }
        else
        {
            verifiedUsers.RemoveAll(u => u.UserID == user.UserID);
        }
    }

    void GetSessionIDCallback(long result, string data)
    {
        LogEx.Log(LogTopics.Networking, $"Session ID result: {result} {data}");
        if (result == 200)
        {
            sessionInfo.SessionPayload = JsonUtility.FromJson<SessionPayload>(data);
        }
    }

    void UploadAvatarsCallback(long result, string data)
    {
        LogEx.Log(LogTopics.Networking, $"Uploading Sprites result: {result} {data}");
    }

    void GetAvatarsCallback(long result, VerifiedUser user, string data)
    {
        LogEx.Log(LogTopics.Networking, $"Get Avatars result: {result} for ID {user.UserID}");
        if (result == 200)
        {
            var avatarPayload = JsonUtility.FromJson<AvatarPayload>(data);
            var spriteExtManager = new SpriteExternalManager(avatarPayload);

            if (user.Character != null)
            {
                user.Character.Initialize(spriteExtManager);
            }
            else
            {
                user.Character = characterManager.CreateExtCharacter(spriteExtManager);
            }

#if UNITY_EDITOR
            foreach (var serializedAvatar in avatarPayload.avatars)
            {
                LogEx.Log(LogTopics.Networking, serializedAvatar.filename);
            }
#endif
        }
    }

    void PingAPICallback(long result, string data)
    {
        apiResponseTime = System.DateTime.Now;
        LogEx.Log(LogTopics.Networking, $"API Ping: {result} {(apiResponseTime - apiRequestTime).TotalMilliseconds}ms");
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
            var newUser = new VerifiedUser(friendIDInputField.text);
            verifiedUsers.Add(newUser);
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
                LogEx.Log(LogTopics.Networking, $"Attempting to get avatars from {verifiedUser.UserID}");
                StartCoroutine(NetworkHelper.GetUserData(GetAvatarsCallback, verifiedUser, uri, $"get-avatars/{sessionInfo.SessionPayload.session_id}/{verifiedUser.UserID}"));
            }
        }
    }

    public void UploadAvatars()
    {
        if (HasSession)
        {
            var cachedSpritePaths = spriteManager.CachedSpritePaths;
            WWWForm uploadForm = new WWWForm();

            foreach (var cachedSpritePath in cachedSpritePaths)
            {
                uploadForm.AddBinaryData("avatar", File.ReadAllBytes(cachedSpritePath), Path.GetFileName(cachedSpritePath));
            }

            StartCoroutine(NetworkHelper.PostRequest(UploadAvatarsCallback, uri, $"upload-avatar/{sessionInfo.SessionPayload.session_id}", uploadForm));
        }
    }

    public void PingAPI()
    {
        apiRequestTime = System.DateTime.Now;
        StartCoroutine(NetworkHelper.GetRequest(PingAPICallback, uri, "ping"));
    }
    #endregion

    #region Websockets
    public async void InitializeWebsocketSender()
    {
        LogEx.Log(LogTopics.Networking, "Initializing Websocket Sender");

        websocketSender = new WebSocket("ws://" + uri + "/send-data/" + sessionInfo.SessionPayload.session_id);

        websocketSender.OnError += (err) =>
        {
            LogEx.Error(LogTopics.Networking, $"Websocket Sender Error: {err}");
        };

        InvokeRepeating("SendWebsocketData", 0.0f, 0.05f);

        await websocketSender.Connect();
    }

    async void AddWebsocketReceiver(VerifiedUser user, string url)
    {
        LogEx.Log(LogTopics.Networking, "Instantiating Websocket Receiver");

        user.WebsocketReceiver = new WebSocket(url);

        user.WebsocketReceiver.OnError += (err) =>
        {
            LogEx.Error(LogTopics.Networking, $"Websocket Receiver Error: {err}");
        };
        user.WebsocketReceiver.OnMessage += (data) =>
        {
            ReceiveWebsocketData(user, data);
        };

        await user.WebsocketReceiver.Connect();
    }

    void ReceiveWebsocketData(VerifiedUser user, byte[] data)
    {
        LogEx.Log(LogTopics.Networking, $"Receiving Websocket Data: {System.Text.Encoding.UTF8.GetString(data)}");

        if (user.Character != null)
        {
            // Update user character with activity information
        }
    }

    async void SendWebsocketData()
    {
        if (websocketSender.State == WebSocketState.Open)
        {
            var activityPayload = new ActivityPayload(selfCharacter.MeanVolume, selfCharacter.CurrentExpressionName);
            LogEx.Log(LogTopics.Networking, $"Sending Websocket Data: {activityPayload.ToString()}");

            await websocketSender.SendText(JsonUtility.ToJson(activityPayload));
        }
        else if (websocketSender.State == WebSocketState.Closed)
        {
            CancelInvoke("SendWebsocketData");
        }
    }
    #endregion

    public void SaveCache()
    {
        File.WriteAllText(sessionFilePath, JsonUtility.ToJson(sessionInfo));
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
        }
    }

    void Awake()
    {
        LoadCache();
    }
}
