using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowAngle : MonoBehaviour
{
    public Slider angleSlider;
    private TextMeshProUGUI angleValue;

    void Start()
    {
        angleValue = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        angleValue.text = (angleSlider.value).ToString("F0") + "°";
    }
}
