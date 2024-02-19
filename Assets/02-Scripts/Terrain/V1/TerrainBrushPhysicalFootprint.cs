﻿/****************************************************
 * File: TerrainBrushPhysicalFootprint.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 01/08/2021
   * Project: RL-terrain-adaptation
   * Last update: 07/12/2022
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract class to call brush for footprint drawing.
/// </summary>
public abstract class TerrainBrushPhysicalFootprint : BrushPhysicalFootprint
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

    public override void DrawFootprint(float xLeft, float zLeft, float xRight, float zRight)
    {
        DrawFootprint((int)xLeft, (int)zLeft, (int)xRight, (int)zRight);
    }
    
    public override void DrawStabilizeFootprint(float xLeft, float zLeft, float xRight, float zRight)
    {
        DrawStabilizeFootprint((int)xLeft, (int)zLeft, (int)xRight, (int)zRight);
    }

    #endregion
}
