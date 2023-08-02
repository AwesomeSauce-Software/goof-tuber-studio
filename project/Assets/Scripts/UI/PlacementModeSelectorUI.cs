using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlacementModeSelectorUI : MonoBehaviour
{
    [System.Serializable]
    public class PlacementMode
    {
        public string ModeName;
        public CharacterManager.eSortingMode SortingMode;
        public GameObject ModeBodyObject;
    }

    [SerializeField] CharacterFocus characterFocus;
    [SerializeField] CharacterManager characterManager;
    [SerializeField] NavigationButtonUI buttonPrefab;
    [SerializeField] List<PlacementMode> modes;

    List<NavigationButtonUI> buttons;

    int currentSortingModeIndex = 0;
    
    void OnSelectorButtonPressed(int index)
    {
        SetModeActive(currentSortingModeIndex, false);
        SetModeActive(index, true);

        currentSortingModeIndex = index;
        characterFocus.SetUIObjectsActive(true);
    }

    void SetModeActive(int index, bool value)
    {
        var mode = modes[index];

        if (mode.ModeBodyObject != null)
        {
            mode.ModeBodyObject.SetActive(value);
        }
        buttons[index].SetSelected(value);

        if (value)
            characterManager.SetSortingMode(mode.SortingMode);
    }

    void CreateSelectorButtons()
    {
        buttons = new List<NavigationButtonUI>();
        for (int i = 0; i < modes.Count; ++i)
        {
            var mode = modes[i];
            var selectorButton = Instantiate(buttonPrefab);

            selectorButton.ButtonIndex = i;
            selectorButton.ButtonText.text = mode.ModeName;
            selectorButton.PressedCallback = OnSelectorButtonPressed;

            selectorButton.transform.SetParent(transform, false);

            if (mode.ModeBodyObject != null)
                mode.ModeBodyObject.SetActive(false);

            buttons.Add(selectorButton);
        }
    }

    void SetCurrentModeActive()
    {
        int index = modes.FindIndex(m => m.SortingMode == characterManager.SortingMode);
        if (index >= 0)
        {
            currentSortingModeIndex = index;
        }

        SetModeActive(currentSortingModeIndex, true);
    }

    void Start()
    {
        SetCurrentModeActive();    
    }

    void Awake()
    {
        CreateSelectorButtons();    
    }
}
