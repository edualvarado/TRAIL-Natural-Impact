/****************************************************
 * File: DataClientVegetation.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 01/10/2020
   * Project: Foot2Trail
   * Last update: 20/02/2023
*****************************************************/

using Microsoft.Unity.VisualStudio.Editor;
using PositionBasedDynamics;
using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Client to request data from the simulation.
/// </summary>
public class DataClientVegetation : MonoBehaviour
{

    #region UI

    public UnityEngine.UI.Image imageVegetation;
    public UnityEngine.UI.Image imageInitialVegetation;

    #endregion

    #region Read-only & Static Fields

    private ExportVegetationMap _dataRequesterVegetation;

    #endregion

    private void Start()
    {
        _dataRequesterVegetation = new ExportVegetationMap();
        _dataRequesterVegetation.Start();

        _dataRequesterVegetation.VegetationBytes = new byte[VegetationCreator.LivingRatioBytes.Length * sizeof(float)];
        _dataRequesterVegetation.InitialVegetationBytes = new byte[VegetationCreator.LivingRatioBytes.Length * sizeof(float)];

    }

    private void Update()
    {
        // Initialize array
        float[,] floatArrayVegetation = new float[257, 257];
        float[,] floatArrayInitialVegetation = new float[257, 257];

        // Convert the byte array to a float array
        floatArrayVegetation.FromBytes(_dataRequesterVegetation.VegetationBytes);
        floatArrayInitialVegetation.FromBytes(_dataRequesterVegetation.InitialVegetationBytes);

        // Create a Texture2D
        Texture2D textureVegetation = new Texture2D(257, 257);
        Texture2D textureInitialVegetation = new Texture2D(257, 257);

        // Convert float array to Color array
        Color[] colorArrayVegetation = new Color[257 * 257];
        Color[] colorArrayInitialVegetation = new Color[257 * 257];
        Gradient gradient = new Gradient();

        // Populate the color keys at the relative time 0, 0.25, 0.5, 0.75 and 1 (0, 25, 50, 75 and 100%)
        GradientColorKey[] colorKey = new GradientColorKey[5];
        colorKey[0].color = new Color(247 / 255f, 252 / 255f, 245 / 255f);
        colorKey[0].time = 0.0f;
        colorKey[1].color = new Color(199 / 255f, 233 / 255f, 192 / 255f);
        colorKey[1].time = 0.25f;
        colorKey[2].color = new Color(116 / 255f, 196 / 255f, 118 / 255f);
        colorKey[2].time = 0.5f;
        colorKey[3].color = new Color(35 / 255f, 139 / 255f, 69 / 255f);
        colorKey[3].time = 0.75f;
        colorKey[4].color = new Color(0 / 255f, 68 / 255f, 27 / 255f);
        colorKey[4].time = 1.0f;

        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
        GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 1.0f;
        alphaKey[1].time = 1.0f;

        // Set gradient keys
        gradient.SetKeys(colorKey, alphaKey);

        // Populate the color array
        for (int i = 0; i < 257; i++)
        {
            for (int j = 0; j < 257; j++)
            {
                float valueVegetation = floatArrayVegetation[i, j] / 255.0f;
                colorArrayVegetation[j * 257 + i] = gradient.Evaluate(valueVegetation);

                float valueInitialVegetation = floatArrayInitialVegetation[i, j] / 255.0f;
                colorArrayInitialVegetation[j * 257 + i] = gradient.Evaluate(valueInitialVegetation);
            }
        }

        // Apply Color array to Texture2D
        textureVegetation.SetPixels(colorArrayVegetation);
        textureVegetation.Apply();
        textureInitialVegetation.SetPixels(colorArrayInitialVegetation);
        textureInitialVegetation.Apply();

        // Convert Texture2D to Sprite
        Rect rect = new Rect(0, 0, 257, 257);
        Sprite spriteVegetation = Sprite.Create(textureVegetation, rect, new Vector2(0.5f, 0.5f));
        Sprite spriteInitialVegetation = Sprite.Create(textureInitialVegetation, rect, new Vector2(0.5f, 0.5f));

        // Assign Sprite to Image
        imageVegetation.sprite = spriteVegetation;
        imageInitialVegetation.sprite = spriteInitialVegetation;

    }

    private void OnDestroy()
    {
        _dataRequesterVegetation.Stop();
    }
}
