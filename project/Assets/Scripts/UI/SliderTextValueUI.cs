using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderTextValueUI : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField][Min(1.0f)] float rounding = 1.0f;
    [SerializeField] bool showMaxValue;
    Text textElement;

    public void OnValueChange(float value)
    {
        float roundedValue = Mathf.Round(value * rounding) / rounding;
        textElement.text = $"{roundedValue}{(showMaxValue ? $"/{slider.maxValue}" : "")}";
    }

    void Awake()
    {
        textElement = GetComponent<Text>();
    }
}
