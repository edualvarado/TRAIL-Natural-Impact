using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetDeg : MonoBehaviour
{
    private Slider degSlider;

    void Start()
    {
        degSlider = this.GetComponent<Slider>();
        degSlider.minValue = 0;
        degSlider.maxValue = 90;
        degSlider.value = 10;
    }
}
