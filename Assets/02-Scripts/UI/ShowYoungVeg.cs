using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowYoungVeg : MonoBehaviour
{
    public Slider youngVegSlider;
    private TextMeshProUGUI youngVegValue;

    private void Start()
    {
        youngVegValue = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        youngVegValue.text = (youngVegSlider.value / 1000000f).ToString("F2") + " MPa";
    }
}
