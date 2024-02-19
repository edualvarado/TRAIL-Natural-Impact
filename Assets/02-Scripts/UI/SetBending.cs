using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetBending : MonoBehaviour
{
    private Slider bendSlider;

    void Start()
    {
        bendSlider = this.GetComponent<Slider>();
        bendSlider.minValue = 0;
        bendSlider.maxValue = 1;
        bendSlider.value = 1;
    }
}
