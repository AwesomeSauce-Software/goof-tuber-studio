#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NetworkManager))]
public class NetworkManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();

        EditorGUI.BeginDisabledGroup(!Application.isPlaying);

        if (GUILayout.Button("Save Network Cache"))
        {
            var networkObject = target as NetworkManager;
            if (networkObject != null)
                networkObject.SaveCache();
        }

        EditorGUI.EndDisabledGroup();
    }
}
#endif
