using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteExternalManager : SpriteCache
{
    [SerializeField] Sprite dummyNonTalking;

    Dictionary<string, Sprite> cachedSprites;

    public override Sprite GetSprite(string spriteName)
    {
        if (cachedSprites.ContainsKey(spriteName))
            return cachedSprites[spriteName];
        else
            return dummyNonTalking;
    }

    public override Sprite GetExpression(string expressionCategory, int categoryIndex = 0)
    {
        return null;
    }

    public override void LoadAvatarPayload(AvatarPayload avatarPayload)
    {
        for (int i = 0; i < avatarPayload.avatars.Length; ++i)
        {
            var serializedAvatar = avatarPayload.avatars[i];
            var cachedSprite = DataSystem.LoadSpriteFromBase64(serializedAvatar.base64);
            cachedSprite.name = serializedAvatar.filename;

            if (!cachedSprites.ContainsKey(serializedAvatar.filename))
                cachedSprites.Add(serializedAvatar.filename, cachedSprite);
            else
                cachedSprites[serializedAvatar.filename] = cachedSprite;
        }
    }

    void Awake()
    {
        cachedSprites = new Dictionary<string, Sprite>();
    }
}
