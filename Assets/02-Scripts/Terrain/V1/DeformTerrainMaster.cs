/****************************************************
 * File: DeformTerrainMaster.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 01/10/2020
   * Project: Foot2Trail
   * Last update: 24/02/2023
*****************************************************/

using System;
using UnityEngine;
using UnityEngine.UI;

using PositionBasedDynamics;

public enum sourceDeformation
{
    useUI,
    useTerrainPrefabs,
    useManualValues,
    useManualValuesWithVegetation
};

/// <summary>
/// Master class where we calculate all forces taking place during gait and call footprint child class.
/// </summary>
public class DeformTerrainMaster : MonoBehaviour
{
    // Test
    public Vector3 posLFoot;
    public Vector3 posLToe;
    public Vector3 oldposLFoot;
    public Vector3 oldposLToe;

    #region Instance Fields

    [Header("Initial Normals and Gradient Matrix")]
    public Vector3[,] initialNormalMatrix;
    public Vector3 slopeInitialNormal;
    public float[,] initialGradientMatrix;
    public float slopeInitialAngle;

    [Header("Exporting Maps")]
    public bool exportMaps;

    [Header("Vegetation Creator")]
    [SerializeField] private VegetationCreator vegetationCreator;
    [SerializeField] private IKFootAdaptation feetPlacement = null;

    // ----------------------------------------------

    [Header("Debug - (SET UP)")]
    public bool printSteps;

    [Header("Bipedal - (SET UP)")]
    [Tooltip("Your character GameObject")]
    public GameObject myBipedalCharacter;
    [Tooltip("Collider attached to Left Foot to leave footprints")]
    public Collider leftFootCollider;
    [Tooltip("Collider attached to Right Foot to leave footprints")]
    public Collider rightFootCollider;
    [Tooltip("RB attached to Left Foot used for velocity estimation")]
    public Rigidbody leftFootRB;
    [Tooltip("RB attached to Right Foot for velocity estimation")]
    public Rigidbody rightFootRB;

    [Header("Offset Center Foot")]
    public Vector3 offsetCenterFoot;

    [Header("Source of Deformation - (SET UP)")]
    public sourceDeformation deformationChoice;

    [Header("Terrain Deformation - Contact Time Settings - (SET UP)")]
    //public float timePassed = 0f;
    public float timePassedLeft = 0f;
    public float timePassedRight = 0f;
    [Tooltip("Time that the terrain requires to absorve the force from the hitting foot. More time results in a smaller require force. On the other hand, for less time, the terrain requires a larger force to stop the foot.")]
    public float contactTime = 0.1f;
    [Tooltip("Small delay, sometimes needed, to give the system enough time to perform the deformation.")]
    public float offset = 0.5f;
    public float timeDuringPressureLeft = 0f;
    public float accumulatedTimeDuringPressureLeft = 0f;
    public float timeDuringPressureRight = 0f;
    public float accumulatedTimeDuringPressureRight = 0f;
    public float totalTime = 0f;
    public float contactRatioLeft = 0f;
    public float contactRatioRight = 0f;

    [Header("Stabilization - (SET UP)")]
    public int maxStabilizations = 0;
    public int counterStabilizationLeft = 0;
    public int counterStabilizationRight = 0;

    [Header("Terrain Prefabs - Settings - (SET UP)")]
    public double youngModulusSnow = 200000;
    public float timeSnow = 0.2f;
    public bool bumpSnow = false;
    public float poissonRatioSnow = 0.1f;
    public int filterIterationsSnow = 0;
    [Space(10)]
    public double youngModulusDrySand = 600000;
    public float timeDrySand = 0.3f;
    public bool bumpSand = false;
    public float poissonRatioSand = 0.2f;
    public int filterIterationsSand = 5;
    [Space(10)]
    public double youngModulusMud = 350000;
    public float timeMud = 0.8f;
    public bool bumpMud = false;
    public float poissonRatioMud = 0.4f;
    public int filterIterationsMud = 2;
    [Space(10)]
    public double youngModulusSoil = 350000;
    public float timeSoil = 0.8f;
    public bool bumpSoil = false;
    public float poissonRatioSoil = 0.4f;
    public int filterIterationsSoil = 2;

    [Header("Terrain - System Info")]
    public LayerMask ground;
    public float slopeAngle;
    public Vector3 slopeNormal;
    
    [Header("Terrain - Debug")]
    public bool drawAngleNormal = false;
    public bool drawCOMProjection = false;

    [Header("Bipedal - System Info")]
    public float mass;
    public bool printFeetPositions = false;
    public bool isLeftFootGrounded;
    public bool isLeftFootHeelGrounded;
    public bool isLeftFootToeGrounded;
    public bool isRightFootGrounded;
    public bool isRightFootHeelGrounded;
    public bool isRightFootToeGrounded;
    public float weightInLeftFoot;
    public float weightInRightFoot;
    public float rightWeightDistribution;
    public float leftWeightDistribution;
    public float heightIKLeft;
    public float heightIKRight;
    public Vector3 centerGridLeftFootHeight;
    public Vector3 centerGridRightFootHeight;

    [Header("Bipedal - Physics - Debug")]
    public bool printFeetForces = false;
    public bool drawWeightForces = false;
    public bool drawWeightForcesComponents = false;
    public bool drawMMVelocities = false;
    public bool drawMomentumForces = false;
    public bool drawMomentumForcesComponents = false;
    public bool drawGRForces = false;
    public bool drawGRForcesComponents = false;
    public bool drawFeetForces = false;
    public bool drawFeetForcesComponents = false;
    public bool drawFeetForcesProjections = false;
    public bool drawForceSystem = false;

    [Header("Bipedal - Physics - Weight Forces Info")]
    public Vector3 weightForce;
    [Space(10)]
    public Vector3 weightForceLeft;
    [Space(5)]
    public Vector3 weightForceLeftLocalVertical;
    public Vector3 weightForceLeftLocalDownward;
    public Vector3 weightForceLeftLocalHorizontal;
    [Space(10)]
    public Vector3 weightForceRight;
    [Space(5)]
    public Vector3 weightForceRightLocalVertical;
    public Vector3 weightForceRightLocalDownward;
    public Vector3 weightForceRightLocalHorizontal;

    [Header("Bipedal - Physics - Feet Velocities Info")]
    public Vector3 feetSpeedLeftDown = Vector3.zero;
    public Vector3 feetSpeedRightDown = Vector3.zero;
    [Space(5)]
    public Vector3 feetSpeedLeftUp = Vector3.zero;
    public Vector3 feetSpeedRightUp = Vector3.zero;

    [Header("Bipedal - Physics - Impulse and Momentum Forces Info")]
    public Vector3 feetImpulseLeft = Vector3.zero;
    public Vector3 feetImpulseRight = Vector3.zero;
    [Space(10)]
    public Vector3 momentumForce = Vector3.zero;
    [Space(10)]
    public Vector3 momentumForceLeft = Vector3.zero;
    [Space(5)]
    public Vector3 momentumForceLeftLocalVertical;
    public Vector3 momentumForceLeftLocalDownward;
    public Vector3 momentumForceLeftLocalHorizontal;
    [Space(10)]
    public Vector3 momentumForceRight = Vector3.zero;
    [Space(5)]
    public Vector3 momentumForceRightLocalVertical;
    public Vector3 momentumForceRightLocalDownward;
    public Vector3 momentumForceRightLocalHorizontal;

    [Header("Bipedal - Physics - GRF Info")]
    public Vector3 totalGRForce;
    [Space(10)]
    public Vector3 totalGRForceLeft;
    [Space(5)]
    public Vector3 totalGRForceLeftLocalVertical;
    public Vector3 totalGRForceLeftLocalDownward;
    public Vector3 totalGRForceLeftLocalHorizontal;
    [Space(10)]
    public Vector3 totalGRForceRight;
    [Space(5)]
    public Vector3 totalGRForceRightLocalVertical;
    public Vector3 totalGRForceRightLocalDownward;
    public Vector3 totalGRForceRightLocalHorizontal;

    [Header("Bipedal - Physics - Feet Forces Info")]
    public Vector3 totalForceFoot;
    [Space(10)]
    public Vector3 totalForceLeftFoot;
    [Space(5)]
    public Vector3 totalForceLeftLocalVertical;
    public Vector3 totalForceLeftLocalDownward;
    public Vector3 totalForceLeftLocalHorizontal;
    public Vector3 totalForceLeftLocalTerrainProjection;
    [Space(10)]
    public Vector3 totalForceRightFoot;
    [Space(5)]
    public Vector3 totalForceRightLocalVertical;
    public Vector3 totalForceRightLocalDownward;
    public Vector3 totalForceRightLocalHorizontal;
    public Vector3 totalForceRightLocalTerrainProjection;

    [Header("UI for DEMO mode")]
    public Slider youngSlider;
    public Slider timeSlider;
    public Slider poissonSlider;
    public Slider iterationsSlider;
    public Toggle activateToggleDef;
    public Toggle activateToggleBump;
    public Toggle activateToggleGauss;
    public Toggle activateToggleShowGrid;
    public Toggle activateToggleShowBump;
    public Toggle activateToggleShowForceModel;

    #endregion

    #region Instance Properties

    public bool RunningCoroutineDeformationLeft { get; set; }
    public bool RunningCoroutineStabilizationLeft { get; set; }
    public bool RunningCoroutineDeformationRight { get; set; }
    public bool RunningCoroutineStabilizationRight { get; set; }

    public TerrainData MyTerrainData
    {
        get { return _terrainData; }
    }

    public Vector3 OriginLeft
    {
        get { return _originLeft; }
        set { _originLeft = value; }
    }
    public Vector3 OriginRight
    {
        get { return _originRight; }
        set { _originRight = value; }
    }

    public float LeftHeelHeight
    {
        get { return _leftHeelHeight; }
        set { _leftHeelHeight = value; }
    }
    public float LeftToeHeight
    {
        get { return _leftToeHeight; }
        set { _leftToeHeight = value; }
    }
    public float RightHeelHeight
    {
        get { return _rightHeelHeight; }
        set { _rightHeelHeight = value; }
    }
    public float RightToeHeight
    {
        get { return _rightToeHeight; }
        set { _rightToeHeight = value; }
    }

    // Arrays to send to server
 
    public static float[,] HeightMapMatrix { get; set; }
    public static byte[] HeightMapBytes { get; set; }

    public static float[,] PressureMapMatrix { get; set; }
    public static byte[] PressureMapBytes { get; set; }

    public static double[,] YoungMapMatrix { get; set; }
    public static byte[] YoungMapBytes { get; set; }

    #endregion

    #region Read-only & Static Fields

    // Character
    private Vector3 _centerGridLeftFoot;
    private Vector3 _centerGridRightFoot;
    private Animator _anim;
    private Rigidbody _rb;
    private Vector3 _COMPosition;
    private Vector3 _COMPositionProjected;

    // Origins
    private Vector3 _originLeft;
    private Vector3 _originRight;

    // Heights
    private float _leftHeelHeight;
    private float _leftToeHeight;
    private float _rightHeelHeight;
    private float _rightToeHeight;

    // Types of brushes
    private BrushPhysicalFootprint _brushPhysicalFootprint;

    // Terrain Properties
    private Terrain _terrain;
    private Collider _terrainCollider;
    private TerrainData _terrainData;
    private Vector3 _terrainSize;
    private float _cellSize;
    private float _lengthCellX;
    private float _lengthCellZ;
    private int _heightmapWidth;
    private int _heightmapHeight;
    private float[,] _heightmapData;
    private float[,] _heightmapDataConstant;
    private float[,] _heightmapDataFiltered;

    // Additional
    //private bool oldIsMoving = false;
    //private bool isMoving = false;
    //private int provCounter = 0;

    #endregion

    /*
    #region Plotting

    //               Extra for plotting              //
    // ============================================= //
    
    [UPyPlot.UPyPlotController.UPyProbe]
    private float weightForceLeftX = 0f;
    [UPyPlot.UPyPlotController.UPyProbe]
    private float weightForceLeftY = 0f; 
    [UPyPlot.UPyPlotController.UPyProbe]
    private float weightForceLeftZ = 0f;

    [UPyPlot.UPyPlotController.UPyProbe]
    private float weightForceRightX = 0f;
    [UPyPlot.UPyPlotController.UPyProbe]
    private float weightForceRightY = 0f;
    [UPyPlot.UPyPlotController.UPyProbe]
    private float weightForceRightZ = 0f;

    ////

    [UPyPlot.UPyPlotController.UPyProbe]
    private float momentumForceLeftX = 0f;
    [UPyPlot.UPyPlotController.UPyProbe]
    private float momentumForceLeftY = 0f;
    [UPyPlot.UPyPlotController.UPyProbe]
    private float momentumForceLeftZ = 0f;

    [UPyPlot.UPyPlotController.UPyProbe]
    private float momentumForceRightX = 0f;
    [UPyPlot.UPyPlotController.UPyProbe]
    private float momentumForceRightY = 0f;
    [UPyPlot.UPyPlotController.UPyProbe]
    private float momentumForceRightZ = 0f;

    ////

    [UPyPlot.UPyPlotController.UPyProbe]
    private float totalGRForceLeftX = 0f;
    [UPyPlot.UPyPlotController.UPyProbe]
    private float totalGRForceLeftY = 0f;
    [UPyPlot.UPyPlotController.UPyProbe]
    private float totalGRForceLeftZ = 0f;

    [UPyPlot.UPyPlotController.UPyProbe]
    private float totalGRForceRightX = 0f;
    [UPyPlot.UPyPlotController.UPyProbe]
    private float totalGRForceRightY = 0f;
    [UPyPlot.UPyPlotController.UPyProbe]
    private float totalGRForceRightZ = 0f;

    ////

    [UPyPlot.UPyPlotController.UPyProbe]
    private float totalForceLeftX = 0f;
    [UPyPlot.UPyPlotController.UPyProbe]
    private float totalForceLeftY = 0f;
    [UPyPlot.UPyPlotController.UPyProbe]
    private float totalForceLeftZ = 0f;

    [UPyPlot.UPyPlotController.UPyProbe]
    private float totalForceRightX = 0f;
    [UPyPlot.UPyPlotController.UPyProbe]
    private float totalForceRightY = 0f;
    [UPyPlot.UPyPlotController.UPyProbe]
    private float totalForceRightZ = 0f;

    // ============================================= //

    #endregion
    */

    #region Unity Methods

    void Awake()
    {

    }

    private void Start()
    {
        // Extract terrain information
        if (!_terrain)
        {
            //terrain = Terrain.activeTerrain;
            //terrain = myBipedalCharacter.GetComponent<RigidBodyControllerSimpleAnimator>().currentTerrain;
            _terrain = myBipedalCharacter.GetComponent<DetectTerrain>().CurrentTerrain;
            Debug.Log("[INFO] First terrain: " + _terrain.name);
        }

        // Retrieve terrain data
        _terrainCollider = _terrain.GetComponent<Collider>();
        _terrainData = _terrain.terrainData;
        _terrainSize = _terrainData.size;
        _heightmapWidth = _terrainData.heightmapResolution;
        _heightmapHeight = _terrainData.heightmapResolution;
        _heightmapData = _terrainData.GetHeights(0, 0, _heightmapWidth, _heightmapHeight);
        _heightmapDataConstant = _terrainData.GetHeights(0, 0, _heightmapWidth, _heightmapHeight);
        _heightmapDataFiltered = _terrainData.GetHeights(0, 0, _heightmapWidth, _heightmapHeight);
        _brushPhysicalFootprint = null;

        _lengthCellX = _terrainSize.x / (_heightmapWidth - 1);
        _lengthCellZ = _terrainSize.z / (_heightmapHeight - 1);
        _cellSize = _lengthCellX * _lengthCellZ;

        // Get classes
        //feetPlacement = FindObjectOfType<IKFeetPlacement>();

        // Retrieve components and attributes from character
        mass = myBipedalCharacter.GetComponent<Rigidbody>().mass;
        _anim = myBipedalCharacter.GetComponent<Animator>();
        _rb = myBipedalCharacter.GetComponent<Rigidbody>();

        // Maps
        HeightMapMatrix = new float[_terrainData.heightmapResolution, _terrainData.heightmapResolution];
        HeightMapBytes = new byte[HeightMapMatrix.Length * sizeof(float)];
        PressureMapMatrix = new float[_terrainData.heightmapResolution, _terrainData.heightmapResolution];
        PressureMapBytes = new byte[PressureMapMatrix.Length * sizeof(float)];
        YoungMapMatrix = new double[_terrainData.heightmapResolution, _terrainData.heightmapResolution];
        YoungMapBytes = new byte[YoungMapMatrix.Length * sizeof(double)];

        // Others
        //oldIsMoving = _anim.GetBool("isMoving");
        
        // Build initial normal matrix
        initialNormalMatrix = new Vector3[_terrainData.heightmapResolution, _terrainData.heightmapResolution];
        for (int i = 0; i < _terrainData.heightmapResolution; i++)
        {
            for (int j = 0; j < _terrainData.heightmapResolution; j++)
            {
                float pos_x = Grid2World(i, j).x / _terrain.terrainData.size.x;
                float pos_z = Grid2World(i, j).z / _terrain.terrainData.size.z;
                
                initialNormalMatrix[i, j] = _terrainData.GetInterpolatedNormal(pos_x, pos_z);
            }
        }

        // Build initial gradient matrix
        initialGradientMatrix = new float[_terrainData.heightmapResolution, _terrainData.heightmapResolution];
        for (int i = 0; i < _terrainData.heightmapResolution; i++)
        {
            for (int j = 0; j < _terrainData.heightmapResolution; j++)
            {
                float pos_x = Grid2World(i, j).x / _terrain.terrainData.size.x;
                float pos_z = Grid2World(i, j).z / _terrain.terrainData.size.z;

                initialGradientMatrix[i, j] = _terrainData.GetSteepness(pos_x, pos_z);
            }
        }

        if (exportMaps)
        {
            InvokeRepeating("ExportHeightMap", 2.0f, 1f);
            InvokeRepeating("ExportYoungMap", 2.0f, 1f);
        }
    }

    void Update()
    {
        // TEST
        totalTime += Time.deltaTime;

        // 0. Update mass (in case we change the mass during the simulation)
        mass = myBipedalCharacter.GetComponent<Rigidbody>().mass;

        // 1. Contact and Angle
        GetTerrainSlopeCOM();

        // 2. Define source of deformation where we are
        DefineSourceDeformation();

        // 3. If we change the terrain, we change the data as well - both must have different GameObject names
        UpdateTerrain();

        // ---

        // 0. Save heights
        LeftHeelHeight = feetPlacement.LeftHeelHeight;
        LeftToeHeight = feetPlacement.LeftToeHeight;
        RightHeelHeight = feetPlacement.RightHeelHeight;
        RightToeHeight = feetPlacement.RightToeHeight;

        // 1. Calculate Origins
        CheckOriginLeft(feetPlacement.isLeftHeelGrounded, feetPlacement.isLeftToeGrounded);
        CheckOriginRight(feetPlacement.isRightHeelGrounded, feetPlacement.isRightToeGrounded);

        // 2. Calculate weights distribution
        CalculateWeights();

        // 3. Calculate velocity for the feet
        CalculateFeetVelocity();

        // 4. Saving other variables for debugging purposes
        UpdateFeetPositions();

        // 5. Calculate Force Model
        CalculateForceModel();

        // Quadrupeds Information //
        /////////// TODO ///////////

        // 6. Extra Debug
        DebugForceModel();

        // 7. Apply procedural brush to deform the terrain based on the force model
        ApplyDeformation(); // -> HEAVY RUNTIME!
    }

    private void ExportHeightMap()
    {
        for (int y = 0; y < _terrainData.heightmapResolution; y++)
        {
            for (int x = 0; x < _terrainData.heightmapResolution; x++)
            {
                HeightMapMatrix[x,y] = Get(x, y);
            }
        }
        
        HeightMapBytes = BytesConversion.ToBytes(HeightMapMatrix);
    }

    private void ExportYoungMap()
    {
        for (int y = 0; y < _terrainData.heightmapResolution; y++)
        {
            for (int x = 0; x < _terrainData.heightmapResolution; x++)
            {
                YoungMapMatrix[x, y] = _brushPhysicalFootprint.YoungModulusGround + _brushPhysicalFootprint.YoungModulusVegetation * VegetationCreator.LivingRatioMatrix[x, y];
            }
        }

        YoungMapBytes = BytesConversion.ToBytes(YoungMapMatrix);
    }

    private void ExportPressureMap()
    {
        foreach (Vector3 cell in _brushPhysicalFootprint.CounterPositionsListLeft)
        {
            // OPTION 1
            //PressureMapMatrix[(int)cell.x, (int)cell.z] += (_brushPhysicalFootprint.PressureLeft); // Add Pressure left foot

            // OPTION 2
            //if(totalTime != 0)
            //    contactRatioLeft = accumulatedTimeDuringPressureLeft / totalTime;
            //if(!MxM.RetrieveVelocity.IsIdle)
            //    PressureMapMatrix[(int)cell.x, (int)cell.z] += (_brushPhysicalFootprint.PressureLeft) * contactRatioLeft; // Add Pressure left foot
            //else
            //    PressureMapMatrix[(int)cell.x, (int)cell.z] += 0; // Add Pressure left foot

            // OPTION 3
            if(!MxM.RetrieveVelocity.IsIdle)
                PressureMapMatrix[(int)cell.x, (int)cell.z] += (_brushPhysicalFootprint.PressureLeft); // Add Pressure left foot
            else
                PressureMapMatrix[(int)cell.x, (int)cell.z] += 0; // Add Pressure left foot

        }

        foreach (Vector3 cell in _brushPhysicalFootprint.CounterPositionsListRight)
        {
            // OPTION 1
            //PressureMapMatrix[(int)cell.x, (int)cell.z] += (_brushPhysicalFootprint.PressureRight); // Add Pressure right foot

            // OPTION 2
            //if (totalTime != 0)
            //    contactRatioRight = accumulatedTimeDuringPressureRight / totalTime;
            //if (!MxM.RetrieveVelocity.IsIdle)
            //    PressureMapMatrix[(int)cell.x, (int)cell.z] += (_brushPhysicalFootprint.PressureRight) * contactRatioRight; // Add Pressure right foot
            //else
            //    PressureMapMatrix[(int)cell.x, (int)cell.z] += 0; // Add Pressure right foot

            // OPTION 3
            if (!MxM.RetrieveVelocity.IsIdle)
                PressureMapMatrix[(int)cell.x, (int)cell.z] += (_brushPhysicalFootprint.PressureRight); // Add Pressure right foot
            else
                PressureMapMatrix[(int)cell.x, (int)cell.z] += 0; // Add Pressure right foot
        }

        // OPTION 4
        //for (int i = 0; i < PressureMapMatrix.GetLength(0); i++)
        //{
        //    for (int j = 0; j < PressureMapMatrix.GetLength(1); j++)
        //    {
        //        PressureMapMatrix[i, j] *= (1 - contactRatioRight);
        //    }
        //}

        PressureMapBytes = BytesConversion.ToBytes(PressureMapMatrix);
    }

    #endregion

    #region Terrain-Update Methods

    /// <summary>
    /// Gets Terrain Slope angle (sign changes if going up or down).
    /// </summary>
    private void GetTerrainSlopeCOM()
    {        
        // Get Position
        float pos_x = _rb.position.x / _terrain.terrainData.size.x;
        float pos_z = _rb.position.z / _terrain.terrainData.size.z;

        // Normal in that point
        Vector3 normal = _terrain.terrainData.GetInterpolatedNormal(pos_x, pos_z);
        Vector3 localNormal = this.transform.InverseTransformDirection(normal);

        // Original normal in that point
        Vector3 initialNormal = initialNormalMatrix[(int)World2Grid(_rb.position).x, (int)World2Grid(_rb.position).z];
        Vector3 initialNormalLocal = this.transform.InverseTransformDirection(initialNormal);

        // Gradient in that point
        float gradient = _terrain.terrainData.GetSteepness(pos_x, pos_z);

        // Original gradient in that point
        float initialGradient = initialGradientMatrix[(int)World2Grid(_rb.position).x, (int)World2Grid(_rb.position).z];

        // Save normals (current and original)
        slopeNormal = localNormal;
        slopeInitialNormal = initialNormalLocal;
        
        // New, for signed angle
        float dotProductDirectionGround = Vector3.Dot(_rb.GetComponent<Transform>().forward.normalized, localNormal.normalized);
        slopeAngle = dotProductDirectionGround < 0 ? gradient : -gradient;
        slopeInitialAngle = dotProductDirectionGround < 0 ? initialGradient : -initialGradient;

        // Calculate COM Projection on the terrain
        _COMPosition = _rb.worldCenterOfMass;
        
        // If the character is walking in an obstacle, instead of the terrain, we replace the values
        RaycastHit hitCOM;
        if (Physics.Raycast(_COMPosition, -Vector3.up, out hitCOM, Mathf.Infinity, ground))
        {
            _COMPositionProjected = hitCOM.point;

            if(hitCOM.collider.gameObject.CompareTag("Obstacle"))
            {
                slopeNormal = hitCOM.normal;
                slopeAngle = Vector3.Angle(Vector3.up, hitCOM.normal);

                // New, for signed angle
                float dotProductDirectionObstacle = Vector3.Dot(_rb.GetComponent<Transform>().forward.normalized, hitCOM.normal.normalized);        
                slopeAngle = dotProductDirectionObstacle < 0 ? slopeAngle : -slopeAngle; 
            }
        }

        if (drawAngleNormal)
            Debug.DrawLine(_rb.position, _rb.position + normal, Color.cyan);
        
        if (drawCOMProjection)
            Debug.DrawRay(_COMPositionProjected, Vector3.up, Color.cyan);
    }

    /// <summary>
    /// Defines the source of deformation.
    /// </summary>
    private void DefineSourceDeformation()
    {
        // Select type of deformation (UI, Prefabs or Manual)
        switch (deformationChoice)
        {
            case sourceDeformation.useUI:
                if (_brushPhysicalFootprint)
                {
                    // Time is managed from the master script - others from the footprint script
                    contactTime = timeSlider.value;

                    // Force Model UI
                    drawWeightForces = activateToggleShowForceModel.isOn;
                    drawMomentumForces = activateToggleShowForceModel.isOn;
                    drawGRForces = activateToggleShowForceModel.isOn;
                    drawFeetForces = activateToggleShowForceModel.isOn;
                }
                break;
            case sourceDeformation.useManualValues:
                break;
            case sourceDeformation.useTerrainPrefabs:
                if (_brushPhysicalFootprint)
                {
                    if (_terrain.CompareTag("Snow"))
                        DefineSnow();
                    else if (_terrain.CompareTag("Dry Sand"))
                        DefineDrySand();
                    else if (_terrain.CompareTag("Mud"))
                        DefineMud();
                    else if (_terrain.CompareTag("Soil"))
                        DefineSoil();
                    else
                        DefineDefault();
                }
                break;
            case sourceDeformation.useManualValuesWithVegetation:
                if (_brushPhysicalFootprint)
                {
                    // TODO: Transfer Vegetation Information

                    _brushPhysicalFootprint.AlphaVegetation = VegetationCreator.LivingRatioMatrix;
                }
                break;
        }
    }

    /// <summary>
    /// Updates the terrain data if we change the terrain.
    /// </summary>
    private void UpdateTerrain()
    {
        // Every time we change to other terrainData, we update
        if (_terrain.name != myBipedalCharacter.GetComponent<DetectTerrain>().CurrentTerrain.name)
        {
            // Extract terrain information
            //terrain = myBipedalCharacter.GetComponent<RigidBodyControllerSimpleAnimator>().currentTerrain;
            _terrain = myBipedalCharacter.GetComponent<DetectTerrain>().CurrentTerrain;
            Debug.Log("[INFO] Updating to new terrain: " + _terrain.name);

            _terrainCollider = _terrain.GetComponent<Collider>();
            _terrainData = _terrain.terrainData;
            _terrainSize = _terrainData.size;
            _heightmapWidth = _terrainData.heightmapResolution;
            _heightmapHeight = _terrainData.heightmapResolution;
            _heightmapData = _terrainData.GetHeights(0, 0, _heightmapWidth, _heightmapHeight);
            _heightmapDataConstant = _terrainData.GetHeights(0, 0, _heightmapWidth, _heightmapHeight);
            _heightmapDataFiltered = _terrainData.GetHeights(0, 0, _heightmapWidth, _heightmapHeight);
        }
    }

    #endregion

    #region Body-Update Methods

    /// <summary>
    /// Gets weight for each foot.
    /// </summary>
    private void CalculateWeights()
    {
        // Estimate the weight for each foot based on the COM position
        rightWeightDistribution = (_COMPositionProjected - feetPlacement.LeftFootIKPosition).magnitude / (feetPlacement.RightFootIKPosition - feetPlacement.LeftFootIKPosition).magnitude;
        leftWeightDistribution = 1 - rightWeightDistribution;
    }

    /// <summary>
    /// Estimates velocities in the feet.
    /// </summary>
    private void CalculateFeetVelocity()
    {
        #region 1. Frame-based

        // Left to compare with the new velocity
        //newIKLeftPosition = _anim.GetBoneTransform(HumanBodyBones.LeftFoot).position; // Before: LeftFoot
        //newIKRightPosition = _anim.GetBoneTransform(HumanBodyBones.RightFoot).position;
        //var mediaLeft = (newIKLeftPosition - oldIKLeftPosition);
        //var mediaRight = (newIKRightPosition - oldIKRightPosition);

        //Vector3 feetSpeedLeftManual = new Vector3((mediaLeft.x / Time.fixedDeltaTime), (mediaLeft.y / Time.fixedDeltaTime), (mediaLeft.z / Time.fixedDeltaTime));
        //Vector3 feetSpeedRightManual = new Vector3((mediaRight.x / Time.fixedDeltaTime), (mediaRight.y / Time.fixedDeltaTime), (mediaRight.z / Time.fixedDeltaTime));

        //oldIKLeftPosition = newIKLeftPosition;
        //oldIKRightPosition = newIKRightPosition;

        //newIKLeftPosition = _anim.GetBoneTransform(HumanBodyBones.LeftFoot).position;
        //newIKRightPosition = _anim.GetBoneTransform(HumanBodyBones.RightFoot).position;

        //if (drawManualVelocities)
        //{
        //    DrawForce.ForDebug3DVelocity(oldIKLeftPosition, feetSpeedLeftManual, Color.red, 1f);
        //    DrawForce.ForDebug3DVelocity(oldIKRightPosition, feetSpeedRightManual, Color.red, 1f);
        //}

        //posLFoot = _anim.GetBoneTransform(HumanBodyBones.LeftFoot).position; // Before: LeftFoot
        //posLToe = _anim.GetBoneTransform(HumanBodyBones.LeftToes).position; // Before: LeftFoot
        //var mediaLeftFoot = (posLFoot - oldposLFoot);
        //var mediaLeftToe = (posLToe - oldposLToe);
        //Vector3 feetSpeedLeftFootManual = new Vector3((mediaLeftFoot.x / Time.fixedDeltaTime), (mediaLeftFoot.y / Time.fixedDeltaTime), (mediaLeftFoot.z / Time.fixedDeltaTime));
        //Vector3 feetSpeedLeftToeManual = new Vector3((mediaLeftToe.x / Time.fixedDeltaTime), (mediaLeftToe.y / Time.fixedDeltaTime), (mediaLeftToe.z / Time.fixedDeltaTime));
        //oldposLFoot = posLFoot;
        //oldposLToe = posLToe;
        //posLFoot = _anim.GetBoneTransform(HumanBodyBones.LeftFoot).position;
        //posLToe = _anim.GetBoneTransform(HumanBodyBones.LeftToes).position;

        //Debug.Log("feetSpeedLeftFootManual: " + feetSpeedLeftFootManual.magnitude);
        //Debug.Log("feetSpeedLeftToeManual: " + feetSpeedLeftToeManual.magnitude);


        #endregion

        #region 2. Using RBs

        // Calculate New Velocity for the feet //
        // =================================== //

        //feetSpeedLeft = leftFootRB.velocity;
        //feetSpeedRight = rightFootRB.velocity;

        //if (drawRBVelocities)
        //{
        //    DrawForce.ForDebug3DVelocity(oldIKLeftPosition, feetSpeedLeft, Color.cyan, 1f);
        //    DrawForce.ForDebug3DVelocity(oldIKRightPosition, feetSpeedRight, Color.cyan, 1f);
        //}

        #endregion

        #region 3. Using MM

        // For slopes, we check the sign of the dot product between the velocity and the normal of the terrain
        // Three cases: 1) Foot Heel 2) Foot Toe

        // Left Foot
        if (LeftHeelHeight < LeftToeHeight)
        {
            if (Vector3.Dot(MxM.RetrieveVelocity.VelocityLeftFoot, slopeNormal) < 0f)
            {
                feetSpeedLeftDown = MxM.RetrieveVelocity.VelocityLeftFoot;
                feetSpeedLeftUp = Vector3.zero;
            }
            else
            {
                feetSpeedLeftDown = Vector3.zero;
                feetSpeedLeftUp = MxM.RetrieveVelocity.VelocityLeftFoot;
            }
        }
        else
        {
            if (Vector3.Dot(MxM.RetrieveVelocity.VelocityLeftToe, slopeNormal) < 0f)
            {
                feetSpeedLeftDown = MxM.RetrieveVelocity.VelocityLeftToe;
                feetSpeedLeftUp = Vector3.zero;
            }
            else
            {
                feetSpeedLeftDown = Vector3.zero;
                feetSpeedLeftUp = MxM.RetrieveVelocity.VelocityLeftToe;
            }
        }

        // Right Foot
        if (RightHeelHeight < RightToeHeight)
        {
            if (Vector3.Dot(MxM.RetrieveVelocity.VelocityRightFoot, slopeNormal) < 0f)
            {
                feetSpeedRightDown = MxM.RetrieveVelocity.VelocityRightFoot;
                feetSpeedRightUp = Vector3.zero;
            }
            else
            {
                feetSpeedRightDown = Vector3.zero;
                feetSpeedRightUp = MxM.RetrieveVelocity.VelocityRightFoot;
            }
        }
        else
        {
            if (Vector3.Dot(MxM.RetrieveVelocity.VelocityRightToe, slopeNormal) < 0f)
            {
                feetSpeedRightDown = MxM.RetrieveVelocity.VelocityRightToe;
                feetSpeedRightUp = Vector3.zero;
            }
            else
            {
                feetSpeedRightDown = Vector3.zero;
                feetSpeedRightUp = MxM.RetrieveVelocity.VelocityRightToe;
            }
        }

        #endregion

        if (drawMMVelocities)
        {
            if (LeftHeelHeight < LeftToeHeight)
            {
                DrawForce.ForDebug3DVelocity(_anim.GetBoneTransform(HumanBodyBones.LeftFoot).position, feetSpeedLeftDown, Color.cyan, 1f);
                DrawForce.ForDebug3DVelocity(_anim.GetBoneTransform(HumanBodyBones.LeftFoot).position, feetSpeedLeftUp, Color.blue, 1f);
            }
            else
            {
                DrawForce.ForDebug3DVelocity(_anim.GetBoneTransform(HumanBodyBones.LeftToes).position, feetSpeedLeftDown, Color.cyan, 1f);
                DrawForce.ForDebug3DVelocity(_anim.GetBoneTransform(HumanBodyBones.LeftToes).position, feetSpeedLeftUp, Color.blue, 1f);
            }

            if (RightHeelHeight < RightToeHeight)
            {
                DrawForce.ForDebug3DVelocity(_anim.GetBoneTransform(HumanBodyBones.RightFoot).position, feetSpeedRightDown, Color.cyan, 1f);
                DrawForce.ForDebug3DVelocity(_anim.GetBoneTransform(HumanBodyBones.RightFoot).position, feetSpeedRightUp, Color.blue, 1f);
            }
            else
            {
                DrawForce.ForDebug3DVelocity(_anim.GetBoneTransform(HumanBodyBones.RightToes).position, feetSpeedRightDown, Color.cyan, 1f);
                DrawForce.ForDebug3DVelocity(_anim.GetBoneTransform(HumanBodyBones.RightToes).position, feetSpeedRightUp, Color.blue, 1f);
            }
        }
    }

    /// <summary>
    /// Updates the feet/grid positions.
    /// </summary>
    private void UpdateFeetPositions()
    {
        // Save height of feet IK
        heightIKLeft = feetPlacement.LeftFootIKPosition.y;
        heightIKRight = feetPlacement.RightFootIKPosition.y;

        // Flags to detect when the foot is on the ground
        isLeftFootGrounded = feetPlacement.isLeftFootGrounded;
        isRightFootGrounded = feetPlacement.isRightFootGrounded;
        isLeftFootHeelGrounded = feetPlacement.isLeftHeelGrounded;
        isLeftFootToeGrounded = feetPlacement.isLeftToeGrounded;
        isRightFootHeelGrounded = feetPlacement.isRightHeelGrounded;
        isRightFootToeGrounded = feetPlacement.isRightToeGrounded;

        // Grid-based feet positions and heights
        _centerGridLeftFoot = World2Grid(feetPlacement.LeftFootIKPosition.x, feetPlacement.LeftFootIKPosition.z);
        _centerGridRightFoot = World2Grid(feetPlacement.RightFootIKPosition.x, feetPlacement.RightFootIKPosition.z);
        centerGridLeftFootHeight = new Vector3(feetPlacement.LeftFootIKPosition.x, Get(_centerGridLeftFoot.x, _centerGridLeftFoot.z), feetPlacement.LeftFootIKPosition.z);
        centerGridRightFootHeight = new Vector3(feetPlacement.RightFootIKPosition.x, Get(_centerGridRightFoot.x, _centerGridRightFoot.z), feetPlacement.RightFootIKPosition.z);
    }

    /// <summary>
    /// Estimate gait forces.
    /// </summary>
    private void CalculateForceModel()
    {
        // Bipedal -- _anim.pivotWeight only for bipeds
        if (isLeftFootGrounded && isRightFootGrounded)
        {
            // Estimate weights from CalculateWeights() - pivot not working with root MM motion
            weightInLeftFoot = leftWeightDistribution;
            weightInRightFoot = rightWeightDistribution;
        }
        else
        {
            if (!isLeftFootGrounded)
            {
                weightInLeftFoot = 0f;
                weightInRightFoot = 1f;
            }
            else if (!isRightFootGrounded)
            {
                weightInLeftFoot = 1f;
                weightInRightFoot = 0f;
            }
        }

        //               Extra for plotting              //
        // ============================================= //
        //weightLeftFloat = weightInLeftFoot;
        //weightRightFloat = weightInRightFoot;
        // ============================================= //

        //       Bipedal Force Model       //
        // =============================== //

        if (_brushPhysicalFootprint)
        {
            EstimateWeightForces();
            EstimateMomentumForces();
            EstimateGRForces();
            EstimateFeetForces();

            if (exportMaps)
                ExportPressureMap();
        }
    }

    /// <summary>
    /// Debug forces.
    /// </summary>
    private void DebugForceModel()
    {
        // Print the position of the feet in both systems (world and grid)
        if (printFeetPositions)
        {
            Debug.Log("[INFO] Left Foot Coords (World): " + feetPlacement.LeftFootIKPosition.ToString());
            Debug.Log("[INFO] Left Foot Coords (Grid): " + _centerGridLeftFoot.ToString());
            Debug.Log("[INFO] Right Foot Coords (World): " + feetPlacement.RightFootIKPosition.ToString());
            Debug.Log("[INFO] Right Foot Coords (Grid): " + _centerGridRightFoot.ToString());
            Debug.Log("-----------------------------------------");
        }

        // Print the forces
        if (printFeetForces)
        {
            Debug.Log("[INFO] Weight Force: " + weightForce);

            Debug.Log("[INFO] Left Foot Speed: " + feetSpeedLeftDown);
            Debug.Log("[INFO] Right Foot Speed: " + feetSpeedRightDown);

            Debug.Log("[INFO] Left Foot Impulse: " + feetImpulseLeft);
            Debug.Log("[INFO] Right Foot Impulse: " + feetImpulseRight);

            Debug.Log("[INFO] Left Foot Momentum: " + momentumForceLeft);
            Debug.Log("[INFO] Right Foot Momentum: " + momentumForceRight);

            Debug.Log("[INFO] GRF Left Foot: " + totalGRForceLeft);
            Debug.Log("[INFO] GRF Right Foot: " + totalGRForceRight);

            Debug.Log("[INFO] Total Force Left Foot: " + totalForceLeftFoot);
            Debug.Log("[INFO] Total Force Right Foot: " + totalForceRightFoot);
            Debug.Log("-----------------------------------------");
        }
    }

    #endregion

    #region Deformation-Update Methods

    /// <summary>
    /// It calls the footprint brush based on dynamics and position.
    /// </summary>
    private void ApplyDeformation()
    {
        // Keep track of one timeframe for each foot
        timePassedLeft += Time.deltaTime;
        timePassedRight += Time.deltaTime;

        // Apply brush to feet (always enters)
        if (_brushPhysicalFootprint)
        {
            // Brush is only called if we are within the contactTime
            // Due to the small values, the provisional solution requires to add an offset to give the system enough time to create the footprint
            
            // If the time is less than the characteristic time, we apply the brush. Otherwise, we stop all the parallel processes
            
            // Left Foot
            if (timePassedLeft <= contactTime + offset)
            {
                if (printSteps)
                {
                    Debug.Log("1 - [INFO] Applying Brush Deformation LEFT - Time passed: " + timePassedLeft);
                }

                // Brush that takes limbs positions and creates physically-based deformations
                _brushPhysicalFootprint.CallFootprint(feetPlacement.LeftFootIKPosition.x, feetPlacement.LeftFootIKPosition.z,
                    feetPlacement.RightFootIKPosition.x, feetPlacement.RightFootIKPosition.z);

                // Reset the stabilization counter and change flag
                counterStabilizationLeft = 0;
                RunningCoroutineStabilizationLeft = false;

                // TEST
                //timeDuringPressureLeft = timePassedLeft;

                // TEST2
                timeDuringPressureLeft += Time.deltaTime;
            }
            else
            {
                // Change flags
                RunningCoroutineStabilizationLeft = true;
                RunningCoroutineDeformationLeft = false;

                // Start the stabilization process for each iteration
                if (counterStabilizationLeft <= maxStabilizations)
                {
                    if (printSteps)
                    {
                        Debug.Log("2 - [INFO] Applying Stabilization LEFT - Iteration " + counterStabilizationLeft + "/" + maxStabilizations);
                    }
                    
                    _brushPhysicalFootprint.StabilizeFootprint(feetPlacement.LeftFootIKPosition.x, feetPlacement.LeftFootIKPosition.z, feetPlacement.RightFootIKPosition.x, feetPlacement.RightFootIKPosition.z);
                    
                    counterStabilizationLeft++;
                }

                // TEST
                //accumulatedTimeDuringPressureLeft += timeDuringPressureLeft;
                //timeDuringPressureLeft = 0f;

                // TEST2
                accumulatedTimeDuringPressureLeft += timeDuringPressureLeft;
                timeDuringPressureLeft = 0f;
            }

            // Right Foot
            if (timePassedRight <= contactTime + offset)
            {
                if (printSteps)
                {
                    Debug.Log("1 - [INFO] Applying Brush Deformation RIGHT - Time passed: " + timePassedRight);
                }

                // Brush that takes limbs positions and creates physically-based deformations
                _brushPhysicalFootprint.CallFootprint(feetPlacement.LeftFootIKPosition.x, feetPlacement.LeftFootIKPosition.z,
                    feetPlacement.RightFootIKPosition.x, feetPlacement.RightFootIKPosition.z);

                // Reset the stabilization counter and change flag
                counterStabilizationRight = 0;
                RunningCoroutineStabilizationRight = false;

                // TEST
                //timeDuringPressureRight = timePassedRight;

                // TEST2
                //Debug.Log("Summing Time.deltaTime to timeDuringPressureRight: " + timeDuringPressureRight);
                timeDuringPressureRight += Time.deltaTime;
            }
            else
            {                
                // Change flags
                RunningCoroutineStabilizationRight = true;
                RunningCoroutineDeformationRight = false;

                // Start the stabilization process for each iteration
                if (counterStabilizationRight <= maxStabilizations)
                {
                    if (printSteps)
                    {
                        Debug.Log("2 - [INFO] Applying Stabilization RIGHT - Iteration " + counterStabilizationRight + "/" + maxStabilizations);
                    }

                    _brushPhysicalFootprint.StabilizeFootprint(feetPlacement.LeftFootIKPosition.x, feetPlacement.LeftFootIKPosition.z, feetPlacement.RightFootIKPosition.x, feetPlacement.RightFootIKPosition.z);
                    
                    counterStabilizationRight++;
                }

                // TEST
                //accumulatedTimeDuringPressureRight += timeDuringPressureRight;
                //Debug.Log("Summing timeDuringPressureRight " + timeDuringPressureRight + ", total accumulatedTimeDuringPressureRight: " + accumulatedTimeDuringPressureRight);
                //timeDuringPressureRight = 0f;

                // TEST2
                //Debug.Log("[RIGHT GROUNDED TIME REACHED] Summing timeDuringPressureRight " + timeDuringPressureRight + ", total accumulatedTimeDuringPressureRight: " + accumulatedTimeDuringPressureRight);
                accumulatedTimeDuringPressureRight += timeDuringPressureRight;
                timeDuringPressureRight = 0f;
            }
        }

        // Reset Times for each foot when we lift them from the ground
        if (!isLeftFootGrounded)
        {
            if (printSteps)
                Debug.Log("3 - [INFO] LEFT Foot NOT grounded - Reset Time to 0");

            // TEST
            timeDuringPressureLeft = 0f;
            accumulatedTimeDuringPressureLeft += timeDuringPressureLeft;

            timePassedLeft = 0f;
            RunningCoroutineDeformationLeft = true;
        }
        
        if (!isRightFootGrounded)
        {
            if (printSteps)
                Debug.Log("3 - [INFO] RIGHT Foot NOT grounded - Reset Time to 0");

            // TEST
            timeDuringPressureRight = 0f;
            //Debug.Log("[RIGHT NOT GROUNDED] Summing timeDuringPressureRight " + timeDuringPressureRight + ", total accumulatedTimeDuringPressureRight: " + accumulatedTimeDuringPressureRight);            
            accumulatedTimeDuringPressureRight += timeDuringPressureRight;

            timePassedRight = 0f;
            RunningCoroutineDeformationRight = true;
        }

        #region Old Code

        /*
        // B. Provisional: We reset the time passed everytime when we lift the feet
        // Not very accurate, it would be better to create a time variable per feet and pass it though the method
        if ((!isLeftFootGrounded || !isRightFootGrounded) && isMoving)
        {
            Debug.Log("(1) 4 - [INFO] Foot NOT grounded - Reset Time to 0"); // TODO: Stop coroutine?
            timePassed = 0f;
        }

        // C. Provisional: When is still (once) - Stopping when reaching the deformation required was not giving very good results
        if (!isMoving && (!isLeftFootGrounded || !isRightFootGrounded) && provCounter <= 3)
        {
            Debug.Log("(2) 4 - [INFO] Foot NOT grounded - Reset Time to 0"); // TODO: Stop coroutine?
            timePassed = 0f;
            provCounter += 1;
        }

        // D. Provisional: Each time I change motion, resets the time
        isMoving = _anim.GetBool("isMoving"); // Changed by isWalking
        if (isMoving != oldIsMoving)
        {
            Debug.Log("(3) 4 - [INFO] Foot NOT grounded - Reset Time to 0"); // TODO: Stop coroutine?
            timePassed = 0f;
            oldIsMoving = isMoving;
            provCounter = 0;
        }
        */

        #endregion
    }

    #endregion

    #region Forces

    private void CheckOriginLeft(bool leftHeel, bool leftToe)
    {
        // Split in two cases
        //if (LeftHeelHeight < LeftToeHeight)
        //    OriginLeft = feetPlacement.groundCheckerLeftFootHeel.position;
        //else
        //    OriginLeft = feetPlacement.groundCheckerLeftFootToe.position;

        if (!leftHeel || !leftToe)
        {
            if (LeftHeelHeight < LeftToeHeight)
                OriginLeft = feetPlacement.groundCheckerLeftFootHeel.position;
            else if (LeftHeelHeight > LeftToeHeight)
                OriginLeft = feetPlacement.groundCheckerLeftFootToe.position;
        }
        else if (leftHeel && leftToe)
            OriginLeft = leftFootRB.position + offsetCenterFoot;
    }
    
    private void CheckOriginRight(bool rightHeel, bool rightToe)
    {
        // Split in two cases
        //if (RightHeelHeight < RightToeHeight)
        //    OriginRight = feetPlacement.groundCheckerRightFootHeel.position;
        //else
        //    OriginRight = feetPlacement.groundCheckerRightFootToe.position;

        if (!rightHeel || !rightToe)
        {
            if (RightHeelHeight < RightToeHeight)
                OriginRight = feetPlacement.groundCheckerRightFootHeel.position;
            else if (RightHeelHeight > RightToeHeight)
                OriginRight = feetPlacement.groundCheckerRightFootToe.position;
        }
        else if (rightHeel && rightToe)
            OriginRight = rightFootRB.position + offsetCenterFoot;
    }

    private void EstimateWeightForces()
    {
        if (!isRightFootGrounded && !isLeftFootGrounded)
        {
            weightInLeftFoot = 0f;
            weightInRightFoot = 0f;
        }

        // Weight Forces
        weightForce = mass * (Physics.gravity);
        weightForceLeft = weightForce * (weightInLeftFoot);
        weightForceRight = weightForce * (weightInRightFoot);

        // Components
        weightForceLeftLocalVertical = Vector3.Project(weightForceLeft, -slopeNormal); // Not much different wrt ground-checker normal
        weightForceRightLocalVertical = Vector3.Project(weightForceRight, -slopeNormal);
        var crossA = Vector3.Cross(Vector3.down, slopeNormal);
        var crossB = Vector3.Cross(slopeNormal, crossA); // Unit vector
        weightForceLeftLocalDownward = Vector3.Project(weightForceLeft, crossB);
        weightForceRightLocalDownward = Vector3.Project(weightForceRight, crossB);
        weightForceLeftLocalHorizontal = Vector3.Project(weightForceLeft, crossA);
        weightForceRightLocalHorizontal = Vector3.Project(weightForceRight, crossA);

        //               Extra for plotting              //
        // ============================================= //
        //weightForceLeftX = weightForceLeft.x;
        //weightForceLeftY = weightForceLeft.y;
        //weightForceLeftZ = weightForceLeft.z;

        //weightForceRightX = weightForceRight.x;
        //weightForceRightY = weightForceRight.y;
        //weightForceRightZ = weightForceRight.z;
        // ============================================= //

        // Weight Force is already zero if the foot is not grounded - however, we draw only when foot is grounded
        if (drawWeightForces)
        {
            DrawForce.ForDebug3D(OriginLeft, weightForceLeft, Color.blue, 0.0025f);
            DrawForce.ForDebug3D(OriginRight, weightForceRight, Color.blue, 0.0025f);
        }

        if (drawWeightForcesComponents)
        {            
            DrawForce.ForDebug3D(OriginLeft, weightForceLeftLocalVertical, Color.green, 0.0025f);
            DrawForce.ForDebug3D(OriginLeft, weightForceLeftLocalDownward, Color.red, 0.0025f);
            DrawForce.ForDebug3D(OriginLeft, weightForceLeftLocalHorizontal, Color.blue, 0.0025f);

            DrawForce.ForDebug3D(OriginRight, weightForceRightLocalVertical, Color.green, 0.0025f);
            DrawForce.ForDebug3D(OriginRight, weightForceRightLocalDownward, Color.red, 0.0025f);
            DrawForce.ForDebug3D(OriginRight, weightForceRightLocalHorizontal, Color.blue, 0.0025f);
        }
    }
    
    private void EstimateMomentumForces()
    {
        // Only velocities going downward //
        // ================================ //

        // Impulse per foot - Linear Momentum change (final velocity for the feet is 0)
        feetImpulseLeft = mass * weightInLeftFoot * (Vector3.zero - feetSpeedLeftDown);
        feetImpulseRight = mass * weightInRightFoot * (Vector3.zero - feetSpeedRightDown);

        // Momentum force exerted by ground to each foot - Calculated using Impulse and Contact Time
        // Positive (upward) if foot lands (negative velocity)
        // Negative (downward) if foot rises (positive velocity)
        momentumForceLeft = feetImpulseLeft / contactTime;
        momentumForceRight = feetImpulseRight / contactTime;
        momentumForce = momentumForceLeft + momentumForceRight;

        // Components
        momentumForceLeftLocalVertical = Vector3.Project(momentumForceLeft, -slopeNormal);
        momentumForceRightLocalVertical = Vector3.Project(momentumForceRight, -slopeNormal);
        var crossA = Vector3.Cross(Vector3.down, slopeNormal);
        var crossB = Vector3.Cross(slopeNormal, crossA); // Unit vector
        momentumForceLeftLocalDownward = Vector3.Project(momentumForceLeft, crossB);
        momentumForceRightLocalDownward = Vector3.Project(momentumForceRight, crossB);
        momentumForceLeftLocalHorizontal = Vector3.Project(momentumForceLeft, crossA);
        momentumForceRightLocalHorizontal = Vector3.Project(momentumForceRight, crossA);

        //  Extra for plotting (only positive values - when feet hit the ground) //
        // ===================================================================== //
        // When foot is landing
        //if (momentumForceLeft.y > 0f)
        //{
        //    momentumForceLeftX = -momentumForceLeft.x;
        //    momentumForceLeftY = -momentumForceLeft.y;
        //    momentumForceLeftZ = -momentumForceLeft.z;
        //}

        //if (momentumForceRight.y > 0f)
        //{
        //    momentumForceRightX = -momentumForceRight.x;
        //    momentumForceRightY = -momentumForceRight.y;
        //    momentumForceRightZ = -momentumForceRight.z;
        //}
        // ===================================================================== //

        // Momentum Forces are created when we hit the ground. To make it clearer, only in contact
        if (drawMomentumForces)
        {
            DrawForce.ForDebug3D(OriginLeft, -momentumForceLeft, Color.red, 0.0025f);
            DrawForce.ForDebug3D(OriginRight, -momentumForceRight, Color.red, 0.0025f);
        }

        if (drawMomentumForcesComponents)
        {
            DrawForce.ForDebug3D(OriginLeft, -momentumForceLeftLocalVertical, Color.green, 0.0025f);
            DrawForce.ForDebug3D(OriginLeft, -momentumForceLeftLocalDownward, Color.red, 0.0025f);
            DrawForce.ForDebug3D(OriginLeft, -momentumForceLeftLocalHorizontal, Color.blue, 0.0025f);

            DrawForce.ForDebug3D(OriginRight, -momentumForceRightLocalVertical, Color.green, 0.0025f);
            DrawForce.ForDebug3D(OriginRight, -momentumForceRightLocalDownward, Color.red, 0.0025f);
            DrawForce.ForDebug3D(OriginRight, -momentumForceRightLocalHorizontal, Color.blue, 0.0025f);
        }
    }

    private void EstimateGRForces()
    {
        // GRF (Ground Reaction Force) that the ground exerts to each foot - using Force Decomposition we get each of them
        totalGRForceLeft = momentumForceLeft - weightForceLeft;
        totalGRForceRight = momentumForceRight - weightForceRight;

        // Left Foot
        totalGRForceLeftLocalVertical = momentumForceLeftLocalVertical - weightForceLeftLocalVertical;
        totalGRForceLeftLocalDownward = momentumForceLeftLocalDownward - weightForceLeftLocalDownward;
        totalGRForceLeftLocalHorizontal = momentumForceLeftLocalHorizontal - weightForceLeftLocalHorizontal;

        // Right Foot
        totalGRForceRightLocalVertical = momentumForceRightLocalVertical - weightForceRightLocalVertical;
        totalGRForceRightLocalDownward = momentumForceRightLocalDownward - weightForceRightLocalDownward;
        totalGRForceRightLocalHorizontal = momentumForceRightLocalHorizontal - weightForceRightLocalHorizontal;

        totalGRForce = totalGRForceLeft + totalGRForceRight;

        //               Extra for plotting              //
        // ============================================= //
        //totalGRForceLeftYFloat = totalGRForceLeft.y;
        //totalGRForceRightYFloat = totalGRForceRight.y;
        //totalGRForceYFloat = totalGRForce.y;

        //totalGRForceLeftX = totalGRForceLeft.x;
        //totalGRForceLeftY = totalGRForceLeft.y;
        //totalGRForceLeftZ = totalGRForceLeft.z;

        //totalGRForceRightX = totalGRForceRight.x;
        //totalGRForceRightY = totalGRForceRight.y;
        //totalGRForceRightZ = totalGRForceRight.z;
        // ============================================= //

        // Color for GR Forces
        Color darkGreen = new Color(0.074f, 0.635f, 0.062f, 1f);

        if (drawGRForces)
        {
            DrawForce.ForDebug3D(OriginLeft, totalGRForceLeft, darkGreen, 0.0025f);
            DrawForce.ForDebug3D(OriginRight, totalGRForceRight, darkGreen, 0.0025f);
        }

        if (drawGRForcesComponents)
        {
            DrawForce.ForDebug3D(OriginLeft, totalGRForceLeftLocalVertical, Color.green, 0.0025f);
            DrawForce.ForDebug3D(OriginLeft, totalGRForceLeftLocalDownward, Color.red, 0.0025f);
            DrawForce.ForDebug3D(OriginLeft, totalGRForceLeftLocalHorizontal, Color.blue, 0.0025f);

            DrawForce.ForDebug3D(OriginRight, totalGRForceRightLocalVertical, Color.green, 0.0025f);
            DrawForce.ForDebug3D(OriginRight, totalGRForceRightLocalDownward, Color.red, 0.0025f);
            DrawForce.ForDebug3D(OriginRight, totalGRForceRightLocalHorizontal, Color.blue, 0.0025f);
        }
    }
    
    private void EstimateFeetForces()
    {
        // Reaction Force for the feet (3rd Newton Law)
        totalForceLeftFoot = -totalGRForceLeft;
        totalForceRightFoot = -totalGRForceRight;

        // Left Foot
        totalForceLeftLocalVertical = -totalGRForceLeftLocalVertical;
        totalForceLeftLocalDownward = -totalGRForceLeftLocalDownward;
        totalForceLeftLocalHorizontal = -totalGRForceLeftLocalHorizontal;
        totalForceLeftLocalTerrainProjection = Vector3.ProjectOnPlane(totalForceLeftFoot, slopeInitialNormal);

        // Right Foot
        totalForceRightLocalVertical = -totalGRForceRightLocalVertical;
        totalForceRightLocalDownward = -totalGRForceRightLocalDownward;
        totalForceRightLocalHorizontal = -totalGRForceRightLocalHorizontal;
        totalForceRightLocalTerrainProjection = Vector3.ProjectOnPlane(totalForceRightFoot, slopeInitialNormal);

        totalForceFoot = totalForceLeftFoot + totalForceRightFoot;

        //               Extra for plotting              //
        // ============================================= //
        //totalForceLeftX = totalForceLeftFoot.x;
        //totalForceLeftY = totalForceLeftFoot.y;
        //totalForceLeftZ = totalForceLeftFoot.z;

        //totalForceRightX = totalForceRightFoot.x;
        //totalForceRightY = totalForceRightFoot.y;
        //totalForceRightZ = totalForceRightFoot.z;
        // ============================================= //

        // Feet Forces are created when we hit the ground (that is, when the Y-component of the Momentum Force is positive)
        // Only when the feet rise up, Feet Forces do not exist. The muscle is the responsable to lift the foot up
        // Also, the foot must be grounded to have a Feet Force actuating onto the ground
        if (drawFeetForces)
        {
            DrawForce.ForDebug3D(OriginLeft, totalForceLeftFoot, Color.black, 0.0025f);
            DrawForce.ForDebug3D(OriginRight, totalForceRightFoot, Color.black, 0.0025f);
        }

        if (drawFeetForcesComponents)
        {
            DrawForce.ForDebug3D(OriginLeft, totalForceLeftLocalVertical, Color.green, 0.0025f);
            DrawForce.ForDebug3D(OriginLeft, totalForceLeftLocalDownward, Color.red, 0.0025f);
            DrawForce.ForDebug3D(OriginLeft, totalForceLeftLocalHorizontal, Color.blue, 0.0025f);


            DrawForce.ForDebug3D(OriginRight, totalForceRightLocalVertical, Color.green, 0.0025f);
            DrawForce.ForDebug3D(OriginRight, totalForceRightLocalDownward, Color.red, 0.0025f);
            DrawForce.ForDebug3D(OriginRight, totalForceRightLocalHorizontal, Color.blue, 0.0025f);
        }

        if (drawFeetForcesProjections)
        {
            DrawForce.ForDebug3D(OriginLeft, totalForceLeftLocalTerrainProjection, Color.magenta, 0.0025f); // Keep projection in "checker" 2D plane
            DrawForce.ForDebug3D(OriginRight, totalForceRightLocalTerrainProjection, Color.magenta, 0.0025f);
        }

        if (drawForceSystem)
        {
            DrawForce.ForDebug3D(OriginLeft, weightForceLeft, Color.blue, 0.0025f);
            DrawForce.ForDebug3D(OriginRight, weightForceRight, Color.blue, 0.0025f);

            DrawForce.ForDebug3D(OriginLeft, -momentumForceLeft, Color.red, 0.0025f);
            DrawForce.ForDebug3D(OriginRight, -momentumForceRight, Color.red, 0.0025f);

            //DrawForce.ForDebug3D(OriginLeft, totalGRForceLeftLocalVertical, Color.green, 0.0025f);
            //DrawForce.ForDebug3D(OriginRight, totalGRForceRightLocalVertical, Color.green, 0.0025f);

            DrawForce.ForDebug3D(OriginLeft, totalForceLeftFoot, Color.black, 0.0025f);
            DrawForce.ForDebug3D(OriginRight, totalForceRightFoot, Color.black, 0.0025f);
        }
    }

    #endregion

    #region Prefabs

    // Methods use to define new materials
    public void DefineSnow()
    {
        _brushPhysicalFootprint.YoungModulus = youngModulusSnow;
        contactTime = timeSnow;
        _brushPhysicalFootprint.FilterIterations = filterIterationsSnow;
        _brushPhysicalFootprint.PoissonRatio = poissonRatioSnow;
        _brushPhysicalFootprint.ActivateBump = bumpSnow;
    }

    public void DefineDrySand()
    {
        _brushPhysicalFootprint.YoungModulus = youngModulusDrySand;
        contactTime = timeDrySand;
        _brushPhysicalFootprint.FilterIterations = filterIterationsSand;
        _brushPhysicalFootprint.PoissonRatio = poissonRatioSand;
        _brushPhysicalFootprint.ActivateBump = bumpSand;
    }

    public void DefineMud()
    {
        _brushPhysicalFootprint.YoungModulus = youngModulusMud;
        contactTime = timeMud;
        _brushPhysicalFootprint.FilterIterations = filterIterationsMud;
        _brushPhysicalFootprint.PoissonRatio = poissonRatioMud;
        _brushPhysicalFootprint.ActivateBump = bumpMud;
    }

    public void DefineSoil()
    {
        _brushPhysicalFootprint.YoungModulus = youngModulusSoil;
        contactTime = timeSoil;
        _brushPhysicalFootprint.FilterIterations = filterIterationsSoil;
        _brushPhysicalFootprint.PoissonRatio = poissonRatioSoil;
        _brushPhysicalFootprint.ActivateBump = bumpSoil;
    }

    public void DefineDefault()
    {
        _brushPhysicalFootprint.YoungModulus = 750000;
        _brushPhysicalFootprint.FilterIterations = 0;
        _brushPhysicalFootprint.PoissonRatio = 0f;
        _brushPhysicalFootprint.ActivateBump = false;
    }

    // ========================= //
    // Define here your material // 

    //public void DefineExample()
    //{
    //    brushPhysicalFootprint.YoungM = youngModulusExample;
    //    brushPhysicalFootprint.FilterIte = timeExamlpe;
    //    brushPhysicalFootprint.PoissonRatio = filterIterationsExample;
    //    brushPhysicalFootprint.ActivateBump = bumpExample;
    //}

    // ========================= //

    #endregion

    #region Getters

    public Vector3 Get3(int x, int z)
    {
        return new Vector3(x, Get(x, z), z);
    }
    public Vector3 Get3(float x, float z)
    {
        return new Vector3(x, Get(x, z), z);
    }
    public Vector3 GetInterp3(float x, float z)
    {
        return new Vector3(x, GetInterp(x, z), z);
    }

    // Given one node of the heightmap, get the height
    public float Get(int x, int z)
    {
        x = (x + _heightmapWidth) % _heightmapWidth;
        z = (z + _heightmapHeight) % _heightmapHeight;
        return _heightmapData[z, x] * _terrainData.heightmapScale.y;
    }
    public float Get(float x, float z)
    {
        return Get((int)x, (int)z);
    }

    // Get entire array with heightmap without being scaled
    public float[,] GetHeightmap()
    {
        // IMPORTANT: When getting a value, must be multiplied by terrain_data.heightmapScale.y!
        return _heightmapData;
    }

    // Given one node of the heightmap (constant at start), get the height
    public float GetConstant(int x, int z)
    {
        x = (x + _heightmapWidth) % _heightmapWidth;
        z = (z + _heightmapHeight) % _heightmapHeight;
        return _heightmapDataConstant[z, x] * _terrainData.heightmapScale.y;
    }
    public float GetConstant(float x, float z)
    {
        return GetConstant((int)x, (int)z);
    }

    // Get entire array with initial constant heightmap without being scaled
    public float[,] GetConstantHeightmap()
    {
        // IMPORTANT: When getting a value, must be multiplied by terrain_data.heightmapScale.y!
        return _heightmapDataConstant;
    }
    // Given one node of the heightmap, get the height (post-filter version)
    public float GetFiltered(int x, int z)
    {
        x = (x + _heightmapWidth) % _heightmapWidth;
        z = (z + _heightmapHeight) % _heightmapHeight;
        return _heightmapDataFiltered[z, x] * _terrainData.heightmapScale.y;
    }
    public float GetFiltered(float x, float z)
    {
        return GetFiltered((int)x, (int)z);
    }

    // Get entire array with post-filtered heightmap
    public float[,] GetFilteredHeightmap()
    {
        // IMPORTANT: When getting a value, must be multiplied by terrain_data.heightmapScale.y!
        return _heightmapDataFiltered;
    }

    public float GetInterp(float x, float z)
    {
        return _terrainData.GetInterpolatedHeight(x / _heightmapWidth,
                                                  z / _heightmapHeight);
    }
    public float GetSteepness(float x, float z)
    {
        return _terrainData.GetSteepness(x / _heightmapWidth,
                                         z / _heightmapHeight);
    }
    public Vector3 GetNormal(float x, float z)
    {
        return _terrainData.GetInterpolatedNormal(x / _heightmapWidth,
                                                  z / _heightmapHeight);
    }

    #endregion

    #region Setters

    // Given one node of the heightmap, set the height
    public void Set(int x, int z, float val)
    {
        x = (x + _heightmapWidth) % _heightmapWidth;
        z = (z + _heightmapHeight) % _heightmapHeight;
        _heightmapData[z, x] = val / _terrainData.heightmapScale.y;
    }

    public void Set(float x, float z, float val)
    {
        Set((int)x, (int)z, val);
    }

    #endregion

    #region Terrain Methods

    // Get dimensions of the heightmap grid
    public Vector3 GridSize()
    {
        return new Vector3(_heightmapWidth, 0.0f, _heightmapHeight);
    }

    // Get real dimensions of the terrain (World Space)
    public Vector3 TerrainSize()
    {
        return _terrainSize;
    }

    // Get area of a cell
    public float CellSize()
    {
        return _cellSize;
    }
    // Get length X of a cell
    public float LengthCellX()
    {
        return _lengthCellX;
    }
    // Get length Z of a cell
    public float LengthCellZ()
    {
        return _lengthCellZ;
    }

    // Get terrain data
    public TerrainData GetTerrainData()
    {
        return _terrainData;
    }

    // Convert from Grid Space to World Space
    public Vector3 Grid2World(Vector3 grid)
    {
        return new Vector3(grid.x * _terrainData.heightmapScale.x,
                           grid.y,
                           grid.z * _terrainData.heightmapScale.z);
    }

    public Vector3 Grid2World(float x, float y, float z)
    {
        return Grid2World(new Vector3(x, y, z));
    }

    public Vector3 Grid2World(float x, float z)
    {
        return Grid2World(x, 0.0f, z);
    }

    // Convert from World Space to Grid Space
    public Vector3 World2Grid(Vector3 grid)
    {
        return new Vector3(grid.x / _terrainData.heightmapScale.x,
                           grid.y,
                           grid.z / _terrainData.heightmapScale.z);
    }

    public Vector3 World2Grid(float x, float y, float z)
    {
        return World2Grid(new Vector3(x, y, z));
    }

    public Vector3 World2Grid(float x, float z)
    {
        return World2Grid(x, 0.0f, z);
    }

    // Reset to flat terrain
    public void Reset()
    {
        for (int z = 0; z < _heightmapHeight; z++)
        {
            for (int x = 0; x < _heightmapWidth; x++)
            {
                _heightmapData[z, x] = 0;
            }
        }

        Save();
    }

    // Smooth terrain
    public void AverageSmooth()
    {
        for (int z = 10; z < _heightmapHeight - 10; z++)
        {
            for (int x = 10; x < _heightmapWidth - 10; x++)
            {
                float n = 2.0f * 2 + 1.0f;
                float sum = 0;
                for (int szi = -2; szi <= 2; szi++)
                {
                    for (int sxi = -2; sxi <= 2; sxi++)
                    {
                        sum += _heightmapData[z + szi, x + sxi];
                    }
                }

                _heightmapData[z, x] = sum / (n * n);
            }
        }

        Save();
    }

    // Calculate Kernel
    public static float[,] CalculateKernel(int length, float sigma)
    {
        float[,] Kernel = new float[length, length];
        float sumTotal = 0f;

        int kernelRadius = length / 2;
        double distance = 0f;

        float calculatedEuler = 1.0f / (2.0f * (float)Math.PI * sigma * sigma);

        for (int idY = -kernelRadius; idY <= kernelRadius; idY++)
        {
            for (int idX = -kernelRadius; idX <= kernelRadius; idX++)
            {
                distance = ((idX * idX) + (idY * idY)) / (2 * (sigma * sigma));

                Kernel[idY + kernelRadius, idX + kernelRadius] = calculatedEuler * (float)Math.Exp(-distance);

                sumTotal += Kernel[idY + kernelRadius, idX + kernelRadius];
            }
        }

        for (int y = 0; y < length; y++)
        {
            for (int x = 0; x < length; x++)
            {
                Kernel[y, x] = Kernel[y, x] * (1.0f / sumTotal);
            }
        }

        return Kernel;
    }

    // Gaussian Filter (Custom Kernel)
    public void GaussianBlurCustom()
    {
        float[,] kernel = CalculateKernel(3, 1f);

        for (int z = 10; z < _heightmapHeight - 10; z++)
        {
            for (int x = 10; x < _heightmapWidth - 10; x++)
            {

                _heightmapData[z, x] =
                    kernel[0, 0] * _heightmapData[z - 1, x - 1]
                    + kernel[0, 1] * _heightmapData[z - 1, x]
                    + kernel[0, 2] * _heightmapData[z - 1, x + 1]
                    + kernel[1, 0] * _heightmapData[z, x - 1]
                    + kernel[1, 1] * _heightmapData[z, x]
                    + kernel[1, 2] * _heightmapData[z, x + 1]
                    + kernel[2, 0] * _heightmapData[z + 1, x - 1]
                    + kernel[2, 1] * _heightmapData[z + 1, x]
                    + kernel[2, 2] * _heightmapData[z + 1, x + 1];
            }
        }

        Save();
    }

    // Gaussian Blur 3x3
    public void GaussianBlur3()
    {
        for (int z = 10; z < _heightmapHeight - 10; z++)
        {
            for (int x = 10; x < _heightmapWidth - 10; x++)
            {

                _heightmapData[z, x] =
                    _heightmapData[z - 1, x - 1]
                    + 2 * _heightmapData[z - 1, x]
                    + 1 * _heightmapData[z - 1, x + 1]
                    + 2 * _heightmapData[z, x - 1]
                    + 4 * _heightmapData[z, x]
                    + 2 * _heightmapData[z, x + 1]
                    + 1 * _heightmapData[z + 1, x - 1]
                    + 2 * _heightmapData[z + 1, x]
                    + 1 * _heightmapData[z + 1, x + 1];

                _heightmapData[z, x] *= 1.0f / 16.0f;

            }
        }

        Save();

    }

    // Gaussian Blur 5x5
    public void GaussianBlur5()
    {
        for (int z = 10; z < _heightmapHeight - 10; z++)
        {
            for (int x = 10; x < _heightmapWidth - 10; x++)
            {

                _heightmapData[z, x] =
                    _heightmapData[z - 2, x - 2]
                    + 4 * _heightmapData[z - 2, x - 1]
                    + 6 * _heightmapData[z - 2, x]
                    + _heightmapData[z - 2, x + 2]
                    + 4 * _heightmapData[z - 2, x + 1]
                    + 4 * _heightmapData[z - 1, x + 2]
                    + 16 * _heightmapData[z - 1, x + 1]
                    + 4 * _heightmapData[z - 1, x - 2]
                    + 16 * _heightmapData[z - 1, x - 1]
                    + 24 * _heightmapData[z - 1, x]
                    + 6 * _heightmapData[z, x - 2]
                    + 24 * _heightmapData[z, x - 1]
                    + 6 * _heightmapData[z, x + 2]
                    + 24 * _heightmapData[z, x + 1]
                    + 36 * _heightmapData[z, x]
                    + _heightmapData[z + 2, x - 2]
                    + 4 * _heightmapData[z + 2, x - 1]
                    + 6 * _heightmapData[z + 2, x]
                    + _heightmapData[z + 2, x + 2]
                    + 4 * _heightmapData[z + 2, x + 1]
                    + 4 * _heightmapData[z + 1, x + 2]
                    + 16 * _heightmapData[z + 1, x + 1]
                    + 4 * _heightmapData[z + 1, x - 2]
                    + 16 * _heightmapData[z + 1, x - 1]
                    + 24 * _heightmapData[z + 1, x];

                _heightmapData[z, x] *= 1.0f / 256.0f;

            }
        }

        Save();

    }

    // Register changes made to the terrain
    public void Save()
    {
        _terrainData.SetHeights(0, 0, _heightmapData);
    }

    // Get and set active brushes
    public void SetFootprintBrush(BrushPhysicalFootprint brush)
    {
        Debug.Log("[INFO] Setting brush to " + brush);
        _brushPhysicalFootprint = brush;
    }
    public BrushPhysicalFootprint GetFootprintBrush()
    {
        return _brushPhysicalFootprint;
    }

    #endregion
}