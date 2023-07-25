#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LogTopicsWindow : EditorWindow
{
    Dictionary<string, bool> cachedTopics = new Dictionary<string, bool>();
    string[] topicNames;

    [MenuItem("Window/BitRapture/LogTopics")]
    public static void ShowWindow()
    {
        GetWindow(typeof(LogTopicsWindow), false, "Debug Log Topics");
    }
    
    void SetupTopics()
    {
        topicNames = Enum.GetNames(typeof(LogTopics));
        foreach (var topic in topicNames)
        {
            if (!cachedTopics.ContainsKey(topic))
                cachedTopics.Add(topic, EditorPrefs.GetBool(topic));
        }
    }

    void OnValidate()
    {
        SetupTopics();
    }

    void Awake()
    {
        SetupTopics();
    }

    void OnGUI()
    {
        GUILayout.BeginVertical();

        EditorGUI.BeginChangeCheck();
        foreach (var topic in topicNames)
        {
            cachedTopics[topic] = EditorGUILayout.Toggle(topic, cachedTopics[topic]);
        }

        if (EditorGUI.EndChangeCheck())
        {
            foreach (var cachedTopic in cachedTopics)
            {
                EditorPrefs.SetBool(cachedTopic.Key, cachedTopic.Value);
            }
        }

        GUILayout.EndVertical();
    }
}

#endif