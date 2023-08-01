#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NetworkManager))]
public class NetworkManagerEditor : Editor
{
    string userIDLabel;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Networking Debug");
        EditorGUI.BeginDisabledGroup(!Application.isPlaying);

        var networkObject = target as NetworkManager;
        if (networkObject != null)
        {
            if (GUILayout.Button("Save Network Cache"))
                networkObject.SaveCache();

            if (GUILayout.Button("Ping API"))
                networkObject.PingAPI();

            EditorGUI.BeginDisabledGroup(!Application.isPlaying || !networkObject.HasSession);
            if (GUILayout.Button("Initialize Websocket"))
                networkObject.InitializeWebsocket();

            if (GUILayout.Button("Upload Sprite Data"))
                networkObject.UploadAvatars();

            if (GUILayout.Button("Get Sprite Data"))
                networkObject.GetAvatars();

            EditorGUILayout.BeginHorizontal();
            userIDLabel = EditorGUILayout.TextField(userIDLabel);
            if (GUILayout.Button("Add Verified User ID"))
            {
                networkObject.AddUserID(userIDLabel);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
        }

        EditorGUI.EndDisabledGroup();
    }
}
#endif
