using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputButtonDisabler : MonoBehaviour
{
    [SerializeField] InputField inputField;
    [SerializeField] int offset;
    [SerializeField] bool invert;
    Button button;

    void UpdateButtonActivity(string value = "")
    {
        button.interactable = (inputField.text.Length >= inputField.characterLimit - offset) ^ invert;
    }

    void Start()
    {
        UpdateButtonActivity();    
    }

    void Awake()
    {
        inputField.onValueChanged.AddListener(UpdateButtonActivity);
        button = GetComponent<Button>();
    }
}
