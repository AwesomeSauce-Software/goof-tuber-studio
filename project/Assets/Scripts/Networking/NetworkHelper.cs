using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public static class NetworkHelper
{
    public delegate void GetRequestCallBack(long statusCode, string data);
    public delegate void SessionRequestCallback(long statusCode, int userIndex);

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

    public static IEnumerator SessionRequest(SessionRequestCallback callback, int userIndex, string uri, string additional)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get("https://" + uri + "/" + additional))
        {
            yield return webRequest.SendWebRequest();

            callback(webRequest.responseCode, userIndex);
        }
    }

    public static IEnumerator PostRequest(GetRequestCallBack callback, string uri, string additional, WWWForm uploadData)
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
}