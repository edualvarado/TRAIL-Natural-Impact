using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetYoungVeg : MonoBehaviour
{
    private Slider youngSlider;

    void Start()
    {
        youngSlider = this.GetComponent<Slider>();
        youngSlider.minValue = 100000;
        youngSlider.maxValue = 10000000;
        youngSlider.value = 7000000;
    }
}
