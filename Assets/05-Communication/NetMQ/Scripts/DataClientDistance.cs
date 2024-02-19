/****************************************************
 * File: DataClientDistance.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 11/09/2023
   * Project: Foot2Trail
   * Last update: 20/02/2023
*****************************************************/

using PositionBasedDynamics;
using UnityEngine;
using TMPro;

/// <summary>
/// Client to request data from the simulation.
/// </summary>
public class DataClientDistance : MonoBehaviour
{

    #region UI

    public TextMeshProUGUI textDistance;

    #endregion

    #region Read-only & Static Fields

    private ExportDistanceMap _dataRequesterDistance;

    #endregion
    
    private void Start()
    {
        _dataRequesterDistance = new ExportDistanceMap();
        _dataRequesterDistance.Start();

        _dataRequesterDistance.DistanceBytes = new byte[sizeof(float)];
    }

    private void Update()
    {
        // Initialize array
        float[,] floatArrayDistance = new float[1, 1];

        // Convert the byte array to a float array
        floatArrayDistance.FromBytes(_dataRequesterDistance.DistanceBytes);

        textDistance.text = "Distance: " + floatArrayDistance[0, 0].ToString("F2") + " m";
    }

    private void OnDestroy()
    {
        _dataRequesterDistance.Stop();
    }
}
