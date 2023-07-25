using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteExternalManager : SpriteCache
{
    Dictionary<string, Sprite> cachedSprites;

    public override Sprite GetSprite(string spriteName)
    {
        return cachedSprites[spriteName];
    }

    public override Sprite GetExpression(string expressionCategory, int categoryIndex = 0)
    {
        return null;
    }

    void AttemptLoadSprites(AvatarPayload avatarPayload)
    {
        cachedSprites = new Dictionary<string, Sprite>();

        for (int i = 0; i < avatarPayload.avatars.Length; ++i)
        {
            var serializedAvatar = avatarPayload.avatars[i];
            var cachedSprite = DataSystem.LoadSpriteFromBase64(serializedAvatar.base64);
            cachedSprite.name = serializedAvatar.filename;
            cachedSprites.Add(serializedAvatar.filename, cachedSprite);
        }
    }

    public SpriteExternalManager(AvatarPayload avatarPayload)
    {
        AttemptLoadSprites(avatarPayload);
    }
}
