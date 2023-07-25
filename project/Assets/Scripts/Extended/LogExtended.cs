using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum LogTopics
{
    Networking
}

public static class LogEx
{
    public static bool TopicEnabled(LogTopics topic)
    {
#if UNITY_EDITOR
        return EditorPrefs.GetBool(topic.ToString(), false);
#else
        return false;
#endif
    }

    public static void SetTopicActivity(LogTopics topic, bool active)
    {
#if UNITY_EDITOR
        EditorPrefs.SetBool(topic.ToString(), active);
#endif
    }

    public static void Error(LogTopics topic, string message)
    {
#if UNITY_EDITOR
        if (TopicEnabled(topic))
        {
            Debug.LogError($"<color=#00FFFF>{topic}</color>: <color=#AA2000>{message}</color>");
        }
#endif
    }

    public static void Log(LogTopics topic, string message)
    {
#if UNITY_EDITOR
        if (TopicEnabled(topic))
        {
            Debug.Log($"<color=#00FFFF>{topic}</color>: {message}");
        }
#endif
    }
}
