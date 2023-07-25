using NativeWebSocket;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
    [SerializeField] SpriteManager spriteManager;
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

    WebSocket webSocket;

    List<string> verifiedUserIDs;
    string networkPath;
    string sessionFilePath;

    SessionInformation sessionInfo;

    #region Callbacks
    void InitiateSessionCallback(long result, string data)
    {
        LogEx.Log(LogTopics.Networking, $"Verification ID result: {result} {data}");
    }

    void RquestSessionCallback(long result, int userIndex)
    {
        LogEx.Log(LogTopics.Networking, $"Request session user ID result: {result}");

        if (result != 200)
        {
            verifiedUserIDs.RemoveAt(userIndex);
        }
    }

    void GetSessionIDCallback(long result, string data)
    {
        LogEx.Log(LogTopics.Networking, $"Session ID result: {result} {data}");
        if (result == 200)
        {
            sessionInfo.SessionPayload = JsonUtility.FromJson<SessionPayload>(data);
            InitializeWebsocket();
        }
    }

    void UploadAvatarsCallback(long result, string data)
    {
        LogEx.Log(LogTopics.Networking, $"Uploading Sprites result: {result} {data}");
    }

    void GetAvatarsCallback(long result, string data)
    {
        LogEx.Log(LogTopics.Networking, $"Get Avatars result: {result} {data}");
        if (result == 200)
        {
            var avatarPayload = JsonUtility.FromJson<AvatarPayload>(data);

            foreach (var serializedAvatar in avatarPayload.internalPayload)
            {
                LogEx.Log(LogTopics.Networking, $"{serializedAvatar.filename} \n\n {serializedAvatar.filename}");
            }
        }
    }
    #endregion

    #region Requests
    public void InitiateSession()
    {
        StartCoroutine(NetworkHelper.GetRequest(InitiateSessionCallback, uri, $"verify/{userIDInputField.text}"));
    }

    public void RequestSession()
    {
        if (sessionInfo.SessionPayload != null)
        {
            StartCoroutine(NetworkHelper.SessionRequest(RquestSessionCallback, verifiedUserIDs.Count, uri, $"request-session/{sessionInfo.SessionPayload.session_id}/{friendIDInputField.text}"));
            verifiedUserIDs.Add(friendIDInputField.text);
        }
    }

    public void GetSessionID()
    {
        StartCoroutine(NetworkHelper.GetRequest(GetSessionIDCallback, uri, $"verify/{userIDInputField.text}/{verifyIDInputField.text}"));
    }

    public void GetAvatars()
    {
        if (sessionInfo.SessionPayload != null && verifiedUserIDs.Count > 0)
        {
            foreach (var verifiedUserID in verifiedUserIDs)
            {
                LogEx.Log(LogTopics.Networking, $"Attempting to get avatars from {verifiedUserID}");
                StartCoroutine(NetworkHelper.GetRequest(GetAvatarsCallback, uri, $"get-avatars/{sessionInfo.SessionPayload.session_id}/{verifiedUserID}"));
            }
        }
    }

    public void UploadAvatars()
    {
        if (sessionInfo.SessionPayload != null)
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
    #endregion

    #region Websockets
    public async void InitializeWebsocket()
    {
        LogEx.Log(LogTopics.Networking, "Initializing Websocket");

        webSocket = new WebSocket("ws://" + uri + "/send-data/" + sessionInfo.SessionPayload.session_id);

        webSocket.OnError += (err) =>
        {
            Debug.LogError($"Websocket Error: {err}");
        };

        InvokeRepeating("SendWebsocketData", 0.0f, 0.01f);

        await webSocket.Connect();
    }

    async void SendWebsocketData()
    {
        if (webSocket.State == WebSocketState.Open)
        {
            var activityPayload = new ActivityPayload(selfCharacter.MeanVolume, selfCharacter.CurrentExpressionName);

            await webSocket.SendText(JsonUtility.ToJson(activityPayload));
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
        if (File.Exists(sessionFilePath))
        {
            var serializedSessionPayload = File.ReadAllText(sessionFilePath);
            sessionInfo = JsonUtility.FromJson<SessionInformation>(serializedSessionPayload);
        }
    }

    void Awake()
    {
        sessionInfo = new SessionInformation();
        LoadCache();
    }
}
