using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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

    public List<CharacterAnimator> Characters
    {
        get
        {
            ValidateCharacterList();
            return characters;
        }
    }

    [SerializeField] Slider sliderLineLeft;
    [SerializeField] Slider sliderLineRight;
    [SerializeField] Slider sliderCharactersVisible;
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
        extCharacter.UserID = userID;

        int index = System.Array.FindIndex(charactersConfig.CharacterPlacements, c => c.UserID == userID);
        if (index >= 0)
        {
            var placement = charactersConfig.CharacterPlacements[index];
            extCharacter.InitialPosition = placement.Position;
            extCharacter.transform.localScale = placement.Scale;
            extCharacter.BobAmount = placement.BobAmount;
            extCharacter.Order = placement.Order;
        }
        else
        {
            extCharacter.transform.localScale = Vector3.one;
            extCharacter.Order = characters.Count;
            extCharacter.BobAmount = 0.5f;
        }

        characters.Add(extCharacter);
        SwapCharacterOrdering(extCharacter, extCharacter.Order);

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
        UpdateSorting();
    }

    public void UpdateLineRight(float value)
    {
        lineRight = value;
        UpdateSorting();
    }

    public void UpdateCharactersVisible(float value)
    {
        charactersVisible = Mathf.CeilToInt(value);
        UpdateSorting();
    }

    public void SetSortingMode(eSortingMode newSortingMode)
    {
        sortingMode = newSortingMode;
        UpdateSorting();
    }

    public void SetCharacterOrderToBack(CharacterAnimator character)
    {
        SwapCharacterOrdering(character, characters.Count - 1);
    }

    public void SwapCharacterOrdering(CharacterAnimator character, int newOrder)
    {
        if (newOrder >= characters.Count)
            newOrder = characters.Count - 1;

        var currentCharacter = characters[newOrder];
        currentCharacter.Order = character.Order;
        character.Order = newOrder;

        UpdateSorting();
    }

    public void UpdateSorting()
    {
        UpdateLineLimits();
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
        ValidateCharacterList();

        charactersConfig.SortingMode = sortingMode;
        charactersConfig.CharactersVisible = charactersVisible;
        charactersConfig.LineSortLeft = lineLeft;
        charactersConfig.LineSortRight = lineRight;

        var characterPlacements = charactersConfig.CharacterPlacements.ToList();
        foreach (var character in characters)
        {
            int index = characterPlacements.FindIndex(c => c.UserID == character.UserID);
            var placement = new CharacterPlacement(character.UserID, character.InitialPosition, 
                character.transform.localScale, character.Order, character.BobAmount);

            if (index >= 0)
                characterPlacements[index] = placement;
            else
                characterPlacements.Add(placement);
        }
        charactersConfig.CharacterPlacements = characterPlacements.ToArray();

        File.WriteAllText(configFilePath, JsonUtility.ToJson(charactersConfig));
    }

    void ValidateCharacterList()
    {
        characters.RemoveAll(c => c == null);
        characters.Sort((a, b) => a.Order > b.Order ? 1 : -1);
    }

    void LineSorting(bool overrideSort = false)
    {
        if (characters.Count <= 0)
            return;

        ValidateCharacterList();

        float spacing = 0.0f;
        for (int i = 0; i < characters.Count; ++i)
        {
            float direction = sortingMode == eSortingMode.LineAnchorRight || overrideSort ? spacing : 1.0f - spacing;
            var character = characters[i];
            var position = Vector3.Lerp(lineMaxRight, lineMaxLeft, direction);

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

    void SetUIElements()
    {
        sliderLineLeft.value = lineLeft;
        sliderLineRight.value = lineRight;
        sliderCharactersVisible.value = charactersVisible;
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
                        characters[index].transform.localScale = characterConfig.Scale;
                        characters[index].Order = characterConfig.Order;
                        characters[index].BobAmount = characterConfig.BobAmount;
                    }
                }
            }
        }
        else
        {
            charactersConfig = new CharactersConfig();
            charactersConfig.CharacterPlacements = new CharacterPlacement[1];
            charactersConfig.CharacterPlacements[0] = new CharacterPlacement("user", Vector3.zero, Vector3.one, 0, 0.5f);
        }
    }

    private void Start()
    {
        SetUIElements();
        UpdateSorting();
    }

    void Awake()
    {
        AddExistingCharacters();
        LoadConfigCache();
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
