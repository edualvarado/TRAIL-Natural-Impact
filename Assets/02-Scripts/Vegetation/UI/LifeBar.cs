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

public class LifeBar : MonoBehaviour
{
    public Slider sliderLifeRatio;
    public Gradient gradientLife;
    public Image fillLifeRatio;

    void Start()
    {
        sliderLifeRatio = GetComponent<Slider>();
        fillLifeRatio = transform.GetChild(0).GetComponent<Image>();
    }

    public void SetMaxLifeBar(float health)
    {
        sliderLifeRatio.maxValue = health;
        sliderLifeRatio.value = health;
        
        fillLifeRatio.color = gradientLife.Evaluate(1f);
    }

    public void SetLifeBar(float health)
    {
        sliderLifeRatio.value = health;
        fillLifeRatio.color = gradientLife.Evaluate(sliderLifeRatio.normalizedValue);
    }
}
