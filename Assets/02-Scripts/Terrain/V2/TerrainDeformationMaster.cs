/****************************************************
 * File: TerrainDeformationMaster.cs
   * Author: Eduardo Alvarado
   * Email: alvaradopinero.eduardo@gmail.com
   * Date: 12/01/2024
   * Project: Foot2Trail
   * Last update: 12/01/2024
*****************************************************/

using System;
using UnityEngine;
using UnityEngine.UI;

using PositionBasedDynamics;
using static UnityEditor.ShaderData;
using System.Security.Cryptography.X509Certificates;

public enum inputDeformation
{
    useUI,
    useTerrainPrefabs,
    useManualValuesWithVegetation
};

public class TerrainDeformationMaster : MonoBehaviour
{
    #region Instance Fields

    [Header("Exporting Maps and Others - SET UP")]
    public bool exportMaps;
    public bool printSteps;

    [Header("Bipedal - SET UP")]
    [Tooltip("Your character GameObject")]
    public GameObject myBipedalCharacter;
    [Tooltip("IK Script")]
    public IKFootAdaptation feetPlacement;
    [Tooltip("Collider attached to Left Foot to leave footprints")]
    public Collider leftFootCollider;
    [Tooltip("Collider attached to Right Foot to leave footprints")]
    public Collider rightFootCollider;
    [Tooltip("RB attached to Left Foot used for velocity estimation")]
    public Rigidbody leftFootRB;
    [Tooltip("RB attached to Right Foot for velocity estimation")]
    public Rigidbody rightFootRB;
    [Tooltip("Offset applied when both points are in contact")]
    public Vector3 offsetCenterFoot;

    [Header("Bipedal - Debug")]
    public float mass;
    public bool printFeetPositions = false;
    public float rightWeightDistribution;
    public float leftWeightDistribution;
    public float weightInLeftFoot;
    public float weightInRightFoot;
    [Space(5)]
    public float heightIKLeft;
    public float heightIKRight;
    [Space(5)]
    public bool isLeftFootGrounded;
    public bool wasLeftFootGrounded;
    public bool isLeftFootHeelGrounded;
    public bool isLeftFootToeGrounded;
    public bool isRightFootGrounded;
    public bool wasRightFootGrounded;
    public bool isRightFootHeelGrounded;
    public bool isRightFootToeGrounded;
    [Space(5)]
    public Vector3 centerGridLeftFoot;
    public Vector3 centerGridRightFoot;
    public Vector3 centerGridLeftFootHeight;
    public Vector3 centerGridRightFootHeight;

    // ---------------------- //

    [Header("Forces - Debug")]
    public bool drawForceSystem = false;
    public bool printFeetForces = false;

    [Header("Weight Forces - Info and Debug")]
    public bool drawWeightForces = false;
    public bool drawWeightForcesComponents = false;
    [Space(10)]
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

    [Header("Feet Velocities - Info and Debug")]
    public bool drawMMVelocities = false;
    [Space(5)]
    public Vector3 feetSpeedLeftDown = Vector3.zero;
    public Vector3 feetSpeedRightDown = Vector3.zero;
    [Space(5)]
    public Vector3 feetSpeedLeftUp = Vector3.zero;
    public Vector3 feetSpeedRightUp = Vector3.zero;

    [Header("Impulse and Momentum Forces - Info and Debug")]
    public bool drawMomentumForces = false;
    public bool drawMomentumForcesComponents = false;
    [Space(10)]
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

    [Header("GR Forces - Info and Debug")]
    public bool drawGRForces = false;
    public bool drawGRForcesComponents = false;
    [Space(10)]
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

    [Header("Feet Forces - Info and Debug")]
    public bool drawFeetForces = false;
    public bool drawFeetForcesComponents = false;
    public bool drawFeetForcesProjections = false;
    [Space(10)]
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

    // ---------------------- //

    [Header("Terrain - SET UP")]
    public inputDeformation deformationChoice;
    public LayerMask ground;
    public bool drawAngleNormal = false;
    public bool drawCOMProjection = false;

    [Header("Terrain Initial Normals/Angles - Debug")]
    public float cellSize;
    public Vector3 slopeNormal;
    public Vector3 slopeInitialNormal;
    public float slopeAngle;
    public float slopeInitialAngle;
    public Vector3[,] initialNormalMatrix;
    public float[,] initialGradientMatrix;

    [Header("Grid - Center Offset")]
    public Vector3 offsetGridLeft;
    public Vector3 offsetGridLeftLocal;
    public Vector3 offsetGridRight;
    public Vector3 offsetGridRightLocal;

    [Header("Terrain Deformation Times - Debug")]
    public float totalTime = 0f;
    [Tooltip("Time that the terrain requires to absorve the force from the hitting foot. More time results in a smaller require force. On the other hand, for less time, the terrain requires a larger force to stop the foot.")]
    public float contactTime = 0.1f;
    [Space(5)]
    public float timePassedLeft = 0f;
    public float timeDuringPressureLeft = 0f;
    public float accumulatedTimeDuringPressureLeft = 0f;
    public float timePassedRight = 0f;
    public float timeDuringPressureRight = 0f;
    public float accumulatedTimeDuringPressureRight = 0f;
    [Space(5)]
    public float timeOffset = 0f;

    [Header("Terrain Stabilization - SET UP")]
    public int maxStabilizations = 0;
    public int counterStabilizationLeft = 0;
    public int counterStabilizationRight = 0;

    [Header("Gaussian Filter")]
    public bool applyGaussianFilter = false;
    public int iterationsGaussianLeft = 0;
    public int iterationsGaussianRight = 0;
    public float smoothStrength = 1f;
    public int smoothRadius = 0;
    [Space(5)]
    public int counterGaussianFiltersLeft = 0;
    public int counterGaussianFiltersRight = 0;
    public int maxGaussianFilters = 1;
    [Space(5)]
    public Vector2 temporalPositionLeft;
    public Vector2 temporalPositionRight;

    [Header("Terrain Prefabs Settings - SET UP")]
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

    // Character
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

    // Forces
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

    // 2D/1D arrays to send to server
    public static float[,] HeightMapMatrix { get; set; }
    public static byte[] HeightMapBytes { get; set; }

    public static float[,] PressureMapMatrix { get; set; }
    public static byte[] PressureMapBytes { get; set; }

    public static double[,] YoungMapMatrix { get; set; }
    public static byte[] YoungMapBytes { get; set; }

    // Stabilization
    public bool RunningCoroutineDeformationLeft { get; set; }
    public bool RunningCoroutineStabilizationLeft { get; set; }
    public bool RunningCoroutineDeformationRight { get; set; }
    public bool RunningCoroutineStabilizationRight { get; set; }

    public TerrainData MyTerrainData
    {
        get { return _terrainData; }
        set { _terrainData = value; }

    }

    public int HeightmapWidth
    {
        get { return _heightmapWidth; }
        set { _heightmapWidth = value; }
    }

    public int HeightmapHeight
    {
        get { return _heightmapHeight; }
        set { _heightmapHeight = value; }
    }

    #endregion

    #region Read-only & Static Fields

    // Character
    private Animator _anim;
    private Rigidbody _rb;

    // Heights
    private float _leftHeelHeight;
    private float _leftToeHeight;
    private float _rightHeelHeight;
    private float _rightToeHeight;

    // Origins
    private Vector3 _originLeft;
    private Vector3 _originRight;

    // COM
    private Vector3 _COMPosition;
    private Vector3 _COMPositionProjected;

    // Terrain Properties
    private Terrain _terrain;
    private Collider _terrainCollider;
    private TerrainData _terrainData;
    private Vector3 _terrainSize;
    private float _lengthCellX;
    private float _lengthCellZ;
    private int _heightmapWidth;
    private int _heightmapHeight;
    private float[,] _heightmapData;
    private float[,] _heightmapDataConstant;
    private float[,] _heightmapDataFiltered;

    // Types of brushes
    private BrushFootprint _brushPhysicalFootprint;

    #endregion

    #region Unity Methods


    // Start is called before the first frame update
    void Start()
    {
        // Extract terrain information
        if (!_terrain)
        {
            _terrain = myBipedalCharacter.GetComponent<DetectTerrain>().CurrentTerrain;

            Debug.Log("[INFO] First terrain: " + _terrain.name);
        }

        ExtractTerrainParameters();
        ExtractCharacterParameters();
        ExtractInitialTerrainConditions();

        // Heightmap Map
        HeightMapMatrix = new float[_terrainData.heightmapResolution, _terrainData.heightmapResolution];
        HeightMapBytes = new byte[HeightMapMatrix.Length * sizeof(float)];

        // Pressure Map
        PressureMapMatrix = new float[_terrainData.heightmapResolution, _terrainData.heightmapResolution];
        PressureMapBytes = new byte[PressureMapMatrix.Length * sizeof(float)];

        // Young's Modulus Map
        YoungMapMatrix = new double[_terrainData.heightmapResolution, _terrainData.heightmapResolution];
        YoungMapBytes = new byte[YoungMapMatrix.Length * sizeof(double)];

        if (exportMaps)
        {
            InvokeRepeating("ExportHeightMap", 2.0f, 1f);
            InvokeRepeating("ExportYoungMap", 2.0f, 1f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Convert offsetGridLeft to local frame with respect to character
        offsetGridLeftLocal = myBipedalCharacter.transform.TransformDirection(offsetGridLeft);
        offsetGridRightLocal = myBipedalCharacter.transform.TransformDirection(offsetGridRight);

        // Update time
        totalTime += Time.deltaTime;

        // 0. Update mass if it changes during the simulation
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

        // 6. Extra Debug
        DebugForceModel();

        // 7. Apply procedural brush to deform the terrain based on the force model
        ApplyDeformation(); // -> HEAVY RUNTIME!
    }

    #endregion

    #region Force Methods

    private void CheckOriginLeft(bool leftHeel, bool leftToe)
    {
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
            Debug.Log("[INFO] Left Foot Coords (Grid): " + centerGridLeftFoot.ToString());
            Debug.Log("[INFO] Right Foot Coords (World): " + feetPlacement.RightFootIKPosition.ToString());
            Debug.Log("[INFO] Right Foot Coords (Grid): " + centerGridRightFoot.ToString());
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

    #region Force Estimation Methods

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

    #region Terrain Parameters

    /// <summary>
    /// Extract Data from Current Terrain
    /// </summary>
    private void ExtractTerrainParameters()
    {
        // Retrieve components from _terrain
        _terrainCollider = _terrain.GetComponent<Collider>();
        _terrainData = _terrain.terrainData;
        MyTerrainData = _terrainData;

        // Extract data from _terrainData
        _terrainSize = _terrainData.size;
        _heightmapWidth = _terrainData.heightmapResolution;
        _heightmapHeight = _terrainData.heightmapResolution;
        _heightmapData = _terrainData.GetHeights(0, 0, _heightmapWidth, _heightmapHeight);
        _heightmapDataConstant = _terrainData.GetHeights(0, 0, _heightmapWidth, _heightmapHeight);
        _heightmapDataFiltered = _terrainData.GetHeights(0, 0, _heightmapWidth, _heightmapHeight);
        _brushPhysicalFootprint = null;

        // Cells
        _lengthCellX = _terrainSize.x / (_heightmapWidth - 1);
        _lengthCellZ = _terrainSize.z / (_heightmapHeight - 1);
        cellSize = _lengthCellX * _lengthCellZ;
    }

    /// <summary>
    /// Extract Initial Conditions of Terrain
    /// </summary>
    private void ExtractInitialTerrainConditions()
    {
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
    }

    #endregion

    #region Character Parameters

    /// <summary>
    /// Extract main properties of character
    /// </summary>
    private void ExtractCharacterParameters()
    {
        // Retrieve components and attributes from character
        mass = myBipedalCharacter.GetComponent<Rigidbody>().mass;
        _anim = myBipedalCharacter.GetComponent<Animator>();
        _rb = myBipedalCharacter.GetComponent<Rigidbody>();
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
        // For slopes, we check the sign of the dot product between the velocity and the normal of the terrain
        // Three cases: 1) Foot Heel 2) Foot Toe

        #region LeftFoot

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

        #endregion

        #region RightFoot

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
                DrawForce.ForDebug3DVelocity(_anim.GetBoneTransform(HumanBodyBones.LeftFoot).position, feetSpeedLeftUp, Color.cyan, 1f);
            }
            else
            {
                DrawForce.ForDebug3DVelocity(_anim.GetBoneTransform(HumanBodyBones.LeftToes).position, feetSpeedLeftDown, Color.blue, 1f);
                DrawForce.ForDebug3DVelocity(_anim.GetBoneTransform(HumanBodyBones.LeftToes).position, feetSpeedLeftUp, Color.blue, 1f);
            }

            if (RightHeelHeight < RightToeHeight)
            {
                DrawForce.ForDebug3DVelocity(_anim.GetBoneTransform(HumanBodyBones.RightFoot).position, feetSpeedRightDown, Color.cyan, 1f);
                DrawForce.ForDebug3DVelocity(_anim.GetBoneTransform(HumanBodyBones.RightFoot).position, feetSpeedRightUp, Color.cyan, 1f);
            }
            else
            {
                DrawForce.ForDebug3DVelocity(_anim.GetBoneTransform(HumanBodyBones.RightToes).position, feetSpeedRightDown, Color.blue, 1f);
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
        centerGridLeftFoot = World2Grid(feetPlacement.LeftFootIKPosition.x + offsetGridLeftLocal.x, feetPlacement.LeftFootIKPosition.z + offsetGridLeftLocal.z);
        centerGridRightFoot = World2Grid(feetPlacement.RightFootIKPosition.x + offsetGridRightLocal.x, feetPlacement.RightFootIKPosition.z + offsetGridRightLocal.z);
        centerGridLeftFootHeight = new Vector3(feetPlacement.LeftFootIKPosition.x + offsetGridLeftLocal.x, Get(centerGridLeftFoot.x + offsetGridLeftLocal.x, centerGridLeftFoot.z + offsetGridLeftLocal.z), feetPlacement.LeftFootIKPosition.z + offsetGridLeftLocal.z);
        centerGridRightFootHeight = new Vector3(feetPlacement.RightFootIKPosition.x + offsetGridRightLocal.x, Get(centerGridRightFoot.x + offsetGridRightLocal.x, centerGridRightFoot.z + offsetGridRightLocal.z), feetPlacement.RightFootIKPosition.z + offsetGridRightLocal.z);
    }

    #endregion

    #region Terrain-Update Methods

    /// <summary>
    /// Get Normal and Gradients from Terrain
    /// </summary>
    private void GetTerrainSlopeCOM()
    {
        // Get character position in terrain
        float pos_x = _rb.position.x / _terrain.terrainData.size.x;
        float pos_z = _rb.position.z / _terrain.terrainData.size.z;

        // Get normal in that point
        Vector3 normal = _terrain.terrainData.GetInterpolatedNormal(pos_x, pos_z);
        Vector3 localNormal = this.transform.InverseTransformDirection(normal);

        // Get initial normal in that point - TODO: CHECK IF IT IS CORRECT
        Vector3 initialNormal = initialNormalMatrix[(int)World2Grid(_rb.position).x, (int)World2Grid(_rb.position).z];
        Vector3 initialNormalLocal = this.transform.InverseTransformDirection(initialNormal);

        // Gradient in that point
        float gradient = _terrain.terrainData.GetSteepness(pos_x, pos_z);

        // Original gradient in that point
        float initialGradient = initialGradientMatrix[(int)World2Grid(_rb.position).x, (int)World2Grid(_rb.position).z];

        // Save normals (current and original)
        slopeNormal = localNormal;
        slopeInitialNormal = initialNormalLocal;

        // Save signed angle
        float dotProductDirectionGround = Vector3.Dot(_rb.GetComponent<Transform>().forward.normalized, localNormal.normalized);
        slopeAngle = dotProductDirectionGround < 0 ? gradient : -gradient;
        slopeInitialAngle = dotProductDirectionGround < 0 ? initialGradient : -initialGradient;

        // Calculate COM Projection on the terrain (values change for obstacles instead of terrains)
        _COMPosition = _rb.worldCenterOfMass;

        RaycastHit hitCOM;
        if (Physics.Raycast(_COMPosition, -Vector3.up, out hitCOM, Mathf.Infinity, ground))
        {
            _COMPositionProjected = hitCOM.point;

            if (hitCOM.collider.gameObject.CompareTag("Obstacle"))
            {
                slopeNormal = hitCOM.normal;
                slopeAngle = Vector3.Angle(Vector3.up, hitCOM.normal);

                // New, for signed angle
                float dotProductDirectionObstacle = Vector3.Dot(_rb.GetComponent<Transform>().forward.normalized, hitCOM.normal.normalized);
                slopeAngle = dotProductDirectionObstacle < 0 ? slopeAngle : -slopeAngle;
            }
        }

        if (drawAngleNormal)
            Debug.DrawLine(_rb.position, _rb.position + normal, Color.blue);

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
            case inputDeformation.useUI:
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
            //case inputDeformation.useManualValues:
            //    break;
            case inputDeformation.useTerrainPrefabs:
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
            case inputDeformation.useManualValuesWithVegetation:
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

            #region Left Foot

            if (timePassedLeft <= contactTime + timeOffset)
            {
                if (isLeftFootGrounded)
                {
                    if (printSteps)
                    {
                        Debug.Log("1 - [INFO] Applying Brush Deformation LEFT - Time passed: " + timePassedLeft);
                    }

                    // Brush that takes limbs positions and creates physically-based deformations
                    _brushPhysicalFootprint.CallFootprint(feetPlacement.LeftFootIKPosition.x + offsetGridLeftLocal.x,
                                                          feetPlacement.LeftFootIKPosition.z + offsetGridLeftLocal.z,
                                                          feetPlacement.RightFootIKPosition.x + offsetGridRightLocal.x,
                                                          feetPlacement.RightFootIKPosition.z + offsetGridRightLocal.z);

                    // Reset the stabilization counter and change flag
                    counterStabilizationLeft = 0;
                    RunningCoroutineStabilizationLeft = false;

                    // TODO: CHECK - Summing time during which the foot is in contact with the ground
                    timeDuringPressureLeft += Time.deltaTime;

                    // GAUSSIAN TEST
                    //Debug.Log("[INFO GAUSSIAN] Reseting counter gaussian Left");
                    counterGaussianFiltersLeft = 0; 
                }
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

                    _brushPhysicalFootprint.StabilizeFootprint(feetPlacement.LeftFootIKPosition.x, 
                                                               feetPlacement.LeftFootIKPosition.z, 
                                                               feetPlacement.RightFootIKPosition.x, 
                                                               feetPlacement.RightFootIKPosition.z);

                    counterStabilizationLeft++;
                }

                // TODO: CHECK
                accumulatedTimeDuringPressureLeft += timeDuringPressureLeft;
                timeDuringPressureLeft = 0f;
            }

            #endregion

            #region Right Foot

            if (timePassedRight <= contactTime + timeOffset)
            {
                if (isRightFootGrounded)
                {
                    if (printSteps)
                    {
                        Debug.Log("1 - [INFO] Applying Brush Deformation RIGHT - Time passed: " + timePassedRight);
                    }

                    // Brush that takes limbs positions and creates physically-based deformations
                    _brushPhysicalFootprint.CallFootprint(feetPlacement.LeftFootIKPosition.x + offsetGridLeftLocal.x,
                                                          feetPlacement.LeftFootIKPosition.z + offsetGridLeftLocal.z,
                                                          feetPlacement.RightFootIKPosition.x + offsetGridRightLocal.x,
                                                          feetPlacement.RightFootIKPosition.z + offsetGridRightLocal.z);

                    // Reset the stabilization counter and change flag
                    counterStabilizationRight = 0;
                    RunningCoroutineStabilizationRight = false;

                    // TODO: CHECK - Summing time during which the foot is in contact with the ground
                    timeDuringPressureRight += Time.deltaTime;

                    // GAUSSIAN TEST
                    //Debug.Log("[INFO GAUSSIAN] Reseting counter gaussian Right");
                    counterGaussianFiltersRight = 0;
                }
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

                    _brushPhysicalFootprint.StabilizeFootprint(feetPlacement.LeftFootIKPosition.x, 
                                                               feetPlacement.LeftFootIKPosition.z, 
                                                               feetPlacement.RightFootIKPosition.x, 
                                                               feetPlacement.RightFootIKPosition.z);

                    counterStabilizationRight++;
                }

                // TODO: CHECK
                accumulatedTimeDuringPressureRight += timeDuringPressureRight;
                timeDuringPressureRight = 0f;
            }

            #endregion
        }

        // ---------------------- //

        #region Left Foot 

        // Reset Times for each foot when we lift them from the ground
        if (!isLeftFootGrounded)
        {
            if (wasLeftFootGrounded)
            {
                //Debug.Log("[INFO] Saving temporal position Left");
                temporalPositionLeft = new Vector2(feetPlacement.LeftFootIKPosition.x + offsetGridLeftLocal.x, feetPlacement.LeftFootIKPosition.z + offsetGridLeftLocal.z);
            }

            // Update last known state
            wasLeftFootGrounded = isLeftFootGrounded;

            if (printSteps)
                Debug.Log("3 - [INFO] LEFT Foot NOT grounded - Reset Time to 0");

            // TODO: CHECK 
            timeDuringPressureLeft = 0f;
            accumulatedTimeDuringPressureLeft += timeDuringPressureLeft;

            // We do not update here anymore
            //timePassedLeft = 0f;
            //RunningCoroutineDeformationLeft = true;
        }

        // If the previous state was not grounded and now we enter here, we reset the time. We do it only when we change state.
        if (isLeftFootGrounded) // BEFORE: isLeftFootHeelGrounded
        {
            if (!wasLeftFootGrounded)
            {
                // ---

                // GAUSSIAN TEST
                if (applyGaussianFilter)
                {
                    if (counterGaussianFiltersLeft < maxGaussianFilters)
                    {
                        //Debug.Log("[INFO] Apply Gaussian Filter Left");
                        for (int i = 0; i < iterationsGaussianLeft; i++)
                        {
                            Debug.Log("[INFO] Applying Iteration Gaussian Left: " + i);
                            _brushPhysicalFootprint.CallGaussianSingle(temporalPositionLeft.x,
                                                                       temporalPositionLeft.y,
                                                                       smoothStrength,
                                                                       smoothRadius);
                        }
                    }

                    //Debug.Log("[INFO] Increasing counter Gaussian Left");
                    counterGaussianFiltersLeft++;
                }

                // ---

                // ERROR: FOR UPHILL THIS WOULD NOT WORK, AS THE HEEL REMAINS ABOVE ALWAYS!
                //Debug.Log("[INFO] Reset time Left");

                timePassedLeft = 0f;
                RunningCoroutineDeformationLeft = true;

                wasLeftFootGrounded = isLeftFootGrounded;
            }
            else if (wasLeftFootGrounded)
            {
                //Debug.Log("[INFO] NOT reset time Left");
            }
        }

        #endregion

        #region Right Foot

        // Reset Times for each foot when we lift them from the ground
        if (!isRightFootGrounded)
        {
            if (wasRightFootGrounded)
            {
                //Debug.Log("[INFO] Saving temporal position Right");
                temporalPositionRight = new Vector2(feetPlacement.RightFootIKPosition.x + offsetGridRightLocal.x, feetPlacement.RightFootIKPosition.z + offsetGridRightLocal.z);
            }

            // Update last known state
            wasRightFootGrounded = isRightFootGrounded;

            if (printSteps)
                Debug.Log("3 - [INFO] RIGHT Foot NOT grounded - Reset Time to 0");

            // TODO: CHECK 
            timeDuringPressureRight = 0f;
            accumulatedTimeDuringPressureRight += timeDuringPressureRight;

            // We do not update here anymore
            //timePassedRight = 0f;
            //RunningCoroutineDeformationRight = true;
        }

        // If the previous state was not grounded and now we enter here, we reset the time. We do it only when we change state.
        if (isRightFootGrounded) // BEFORE: isRightFootHeelGrounded
        {
            if (!wasRightFootGrounded)
            {
                // ---

                // GAUSSIAN TEST
                if (applyGaussianFilter)
                {
                    if (counterGaussianFiltersRight < maxGaussianFilters)
                    {
                        //Debug.Log("[INFO] Apply Gaussian Filter Right");
                        for (int i = 0; i < iterationsGaussianRight; i++)
                        {
                            Debug.Log("[INFO] Applying Iteration Gaussian Right: " + i);
                            _brushPhysicalFootprint.CallGaussianSingle(temporalPositionRight.x,
                                                                       temporalPositionRight.y,
                                                                       smoothStrength,
                                                                       smoothRadius);
                        }
                    }

                    //Debug.Log("[INFO] Increasing counter Gaussian Left");
                    counterGaussianFiltersRight++;
                }

                // ---

                // ERROR: FOR UPHILL THIS WOULD NOT WORK, AS THE HEEL REMAINS ABOVE ALWAYS!
                //Debug.Log("[INFO] Reset time");

                timePassedRight = 0f;
                RunningCoroutineDeformationRight = true;
                
                wasRightFootGrounded = isRightFootGrounded;
            }
            else if (wasRightFootGrounded)
            {
                //Debug.Log("[INFO] NOT reset time");
            }
        }

        #endregion
    }

    #endregion

    #region Exporting Maps

    private void ExportHeightMap()
    {
        for (int y = 0; y < _terrainData.heightmapResolution; y++)
        {
            for (int x = 0; x < _terrainData.heightmapResolution; x++)
            {
                HeightMapMatrix[x, y] = Get(x, y);
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
            if (!MxM.RetrieveVelocity.IsIdle)
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
        return cellSize;
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
    public void SetFootprintBrush(BrushFootprint brush)
    {
        Debug.Log("[INFO] Setting brush to " + brush);
        _brushPhysicalFootprint = brush;
    }
    public BrushFootprint GetFootprintBrush()
    {
        return _brushPhysicalFootprint;
    }

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
        //Debug.Log("x: " + x + " z: " + z + " heightmapWidth: " + _heightmapWidth + " heightmapHeight: " + _heightmapHeight + " heightmapData: " + _heightmapData[z, x] + " terrainData.heightmapScale.y: " + _terrainData.heightmapScale.y);
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
}
