using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [SerializeField] CharacterAnimator characterPrefab;
    [SerializeField] SpriteExternalManager spriteExtManagerPrefab;

    public SpriteExternalManager CreateExtSpriteManager(AvatarPayload avatarPayload)
    {
        var extSpriteManager = Instantiate(spriteExtManagerPrefab);
        extSpriteManager.AttemptLoadSprites(avatarPayload);

        return extSpriteManager;
    }

    public CharacterAnimator CreateExtCharacter(AvatarPayload avatarPayload)
    {
        var extCharacter = Instantiate(characterPrefab);
        var extSpriteManager = CreateExtSpriteManager(avatarPayload);

        extCharacter.Initialize(extSpriteManager);
        extCharacter.transform.SetParent(transform);

        return extCharacter;
    }
}
