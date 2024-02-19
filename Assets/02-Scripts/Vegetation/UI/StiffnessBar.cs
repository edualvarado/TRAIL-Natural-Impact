/****************************************************
 * File: VegetationCreator.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 01/08/2021
   * Project: Foot2Trail
   * Last update: 16/02/2023
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StiffnessBar : MonoBehaviour
{
    public Slider sliderStiffness;
    public Gradient gradientStiffness;
    public Image fillStiffness;

    void Start()
    {
        sliderStiffness = GetComponent<Slider>();
        fillStiffness = transform.GetChild(0).GetComponent<Image>();
    }

    public void SetMaxStiffnessBar(float stiffness)
    {
        sliderStiffness.maxValue = stiffness;
        sliderStiffness.value = stiffness;

        fillStiffness.color = gradientStiffness.Evaluate(1f);

    }

    public void SetStiffnessBar(float stiffness)
    {
        sliderStiffness.value = stiffness;
        fillStiffness.color = gradientStiffness.Evaluate(sliderStiffness.normalizedValue);
    }
}
