using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

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


public class NetworkManager : MonoBehaviour
{
    [SerializeField] SpriteManager spriteManager;
    [SerializeField] CharacterManager characterManager;
    [Header("UI References")]
    [SerializeField] InputField userIDInputField;
    [SerializeField] InputField additionalUserIDInputField;
    [SerializeField] InputField verifyIDInputField;
    [Header("Network Options")]
    [SerializeField] string networkFolder;
    [SerializeField] string sessionFileName;
    [SerializeField] string uri;

    [SerializeField] string friendID;

    List<string> verifiedUserIDs;
    string networkPath;
    string sessionFilePath;
    SessionPayload sessionPayload;

    delegate void GetRequestCallBack(long statusCode, string data);

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

    void RquestSessionCallback(long result, string data)
    {
        Debug.Log($"Request session user ID result: {result} {data}");

        // todo
        //  check body message if result is successful
        bool notVerified = result != 200;

        if (notVerified)
        {
            verifiedUserIDs.RemoveAt(verifiedUserIDs.Count - 1);
        }
    }

    void GetSessionIDCallback(long result, string data)
    {
        Debug.Log($"Session ID result: {result} {data}");
        if (result == 200)
        {
            sessionPayload = JsonUtility.FromJson<SessionPayload>(data);
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
        if (sessionPayload != null)
        {
            verifiedUserIDs.Add(additionalUserIDInputField.text);
            StartCoroutine(GetRequest(RquestSessionCallback, $"request-session/{sessionPayload.session_id}/{additionalUserIDInputField.text}"));
        }
    }

    public void GetSessionID()
    {
        StartCoroutine(GetRequest(GetSessionIDCallback, $"verify/{userIDInputField.text}/{verifyIDInputField.text}"));
    }

    public void GetAvatars()
    {
        if (sessionPayload != null && verifiedUserIDs.Count > 0)
        {
            foreach (var verifiedUserID in verifiedUserIDs)
            {
                Debug.Log($"Attempting to get avatars from {verifiedUserID}");
                StartCoroutine(GetRequest(GetAvatarsCallback, $"get-avatars/{sessionPayload.session_id}/{verifiedUserID}"));
            }
        }
    }

    public void UploadAvatars()
    {
        if (sessionPayload != null)
        {
            var cachedSpritePaths = spriteManager.CachedSpritePaths;
            WWWForm uploadForm = new WWWForm();

            foreach (var cachedSpritePath in cachedSpritePaths)
            {
                uploadForm.AddBinaryData("avatar", File.ReadAllBytes(cachedSpritePath), Path.GetFileName(cachedSpritePath));
            }

            StartCoroutine(PostRequest(UploadAvatarsCallback, $"upload-avatar/{sessionPayload.session_id}", uploadForm));
        }
    }

    public void SaveCache()
    {
        if (sessionPayload != null)
            File.WriteAllText(sessionFilePath, JsonUtility.ToJson(sessionPayload));
    }

    void LoadCache()
    {
        networkPath = DataSystem.CreateSpace(networkFolder);

        sessionFilePath = networkPath + sessionFileName;
        if (File.Exists(sessionFilePath))
        {
            var serializedSessionPayload = File.ReadAllText(sessionFilePath);
            sessionPayload = JsonUtility.FromJson<SessionPayload>(serializedSessionPayload);
        }
    }

    void Awake()
    {
        verifiedUserIDs = new List<string>();

        //verifiedUserIDs.Add(friendID);
        LoadCache();
    }
}
