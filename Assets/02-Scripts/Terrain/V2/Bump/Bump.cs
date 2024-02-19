/****************************************************
 * File: Bump.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 07/11/2022
   * Project: RL-terrain-adaptation
   * Last update: 23/11/2022
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bump
{
    #region Read-only & Static Fields

    private Vector3 posNeighbour;
    private float distance;
    private float cosine;
    private float initialWeight;
    private float orientationWeight;

    #endregion

    #region Instance Properties

    public Vector3 PosNeighbour
    {
        get { return posNeighbour; }
        set { posNeighbour = value; }
    }

    public float Distance
    {
        get { return distance; }
        set { distance = value; }
    }

    public float Cosine
    {
        get { return cosine; }
        set { cosine = value; }
    }

    public float InitialWeight
    {
        get { return initialWeight; }
        set { initialWeight = value; }
    }

    public float OrientationWeight
    {
        get { return orientationWeight; }
        set { orientationWeight = value; }
    }

    #endregion

    public Bump(Vector3 posNeighbour, float distance, float cosine, float initialWeight, float orientationWeight)
    {
        this.posNeighbour = posNeighbour;
        this.distance = distance;
        this.cosine = cosine;
        this.initialWeight = initialWeight;
        this.orientationWeight = orientationWeight;

    }
}
