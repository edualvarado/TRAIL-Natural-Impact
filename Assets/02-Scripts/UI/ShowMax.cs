using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowMax : MonoBehaviour
{
    public Slider maxSlider;
    private TextMeshProUGUI maxValue;

    private void Start()
    {
        maxValue = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        maxValue.text = (maxSlider.value).ToString("F0") + "°";
    }
}
