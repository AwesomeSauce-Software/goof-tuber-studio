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

[System.Serializable]
public class SessionPayload
{
    public string message;
    public string session_id;
}

[System.Serializable]
public class AvatarInternalPayload
{
    public string base64;
    public string filename;
}

[System.Serializable]
public class AvatarPayload
{
    public List<AvatarInternalPayload> internalPayload;
}

[System.Serializable]
public class ActivityPayload
{
    public float voice_activity;
    public string action;

    public ActivityPayload(float newVoiceActivity, string newAction)
    {
        voice_activity = newVoiceActivity;
        action = newAction;
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

    delegate void GetRequestCallBack(long statusCode, string data);
    delegate void SessionRequestCallback(long statusCode, int userIndex);

    IEnumerator GetRequest(GetRequestCallBack callback, string additional)
    {
        string data = "";
        using (UnityWebRequest webRequest = UnityWebRequest.Get("https://" + uri + '/' + additional))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
                data = webRequest.downloadHandler.text;

            callback(webRequest.responseCode, data);
        }
    }

    IEnumerator SessionRequest(SessionRequestCallback callback, int userIndex, string additional)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get("https://" + uri + "/" + additional))
        {
            yield return webRequest.SendWebRequest();

            callback(webRequest.responseCode, userIndex);
        }
    }

    IEnumerator PostRequest(GetRequestCallBack callback, string additional, WWWForm uploadData)
    {
        string downloadData = "";
        using (UnityWebRequest webRequest = UnityWebRequest.Post("https://" + uri + '/' + additional, uploadData))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
                downloadData = webRequest.downloadHandler.text;

            callback(webRequest.responseCode, downloadData);
        }
    }

    void InitiateSessionCallback(long result, string data)
    {
        Debug.Log($"Verification ID result: {result} {data}");
    }

    void RquestSessionCallback(long result, int userIndex)
    {
        Debug.Log($"Request session user ID result: {result}");

        if (result != 200)
        {
            verifiedUserIDs.RemoveAt(userIndex);
        }
    }

    void GetSessionIDCallback(long result, string data)
    {
        Debug.Log($"Session ID result: {result} {data}");
        if (result == 200)
        {
            sessionInfo.SessionPayload = JsonUtility.FromJson<SessionPayload>(data);
            InitializeWebsocket();
        }
    }

    void UploadAvatarsCallback(long result, string data)
    {
        Debug.Log($"Uploading Sprites result: {result} {data}");
    }

    void GetAvatarsCallback(long result, string data)
    {
        Debug.Log($"Get Avatars result: {result} {data}");
        if (result == 200)
        {
            var avatarPayload = JsonUtility.FromJson<AvatarPayload>(data);

            foreach (var serializedAvatar in avatarPayload.internalPayload)
            {
                Debug.Log($"{serializedAvatar.filename} \n\n {serializedAvatar.filename}");
            }
        }
    }

    public void InitiateSession()
    {
        StartCoroutine(GetRequest(InitiateSessionCallback, $"verify/{userIDInputField.text}"));
    }

    public void RequestSession()
    {
        if (sessionInfo.SessionPayload != null)
        {
            StartCoroutine(SessionRequest(RquestSessionCallback, verifiedUserIDs.Count, $"request-session/{sessionInfo.SessionPayload.session_id}/{friendIDInputField.text}"));
            verifiedUserIDs.Add(friendIDInputField.text);
        }
    }

    public void GetSessionID()
    {
        StartCoroutine(GetRequest(GetSessionIDCallback, $"verify/{userIDInputField.text}/{verifyIDInputField.text}"));
    }

    public void GetAvatars()
    {
        if (sessionInfo.SessionPayload != null && verifiedUserIDs.Count > 0)
        {
            foreach (var verifiedUserID in verifiedUserIDs)
            {
                Debug.Log($"Attempting to get avatars from {verifiedUserID}");
                StartCoroutine(GetRequest(GetAvatarsCallback, $"get-avatars/{sessionInfo.SessionPayload.session_id}/{verifiedUserID}"));
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

            StartCoroutine(PostRequest(UploadAvatarsCallback, $"upload-avatar/{sessionInfo.SessionPayload.session_id}", uploadForm));
        }
    }

    public async void InitializeWebsocket()
    {
        Debug.Log("Initializing Websocket");

        webSocket = new WebSocket("ws://" + uri + "/send-data/" + sessionInfo.SessionPayload.session_id);

        webSocket.OnError += (err) =>
        {
            Debug.LogError($"Websocket Error: {err}");
        };

        InvokeRepeating("SendWebsocketData", 0.0f, 0.01f);

        await webSocket.Connect();
    }

    public void SaveCache()
    {
        if (sessionInfo.SessionPayload != null)
            File.WriteAllText(sessionFilePath, JsonUtility.ToJson(sessionInfo.SessionPayload));
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

    async void SendWebsocketData()
    {
        if (webSocket.State == WebSocketState.Open)
        {
            var activityPayload = new ActivityPayload(selfCharacter.MeanVolume, selfCharacter.CurrentExpressionName);

            await webSocket.SendText(JsonUtility.ToJson(activityPayload));
        }
    }

    void Awake()
    {
        sessionInfo = new SessionInformation();
        LoadCache();
    }
}
