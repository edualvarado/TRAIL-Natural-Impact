/****************************************************
 * File: BrushFootprint.cs
   * Author: Eduardo Alvarado
   * Email: alvaradopinero.eduardo@gmail.com
   * Date: 12/01/2024
   * Project: Foot2Trail
   * Last update: 12/01/2024
*****************************************************/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static UnityEditor.ShaderData;

/// <summary>
/// General parent class to create brushses that affect the ground (e.g. footprint brush).
/// </summary>
abstract public class BrushFootprint : MonoBehaviour
{
    #region Instance Fields

    private bool active = false;

    // Test
    private Vector3 slopeInitialNormal;
    private float slopeInitialAngle;

    // Coroutines
    private bool runningCoroutineDeformationLeft;
    private bool runningCoroutineStabilizationLeft;
    private bool runningCoroutineDeformationRight;
    private bool runningCoroutineStabilizationRight;

    // Terrain Data
    protected TerrainDeformationMaster terrain;
    private Vector3 heightmapSize;
    private Vector3 terrainSize;
    private float areaCell;
    private float lengthCellX;
    private float lengthCellZ;

    // Body Properties
    private GameObject myBipedalCharacter;
    private float mass;
    private bool isLeftFootGrounded;
    private bool isLeftFootHeelGrounded;
    private bool isLeftFootToeGrounded;
    private bool isRightFootGrounded;
    private bool isRightFootHeelGrounded;
    private bool isRightFootToeGrounded;
    private Collider leftFootCollider;
    private Collider rightFootCollider;
    private float leftHeelHeight;
    private float leftToeHeight;
    private float rightHeelHeight;
    private float rightToeHeight;

    // Physics
    private float contactTime;
    private Vector3 totalForceLeft;
    private Vector3 totalForceRight;
    private float totalForceLeftVertical;
    private float totalForceRightVertical;
    private float totalForceLeftDownward;
    private float totalForceRightDownward;
    private float totalForceLeftHorizontal;
    private float totalForceRightHorizontal;
    private Vector3 totalForceLeftTerrainProjection;
    private Vector3 totalForceRightTerrainProjection;
    private Vector3 totalGRForceLeft;
    private Vector3 totalGRForceRight;
    private Vector3 centerGridLeftFootHeight;
    private Vector3 centerGridRightFootHeight;
    private Vector3 originLeft;
    private Vector3 originRight;
    private float pressureLeft;
    private float pressureRight;
    private List<Vector3> counterPositionsListLeft = new List<Vector3>();
    private List<Vector3> counterPositionsListRight = new List<Vector3>();

    // Material/Ground
    private inputDeformation deformationChoice;
    private double youngModulus;
    private double youngModulusGround;
    private double youngModulusVegetation;
    private int filterIterations;
    private float poissonRatio;
    private bool activateBump;
    private float[,] alphaVegetation;

    // UI
    private Slider youngSlider;
    private Slider poissonSlider;
    private Slider iterationsSlider;
    private Toggle activateToggleDef;
    private Toggle activateToggleBump;
    private Toggle activateToggleGauss;
    private Toggle activateToggleShowGrid;
    private Toggle activateToggleShowBump;

    #endregion

    #region Properties - Test

    public Vector3 SlopeInitialNormal
    {
        get { return slopeInitialNormal; }
        set { slopeInitialNormal = value; }
    }

    public float SlopeInitialAngle
    {
        get { return slopeInitialAngle; }
        set { slopeInitialAngle = value; }
    }

    #endregion

    #region Properties - Coroutines

    public bool RunningCoroutineDeformationLeft
    {
        get { return runningCoroutineDeformationLeft; }
        set { runningCoroutineDeformationLeft = value; }
    }

    public bool RunningCoroutineStabilizationLeft
    {
        get { return runningCoroutineStabilizationLeft; }
        set { runningCoroutineStabilizationLeft = value; }
    }

    public bool RunningCoroutineDeformationRight
    {
        get { return runningCoroutineDeformationRight; }
        set { runningCoroutineDeformationRight = value; }
    }

    public bool RunningCoroutineStabilizationRight
    {
        get { return runningCoroutineStabilizationRight; }
        set { runningCoroutineStabilizationRight = value; }
    }

    #endregion

    #region Instance Properties - Body

    public float Mass
    {
        get { return mass; }
        set { mass = value; }
    }
    public bool IsLeftFootGrounded
    {
        get { return isLeftFootGrounded; }
        set { isLeftFootGrounded = value; }
    }
    public bool IsLeftFootHeelGrounded
    {
        get { return isLeftFootHeelGrounded; }
        set { isLeftFootHeelGrounded = value; }
    }
    public bool IsLeftFootToeGrounded
    {
        get { return isLeftFootToeGrounded; }
        set { isLeftFootToeGrounded = value; }
    }
    public bool IsRightFootGrounded
    {
        get { return isRightFootGrounded; }
        set { isRightFootGrounded = value; }
    }
    public bool IsRightFootHeelGrounded
    {
        get { return isRightFootHeelGrounded; }
        set { isRightFootHeelGrounded = value; }
    }
    public bool IsRightFootToeGrounded
    {
        get { return isRightFootToeGrounded; }
        set { isRightFootToeGrounded = value; }
    }
    public Collider LeftFootCollider
    {
        get { return leftFootCollider; }
        set { leftFootCollider = value; }
    }
    public Collider RightFootCollider
    {
        get { return rightFootCollider; }
        set { rightFootCollider = value; }
    }

    #endregion

    #region Instance Properties - Materials

    public bool ActivateBump
    {
        get { return activateBump; }
        set { activateBump = value; }
    }
    public float PoissonRatio
    {
        get { return poissonRatio; }
        set { poissonRatio = value; }
    }
    public double YoungModulus
    {
        get { return youngModulus; }
        set { youngModulus = value; }
    }
    public double YoungModulusGround
    {
        get { return youngModulusGround; }
        set { youngModulusGround = value; }
    }
    public double YoungModulusVegetation
    {
        get { return youngModulusVegetation; }
        set { youngModulusVegetation = value; }
    }
    public int FilterIterations
    {
        get { return filterIterations; }
        set { filterIterations = value; }
    }

    #endregion

    #region Instance Properties - Vegetation

    public float[,] AlphaVegetation
    {
        get { return alphaVegetation; }
        set { alphaVegetation = value; }
    }

    #endregion

    #region Instance Properties - Terrain

    public Vector3 TerrainSize
    {
        get { return terrainSize; }
        set { terrainSize = value; }
    }
    public Vector3 HeightmapSize
    {
        get { return heightmapSize; }
        set { heightmapSize = value; }
    }
    public float AreaCell
    {
        get { return areaCell; }
        set { areaCell = value; }
    }

    public float LengthCellX
    {
        get { return lengthCellX; }
        set { lengthCellX = value; }
    }
    public float LengthCellZ
    {
        get { return lengthCellZ; }
        set { lengthCellZ = value; }
    }

    #endregion

    #region Instance Properties - Forces

    public Vector3 TotalForceLeft
    {
        get { return totalForceLeft; }
        set { totalForceLeft = value; }
    }
    public Vector3 TotalForceRight
    {
        get { return totalForceRight; }
        set { totalForceRight = value; }
    }

    public float TotalForceLeftVertical
    {
        get { return totalForceLeftVertical; }
        set { totalForceLeftVertical = value; }
    }
    public float TotalForceRightVertical
    {
        get { return totalForceRightVertical; }
        set { totalForceRightVertical = value; }
    }
    public float TotalForceLeftDownward
    {
        get { return totalForceLeftDownward; }
        set { totalForceLeftDownward = value; }
    }
    public float TotalForceRightDownward
    {
        get { return totalForceRightDownward; }
        set { totalForceRightDownward = value; }
    }
    public float TotalForceLeftHorizontal
    {
        get { return totalForceLeftHorizontal; }
        set { totalForceLeftHorizontal = value; }
    }
    public float TotalForceRightHorizontal
    {
        get { return totalForceRightHorizontal; }
        set { totalForceRightHorizontal = value; }
    }
    public Vector3 TotalForceLeftTerrainProjection
    {
        get { return totalForceLeftTerrainProjection; }
        set { totalForceLeftTerrainProjection = value; }
    }
    public Vector3 TotalForceRightTerrainProjection
    {
        get { return totalForceRightTerrainProjection; }
        set { totalForceRightTerrainProjection = value; }
    }

    public Vector3 TotalGRForceLeft
    {
        get { return totalGRForceLeft; }
        set { totalGRForceLeft = value; }
    }
    public Vector3 TotalGRForceRight
    {
        get { return totalGRForceRight; }
        set { totalGRForceRight = value; }
    }
    public float ContactTime
    {
        get { return contactTime; }
        set { contactTime = value; }
    }

    public Vector3 OriginLeft
    {
        get { return originLeft; }
        set { originLeft = value; }
    }

    public Vector3 OriginRight
    {
        get { return originRight; }
        set { originRight = value; }
    }

    public float PressureLeft
    {
        get { return pressureLeft; }
        set { pressureLeft = value; }
    }

    public float PressureRight
    {
        get { return pressureRight; }
        set { pressureRight = value; }
    }

    public List<Vector3> CounterPositionsListLeft
    {
        get { return counterPositionsListLeft; }
        set { counterPositionsListLeft = value; }
    }

    public List<Vector3> CounterPositionsListRight
    {
        get { return counterPositionsListRight; }
        set { counterPositionsListRight = value; }
    }

    #endregion

    #region Instance Properties - Character

    public Vector3 CenterGridLeftFootHeight
    {
        get { return centerGridLeftFootHeight; }
        set { centerGridLeftFootHeight = value; }
    }
    public Vector3 CenterGridRightFootHeight
    {
        get { return centerGridRightFootHeight; }
        set { centerGridRightFootHeight = value; }
    }
    public float LeftHeelHeight
    {
        get { return leftHeelHeight; }
        set { leftHeelHeight = value; }
    }
    public float LeftToeHeight
    {
        get { return leftToeHeight; }
        set { leftToeHeight = value; }
    }
    public float RightHeelHeight
    {
        get { return rightHeelHeight; }
        set { rightHeelHeight = value; }
    }
    public float RightToeHeight
    {
        get { return rightToeHeight; }
        set { rightToeHeight = value; }
    }

    public GameObject MyBipedalCharacter
    {
        get { return myBipedalCharacter; }
        set { myBipedalCharacter = value; }
    }

    #endregion

    #region Instance Properties - Others/UI

    public inputDeformation DeformationChoice
    {
        get { return deformationChoice; }
        set { deformationChoice = value; }
    }

    public Slider YoungSlider
    {
        get { return youngSlider; }
        set { youngSlider = value; }
    }

    public Slider PoissonSlider
    {
        get { return poissonSlider; }
        set { poissonSlider = value; }
    }

    public Slider IterationsSlider
    {
        get { return iterationsSlider; }
        set { iterationsSlider = value; }
    }

    public Toggle ActivateToggleDef
    {
        get { return activateToggleDef; }
        set { activateToggleDef = value; }
    }

    public Toggle ActivateToggleBump
    {
        get { return activateToggleBump; }
        set { activateToggleBump = value; }
    }

    public Toggle ActivateToggleGauss
    {
        get { return activateToggleGauss; }
        set { activateToggleGauss = value; }
    }

    public Toggle ActivateToggleShowGrid
    {
        get { return activateToggleShowGrid; }
        set { activateToggleShowGrid = value; }
    }

    public Toggle ActivateToggleShowBump
    {
        get { return activateToggleShowBump; }
        set { activateToggleShowBump = value; }
    }

    #endregion

    #region Unity Methods

    void Start()
    {
        // Get the DeformTerrainMaster class
        terrain = GetComponent<TerrainDeformationMaster>();

        // Retrieve once public variables from DeformTerrainMaster.cs
        LeftFootCollider = terrain.leftFootCollider;
        RightFootCollider = terrain.rightFootCollider;
        Mass = terrain.mass;
        ContactTime = terrain.contactTime;

        // Retrieve once though methods of DeformTerrainMaster.cs
        HeightmapSize = terrain.GridSize();
        TerrainSize = terrain.TerrainSize();
        AreaCell = terrain.CellSize();
        LengthCellX = terrain.LengthCellX();
        LengthCellZ = terrain.LengthCellZ();
    }

    void Update()
    {
        // TODO: Remove from UPDATE
        // Retrieve once though methods of DeformTerrainMaster.cs
        HeightmapSize = terrain.GridSize();
        TerrainSize = terrain.TerrainSize();
        AreaCell = terrain.CellSize();
        LengthCellX = terrain.LengthCellX();
        LengthCellZ = terrain.LengthCellZ();

        //       Retrieve each frame public variables from DeformTerrainMaster.cs       //
        // ============================================================================ //

        // 1. Retrieve Character
        MyBipedalCharacter = terrain.myBipedalCharacter;

        // =============================== IMPORTANT ================================== //
        //              Make sure the components are the correct ones                   //
        // ============================================================================ //

        // Not correct for slopes
        //TotalForceY = terrain.totalGRForce.y;
        //TotalForceLeftY = terrain.totalGRForceLeft.y;
        //TotalForceRightY = terrain.totalGRForceRight.y;

        // 2. Retrieve vertical component Foot Force - correct for slopes
        TotalForceLeftVertical = Mathf.Abs(Vector3.Magnitude(terrain.totalForceLeftLocalVertical)); // or GRF without Abs
        TotalForceRightVertical = Mathf.Abs(Vector3.Magnitude(terrain.totalForceRightLocalVertical));

        // 3. Retrieve projection Foot Force (for modulation)
        TotalForceLeftTerrainProjection = terrain.totalForceLeftLocalTerrainProjection;
        TotalForceRightTerrainProjection = terrain.totalForceRightLocalTerrainProjection;

        // 3. Retrieve Foot Force
        TotalForceLeft = terrain.totalForceLeftFoot;
        TotalForceRight = terrain.totalForceRightFoot;

        // 4. Retrieve GRForces
        TotalGRForceLeft = terrain.totalGRForceLeft;
        TotalGRForceRight = terrain.totalGRForceRight;

        // 5. Retrieve Center Grids
        CenterGridLeftFootHeight = terrain.centerGridLeftFootHeight;
        CenterGridRightFootHeight = terrain.centerGridLeftFootHeight;

        // 6. Are the feet grounded?
        IsLeftFootGrounded = terrain.isLeftFootGrounded;
        IsLeftFootHeelGrounded = terrain.isLeftFootHeelGrounded;
        IsLeftFootToeGrounded = terrain.isLeftFootToeGrounded;

        IsRightFootGrounded = terrain.isRightFootGrounded;
        IsRightFootHeelGrounded = terrain.isRightFootHeelGrounded;
        IsRightFootToeGrounded = terrain.isRightFootToeGrounded;

        // 7. Deformation choice
        DeformationChoice = terrain.deformationChoice;

        // 8. Retrieve Origins
        OriginLeft = terrain.OriginLeft;
        OriginRight = terrain.OriginRight;

        // 9. Retrieve Heel/Toe Heights
        LeftHeelHeight = terrain.LeftHeelHeight;
        LeftToeHeight = terrain.LeftToeHeight;
        RightHeelHeight = terrain.RightHeelHeight;
        RightToeHeight = terrain.RightToeHeight;

        // 10. In case we are using the UI
        YoungSlider = terrain.youngSlider;
        PoissonSlider = terrain.poissonSlider;
        IterationsSlider = terrain.iterationsSlider;
        ActivateToggleDef = terrain.activateToggleDef;
        ActivateToggleBump = terrain.activateToggleBump;
        ActivateToggleGauss = terrain.activateToggleGauss;
        ActivateToggleShowGrid = terrain.activateToggleShowGrid;
        ActivateToggleShowBump = terrain.activateToggleShowBump;

        // 11. Coroutines
        RunningCoroutineDeformationLeft = terrain.RunningCoroutineDeformationLeft;
        RunningCoroutineStabilizationLeft = terrain.RunningCoroutineStabilizationLeft;
        RunningCoroutineDeformationRight = terrain.RunningCoroutineDeformationRight;
        RunningCoroutineStabilizationRight = terrain.RunningCoroutineStabilizationRight;

        // 12. Test
        SlopeInitialNormal = terrain.slopeInitialNormal;
        SlopeInitialAngle = terrain.slopeInitialAngle;
    }

    #endregion

    #region Instance Methods

    public void Deactivate()
    {
        if (active)
            terrain.SetFootprintBrush(null);
        active = false;
    }

    public void Activate()
    {
        BrushFootprint active_brush = terrain.GetFootprintBrush();
        if (active_brush)
            active_brush.Deactivate();
        terrain.SetFootprintBrush(this);
        active = true;
    }

    public void Toggle()
    {
        if (IsActive())
            Deactivate();
        else
            Activate();
    }

    public bool IsActive()
    {
        return active;
    }

    // Virtual method that is used to pass the feet positions and create the physically-based footprint
    public virtual void CallFootprint(float xLeft, float zLeft, float xRight, float zRight)
    {
        DrawFootprint(xLeft, zLeft, xRight, zRight);
    }

    public virtual void StabilizeFootprint(float xLeft, float zLeft, float xRight, float zRight)
    {
        DrawStabilizeFootprint(xLeft, zLeft, xRight, zRight);
    }

    public virtual void CallGaussianSingle(float x, float z, float smoothStrength, int smoothRadius)
    {
        DrawGaussianSingle(x, z, smoothStrength, smoothRadius);
    }

    // Abstract = incomplete implementation that will be fullfiled in the child class (TerrainBrush)
    public abstract void DrawFootprint(float xLeft, float zLeft, float xRight, float zRight);
    public abstract void DrawFootprint(int xLeft, int zLeft, int xRight, int zRight);

    // abstract = incomplete implementation that will be fullfiled in the child class (TerrainBrush)
    public abstract void DrawStabilizeFootprint(float xLeft, float zLeft, float xRight, float zRight);
    public abstract void DrawStabilizeFootprint(int xLeft, int zLeft, int xRight, int zRight);

    // abstract = incomplete implementation that will be fullfiled in the child class (TerrainBrush)
    public abstract void DrawGaussianSingle(float x, float z, float smoothStrength, int smoothRadius);
    public abstract void DrawGaussianSingle(int x, int z, float smoothStrength, int smoothRadius);

    #endregion
}
