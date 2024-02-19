/****************************************************
 * File: TerrainBrushFootprint.cs
   * Author: Eduardo Alvarado
   * Email: alvaradopinero.eduardo@gmail.com
   * Date: 12/01/2024
   * Project: Foot2Trail
   * Last update: 12/01/2024
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract class to call brush for footprint drawing.
/// </summary>
public abstract class TerrainBrushFootprint : BrushFootprint
{
    #region Instance Methods

    public override void CallFootprint(float xLeft, float zLeft, float xRight, float zRight)
    {
        // 1. Pass the positions though here to convert them wrt Heightmap before calling final brush
        Vector3 gridLeft = terrain.World2Grid(xLeft, zLeft);
        Vector3 gridRight = terrain.World2Grid(xRight, zRight);

        // 2. Call Footprint method and filter it 
        DrawFootprint((int)gridLeft.x, (int)gridLeft.z, (int)gridRight.x, (int)gridRight.z);

        // 3. Save the terrain
        terrain.Save();
    }

    public override void StabilizeFootprint(float xLeft, float zLeft, float xRight, float zRight)
    {
        // 1. Pass the positions though here to convert them wrt Heightmap before calling final brush
        Vector3 gridLeft = terrain.World2Grid(xLeft, zLeft);
        Vector3 gridRight = terrain.World2Grid(xRight, zRight);

        // 2. Call Stabilization method
        DrawStabilizeFootprint((int)gridLeft.x, (int)gridLeft.z, (int)gridRight.x, (int)gridRight.z);

        // 3. Save the terrain
        terrain.Save();
    }

    public override void CallGaussianSingle(float x, float z, float smoothStrength, int smoothRadius)
    {
        // 1. Pass the positions though here to convert them wrt Heightmap before calling final brush
        Vector3 grid = terrain.World2Grid(x, z);

        // 2. Call Footprint method and filter it 
        DrawGaussianSingle((int)grid.x, (int)grid.z, smoothStrength, smoothRadius);

        // 3. Save the terrain
        terrain.Save();
    }


    public override void DrawFootprint(float xLeft, float zLeft, float xRight, float zRight)
    {
        DrawFootprint((int)xLeft, (int)zLeft, (int)xRight, (int)zRight);
    }

    public override void DrawStabilizeFootprint(float xLeft, float zLeft, float xRight, float zRight)
    {
        DrawStabilizeFootprint((int)xLeft, (int)zLeft, (int)xRight, (int)zRight);
    }

    public override void DrawGaussianSingle(float x, float z, float smoothStrength, int smoothRadius)
    {
        DrawGaussianSingle((int)x, (int)z, smoothStrength, smoothRadius);
    }

    #endregion
}