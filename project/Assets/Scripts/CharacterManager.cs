using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [SerializeField] CharacterAnimator characterPrefab;

    public CharacterAnimator CreateExtCharacter(AvatarPayload avatarPayload)
    {
        var extCharacter = Instantiate(characterPrefab);

        extCharacter.transform.SetParent(transform);
        extCharacter.LoadAvatarPayload(avatarPayload);
        extCharacter.transform.localScale = Vector3.one;

        return extCharacter;
    }
}
