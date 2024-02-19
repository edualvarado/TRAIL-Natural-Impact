using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetStretching : MonoBehaviour
{
    private Slider stretchSlider;

    void Start()
    {
        stretchSlider = this.GetComponent<Slider>();
        stretchSlider.minValue = 0;
        stretchSlider.maxValue = 1;
        stretchSlider.value = 0.1f;
    }
}
