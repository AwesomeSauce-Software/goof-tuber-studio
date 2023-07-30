using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigatorUI : MonoBehaviour
{
    [SerializeField] NavigationButtonUI buttonPrefab;
    [SerializeField] List<GameObject> UICategories;

    int currentActiveMenu = -1;

    void OnNavigationButtonPressed(int buttonIndex)
    {
        if (currentActiveMenu == -1)
        {
            UICategories[buttonIndex].SetActive(true);
            currentActiveMenu = buttonIndex;
        }
    }

    void CreateNavigationButtons()
    {
        for (int i = 0; i < UICategories.Count; ++i)
        {
            var newButton = Instantiate(buttonPrefab);
            newButton.PressedCallback = OnNavigationButtonPressed;
            newButton.ButtonText.text = UICategories[i].name;
            newButton.ButtonIndex = i;
            newButton.transform.SetParent(transform, false);

            UICategories[i].SetActive(false);
        }
    }
    
    void Awake()
    {
        CreateNavigationButtons();
    }
}
