using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetMax : MonoBehaviour
{
    private Slider maxSlider;

    void Start()
    {
        maxSlider = this.GetComponent<Slider>();
        maxSlider.minValue = 0;
        maxSlider.maxValue = 90;
        maxSlider.value = 70;
    }
}
