using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowDeg : MonoBehaviour
{
    public Slider degSlider;
    private TextMeshProUGUI degValue;

    private void Start()
    {
        degValue = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        degValue.text = (degSlider.value).ToString("F0") + "°";
    }
}
