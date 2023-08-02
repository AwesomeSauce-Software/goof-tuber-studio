using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NavigationButtonUI : MonoBehaviour
{
    [HideInInspector] public int ButtonIndex = -1;
    public Text ButtonText;

    Button button;
    Color selectedColor;
    Color unselectedColor;

    public delegate void NavButtonPressed(int index);
    
    public NavButtonPressed PressedCallback;

    public void SetSelected(bool value)
    {
        var buttonColors = button.colors;
        buttonColors.normalColor = value ? selectedColor : unselectedColor;

        button.colors = buttonColors;
    }

    public void OnPressed()
    {
        PressedCallback(ButtonIndex);
    }

    void Awake()
    {
        button = GetComponent<Button>();

        unselectedColor = button.colors.normalColor;
        selectedColor = button.colors.selectedColor;
    }
}
