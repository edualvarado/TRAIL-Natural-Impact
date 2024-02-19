using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetAngle : MonoBehaviour
{
    private Slider angleSlider;

    void Start()
    {
        angleSlider = this.GetComponent<Slider>();
        angleSlider.minValue = 0;
        angleSlider.maxValue = 90;
        angleSlider.value = 10;

    }
}
