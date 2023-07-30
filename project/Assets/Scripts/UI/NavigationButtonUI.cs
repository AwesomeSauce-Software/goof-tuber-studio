using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NavigationButtonUI : MonoBehaviour
{
    [HideInInspector] public int ButtonIndex = -1;
    public Text ButtonText;

    public delegate void NavButtonPressed(int index);
    public NavButtonPressed PressedCallback;

    public void OnPressed()
    {
        PressedCallback(ButtonIndex);
    }
}
