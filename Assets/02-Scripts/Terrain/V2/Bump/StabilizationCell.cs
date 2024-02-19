/****************************************************
 * File: StabilizationCell.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 07/11/2022
   * Project: RL-terrain-adaptation
   * Last update: 23/11/2022
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StabilizationCell
{
    #region Read-only & Static Fields

    private Vector3 posCenterCell;
    private Vector2 pos2DNeighbour;
    private float height;
    private float angle;
    private float heightDifference;
    private float weightStabilization;

    #endregion

    #region Instance Properties

    public Vector3 PosCenterCell
    {
        get { return posCenterCell; }
        set { posCenterCell = value; }
    }
    
    public Vector2 Pos2DNeighbour
    {
        get { return pos2DNeighbour; }
        set { pos2DNeighbour = value; }
    }
    
    public float Height
    {
        get { return height; }
        set { height = value; }
    }

    public float Angle
    {
        get { return angle; }
        set { angle = value; }
    }

    public float HeightDifference
    {
        get { return heightDifference; }
        set { heightDifference = value; }
    }

    public float WeigthStabilization
    {
        get { return weightStabilization; }
        set { weightStabilization = value; }
    }

    #endregion

    public StabilizationCell(Vector3 posCenterCell, Vector2 pos2DNeighbour, float height, float angle, float heightDifference, float weightStabilization)
    {
        this.posCenterCell = posCenterCell;
        this.pos2DNeighbour = pos2DNeighbour;
        this.height = height;
        this.angle = angle;
        this.heightDifference = heightDifference;
        this.weightStabilization = weightStabilization;
    }
}
