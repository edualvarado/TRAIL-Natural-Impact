using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowBending : MonoBehaviour
{
    public Slider bendSlider;
    private TextMeshProUGUI bendValue;

    private void Start()
    {
        bendValue = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        bendValue.text = (bendSlider.value).ToString("F1");
    }
}
