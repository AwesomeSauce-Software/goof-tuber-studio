
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public enum SortingMode
    {
        Free,
        LineAnchorRight,
        LineAnchorLeft
    }

    [SerializeField] SpaceManager spaceManager;
    [SerializeField] CharacterAnimator characterPrefab;
    [Header("Sorting Modes")]
    [SerializeField] SortingMode sortingMode;
    [SerializeField] [Range(0.0f, 1.0f)] float lineLeft;
    [SerializeField] [Range(0.0f, 1.0f)] float lineRight;
    [SerializeField] [Min(1)] int charactersVisible;

    float lineIncrement;

    Vector3 lineMaxLeft;
    Vector3 lineMaxRight;

    List<CharacterAnimator> characters;

    public CharacterAnimator CreateExtCharacter()
    {
        var extCharacter = Instantiate(characterPrefab);
        extCharacter.transform.SetParent(transform);
        extCharacter.transform.localScale = Vector3.one;

        characters.Add(extCharacter);
        UpdateSorting();

        return extCharacter;
    }

    public CharacterAnimator CreateExtCharacter(AvatarPayload avatarPayload)
    {
        var extCharacter = CreateExtCharacter();
        extCharacter.LoadAvatarPayload(avatarPayload);
        return extCharacter;
    }

    public void UpdateLineLeft(float value)
    {
        lineLeft = value;
        UpdateLineLimits();
        UpdateSorting();
    }

    public void UpdateLineRight(float value)
    {
        lineRight = value;
        UpdateLineLimits();
        UpdateSorting();
    }

    void LineSorting()
    {
        if (characters.Count <= 0)
            return;

        float spacing = 0.0f;
        for (int i = 0; i < characters.Count; ++i)
        {
            float direction = sortingMode == SortingMode.LineAnchorRight ? spacing : 1.0f - spacing;
            var character = characters[i];
            var position = Vector3.Lerp(lineMaxRight, lineMaxLeft, direction);
            position.z = i * 0.001f;

            character.InitialPosition = position;
            spacing += lineIncrement;
        }
    }

    void UpdateSorting()
    {
        switch (sortingMode)
        {
            case SortingMode.LineAnchorRight:
            case SortingMode.LineAnchorLeft:
                LineSorting();
                break;
        }
    }

    void UpdateLineLimits()
    {
        lineMaxLeft = Vector3.Lerp(spaceManager.LeftMostPoint, spaceManager.RightMostPoint, lineLeft);
        lineMaxLeft.z = 0.0f;
        lineMaxRight = Vector3.Lerp(spaceManager.RightMostPoint, spaceManager.LeftMostPoint, lineRight);
        lineMaxRight.z = 0.0f;
        lineIncrement = 1.0f / charactersVisible;
    }

    void AddExistingCharacters()
    {
        characters = new List<CharacterAnimator>();
        for (int i = 0; i < transform.childCount; ++i)
        {
            var potentialCharacter = transform.GetChild(i).GetComponent<CharacterAnimator>();
            if (potentialCharacter != null)
                characters.Add(potentialCharacter);
        }
        UpdateSorting();
    }

    void Awake()
    {
        AddExistingCharacters();
        UpdateLineLimits();
    }

#if UNITY_EDITOR

    void OnValidate()
    {
        UpdateLineLimits();
        if (characters != null)
            UpdateSorting();
    }

    const int loopMax = 500;
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(lineMaxLeft, 0.085f);
        Gizmos.DrawSphere(lineMaxRight, 0.085f);

        Gizmos.color = Color.yellow;
        float j = 0.0f;
        for (int i = 0; j < 1.0f && i < loopMax; ++i)
        {
            j += lineIncrement;
            Gizmos.DrawSphere(Vector3.Lerp(lineMaxLeft, lineMaxRight, j) + Vector3.up * 0.001f, 0.05f);
        }
    }

#endif
}
