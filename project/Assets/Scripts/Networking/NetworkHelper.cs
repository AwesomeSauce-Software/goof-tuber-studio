using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public static class NetworkHelper
{
    public delegate void GetRequestCallBack(long statusCode, string data);
    public delegate void GetUserDataCallback(long statusCode, VerifiedUser user, string data);
    public delegate void SessionRequestCallback(long statusCode, VerifiedUser user);

    public static IEnumerator GetUserData(GetUserDataCallback callback, VerifiedUser user, string uri, string additional)
    {
        string data = "";
        using (UnityWebRequest webRequest = UnityWebRequest.Get("https://" + uri + '/' + additional))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
                data = webRequest.downloadHandler.text;

            callback(webRequest.responseCode, user, data);
        }
    }

    public static IEnumerator GetRequest(GetRequestCallBack callback, string uri, string additional)
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

    public static IEnumerator SessionRequest(SessionRequestCallback callback, VerifiedUser user, string uri, string additional)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get("https://" + uri + "/" + additional))
        {
            yield return webRequest.SendWebRequest();

            callback(webRequest.responseCode, user);
        }
    }

    public static IEnumerator PostRequestForm(GetRequestCallBack callback, string uri, string additional, WWWForm uploadData)
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

    public static IEnumerator PostRequestRaw(GetRequestCallBack callback, string uri, string additional, string uploadData)
    {
        string downloadData = "";
        using (UnityWebRequest webRequest = new UnityWebRequest("https://" + uri + '/' + additional, UnityWebRequest.kHttpVerbPOST))
        {
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(uploadData));

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
                downloadData = webRequest.downloadHandler.text;

            callback(webRequest.responseCode, downloadData);
        }
    }
}