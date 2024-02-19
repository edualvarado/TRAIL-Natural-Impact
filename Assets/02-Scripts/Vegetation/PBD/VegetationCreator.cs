/****************************************************
 * File: VegetationCreator.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 01/10/2020
   * Project: Foot2Trail
   * Last update: 24/02/2023
*****************************************************/

using UnityEngine;

using Common.Mathematics.LinearAlgebra;
using Common.Unity.Drawing;
using Common.Unity.Mathematics;

using PositionBasedDynamics.Bodies.Cloth;
using PositionBasedDynamics.Forces;
using PositionBasedDynamics.Solvers;
using PositionBasedDynamics.Collisions;

using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using System.Collections;
using System.Collections.Generic;

namespace PositionBasedDynamics
{
    public enum distribution 
    { 
        array, 
        random 
    };

    /// <summary>
    /// Master class to create vegetation on the environment.
    /// </summary>
    public class VegetationCreator : MonoBehaviour
    {
        #region Update Methods

        [Header("Run Update")]
        public bool runUpdate;
        public bool runFixedUpdate;

        #endregion

        #region Instance Fields

        //[Header("Vegetation Parameters")]
        //public float A = 1;
        //public float B = 1;
        //public float C = 1;
        public bool activateRegeneration = false;

        [Header("Terrain Deformation")]
        [SerializeField] private TerrainDeformationMaster terrainDeformationMaster;

        [Header("Vegetation Measurement - OUTPUT")]
        public float[] livingRatio;
        public int subGridRadius = 1;

        // ----------------------------------------------

        [Header("Body - Global position and orientation")]
        public Vector3 translation;
        public Vector3 rotation;

        [Header("Body - Randomizer Position/Rotation")]
        public distribution distributionType = distribution.array;
        public float distanceArrayMode = 0.05f;
        public Vector2 limitsTerrainRandomMode;
        public Vector3 originLimitsTerrainRandomMode;
        public Vector3 randomTranslation;
        public Vector3 randomRotation;

        [Header("Body - Properties")]
        public int numberOfPlants = 2;
        public List<ClothBody3d> plantsList = new List<ClothBody3d>();
        public Vector2 plantSize;
        public double mass = 1.0;
        public double diameter = 0.5;
        public double spaceBetween = 0;

        [Header("Body - Stiffness & Damping")]
        [Range(0f, 1f)] public double stretchStiffness = 0.25;
        [Range(0f, 3f)] public double bendStiffness = 0.5;
        [Range(1f, 10f)] public double damping = 10f;

        [Header("Body - Breaking/Degradation")]
        public float breakingPoint = 25;
        public float degradationPoint = 10;
        public float tStiffness = 10f;
        public float tGrowing = 10f;
        public float stepStiffness = 0.1f;
        
        // ----------------------------------------------

        [Header("Particles - Debug/UI")]
        public bool drawParticles = true;

        [Header("Mesh - Debug/UI")]
        public bool drawMesh = true;
        public bool drawLines = true;
        public bool drawStaticBounds = true;
        public bool drawRenderedLines;

        [Header("Body - Debug/UI")]
        public GameObject canvas;
        public bool showLifeRatio = false;
        public Vector3 offsetPositionBar;
        public Transform rotateWithRespect;

        [Header("Body - Debug/Collision")]
        public bool printCollisionInformation;
        public bool usePenetrationDistance;
        public float collisionFixedStep;

        // ----------------------------------------------

        [Header("External")]
        public bool applyGravity;
        public GameObject leftFoot;
        public GameObject rightFoot;

        [Header("Particles - Materials")]
        public Material grass;
        //public Material particleMaterial;
        public Material particleMaterialNoContact;
        public Material particleMaterialContact;
        public Material particleMaterialBroken;
        public Material constraintMaterialStretch;
        public Gradient gradientColorStiffness;
        public double maxValueGradient;

        [Header("PBD Solver")]
        public bool multipleSolver;
        public int iterations = 2; // 4 before
        public int solverIterations = 1; // 2 before
        public int collisionIterations = 1; // 2 before

        [Header("Experimental")]
        public bool useJobs;
        public bool useJobsInSolver;

        #endregion

        #region Instance Properties

        public static float[,] LivingRatioMatrix { get; set; }
        public static byte[] LivingRatioBytes { get; set; }
        public static int[,] NumberPlantsMatrix { get; set; }

        public List<GameObject> Particles { get; set; }

        public Plant PlantType { get; set; }
        public ClothBody3d Body { get; set; }

        public Rigidbody ExtRigidBodyLeftFoot { get; set; }
        public Rigidbody ExtRigidBodyRightFoot { get; set; }

        public Solver3d Solver { get; set; }
        public Solver3d[] SolverArray { get; set; }
        public List<Solver3d> SolverList { get; set; }

        #endregion

        #region Read-only & Static Fields

        private GameObject[] _bodyPlantParent;
        private GameObject[] _bodyPlantMaster;

        private Coroutine _runSolver;

        private GameObject _myTerrain;

        private double _timeStep;
        private int GRID_SIZE = 2;

        private float[] _currentStiffness;
        private float[] _totalStiffness;

        public static float[] currentStiffnessProxis;

        private GameObject[] _bars;
        private LifeBar[] _lifeBars;
        private StiffnessBar[] _stiffnessBars;

        private Mesh[] _deformingMesh;
        private MeshRenderer[] _meshRendererMesh;
        private MeshRenderer[,] _meshRendererParticles;
        private Vector3[] _originalVertices, _displacedVertices;
        private Vector3[] _originalVerticesLocal, _displacedVerticesLocal;
        private Vector2[] _originalUV, _displacedUV;
        private int[] _triangles;

        private GameObject[,] _particlesPlant; // Each particle for each plant in a 2D array

        private LineRenderer[,] lineRendererStretch1;
        private LineRenderer[,] lineRendererStretch2;
        private LineRenderer[,] lineRendererStretch3;

        private LineRenderer[,] lineRendererBend1;
        private LineRenderer[,] lineRendererBend2;

        private float[,] distanceArray;

        private float[] stiffnessRatio;
        private float[] deltaL;
        

        #endregion

        #region Unity Methods

        private void Awake()
        {
            // Maps - TODO
            LivingRatioMatrix = new float[257, 257];
            NumberPlantsMatrix = new int[257, 257];
            LivingRatioBytes = new byte[LivingRatioMatrix.Length * sizeof(float)];

            //LivingRatioMatrix = new float[513, 513];
            //NumberPlantsMatrix = new int[513, 513];
            //LivingRatioBytes = new byte[LivingRatioMatrix.Length * sizeof(float)];
        }

        void Start()
        {
            if (numberOfPlants < 1)
                return;

            // MULTIPLE SOLVERs - Initialization
            SolverArray = new Solver3d[numberOfPlants];
            SolverList = new List<Solver3d>();

            // Initialize parents
            _bodyPlantParent = new GameObject[numberOfPlants];
            _bodyPlantMaster = new GameObject[numberOfPlants];

            // Initialize Ratios and Total Stiffness
            livingRatio = new float[numberOfPlants];
            _currentStiffness = new float[numberOfPlants];
            _totalStiffness = new float[numberOfPlants];
            currentStiffnessProxis = new float[numberOfPlants];

            // TEST
            stiffnessRatio = new float[numberOfPlants];
            deltaL = new float[numberOfPlants];

            // Initialize UI
            _bars = new GameObject[numberOfPlants];

            // External rigidbody
            ExtRigidBodyLeftFoot = leftFoot.GetComponent<Rigidbody>(); // TODO: Needed?
            ExtRigidBodyRightFoot = rightFoot.GetComponent<Rigidbody>(); // TODO: Needed?

            // Retrieve Terrain
            _myTerrain = terrainDeformationMaster.gameObject;

            // External collisions
            Collision3d realGround = new PlanarCollision3d(Vector3d.UnitY, _myTerrain.transform.position.y + 1f - (float)diameter / 2); // TODO: REPLACE 1f and PUT HEIGHT FOR EACH PLANT

            // UI
            _lifeBars = new LifeBar[numberOfPlants];
            _stiffnessBars = new StiffnessBar[numberOfPlants];

            // Maps
            //LivingRatioMatrix = new float[terrainDeformationMaster.MyTerrainData.heightmapResolution, terrainDeformationMaster.MyTerrainData.heightmapResolution];
            //NumberPlantsMatrix = new int[terrainDeformationMaster.MyTerrainData.heightmapResolution, terrainDeformationMaster.MyTerrainData.heightmapResolution];
            //LivingRatioBytes = new byte[LivingRatioMatrix.Length * sizeof(float)];
            // Manual
            LivingRatioMatrix = new float[257, 257];
            NumberPlantsMatrix = new int[257, 257];
            LivingRatioBytes = new byte[LivingRatioMatrix.Length * sizeof(float)];

            // For each plant - for one type
            for (int i = 0; i < numberOfPlants; i++)
            {
                // Create parents for each plant instance
                _bodyPlantParent[i] = new GameObject(i.ToString());
                _bodyPlantMaster[i] = new GameObject("Plant_" + i.ToString());
                
                Debug.Log("[START-VEG] Creating " + _bodyPlantMaster[i].name);

                // SEED RANDOM
                int seed = 12 + i; // Choose a seed value
                Random.InitState(seed);

                randomTranslation = new Vector3(originLimitsTerrainRandomMode.x + Random.Range(-limitsTerrainRandomMode.x, limitsTerrainRandomMode.x), 0f, originLimitsTerrainRandomMode.y + Random.Range(-limitsTerrainRandomMode.y, limitsTerrainRandomMode.y));
                randomRotation = new Vector3(0f, Random.Range(0, 90f), 0f);

                Vector3 gridPlant = terrainDeformationMaster.World2Grid(randomTranslation.x, randomTranslation.z);
                float heightInPlant = terrainDeformationMaster.Get(gridPlant.x, gridPlant.z);

                // Position/Rotation - TODO: Change hardcoded values
                if (distributionType == distribution.random)
                {
                    // Put before random translation and rotation
                    _bodyPlantMaster[i].transform.position = Vector3.zero + new Vector3(randomTranslation.x, heightInPlant, randomTranslation.z); // TODO: Replace translation.y by heightmap 

                    _bodyPlantMaster[i].transform.rotation = Quaternion.Euler(new Vector3(0f, -randomRotation.y, 0f));
                }
                else if (distributionType == distribution.array)
                {
                    if (i < 10) // Before 20
                    {
                        _bodyPlantMaster[i].transform.position = Vector3.zero + new Vector3(translation.x - 0.2f, translation.y, translation.z + i * distanceArrayMode);
                        _bodyPlantMaster[i].transform.rotation = Quaternion.Euler(new Vector3(0f, -rotation.y, 0f));
                    }
                    else if (i >= 10 && i < 20)
                    {
                        _bodyPlantMaster[i].transform.position = Vector3.zero + new Vector3(translation.x - 0.1f, translation.y, translation.z + (i - 10) * distanceArrayMode);
                        _bodyPlantMaster[i].transform.rotation = Quaternion.Euler(new Vector3(0f, -rotation.y, 0f));
                    }
                    else if (i >= 20 && i < 30)
                    {
                        _bodyPlantMaster[i].transform.position = Vector3.zero + new Vector3(translation.x, translation.y, translation.z + (i - 20) * distanceArrayMode);
                        _bodyPlantMaster[i].transform.rotation = Quaternion.Euler(new Vector3(0f, -rotation.y, 0f));
                    }
                    else if (i >= 30 && i < 40)
                    {
                        _bodyPlantMaster[i].transform.position = Vector3.zero + new Vector3(translation.x + 0.1f, translation.y, translation.z + (i - 30) * distanceArrayMode);
                        _bodyPlantMaster[i].transform.rotation = Quaternion.Euler(new Vector3(0f, -rotation.y, 0f));
                    }
                    else if (i >= 40)
                    {
                        _bodyPlantMaster[i].transform.position = Vector3.zero + new Vector3(translation.x + 0.2f, translation.y, translation.z + (i - 40) * distanceArrayMode);
                        _bodyPlantMaster[i].transform.rotation = Quaternion.Euler(new Vector3(0f, -rotation.y, 0f));
                    }
                }
                else
                {
                    Debug.LogError("Choose correct vegetation mode! (random, array)");
                }

                // Finish parent for mesh
                _bodyPlantParent[i].transform.parent = _bodyPlantMaster[i].transform;
                _bodyPlantParent[i].AddComponent<MeshFilter>();
                _bodyPlantParent[i].AddComponent<MeshRenderer>();

                _bodyPlantParent[i].GetComponent<MeshRenderer>().material = grass;

                // Instantiate UI canvas for each plant
                GameObject canvasInstance = Instantiate(canvas, _bodyPlantMaster[i].transform.position + offsetPositionBar, Quaternion.identity, _bodyPlantMaster[i].transform);
                _bars[i] = canvasInstance;
                _lifeBars[i] = canvasInstance.transform.GetChild(0).GetComponent<LifeBar>();
                _stiffnessBars[i] = canvasInstance.transform.GetChild(1).GetComponent<StiffnessBar>();

                // Initialize plant instance
                PlantType = new Plant(plantSize, mass, diameter, spaceBetween, stretchStiffness, bendStiffness, damping, breakingPoint, degradationPoint, tStiffness, stepStiffness, tGrowing);
                
                if (distributionType == distribution.random)
                {
                    //Body = PlantType.CreatePlant(new Vector3(randomTranslation.x, translation.y, randomTranslation.z), new Vector3(rotation.x, randomRotation.y, rotation.z), _bodyPlantMaster[i]);
                    Body = PlantType.CreatePlant(new Vector3(randomTranslation.x, heightInPlant, randomTranslation.z), new Vector3(rotation.x, randomRotation.y, rotation.z), _bodyPlantMaster[i], i);
                }
                else if (distributionType == distribution.array)
                {
                    if (i < 10)
                        Body = PlantType.CreatePlant(new Vector3(translation.x - 0.2f, translation.y, translation.z + i * distanceArrayMode), new Vector3(rotation.x, rotation.y, rotation.z), _bodyPlantMaster[i], i);
                    else if (i >= 10 && i < 20)
                        Body = PlantType.CreatePlant(new Vector3(translation.x - 0.1f, translation.y, translation.z + (i - 10) * distanceArrayMode), new Vector3(rotation.x, rotation.y, rotation.z), _bodyPlantMaster[i], i);
                    else if (i >= 20 && i < 30)
                        Body = PlantType.CreatePlant(new Vector3(translation.x, translation.y, translation.z + (i - 20) * distanceArrayMode), new Vector3(rotation.x, rotation.y, rotation.z), _bodyPlantMaster[i], i);
                    else if (i >= 30 && i < 40)
                        Body = PlantType.CreatePlant(new Vector3(translation.x + 0.1f, translation.y, translation.z + (i - 30) * distanceArrayMode), new Vector3(rotation.x, rotation.y, rotation.z), _bodyPlantMaster[i], i);
                    else if (i >= 40)
                        Body = PlantType.CreatePlant(new Vector3(translation.x + 0.2f, translation.y, translation.z + (i - 40) * distanceArrayMode), new Vector3(rotation.x, rotation.y, rotation.z), _bodyPlantMaster[i], i);
                }

                // Add to list of plants
                plantsList.Add(Body);

                // -- Solvers

                // SINGLE SOLVER - Initialize Solver
                //Solver = new Solver3d(Body.NumParticles);

                // SINGLE SOLVER - Add particle-based body
                //Solver.AddBody(Body);

                Debug.Log("[START-VEG] Creating Solver " + i);

                // MULTIPLE SOLVERs - Initialize Solvers
                SolverArray[i] = new Solver3d(Body.NumParticles);

                // MULTIPLE SOLVERs - Add particle-based body
                SolverArray[i].AddBody(Body);

                // MULTIPLE SOLVERs - Add collisions for each solver with external obstacles
                SolverArray[i].AddExternalBody(ExtRigidBodyLeftFoot);
                SolverArray[i].AddExternalBody(ExtRigidBodyRightFoot);
                SolverArray[i].AddCollision(realGround);

                // MULTIPLE SOLVERs - Add external forces
                if (applyGravity)
                    SolverArray[i].AddForce(new GravitationalForce3d());

                // MULTIPLE SOLVERs - Set iterations for each solver
                SolverArray[i].SolverIterations = solverIterations;
                SolverArray[i].CollisionIterations = collisionIterations;
                SolverArray[i].SleepThreshold = 1;


                // MODEL 2!!!
                //livingRatio[i] = 1;
            }

            // Initialize particles for each plant of one type
            _particlesPlant = new GameObject[plantsList.Count, Body.NumParticles];

            // Mesh Renderers
            _deformingMesh = new Mesh[numberOfPlants];
            _meshRendererMesh = new MeshRenderer[numberOfPlants];
            _meshRendererParticles = new MeshRenderer[plantsList.Count, Body.NumParticles];

            // SINGLE SOLVER - Add external Unity bodies
            //Solver.AddExternalBody(ExtRigidBody);
            //Solver.AddExternalBody(ExtRigidBody2);

            // SINGLE SOLVER - Add external forces
            //if (applyGravity)
            //    Solver.AddForce(new GravitationalForce3d());

            // SINGLE SOLVER - Add collisions with ground
            //Solver.AddCollision(realGround);

            // External Collisions
            for (int i = 0; i < numberOfPlants; i++)
            {
                // Add collisions with external bodies
                CollisionExternal3d bodyWithExternal = new BodyCollisionExternal3d(plantsList[i], ExtRigidBodyLeftFoot);
                CollisionExternal3d bodyWithExternal2 = new BodyCollisionExternal3d(plantsList[i], ExtRigidBodyRightFoot);

                // SINGLE SOLVER - Add external collisions
                //Solver.AddExternalCollision(bodyWithExternal);
                //Solver.AddExternalCollision(bodyWithExternal2);

                // MULTIPLE SOLVERs - Add external collisions
                SolverArray[i].AddExternalCollision(bodyWithExternal);
                SolverArray[i].AddExternalCollision(bodyWithExternal2);
            }

            // SINGLE SOLVER - Iterations
            //Solver.SolverIterations = solverIterations;
            //Solver.CollisionIterations = collisionIterations;
            //Solver.SleepThreshold = 1;

            for (int i = 0; i < numberOfPlants; i++)
            {
                // MULTIPLE SOLVERs
                SolverList.Add(SolverArray[i]);
            }

            // Create mesh
            CreateMesh();

            // Create particles
            CreateParticles();

            // Precalculate a matrix of distances
            distanceArray = new float[subGridRadius * 2 + 1, subGridRadius * 2 + 1];
            for (int xi = -subGridRadius; xi <= subGridRadius; xi++)
            {
                for (int zi = -subGridRadius; zi <= subGridRadius; zi++)
                {
                    float distance = new Vector2(xi, zi).magnitude;
                    distanceArray[xi + subGridRadius, zi + subGridRadius] = distance;
                }
            }

            // Create Rendered Lines
            CreateColorRenderedLines();
        }
        
        private void FixedUpdate()
        {
            if (numberOfPlants < 1)
                return;

            if (runFixedUpdate)
            {
                // Update time step
                _timeStep = Time.fixedDeltaTime;

                // Update dts
                double dts = _timeStep / iterations;

                // Start time
                float startTime = Time.realtimeSinceStartup;

                if (!multipleSolver)
                {
                    // SINGLE SOLVER
                    //for (int i = 0; i < iterations; i++)
                    //    Solver.StepPhysics(dts, false, usePenetrationDistance);
                }
                else
                {
                    // MULTIPLE SOLVERs
                    if (!useJobs)
                    {
                        // Option 1: Using array
                        for (int i = 0; i < numberOfPlants; i++)
                        {
                            // Option 1.a: Always run solver
                            RunSolver(dts, i);

                            // Option 1.b: Using flag
                            //Debug.Log("plantsList[i]: " + i + " ActivateSolver " + plantsList[i].ActivateSolver);

                            //if (plantsList[i].ActivateSolver)
                            //{
                            //    RunSolver(dts, i);
                            //}

                            //if (plantsList[i].ActivateSolver)
                            //{
                            //    RunSolver(dts, i);
                            //    StartCoroutine(ExecuteAfterTime(0.1f, i));
                            //}

                            //if (plantsList[i].ActivateSolver)
                            //{
                            //    Debug.Log("Running solvers...");
                            //    _runSolver = StartCoroutine(RunSolverCoroutine(dts, i));
                            //}
                        }

                        // Option 2: Using list
                        //foreach (Solver3d solver in SolverList)
                        //{
                        //    RunSolverList(solver, dts);
                        //}

                        // Option 3: Coroutine
                        //for (int i = 0; i < numberOfPlants; i++)
                        //{
                        //    _runSolver = StartCoroutine(RunSolverCoroutine(dts, i));
                        //}
                    }
                    else if (useJobs)
                    {
                        // TODO: Implement jobs
                        // 4. Create temporary arrays of data that we  want to change (native arrays)
                        //NativeList<Solver3d> moveYArray = new NativeList<Solver3d>(zombieList.Count, Allocator.TempJob);

                        // 5. Full the copy, native arrays

                        // 6. Tell job system to complete that particular job
                        //JobHandle jobHandle = ReallyToughParallelTaskJob();
                        //jobHandle.Complete();

                        // 7. Copy back the data

                        // 8. Dispose
                    }
                }

                //Debug.Log("Time: " + (Time.realtimeSinceStartup - startTime) * 1000 + " ms"); 
            }
        }

        private void Update()
        {
            if (numberOfPlants < 1)
                return;
            
            // Re-initialized Living Matrix in the terrain
            if (runUpdate)
            {
                ((IList)LivingRatioMatrix).Clear();
                ((IList)NumberPlantsMatrix).Clear();
                
                // --- Improve!           

                for (int i = 0; i < numberOfPlants; i++)
                {
                    // ------------------------------

                    // MODEL 1!!!!!!
                    // Estimate living ratio for each plant
                    livingRatio[i] = plantsList[i].BendingConstraintsVertical.Count / plantsList[i].TotalVerticalBendingConstraints;

                    // MODEL 2!!!!!!
                    //stiffnessRatio[i] = _currentStiffness[i] / _totalStiffness[i];
                    //deltaL[i] = stiffnessRatio[i] - livingRatio[i];
                    //livingRatio[i] = livingRatio[i] + (A * livingRatio[i] + B) * deltaL[i] * Time.deltaTime;

                    // MODEL 3!!!!!!
                    //stiffnessRatio[i] = _currentStiffness[i] / _totalStiffness[i];
                    //deltaL[i] = stiffnessRatio[i] - livingRatio[i];
                    //livingRatio[i] = livingRatio[i] + (A * (1 / (1 + Mathf.Exp(-livingRatio[i] * C))) + B) * deltaL[i] * Time.deltaTime;

                    //Debug.Log("Plant " + i + " - stiffnessRatio: " + stiffnessRatio[i] + " - deltaL[i]: " + deltaL[i]);

                    // ------------------------------

                    // Get grid-based plant position
                    Vector3 gridPlant = terrainDeformationMaster.World2Grid(_bodyPlantMaster[i].transform.position);

                    // Using subgrid around the plant

                    // A: Automatically based on plant size 
                    //var areaCell = deformTerrainMaster.LengthCellX();
                    //subGridRadius = (int)(plantSize.x / areaCell);    
                    // B: Manually (comment above)

                    for (int xi = -subGridRadius; xi <= subGridRadius; xi++)
                    {
                        for (int zi = -subGridRadius; zi <= subGridRadius; zi++)
                        {
                            // Get the corresponding distance
                            float distance = distanceArray[xi + subGridRadius, zi + subGridRadius];

                            //Debug.DrawRay(deformTerrainMaster.Grid2World(new Vector3(gridPlant.x + xi, gridPlant.y, gridPlant.z + zi)), Vector3.up, Color.red, 1f);

                            if (distance <= subGridRadius)
                            {
                                // Version 1
                                if (distance >= 1)
                                {
                                    LivingRatioMatrix[(int)gridPlant.x + xi, (int)gridPlant.z + zi] += livingRatio[i] / distance;
                                }
                                else
                                {
                                    LivingRatioMatrix[(int)gridPlant.x + xi, (int)gridPlant.z + zi] += livingRatio[i];
                                }

                                // Version 2
                                //LivingRatioMatrix[(int)gridPlant.x + xi, (int)gridPlant.z + zi] += (1 - (distance / subGridRadius)) * livingRatio[i];


                                if (NumberPlantsMatrix[(int)gridPlant.x + xi, (int)gridPlant.z + zi] < numberOfPlants)
                                    NumberPlantsMatrix[(int)gridPlant.x + xi, (int)gridPlant.z + zi] += 1;
                            }
                        }
                    }


                    // Update UI
                    if (showLifeRatio)
                    {
                        // Activate UI
                        _bars[i].gameObject.SetActive(true);
                        _bars[i].gameObject.transform.rotation = Quaternion.Euler(0f, Quaternion.LookRotation(_lifeBars[i].gameObject.transform.position - rotateWithRespect.position).eulerAngles.y, 0f);

                        _lifeBars[i].SetLifeBar(livingRatio[i]);
                        _stiffnessBars[i].SetStiffnessBar(_currentStiffness[i] / _totalStiffness[i]);
                        //Debug.Log("VEGETATION CREATOR: " + _currentStiffness[i] + " / " + _totalStiffness[i]);

                        _lifeBars[i].gameObject.transform.rotation = Quaternion.Euler(0f, Quaternion.LookRotation(_lifeBars[i].gameObject.transform.position - rotateWithRespect.position).eulerAngles.y, 90f);
                        _stiffnessBars[i].gameObject.transform.rotation = Quaternion.Euler(0f, Quaternion.LookRotation(_stiffnessBars[i].gameObject.transform.position - rotateWithRespect.position).eulerAngles.y, 90f);
                    }
                    else
                    {
                        _bars[i].gameObject.SetActive(false);
                    }
                }

                // Average vegetation impact and create final impact map
                int resolution = terrainDeformationMaster.MyTerrainData.heightmapResolution;
                for (int x = 0; x < resolution; x++)
                {
                    for (int y = 0; y < resolution; y++)
                    {
                        if (NumberPlantsMatrix[x, y] != 0)
                            LivingRatioMatrix[x, y] /= NumberPlantsMatrix[x, y];

                        //Debug.Log("x: " + x + " y: " + y + " LivingRatioMatrix[x, y]: " + LivingRatioMatrix[x, y]);
                    }
                }

                // Transform to byte array to transfer via socket
                LivingRatioBytes = BytesConversion.ToBytes(LivingRatioMatrix);
            }
        }

        private void LateUpdate()
        {
            if (numberOfPlants < 1)
                return;
            
            // Update mesh
            UpdateMesh();

            // Update particles
            UpdateParticles();

            // Update Rendered Lines
            UpdateColorRenderedLines();
        }

        #endregion

        // ----

        #region No-jobs

        // Task used by the non-job system
        private void RunSolver(double dts, int i)
        {
            for (int j = 0; j < iterations; j++)
            {
                SolverArray[i].StepPhysics(i, plantsList, dts, useJobsInSolver, usePenetrationDistance, collisionFixedStep, activateRegeneration);
            }
        }

        // Task used by the non-job system (list)
        private void RunSolverList(Solver3d solver, double dts)
        {
            for (int j = 0; j < iterations; j++)
            {
                solver.StepPhysics(0, plantsList, dts, useJobsInSolver, usePenetrationDistance, collisionFixedStep, activateRegeneration); // Warning: Placeholder!
            }
        }

        IEnumerator RunSolverCoroutine(double dts, int i)
        {
            for (int j = 0; j < iterations; j++)
            {
                SolverArray[i].StepPhysics(i, plantsList, dts, useJobsInSolver, usePenetrationDistance, collisionFixedStep, activateRegeneration);
            }
            
            yield break;
        }

        #endregion

        #region Jobs

        /*
        private JobHandle ReallyToughParallelTaskJob()
        {
            // 2. Create instance of the struct passing the copied arguments
            ReallyToughParallelJob job = new ReallyToughParallelJob
            {
                deltaTime = Time.deltaTime,
                iterations = iterations,
                SolverArray = SolverArray,
                useJobsInside = useJobsInside,
                usePenetrationDistance = usePenetrationDistance
            };

            return job.Schedule(SolverList.Count, 10);
        }

        // 1. Struct with parallel job containing information and behavior for the job by modifying copies (native arrays) of data though a list
        [BurstCompile]
        public struct ReallyToughParallelJob : IJobParallelFor
        {
            [ReadOnly] public float deltaTime;
            [ReadOnly] public int iterations;
            [ReadOnly] public bool useJobsInside;
            [ReadOnly] public bool usePenetrationDistance;

            public Solver3d[] SolverArray; // Can's use managed objects

            public void Execute(int index)
            {
                float dts = deltaTime / iterations;
                
                for (int j = 0; j < iterations; j++)
                {
                    SolverArray[index].StepPhysics(dts, useJobsInside, usePenetrationDistance);
                }
            }
        }
        */

        #endregion

        // ----

        #region Create/Update Particles

        private void CreateParticles()
        {
            if (particleMaterialNoContact == null) return;

            // Initialize GameObjects
            Particles = new List<GameObject>();

            for (int i = 0; i < numberOfPlants; i++)
            {
                // Initialize total stiffnesss
                _totalStiffness[i] = 0f;    
                for (int k = 0; k < plantsList[i].BendingConstraintsVertical.Count; k++)
                {
                    _totalStiffness[i] += (float)plantsList[i].BendingConstraintsVertical[k].CurrentStiffness;
                }

                // Add Trigger
                _bodyPlantParent[i].AddComponent<PlantActivation>();
                _bodyPlantParent[i].AddComponent<CapsuleCollider>();
                CapsuleCollider activator = _bodyPlantParent[i].GetComponent<CapsuleCollider>();
                activator.isTrigger = true;

                // Define spheres for each particle
                int numParticles = plantsList[i].NumParticles;
                float diam = (float)plantsList[i].ParticleRadius * 2.0f;

                for (int j = 0; j < numParticles; j++)
                {
                    Vector3 pos = MathConverter.ToVector3(plantsList[i].Positions[j]);

                    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                    sphere.name = j.ToString();
                    sphere.transform.parent = _bodyPlantParent[i].transform;
                    sphere.transform.position = pos;
                    sphere.transform.localScale = new Vector3(diam, diam, diam);
                    sphere.AddComponent<DetectionCollision>();

                    // Take component and modify its variable
                    sphere.GetComponent<DetectionCollision>().ParentScript = this;
                    
                    sphere.GetComponent<Collider>().enabled = true;

                    _meshRendererParticles[i,j] = sphere.GetComponent<MeshRenderer>();
                    _meshRendererParticles[i, j].material = particleMaterialNoContact;

                    Particles.Add(sphere);
                    _particlesPlant[i, j] = sphere;
                }
            }
        }

        public void UpdateParticles()
        {
            if (Particles != null)
            {
                for (int i = 0; i < numberOfPlants; i++)
                {
                    // Retrieve current stiffnesss
                    _currentStiffness[i] = 0f;
                    currentStiffnessProxis[i] = 0f;
                    for (int k = 0; k < plantsList[i].BendingConstraintsVertical.Count; k++)
                    {
                        _currentStiffness[i] += (float)plantsList[i].BendingConstraintsVertical[k].CurrentStiffness;
                        currentStiffnessProxis[i] += (float)plantsList[i].BendingConstraintsVertical[k].CurrentStiffness;
                    }

                    // TEST
                    _totalStiffness[i] = plantsList[i].BendingConstraintsVertical.Count * 1f;

                    for (int j = 0; j < _particlesPlant.GetLength(1); j++)
                    {
                        Vector3d pos = plantsList[i].Positions[j];
                        _particlesPlant[i, j].transform.position = new Vector3((float)pos.x, (float)pos.y, (float)pos.z);

                        if (drawParticles)
                        {
                            _meshRendererParticles[i, j].enabled = true;

                            if (plantsList[i].IsContact[j] && !plantsList[i].IsBroken[j])
                            {
                                // Turn on GREEN
                                _meshRendererParticles[i, j].material = particleMaterialContact;
                            }
                            else if (!plantsList[i].IsContact[j] && !plantsList[i].IsBroken[j])
                            {
                                _meshRendererParticles[i, j].material = particleMaterialNoContact;

                                for (int k = 0; k < plantsList[i].BendingConstraintsVertical.Count; k++)
                                {
                                    if (j == plantsList[i].BendingConstraintsVertical[k].ParticleConstrained[1])
                                    {
                                        //Debug.Log("CurrentStiffness for plant: " + i + " and plantsList[i].BendingConstraintsVertical[k].ParticleConstrained: " + plantsList[i].BendingConstraintsVertical[k].ParticleConstrained +
                                        //    " the CurrentStiffness is: " + plantsList[i].BendingConstraintsVertical[k].CurrentStiffness);

                                        // Turn on YELLOW GRADIENT (SPHERE)
                                        //_meshRendererParticles[i, j].material.color = new Color(1f, 1f, Mathf.Lerp(0, 1, (float)plantsList[i].BendingConstraintsVertical[k].CurrentStiffness));
                                        //_meshRendererParticles[i, j].material.color = new Color(1f, 1f, Mathf.Lerp(0, 1, (float)plantsList[i].BendingConstraintsVertical[k].CurrentStiffness));
                                        //_meshRendererParticles[i, j].material.color = new Color(1f, 1f, Mathf.Lerp(0, 1, (float)plantsList[i].BendingConstraintsVertical[k].CurrentStiffness));
                                    }
                                }

                            }
                            else if (plantsList[i].IsBroken[j])
                            {
                                // Turn on RED
                                _meshRendererParticles[i, j].material = particleMaterialBroken;
                            }
                        }
                        else
                        {
                            _meshRendererParticles[i, j].enabled = false;
                        }
                    }
                }
            }
        }

        #endregion

        #region Create/Update Mesh

        private void CreateMesh()
        {           
            for (int i = 0; i < numberOfPlants; i++)
            {
                // Get parents
                GameObject parent = _bodyPlantParent[i];
                GameObject plant = _bodyPlantMaster[i];
                
                // Create mesh
                _deformingMesh[i] = new Mesh();
                _deformingMesh[i].name = "TextureMesh_" + i.ToString();

                // Assign to parent
                parent.GetComponent<MeshFilter>().mesh = _deformingMesh[i];

                // Save mesh renderer
                _meshRendererMesh[i] = parent.GetComponent<MeshRenderer>();

                // Initialize vertices
                _originalVertices = new Vector3[plantsList[i].Positions.Length];
                _originalVerticesLocal = new Vector3[plantsList[i].Positions.Length];
                _displacedVertices = new Vector3[plantsList[i].Positions.Length];
                _displacedVerticesLocal = new Vector3[plantsList[i].Positions.Length];
                
                for (int j = 0; j < plantsList[i].Positions.Length; j++)
                {
                    _originalVertices[j] = plantsList[i].Positions[j].ToVector3();
                    _originalVerticesLocal[j] = plant.transform.InverseTransformPoint(plantsList[i].Positions[j].ToVector3());
                }

                // Initialize UV
                _originalUV = new Vector2[_originalVertices.Length];
                _displacedUV = new Vector2[_displacedVertices.Length];

                for (int j = 0; j < plantsList[i].Positions.Length; j++)
                {
                    _originalUV[j] = new Vector2(_originalVerticesLocal[j].x, _originalVerticesLocal[j].y); // ERROR
                }

                // Initialize Triangles
                _triangles = new int[plantsList[i].Indices.Length];
                for (int j = 0; j < plantsList[i].Indices.Length; j++)
                {
                    _triangles[j] = plantsList[i].Indices[j];
                }

                // Set to mesh
                _deformingMesh[i].vertices = _originalVertices;
                _deformingMesh[i].uv = _originalUV;
                _deformingMesh[i].triangles = _triangles;
            }
        }

        private void UpdateMesh()
        {
            for (int i = 0; i < numberOfPlants; i++)
            {
                // Get parents
                GameObject plant = _bodyPlantMaster[i];
                
                if (drawMesh)
                {
                    // Make rendered visible
                    _meshRendererMesh[i].enabled = true;

                    for (int j = 0; j < plantsList[i].Positions.Length; j++)
                    {
                        // Update Vertices
                        _displacedVertices[j] = plantsList[i].Positions[j].ToVector3();
                        _displacedVerticesLocal[j] = plant.transform.InverseTransformPoint(plantsList[i].Positions[j].ToVector3());

                        // Update UV
                        _displacedUV[j] = new Vector2(_displacedVerticesLocal[j].x, _displacedVerticesLocal[j].y);
                    }

                    // Define triangles
                    _triangles = new int[plantsList[i].Indices.Length];
                    for (int j = 0; j < plantsList[i].Indices.Length; j++)
                    {
                        _triangles[j] = plantsList[i].Indices[j];
                    }

                    // Set to mesh
                    _deformingMesh[i].vertices = _displacedVertices;
                    _deformingMesh[i].uv = _displacedUV;
                    _deformingMesh[i].triangles = _triangles;
                }
                else
                {
                    // Make rendered invisible
                    _meshRendererMesh[i].enabled = false;
                }
            }
        }

        private void OnDestroy()
        {
            if (Particles != null)
            {
                for (int i = 0; i < Particles.Count; i++)
                    DestroyImmediate(Particles[i]);
            }
        }

        #endregion

        #region Create/Update Lines

        private void CreateColorRenderedLines()
        {
            if (drawRenderedLines)
            {
                lineRendererStretch1 = new LineRenderer[numberOfPlants, 7];
                lineRendererStretch2 = new LineRenderer[numberOfPlants, 7];
                lineRendererStretch3 = new LineRenderer[numberOfPlants, 7];
                lineRendererBend1 = new LineRenderer[numberOfPlants, 7];
                lineRendererBend2 = new LineRenderer[numberOfPlants, 7];

                for (int i = 0; i < numberOfPlants; i++)
                {
                    for (int col = 0; col < 7; col++)
                    {
                        GameObject lineStretch1 = new GameObject("LineStretch1_" + i.ToString() + "_" + col.ToString());
                        lineRendererStretch1[i, col] = lineStretch1.AddComponent<LineRenderer>();
                        lineRendererStretch1[i, col].positionCount = 2;
                        lineRendererStretch1[i, col].material = constraintMaterialStretch;

                        GameObject lineStretch2 = new GameObject("LineStretch2_" + i.ToString() + "_" + col.ToString());
                        lineRendererStretch2[i, col] = lineStretch2.AddComponent<LineRenderer>();
                        lineRendererStretch2[i, col].positionCount = 2;
                        lineRendererStretch2[i, col].material = constraintMaterialStretch;

                        GameObject lineStretch3 = new GameObject("LineStretch3_" + i.ToString() + "_" + col.ToString());
                        lineRendererStretch3[i, col] = lineStretch3.AddComponent<LineRenderer>();
                        lineRendererStretch3[i, col].positionCount = 2;
                        lineRendererStretch3[i, col].material = constraintMaterialStretch;

                        GameObject lineBend1 = new GameObject("LineBend1_" + i.ToString() + "_" + col.ToString());
                        lineRendererBend1[i, col] = lineBend1.AddComponent<LineRenderer>();
                        lineRendererBend1[i, col].positionCount = 2;
                        lineRendererBend1[i, col].colorGradient = gradientColorStiffness;

                        GameObject lineBend2 = new GameObject("LineBend2_" + i.ToString() + "_" + col.ToString());
                        lineRendererBend2[i, col] = lineBend2.AddComponent<LineRenderer>();
                        lineRendererBend2[i, col].positionCount = 2;
                        lineRendererBend2[i, col].colorGradient = gradientColorStiffness;

                        maxValueGradient = plantsList[i].BendingConstraintsVertical[0].CurrentStiffness; 
                    }
                }
            }
        }

        private void UpdateColorRenderedLines()
        {
            if (drawRenderedLines)
            {
                Camera camera = Camera.current;

                // Transformation
                Matrix4x4d m = MathConverter.ToMatrix4x4d(transform.localToWorldMatrix);

                for (int i = 0; i < numberOfPlants; i++)
                {
                    for (int col = 0; col < 7; col++)
                    {
                        // set width of the renderer
                        lineRendererStretch1[i, col].startWidth = 0.002f;
                        lineRendererStretch1[i, col].endWidth = 0.002f;
                        lineRendererStretch2[i, col].startWidth = 0.002f;
                        lineRendererStretch2[i, col].endWidth = 0.002f;
                        lineRendererStretch3[i, col].startWidth = 0.002f;
                        lineRendererStretch3[i, col].endWidth = 0.002f;
                        lineRendererBend1[i, col].startWidth = 0.002f;
                        lineRendererBend1[i, col].endWidth = 0.002f;
                        lineRendererBend2[i, col].startWidth = 0.002f;
                        lineRendererBend2[i, col].endWidth = 0.002f;

                        // set the position
                        if (col == 0)
                        {
                            lineRendererStretch1[i, col].SetPosition(0, plantsList[i].Positions[0].ToVector3());
                            lineRendererStretch1[i, col].SetPosition(1, plantsList[i].Positions[7].ToVector3());

                            lineRendererStretch2[i, col].SetPosition(0, plantsList[i].Positions[7].ToVector3());
                            lineRendererStretch2[i, col].SetPosition(1, plantsList[i].Positions[14].ToVector3());

                            lineRendererStretch3[i, col].SetPosition(0, plantsList[i].Positions[14].ToVector3());
                            lineRendererStretch3[i, col].SetPosition(1, plantsList[i].Positions[21].ToVector3());

                            lineRendererBend1[i, col].SetPosition(0, plantsList[i].Positions[0].ToVector3());
                            lineRendererBend1[i, col].SetPosition(1, plantsList[i].Positions[14].ToVector3());

                            lineRendererBend2[i, col].SetPosition(0, plantsList[i].Positions[7].ToVector3());
                            lineRendererBend2[i, col].SetPosition(1, plantsList[i].Positions[21].ToVector3()); 
                        }

                        if (col == 1)
                        {
                            lineRendererStretch1[i, col].SetPosition(0, plantsList[i].Positions[1].ToVector3());
                            lineRendererStretch1[i, col].SetPosition(1, plantsList[i].Positions[8].ToVector3());

                            lineRendererStretch2[i, col].SetPosition(0, plantsList[i].Positions[8].ToVector3());
                            lineRendererStretch2[i, col].SetPosition(1, plantsList[i].Positions[15].ToVector3());

                            lineRendererStretch3[i, col].SetPosition(0, plantsList[i].Positions[15].ToVector3());
                            lineRendererStretch3[i, col].SetPosition(1, plantsList[i].Positions[22].ToVector3());

                            lineRendererBend1[i, col].SetPosition(0, plantsList[i].Positions[1].ToVector3());
                            lineRendererBend1[i, col].SetPosition(1, plantsList[i].Positions[15].ToVector3());

                            lineRendererBend2[i, col].SetPosition(0, plantsList[i].Positions[8].ToVector3());
                            lineRendererBend2[i, col].SetPosition(1, plantsList[i].Positions[22].ToVector3());
                        }

                        if (col == 2)
                        {
                            lineRendererStretch1[i, col].SetPosition(0, plantsList[i].Positions[2].ToVector3());
                            lineRendererStretch1[i, col].SetPosition(1, plantsList[i].Positions[9].ToVector3());

                            lineRendererStretch2[i, col].SetPosition(0, plantsList[i].Positions[9].ToVector3());
                            lineRendererStretch2[i, col].SetPosition(1, plantsList[i].Positions[16].ToVector3());

                            lineRendererStretch3[i, col].SetPosition(0, plantsList[i].Positions[16].ToVector3());
                            lineRendererStretch3[i, col].SetPosition(1, plantsList[i].Positions[23].ToVector3());

                            lineRendererBend1[i, col].SetPosition(0, plantsList[i].Positions[2].ToVector3());
                            lineRendererBend1[i, col].SetPosition(1, plantsList[i].Positions[16].ToVector3());

                            lineRendererBend2[i, col].SetPosition(0, plantsList[i].Positions[9].ToVector3());
                            lineRendererBend2[i, col].SetPosition(1, plantsList[i].Positions[23].ToVector3());
                        }

                        if (col == 3)
                        {
                            lineRendererStretch1[i, col].SetPosition(0, plantsList[i].Positions[3].ToVector3());
                            lineRendererStretch1[i, col].SetPosition(1, plantsList[i].Positions[10].ToVector3());

                            lineRendererStretch2[i, col].SetPosition(0, plantsList[i].Positions[10].ToVector3());
                            lineRendererStretch2[i, col].SetPosition(1, plantsList[i].Positions[17].ToVector3());

                            lineRendererStretch3[i, col].SetPosition(0, plantsList[i].Positions[17].ToVector3());
                            lineRendererStretch3[i, col].SetPosition(1, plantsList[i].Positions[24].ToVector3());

                            lineRendererBend1[i, col].SetPosition(0, plantsList[i].Positions[3].ToVector3());
                            lineRendererBend1[i, col].SetPosition(1, plantsList[i].Positions[17].ToVector3());

                            lineRendererBend2[i, col].SetPosition(0, plantsList[i].Positions[10].ToVector3());
                            lineRendererBend2[i, col].SetPosition(1, plantsList[i].Positions[24].ToVector3());
                        }

                        if (col == 4)
                        {
                            lineRendererStretch1[i, col].SetPosition(0, plantsList[i].Positions[4].ToVector3());
                            lineRendererStretch1[i, col].SetPosition(1, plantsList[i].Positions[11].ToVector3());

                            lineRendererStretch2[i, col].SetPosition(0, plantsList[i].Positions[11].ToVector3());
                            lineRendererStretch2[i, col].SetPosition(1, plantsList[i].Positions[18].ToVector3());

                            lineRendererStretch3[i, col].SetPosition(0, plantsList[i].Positions[18].ToVector3());
                            lineRendererStretch3[i, col].SetPosition(1, plantsList[i].Positions[25].ToVector3());

                            lineRendererBend1[i, col].SetPosition(0, plantsList[i].Positions[4].ToVector3());
                            lineRendererBend1[i, col].SetPosition(1, plantsList[i].Positions[18].ToVector3());

                            lineRendererBend2[i, col].SetPosition(0, plantsList[i].Positions[11].ToVector3());
                            lineRendererBend2[i, col].SetPosition(1, plantsList[i].Positions[25].ToVector3());
                        }

                        if (col == 5)
                        {
                            lineRendererStretch1[i, col].SetPosition(0, plantsList[i].Positions[5].ToVector3());
                            lineRendererStretch1[i, col].SetPosition(1, plantsList[i].Positions[12].ToVector3());

                            lineRendererStretch2[i, col].SetPosition(0, plantsList[i].Positions[12].ToVector3());
                            lineRendererStretch2[i, col].SetPosition(1, plantsList[i].Positions[19].ToVector3());

                            lineRendererStretch3[i, col].SetPosition(0, plantsList[i].Positions[19].ToVector3());
                            lineRendererStretch3[i, col].SetPosition(1, plantsList[i].Positions[26].ToVector3());

                            lineRendererBend1[i, col].SetPosition(0, plantsList[i].Positions[5].ToVector3());
                            lineRendererBend1[i, col].SetPosition(1, plantsList[i].Positions[19].ToVector3());

                            lineRendererBend2[i, col].SetPosition(0, plantsList[i].Positions[12].ToVector3());
                            lineRendererBend2[i, col].SetPosition(1, plantsList[i].Positions[26].ToVector3());
                        }

                        if (col == 6)
                        {
                            lineRendererStretch1[i, col].SetPosition(0, plantsList[i].Positions[6].ToVector3());
                            lineRendererStretch1[i, col].SetPosition(1, plantsList[i].Positions[13].ToVector3());

                            lineRendererStretch2[i, col].SetPosition(0, plantsList[i].Positions[13].ToVector3());
                            lineRendererStretch2[i, col].SetPosition(1, plantsList[i].Positions[20].ToVector3());

                            lineRendererStretch3[i, col].SetPosition(0, plantsList[i].Positions[20].ToVector3());
                            lineRendererStretch3[i, col].SetPosition(1, plantsList[i].Positions[27].ToVector3());

                            lineRendererBend1[i, col].SetPosition(0, plantsList[i].Positions[6].ToVector3());
                            lineRendererBend1[i, col].SetPosition(1, plantsList[i].Positions[20].ToVector3());

                            lineRendererBend2[i, col].SetPosition(0, plantsList[i].Positions[13].ToVector3());
                            lineRendererBend2[i, col].SetPosition(1, plantsList[i].Positions[27].ToVector3());
                        }

                        //Debug.Log("plantsList[i].BendingConstraintsVertical[0].ParticleConstrained[1] " + plantsList[i].BendingConstraintsVertical[0].ParticleConstrained[1]);
                        //Debug.Log("plantsList[i].BendingConstraintsVertical[0].CurrentStiffness " + plantsList[i].BendingConstraintsVertical[0].CurrentStiffness);
                        //Debug.Log("plantsList[i].BendingConstraintsVertical[7].CurrentStiffness " + plantsList[i].BendingConstraintsVertical[7].CurrentStiffness);

                        for (int j = 0; j < _particlesPlant.GetLength(1); j++)
                        {
                            if (!plantsList[i].IsBroken[j])
                            {
                                for (int k = 0; k < plantsList[i].BendingConstraintsVertical.Count; k++)
                                {
                                    if (j == plantsList[i].BendingConstraintsVertical[k].ParticleConstrained[1])
                                    {
                                        if (j == 7)
                                            lineRendererBend1[i, 0].material.color = gradientColorStiffness.Evaluate((float)(plantsList[i].BendingConstraintsVertical[k].CurrentStiffness / maxValueGradient));
                                        if (j == 14)
                                            lineRendererBend2[i, 0].material.color = gradientColorStiffness.Evaluate((float)(plantsList[i].BendingConstraintsVertical[k].CurrentStiffness / maxValueGradient));

                                        if (j == 8)
                                            lineRendererBend1[i, 1].material.color = gradientColorStiffness.Evaluate((float)(plantsList[i].BendingConstraintsVertical[k].CurrentStiffness / maxValueGradient));
                                        if (j == 15)
                                            lineRendererBend2[i, 1].material.color = gradientColorStiffness.Evaluate((float)(plantsList[i].BendingConstraintsVertical[k].CurrentStiffness / maxValueGradient));

                                        if (j == 9)
                                            lineRendererBend1[i, 2].material.color = gradientColorStiffness.Evaluate((float)(plantsList[i].BendingConstraintsVertical[k].CurrentStiffness / maxValueGradient));
                                        if (j == 16)
                                            lineRendererBend2[i, 2].material.color = gradientColorStiffness.Evaluate((float)(plantsList[i].BendingConstraintsVertical[k].CurrentStiffness / maxValueGradient));

                                        if (j == 10)
                                            lineRendererBend1[i, 3].material.color = gradientColorStiffness.Evaluate((float)(plantsList[i].BendingConstraintsVertical[k].CurrentStiffness / maxValueGradient));
                                        if (j == 17)
                                            lineRendererBend2[i, 3].material.color = gradientColorStiffness.Evaluate((float)(plantsList[i].BendingConstraintsVertical[k].CurrentStiffness / maxValueGradient));

                                        if (j == 11)
                                            lineRendererBend1[i, 4].material.color = gradientColorStiffness.Evaluate((float)(plantsList[i].BendingConstraintsVertical[k].CurrentStiffness / maxValueGradient));
                                        if (j == 18)
                                            lineRendererBend2[i, 4].material.color = gradientColorStiffness.Evaluate((float)(plantsList[i].BendingConstraintsVertical[k].CurrentStiffness / maxValueGradient));

                                        if (j == 12)
                                            lineRendererBend1[i, 5].material.color = gradientColorStiffness.Evaluate((float)(plantsList[i].BendingConstraintsVertical[k].CurrentStiffness / maxValueGradient));
                                        if (j == 19)
                                            lineRendererBend2[i, 5].material.color = gradientColorStiffness.Evaluate((float)(plantsList[i].BendingConstraintsVertical[k].CurrentStiffness / maxValueGradient));

                                        if (j == 13)
                                            lineRendererBend1[i, 6].material.color = gradientColorStiffness.Evaluate((float)(plantsList[i].BendingConstraintsVertical[k].CurrentStiffness / maxValueGradient));
                                        if (j == 20)
                                            lineRendererBend2[i, 6].material.color = gradientColorStiffness.Evaluate((float)(plantsList[i].BendingConstraintsVertical[k].CurrentStiffness / maxValueGradient));

                                    }
                                }
                            }
                            else
                            {
                                if (j == 7)
                                    lineRendererBend1[i, 0].material.color = gradientColorStiffness.Evaluate(0f);
                                if (j == 14)
                                    lineRendererBend2[i, 0].material.color = gradientColorStiffness.Evaluate(0f);
                                
                                if (j == 8)
                                    lineRendererBend1[i, 1].material.color = gradientColorStiffness.Evaluate(0f);
                                if (j == 15)
                                    lineRendererBend2[i, 1].material.color = gradientColorStiffness.Evaluate(0f);

                                if (j == 9)
                                    lineRendererBend1[i, 2].material.color = gradientColorStiffness.Evaluate(0f);
                                if (j == 16)
                                    lineRendererBend2[i, 2].material.color = gradientColorStiffness.Evaluate(0f);

                                if (j == 10)
                                    lineRendererBend1[i, 3].material.color = gradientColorStiffness.Evaluate(0f);
                                if (j == 17)
                                    lineRendererBend2[i, 3].material.color = gradientColorStiffness.Evaluate(0f);

                                if (j == 11)
                                    lineRendererBend1[i, 4].material.color = gradientColorStiffness.Evaluate(0f);
                                if (j == 18)
                                    lineRendererBend2[i, 4].material.color = gradientColorStiffness.Evaluate(0f);

                                if (j == 12)
                                    lineRendererBend1[i, 5].material.color = gradientColorStiffness.Evaluate(0f);
                                if (j == 19)
                                    lineRendererBend2[i, 5].material.color = gradientColorStiffness.Evaluate(0f);

                                if (j == 13)
                                    lineRendererBend1[i, 6].material.color = gradientColorStiffness.Evaluate(0f);
                                if (j == 20)
                                    lineRendererBend2[i, 6].material.color = gradientColorStiffness.Evaluate(0f);

                            }
                        } 
                    }
                }
            }
        }
        
        #endregion

        #region Rendering 

        private void OnRenderObject()
        {
            if (drawLines)
            {
                Camera camera = Camera.current;

                // Grid
                Vector3 min = new Vector3(-GRID_SIZE, 0, -GRID_SIZE);
                Vector3 max = new Vector3(GRID_SIZE, 0, GRID_SIZE);
                DrawLines.DrawGrid(camera, Color.white, min, max, 1, transform.localToWorldMatrix);

                // Transformation
                Matrix4x4d m = MathConverter.ToMatrix4x4d(transform.localToWorldMatrix);

                for (int i = 0; i < numberOfPlants; i++)
                {
                    // Vertices
                    DrawLines.DrawVertices(LINE_MODE.TRIANGLES, camera, Color.grey, plantsList[i].Positions, plantsList[i].Indices, m);
                    
                    // Draw Static Bounds
                    if(drawStaticBounds)
                        DrawLines.DrawBounds(camera, Color.green, plantsList[i].StaticBounds, Matrix4x4d.Identity); 
                }
            }  
        }

        #endregion

        #region Collision Detectors

        public void CollisionFromChildBody(Collision hit, float penetrationDistance, GameObject children, GameObject parent, bool plantCollision)
        {
            if (printCollisionInformation)
            {
                Debug.Log("[BasicPBDDemo] Collision Detected with object: " + hit.gameObject.name + " with particle: " + children.name + " of parent: " + parent.name);
            }

            plantsList[int.Parse(parent.name)].ExternalHit[int.Parse(children.name)] = hit.GetContact(0);
            plantsList[int.Parse(parent.name)].PenetrationDistance[int.Parse(children.name)] = penetrationDistance;
            plantsList[int.Parse(parent.name)].IsContact[int.Parse(children.name)] = true;
            
            //plantsList[int.Parse(parent.name)].ActivateSolver = plantCollision;
        }

        public void ExitCollisionFromChildBody(GameObject children, GameObject parent, bool plantCollision)
        {

            plantsList[int.Parse(parent.name)].ExternalHit[int.Parse(children.name)] = new ContactPoint();
            plantsList[int.Parse(parent.name)].PenetrationDistance[int.Parse(children.name)] = 0f;
            plantsList[int.Parse(parent.name)].IsContact[int.Parse(children.name)] = false;
            
            //plantsList[int.Parse(parent.name)].ActivateSolver = plantCollision; 
        }

        public void TriggerPlantActivation(GameObject parent, bool plantActivation)
        {
            plantsList[int.Parse(parent.name)].ActivateSolver = plantActivation;
        }

        #endregion
    }
}

