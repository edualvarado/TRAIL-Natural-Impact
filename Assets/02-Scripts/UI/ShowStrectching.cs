using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowStrectching : MonoBehaviour
{
    public Slider stretchSlider;
    private TextMeshProUGUI stretchValue;

    private void Start()
    {
        stretchValue = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        stretchValue.text = (stretchSlider.value).ToString("F1");
    }
}
