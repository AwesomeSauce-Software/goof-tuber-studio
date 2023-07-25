using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [SerializeField] CharacterAnimator characterPrefab;

    public CharacterAnimator CreateExtCharacter(SpriteExternalManager spriteManager)
    {
        var extCharacter = Instantiate(characterPrefab);
        extCharacter.Initialize(spriteManager);

        extCharacter.transform.parent = transform;

        return extCharacter;
    }
}
