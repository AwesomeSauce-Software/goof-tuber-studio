using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public enum eSortingMode
    {
        Free,
        FreeLine,
        LineAnchorRight,
        LineAnchorLeft
    }

    public eSortingMode SortingMode => sortingMode;

    [SerializeField] SpaceManager spaceManager;
    [SerializeField] CharacterAnimator characterPrefab;
    [Header("Sorting Modes")]
    [SerializeField] eSortingMode sortingMode;
    [SerializeField] [Range(0.0f, 1.0f)] float lineLeft;
    [SerializeField] [Range(0.0f, 1.0f)] float lineRight;
    [SerializeField] [Min(1)] int charactersVisible;
    [Header("Config Cache")]
    [SerializeField] string configFolder;
    [SerializeField] string configFileName;

    float lineIncrement;
    Vector3 lineMaxLeft;
    Vector3 lineMaxRight;

    List<CharacterAnimator> characters;

    CharactersConfig charactersConfig;
    string configFolderPath;
    string configFilePath;

    public CharacterAnimator CreateExtCharacter(string userID)
    {
        var extCharacter = Instantiate(characterPrefab);
        extCharacter.transform.SetParent(transform);
        extCharacter.transform.localScale = Vector3.one;
        extCharacter.UserID = userID;

        int index = System.Array.FindIndex(charactersConfig.CharacterPlacements, c => c.UserID == userID);
        if (index >= 0)
        {
            var placement = charactersConfig.CharacterPlacements[index];
            extCharacter.InitialPosition = placement.Position;
        }

        characters.Add(extCharacter);
        UpdateSorting();

        return extCharacter;
    }

    public CharacterAnimator CreateExtCharacter(string userID, AvatarPayload avatarPayload)
    {
        var extCharacter = CreateExtCharacter(userID);
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

    public void SetSortingMode(eSortingMode newSortingMode)
    {
        sortingMode = newSortingMode;
        UpdateSorting();
    }

    public void UpdateSorting()
    {
        switch (sortingMode)
        {
            case eSortingMode.FreeLine:
                FreeLineSorting();
                break;
            case eSortingMode.LineAnchorRight:
            case eSortingMode.LineAnchorLeft:
                LineSorting();
                break;
        }
    }

    public void SaveConfigCache()
    {
        charactersConfig.SortingMode = sortingMode;
        charactersConfig.CharactersVisible = charactersVisible;
        charactersConfig.LineSortLeft = lineLeft;
        charactersConfig.LineSortRight = lineRight;

        var characterPlacements = charactersConfig.CharacterPlacements.ToList();
        foreach (var character in characters)
        {
            int index = characterPlacements.FindIndex(c => c.UserID == character.UserID);
            var placement = new CharacterPlacement(character.UserID, character.InitialPosition);

            if (index >= 0)
                characterPlacements[index] = placement;
            else
                characterPlacements.Add(placement);
        }
        charactersConfig.CharacterPlacements = characterPlacements.ToArray();

        File.WriteAllText(configFilePath, JsonUtility.ToJson(charactersConfig));
    }

    void LineSorting(bool overrideSort = false)
    {
        if (characters.Count <= 0)
            return;

        float spacing = 0.0f;
        for (int i = 0; i < characters.Count; ++i)
        {
            float direction = sortingMode == eSortingMode.LineAnchorRight || overrideSort ? spacing : 1.0f - spacing;
            var character = characters[i];
            var position = Vector3.Lerp(lineMaxRight, lineMaxLeft, direction);
            position.z = i * 0.001f;

            character.InitialPosition = position;
            spacing += lineIncrement;
        }
    }

    void FreeLineSorting()
    {
        foreach (var character in characters)
        {
            character.InitialPosition.y = spaceManager.RightMostPoint.y;
        }
    }

    void UpdateLineLimits()
    {
        lineMaxLeft = Vector3.Lerp(spaceManager.LeftMostPoint, spaceManager.RightMostPoint, Mathf.Clamp01(lineLeft));
        lineMaxLeft.z = 0.0f;
        lineMaxRight = Vector3.Lerp(spaceManager.RightMostPoint, spaceManager.LeftMostPoint, Mathf.Clamp01(lineRight));
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

    void LoadConfigCache()
    {
        configFolderPath = DataSystem.CreateSpace(configFolder);
        configFilePath = configFolderPath + configFileName;

        if (File.Exists(configFilePath))
        {
            var serializedCache = File.ReadAllText(configFilePath);
            charactersConfig = JsonUtility.FromJson<CharactersConfig>(serializedCache);

            if (charactersConfig != null)
            {
                sortingMode = charactersConfig.SortingMode;
                charactersVisible = charactersConfig.CharactersVisible > 0 ? charactersConfig.CharactersVisible : 1;
                lineLeft = charactersConfig.LineSortLeft;
                lineRight = charactersConfig.LineSortRight;

                foreach (var characterConfig in charactersConfig.CharacterPlacements)
                {
                    int index = characters.FindIndex(c => c.UserID == characterConfig.UserID);
                    if (index >= 0)
                    {
                        characters[index].InitialPosition = characterConfig.Position;
                    }
                }
            }
        }
        else
        {
            charactersConfig = new CharactersConfig();
            charactersConfig.CharacterPlacements = new CharacterPlacement[1];
            charactersConfig.CharacterPlacements[0] = new CharacterPlacement("user", Vector3.zero);
        }
    }

    private void Start()
    {
        UpdateSorting();
    }

    void Awake()
    {
        AddExistingCharacters();
        LoadConfigCache();
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
