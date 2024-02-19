/****************************************************
 * File: PhysicalFootprint.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 01/08/2021
   * Project: RL-terrain-adaptation
   * Last update: 07/12/2022
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Brush to create dynamic footprints on the heighmap terrain. 
/// First, it analyzes the cells to be affected based on the ray-cast and feet colliders.
/// Then, it calculates the displacement per cell based on contact area and character weight / force applied.
/// </summary>
public class PhysicalFootprint : TerrainBrushPhysicalFootprint
{
    #region Instance Fields
    
    [Header("Physically-based Footprints Deformation - (SET UP)")]
    public bool applyFootprints = false;
    public bool applyBumps = false;
    public bool applyModulation = false;
    public bool applyStabilization = false;
    public bool printSteps;

    [Header("Experimental - (SET UP)")]
    public bool applyStabilizationSecond = false; // Go to another level - bad performance

    [Header("Terrain Compression - (SET UP)")]
    public bool deformLeftFootOnly;
    [Range(10000, 10000000)] public double youngM = 1000000; // Reduced min from 100k to 10k
    public float originalLengthZero = 0.3f;

    [Header("Terrain Compression with Vegetation - (SET UP)")]
    [Range(10000, 10000000)] public double youngMGround = 1000000;
    [Range(10000, 100000000)] public double youngMVegetation = 1000000;
    public float alphaVegLeft;
    //[Range(0f, 1f)] public float testAlphaLeft;
    public double youngMLeft = 1000000; // Reduced min from 100k to 10k
    public float alphaVegRight;
    //[Range(0f, 1f)] public float testAlphaRight;
    public double youngMRight = 1000000; // Reduced min from 100k to 10k
    [Range(0f, 1f)] public float betaVeg = 1f;

    [Header("Vertical Displacement - (SET UP)")]
    [Range(0, 0.5f)] public float poissonR = 0.4f;

    [Header("Compression Grid - (SET UP)")]
    [Range(0, 15)] public int gridSize = 10;
    [Range(0f, 1f)] public float raycastDistance = 0.075f;
    [Range(0f, 1f)] public float offsetRay = 0.04f;

    [Header("Modulation")]
    public bool modulateLeftFootOnly;
    [Range(0f, 1f)] public float beta = 0.4f;
    public List<Bump> neighboursLeft = new List<Bump>();
    public List<Bump> neighboursRight = new List<Bump>();
    public float[,] weightsBumpLeftInit;
    public float[,] weightsBumpRightInit;
    public float[,] weightsBumpLeft;
    public float[,] weightsBumpRight;
    public float sumCosineLeft;
    public float sumCosineRight;
    public bool printModulationInfo;

    [Header("Modulation - Experimental")]
    public float thresholdProjection = 1f;
    public float maxForceMagnitude;
    public bool useForceMagnitudeAsBeta = false;

    [Header("Stabilization - (SET UP)")]
    public bool stabilizeLeftFootOnly;
    public float restingAngle = 10f;
    public float stepStabilization = 0.001f;
    // ---
    public List<Vector3> cellsToStabilizeFirstLeft = new List<Vector3>();
    public List<StabilizationCell> cellsStabilizationFirstLeft = new List<StabilizationCell>();
    public List<StabilizationCell> cellsStabilizationDescendentFirstLeft = new List<StabilizationCell>();
    public List<Vector3> cellsToStabilizeSecondLeft = new List<Vector3>();
    public List<StabilizationCell> cellsStabilizationSecondLeft = new List<StabilizationCell>();
    public List<StabilizationCell> cellsStabilizationDescendentSecondLeft = new List<StabilizationCell>();
    // ---
    public List<Vector3> cellsToStabilizeFirstRight = new List<Vector3>();
    public List<StabilizationCell> cellsStabilizationFirstRight = new List<StabilizationCell>();
    public List<StabilizationCell> cellsStabilizationDescendentFirstRight = new List<StabilizationCell>();
    public List<Vector3> cellsToStabilizeSecondRight = new List<Vector3>();
    public List<StabilizationCell> cellsStabilizationSecondRight = new List<StabilizationCell>();
    public List<StabilizationCell> cellsStabilizationDescendentSecondRight = new List<StabilizationCell>();
    // ---
    public bool showStabilizationLeft;
    public bool showStabilizationRight;
    public bool showLevelsContourGridSphere;
    public bool printStabilizationInfoLeft;
    public bool printStabilizationInfoRight;

    [Header("Grids - Debug")]
    public bool showGridDebugLeft = false;
    public bool showGridDebugRight = false;
    [Space(10)]
    public bool showGridBumpDebugLeft = false;
    public bool showGridBumpDebugRight = false;
    [Space(10)]
    public bool showGridBumpModulationNeighboursLeft = false;
    public bool showGridBumpModulationNeighboursRight = false;
    [Space(10)]
    public bool showVerticalIncreaseDeformationLeft = false;
    public bool showVerticalIncreaseDeformationRight = false;
    [Space(10)]
    public bool printTerrainInformation = false;

    [Header("Gaussian Filtering - (SET UP)")]
    public bool applyFilterLeft = false;
    public bool applyFilterRight = false;
    public int filterIterationsLeftFoot = 2;
    public int filterIterationsRightFoot = 2;
    [Space(10)]
    public int marginAroundGrid = 3;
    public bool isFilteredLeft = false;
    public bool isFilteredRight = false;
    //[Range(0, 5)] private int gridSizeKernel = 1;

    [Header("Grids - Number of hits")]
    public int counterHitsLeft;
    public int counterHitsRight;
    public List<Vector3> counterPositionsLeft = new List<Vector3>();
    public List<Vector3> counterPositionsRight = new List<Vector3>();
    [Space(10)]
    public int neighbourCellsLeft;
    public int neighbourCellsRight;
    public List<Vector3> neighboursPositionsLeft = new List<Vector3>();
    public List<Vector3> neighboursPositionsRight = new List<Vector3>();

    [Header("Grids - Contact Area Feet-Ground")]
    public float areaTotal = 0f;
    public float areaTotalLeft = 0f;
    public float areaTotalRight = 0f;
    [Space(10)]
    public float neighbourAreaTotal = 0f;
    public float neighbourAreaTotalLeft = 0f;
    public float neighbourAreaTotalRight = 0f;

    [Header("Terrain Deformation - Pressure")]
    public float pressureStress;
    [Space(5)]
    public float pressureStressLeft;
    public float pressureStressRight;

    [Header("Terrain Compression - Displacement")]
    public double heightCellDisplacementYoungLeft = 0f;
    public double heightCellDisplacementYoungRight = 0f;
    [Space(10)]
    public float displacementLeft;
    public float displacementRight;

    [Header("Vertical Displacement - Displacement")]
    public double bumpHeightDeformationLeft = 0f;
    public double bumpHeightDeformationRight = 0f;
    [Space(10)]
    public float bumpDisplacementLeft;
    public float bumpDisplacementRight;
    [Space(10)]
    public double bumpDisplacementWeightedLeft;
    public double bumpDisplacementWeightedRight;

    [Header("Arrays for Heightmaps - Displacement")]
    public float[,] heightMapLeft;
    public float[,] heightMapRight;
    public int[,] heightMapLeftBool;
    public int[,] heightMapRightBool;
    public float[,] heightMapLeftWorld;
    public float[,] heightMapRightWorld;
    public int[,] heightMapLeftBoolWorld;
    public int[,] heightMapRightBoolWorld;
    
    [Header("Terrain Deformation - Volume Rod Approximation")]
    [Space(20)]
    public double volumeOriginalLeft = 0f; // Original volume under left foot
    public double volumeOriginalRight = 0f; // Original volume under right foot
    public double volumeTotalLeft = 0f; // Volume left after deformation
    public double volumeTotalRight = 0f; // Volume left after deformation
    public double volumeVariationPoissonLeft; // Volume change due to compressibility of the material
    public double volumeVariationPoissonRight; // Volume change due to compressibility of the material
    public double volumeDifferenceLeft; // Volume difference pre/post deformation without taking into account compressibility
    public double volumeDifferenceRight; // Volume difference pre/post deformation without taking into account compressibility
    public double volumeNetDifferenceLeft; // Volume difference pre/post deformation taking into account compressibility
    public double volumeNetDifferenceRight; // Volume difference pre/post deformation taking into account compressibility
    public double volumeCellLeft; // Volume/cell distributed over countour
    public double volumeCellRight; // Volume/cell distributed over countour

    // AUX


    #endregion

    #region Read-only & Static Fields

    // Bump - [Header("Vertical Displcamenet Grid - (SET UP)")]
    private int offsetBumpGridFixed = 1;
    private int neighboursSearchAreaFixed = 1;
    private int nAround = 0; // TEST: If 0, transfer goes to next. Otherwise, transfer goes to more at the same time.
    [Range(0, 1)] private int numberLevels = 0;

    // Filter
    private int filterIterationsLeftCounter = 0;
    private int filterIterationsRightCounter = 0;

    // Contact Area
    private float oldAreaTotalLeft = 0f;
    private float oldAreaTotalRight = 0f;

    // Compression
    private double oldHeightCellDisplacementYoungLeft = 0f;
    private double oldHeightCellDisplacementYoungRight = 0f;

    // Vertical Displacement
    private float oldNeighbourAreaTotalLeft;
    private float oldNeighbourAreaTotalRight;

    // Force for bump
    private Vector3 forcePositionLeft;
    private Vector3 forcePositionLeft2D;
    private Vector3 forcePositionLeft2DWorld;
    private Vector3 forcePositionRight;
    private Vector3 forcePositionRight2D;
    private Vector3 forcePositionRight2DWorld;

    // Others
    private float[,] heightMapLeftFiltered;
    private float[,] heightMapRightFiltered;

    #endregion

    #region Instance Methods

    /// <summary>
    /// Method that takes the IK positions for each feet and apply displacement to ground.
    /// </summary>
    /// <param name="xLeft"></param>
    /// <param name="zLeft"></param>
    /// <param name="xRight"></param>
    /// <param name="zRight"></param>
    public override void DrawFootprint(int xLeft, int zLeft, int xRight, int zRight)
    {
        #region Initial Declarations

        // 0. Define number of levels based on selection
        if (applyStabilization == true && applyStabilizationSecond == false)
            numberLevels = 0;
        else if (applyStabilization == true && applyStabilizationSecond == true)
            numberLevels = 1;
        else if (applyStabilization == false && applyStabilizationSecond == true)
            Debug.LogError("Select Level 1 stabilization. Unpredictible results!");

        // 1. Use Master Script or UI to take values
        ChooseDeformationChoice(xLeft, zLeft, xRight, zRight);

        // 2. Reset counter hits and lists
        counterHitsLeft = 0;
        counterHitsRight = 0;
        neighbourCellsLeft = 0;
        neighbourCellsRight = 0;
        counterPositionsLeft.Clear();
        counterPositionsRight.Clear();
        neighboursPositionsLeft.Clear();
        neighboursPositionsRight.Clear();
        neighboursLeft.Clear();
        neighboursRight.Clear();
        
        // Only clear while doing the deformation. When jumping to Stabilization stage, we freeze the list
        if (RunningCoroutineDeformationLeft == true)
        {
            cellsToStabilizeFirstLeft.Clear();
            cellsToStabilizeSecondLeft.Clear();
        }

        // Only clear while doing the deformation. When jumping to Stabilization stage, we freeze the list
        if (RunningCoroutineDeformationRight == true)
        {
            cellsToStabilizeFirstRight.Clear();
            cellsToStabilizeSecondRight.Clear();
        }
   
        // 3. Heightmaps for each foot
        heightMapLeft = new float[2 * gridSize + 1, 2 * gridSize + 1];
        heightMapRight = new float[2 * gridSize + 1, 2 * gridSize + 1];
        heightMapLeftBool = new int[2 * gridSize + 1, 2 * gridSize + 1];
        heightMapRightBool = new int[2 * gridSize + 1, 2 * gridSize + 1];

        // Only reset while doing the deformation. When jumping to Stabilization stage, we freeze the array
        if (RunningCoroutineDeformationLeft == true)
        {
            heightMapLeftWorld = new float[(int)terrain.GridSize().x, (int)terrain.GridSize().z];
            heightMapLeftBoolWorld = new int[(int)terrain.GridSize().x, (int)terrain.GridSize().z];
        }

        // Only reset while doing the deformation. When jumping to Stabilization stage, we freeze the array
        if (RunningCoroutineDeformationRight == true)
        {
            heightMapRightWorld = new float[(int)terrain.GridSize().x, (int)terrain.GridSize().z];
            heightMapRightBoolWorld = new int[(int)terrain.GridSize().x, (int)terrain.GridSize().z];
        }

        // === Modulated Bump === //

        float[,] weightsBumpLeft = new float[2 * gridSize + 1, 2 * gridSize + 1];
        float[,] weightsBumpRight = new float[2 * gridSize + 1, 2 * gridSize + 1];
        float[,] weightsBumpLeftInit = new float[2 * gridSize + 1, 2 * gridSize + 1];
        float[,] weightsBumpRightInit = new float[2 * gridSize + 1, 2 * gridSize + 1];

        #endregion

        #region Terrain Declaration

        if (printSteps)
        {
            Debug.Log("1 - [INFO] Terrain Declaration");
        }
        
        // Warning: Supossing that terrain is squared!
        if (printTerrainInformation)
        {
            Debug.Log("[INFO] Length Terrain - X: " + terrain.TerrainSize().x);
            Debug.Log("[INFO] Length Terrain - Z: " + terrain.TerrainSize().z);
            Debug.Log("[INFO] Number of heightmap cells: " + (terrain.GridSize().x - 1));
            Debug.Log("[INFO] Lenght of one cell - X: " + (terrain.TerrainSize().x / (terrain.GridSize().x - 1)));
            Debug.Log("[INFO] Lenght of one cell - Z: " + (terrain.TerrainSize().z / (terrain.GridSize().z - 1)));
            Debug.Log("[INFO] Area of one cell: " + (terrain.TerrainSize().x / (terrain.GridSize().x - 1)) * (terrain.TerrainSize().z / (terrain.GridSize().z - 1)));
        }

        #endregion

        #region Contact Area / Contour Calculation

        if (printSteps)
        {
            Debug.Log("2 - [INFO] Contact Area Calculation");
            Debug.Log("3 - [INFO] Contour Area Calculation");
        }

        // -- 1st Option -- //

        // 1. Calculate number of hits
        CellsCounter(xLeft, zLeft, xRight, zRight, heightMapLeftBool, heightMapRightBool, heightMapLeftBoolWorld, heightMapRightBoolWorld);

        // -- 2nd Option -- //

        // 1. Calculate number of neighbours and initialize the weights for 1-distance contour
        //CellsAndContourCounter(xLeft, zLeft, xRight, zRight, heightMapLeftBool, heightMapRightBool, weightsBumpLeftInit, weightsBumpRightInit, heightMapLeftBoolWorld, heightMapRightBoolWorld, numberLevels);

        // 2. Terrain Deformation is affected by an increasing value of the contact area, therefore the deformation
        // will be defined by the maximum contact area in each frame
        oldAreaTotalLeft = ((counterHitsLeft) * AreaCell);
        if (oldAreaTotalLeft >= areaTotalLeft)
        {
            // Area of contact
            areaTotalLeft = ((counterHitsLeft) * AreaCell);

            // Volume under the foot for that recent calculated area
            volumeOriginalLeft = areaTotalLeft * (originalLengthZero);
        }

        oldAreaTotalRight = ((counterHitsRight) * AreaCell);
        if (oldAreaTotalRight >= areaTotalRight)
        {
            // Area of contact
            areaTotalRight = ((counterHitsRight) * AreaCell);

            // Volume under the foot for that recent calculated area
            volumeOriginalRight = areaTotalRight * (originalLengthZero);
        }

        // 3. Total Area and Volume for both feet
        areaTotal = areaTotalLeft + areaTotalRight;

        // ---

        ContourCounter(xLeft, zLeft, xRight, zRight, heightMapLeftBool, heightMapRightBool, weightsBumpLeftInit, weightsBumpRightInit, heightMapLeftBoolWorld, heightMapRightBoolWorld, numberLevels);

        // 2. Calculate the neighbour area for each foot
        oldNeighbourAreaTotalLeft = ((neighbourCellsLeft) * AreaCell);
        if (oldNeighbourAreaTotalLeft >= neighbourAreaTotalLeft)
        {
            // Area of bump
            neighbourAreaTotalLeft = ((neighbourCellsLeft) * AreaCell);
        }

        oldNeighbourAreaTotalRight = ((neighbourCellsRight) * AreaCell);
        if (oldNeighbourAreaTotalRight >= neighbourAreaTotalRight)
        {
            // Area of bump
            neighbourAreaTotalRight = ((neighbourCellsRight) * AreaCell);
        }

        // 3. Total Neighbour Area and Volume for both feet
        neighbourAreaTotal = neighbourAreaTotalLeft + neighbourAreaTotalRight;

        #endregion

        // ----

        #region New Modulated Bump

        if (applyModulation)
        {
            if (printSteps)
            {
                Debug.Log("4 - [INFO] Modulation");
            }

            //Debug.Log("LeftHeelHeight: " + LeftHeelHeight);
            //Debug.Log("LeftToeHeight: " + LeftToeHeight);

            // Left Foot Modulation
            if (LeftHeelHeight < LeftToeHeight)
            {
                // Reset Cosines
                sumCosineLeft = 0f;
                
                // 1. Print initial weights in the contour
                foreach (Vector3 pos in neighboursPositionsLeft)
                {
                    Vector3 originContourLeft = new Vector3(OriginLeft.x, OriginLeft.y - LeftHeelHeight, OriginLeft.z); // This will be the grounder checker projected on the plane
                    float angle = Vector3.SignedAngle((pos - originContourLeft), TotalForceLeftTerrainProjection, Vector3.up); // TEST
                    float cosineAngle = Mathf.Clamp(Mathf.Cos(angle * Mathf.Deg2Rad), 0f, 1f);

                    if (TotalForceLeftTerrainProjection.x < thresholdProjection && TotalForceLeftTerrainProjection.z < thresholdProjection)
                    {
                        cosineAngle = 0f;
                    }

                    //if (useForceMagnitudeAsBeta)
                    //{
                    //    beta = Mathf.Lerp(0f, 1f, TotalForceLeftTerrainProjection.magnitude / maxForceMagnitude);
                    //}

                    if (printModulationInfo)
                    {
                        Debug.Log("[INFO] Modulation - Angle: " + angle);
                        Debug.Log("[INFO] Modulation - cosineAngle: " + cosineAngle);
                        Debug.Log("[INFO] Modulation - TotalForceLeftTerrainProjection: " + TotalForceLeftTerrainProjection);
                    }
                    
                    sumCosineLeft += cosineAngle;

                    // Debug.Log("Adding to neighboursLeft");
                    // Position, distance, cosine, initial weight, orientation weight
                    neighboursLeft.Add(new Bump(pos, Vector3.SqrMagnitude(pos - originContourLeft), cosineAngle, (1f / neighbourCellsLeft), cosineAngle * 1f)); // TODO - Last weight should be cos * mag

                    if (showGridBumpModulationNeighboursLeft)
                    {
                        Debug.DrawRay(originContourLeft, (pos - originContourLeft), Color.green, Time.deltaTime);
                        Debug.DrawRay(originContourLeft, TotalForceLeftTerrainProjection, Color.red, Time.deltaTime);

                        Debug.DrawRay(pos, Vector3.up * (1f / neighbourCellsLeft), Color.yellow);

                        // Only for visualization purposes
                        if (cosineAngle > 0)
                            Debug.DrawRay(new Vector3(pos.x, pos.y + (1f / neighbourCellsLeft), pos.z), Vector3.up * cosineAngle, Color.cyan);
                        else
                            Debug.DrawRay(pos, Vector3.up * cosineAngle, Color.cyan);
                    }
                }
            }
            else
            {
                // Reset Cosines
                sumCosineLeft = 0f;
                
                // 1. Print initial weights in the contour
                foreach (Vector3 pos in neighboursPositionsLeft)
                {
                    Vector3 originContourLeft = new Vector3(OriginLeft.x, OriginLeft.y - LeftToeHeight, OriginLeft.z); // This will be the grounder checker projected on the plane
                    float angle = Vector3.SignedAngle((pos - originContourLeft), TotalForceLeftTerrainProjection, Vector3.up); // TEST
                    float cosineAngle = Mathf.Clamp(Mathf.Cos(angle * Mathf.Deg2Rad), 0f, 1f);

                    if (TotalForceLeftTerrainProjection.x < thresholdProjection && TotalForceLeftTerrainProjection.z < thresholdProjection)
                    {
                        cosineAngle = 0f;
                    }

                    //if (useForceMagnitudeAsBeta)
                    //{
                    //    beta = Mathf.Lerp(0f, 1f, TotalForceLeftTerrainProjection.magnitude / maxForceMagnitude);
                    //}

                    if (printModulationInfo)
                    {
                        Debug.Log("[INFO] Modulation - Angle: " + angle);
                        Debug.Log("[INFO] Modulation - cosineAngle: " + cosineAngle);
                        Debug.Log("[INFO] Modulation - TotalForceLeftTerrainProjection: " + TotalForceLeftTerrainProjection);
                    }
                    
                    sumCosineLeft += cosineAngle;

                    // Position, distance, cosine, initial weight, orientation weight
                    neighboursLeft.Add(new Bump(pos, Vector3.SqrMagnitude(pos - originContourLeft), cosineAngle, (1f / neighbourCellsLeft), cosineAngle * 1f)); // TODO - Last weight should be cos * mag

                    if (showGridBumpModulationNeighboursLeft)
                    {
                        Debug.DrawRay(originContourLeft, (pos - originContourLeft), Color.green, Time.deltaTime);
                        Debug.DrawRay(originContourLeft, TotalForceLeftTerrainProjection, Color.red, Time.deltaTime);

                        Debug.DrawRay(pos, Vector3.up * (1f / neighbourCellsLeft), Color.yellow);

                        // Only for visualization purposes
                        if (cosineAngle > 0)
                            Debug.DrawRay(new Vector3(pos.x, pos.y + (1f / neighbourCellsLeft), pos.z), Vector3.up * cosineAngle, Color.cyan);
                        else
                            Debug.DrawRay(pos, Vector3.up * cosineAngle, Color.cyan);
                    }
                }
            }

            // Right Foot Modulation
            if (RightHeelHeight < RightToeHeight)
            {
                // Reset Cosines
                sumCosineRight = 0f;

                // 1. Print initial weights in the contour
                foreach (Vector3 pos in neighboursPositionsRight)
                {
                    Vector3 originContourRight = new Vector3(OriginRight.x, OriginRight.y - RightHeelHeight, OriginRight.z); // This will be the grounder checker projected on the plane
                    float angle = Vector3.SignedAngle((pos - originContourRight), TotalForceRightTerrainProjection, Vector3.up); // TEST
                    float cosineAngle = Mathf.Clamp(Mathf.Cos(angle * Mathf.Deg2Rad), 0f, 1f);
                    
                    if (TotalForceRightTerrainProjection.x < thresholdProjection && TotalForceRightTerrainProjection.z < thresholdProjection)
                    {
                        cosineAngle = 0f;
                    }

                    //if (useForceMagnitudeAsBeta)
                    //{
                    //    beta = Mathf.Lerp(0f, 1f, TotalForceRightTerrainProjection.magnitude / maxForceMagnitude);
                    //}

                    if (printModulationInfo)
                    {
                        Debug.Log("[INFO] Modulation - Angle: " + angle);
                        Debug.Log("[INFO] Modulation - cosineAngle: " + cosineAngle);
                        Debug.Log("[INFO] Modulation - TotalForceRightTerrainProjection: " + TotalForceRightTerrainProjection);
                    }

                    sumCosineRight += cosineAngle;

                    // Position, distance, cosine, initial weight, orientation weight
                    neighboursRight.Add(new Bump(pos, Vector3.SqrMagnitude(pos - originContourRight), cosineAngle, (1f / neighbourCellsRight), cosineAngle * 1f)); // TODO - Last weight should be cos * mag

                    if (showGridBumpModulationNeighboursRight)
                    {
                        Debug.DrawRay(originContourRight, (pos - originContourRight), Color.green, Time.deltaTime);
                        Debug.DrawRay(originContourRight, TotalForceRightTerrainProjection, Color.red, Time.deltaTime);

                        Debug.DrawRay(pos, Vector3.up * (1f / neighbourCellsRight), Color.yellow);

                        // Only for visualization purposes
                        if (cosineAngle > 0)
                            Debug.DrawRay(new Vector3(pos.x, pos.y + (1f / neighbourCellsRight), pos.z), Vector3.up * cosineAngle, Color.cyan);
                        else
                            Debug.DrawRay(pos, Vector3.up * cosineAngle, Color.cyan);
                    }
                }
            }
            else
            {
                // Reset Cosines
                sumCosineRight = 0f;

                // 1. Print initial weights in the contour
                foreach (Vector3 pos in neighboursPositionsRight)
                {
                    Vector3 originContourRight = new Vector3(OriginRight.x, OriginRight.y - RightToeHeight, OriginRight.z); // This will be the grounder checker projected on the plane
                    //float angle = Vector3.Angle((pos - originContourRight), TotalForceRightTerrainProjection);
                    float angle = Vector3.SignedAngle((pos - originContourRight), TotalForceRightTerrainProjection, Vector3.up); // TEST
                    float cosineAngle = Mathf.Clamp(Mathf.Cos(angle * Mathf.Deg2Rad), 0f, 1f);

                    if (TotalForceRightTerrainProjection.x < thresholdProjection && TotalForceRightTerrainProjection.z < thresholdProjection)
                    {
                        cosineAngle = 0f;
                    }

                    //if (useForceMagnitudeAsBeta)
                    //{
                    //    beta = Mathf.Lerp(0f, 1f, TotalForceRightTerrainProjection.magnitude / maxForceMagnitude);
                    //}

                    if (printModulationInfo)
                    {
                        Debug.Log("[INFO] Modulation - Angle: " + angle);
                        Debug.Log("[INFO] Modulation - cosineAngle: " + cosineAngle);
                        Debug.Log("[INFO] Modulation - TotalForceRightTerrainProjection: " + TotalForceRightTerrainProjection);
                    }

                    sumCosineRight += cosineAngle;

                    // Position, distance, cosine, initial weight, orientation weight
                    neighboursRight.Add(new Bump(pos, Vector3.SqrMagnitude(pos - originContourRight), cosineAngle, (1f / neighbourCellsRight), cosineAngle * 1f)); // TODO - Last weight should be cos * mag

                    if (showGridBumpModulationNeighboursRight)
                    {
                        Debug.DrawRay(originContourRight, (pos - originContourRight), Color.green, Time.deltaTime);
                        Debug.DrawRay(originContourRight, TotalForceRightTerrainProjection, Color.red, Time.deltaTime);

                        Debug.DrawRay(pos, Vector3.up * (1f / neighbourCellsRight), Color.yellow);

                        // Only for visualization purposes
                        if (cosineAngle > 0)
                            Debug.DrawRay(new Vector3(pos.x, pos.y + (1f / neighbourCellsRight), pos.z), Vector3.up * cosineAngle, Color.cyan);
                        else
                            Debug.DrawRay(pos, Vector3.up * cosineAngle, Color.cyan);
                    }
                }
            }


            // Define final modulation weights for each neighbour cell
            for (int zi = -gridSize + offsetBumpGridFixed; zi <= gridSize - offsetBumpGridFixed; zi++)
            {
                for (int xi = -gridSize + offsetBumpGridFixed; xi <= gridSize - offsetBumpGridFixed; xi++)
                {
                    // CONTOUR (BOOL = 2) - CHANGED
                    if (heightMapLeftBool[zi + gridSize, xi + gridSize] == 2)
                    {
                        Vector3 rayGridLeft = new Vector3(xLeft + xi, terrain.Get(xLeft + xi, zLeft + zi), zLeft + zi);
                        Vector3 rayGridWorldLeft = terrain.Grid2World(rayGridLeft);

                        Bump nCell = neighboursLeft.Find(x => x.PosNeighbour == rayGridWorldLeft); // TODO: IMPROVE! Little afraid of doing this, might be unstable.

                        // We modulate using the cosine angle normalizing it by the sum of the cosines
                        if (sumCosineLeft != 0)
                            weightsBumpLeft[zi + gridSize, xi + gridSize] = nCell.OrientationWeight / sumCosineLeft;
                        else
                            weightsBumpLeft[zi + gridSize, xi + gridSize] = 0f;
                        
                        //Debug.DrawRay(nCell.PosNeighbour, Vector3.up * nCell.OrientationWeight, Color.cyan);
                    }

                    // CONTOUR (BOOL = 2) - CHANGED
                    if (heightMapRightBool[zi + gridSize, xi + gridSize] == 2)
                    {
                        Vector3 rayGridRight = new Vector3(xRight + xi, terrain.Get(xRight + xi, zRight + zi), zRight + zi);
                        Vector3 rayGridWorldRight = terrain.Grid2World(rayGridRight);

                        Bump nCell = neighboursRight.Find(x => x.PosNeighbour == rayGridWorldRight); // TODO: IMPROVE! Little afraid of doing this, might be unstable.

                        // We modulate using the cosine angle normalizing it by the sum of the cosines
                        if (sumCosineRight != 0)
                            weightsBumpRight[zi + gridSize, xi + gridSize] = nCell.OrientationWeight / sumCosineRight;
                        else
                            weightsBumpRight[zi + gridSize, xi + gridSize] = 0f;

                        //Debug.DrawRay(nCell.PosNeighbour, Vector3.up * nCell.OrientationWeight, Color.cyan);
                    }
                }
            }

            // TEST: Check if cosine sum up to 1
            //float sum = 0f;
            //for (int zi = -gridSize + offsetBumpGridFixed; zi <= gridSize - offsetBumpGridFixed; zi++)
            //{
            //    for (int xi = -gridSize + offsetBumpGridFixed; xi <= gridSize - offsetBumpGridFixed; xi++)
            //    {
            //        if (heightMapLeftBool[zi + gridSize, xi + gridSize] == 1)
            //        {
            //            sum += weightsBumpLeft[zi + gridSize, xi + gridSize];
            //        }
            //    }
            //}
            //Debug.Log("SUM OF COSINES SHOULD BE 1: " + sum);
        }

        #endregion

        // ----

        #region Physics Calculation

        if (printSteps)
        {
            Debug.Log("5 - [INFO] Physics Calculation");
        }

        // 1. Calculate Pressure applicable per frame - if no contact, there is no pressure
        // The three values should be similar, since pressure is based on the contact area

        // Pressure by Left Foot
        if (counterHitsLeft == 0)
            pressureStressLeft = 0f;
        else
            pressureStressLeft = (TotalForceLeftVertical) / areaTotalLeft;

        // Pressure by Right Foot
        if (counterHitsRight == 0)
            pressureStressRight = 0f;
        else
            pressureStressRight = (TotalForceRightVertical) / areaTotalRight;

        // TEST: Bringing pressure to DeformTerrainMaster
        PressureLeft = pressureStressLeft;
        PressureRight = pressureStressRight;

        // 2. Total Pressure
        pressureStress = pressureStressLeft + pressureStressRight;

        #endregion

        #region Deformation Calculation

        if (printSteps)
        {
            Debug.Log("6 - [INFO] Deformation Calculation");
        }

        // Given area, pressure and terrain parameters, we calculate the displacement on the terrain
        // The decrement will depend also on the ContactTime used to calculate the corresponding force

        // 1. Estimate target deformation for compression and accumulation
        if (base.DeformationChoice == sourceDeformation.useManualValuesWithVegetation)
            EstimateTargetDeformationsVegetation();
        else
            EstimateTargetDeformations();

        // 2. Given the entire deformation in Y, we calculate the corresponding frame-based deformation based on the frame-time.
        displacementLeft = (Time.deltaTime * (float)heightCellDisplacementYoungLeft) / ContactTime;
        displacementRight = (Time.deltaTime * (float)heightCellDisplacementYoungRight) / ContactTime;

        // 3. Given the  deformation in Y for the bump, we calculate the corresponding frame-based deformation based on the frame-time.
        bumpDisplacementLeft = (Time.deltaTime * (float)bumpHeightDeformationLeft) / ContactTime;
        bumpDisplacementRight = (Time.deltaTime * (float)bumpHeightDeformationRight) / ContactTime;

        #endregion
        
        #region Apply Deformation

        if (printSteps)
        {
            Debug.Log("7 - [INFO] Starting Coroutines (Decrease and Increase)");
        } 

        // 2D iteration Deformation
        // Once we have the displacement, we saved the actual result of applying it to the terrain (only when the foot is grounded)    
        if (IsLeftFootGrounded && RunningCoroutineDeformationLeft == true)
        {
            // Start deformation and vertical displacement
            StartCoroutine(DecreaseTerrainLeftNew(heightMapLeft, heightMapLeftBool, weightsBumpLeftInit, weightsBumpLeft, xLeft, zLeft));
            StartCoroutine(IncreaseTerrainLeftNew(heightMapLeft, heightMapLeftBool, weightsBumpLeftInit, weightsBumpLeft, xLeft, zLeft));
        }
        else
        {
            // When the time is over or lift the foot, deformation stops
            StopCoroutine(DecreaseTerrainLeftNew(heightMapLeft, heightMapLeftBool, weightsBumpLeftInit, weightsBumpLeft, xLeft, zLeft));
            StopCoroutine(IncreaseTerrainLeftNew(heightMapLeft, heightMapLeftBool, weightsBumpLeftInit, weightsBumpLeft, xLeft, zLeft));
        }

        if (IsRightFootGrounded && RunningCoroutineDeformationRight == true)
        {
            if (!deformLeftFootOnly)
            {
                // Start deformation and vertical displacement
                StartCoroutine(DecreaseTerrainRightNew(heightMapRight, heightMapRightBool, weightsBumpRightInit, weightsBumpRight, xRight, zRight));
                StartCoroutine(IncreaseTerrainRightNew(heightMapRight, heightMapRightBool, weightsBumpRightInit, weightsBumpRight, xRight, zRight)); 
            }
        }
        else
        {
            if (!deformLeftFootOnly)
            {
                // When the time is over or lift the foot, deformation stops
                StopCoroutine(DecreaseTerrainRightNew(heightMapRight, heightMapRightBool, weightsBumpRightInit, weightsBumpRight, xRight, zRight));
                StopCoroutine(IncreaseTerrainRightNew(heightMapRight, heightMapRightBool, weightsBumpRightInit, weightsBumpRight, xRight, zRight));
            }
        }


        #endregion

        // ----

        // To solve the discontinuity problem, filter is applied after each terrain displacement per-frame, not at the end.

        #region Apply Smoothing

        /*
        if (printSteps)
        {
            Debug.Log("8 - [INFO] Apply Smoothing");
        }
        
        // 1. First smoothing version
        if (applyFilterLeft)
        {
            // 2. Provisional: When do we smooth?
            if (IsLeftFootGrounded)
            {
                if (!isFilteredLeft)
                {
                    NewFilterHeightMap(xLeft, zLeft, heightMapLeft);
                    filterIterationsLeftCounter++;
                }

                if (filterIterationsLeftCounter >= filterIterationsLeftFoot)
                {
                    isFilteredLeft = true;
                }
            }
            else
            {
                isFilteredLeft = false;
                filterIterationsLeftCounter = 0;
            }
        }

        // 1. First smoothing version
        if (applyFilterRight)
        {
            // 2. Provisional: When do we smooth?
            if (IsRightFootGrounded)
            {
                if (!isFilteredRight)
                {
                    NewFilterHeightMap(xRight, zRight, heightMapRight);
                    filterIterationsRightCounter++;
                }

                if (filterIterationsRightCounter >= filterIterationsRightFoot)
                {
                    isFilteredRight = true;
                }
            }
            else
            {
                isFilteredRight = false;
                filterIterationsRightCounter = 0;
            }
        }
        */

        #endregion
    }

    public override void DrawStabilizeFootprint(int xLeft, int zLeft, int xRight, int zRight)
    {
        #region Stabilization

        if (printSteps)
        {
            Debug.Log("X - [INFO] Stabilization");
        }

        // Reset lists
        if (RunningCoroutineStabilizationLeft == true)
        {
            cellsStabilizationFirstLeft.Clear();
            cellsStabilizationDescendentFirstLeft.Clear();

            cellsStabilizationSecondLeft.Clear();
            cellsStabilizationDescendentSecondLeft.Clear(); 
        }

        if (RunningCoroutineStabilizationRight == true)
        {
            cellsStabilizationFirstRight.Clear();
            cellsStabilizationDescendentFirstRight.Clear();

            cellsStabilizationSecondRight.Clear();
            cellsStabilizationDescendentSecondRight.Clear(); 
        }

        if (applyStabilization)
        {
            if (RunningCoroutineStabilizationLeft == true)
            {
                // 1) Create list of cells to stabilize
                StabilizationCounterLeft(cellsToStabilizeFirstLeft, heightMapLeftBoolWorld, nAround);

                // 1) Apply stabilization
                Debug.Log(" - Start Coroutine Stabilization FIRST LEFT");
                StartCoroutine(StabilizationLeft(heightMapLeftWorld, cellsStabilizationDescendentFirstLeft));
            }
            else
            {
                // 1) Stop stabilization
                Debug.Log(" - Stop Coroutine Stabilization FIRST LEFT");
                StopCoroutine(StabilizationLeft(heightMapLeftWorld, cellsStabilizationDescendentFirstLeft));
            }

            if (RunningCoroutineStabilizationRight == true)
            {
                if (!stabilizeLeftFootOnly)
                {
                    // 1) Create list of cells to stabilize
                    StabilizationCounterRight(cellsToStabilizeFirstRight, heightMapRightBoolWorld, nAround);

                    // 1) Apply stabilization
                    Debug.Log(" - Start Coroutine Stabilization FIRST RIGHT");
                    StartCoroutine(StabilizationRight(heightMapRightWorld, cellsStabilizationDescendentFirstRight)); 
                }
            }
            else
            {
                if (!stabilizeLeftFootOnly)
                {
                    // 1) Stop stabilization
                    Debug.Log(" - Stop Coroutine Stabilization FIRST RIGHT");
                    StopCoroutine(StabilizationRight(heightMapRightWorld, cellsStabilizationDescendentFirstRight));
                }
            }

            // WARNING: Experimental!
            if (applyStabilizationSecond)
            {
                // 2) Create list of cells to stabilize
                StabilizationCounterLeft(cellsToStabilizeSecondLeft, heightMapLeftBoolWorld, nAround);

                if (!stabilizeLeftFootOnly)
                {
                    StabilizationCounterRight(cellsToStabilizeSecondRight, heightMapRightBoolWorld, nAround);
                }

                // 2) Apply stabilization
                if (RunningCoroutineStabilizationLeft == true)
                {
                    Debug.Log(" - Start Coroutine Stabilization SECOND LEFT");
                    StartCoroutine(StabilizationLeft(heightMapLeftWorld, cellsStabilizationDescendentSecondLeft));
                }
                else
                {
                    Debug.Log(" - Stop Coroutine Stabilization SECOND LEFT");
                    StopCoroutine(StabilizationLeft(heightMapLeftWorld, cellsStabilizationDescendentSecondLeft));
                }

                // 2) Apply stabilization
                if (RunningCoroutineStabilizationRight == true)
                {
                    if (!stabilizeLeftFootOnly)
                    {
                        Debug.Log(" - Start Coroutine Stabilization SECOND RIGHT");
                        StartCoroutine(StabilizationRight(heightMapRightWorld, cellsStabilizationDescendentSecondRight));
                    }
                }
                else
                {
                    if (!stabilizeLeftFootOnly)
                    {
                        Debug.Log(" - Stop Coroutine Stabilization SECOND RIGHT");
                        StopCoroutine(StabilizationRight(heightMapRightWorld, cellsStabilizationDescendentSecondRight));
                    }
                }
            }
        }

        #endregion
    }

    private void ChooseDeformationChoice(int xLeft, int zLeft, int xRight, int zRight)
    {
        // 1. If activated, takes the prefab information from the master script
        if (base.DeformationChoice == sourceDeformation.useTerrainPrefabs)
        {
            youngM = YoungModulus;
            poissonR = PoissonRatio;
            applyBumps = ActivateBump;

            if (FilterIterations != 0)
            {
                applyFilterLeft = true;
                applyFilterRight = true;
                filterIterationsLeftFoot = FilterIterations;
                filterIterationsRightFoot = FilterIterations;
            }
            else if (FilterIterations == 0)
            {
                applyFilterLeft = false;
                applyFilterRight = false;
            }
        }

        // 2. If activated, takes the UI information from the interface
        if (base.DeformationChoice == sourceDeformation.useUI)
        {
            applyFootprints = ActivateToggleDef.isOn;
            applyBumps = ActivateToggleBump.isOn;
            applyFilterLeft = ActivateToggleGauss.isOn;
            applyFilterRight = ActivateToggleGauss.isOn;

            showGridDebugLeft = ActivateToggleShowGrid.isOn;
            showGridDebugRight = ActivateToggleShowGrid.isOn;
            showGridBumpDebugLeft = ActivateToggleShowBump.isOn;
            showGridBumpDebugRight = ActivateToggleShowBump.isOn;

            youngM = YoungSlider.value;
            poissonR = PoissonSlider.value;
            filterIterationsLeftFoot = (int)IterationsSlider.value;
            filterIterationsRightFoot = (int)IterationsSlider.value;
        }

        // 3. Include effect of having vegetation
        if(base.DeformationChoice == sourceDeformation.useManualValuesWithVegetation)
        {
            alphaVegLeft = AlphaVegetation[xLeft, zLeft];
            alphaVegRight = AlphaVegetation[xRight, zRight];

            //youngMLeft = betaVeg * youngMGround + (1 - betaVeg) * youngMVegetation * alphaVegLeft;
            //youngMRight = betaVeg * youngMGround + (1 - betaVeg) * youngMVegetation * alphaVegRight;
            youngMLeft = youngMGround + youngMVegetation * alphaVegLeft;
            youngMRight = youngMGround + youngMVegetation * alphaVegRight;

            // TEST
            YoungModulusGround = youngMGround;
            YoungModulusVegetation = youngMVegetation;
        }
    }

    /// <summary>
    /// Does not work right!
    /// </summary>
    private void CellsAndContourCounter(int xLeft, int zLeft, int xRight, int zRight, int[,] heightMapLeftBool, int[,] heightMapRightBool, float[,] weightsBumpLeftInit, float[,] weightsBumpRightInit, int[,] heightMapLeftBoolWorld, int[,] heightMapRightBoolWorld, int n)
    {
        // 1. We don't need to check the whole grid - just in the inner grid is enough
        for (int zi = -gridSize + offsetBumpGridFixed + n; zi <= gridSize - offsetBumpGridFixed - n; zi++)
        {
            for (int xi = -gridSize + offsetBumpGridFixed + n; xi <= gridSize - offsetBumpGridFixed - n; xi++)
            {
                // TEST 
                // -----

                // A. Calculate each cell position wrt World and Heightmap - Left Foot
                // The sensors that counts the number of hits always remain on the surface
                Vector3 rayGridLeft = new Vector3(xLeft + xi, terrain.Get(xLeft + xi, zLeft + zi) - offsetRay, zLeft + zi);
                Vector3 rayGridWorldLeft = terrain.Grid2World(rayGridLeft);

                // A. Calculate each cell position wrt World and Heightmap - Right Foot
                // The sensors that counts the number of hits always remain on the surface
                Vector3 rayGridRight = new Vector3(xRight + xi, terrain.Get(xRight + xi, zRight + zi) - offsetRay, zRight + zi);
                Vector3 rayGridWorldRight = terrain.Grid2World(rayGridRight);

                //------//

                // B. Create each ray for the grid (wrt World) - Left
                RaycastHit leftFootHit;
                Ray upRayLeftFoot = new Ray(rayGridWorldLeft, Vector3.up);

                // B. Create each ray for the grid (wrt World) - Right
                RaycastHit rightFootHit;
                Ray upRayRightFoot = new Ray(rayGridWorldRight, Vector3.up);

                //------//

                // C. If hits the Left Foot, increase counter and add cell to be affected
                if (LeftFootCollider.Raycast(upRayLeftFoot, out leftFootHit, raycastDistance))
                {
                    // Cell contacting directly (BOOL = 1) - CHANGED
                    heightMapLeftBool[zi + gridSize, xi + gridSize] = 1;

                    // Only update while doing the deformation. When jumping to Stabilization stage, we freeze the array
                    if (RunningCoroutineDeformationLeft == true)
                    {
                        heightMapLeftBoolWorld[(int)rayGridLeft.z, (int)rayGridLeft.x] = 1;
                    }

                    // Add counter
                    counterHitsLeft++;

                    if (showGridDebugLeft)
                        Debug.DrawRay(rayGridWorldLeft, Vector3.up * raycastDistance, Color.blue);
                }
                else
                {
                    // No contact (BOOL = 0)
                    heightMapLeftBool[zi + gridSize, xi + gridSize] = 0;

                    // Only update while doing the deformation. When jumping to Stabilization stage, we freeze the array
                    if (RunningCoroutineDeformationLeft)
                    {
                        heightMapLeftBoolWorld[(int)rayGridLeft.z, (int)rayGridLeft.x] = 0;
                    }

                    if (showGridDebugLeft)
                        Debug.DrawRay(rayGridWorldLeft, Vector3.up * raycastDistance, Color.red);
                }

                // C. If hits the Right Foot, increase counter and add cell to be affected
                if (RightFootCollider.Raycast(upRayRightFoot, out rightFootHit, raycastDistance))
                {
                    // Cell contacting directly (BOOL = 1) - CHANGED
                    heightMapRightBool[zi + gridSize, xi + gridSize] = 1;

                    // Only update while doing the deformation. When jumping to Stabilization stage, we freeze the array
                    if (RunningCoroutineDeformationRight == true)
                    {
                        heightMapRightBoolWorld[(int)rayGridRight.z, (int)rayGridRight.x] = 1;
                    }

                    // Add counter
                    counterHitsRight++;

                    if (showGridDebugRight)
                        Debug.DrawRay(rayGridWorldRight, Vector3.up * raycastDistance, Color.blue);
                }
                else
                {
                    // No contact (BOOL = 0)
                    heightMapRightBool[zi + gridSize, xi + gridSize] = 0;

                    // Only update while doing the deformation. When jumping to Stabilization stage, we freeze the array
                    if (RunningCoroutineDeformationRight)
                    {
                        heightMapRightBoolWorld[(int)rayGridRight.z, (int)rayGridRight.x] = 0;
                    }

                    if (showGridDebugRight)
                        Debug.DrawRay(rayGridWorldRight, Vector3.up * raycastDistance, Color.red);
                }

                // -----

                if (IsLeftFootGrounded)
                {
                    // 2. For each level, we build the grid with 1 on contact and 2, 3...on the contour
                    for (int i = 0; i <= n; i++)
                    {
                        // A. If the cell was not in contact, it's a potential neighbour (contour) cell
                        if (heightMapLeftBool[zi + gridSize, xi + gridSize] == 0)
                        {
                            // B. Only checking adjacent cells - increasing this would allow increasing the area of the bump
                            for (int zi_sub = -neighboursSearchAreaFixed - i; zi_sub <= neighboursSearchAreaFixed + i; zi_sub = zi_sub + 1 + i)
                            {
                                for (int xi_sub = -neighboursSearchAreaFixed - i; xi_sub <= neighboursSearchAreaFixed + i; xi_sub = xi_sub + 1 + i)
                                {
                                    // C. If there is a contact point around the cell (BOOL = 1) convert to BOOL = 2 - CHANGED
                                    // By going through the levels, you look farther for contact 1 and convert to BOOL = 3, 4, 5...
                                    // TODO: It would be simpler to look for the adjacent cells
                                    if (heightMapLeftBool[zi + zi_sub + gridSize, xi + xi_sub + gridSize] == 1)
                                    {
                                        // TEST
                                        //Vector3 rayGridLeft = new Vector3(xLeft + xi, terrain.Get(xLeft + xi, zLeft + zi) - offsetRay, zLeft + zi);
                                        //Vector3 rayGridWorldLeft = terrain.Grid2World(rayGridLeft);

                                        // D. Mark that cell as a countour point (BOOL = 2) - CHANGED
                                        heightMapLeftBool[zi + gridSize, xi + gridSize] = 2 + i;

                                        // Only update while doing the deformation. When jumping to Stabilization stage, we freeze the array.
                                        if (RunningCoroutineDeformationLeft == true)
                                        {
                                            heightMapLeftBoolWorld[(int)rayGridLeft.z, (int)rayGridLeft.x] = 2 + i;
                                        }

                                        break;
                                    }
                                }
                            }
                        }
                    }

                    // TODO: Iterate instead of hardcoding.
                    // For now, we only consider the first and second level of the bump.

                    // If the cell is the first inmediate contour point (BOOL = 2) - CHANGED
                    if (heightMapLeftBool[zi + gridSize, xi + gridSize] == 2)
                    {
                        // Each neightbour cell in world space
                        //Vector3 rayGridLeft = new Vector3(xLeft + xi, terrain.Get(xLeft + xi, zLeft + zi) - offsetRay, zLeft + zi); // Remove offset

                        // TEST
                        //Vector3 rayGridLeft = new Vector3(xLeft + xi, terrain.Get(xLeft + xi, zLeft + zi), zLeft + zi);
                        //Vector3 rayGridWorldLeft = terrain.Grid2World(rayGridLeft);

                        if (showLevelsContourGridSphere)
                            Debug.DrawRay(rayGridWorldLeft, Vector3.up * 1 / heightMapLeftBool[zi + gridSize, xi + gridSize], Color.yellow); // First level (bool should be 2)

                        // Store array for modulation
                        neighboursPositionsLeft.Add(rayGridWorldLeft);

                        // Counter neightbours
                        neighbourCellsLeft++;

                        // Store array for stabilization. Only update while doing the deformation. When jumping to Stabilization stage, we freeze the list.
                        if (RunningCoroutineDeformationLeft == true)
                        {
                            cellsToStabilizeFirstLeft.Add(rayGridLeft);
                        }
                    }
                    else if (heightMapLeftBool[zi + gridSize, xi + gridSize] == 3) // For next levels (used only for stabilization)
                    {
                        // Each neightbour cell in world space
                        //Vector3 rayGridLeft = new Vector3(xLeft + xi, terrain.Get(xLeft + xi, zLeft + zi) - offsetRay, zLeft + zi); // Remove offset

                        // TEST
                        //Vector3 rayGridLeft = new Vector3(xLeft + xi, terrain.Get(xLeft + xi, zLeft + zi), zLeft + zi);
                        //Vector3 rayGridWorldLeft = terrain.Grid2World(rayGridLeft);

                        if (showLevelsContourGridSphere)
                            Debug.DrawRay(rayGridWorldLeft, Vector3.up * 1 / heightMapLeftBool[zi + gridSize, xi + gridSize], Color.red); // First level (bool should be 2)

                        // Store array for stabilization. Only update while doing the deformation. When jumping to Stabilization stage, we freeze the list.
                        if (RunningCoroutineDeformationLeft == true)
                        {
                            cellsToStabilizeSecondLeft.Add(rayGridLeft);
                        }
                    }
                }

                if (IsRightFootGrounded)
                {
                    // 2. For each level, we build the grid with 1 on contact and 2, 3...on the contour
                    for (int i = 0; i <= n; i++)
                    {
                        // A. If the cell was not in contact, it's a potential neighbour (contour) cell
                        if (heightMapRightBool[zi + gridSize, xi + gridSize] == 0)
                        {
                            // B. Only checking adjacent cells - increasing this would allow increasing the area of the bump
                            for (int zi_sub = -neighboursSearchAreaFixed - i; zi_sub <= neighboursSearchAreaFixed + i; zi_sub = zi_sub + 1 + i)
                            {
                                for (int xi_sub = -neighboursSearchAreaFixed - i; xi_sub <= neighboursSearchAreaFixed + i; xi_sub = xi_sub + 1 + i)
                                {
                                    // C. If there is a contact point around the cell (BOOL = 1) convert to BOOL = 2 - CHANGED
                                    // By going through the levels, you look farther for contact 1 and convert to BOOL = 3, 4, 5...
                                    // TODO: It would be simpler to look for the adjacent cells
                                    if (heightMapRightBool[zi + zi_sub + gridSize, xi + xi_sub + gridSize] == 1)
                                    {
                                        // TEST
                                        //Vector3 rayGridRight = new Vector3(xRight + xi, terrain.Get(xRight + xi, zRight + zi) - offsetRay, zRight + zi);
                                        //Vector3 rayGridWorldRight = terrain.Grid2World(rayGridRight);

                                        // D. Mark that cell as a countour point (BOOL = 2) - CHANGED
                                        heightMapRightBool[zi + gridSize, xi + gridSize] = 2 + i;

                                        // Only update while doing the deformation. When jumping to Stabilization stage, we freeze the array.
                                        if (RunningCoroutineDeformationRight == true)
                                        {
                                            heightMapRightBoolWorld[(int)rayGridRight.z, (int)rayGridRight.x] = 2 + i;
                                        }

                                        break;
                                    }
                                }
                            }
                        }
                    }

                    // TODO: Iterate instead of hardcoding.
                    // For now, we only consider the first and second level of the bump.

                    // If the cell is the first inmediate contour point (BOOL = 2) - CHANGED
                    if (heightMapRightBool[zi + gridSize, xi + gridSize] == 2)
                    {
                        // Each neightbour cell in world space
                        //Vector3 rayGridRight = new Vector3(xRight + xi, terrain.Get(xRight + xi, zRight + zi) - offsetRay, zRight + zi); // Remove offset

                        // TEST
                        //Vector3 rayGridRight = new Vector3(xRight + xi, terrain.Get(xRight + xi, zRight + zi), zRight + zi);
                        //Vector3 rayGridWorldRight = terrain.Grid2World(rayGridRight);

                        if (showLevelsContourGridSphere)
                            Debug.DrawRay(rayGridWorldRight, Vector3.up * 1 / heightMapRightBool[zi + gridSize, xi + gridSize], Color.yellow); // First level (bool should be 2)

                        // Store array for modulation
                        neighboursPositionsRight.Add(rayGridWorldRight);

                        // Counter neightbours
                        neighbourCellsRight++;

                        // Store array for stabilization. Only update while doing the deformation. When jumping to Stabilization stage, we freeze the list.
                        if (RunningCoroutineDeformationRight == true)
                        {
                            cellsToStabilizeFirstRight.Add(rayGridRight);
                        }
                    }
                    else if (heightMapRightBool[zi + gridSize, xi + gridSize] == 3) // For next levels (used only for stabilization)
                    {
                        // Each neightbour cell in world space
                        //Vector3 rayGridRight = new Vector3(xRight + xi, terrain.Get(xRight + xi, zRight + zi) - offsetRay, zRight + zi); // Remove offset

                        // TEST
                        //Vector3 rayGridRight = new Vector3(xRight + xi, terrain.Get(xRight + xi, zRight + zi), zRight + zi);
                        //Vector3 rayGridWorldRight = terrain.Grid2World(rayGridRight);

                        if (showLevelsContourGridSphere)
                            Debug.DrawRay(rayGridWorldRight, Vector3.up * 1 / heightMapRightBool[zi + gridSize, xi + gridSize], Color.red); // First level (bool should be 2)

                        // Store array for stabilization. Only update while doing the deformation. When jumping to Stabilization stage, we freeze the list.
                        if (RunningCoroutineDeformationRight == true)
                        {
                            cellsToStabilizeSecondRight.Add(rayGridRight);
                        }
                    }
                }
            }
        }

        // -----------

        // Initialize the weights uniformly for the modulation (ocurring in contour 2 only)
        for (int zi = -gridSize + offsetBumpGridFixed; zi <= gridSize - offsetBumpGridFixed; zi++)
        {
            for (int xi = -gridSize + offsetBumpGridFixed; xi <= gridSize - offsetBumpGridFixed; xi++)
            {
                // CONTOUR (BOOL = 2) - CHANGED
                if (heightMapLeftBool[zi + gridSize, xi + gridSize] == 2)
                {
                    // Each neightbour cell in world space
                    //Vector3 rayGridLeft = new Vector3(xLeft + xi, terrain.Get(xLeft + xi, zLeft + zi) - offsetRay, zLeft + zi); // Remove offset
                    Vector3 rayGridLeft = new Vector3(xLeft + xi, terrain.Get(xLeft + xi, zLeft + zi), zLeft + zi);
                    Vector3 rayGridWorldLeft = terrain.Grid2World(rayGridLeft);

                    weightsBumpLeftInit[zi + gridSize, xi + gridSize] = 1f / neighbourCellsLeft;

                    if (showGridBumpDebugLeft)
                        Debug.DrawRay(rayGridWorldLeft, Vector3.up * weightsBumpLeftInit[zi + gridSize, xi + gridSize], Color.yellow);
                }

                // CONTOUR (BOOL = 2) - CHANGED
                if (heightMapRightBool[zi + gridSize, xi + gridSize] == 2)
                {
                    // Each neightbour cell in world space
                    //Vector3 rayGridRight = new Vector3(xRight + xi, terrain.Get(xRight + xi, zRight + zi) - offsetRay, zRight + zi); // Remove offset
                    Vector3 rayGridRight = new Vector3(xRight + xi, terrain.Get(xRight + xi, zRight + zi), zRight + zi);
                    Vector3 rayGridWorldRight = terrain.Grid2World(rayGridRight);

                    weightsBumpRightInit[zi + gridSize, xi + gridSize] = 1f / neighbourCellsRight;

                    if (showGridBumpDebugRight)
                        Debug.DrawRay(rayGridWorldRight, Vector3.up * weightsBumpRightInit[zi + gridSize, xi + gridSize], Color.yellow);
                }
            }
        }
    }

    private void CellsCounter(int xLeft, int zLeft, int xRight, int zRight, int[,] heightMapLeftBool, int[,] heightMapRightBool, int[,] heightMapLeftBoolWorld, int[,] heightMapRightBoolWorld)
    {
        // 2D iteration for both feet
        // 1. It counts the number of hits, save the classified cell in a list and debug ray-casting
        for (int zi = -gridSize; zi <= gridSize; zi++)
        {
            for (int xi = -gridSize; xi <= gridSize; xi++)
            {
                // A. Calculate each cell position wrt World and Heightmap - Left Foot
                // The sensors that counts the number of hits always remain on the surface
                Vector3 rayGridLeft = new Vector3(xLeft + xi, terrain.Get(xLeft + xi, zLeft + zi) - offsetRay, zLeft + zi);
                Vector3 rayGridWorldLeft = terrain.Grid2World(rayGridLeft);

                // A. Calculate each cell position wrt World and Heightmap - Right Foot
                // The sensors that counts the number of hits always remain on the surface
                Vector3 rayGridRight = new Vector3(xRight + xi, terrain.Get(xRight + xi, zRight + zi) - offsetRay, zRight + zi);
                Vector3 rayGridWorldRight = terrain.Grid2World(rayGridRight);

                //------//

                // B. Create each ray for the grid (wrt World) - Left
                RaycastHit leftFootHit;
                Ray upRayLeftFoot = new Ray(rayGridWorldLeft, Vector3.up);

                // B. Create each ray for the grid (wrt World) - Right
                RaycastHit rightFootHit;
                Ray upRayRightFoot = new Ray(rayGridWorldRight, Vector3.up);

                //------//

                // C. If hits the Left Foot, increase counter and add cell to be affected
                if (LeftFootCollider.Raycast(upRayLeftFoot, out leftFootHit, raycastDistance))
                {
                    // Cell contacting directly (BOOL = 1) - CHANGED
                    heightMapLeftBool[zi + gridSize, xi + gridSize] = 1;

                    // Only update while doing the deformation. When jumping to Stabilization stage, we freeze the array
                    if (RunningCoroutineDeformationLeft == true)
                    {
                        heightMapLeftBoolWorld[(int)rayGridLeft.z, (int)rayGridLeft.x] = 1;
                    }

                    // Add position in grid to list
                    counterPositionsLeft.Add(rayGridLeft);

                    // Add counter
                    counterHitsLeft++;

                    if (showGridDebugLeft)
                        Debug.DrawRay(rayGridWorldLeft, Vector3.up * raycastDistance, Color.blue);
                }
                else
                {
                    // No contact (BOOL = 0)
                    heightMapLeftBool[zi + gridSize, xi + gridSize] = 0;

                    // Only update while doing the deformation. When jumping to Stabilization stage, we freeze the array
                    if (RunningCoroutineDeformationLeft)
                    {
                        heightMapLeftBoolWorld[(int)rayGridLeft.z, (int)rayGridLeft.x] = 0;
                    }

                    if (showGridDebugLeft)
                        Debug.DrawRay(rayGridWorldLeft, Vector3.up * raycastDistance, Color.red);
                }

                // C. If hits the Right Foot, increase counter and add cell to be affected
                if (RightFootCollider.Raycast(upRayRightFoot, out rightFootHit, raycastDistance))
                {
                    // Cell contacting directly (BOOL = 1) - CHANGED
                    heightMapRightBool[zi + gridSize, xi + gridSize] = 1;

                    // Only update while doing the deformation. When jumping to Stabilization stage, we freeze the array
                    if (RunningCoroutineDeformationRight == true)
                    {
                        heightMapRightBoolWorld[(int)rayGridRight.z, (int)rayGridRight.x] = 1;
                    }

                    // Add position in grid to list
                    counterPositionsRight.Add(rayGridRight);

                    // Add counter
                    counterHitsRight++;

                    if (showGridDebugRight)
                        Debug.DrawRay(rayGridWorldRight, Vector3.up * raycastDistance, Color.blue);
                }
                else
                {
                    // No contact (BOOL = 0)
                    heightMapRightBool[zi + gridSize, xi + gridSize] = 0;

                    // Only update while doing the deformation. When jumping to Stabilization stage, we freeze the array
                    if (RunningCoroutineDeformationRight)
                    {
                        heightMapRightBoolWorld[(int)rayGridRight.z, (int)rayGridRight.x] = 0;
                    }

                    if (showGridDebugRight)
                        Debug.DrawRay(rayGridWorldRight, Vector3.up * raycastDistance, Color.red);
                }
            }
        }

        // TEST
        CounterPositionsListLeft = counterPositionsLeft;
        CounterPositionsListRight = counterPositionsRight;
    }

    private void ContourCounter(int xLeft, int zLeft, int xRight, int zRight, int[,] heightMapLeftBool, int[,] heightMapRightBool, float[,] weightsBumpLeftInit, float[,] weightsBumpRightInit, int[,] heightMapLeftBoolWorld, int[,] heightMapRightBoolWorld, int n)
    {
        if (IsLeftFootGrounded) // TODO : Try true
        {
            // 1. We don't need to check the whole grid - just in the inner grid is enough
            for (int zi = -gridSize + offsetBumpGridFixed + n; zi <= gridSize - offsetBumpGridFixed - n; zi++)
            {
                for (int xi = -gridSize + offsetBumpGridFixed + n; xi <= gridSize - offsetBumpGridFixed - n; xi++)
                {
                    // 2. For each level, we build the grid with 1 on contact and 2, 3...on the contour
                    for (int i = 0; i <= n; i++)
                    {
                        // A. If the cell was not in contact, it's a potential neighbour (contour) cell
                        if (heightMapLeftBool[zi + gridSize, xi + gridSize] == 0)
                        {
                            // B. Only checking adjacent cells - increasing this would allow increasing the area of the bump
                            for (int zi_sub = -neighboursSearchAreaFixed - i; zi_sub <= neighboursSearchAreaFixed + i; zi_sub = zi_sub + 1 + i)
                            {
                                for (int xi_sub = -neighboursSearchAreaFixed - i; xi_sub <= neighboursSearchAreaFixed + i; xi_sub = xi_sub + 1 + i)
                                {
                                    // C. If there is a contact point around the cell (BOOL = 1) convert to BOOL = 2 - CHANGED
                                    // By going through the levels, you look farther for contact 1 and convert to BOOL = 3, 4, 5...
                                    // TODO: It would be simpler to look for the adjacent cells
                                    if (heightMapLeftBool[zi + zi_sub + gridSize, xi + xi_sub + gridSize] == 1)
                                    {
                                        // TEST
                                        Vector3 rayGridLeft = new Vector3(xLeft + xi, terrain.Get(xLeft + xi, zLeft + zi) - offsetRay, zLeft + zi);
                                        Vector3 rayGridWorldLeft = terrain.Grid2World(rayGridLeft);

                                        // D. Mark that cell as a countour point (BOOL = 2) - CHANGED
                                        heightMapLeftBool[zi + gridSize, xi + gridSize] = 2 + i;

                                        // Only update while doing the deformation. When jumping to Stabilization stage, we freeze the array.
                                        if (RunningCoroutineDeformationLeft == true)
                                        {
                                            heightMapLeftBoolWorld[(int)rayGridLeft.z, (int)rayGridLeft.x] = 2 + i;
                                        }

                                        break;
                                    }
                                }
                            }
                        }
                    }

                    // TODO: Iterate instead of hardcoding.
                    // For now, we only consider the first and second level of the bump.

                    // If the cell is the first inmediate contour point (BOOL = 2) - CHANGED
                    if (heightMapLeftBool[zi + gridSize, xi + gridSize] == 2)
                    {
                        // Each neightbour cell in world space
                        //Vector3 rayGridLeft = new Vector3(xLeft + xi, terrain.Get(xLeft + xi, zLeft + zi) - offsetRay, zLeft + zi); // Remove offset
                        
                        // TEST
                        Vector3 rayGridLeft = new Vector3(xLeft + xi, terrain.Get(xLeft + xi, zLeft + zi), zLeft + zi);
                        Vector3 rayGridWorldLeft = terrain.Grid2World(rayGridLeft);

                        if (showLevelsContourGridSphere)
                            Debug.DrawRay(rayGridWorldLeft, Vector3.up * 1 / heightMapLeftBool[zi + gridSize, xi + gridSize], Color.yellow); // First level (bool should be 2)

                        // Store array for modulation
                        neighboursPositionsLeft.Add(rayGridWorldLeft);

                        // Counter neightbours
                        neighbourCellsLeft++;

                        // Store array for stabilization. Only update while doing the deformation. When jumping to Stabilization stage, we freeze the list.
                        if (RunningCoroutineDeformationLeft == true)
                        {
                            cellsToStabilizeFirstLeft.Add(rayGridLeft);
                        }
                    }
                    else if (heightMapLeftBool[zi + gridSize, xi + gridSize] == 3) // For next levels (used only for stabilization)
                    {
                        // Each neightbour cell in world space
                        //Vector3 rayGridLeft = new Vector3(xLeft + xi, terrain.Get(xLeft + xi, zLeft + zi) - offsetRay, zLeft + zi); // Remove offset
                        
                        // TEST
                        Vector3 rayGridLeft = new Vector3(xLeft + xi, terrain.Get(xLeft + xi, zLeft + zi), zLeft + zi);
                        Vector3 rayGridWorldLeft = terrain.Grid2World(rayGridLeft);

                        if (showLevelsContourGridSphere)
                            Debug.DrawRay(rayGridWorldLeft, Vector3.up * 1 / heightMapLeftBool[zi + gridSize, xi + gridSize], Color.red); // First level (bool should be 2)

                        // Store array for stabilization. Only update while doing the deformation. When jumping to Stabilization stage, we freeze the list.
                        if (RunningCoroutineDeformationLeft == true)
                        {
                            cellsToStabilizeSecondLeft.Add(rayGridLeft);
                        }
                    }
                }
            }
        }

        if (IsRightFootGrounded) 
        {
            // 1. We don't need to check the whole grid - just in the inner grid is enough
            for (int zi = -gridSize + offsetBumpGridFixed + n; zi <= gridSize - offsetBumpGridFixed - n; zi++)
            {
                for (int xi = -gridSize + offsetBumpGridFixed + n; xi <= gridSize - offsetBumpGridFixed - n; xi++)
                {       
                    // 2. For each level, we build the grid with 1 on contact and 2, 3...on the contour
                    for (int i = 0; i <= n; i++)
                    {
                        // A. If the cell was not in contact, it's a potential neighbour (contour) cell
                        if (heightMapRightBool[zi + gridSize, xi + gridSize] == 0)
                        {
                            // B. Only checking adjacent cells - increasing this would allow increasing the area of the bump
                            for (int zi_sub = -neighboursSearchAreaFixed - i; zi_sub <= neighboursSearchAreaFixed + i; zi_sub = zi_sub + 1 + i)
                            {
                                for (int xi_sub = -neighboursSearchAreaFixed - i; xi_sub <= neighboursSearchAreaFixed + i; xi_sub = xi_sub + 1 + i)
                                {
                                    // C. If there is a contact point around the cell (BOOL = 1) convert to BOOL = 2 - CHANGED
                                    // By going through the levels, you look farther for contact 1 and convert to BOOL = 3, 4, 5...
                                    // TODO: It would be simpler to look for the adjacent cells
                                    if (heightMapRightBool[zi + zi_sub + gridSize, xi + xi_sub + gridSize] == 1)
                                    {
                                        // TEST
                                        Vector3 rayGridRight = new Vector3(xRight + xi, terrain.Get(xRight + xi, zRight + zi) - offsetRay, zRight + zi);
                                        Vector3 rayGridWorldRight = terrain.Grid2World(rayGridRight);

                                        // D. Mark that cell as a countour point (BOOL = 2) - CHANGED
                                        heightMapRightBool[zi + gridSize, xi + gridSize] = 2 + i;

                                        // Only update while doing the deformation. When jumping to Stabilization stage, we freeze the array.
                                        if (RunningCoroutineDeformationRight == true)
                                        {
                                            heightMapRightBoolWorld[(int)rayGridRight.z, (int)rayGridRight.x] = 2 + i;
                                        }

                                        break;
                                    }
                                }
                            }
                        }
                    }

                    // TODO: Iterate instead of hardcoding.
                    // For now, we only consider the first and second level of the bump.

                    // If the cell is the first inmediate contour point (BOOL = 2) - CHANGED
                    if (heightMapRightBool[zi + gridSize, xi + gridSize] == 2)
                    {
                        // Each neightbour cell in world space
                        //Vector3 rayGridRight = new Vector3(xRight + xi, terrain.Get(xRight + xi, zRight + zi) - offsetRay, zRight + zi); // Remove offset
                        
                        // TEST
                        Vector3 rayGridRight = new Vector3(xRight + xi, terrain.Get(xRight + xi, zRight + zi), zRight + zi);
                        Vector3 rayGridWorldRight = terrain.Grid2World(rayGridRight);

                        if (showLevelsContourGridSphere)
                            Debug.DrawRay(rayGridWorldRight, Vector3.up * 1 / heightMapRightBool[zi + gridSize, xi + gridSize], Color.yellow); // First level (bool should be 2)

                        // Store array for modulation
                        neighboursPositionsRight.Add(rayGridWorldRight);

                        // Counter neightbours
                        neighbourCellsRight++;

                        // Store array for stabilization. Only update while doing the deformation. When jumping to Stabilization stage, we freeze the list.
                        if (RunningCoroutineDeformationRight == true)
                        {
                            cellsToStabilizeFirstRight.Add(rayGridRight);
                        }
                    }
                    else if (heightMapRightBool[zi + gridSize, xi + gridSize] == 3) // For next levels (used only for stabilization)
                    {
                        // Each neightbour cell in world space
                        //Vector3 rayGridRight = new Vector3(xRight + xi, terrain.Get(xRight + xi, zRight + zi) - offsetRay, zRight + zi); // Remove offset

                        // TEST
                        Vector3 rayGridRight = new Vector3(xRight + xi, terrain.Get(xRight + xi, zRight + zi), zRight + zi);
                        Vector3 rayGridWorldRight = terrain.Grid2World(rayGridRight);

                        if (showLevelsContourGridSphere)
                            Debug.DrawRay(rayGridWorldRight, Vector3.up * 1 / heightMapRightBool[zi + gridSize, xi + gridSize], Color.red); // First level (bool should be 2)

                        // Store array for stabilization. Only update while doing the deformation. When jumping to Stabilization stage, we freeze the list.
                        if (RunningCoroutineDeformationRight == true)
                        {
                            cellsToStabilizeSecondRight.Add(rayGridRight);
                        }
                    }
                }
            }
        }

        // -----------

        // Initialize the weights uniformly for the modulation (ocurring in contour 2 only)
        for (int zi = -gridSize + offsetBumpGridFixed; zi <= gridSize - offsetBumpGridFixed; zi++)
        {
            for (int xi = -gridSize + offsetBumpGridFixed; xi <= gridSize - offsetBumpGridFixed; xi++)
            {
                // CONTOUR (BOOL = 2) - CHANGED
                if (heightMapLeftBool[zi + gridSize, xi + gridSize] == 2)
                {
                    // Each neightbour cell in world space
                    //Vector3 rayGridLeft = new Vector3(xLeft + xi, terrain.Get(xLeft + xi, zLeft + zi) - offsetRay, zLeft + zi); // Remove offset
                    Vector3 rayGridLeft = new Vector3(xLeft + xi, terrain.Get(xLeft + xi, zLeft + zi), zLeft + zi);
                    Vector3 rayGridWorldLeft = terrain.Grid2World(rayGridLeft);

                    weightsBumpLeftInit[zi + gridSize, xi + gridSize] = 1f / neighbourCellsLeft;

                    if (showGridBumpDebugLeft)
                        Debug.DrawRay(rayGridWorldLeft, Vector3.up * weightsBumpLeftInit[zi + gridSize, xi + gridSize], Color.yellow);
                }

                // CONTOUR (BOOL = 2) - CHANGED
                if (heightMapRightBool[zi + gridSize, xi + gridSize] == 2)
                {
                    // Each neightbour cell in world space
                    //Vector3 rayGridRight = new Vector3(xRight + xi, terrain.Get(xRight + xi, zRight + zi) - offsetRay, zRight + zi); // Remove offset
                    Vector3 rayGridRight = new Vector3(xRight + xi, terrain.Get(xRight + xi, zRight + zi), zRight + zi);
                    Vector3 rayGridWorldRight = terrain.Grid2World(rayGridRight);

                    weightsBumpRightInit[zi + gridSize, xi + gridSize] = 1f / neighbourCellsRight;

                    if (showGridBumpDebugRight)
                        Debug.DrawRay(rayGridWorldRight, Vector3.up * weightsBumpRightInit[zi + gridSize, xi + gridSize], Color.yellow);
                }
            }
        }
    }

    private void StabilizationCounterLeft(List<Vector3> cellsToStabilizeLeft, int[,] heightMapLeftBoolWorld, int nAround)
    {        
        // nAround for test only
        // We call StabilizationCounter different times, once for each level of cells to stabilize
        foreach (Vector3 centerCell in cellsToStabilizeLeft)
        {
            // Estimate the totalWeight for the outer level.
            // The problem is that we need to count others (e.g. 3 if the current cell is 2, 4 if the current cell is 3...)
            // TODO: Hardcoded now. Improve iterations.

            float totalWeightFirst = 0f;
            float totalWeightSecond = 0f;

            for (int zi = -1 + (int)centerCell.z - nAround; zi <= 1 + (int)centerCell.z + nAround; zi++)
            {
                for (int xi = -1 + (int)centerCell.x - nAround; xi <= 1 + (int)centerCell.x + nAround; xi++)
                {
                    if (heightMapLeftBoolWorld[(int)centerCell.z, (int)centerCell.x] == 2 && heightMapLeftBoolWorld[zi, xi] == 0)
                    {
                        // First level
                        Vector3 ray = new Vector3(xi, terrain.Get(xi, zi), zi);
                        Vector3 rayWorld = terrain.Grid2World(ray);

                        totalWeightFirst += (terrain.Get(centerCell.x, centerCell.z) - ray.y);
                    }
                    else if (heightMapLeftBoolWorld[(int)centerCell.z, (int)centerCell.x] == 2 && heightMapLeftBoolWorld[zi, xi] == 3)
                    {
                        // First level
                        Vector3 ray = new Vector3(xi, terrain.Get(xi, zi), zi);
                        Vector3 rayWorld = terrain.Grid2World(ray);

                        totalWeightFirst += (terrain.Get(centerCell.x, centerCell.z) - ray.y);
                    }
                    else if (heightMapLeftBoolWorld[(int)centerCell.z, (int)centerCell.x] == 3 && heightMapLeftBoolWorld[zi, xi] == 0)
                    {
                        // Second level
                        Vector3 ray = new Vector3(xi, terrain.Get(xi, zi), zi);
                        Vector3 rayWorld = terrain.Grid2World(ray);

                        totalWeightSecond += (terrain.Get(centerCell.x, centerCell.z) - ray.y);
                    }
                }
            }

            // Now we have the totalWeight, we can calculate the weights for the outer level.
            for (int zi = -1 + (int)centerCell.z - nAround; zi <= 1 + (int)centerCell.z + nAround; zi++)
            {
                for (int xi = -1 + (int)centerCell.x - nAround; xi <= 1 + (int)centerCell.x + nAround; xi++)
                {
                    if (heightMapLeftBoolWorld[(int)centerCell.z, (int)centerCell.x] == 2 && heightMapLeftBoolWorld[zi, xi] == 0)
                    {
                        // First level
                        Vector3 ray = new Vector3(xi, terrain.Get(xi, zi), zi);
                        Vector3 rayWorld = terrain.Grid2World(ray);

                        float differenceHeight = terrain.Get(centerCell.x, centerCell.z) - ray.y;
                        float angle = Mathf.Atan2(differenceHeight, LengthCellX) * Mathf.Rad2Deg;

                        // Clamp angle
                        angle = Mathf.Clamp(angle, 0f, 90f);

                        if (printStabilizationInfoLeft)
                        {
                            Debug.Log("[INFO] Angle: " + angle + " is estimated using the heigth difference: " + differenceHeight + " and the lenghtCellX: " + LengthCellX);
                        }

                        // Include in list of first level cells to stabilize
                        cellsStabilizationFirstLeft.Add(new StabilizationCell(centerCell, new Vector2(xi, zi), ray.y, angle, differenceHeight, differenceHeight / totalWeightFirst));

                        if (showStabilizationLeft)
                        {
                            StabilizationCell nCell = cellsStabilizationFirstLeft.Find(x => x.Pos2DNeighbour == new Vector2(xi, zi));
                            Debug.DrawRay(terrain.Grid2World(centerCell), Vector3.up, Color.black);
                            Debug.DrawRay(terrain.Grid2World(new Vector3(nCell.Pos2DNeighbour.x, nCell.Height, nCell.Pos2DNeighbour.y)), Vector3.up * nCell.Angle, Color.magenta);
                        }
                    }
                    else if (heightMapLeftBoolWorld[(int)centerCell.z, (int)centerCell.x] == 2 && heightMapLeftBoolWorld[zi, xi] == 3)
                    {
                        // First level
                        Vector3 ray = new Vector3(xi, terrain.Get(xi, zi), zi);
                        Vector3 rayWorld = terrain.Grid2World(ray);

                        float differenceHeight = terrain.Get(centerCell.x, centerCell.z) - ray.y;
                        float angle = Mathf.Atan2(differenceHeight, LengthCellX) * Mathf.Rad2Deg;

                        // Clamp angle
                        angle = Mathf.Clamp(angle, 0f, 90f);

                        if (printStabilizationInfoLeft)
                        {
                            Debug.Log("[INFO] Angle: " + angle + " is estimated using the heigth difference: " + differenceHeight + " and the lenghtCellX: " + LengthCellX);
                        }

                        // Include in list of first level cells to stabilize
                        cellsStabilizationFirstLeft.Add(new StabilizationCell(centerCell, new Vector2(xi, zi), ray.y, angle, differenceHeight, differenceHeight / totalWeightFirst));

                        if (showStabilizationLeft)
                        {
                            StabilizationCell nCell = cellsStabilizationFirstLeft.Find(x => x.Pos2DNeighbour == new Vector2(xi, zi));
                            Debug.DrawRay(terrain.Grid2World(centerCell), Vector3.up, Color.black);
                            Debug.DrawRay(terrain.Grid2World(new Vector3(nCell.Pos2DNeighbour.x, nCell.Height, nCell.Pos2DNeighbour.y)), Vector3.up * nCell.Angle, Color.magenta);
                        }
                    }
                    else if (heightMapLeftBoolWorld[(int)centerCell.z, (int)centerCell.x] == 3 && heightMapLeftBoolWorld[zi, xi] == 0)
                    {
                        // Second level
                        Vector3 ray = new Vector3(xi, terrain.Get(xi, zi), zi);
                        Vector3 rayWorld = terrain.Grid2World(ray);

                        float differenceHeight = terrain.Get(centerCell.x, centerCell.z) - ray.y;
                        float angle = Mathf.Atan2(differenceHeight, LengthCellX) * Mathf.Rad2Deg;

                        // Clamp angle
                        angle = Mathf.Clamp(angle, 0f, 90f);
                        
                        if (printStabilizationInfoLeft)
                        {
                            Debug.Log("[INFO] Angle: " + angle + " is estimated using the heigth difference: " + differenceHeight + " and the lenghtCellX: " + LengthCellX);
                        }

                        // Include in list of second level cells to stabilize
                        cellsStabilizationSecondLeft.Add(new StabilizationCell(centerCell, new Vector2(xi, zi), ray.y, angle, differenceHeight, differenceHeight / totalWeightSecond));

                        if (showStabilizationLeft)
                        {
                            StabilizationCell nCell = cellsStabilizationSecondLeft.Find(x => x.Pos2DNeighbour == new Vector2(xi, zi));
                            Debug.DrawRay(terrain.Grid2World(centerCell), Vector3.up, Color.black);
                            Debug.DrawRay(terrain.Grid2World(new Vector3(nCell.Pos2DNeighbour.x, nCell.Height, nCell.Pos2DNeighbour.y)), Vector3.up * nCell.Angle, Color.magenta);
                        }
                    }
                }
            }

            // To try one cell only
            //break;
        }

        // Sort list for each level in descendent height
        cellsStabilizationDescendentFirstLeft = cellsStabilizationFirstLeft.OrderByDescending(x => x.PosCenterCell.y).ToList();
        cellsStabilizationDescendentSecondLeft = cellsStabilizationSecondLeft.OrderByDescending(x => x.PosCenterCell.y).ToList();
    }

    private void StabilizationCounterRight(List<Vector3> cellsToStabilizeRight, int[,] heightMapRightBoolWorld, int nAround)
    {
        // nAround for test only
        // We call StabilizationCounter different times, once for each level of cells to stabilize
        foreach (Vector3 centerCell in cellsToStabilizeRight)
        {
            // Estimate the totalWeight for the outer level.
            // The problem is that we need to count others (e.g. 3 if the current cell is 2, 4 if the current cell is 3...)
            // TODO: Hardcoded now. Improve iterations.

            float totalWeightFirst = 0f;
            float totalWeightSecond = 0f;

            for (int zi = -1 + (int)centerCell.z - nAround; zi <= 1 + (int)centerCell.z + nAround; zi++)
            {
                for (int xi = -1 + (int)centerCell.x - nAround; xi <= 1 + (int)centerCell.x + nAround; xi++)
                {
                    if (heightMapRightBoolWorld[(int)centerCell.z, (int)centerCell.x] == 2 && heightMapRightBoolWorld[zi, xi] == 0)
                    {
                        // First level
                        Vector3 ray = new Vector3(xi, terrain.Get(xi, zi), zi);
                        Vector3 rayWorld = terrain.Grid2World(ray);

                        totalWeightFirst += (terrain.Get(centerCell.x, centerCell.z) - ray.y);
                    }
                    else if (heightMapRightBoolWorld[(int)centerCell.z, (int)centerCell.x] == 2 && heightMapRightBoolWorld[zi, xi] == 3)
                    {
                        // First level
                        Vector3 ray = new Vector3(xi, terrain.Get(xi, zi), zi);
                        Vector3 rayWorld = terrain.Grid2World(ray);

                        totalWeightFirst += (terrain.Get(centerCell.x, centerCell.z) - ray.y);
                    }
                    else if (heightMapRightBoolWorld[(int)centerCell.z, (int)centerCell.x] == 3 && heightMapRightBoolWorld[zi, xi] == 0)
                    {
                        // Second level
                        Vector3 ray = new Vector3(xi, terrain.Get(xi, zi), zi);
                        Vector3 rayWorld = terrain.Grid2World(ray);

                        totalWeightSecond += (terrain.Get(centerCell.x, centerCell.z) - ray.y);
                    }
                }
            }

            // Now we have the totalWeight, we can calculate the weights for the outer level.
            for (int zi = -1 + (int)centerCell.z - nAround; zi <= 1 + (int)centerCell.z + nAround; zi++)
            {
                for (int xi = -1 + (int)centerCell.x - nAround; xi <= 1 + (int)centerCell.x + nAround; xi++)
                {
                    if (heightMapRightBoolWorld[(int)centerCell.z, (int)centerCell.x] == 2 && heightMapRightBoolWorld[zi, xi] == 0)
                    {
                        // First level
                        Vector3 ray = new Vector3(xi, terrain.Get(xi, zi), zi);
                        Vector3 rayWorld = terrain.Grid2World(ray);

                        float differenceHeight = terrain.Get(centerCell.x, centerCell.z) - ray.y;
                        float angle = Mathf.Atan2(differenceHeight, LengthCellX) * Mathf.Rad2Deg;

                        // Clamp angle
                        angle = Mathf.Clamp(angle, 0f, 90f);

                        if (printStabilizationInfoRight)
                        {
                            Debug.Log("[INFO] Angle: " + angle + " is estimated using the heigth difference: " + differenceHeight + " and the lenghtCellX: " + LengthCellX);
                        }

                        // Include in list of first level cells to stabilize
                        cellsStabilizationFirstRight.Add(new StabilizationCell(centerCell, new Vector2(xi, zi), ray.y, angle, differenceHeight, differenceHeight / totalWeightFirst));

                        if (showStabilizationRight)
                        {
                            StabilizationCell nCell = cellsStabilizationFirstRight.Find(x => x.Pos2DNeighbour == new Vector2(xi, zi));
                            Debug.DrawRay(terrain.Grid2World(centerCell), Vector3.up, Color.black);
                            Debug.DrawRay(terrain.Grid2World(new Vector3(nCell.Pos2DNeighbour.x, nCell.Height, nCell.Pos2DNeighbour.y)), Vector3.up * nCell.Angle, Color.magenta);
                        }
                    }
                    else if (heightMapRightBoolWorld[(int)centerCell.z, (int)centerCell.x] == 2 && heightMapRightBoolWorld[zi, xi] == 3)
                    {
                        // First level
                        Vector3 ray = new Vector3(xi, terrain.Get(xi, zi), zi);
                        Vector3 rayWorld = terrain.Grid2World(ray);

                        float differenceHeight = terrain.Get(centerCell.x, centerCell.z) - ray.y;
                        float angle = Mathf.Atan2(differenceHeight, LengthCellX) * Mathf.Rad2Deg;

                        // Clamp angle
                        angle = Mathf.Clamp(angle, 0f, 90f);

                        if (printStabilizationInfoRight)
                        {
                            Debug.Log("[INFO] Angle: " + angle + " is estimated using the heigth difference: " + differenceHeight + " and the lenghtCellX: " + LengthCellX);
                        }

                        // Include in list of first level cells to stabilize
                        cellsStabilizationFirstRight.Add(new StabilizationCell(centerCell, new Vector2(xi, zi), ray.y, angle, differenceHeight, differenceHeight / totalWeightFirst));

                        if (showStabilizationRight)
                        {
                            StabilizationCell nCell = cellsStabilizationFirstRight.Find(x => x.Pos2DNeighbour == new Vector2(xi, zi));
                            Debug.DrawRay(terrain.Grid2World(centerCell), Vector3.up, Color.black);
                            Debug.DrawRay(terrain.Grid2World(new Vector3(nCell.Pos2DNeighbour.x, nCell.Height, nCell.Pos2DNeighbour.y)), Vector3.up * nCell.Angle, Color.magenta);
                        }
                    }
                    else if (heightMapRightBoolWorld[(int)centerCell.z, (int)centerCell.x] == 3 && heightMapRightBoolWorld[zi, xi] == 0)
                    {
                        // Second level
                        Vector3 ray = new Vector3(xi, terrain.Get(xi, zi), zi);
                        Vector3 rayWorld = terrain.Grid2World(ray);

                        float differenceHeight = terrain.Get(centerCell.x, centerCell.z) - ray.y;
                        float angle = Mathf.Atan2(differenceHeight, LengthCellX) * Mathf.Rad2Deg;

                        // Clamp angle
                        angle = Mathf.Clamp(angle, 0f, 90f);

                        if (printStabilizationInfoRight)
                        {
                            Debug.Log("[INFO] Angle: " + angle + " is estimated using the heigth difference: " + differenceHeight + " and the lenghtCellX: " + LengthCellX);
                        }

                        // Include in list of second level cells to stabilize
                        cellsStabilizationSecondRight.Add(new StabilizationCell(centerCell, new Vector2(xi, zi), ray.y, angle, differenceHeight, differenceHeight / totalWeightSecond));

                        if (showStabilizationRight)
                        {
                            StabilizationCell nCell = cellsStabilizationSecondRight.Find(x => x.Pos2DNeighbour == new Vector2(xi, zi));
                            Debug.DrawRay(terrain.Grid2World(centerCell), Vector3.up, Color.black);
                            Debug.DrawRay(terrain.Grid2World(new Vector3(nCell.Pos2DNeighbour.x, nCell.Height, nCell.Pos2DNeighbour.y)), Vector3.up * nCell.Angle, Color.magenta);
                        }
                    }
                }
            }

            // To try one cell only
            //break;
        }

        // Sort list for each level in descendent height
        cellsStabilizationDescendentFirstRight = cellsStabilizationFirstRight.OrderByDescending(x => x.PosCenterCell.y).ToList();
        cellsStabilizationDescendentSecondRight = cellsStabilizationSecondRight.OrderByDescending(x => x.PosCenterCell.y).ToList();
    }

    private void EstimateTargetDeformations()
    {
        // As for the area, we keep the maximum value
        oldHeightCellDisplacementYoungLeft = pressureStressLeft * (originalLengthZero / (youngM));
        if (oldHeightCellDisplacementYoungLeft >= heightCellDisplacementYoungLeft)
        {
            // We use abs. value but for compression, the change in length is negative
            heightCellDisplacementYoungLeft = pressureStressLeft * (originalLengthZero / youngM);

            // Resulting volume under the left foot after displacement - CHANGED
            volumeTotalLeft = areaTotalLeft * (originalLengthZero + (-heightCellDisplacementYoungLeft));

            // 3. If Poisson is 0.5 : ideal imcompressible material (no change in volume) - Compression : -/delta_L
            volumeVariationPoissonLeft = (1 - 2 * poissonR) * (-heightCellDisplacementYoungLeft / originalLengthZero) * volumeOriginalLeft; // NEGATIVE CHANGE

            // Calculate the difference in volume, takes into account the compressibility and estimate volume up per neighbour cell
            volumeDifferenceLeft = volumeTotalLeft - volumeOriginalLeft; // NEGATIVE CHANGE
            volumeNetDifferenceLeft = -volumeDifferenceLeft + volumeVariationPoissonLeft; // Calculate directly the volume in the bump upwards (positive)

            // Distribute volume
            if (neighbourCellsLeft != 0)
                volumeCellLeft = volumeNetDifferenceLeft / neighbourCellsLeft;

            // 2. In this case, we do it with volume. Remember: must be negative for later.
            bumpHeightDeformationLeft = volumeCellLeft / AreaCell;
        }

        oldHeightCellDisplacementYoungRight = pressureStressRight * (originalLengthZero / (youngM));
        if (oldHeightCellDisplacementYoungRight >= heightCellDisplacementYoungRight)
        {
            // We use abs. value but for compression, the change in length is negative
            heightCellDisplacementYoungRight = pressureStressRight * (originalLengthZero / youngM);

            // Resulting volume under the right foot after displacement
            volumeTotalRight = areaTotalRight * (originalLengthZero + (-heightCellDisplacementYoungRight));

            // 3. If Poisson is 0.5 : ideal imcompressible material (no change in volume) - Compression : -/delta_L
            volumeVariationPoissonRight = (1 - 2 * poissonR) * (-heightCellDisplacementYoungRight / originalLengthZero) * volumeOriginalRight;

            // Calculate the difference in volume, takes into account the compressibility and estimate volume up per neighbour cell
            volumeDifferenceRight = volumeTotalRight - volumeOriginalRight; // NEGATIVE CHANGE
            volumeNetDifferenceRight = -volumeDifferenceRight + volumeVariationPoissonRight; // Calculate directly the volume in the bump upwards (positive)

            // Distribute volume
            if (neighbourCellsRight != 0)
                volumeCellRight = volumeNetDifferenceRight / neighbourCellsRight;

            // 2. In this case, we do it with volume. Remember: must be negative for later.
            bumpHeightDeformationRight = volumeCellRight / AreaCell;
        }
    }

    private void EstimateTargetDeformationsVegetation()
    {
        // We use abs. value but for compression, the change in length is negative
        heightCellDisplacementYoungLeft = pressureStressLeft * (originalLengthZero / youngMLeft);

        // Resulting volume under the left foot after displacement - CHANGED
        volumeTotalLeft = areaTotalLeft * (originalLengthZero + (-heightCellDisplacementYoungLeft));

        // 3. If Poisson is 0.5 : ideal imcompressible material (no change in volume) - Compression : -/delta_L
        volumeVariationPoissonLeft = (1 - 2 * poissonR) * (-heightCellDisplacementYoungLeft / originalLengthZero) * volumeOriginalLeft; // NEGATIVE CHANGE

        // Calculate the difference in volume, takes into account the compressibility and estimate volume up per neighbour cell
        volumeDifferenceLeft = volumeTotalLeft - volumeOriginalLeft; // NEGATIVE CHANGE
        volumeNetDifferenceLeft = -volumeDifferenceLeft + volumeVariationPoissonLeft; // Calculate directly the volume in the bump upwards (positive)

        // Distribute volume
        if (neighbourCellsLeft != 0)
            volumeCellLeft = volumeNetDifferenceLeft / neighbourCellsLeft;

        // 2. In this case, we do it with volume. Remember: must be negative for later.
        bumpHeightDeformationLeft = volumeCellLeft / AreaCell;

        // We use abs. value but for compression, the change in length is negative
        heightCellDisplacementYoungRight = pressureStressRight * (originalLengthZero / youngMRight);

        // Resulting volume under the right foot after displacement
        volumeTotalRight = areaTotalRight * (originalLengthZero + (-heightCellDisplacementYoungRight));

        // 3. If Poisson is 0.5 : ideal imcompressible material (no change in volume) - Compression : -/delta_L
        volumeVariationPoissonRight = (1 - 2 * poissonR) * (-heightCellDisplacementYoungRight / originalLengthZero) * volumeOriginalRight;

        // Calculate the difference in volume, takes into account the compressibility and estimate volume up per neighbour cell
        volumeDifferenceRight = volumeTotalRight - volumeOriginalRight; // NEGATIVE CHANGE
        volumeNetDifferenceRight = -volumeDifferenceRight + volumeVariationPoissonRight; // Calculate directly the volume in the bump upwards (positive)

        // Distribute volume
        if (neighbourCellsRight != 0)
            volumeCellRight = volumeNetDifferenceRight / neighbourCellsRight;

        // 2. In this case, we do it with volume. Remember: must be negative for later.
        bumpHeightDeformationRight = volumeCellRight / AreaCell;
    }

    #endregion

    #region New Coroutines

    IEnumerator DecreaseTerrainLeftNew(float[,] heightMapLeft, int[,] heightMapLeftBool, float[,] weightsBumpLeftInit, float[,] weightsBumpLeft, int xLeft, int zLeft)
    {        
        // 1. Apply frame-per-frame deformation ("displacement")
        for (int zi = -gridSize; zi <= gridSize; zi++)
        {
            for (int xi = -gridSize; xi <= gridSize; xi++)
            {
                // A. Calculate each cell position wrt World and Heightmap - Left Foot
                Vector3 rayGridLeft = new Vector3(xLeft + xi, terrain.Get(xLeft + xi, zLeft + zi), zLeft + zi);
                Vector3 rayGridWorldLeft = terrain.Grid2World(rayGridLeft);

                // B. Create each ray for the grid (wrt World) - Left
                RaycastHit leftFootHit;
                Ray upRayLeftFoot = new Ray(rayGridWorldLeft, Vector3.up);

                // C. If hits the Left Foot and the cell was classified with 1 (direct contact) or 2 (countour): - CHANGED
                if (LeftFootCollider.Raycast(upRayLeftFoot, out leftFootHit, raycastDistance) && (heightMapLeftBool[zi + gridSize, xi + gridSize] == 1))
                {
                    // D. Cell contacting directly - Decrease until limit reached
                    if (terrain.Get(rayGridLeft.x, rayGridLeft.z) >= terrain.GetConstant(rayGridLeft.x, rayGridLeft.z) - heightCellDisplacementYoungLeft)
                    {
                        // E. Substract
                        heightMapLeft[zi + gridSize, xi + gridSize] = terrain.Get(rayGridLeft.x, rayGridLeft.z) - (displacementLeft);
                    }
                    else
                    {
                        // F. Keep same
                        heightMapLeft[zi + gridSize, xi + gridSize] = terrain.Get(rayGridLeft.x, rayGridLeft.z);
                    }
                }
                else
                {
                    // J. If is out of reach
                    heightMapLeft[zi + gridSize, xi + gridSize] = terrain.Get(rayGridLeft.x, rayGridLeft.z);
                }

                // TEST: Applying already in this for-loop
                if (applyFootprints)
                {
                    terrain.Set(rayGridLeft.x, rayGridLeft.z, heightMapLeft[zi + gridSize, xi + gridSize]);
                }
            }
        }

        // 2. Applying filtering in frame-basis
        //if (applyFilterLeft)
        //{
        //    if (IsLeftFootGrounded)
        //    {
        //        if (!isFilteredLeft)
        //        {
        //            heightMapLeft = NewFilterHeightMapReturn(xLeft, zLeft, heightMapLeft);
        //            filterIterationsLeftCounter++;
        //        }

        //        if (filterIterationsLeftCounter >= filterIterationsLeftFoot)
        //        {
        //            isFilteredLeft = true;
        //        }
        //    }
        //    else
        //    {
        //        isFilteredLeft = false;
        //        filterIterationsLeftCounter = 0;
        //    }
        //}

        // 3. Save terrain -> REPLACED TO BE APPLIED BEFORE
        //if (applyFootprints)
        //{
        //    for (int zi = -gridSize; zi <= gridSize; zi++)
        //    {
        //        for (int xi = -gridSize; xi <= gridSize; xi++)
        //        {
        //            Vector3 rayGridLeft = new Vector3(xLeft + xi, terrain.Get(xLeft + xi, zLeft + zi), zLeft + zi);
        //            terrain.Set(rayGridLeft.x, rayGridLeft.z, heightMapLeft[zi + gridSize, xi + gridSize]);
        //        }
        //    }
        //}

        yield return null;
    }

    IEnumerator IncreaseTerrainLeftNew(float[,] heightMapLeft, int[,] heightMapLeftBool, float[,] weightsBumpLeftInit, float[,] weightsBumpLeft, int xLeft, int zLeft)
    {
        // 1. Apply frame-per-frame deformation ("displacement")
        for (int zi = -gridSize; zi <= gridSize; zi++)
        {
            for (int xi = -gridSize; xi <= gridSize; xi++)
            {
                // A. Calculate each cell position wrt World and Heightmap - Left Foot
                Vector3 rayGridLeft = new Vector3(xLeft + xi, terrain.Get(xLeft + xi, zLeft + zi), zLeft + zi);
                Vector3 rayGridWorldLeft = terrain.Grid2World(rayGridLeft);

                // B. Create each ray for the grid (wrt World) - Left
                RaycastHit leftFootHit;
                Ray upRayLeftFoot = new Ray(rayGridWorldLeft, Vector3.up);

                if (!LeftFootCollider.Raycast(upRayLeftFoot, out leftFootHit, raycastDistance) && (heightMapLeftBool[zi + gridSize, xi + gridSize] == 2) && applyBumps)
                {
                    if (applyModulation)
                    {
                        float finalWeightCell = beta * weightsBumpLeft[zi + gridSize, xi + gridSize] + (1f - beta) * weightsBumpLeftInit[zi + gridSize, xi + gridSize];

                        if (printModulationInfo)
                        {
                            Debug.Log("[INFO] finalWeightCell: " + finalWeightCell);
                        }

                        bumpDisplacementWeightedLeft = bumpHeightDeformationLeft * finalWeightCell * neighbourCellsLeft;

                        if (showVerticalIncreaseDeformationLeft)
                            Debug.DrawRay(rayGridWorldLeft, new Vector3(0f, (float)bumpDisplacementWeightedLeft, 0f), Color.black);

                        if (terrain.Get(rayGridLeft.x, rayGridLeft.z) <= terrain.GetConstant(rayGridLeft.x, rayGridLeft.z) + bumpDisplacementWeightedLeft)
                        {
                            heightMapLeft[zi + gridSize, xi + gridSize] = terrain.Get(rayGridLeft.x, rayGridLeft.z) + (bumpDisplacementLeft); // TODO: This frame-based displacement is calculated from the unweighted target!
                        }
                        else
                        {
                            heightMapLeft[zi + gridSize, xi + gridSize] = terrain.Get(rayGridLeft.x, rayGridLeft.z);
                        }
                    }
                    else
                    {
                        float finalWeightCell = weightsBumpLeftInit[zi + gridSize, xi + gridSize];

                        if (printModulationInfo)
                        {
                            Debug.Log("[INFO] finalWeightCell: " + finalWeightCell);
                        }

                        bumpDisplacementWeightedLeft = bumpHeightDeformationLeft * finalWeightCell * neighbourCellsLeft;

                        if (showVerticalIncreaseDeformationLeft)
                            Debug.DrawRay(rayGridWorldLeft, new Vector3(0f, (float)bumpDisplacementWeightedLeft, 0f), Color.black);

                        if (terrain.Get(rayGridLeft.x, rayGridLeft.z) <= terrain.GetConstant(rayGridLeft.x, rayGridLeft.z) + bumpDisplacementWeightedLeft) // TODO: This frame-based displacement is calculated from the unweighted target!
                        {
                            heightMapLeft[zi + gridSize, xi + gridSize] = terrain.Get(rayGridLeft.x, rayGridLeft.z) + (bumpDisplacementLeft);
                        }
                        else
                        {
                            heightMapLeft[zi + gridSize, xi + gridSize] = terrain.Get(rayGridLeft.x, rayGridLeft.z);
                        }
                    }
                }
                else
                {
                    // J. If is out of reach
                    heightMapLeft[zi + gridSize, xi + gridSize] = terrain.Get(rayGridLeft.x, rayGridLeft.z);
                }

                // TEST: Applying already in this for-loop
                if (applyFootprints)
                {
                    terrain.Set(rayGridLeft.x, rayGridLeft.z, heightMapLeft[zi + gridSize, xi + gridSize]);
                }
            }
        }

        // 2. Applying filtering in frame-basis
        //if (applyFilterLeft)
        //{
        //    if (IsLeftFootGrounded)
        //    {
        //        if (!isFilteredLeft)
        //        {
        //            heightMapLeft = NewFilterHeightMapReturn(xLeft, zLeft, heightMapLeft);
        //            filterIterationsLeftCounter++;
        //        }

        //        if (filterIterationsLeftCounter >= filterIterationsLeftFoot)
        //        {
        //            isFilteredLeft = true;
        //        }
        //    }
        //    else
        //    {
        //        isFilteredLeft = false;
        //        filterIterationsLeftCounter = 0;
        //    }
        //}

        // 3. Save terrain -> REPLACED TO BE APPLIED BEFORE
        //if (applyFootprints)
        //{
        //    for (int zi = -gridSize; zi <= gridSize; zi++)
        //    {
        //        for (int xi = -gridSize; xi <= gridSize; xi++)
        //        {
        //            Vector3 rayGridLeft = new Vector3(xLeft + xi, terrain.Get(xLeft + xi, zLeft + zi), zLeft + zi);
        //            terrain.Set(rayGridLeft.x, rayGridLeft.z, heightMapLeft[zi + gridSize, xi + gridSize]);
        //        }
        //    }
        //}

        yield return null;
    }

    IEnumerator DecreaseTerrainRightNew(float[,] heightMapRight, int[,] heightMapRightBool, float[,] weightsBumpRightInit, float[,] weightsBumpRight, int xRight, int zRight)
    {
        // 1. Apply frame-per-frame deformation ("displacement")
        for (int zi = -gridSize; zi <= gridSize; zi++)
        {
            for (int xi = -gridSize; xi <= gridSize; xi++)
            {
                // A. Calculate each cell position wrt World and Heightmap - Right Foot
                Vector3 rayGridRight = new Vector3(xRight + xi, terrain.Get(xRight + xi, zRight + zi), zRight + zi);
                Vector3 rayGridWorldRight = terrain.Grid2World(rayGridRight);

                // B. Create each ray for the grid (wrt World) - Right
                RaycastHit rightFootHit;
                Ray upRayRightFoot = new Ray(rayGridWorldRight, Vector3.up);

                // C. If hits the Left Foot and the cell was classified with 1 (direct contact) or 2 (countour): - CHANGED
                if (RightFootCollider.Raycast(upRayRightFoot, out rightFootHit, raycastDistance) && (heightMapRightBool[zi + gridSize, xi + gridSize] == 1))
                {
                    // D. Cell contacting directly - Decrease until limit reached
                    if (terrain.Get(rayGridRight.x, rayGridRight.z) >= terrain.GetConstant(rayGridRight.x, rayGridRight.z) - heightCellDisplacementYoungRight)
                    {
                        // E. Substract
                        heightMapRight[zi + gridSize, xi + gridSize] = terrain.Get(rayGridRight.x, rayGridRight.z) - (displacementRight);
                    }
                    else
                    {
                        // F. Keep same
                        heightMapRight[zi + gridSize, xi + gridSize] = terrain.Get(rayGridRight.x, rayGridRight.z);
                    }
                } 
                else
                {
                    // J. If is out of reach
                    heightMapRight[zi + gridSize, xi + gridSize] = terrain.Get(rayGridRight.x, rayGridRight.z);
                }

                // TEST: Applying already in this for-loop
                if (applyFootprints)
                {
                    terrain.Set(rayGridRight.x, rayGridRight.z, heightMapRight[zi + gridSize, xi + gridSize]);
                }
            }
        }

        // 2. Applying filtering in frame-basis
        //if (applyFilterRight)
        //{
        //    if (IsRightFootGrounded && !IsLeftFootGrounded)
        //    {
        //        if (!isFilteredRight)
        //        {
        //            heightMapRight = NewFilterHeightMapReturn(xRight, zRight, heightMapRight);
        //            filterIterationsRightCounter++;
        //        }

        //        if (filterIterationsRightCounter >= filterIterationsRightFoot)
        //        {
        //            isFilteredRight = true;
        //        }
        //    }
        //    else
        //    {
        //        isFilteredRight = false;
        //        filterIterationsRightCounter = 0;
        //    }
        //}

        // 3. Save terrain -> REPLACED TO BE APPLIED BEFORE
        //if (applyFootprints)
        //{
        //    for (int zi = -gridSize; zi <= gridSize; zi++)
        //    {
        //        for (int xi = -gridSize; xi <= gridSize; xi++)
        //        {
        //            Vector3 rayGridRight = new Vector3(xRight + xi, terrain.Get(xRight + xi, zRight + zi), zRight + zi);
        //            terrain.Set(rayGridRight.x, rayGridRight.z, heightMapRight[zi + gridSize, xi + gridSize]);
        //        }
        //    }
        //}

        yield return null;
    }

    IEnumerator IncreaseTerrainRightNew(float[,] heightMapRight, int[,] heightMapRightBool, float[,] weightsBumpRightInit, float[,] weightsBumpRight, int xRight, int zRight)
    {
        // 1. Apply frame-per-frame deformation ("displacement")
        for (int zi = -gridSize; zi <= gridSize; zi++)
        {
            for (int xi = -gridSize; xi <= gridSize; xi++)
            {
                // A. Calculate each cell position wrt World and Heightmap - Right Foot
                Vector3 rayGridRight = new Vector3(xRight + xi, terrain.Get(xRight + xi, zRight + zi), zRight + zi);
                Vector3 rayGridWorldRight = terrain.Grid2World(rayGridRight);

                // B. Create each ray for the grid (wrt World) - Right
                RaycastHit rightFootHit;
                Ray upRayRightFoot = new Ray(rayGridWorldRight, Vector3.up);

                if (!RightFootCollider.Raycast(upRayRightFoot, out rightFootHit, raycastDistance) && (heightMapRightBool[zi + gridSize, xi + gridSize] == 2) && applyBumps)
                {
                    if (applyModulation && !modulateLeftFootOnly)
                    {
                        float finalWeightCell = beta * weightsBumpRight[zi + gridSize, xi + gridSize] + (1f - beta) * weightsBumpRightInit[zi + gridSize, xi + gridSize];

                        if (printModulationInfo)
                        {
                            Debug.Log("[INFO] finalWeightCell: " + finalWeightCell);
                        }

                        bumpDisplacementWeightedRight = bumpHeightDeformationRight * finalWeightCell * neighbourCellsRight;

                        if (showVerticalIncreaseDeformationRight)
                            Debug.DrawRay(rayGridWorldRight, new Vector3(0f, (float)bumpDisplacementWeightedRight, 0f), Color.black);

                        if (terrain.Get(rayGridRight.x, rayGridRight.z) <= terrain.GetConstant(rayGridRight.x, rayGridRight.z) + bumpDisplacementWeightedRight)
                        {
                            heightMapRight[zi + gridSize, xi + gridSize] = terrain.Get(rayGridRight.x, rayGridRight.z) + (bumpDisplacementRight); // TODO: This frame-based displacement is calculated from the unweighted target!
                        }
                        else
                        {
                            heightMapRight[zi + gridSize, xi + gridSize] = terrain.Get(rayGridRight.x, rayGridRight.z);
                        }
                    }
                    else
                    {
                        float finalWeightCell = weightsBumpRightInit[zi + gridSize, xi + gridSize];

                        if (printModulationInfo)
                        {
                            Debug.Log("[INFO] finalWeightCell: " + finalWeightCell);
                        }
                        
                        bumpDisplacementWeightedRight = bumpHeightDeformationRight * finalWeightCell * neighbourCellsRight;

                        if (showVerticalIncreaseDeformationRight)
                            Debug.DrawRay(rayGridWorldRight, new Vector3(0f, (float)bumpDisplacementWeightedRight, 0f), Color.black);

                        if (terrain.Get(rayGridRight.x, rayGridRight.z) <= terrain.GetConstant(rayGridRight.x, rayGridRight.z) + bumpDisplacementWeightedRight)
                        {
                            heightMapRight[zi + gridSize, xi + gridSize] = terrain.Get(rayGridRight.x, rayGridRight.z) + (bumpDisplacementRight); // TODO: This frame-based displacement is calculated from the unweighted target!
                        }
                        else
                        {
                            heightMapRight[zi + gridSize, xi + gridSize] = terrain.Get(rayGridRight.x, rayGridRight.z);
                        }
                    }
                }
                else
                {
                    // J. If is out of reach
                    heightMapRight[zi + gridSize, xi + gridSize] = terrain.Get(rayGridRight.x, rayGridRight.z);
                }

                // TEST: Applying already in this for-loop
                if (applyFootprints)
                {
                    terrain.Set(rayGridRight.x, rayGridRight.z, heightMapRight[zi + gridSize, xi + gridSize]);
                }
            }
        }

        // 2. Applying filtering in frame-basis
        //if (applyFilterRight)
        //{
        //    if (IsRightFootGrounded && !IsLeftFootGrounded)
        //    {
        //        if (!isFilteredRight)
        //        {
        //            heightMapRight = NewFilterHeightMapReturn(xRight, zRight, heightMapRight);
        //            filterIterationsRightCounter++;
        //        }

        //        if (filterIterationsRightCounter >= filterIterationsRightFoot)
        //        {
        //            isFilteredRight = true;
        //        }
        //    }
        //    else
        //    {
        //        isFilteredRight = false;
        //        filterIterationsRightCounter = 0;
        //    }
        //}

        // 3. Save terrain -> REPLACED TO BE APPLIED BEFORE
        //if (applyFootprints)
        //{
        //    for (int zi = -gridSize; zi <= gridSize; zi++)
        //    {
        //        for (int xi = -gridSize; xi <= gridSize; xi++)
        //        {
        //            Vector3 rayGridRight = new Vector3(xRight + xi, terrain.Get(xRight + xi, zRight + zi), zRight + zi);
        //            terrain.Set(rayGridRight.x, rayGridRight.z, heightMapRight[zi + gridSize, xi + gridSize]);
        //        }
        //    }
        //}

        yield return null;
    }

    #endregion

    #region Coroutines Stabilization
    
    IEnumerator StabilizationLeft(float[,] heightMapLeftWorld, List<StabilizationCell> cellsStabilizationDescendentLeft)
    {
        if (printStabilizationInfoLeft)
            Debug.Log("[INFO] Starting Coroutine Stabilization() - We stabilize a total of " + cellsStabilizationDescendentLeft.Count + " cells");

        foreach (StabilizationCell cell in cellsStabilizationDescendentLeft)
        {
            if (printStabilizationInfoLeft)
            {
                Debug.Log("[INFO] Forach) Center cell: " + cell.PosCenterCell + " with n-cell " + cell.Pos2DNeighbour + " has an angle: " + cell.Angle);
                Debug.Log("[INFO] n-cell " + cell.Pos2DNeighbour + " has an difference height of : " + cell.HeightDifference + " and weight: " + cell.WeigthStabilization);
            }

            /*
             * Simulation should finish if:
             * Angle reaches resting angle
             * We reach max. iterations of simulation
             */

            // Move only if the cell exceeds the rest angle
            if (cell.Angle > restingAngle)
            {
                if (printStabilizationInfoLeft)
                {
                    Debug.Log("[INFO] For n-cell: " + cell.Pos2DNeighbour + " the angle is: " + cell.Angle + " > restingAngle " + restingAngle + " -> Stabilize!");
                }

                #region Other Options

                // First option
                //Debug.Log("CENTER) terrain.Get(cell.PosCenterCell.x, cell.PosCenterCell.z): " + terrain.Get(cell.PosCenterCell.x, cell.PosCenterCell.z));
                //Debug.Log("...is larger than...(cell.PosCenterCell.y " + cell.PosCenterCell.y + " - stepStabilization " + stepStabilization + ") = " + (cell.PosCenterCell.y - stepStabilization));

                // TEST -> Numerically is OK
                //if (terrain.Get(cell.PosCenterCell.x, cell.PosCenterCell.z) >= cell.PosCenterCell.y - stepStabilization)
                //{
                //    Debug.Log("Decreasing Center");
                //    heightMapSphereWorld[(int)cell.PosCenterCell.z, (int)cell.PosCenterCell.x] = terrain.Get(cell.PosCenterCell.x, cell.PosCenterCell.z) - stepStabilizationFrame;
                //}
                //else
                //{
                //    Debug.Log("NOT Decreasing Center");
                //    heightMapSphereWorld[(int)cell.PosCenterCell.z, (int)cell.PosCenterCell.x] = terrain.Get(cell.PosCenterCell.x, cell.PosCenterCell.z);
                //}

                //Debug.Log("N-CELL) terrain.Get(cell.Pos2DNeighbour.x, cell.Pos2DNeighbour.y): " + terrain.Get(cell.Pos2DNeighbour.x, cell.Pos2DNeighbour.y));
                //Debug.Log("...is smaller than...(terrain.GetConstant(cell.Pos2DNeighbour.x, cell.Pos2DNeighbour.y) " + terrain.GetConstant(cell.Pos2DNeighbour.x, cell.Pos2DNeighbour.y) + " + stepStabilization * cell.WeigthStabilization " + stepStabilization * cell.WeigthStabilization + ") = " + (terrain.GetConstant(cell.Pos2DNeighbour.x, cell.Pos2DNeighbour.y) + stepStabilization * cell.WeigthStabilization));

                // TEST -> Numerically is NOW OK :)
                // If using terrain.GetConstant(cell.Pos2DNeighbour.x, cell.Pos2DNeighbour.y), doesn't work - leave with cell.Height for now
                //if (terrain.Get(cell.Pos2DNeighbour.x, cell.Pos2DNeighbour.y) < terrain.GetConstant(cell.Pos2DNeighbour.x, cell.Pos2DNeighbour.y) + (stepStabilization * cell.WeigthStabilization))
                //{
                //    Debug.Log("Increasing n-cell");
                //    heightMapSphereWorld[(int)cell.Pos2DNeighbour.y, (int)cell.Pos2DNeighbour.x] = terrain.Get(cell.Pos2DNeighbour.x, cell.Pos2DNeighbour.y) + (stepStabilizationFrame * cell.WeigthStabilization);
                //}
                //else
                //{
                //    Debug.Log("NOT Increasing n-cell");
                //    heightMapSphereWorld[(int)cell.Pos2DNeighbour.y, (int)cell.Pos2DNeighbour.x] = terrain.Get(cell.Pos2DNeighbour.x, cell.Pos2DNeighbour.y);
                //}

                #endregion

                if (printStabilizationInfoLeft)
                {
                    Debug.Log("[BEFORE INFO] In the array:  heightMapSphereWorld[(int)cell.PosCenterCell.z, (int)cell.PosCenterCell.x]: " + heightMapLeftWorld[(int)cell.PosCenterCell.z, (int)cell.PosCenterCell.x]);
                    Debug.Log("[BEFORE INFO] In the array:  heightMapSphereWorld[(int)cell.Pos2DNeighbour.y, (int)cell.Pos2DNeighbour.x]: " + heightMapLeftWorld[(int)cell.Pos2DNeighbour.y, (int)cell.Pos2DNeighbour.x]);
                }

                // Second option -> Gives better result
                heightMapLeftWorld[(int)cell.PosCenterCell.z, (int)cell.PosCenterCell.x] = terrain.Get(cell.PosCenterCell.x, cell.PosCenterCell.z) - stepStabilization;
                heightMapLeftWorld[(int)cell.Pos2DNeighbour.y, (int)cell.Pos2DNeighbour.x] = terrain.Get(cell.Pos2DNeighbour.x, cell.Pos2DNeighbour.y) + (stepStabilization * cell.WeigthStabilization);

                // Third option: Achieve target stepStabilization but gradually
                //heightMapLeftWorld[(int)cell.PosCenterCell.z, (int)cell.PosCenterCell.x] = terrain.Get(cell.PosCenterCell.x, cell.PosCenterCell.z) - (stepStabilization * Time.deltaTime);
                //heightMapLeftWorld[(int)cell.Pos2DNeighbour.y, (int)cell.Pos2DNeighbour.x] = terrain.Get(cell.Pos2DNeighbour.x, cell.Pos2DNeighbour.y) + (stepStabilization * cell.WeigthStabilization * Time.deltaTime);

                if (printStabilizationInfoLeft)
                {
                    Debug.Log("[AFTER INFO] In the array:  heightMapSphereWorld[(int)cell.PosCenterCell.z, (int)cell.PosCenterCell.x]: " + heightMapLeftWorld[(int)cell.PosCenterCell.z, (int)cell.PosCenterCell.x]);
                    Debug.Log("[AFTER INFO] In the array:  heightMapSphereWorld[(int)cell.Pos2DNeighbour.y, (int)cell.Pos2DNeighbour.x]: " + heightMapLeftWorld[(int)cell.Pos2DNeighbour.y, (int)cell.Pos2DNeighbour.x]);
                }
            }
            else
            {
                if (printStabilizationInfoLeft)
                {
                    Debug.Log("[INFO] For n-cell: " + cell.Pos2DNeighbour + " the angle is: " + cell.Angle + " < restingAngle " + restingAngle + " -> STOP!");
                    Debug.Log("[INFO] " + cell.PosCenterCell + " is now stable!");
                }

                heightMapLeftWorld[(int)cell.PosCenterCell.z, (int)cell.PosCenterCell.x] = terrain.Get(cell.PosCenterCell.x, cell.PosCenterCell.z);
                heightMapLeftWorld[(int)cell.Pos2DNeighbour.y, (int)cell.Pos2DNeighbour.x] = terrain.Get(cell.Pos2DNeighbour.x, cell.Pos2DNeighbour.y);
            }
        }

        if (true)
        {
            foreach (StabilizationCell cell in cellsStabilizationDescendentLeft)
            {
                if (printStabilizationInfoLeft)
                {
                    Debug.Log("[INFO] ============ Applying Stabilization LEFT ============");
                    Debug.Log("|| Center cell: " + cell.PosCenterCell + " BEFORE (GET): " + terrain.Get(cell.PosCenterCell.x, cell.PosCenterCell.z) + " ||");
                    Debug.Log("|| Center cell: " + cell.PosCenterCell + " BEFORE (vehicle): " + heightMapLeftWorld[(int)cell.PosCenterCell.z, (int)cell.PosCenterCell.x] + " ||");

                    Debug.Log("|| n-cell: " + cell.Pos2DNeighbour + " BEFORE (GET): " + terrain.Get(cell.Pos2DNeighbour.x, cell.Pos2DNeighbour.y) + " ||");
                    Debug.Log("|| n-cell: " + cell.Pos2DNeighbour + " BEFORE (vehicle): " + heightMapLeftWorld[(int)cell.Pos2DNeighbour.y, (int)cell.Pos2DNeighbour.x] + " ||");
                }

                if (heightMapLeftWorld[(int)cell.PosCenterCell.z, (int)cell.PosCenterCell.x] >= heightMapLeftWorld[(int)cell.Pos2DNeighbour.y, (int)cell.Pos2DNeighbour.x])
                {
                    terrain.Set(cell.PosCenterCell.x, cell.PosCenterCell.z, heightMapLeftWorld[(int)cell.PosCenterCell.z, (int)cell.PosCenterCell.x]);
                    terrain.Set(cell.Pos2DNeighbour.x, cell.Pos2DNeighbour.y, heightMapLeftWorld[(int)cell.Pos2DNeighbour.y, (int)cell.Pos2DNeighbour.x]);
                }
                else
                {
                    // Overshoot, skipping stabilization and stopping
                    Debug.Log("[INFO] WARNING LEFT: Overshoot, skipping this stabilization");
                    Debug.Log("heightMapLeftWorld[(int)cell.PosCenterCell.z, (int)cell.PosCenterCell.x]: " + heightMapLeftWorld[(int)cell.PosCenterCell.z, (int)cell.PosCenterCell.x] + ">= heightMapLeftWorld[(int)cell.Pos2DNeighbour.y, (int)cell.Pos2DNeighbour.x]" + heightMapLeftWorld[(int)cell.Pos2DNeighbour.y, (int)cell.Pos2DNeighbour.x]);
                    //break;
                    //continue;
                }

                if (printStabilizationInfoLeft)
                {
                    Debug.Log("|| Center cell: " + cell.PosCenterCell + " AFTER (GET): " + terrain.Get(cell.PosCenterCell.x, cell.PosCenterCell.z) + " ||");
                    Debug.Log("|| Center cell: " + cell.PosCenterCell + " AFTER (vehicle): " + heightMapLeftWorld[(int)cell.PosCenterCell.z, (int)cell.PosCenterCell.x] + " ||");

                    Debug.Log("|| n-cell: " + cell.Pos2DNeighbour + " AFTER (GET): " + terrain.Get(cell.Pos2DNeighbour.x, cell.Pos2DNeighbour.y) + " ||");
                    Debug.Log("|| n-cell: " + cell.Pos2DNeighbour + " AFTER (vehicle): " + heightMapLeftWorld[(int)cell.Pos2DNeighbour.y, (int)cell.Pos2DNeighbour.x] + " ||");
                    Debug.Log("=================================================");
                }
            }
        }

        yield return null;
    }

    IEnumerator StabilizationRight(float[,] heightMapRightWorld, List<StabilizationCell> cellsStabilizationDescendentRight)
    {
        if (printStabilizationInfoRight)
            Debug.Log("[INFO] Starting Coroutine Stabilization() - We stabilize a total of " + cellsStabilizationDescendentRight.Count + " cells");

        foreach (StabilizationCell cell in cellsStabilizationDescendentRight)
        {
            if (printStabilizationInfoRight)
            {
                Debug.Log("[INFO] Forach) Center cell: " + cell.PosCenterCell + " with n-cell " + cell.Pos2DNeighbour + " has an angle: " + cell.Angle);
                Debug.Log("[INFO] n-cell " + cell.Pos2DNeighbour + " has an difference height of : " + cell.HeightDifference + " and weight: " + cell.WeigthStabilization);        
            }

            /*
             * Simulation should finish if:
             * Angle reaches resting angle
             * We reach max. iterations of simulation
             */

            // Move only if the cell exceeds the rest angle
            if (cell.Angle > restingAngle)
            {
                if (printStabilizationInfoRight)
                {
                    Debug.Log("[INFO] For n-cell: " + cell.Pos2DNeighbour + " the angle is: " + cell.Angle + " > restingAngle " + restingAngle + " -> Stabilize!");
                }

                #region Other Options

                #endregion

                if (printStabilizationInfoRight)
                {
                    Debug.Log("[BEFORE INFO] In the array:  heightMapSphereWorld[(int)cell.PosCenterCell.z, (int)cell.PosCenterCell.x]: " + heightMapRightWorld[(int)cell.PosCenterCell.z, (int)cell.PosCenterCell.x]);
                    Debug.Log("[BEFORE INFO] In the array:  heightMapSphereWorld[(int)cell.Pos2DNeighbour.y, (int)cell.Pos2DNeighbour.x]: " + heightMapRightWorld[(int)cell.Pos2DNeighbour.y, (int)cell.Pos2DNeighbour.x]);
                }

                // Second option -> Gives better result
                heightMapRightWorld[(int)cell.PosCenterCell.z, (int)cell.PosCenterCell.x] = terrain.Get(cell.PosCenterCell.x, cell.PosCenterCell.z) - stepStabilization;
                heightMapRightWorld[(int)cell.Pos2DNeighbour.y, (int)cell.Pos2DNeighbour.x] = terrain.Get(cell.Pos2DNeighbour.x, cell.Pos2DNeighbour.y) + (stepStabilization * cell.WeigthStabilization);

                // Third option: Achieve target stepStabilization but gradually
                //heightMapRightWorld[(int)cell.PosCenterCell.z, (int)cell.PosCenterCell.x] = terrain.Get(cell.PosCenterCell.x, cell.PosCenterCell.z) - (stepStabilization * Time.deltaTime);
                //heightMapRightWorld[(int)cell.Pos2DNeighbour.y, (int)cell.Pos2DNeighbour.x] = terrain.Get(cell.Pos2DNeighbour.x, cell.Pos2DNeighbour.y) + (stepStabilization * cell.WeigthStabilization * Time.deltaTime);

                if (printStabilizationInfoRight)
                {
                    Debug.Log("[AFTER INFO] In the array:  heightMapSphereWorld[(int)cell.PosCenterCell.z, (int)cell.PosCenterCell.x]: " + heightMapRightWorld[(int)cell.PosCenterCell.z, (int)cell.PosCenterCell.x]);
                    Debug.Log("[AFTER INFO] In the array:  heightMapSphereWorld[(int)cell.Pos2DNeighbour.y, (int)cell.Pos2DNeighbour.x]: " + heightMapRightWorld[(int)cell.Pos2DNeighbour.y, (int)cell.Pos2DNeighbour.x]);
                }
            }
            else
            {
                if (printStabilizationInfoRight)
                {
                    Debug.Log("[INFO] For n-cell: " + cell.Pos2DNeighbour + " the angle is: " + cell.Angle + " < restingAngle " + restingAngle + " -> STOP!");
                    Debug.Log("[INFO] " + cell.PosCenterCell + " is now stable!");
                }   
                
                heightMapRightWorld[(int)cell.PosCenterCell.z, (int)cell.PosCenterCell.x] = terrain.Get(cell.PosCenterCell.x, cell.PosCenterCell.z);
                heightMapRightWorld[(int)cell.Pos2DNeighbour.y, (int)cell.Pos2DNeighbour.x] = terrain.Get(cell.Pos2DNeighbour.x, cell.Pos2DNeighbour.y);
            }
        }

        if (true)
        {
            foreach (StabilizationCell cell in cellsStabilizationDescendentRight)
            {
                if (printStabilizationInfoRight)
                {
                    Debug.Log("[INFO] ============ Applying Stabilization Right ============");
                    Debug.Log("|| Center cell: " + cell.PosCenterCell + " BEFORE (GET): " + terrain.Get(cell.PosCenterCell.x, cell.PosCenterCell.z) + " ||");
                    Debug.Log("|| Center cell: " + cell.PosCenterCell + " BEFORE (vehicle): " + heightMapRightWorld[(int)cell.PosCenterCell.z, (int)cell.PosCenterCell.x] + " ||");

                    Debug.Log("|| n-cell: " + cell.Pos2DNeighbour + " BEFORE (GET): " + terrain.Get(cell.Pos2DNeighbour.x, cell.Pos2DNeighbour.y) + " ||");
                    Debug.Log("|| n-cell: " + cell.Pos2DNeighbour + " BEFORE (vehicle): " + heightMapRightWorld[(int)cell.Pos2DNeighbour.y, (int)cell.Pos2DNeighbour.x] + " ||");
                }

                if (heightMapRightWorld[(int)cell.PosCenterCell.z, (int)cell.PosCenterCell.x] >= heightMapRightWorld[(int)cell.Pos2DNeighbour.y, (int)cell.Pos2DNeighbour.x])
                {
                    terrain.Set(cell.PosCenterCell.x, cell.PosCenterCell.z, heightMapRightWorld[(int)cell.PosCenterCell.z, (int)cell.PosCenterCell.x]);
                    terrain.Set(cell.Pos2DNeighbour.x, cell.Pos2DNeighbour.y, heightMapRightWorld[(int)cell.Pos2DNeighbour.y, (int)cell.Pos2DNeighbour.x]);
                }
                else
                {
                    // Overshoot, skipping stabilization and stopping
                    Debug.Log("[INFO] WARNING RIGHT: Overshoot, skipping this stabilization");
                    Debug.Log("heightMapRightWorld[(int)cell.PosCenterCell.z, (int)cell.PosCenterCell.x]: " + heightMapRightWorld[(int)cell.PosCenterCell.z, (int)cell.PosCenterCell.x] + ">= heightMapRightWorld[(int)cell.Pos2DNeighbour.y, (int)cell.Pos2DNeighbour.x]" + heightMapRightWorld[(int)cell.Pos2DNeighbour.y, (int)cell.Pos2DNeighbour.x]);
                    //break;
                    //continue;
                }

                if (printStabilizationInfoRight)
                {
                    Debug.Log("|| Center cell: " + cell.PosCenterCell + " AFTER (GET): " + terrain.Get(cell.PosCenterCell.x, cell.PosCenterCell.z) + " ||");
                    Debug.Log("|| Center cell: " + cell.PosCenterCell + " AFTER (vehicle): " + heightMapRightWorld[(int)cell.PosCenterCell.z, (int)cell.PosCenterCell.x] + " ||");

                    Debug.Log("|| n-cell: " + cell.Pos2DNeighbour + " AFTER (GET): " + terrain.Get(cell.Pos2DNeighbour.x, cell.Pos2DNeighbour.y) + " ||");
                    Debug.Log("|| n-cell: " + cell.Pos2DNeighbour + " AFTER (vehicle): " + heightMapRightWorld[(int)cell.Pos2DNeighbour.y, (int)cell.Pos2DNeighbour.x] + " ||");
                    Debug.Log("==================================================");
                }
            }
        }

        yield return null;
    }

    #endregion

    #region Post-filter Methods

    // New-version Gaussian Blur (3x3) 
    public void NewFilterHeightMap(int x, int z, float[,] heightMap)
    {
        float[,] heightMapFiltered = new float[2 * gridSize + 1, 2 * gridSize + 1];

        for (int zi = -gridSize + marginAroundGrid; zi <= gridSize - marginAroundGrid; zi++)
        {
            for (int xi = -gridSize + marginAroundGrid; xi <= gridSize - marginAroundGrid; xi++)
            {
                Vector3 rayGridLeft = new Vector3(x + xi, terrain.Get(x + xi, z + zi), z + zi);

                heightMapFiltered[zi + gridSize, xi + gridSize] =
                    heightMap[zi + gridSize - 1, xi + gridSize - 1]
                    + 2 * heightMap[zi + gridSize - 1, xi + gridSize]
                    + 1 * heightMap[zi + gridSize - 1, xi + gridSize + 1]
                    + 2 * heightMap[zi + gridSize, xi + gridSize - 1]
                    + 4 * heightMap[zi + gridSize, xi + gridSize]
                    + 2 * heightMap[zi + gridSize, xi + gridSize + 1]
                    + 1 * heightMap[zi + gridSize + 1, xi + gridSize - 1]
                    + 2 * heightMap[zi + gridSize + 1, xi + gridSize]
                    + 1 * heightMap[zi + gridSize + 1, xi + gridSize + 1];

                heightMapFiltered[zi + gridSize, xi + gridSize] *= 1.0f / 16.0f;

                terrain.Set(rayGridLeft.x, rayGridLeft.z, (heightMapFiltered[zi + gridSize, xi + gridSize]));
            }
        }
    }

    // New-version Gaussian Blur (3x3) with return 
    public float[,] NewFilterHeightMapReturn(int x, int z, float[,] heightMap)
    {
        float[,] heightMapFiltered = new float[2 * gridSize + 1, 2 * gridSize + 1];

        // Places outside filtering will remain the same
        heightMapFiltered = heightMap;

        for (int zi = -gridSize + marginAroundGrid; zi <= gridSize - marginAroundGrid; zi++)
        {
            for (int xi = -gridSize + marginAroundGrid; xi <= gridSize - marginAroundGrid; xi++)
            {
                Vector3 rayGridLeft = new Vector3(x + xi, terrain.Get(x + xi, z + zi), z + zi);

                heightMapFiltered[zi + gridSize, xi + gridSize] =
                    heightMap[zi + gridSize - 1, xi + gridSize - 1]
                    + 2 * heightMap[zi + gridSize - 1, xi + gridSize]
                    + 1 * heightMap[zi + gridSize - 1, xi + gridSize + 1]
                    + 2 * heightMap[zi + gridSize, xi + gridSize - 1]
                    + 4 * heightMap[zi + gridSize, xi + gridSize]
                    + 2 * heightMap[zi + gridSize, xi + gridSize + 1]
                    + 1 * heightMap[zi + gridSize + 1, xi + gridSize - 1]
                    + 2 * heightMap[zi + gridSize + 1, xi + gridSize]
                    + 1 * heightMap[zi + gridSize + 1, xi + gridSize + 1];

                heightMapFiltered[zi + gridSize, xi + gridSize] *= 1.0f / 16.0f;

                //Gaussian filter 5x5
                //heightMapFiltered[zi + gridSize, xi + gridSize] =
                //    heightMap[zi + gridSize - 2, xi + gridSize - 2]
                //    + 4 * heightMap[zi + gridSize - 2, xi + gridSize - 1]
                //    + 6 * heightMap[zi + gridSize - 2, xi + gridSize]
                //    + heightMap[zi + gridSize - 2, xi + gridSize + 2]
                //    + 4 * heightMap[zi + gridSize - 2, xi + gridSize + 1]
                //    + 4 * heightMap[zi + gridSize - 1, xi + gridSize + 2]
                //    + 16 * heightMap[zi + gridSize - 1, xi + gridSize + 1]
                //    + 4 * heightMap[zi + gridSize - 1, xi + gridSize - 2]
                //    + 16 * heightMap[zi + gridSize - 1, xi + gridSize - 1]
                //    + 24 * heightMap[zi + gridSize - 1, xi + gridSize]
                //    + 6 * heightMap[zi + gridSize, xi + gridSize - 2]
                //    + 24 * heightMap[zi + gridSize, xi + gridSize - 1]
                //    + 6 * heightMap[zi + gridSize, xi + gridSize + 2]
                //    + 24 * heightMap[zi + gridSize, xi + gridSize + 1]
                //    + 36 * heightMap[zi + gridSize, xi + gridSize]
                //    + heightMap[zi + gridSize + 2, xi + gridSize - 2]
                //    + 4 * heightMap[zi + gridSize + 2, xi + gridSize - 1]
                //    + 6 * heightMap[zi + gridSize + 2, xi + gridSize]
                //    + heightMap[zi + gridSize + 2, xi + gridSize + 2]
                //    + 4 * heightMap[zi + gridSize + 2, xi + gridSize + 1]
                //    + 4 * heightMap[zi + gridSize + 1, xi + gridSize + 2]
                //    + 16 * heightMap[zi + gridSize + 1, xi + gridSize + 1]
                //    + 4 * heightMap[zi + gridSize + 1, xi + gridSize - 2]
                //    + 16 * heightMap[zi + gridSize + 1, xi + gridSize - 1]
                //    + 24 * heightMap[zi + gridSize + 1, xi + gridSize];

                //heightMapFiltered[zi + gridSize, xi + gridSize] *= 1.0f / 256.0f;
            }
        }

        return heightMapFiltered;
    }

    #endregion

    #region Barycentric Coordinates - Not used

    // TEST - Calculate Barycentric Coordinates Right
    /*
    private void computeBarycentricCoordinatesRight(Vector3 center, List<Vector3> neighboursPositionsRight)
    {
        float weightSumRight = 0;

        for (int i = 0; i < neighboursPositionsRight.Count; i++)
        {
            int prev = (i + neighboursPositionsRight.Count - 1) % neighboursPositionsRight.Count;
            int next = (i + 1) % neighboursPositionsRight.Count;

            allSumCotRight.Add(contangAnglePreviousRight(center, neighboursPositionsRight[i], neighboursPositionsRight[prev], i, prev) + contangAngleNextRight(center, neighboursPositionsRight[i], neighboursPositionsRight[next], i, next));
            //allSumCotRight[i] = contangAnglePreviousRight(center, neighboursPositionsRight[i], neighboursPositionsRight[prev], i, prev) + contangAngleNextRight(center, neighboursPositionsRight[i], neighboursPositionsRight[next], i, next);

            neighboursWeightsRight.Add(allSumCotRight[i] / Vector3.Distance(center, neighboursPositionsRight[i]));
            weightSumRight += neighboursWeightsRight[i];
        }

        for (int i = 0; i < neighboursWeightsRight.Count; i++)
        {
            neighboursWeightsRight[i] /= weightSumRight;
        }
    }

    private float contangAnglePreviousRight(Vector3 p, Vector3 j, Vector3 neighbour, int vertex, int vertex_neightbour)
    {
        var pj = p - j;
        var bc = neighbour - j;

        float angle = Mathf.Atan2(Vector3.Cross(pj, bc).magnitude, Vector3.Dot(pj, bc));
        float angleCot = 1f / Mathf.Tan(angle);

        //allAnglesPrevRight[vertex] = angle * Mathf.Rad2Deg;

        return angleCot;
    }

    private float contangAngleNextRight(Vector3 p, Vector3 j, Vector3 neighbour, int vertex, int vertex_neightbour)
    {
        var pj = p - j;
        var bc = neighbour - j;

        float angle = Mathf.Atan2(Vector3.Cross(pj, bc).magnitude, Vector3.Dot(pj, bc));
        float angleCot = 1f / Mathf.Tan(angle);

        //allAnglesNextRight[vertex] = angle * Mathf.Rad2Deg;

        return angleCot;
    }
    */

    #endregion

    #region Pre-filter Methods - Not used

    //private float[,] FilterBufferLeft(float[,] heightMapLeft, int[,] heightMapLeftBool)
    //{
    //    float[,] heightMapLeftFiltered = new float[2 * gridSize + 1, 2 * gridSize + 1];

    //    for (int zi = -gridSize; zi <= gridSize; zi++)
    //    {
    //        for (int xi = -gridSize; xi <= gridSize; xi++)
    //        {
    //            if (heightMapLeftBool[zi + gridSize, xi + gridSize] == 0)
    //            {
    //                heightMapLeftFiltered[zi + gridSize, xi + gridSize] = heightMapLeft[zi + gridSize, xi + gridSize];
    //            }
    //            else
    //            {
    //                float n = 2.0f * gridSizeKernel + 1.0f;
    //                float sum = 0;

    //                for (int szi = -gridSizeKernel; szi <= gridSizeKernel; szi++)
    //                {
    //                    for (int sxi = -gridSizeKernel; sxi <= gridSizeKernel; sxi++)
    //                    {
    //                        sum += heightMapLeft[gridSize + szi, gridSize + sxi];
    //                    }
    //                }

    //                heightMapLeftFiltered[zi + gridSize, xi + gridSize] = sum / (n * n);
    //            }
    //        }
    //    }

    //    return heightMapLeftFiltered;
    //}

    //private float[,] FilterBufferRight(float[,] heightMapRight, int[,] heightMapRightBool)
    //{
    //    float[,] heightMapRightFiltered = new float[2 * gridSize + 1, 2 * gridSize + 1];

    //    for (int zi = -gridSize; zi <= gridSize; zi++)
    //    {
    //        for (int xi = -gridSize; xi <= gridSize; xi++)
    //        {
    //            if (heightMapRightBool[zi + gridSize, xi + gridSize] == 0)
    //            {
    //                heightMapRightFiltered[zi + gridSize, xi + gridSize] = heightMapRight[zi + gridSize, xi + gridSize];
    //            }
    //            else
    //            {
    //                float n = 2.0f * gridSizeKernel + 1.0f;
    //                float sum = 0;

    //                for (int szi = -gridSizeKernel; szi <= gridSizeKernel; szi++)
    //                {
    //                    for (int sxi = -gridSizeKernel; sxi <= gridSizeKernel; sxi++)
    //                    {
    //                        sum += heightMapRight[gridSize + szi, gridSize + sxi];
    //                    }
    //                }

    //                heightMapRightFiltered[zi + gridSize, xi + gridSize] = sum / (n * n);
    //            }
    //        }
    //    }

    //    return heightMapRightFiltered;
    //}

    #endregion
}
