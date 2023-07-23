using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SessionPayload
{
    public string message;
    public string session_id;
}

public class NetworkManager : MonoBehaviour
{
    [SerializeField] CharacterManager characterManager;
    [Header("UI References")]
    [SerializeField] InputField userIDInputField;
    [SerializeField] InputField verifyIDInputField;
    [Header("Network Options")]
    [SerializeField] string networkFolder;
    [SerializeField] string sessionFileName;
    [SerializeField] string uri;

    string networkPath;
    string sessionFilePath;
    SessionPayload sessionPayload;

    delegate void RequestCallBack(UnityWebRequest.Result result, string data);

    IEnumerator GetRequest(RequestCallBack callback,  string additional = "")
    {
        string data = "";
        using (UnityWebRequest webRequest = UnityWebRequest.Get("https://" + uri + '/' + additional))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
                data = webRequest.downloadHandler.text;

            callback(webRequest.result, data);
        }
    }

    void InitiateSessionCallback(UnityWebRequest.Result result, string data)
    {
        if (result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"Yippeee, {data}");
        }
    }

    void GetSessionIDCallback(UnityWebRequest.Result result, string data)
    {
        if (result == UnityWebRequest.Result.Success)
        {
            sessionPayload = JsonUtility.FromJson<SessionPayload>(data);
            Debug.Log($"{sessionPayload.message}: {sessionPayload.session_id}");
        }
    }

    public void InitiateSession()
    {
        StartCoroutine(GetRequest(InitiateSessionCallback, $"verify/{userIDInputField.text}"));
    }

    public void GetSessionID()
    {
        StartCoroutine(GetRequest(GetSessionIDCallback, $"verify/{userIDInputField.text}/{verifyIDInputField.text}"));
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
        LoadCache();
    }
}
