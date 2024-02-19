﻿/****************************************************
 * File: ShowYoung.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 01/08/2021
   * Project: Real-Time Locomotion on Soft Grounds with Dynamic Footprints
   * Last update: 07/02/2022
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShowYoung : MonoBehaviour
{
    public Slider youngSlider;
    private TextMeshProUGUI youngValue;

    private void Start()
    {
        youngValue = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        youngValue.text = (youngSlider.value/1000000f).ToString("F2") + " MPa";
    }
}
