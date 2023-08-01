#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CharacterManager))]
public class CharacterManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var characterManager = target as CharacterManager;
        if (characterManager != null)
        {
            EditorGUI.BeginDisabledGroup(!Application.isPlaying);

            if (GUILayout.Button("Create Dummy Ext Character"))
            {
                characterManager.CreateExtCharacter("dummy");
            }

            if (GUILayout.Button("Save Character Config"))
            {
                characterManager.SaveConfigCache();
            }

            EditorGUI.EndDisabledGroup();
        }
    }
}
#endif