/****************************************************
 * File: DataClientYoung.cs
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
public class DataClientYoung : MonoBehaviour
{
    #region UI

    public UnityEngine.UI.Image imageYoung;

    #endregion

    #region Read-only & Static Fields

    private ExportYoungMap _dataRequesterYoung;

    double maxYoung = 1250000;
    double minYoung = 0;

    #endregion

    private void Start()
    {
        _dataRequesterYoung = new ExportYoungMap();
        _dataRequesterYoung.Start();

        _dataRequesterYoung.YoungBytes = new byte[TerrainDeformationMaster.PressureMapBytes.Length * sizeof(double)];
    }

    private void Update()
    {
        // Initialize array
        double[,] floatArrayYoung = new double[257, 257];
        double[,] floatArrayYoungNormalized = new double[257, 257];

        // Convert the byte array to a float array
        floatArrayYoung.FromBytes(_dataRequesterYoung.YoungBytes);

        // Separate into compression and accumulation and normalize
        for (int i = 0; i < floatArrayYoung.GetLength(0); i++)
        {
            for (int j = 0; j < floatArrayYoung.GetLength(1); j++)
            {
                floatArrayYoungNormalized[i, j] = ((floatArrayYoung[i, j] - minYoung) / (maxYoung - minYoung)) * 255;
            }
        }

        // Create a Texture2D
        Texture2D textureYoung = new Texture2D(257, 257);

        // Convert float array to Color array
        Color[] colorArrayYoung = new Color[257 * 257];
        Gradient gradient = new Gradient();

        // Populate the color keys at the relative time 0, 0.25, 0.5, 0.75 and 1 (0, 25, 50, 75 and 100%)
        GradientColorKey[] colorKey = new GradientColorKey[5];
        colorKey[0].color = new Color(247 / 255f, 251 / 255f, 255 / 255f);
        colorKey[0].time = 0.0f;
        colorKey[1].color = new Color(198 / 255f, 219 / 255f, 239 / 255f);
        colorKey[1].time = 0.25f;
        colorKey[2].color = new Color(107 / 255f, 174 / 255f, 214 / 255f);
        colorKey[2].time = 0.5f;
        colorKey[3].color = new Color(33 / 255f, 113 / 255f, 181 / 255f);
        colorKey[3].time = 0.75f;
        colorKey[4].color = new Color(8 / 255f, 48 / 255f, 107 / 255f);
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
                double valueYoung = floatArrayYoungNormalized[i, j] / 255.0f;
                colorArrayYoung[j * 257 + i] = gradient.Evaluate((float)valueYoung);
            }
        }

        // Apply Color array to Texture2D
        textureYoung.SetPixels(colorArrayYoung);
        textureYoung.Apply();

        // Convert Texture2D to Sprite
        Rect rect = new Rect(0, 0, 257, 257);
        Sprite spriteYoung = Sprite.Create(textureYoung, rect, new Vector2(0.5f, 0.5f));

        // Assign Sprite to Image
        imageYoung.sprite = spriteYoung;
    }


    private void OnDestroy()
    {
        _dataRequesterYoung.Stop();
    }
}
