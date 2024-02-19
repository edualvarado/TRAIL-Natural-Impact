/****************************************************
 * File: DataClientHeightmap.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 01/10/2020
   * Project: Foot2Trail
   * Last update: 24/02/2023
*****************************************************/

using PositionBasedDynamics;
using System;
using UnityEngine;

/// <summary>
/// Client to request data from the simulation.
/// </summary>
public class DataClientHeightmap : MonoBehaviour
{
    #region UI

    public UnityEngine.UI.Image imageCompression;
    public UnityEngine.UI.Image imageAccumulation;

    #endregion

    #region Read-only & Static Fields

    private ExportHeightMap _dataRequesterHeightmap;

    float maxCompression = 0.0f;
    float minCompression = -0.05f;

    float maxAccumulation = 0.05f;
    float minAccumulation = 0.0f;

    #endregion

    private void Start()
    {
        _dataRequesterHeightmap = new ExportHeightMap();
        _dataRequesterHeightmap.Start();

        _dataRequesterHeightmap.HeightmapBytes = new byte[TerrainDeformationMaster.HeightMapBytes.Length * sizeof(float)];
    }

    private void Update()
    {
        // Initialize array
        float[,] floatArrayHeightmapDifference = new float[257, 257];

        float[,] floatArrayCompression = new float[257, 257];
        float[,] floatArrayAccumulation = new float[257, 257];

        float[,] floatArrayCompressionNormalized = new float[257, 257];
        float[,] floatArrayAccumulationNormalized = new float[257, 257];

        // Convert the byte array to a float array
        floatArrayHeightmapDifference.FromBytes(_dataRequesterHeightmap.HeightmapBytes);

        // Separate into compression and accumulation and normalize
        for (int i = 0; i < floatArrayHeightmapDifference.GetLength(0); i++)
        {
            for (int j = 0; j < floatArrayHeightmapDifference.GetLength(1); j++)
            {
                floatArrayCompression[i, j] = floatArrayHeightmapDifference[i, j] < 0 ? floatArrayHeightmapDifference[i, j] : 0;
                floatArrayAccumulation[i, j] = floatArrayHeightmapDifference[i, j] > 0 ? floatArrayHeightmapDifference[i, j] : 0;

                floatArrayCompressionNormalized[i, j] = ((floatArrayCompression[i, j] - maxCompression) / (minCompression - maxCompression)) * 255;
                floatArrayAccumulationNormalized[i, j] = ((floatArrayAccumulation[i, j] - minAccumulation) / (maxAccumulation - minAccumulation)) * 255;

            }
        }

        // Debug Max and Min
        //float max = floatArrayCompressionNormalized[0, 0];
        //float min = floatArrayCompressionNormalized[0, 0];
        //for (int i = 0; i < 257; i++)
        //{
        //    for (int j = 0; j < 257; j++)
        //    {
        //        max = Mathf.Max(max, floatArrayCompressionNormalized[i, j]);
        //        min = Mathf.Min(min, floatArrayCompressionNormalized[i, j]);
        //    }
        //}
        //Debug.Log("Max: " + max + ", Min: " + min);

        // Create a Texture2D
        Texture2D textureCompression = new Texture2D(257, 257);
        Texture2D textureAccumulation = new Texture2D(257, 257);

        // Convert float array to Color array
        Color[] colorArrayCompression = new Color[257 * 257];
        Color[] colorArrayAccumulation = new Color[257 * 257];
        Gradient gradientCompression = new Gradient();
        Gradient gradientAccumulation = new Gradient();

        // Populate the color keys at the relative time 0, 0.25, 0.5, 0.75 and 1 (0, 25, 50, 75 and 100%)
        GradientColorKey[] colorKeyCompression = new GradientColorKey[5];
        colorKeyCompression[0].color = new Color(255 / 255f, 245 / 255f, 240 / 255f);
        colorKeyCompression[0].time = 0.0f;
        colorKeyCompression[1].color = new Color(252 / 255f, 187 / 255f, 161 / 255f);
        colorKeyCompression[1].time = 0.25f;
        colorKeyCompression[2].color = new Color(251 / 255f, 106 / 255f, 74 / 255f);
        colorKeyCompression[2].time = 0.5f;
        colorKeyCompression[3].color = new Color(203 / 255f, 24 / 255f, 29 / 255f);
        colorKeyCompression[3].time = 0.75f;
        colorKeyCompression[4].color = new Color(103 / 255f, 0 / 255f, 13 / 255f);
        colorKeyCompression[4].time = 1.0f;


        // Populate the color keys at the relative time 0, 0.25, 0.5, 0.75 and 1 (0, 25, 50, 75 and 100%)
        GradientColorKey[] colorKeyAccumulation = new GradientColorKey[5];
        colorKeyAccumulation[0].color = new Color(247 / 255f, 251 / 255f, 255 / 255f);
        colorKeyAccumulation[0].time = 0.0f;
        colorKeyAccumulation[1].color = new Color(198 / 255f, 219 / 255f, 239 / 255f);
        colorKeyAccumulation[1].time = 0.25f;
        colorKeyAccumulation[2].color = new Color(107 / 255f, 174 / 255f, 214 / 255f);
        colorKeyAccumulation[2].time = 0.5f;
        colorKeyAccumulation[3].color = new Color(33 / 255f, 113 / 255f, 181 / 255f);
        colorKeyAccumulation[3].time = 0.75f;
        colorKeyAccumulation[4].color = new Color(8 / 255f, 48 / 255f, 107 / 255f);
        colorKeyAccumulation[4].time = 1.0f;

        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
        GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 1.0f;
        alphaKey[1].time = 1.0f;

        // Set gradient keys
        gradientCompression.SetKeys(colorKeyCompression, alphaKey);
        gradientAccumulation.SetKeys(colorKeyAccumulation, alphaKey);

        // Populate the color array Compression
        for (int i = 0; i < 257; i++)
        {
            for (int j = 0; j < 257; j++)
            {
                float valueCompression = floatArrayCompressionNormalized[i, j] / 255.0f;
                colorArrayCompression[j * 257 + i] = gradientCompression.Evaluate(valueCompression);

                float valueAccumulation = floatArrayAccumulationNormalized[i, j] / 255.0f;
                colorArrayAccumulation[j * 257 + i] = gradientAccumulation.Evaluate(valueAccumulation);
            }
        }

        // Apply Color array to Texture2D
        textureCompression.SetPixels(colorArrayCompression);
        textureCompression.Apply();
        textureAccumulation.SetPixels(colorArrayAccumulation);
        textureAccumulation.Apply();

        // Convert Texture2D to Sprite
        Rect rect = new Rect(0, 0, 257, 257);
        Sprite spriteCompression = Sprite.Create(textureCompression, rect, new Vector2(0.5f, 0.5f));
        Sprite spriteAccumulation = Sprite.Create(textureAccumulation, rect, new Vector2(0.5f, 0.5f));

        // Assign Sprite to Image
        imageCompression.sprite = spriteCompression;
        imageAccumulation.sprite = spriteAccumulation;

    }

    private void OnDestroy()
    {
        _dataRequesterHeightmap.Stop();
    }
}
