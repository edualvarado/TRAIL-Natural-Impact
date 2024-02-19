/****************************************************
 * File: DataClientPressure.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 01/10/2020
   * Project: Foot2Trail
   * Last update: 20/02/2023
*****************************************************/

using UnityEngine;

/// <summary>
/// Client to request data from the simulation.
/// </summary>
public class DataClientPressure : MonoBehaviour
{
    #region UI

    public UnityEngine.UI.Image imagePressure;

    #endregion

    #region Read-only & Static Fields

    private ExportPressureMap _dataRequesterPressure;

    float maxPressure = 5000000f;
    float minPressure = 0;

    #endregion

    private void Start()
    {
        _dataRequesterPressure = new ExportPressureMap();
        _dataRequesterPressure.Start();

        _dataRequesterPressure.PressureBytes = new byte[TerrainDeformationMaster.PressureMapBytes.Length * sizeof(float)];
    }

    private void Update()
    {
        // Initialize array
        float[,] floatArrayPressure = new float[257, 257];
        float[,] floatArrayPressureNormalized = new float[257, 257];

        // Convert the byte array to a float array
        floatArrayPressure.FromBytes(_dataRequesterPressure.PressureBytes);

        // Separate into compression and accumulation and normalize
        for (int i = 0; i < floatArrayPressure.GetLength(0); i++)
        {
            for (int j = 0; j < floatArrayPressure.GetLength(1); j++)
            {
                floatArrayPressureNormalized[i, j] = ((floatArrayPressure[i, j] - maxPressure) / (minPressure - maxPressure)) * 255;
            }
        }

        // Create a Texture2D
        Texture2D texturePressure = new Texture2D(257, 257);

        // Convert float array to Color array
        Color[] colorArrayPressure = new Color[257 * 257];
        Gradient gradient = new Gradient();

        // Populate the color keys at the relative time 0, 0.25, 0.5, 0.75 and 1 (0, 25, 50, 75 and 100%)
        GradientColorKey[] colorKey = new GradientColorKey[5];
        colorKey[0].color = new Color(255 / 255f, 245 / 255f, 240 / 255f);
        colorKey[0].time = 0.0f;
        colorKey[1].color = new Color(252 / 255f, 187 / 255f, 161 / 255f);
        colorKey[1].time = 0.25f;
        colorKey[2].color = new Color(251 / 255f, 106 / 255f, 74 / 255f);
        colorKey[2].time = 0.5f;
        colorKey[3].color = new Color(203 / 255f, 24 / 255f, 29 / 255f);
        colorKey[3].time = 0.75f;
        colorKey[4].color = new Color(103 / 255f, 0 / 255f, 13 / 255f);
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
                float valuePressure = floatArrayPressure[i, j] / 255.0f;
                colorArrayPressure[j * 257 + i] = gradient.Evaluate(valuePressure);
            }
        }

        // Apply Color array to Texture2D
        texturePressure.SetPixels(colorArrayPressure);
        texturePressure.Apply();

        // Convert Texture2D to Sprite
        Rect rect = new Rect(0, 0, 257, 257);
        Sprite spritePressure = Sprite.Create(texturePressure, rect, new Vector2(0.5f, 0.5f));

        // Assign Sprite to Image
        imagePressure.sprite = spritePressure;
    }

    private void OnDestroy()
    {
        _dataRequesterPressure.Stop();
    }
}
