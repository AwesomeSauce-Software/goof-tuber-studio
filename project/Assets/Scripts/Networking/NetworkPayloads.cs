using System.Collections;
using System.Collections.Generic;

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
    public AvatarInternalPayload[] avatars;
}

[System.Serializable]
public class ActivityPayload
{
    public float voice_activity;
    public string action;
}

[System.Serializable]
public class ReceiveInternalActivitiyPayload
{
    public string userid;
    public ActivityPayload activity;
}

[System.Serializable]
public class ReceiveActivitiyPayload
{
    public ReceiveInternalActivitiyPayload[] data;
}

[System.Serializable]
public class VerifiedUser
{
    public string UserID;
    public CharacterAnimator Character;

    public VerifiedUser(string newUserID)
    {
        UserID = newUserID;
    }
}