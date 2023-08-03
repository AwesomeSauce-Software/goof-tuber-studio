using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NavigatorUI : MonoBehaviour
{
    [SerializeField] Image backgroundImage;
    [SerializeField] NavigationButtonUI buttonPrefab;
    [SerializeField] List<GameObject> UICategories;

    List<NavigationButtonUI> buttons;

    int currentActiveMenu = -1;
    bool menuVisible;

    public void GotoNavMenu()
    {
        currentActiveMenu = -1;
        for (int i = 0; i < UICategories.Count; ++i)
        {
            UICategories[i].SetActive(false);
            buttons[i].gameObject.SetActive(true);
        }
    }

    public void ShowMenu()
    {
        currentActiveMenu = -1;
        foreach (var button in buttons)
            button.gameObject.SetActive(true);
        menuVisible = true;
        backgroundImage.enabled = true;
    }

    public void HideMenu()
    {
        currentActiveMenu = -1;
        for (int i = 0; i < UICategories.Count; ++i)
        {
            UICategories[i].SetActive(false);
            buttons[i].gameObject.SetActive(false);
        }
        menuVisible = false;
        backgroundImage.enabled = false;
    }

    void OnNavigationButtonPressed(int buttonIndex)
    {
        bool offMenu = currentActiveMenu == -1 && buttonIndex != -1;
        bool onMenu = currentActiveMenu != -1 && buttonIndex == -1;

        if (onMenu || offMenu)
            foreach (var button in buttons)
                button.gameObject.SetActive(onMenu);

        if (currentActiveMenu != -1)
            UICategories[currentActiveMenu].SetActive(false);

        UICategories[buttonIndex].SetActive(true);
        currentActiveMenu = buttonIndex;
    }

    void CreateNavigationButtons()
    {
        buttons = new List<NavigationButtonUI>();
        for (int i = 0; i < UICategories.Count; ++i)
        {
            var newButton = Instantiate(buttonPrefab);
            newButton.PressedCallback = OnNavigationButtonPressed;
            newButton.ButtonText.text = UICategories[i].name;
            newButton.ButtonIndex = i;
            newButton.transform.SetParent(transform, false);

            UICategories[i].SetActive(false);
            buttons.Add(newButton);
        }
        menuVisible = true;
    }

    void UpdateMenuVisibility()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (menuVisible)
            {
                if (currentActiveMenu != -1)
                {
                    HideMenu();
                    ShowMenu();
                }
                else
                    HideMenu();
            }
            else
                ShowMenu();
        }
    }

    void Update()
    {
        UpdateMenuVisibility();
    }

    void Awake()
    {
        CreateNavigationButtons();
        HideMenu();
    }
}
