using UnityEngine;
using System.Collections.Generic;

using Common.Mathematics.LinearAlgebra;
using Common.Geometry.Shapes;
using Common.Unity.Drawing;
using Common.Unity.Mathematics;

using PositionBasedDynamics.Bodies;
using PositionBasedDynamics.Bodies.Deformable;
using PositionBasedDynamics.Bodies.Cloth;
using PositionBasedDynamics.Sources;
using PositionBasedDynamics.Forces;
using PositionBasedDynamics.Solvers;
using PositionBasedDynamics.Collisions;

namespace PositionBasedDynamics
{
    public class BasicPBDDemo : MonoBehaviour
    {
        #region Instance Fields

        // Debug
        [Header("Debug")]
        public bool printInformation;

        // External Obstacles
        [Header("External")]
        public GameObject obstacle1;
        public GameObject obstacle2;

        // Reference Grid (White)
        [Header("Grid")]
        public int GRID_SIZE = 2;

        // Global pos and rotation of mesh
        [Header("Body 1 - Global pos and rot")]
        public GameObject tet;
        public Vector3 translation1;
        public Vector3 rotation1;

        // Meshes
        [Header("Body 1 - Properties")]
        public Vector3 minVector3;
        public Vector3 maxVector3;
        public double radius1 = 0.25;
        public double stiffness1 = 0.2;
        public double mass1 = 1.0;
        public Vector3 fxMin;
        public Vector3 fxMax;

        // Global pos and rotation of cloth
        [Header("Body 2 - Global pos and rot")]
        public GameObject cloth;
        public Vector3 translation2;
        public Vector3 rotation2;

        [Header("Body 2 - Properties")]
        public double radius2 = 0.25;
        public double mass2 = 1.0;
        public double width2 = 5.0;
        public double height2 = 4.0;
        public double depth2 = 5.0;
        public double stretchStiffness = 0.25;
        public double bendStiffness = 0.5;
        public Vector3 fxMinCloth;
        public Vector3 fxMaxCloth;

        // Global pos and rotation of cloth
        [Header("Body 3 - Global pos and rot")]
        public GameObject cloth3;
        public Vector3 translation3;
        public Vector3 rotation3;

        [Header("Body 3 - Properties")]
        public double radius3 = 0.25;
        public double mass3 = 1.0;
        public double width3 = 5.0;
        public double height3 = 4.0;
        public double depth3 = 5.0;
        public Vector3 fxMinCloth3;
        public Vector3 fxMaxCloth3;

        [Header("Mesh - Debug")]
        public bool drawLines = true;
        public bool drawMesh = true;
        public bool drawSpheres = true;
        //public Mesh mesh;
        public Material sphereMaterial;
        public Material sphereMaterialNoContact;
        public Material sphereMaterialContact;
        public Material sphereMaterialBroken;
        [Range(0f, 1f)] public float scaleRadius;

        [Header("Solver")]
        public int iterations = 1; // 4 before
        public int solverIterations = 1; // 2 before
        public int collisionIterations = 1; // 2 before

        [Header("Not used")]
        public float thresholdAngle = 50f;

        #endregion

        #region Instance Properties

        private List<GameObject> Spheres { get; set; }
        private List<GameObject> Spheres2 { get; set; }
        private List<GameObject> Spheres3 { get; set; }

        private DeformableBody3d Body1 { get; set; }
        private ClothBody3d Body2 { get; set; }
        private ClothBody3d Body3 { get; set; }
        private Rigidbody ExtRigidBody1 { get; set; }
        private Rigidbody ExtRigidBody2 { get; set; }

        private Solver3d Solver { get; set; }

        private Box3d StaticBounds1 { get; set; }
        private Box3d StaticBounds2 { get; set; }
        private Box3d StaticBounds3 { get; set; }

        public List<ClothBody3d> dummyList = new List<ClothBody3d>();

        #endregion

        #region Read-only & Static Fields

        //private double timeStep = 1.0 / 60.0; 
        private double timeStep; 

        private Vector3d min;
        private Vector3d max;

        #endregion

        // Start is called before the first frame update
        void Start()
        {
            // 1. Create Tetrahedron Body
            // ==========================
            
            // Global pos and rotation of mesh
            Matrix4x4d T = Matrix4x4d.Translate(new Vector3d(translation1.x, translation1.y, translation1.z));
            Matrix4x4d R = Matrix4x4d.Rotate(new Vector3d(rotation1.x, rotation1.y, rotation1.z));
            Matrix4x4d TR = T * R;

            // Mesh (body) - using PBD
            min = new Vector3d(minVector3.x, minVector3.y, minVector3.z);
            max = new Vector3d(maxVector3.x, maxVector3.y, maxVector3.z);
            Box3d bounds = new Box3d(min, max);

            TetrahedronsFromBounds source1 = new TetrahedronsFromBounds(radius1, bounds);

            Body1 = new DeformableBody3d(source1, radius1, mass1, stiffness1, TR);

            // 2. Create Cloth Body
            // ==========================

            //Matrix4x4d TCloth = Matrix4x4d.Translate(new Vector3d(translation2.x, translation2.y, translation2.z));
            //Matrix4x4d RCloth = Matrix4x4d.Rotate(new Vector3d(rotation2.x, rotation2.y, rotation2.z));
            //Matrix4x4d TRCloth = TCloth * RCloth;

            //TrianglesFromGrid source2 = new TrianglesFromGrid(radius2, width2, depth2);
            
            //Body2 = new ClothBody3d(source2, radius1, mass1, stretchStiffness, bendStiffness, thresholdAngle, thresholdAngle, 0f, 0f, TRCloth, 0f);

            // 3. Create Cloth Body 4x4
            // ==========================

            //Matrix4x4d TCloth3 = Matrix4x4d.Translate(new Vector3d(translation3.x, translation3.y, translation3.z));
            //Matrix4x4d RCloth3 = Matrix4x4d.Rotate(new Vector3d(rotation3.x, rotation3.y, rotation3.z));
            //Matrix4x4d TRCloth3 = TCloth3 * RCloth3;

            //TrianglesFromGrid source3 = new TrianglesFromGrid(radius3, width3, depth3);

            //Body3 = new ClothBody3d(source3, radius3, mass3, stretchStiffness, bendStiffness, thresholdAngle, thresholdAngle, 0f, 0f, TRCloth3, 0f);

            // ----------
            
            // Add external body
            ExtRigidBody1 = obstacle1.GetComponent<Rigidbody>();
            ExtRigidBody2 = obstacle2.GetComponent<Rigidbody>();

            // Damping
            System.Random rnd = new System.Random(0);
            Body1.Dampning = 1.0;
            Body1.RandomizePositions(rnd, radius1 * 0.01);
            Body1.RandomizeConstraintOrder(rnd);

            //Body2.Dampning = 1.0;

            //Body3.Dampning = 1.0;

            // -------------------------

            // Static Bounds (Green) - Fix what is inside the bounds
            Vector3d smin = new Vector3d(fxMin.x, fxMin.y, fxMin.z);
            Vector3d smax = new Vector3d(fxMax.x, fxMax.y, fxMax.z);
            StaticBounds1 = new Box3d(smin, smax);
            Body1.MarkAsStatic(StaticBounds1);

            //Vector3d sminCloth = new Vector3d(fxMinCloth.x, fxMinCloth.y, fxMinCloth.z);
            //Vector3d smaxCloth = new Vector3d(fxMaxCloth.x, fxMaxCloth.y, fxMaxCloth.z);
            //StaticBounds2 = new Box3d(sminCloth, smaxCloth);
            //Body2.MarkAsStatic(StaticBounds2);

            //Vector3d sminCloth3 = new Vector3d(fxMinCloth3.x, fxMinCloth3.y, fxMinCloth3.z);
            //Vector3d smaxCloth3 = new Vector3d(fxMaxCloth3.x, fxMaxCloth3.y, fxMaxCloth3.z);
            //StaticBounds3 = new Box3d(sminCloth3, smaxCloth3);
            //Body3.MarkAsStatic(StaticBounds3);

            // -------------------------

            // Create Solver
            Solver = new Solver3d(Body1.NumParticles);

            // Add particle-based bodies
            Solver.AddBody(Body1);
            //Solver.AddBody(Body2);
            //Solver.AddBody(Body3);

            // Add external Unity bodies
            Solver.AddExternalBody(ExtRigidBody1);
            Solver.AddExternalBody(ExtRigidBody2);

            // Add external forces
            Solver.AddForce(new GravitationalForce3d());

            // -------

            // Add collisions with ground
            Collision3d ground = new PlanarCollision3d(Vector3d.UnitY, 0); // Before 0 - changed to counteract the scaling factor
            Solver.AddCollision(ground);

            // Add collision with particle-based bodies
            //Collision3d bodyBody = new BodyCollision3d(Body1, Body2);
            //Solver.AddCollision(bodyBody);
            
            // Add collision with external bodies
            CollisionExternal3d bodyWithExt = new BodyCollisionExternal3d(Body1, ExtRigidBody1);
            Solver.AddExternalCollision(bodyWithExt);
            
            //CollisionExternal3d bodyWithExt2 = new BodyCollisionExternal3d(Body2, ExtRigidBody1);
            //Solver.AddExternalCollision(bodyWithExt2);

            //CollisionExternal3d bodyWithExt3 = new BodyCollisionExternal3d(Body1, ExtRigidBody2);
            //Solver.AddExternalCollision(bodyWithExt3);

            //CollisionExternal3d bodyWithExt4 = new BodyCollisionExternal3d(Body2, ExtRigidBody2);
            //Solver.AddExternalCollision(bodyWithExt4);

            //CollisionExternal3d bodyWithExt5 = new BodyCollisionExternal3d(Body3, ExtRigidBody2);
            //Solver.AddExternalCollision(bodyWithExt5);

            // -------

            Solver.SolverIterations = solverIterations;
            Solver.CollisionIterations = collisionIterations;
            Solver.SleepThreshold = 1;

            // Create Spheres
            if (drawSpheres)
            {
                CreateSpheres();
                //CreateSpheres2();
                //CreateSpheres3();
            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            // Update time step
            timeStep = Time.fixedDeltaTime;
            
            // Update dts
            double dts = timeStep / iterations;

            for (int i = 0; i < iterations; i++)
                Solver.StepPhysics(0, dummyList, dts, false, false, 0f, false);

            // Update Spheres
            if (drawSpheres)
            {
                UpdateSpheres();
                //UpdateSpheres2();
                //UpdateSpheres3();
            }
        }

        void OnDestroy()
        {
            if (Spheres != null)
            {
                for (int i = 0; i < Spheres.Count; i++)
                    DestroyImmediate(Spheres[i]);
            }

            //if (Spheres2 != null)
            //{
            //    for (int i = 0; i < Spheres2.Count; i++)
            //        DestroyImmediate(Spheres2[i]);
            //}


            //if (Spheres3 != null)
            //{
            //    for (int i = 0; i < Spheres3.Count; i++)
            //        DestroyImmediate(Spheres3[i]);
            //}
        }

        private void OnRenderObject()
        {
            if (drawLines)
            {
                Camera camera = Camera.current;

                Vector3 min = new Vector3(-GRID_SIZE, 0, -GRID_SIZE);
                Vector3 max = new Vector3(GRID_SIZE, 0, GRID_SIZE);

                DrawLines.DrawGrid(camera, Color.white, min, max, 1, transform.localToWorldMatrix);

                Matrix4x4d m = MathConverter.ToMatrix4x4d(transform.localToWorldMatrix);
                DrawLines.DrawVertices(LINE_MODE.TETRAHEDRON, camera, Color.grey, Body1.Positions, Body1.Indices, m);
                //DrawLines.DrawVertices(LINE_MODE.TRIANGLES, camera, Color.red, Body2.Positions, Body2.Indices, m);
                //DrawLines.DrawVertices(LINE_MODE.TRIANGLES, camera, Color.red, Body3.Positions, Body3.Indices, m);

                //DrawLines.DrawBounds(camera, Color.green, StaticBounds1, Matrix4x4d.Identity);
                //DrawLines.DrawBounds(camera, Color.green, StaticBounds2, Matrix4x4d.Identity);
                //DrawLines.DrawBounds(camera, Color.green, StaticBounds3, Matrix4x4d.Identity);

            }

            if (drawMesh)
            {
                //Vector3[] vertices = new Vector3[Body2.Positions.Length * 2]; // 24
                //Vector3[] normals = new Vector3[(Body2.Indices.Length / 3) * 2]; // 24

                //GetComponent<MeshFilter>().mesh = mesh = new Mesh();
                //mesh.name = "TextureMesh";
                
                //for (int i = 0; i < Body2.Positions.Length; i++) // From 0 to 12
                //{
                //    vertices[i] = Body2.Positions[i].ToVector3();
                //}

                //for (int i = Body2.Positions.Length; i < Body2.Positions.Length * 2; i++) // From 12 to 24 // TEST
                //{
                //    vertices[i] = Body2.Positions[i - Body2.Positions.Length].ToVector3();
                //}

                //mesh.vertices = vertices;

                //int[] triangles = new int[Body2.Indices.Length * 2]; // 72

                //for (int i = 0; i < Body2.Indices.Length; i++) // From 0 to 36
                //{
                //    triangles[i] = Body2.Indices[i];
                //}

                //for (int i = Body2.Indices.Length; i < Body2.Indices.Length * 2; i++) // From 36 to 72 // ERROR - FLIP NORMAL
                //{
                //    triangles[i] = Body2.Indices[i - Body2.Indices.Length];
                //}

                //mesh.triangles = triangles;

                //Debug.Log("Normals: " + normals.Length);

                //for (var n = 12; n < normals.Length; n++)
                //{
                //    normals[n] = -normals[n];
                //}

                //mesh.normals = normals;

                //mesh.RecalculateNormals();
            }
        }

        private void CreateSpheres()
        {
            if (sphereMaterial == null) return;

            Spheres = new List<GameObject>();

            int numParticles = Body1.NumParticles;
            float diam = (float)Body1.ParticleRadius * 2.0f * scaleRadius;

            for (int i = 0; i < numParticles; i++)
            {
                Vector3 pos = MathConverter.ToVector3(Body1.Positions[i]);

                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.name = i.ToString(); // TEST
                sphere.transform.parent = tet.transform;
                sphere.transform.position = pos;
                sphere.transform.localScale = new Vector3(diam, diam, diam);
                sphere.GetComponent<Collider>().enabled = true;
                sphere.GetComponent<MeshRenderer>().material = sphereMaterial;
                sphere.AddComponent<DetectCollision>();

                sphere.GetComponent<MeshRenderer>().material = sphereMaterialNoContact;

                Spheres.Add(sphere);
            }
        }

        private void CreateSpheres2()
        {
            if (sphereMaterial == null) return;

            Spheres2 = new List<GameObject>();

            int numParticles = Body2.NumParticles;
            float diam = (float)Body2.ParticleRadius * 2.0f * scaleRadius;

            for (int i = 0; i < numParticles; i++)
            {
                Vector3 pos = MathConverter.ToVector3(Body2.Positions[i]);

                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.name = i.ToString(); // TEST
                sphere.transform.parent = cloth.transform;
                sphere.transform.position = pos;
                sphere.transform.localScale = new Vector3(diam, diam, diam);
                sphere.GetComponent<Collider>().enabled = true;
                sphere.GetComponent<MeshRenderer>().material = sphereMaterial;
                sphere.AddComponent<DetectCollision>();

                sphere.GetComponent<MeshRenderer>().material = sphereMaterialNoContact;

                Spheres2.Add(sphere);
            }
        }

        private void CreateSpheres3()
        {
            if (sphereMaterial == null) return;

            Spheres3 = new List<GameObject>();

            int numParticles = Body3.NumParticles;
            float diam = (float)Body3.ParticleRadius * 2.0f * scaleRadius;

            for (int i = 0; i < numParticles; i++)
            {
                Vector3 pos = MathConverter.ToVector3(Body3.Positions[i]);

                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.name = i.ToString(); // TEST
                sphere.transform.parent = cloth3.transform;
                sphere.transform.position = pos;
                sphere.transform.localScale = new Vector3(diam, diam, diam);
                sphere.GetComponent<Collider>().enabled = true;
                sphere.GetComponent<MeshRenderer>().material = sphereMaterial;
                sphere.AddComponent<DetectCollision>();

                sphere.GetComponent<MeshRenderer>().material = sphereMaterialNoContact;

                Spheres3.Add(sphere);
            }
        }

        public void UpdateSpheres()
        {
            if (Spheres != null)
            {
                for (int i = 0; i < Spheres.Count; i++)
                {
                    Vector3d pos = Body1.Positions[i];
                    Spheres[i].transform.position = new Vector3((float)pos.x, (float)pos.y, (float)pos.z);

                    if (Body1.IsContact[i] && !Body1.IsBroken[i])
                        Spheres[i].GetComponent<MeshRenderer>().material = sphereMaterialContact;
                    else if (!Body1.IsContact[i] && !Body1.IsBroken[i])
                        Spheres[i].GetComponent<MeshRenderer>().material = sphereMaterialNoContact;
                    else if (Body1.IsBroken[i])
                        Spheres[i].GetComponent<MeshRenderer>().material = sphereMaterialBroken;
                }
            }
        }

        public void UpdateSpheres2()
        {
            if (Spheres2 != null)
            {
                for (int i = 0; i < Spheres2.Count; i++)
                {
                    Vector3d pos = Body2.Positions[i];
                    Spheres2[i].transform.position = new Vector3((float)pos.x, (float)pos.y, (float)pos.z);

                    if (Body2.IsContact[i] && !Body2.IsBroken[i])
                        Spheres2[i].GetComponent<MeshRenderer>().material = sphereMaterialContact;
                    else if (!Body2.IsContact[i] && !Body2.IsBroken[i])
                        Spheres2[i].GetComponent<MeshRenderer>().material = sphereMaterialNoContact;
                    else if (Body2.IsBroken[i])
                        Spheres2[i].GetComponent<MeshRenderer>().material = sphereMaterialBroken;
                }
            }
        }

        public void UpdateSpheres3()
        {
            if (Spheres3 != null)
            {
                for (int i = 0; i < Spheres3.Count; i++)
                {
                    Vector3d pos = Body3.Positions[i];
                    Spheres3[i].transform.position = new Vector3((float)pos.x, (float)pos.y, (float)pos.z);

                    if (Body3.IsContact[i] && !Body3.IsBroken[i])
                        Spheres3[i].GetComponent<MeshRenderer>().material = sphereMaterialContact;
                    else if (!Body3.IsContact[i] && !Body3.IsBroken[i])
                        Spheres3[i].GetComponent<MeshRenderer>().material = sphereMaterialNoContact;
                    else if (Body3.IsBroken[i])
                        Spheres3[i].GetComponent<MeshRenderer>().material = sphereMaterialBroken;
                }
            }
        }

        public void CollisionFromChildBody1(Collision hit, GameObject children)
        {
            if (printInformation)
                Debug.Log("[BasicPBDDemo] Collision Detected with object: " + hit.gameObject.name + " with particle/sphere: " + children.name);

            if (printInformation)
                Debug.Log("CHANGING SPHERE: " + int.Parse(children.name) + " with the value " + Spheres[int.Parse(children.name)].GetComponent<DetectCollision>().Hit.GetContact(0).point);

            Body1.ExternalHit[int.Parse(children.name)] = hit.GetContact(0);
            Body1.IsContact[int.Parse(children.name)] = true;

            if (printInformation)
            {
                Debug.Log("==================");
                for (int x = 0; x < Body1.NumParticles; x++)
                {
                    if (Body1.IsContact[x])
                        Debug.Log("[INFO] Sphere: " + x + " is in contact: " + Body1.IsContact[x] + " - Body1.ExternalHit[x]: " + Body1.ExternalHit[x].point);
                    else
                        Debug.Log("[INFO] Sphere: " + x + " NOT in contact: " + Body1.IsContact[x] + " - Body1.ExternalHit[x]: " + Body1.ExternalHit[x].point);
                }
                Debug.Log("==================");

                // It draws multiple rays
                Debug.DrawRay(Body1.ExternalHit[int.Parse(children.name)].point, Body1.ExternalHit[int.Parse(children.name)].normal, Color.blue, 0.1f);
            }
        }

        public void ExitCollisionFromChildBody1(GameObject children)
        {
            Body1.ExternalHit[int.Parse(children.name)] = new ContactPoint();
            Body1.IsContact[int.Parse(children.name)] = false;

            if (printInformation)
            {
                Debug.Log("==================");
                for (int x = 0; x < Body1.NumParticles; x++)
                {
                    if (Body1.IsContact[x])
                        Debug.Log("[INFO] Sphere: " + x + " is in contact: " + Body1.IsContact[x] + " - Body1.ExternalHit[x]: " + Body1.ExternalHit[x].point);
                    else
                        Debug.Log("[INFO] Sphere: " + x + " NOT in contact: " + Body1.IsContact[x] + " - Body1.ExternalHit[x]: " + Body1.ExternalHit[x].point);
                }
                Debug.Log("==================");

                // It draws multiple rays
                Debug.DrawRay(Body1.ExternalHit[int.Parse(children.name)].point, Body1.ExternalHit[int.Parse(children.name)].normal, Color.blue, 0.1f);
            }
        }

        public void CollisionFromChildBody2(Collision hit, GameObject children)
        {
            if(printInformation)
                Debug.Log("[BasicPBDDemo] Collision Detected with object: " + hit.gameObject.name + " with particle/sphere: " + children.name);

            if (printInformation)
                Debug.Log("CHANGING SPHERE: " + int.Parse(children.name) + " with the value " + Spheres2[int.Parse(children.name)].GetComponent<DetectCollision>().Hit.GetContact(0).point);

            Body2.ExternalHit[int.Parse(children.name)] = hit.GetContact(0);
            Body2.IsContact[int.Parse(children.name)] = true;

            if (printInformation)
            {
                Debug.Log("==================");
                for (int x = 0; x < Body2.NumParticles; x++)
                {
                    if (Body2.IsContact[x])
                        Debug.Log("[INFO] Sphere: " + x + " is in contact: " + Body2.IsContact[x] + " - Body2.ExternalHit[x]: " + Body2.ExternalHit[x].point);
                    else
                        Debug.Log("[INFO] Sphere: " + x + " NOT in contact: " + Body2.IsContact[x] + " - Body2.ExternalHit[x]: " + Body2.ExternalHit[x].point);
                }
                Debug.Log("==================");

                // It draws multiple rays
                Debug.DrawRay(Body2.ExternalHit[int.Parse(children.name)].point, Body2.ExternalHit[int.Parse(children.name)].normal, Color.blue, 0.1f);
            }
        }

        public void ExitCollisionFromChildBody2(GameObject children)
        {
            Body2.ExternalHit[int.Parse(children.name)] = new ContactPoint();
            Body2.IsContact[int.Parse(children.name)] = false;

            if (printInformation)
            {
                Debug.Log("==================");
                for (int x = 0; x < Body2.NumParticles; x++)
                {
                    if (Body2.IsContact[x])
                        Debug.Log("[INFO] Sphere: " + x + " is in contact: " + Body2.IsContact[x] + " - Body2.ExternalHit[x]: " + Body2.ExternalHit[x].point);
                    else
                        Debug.Log("[INFO] Sphere: " + x + " NOT in contact: " + Body2.IsContact[x] + " - Body2.ExternalHit[x]: " + Body2.ExternalHit[x].point);
                }
                Debug.Log("==================");

                // It draws multiple rays
                Debug.DrawRay(Body2.ExternalHit[int.Parse(children.name)].point, Body2.ExternalHit[int.Parse(children.name)].normal, Color.blue, 0.1f);
            }
        }

        public void CollisionFromChildBody3(Collision hit, GameObject children)
        {
            if (printInformation)
                Debug.Log("[BasicPBDDemo] Collision Detected with object: " + hit.gameObject.name + " with particle/sphere: " + children.name);

            if (printInformation)
                Debug.Log("CHANGING SPHERE: " + int.Parse(children.name) + " with the value " + Spheres3[int.Parse(children.name)].GetComponent<DetectCollision>().Hit.GetContact(0).point);

            Body3.ExternalHit[int.Parse(children.name)] = hit.GetContact(0);
            Body3.IsContact[int.Parse(children.name)] = true;

            if (printInformation)
            {
                Debug.Log("==================");
                for (int x = 0; x < Body3.NumParticles; x++)
                {
                    if (Body3.IsContact[x])
                        Debug.Log("[INFO] Sphere: " + x + " is in contact: " + Body3.IsContact[x] + " - Body3.ExternalHit[x]: " + Body3.ExternalHit[x].point);
                    else
                        Debug.Log("[INFO] Sphere: " + x + " NOT in contact: " + Body3.IsContact[x] + " - Body3.ExternalHit[x]: " + Body3.ExternalHit[x].point);
                }
                Debug.Log("==================");

                // It draws multiple rays
                Debug.DrawRay(Body3.ExternalHit[int.Parse(children.name)].point, Body3.ExternalHit[int.Parse(children.name)].normal, Color.blue, 0.1f);
            }
        }

        public void ExitCollisionFromChildBody3(GameObject children)
        {
            Body3.ExternalHit[int.Parse(children.name)] = new ContactPoint();
            Body3.IsContact[int.Parse(children.name)] = false;

            if (printInformation)
            {
                Debug.Log("==================");
                for (int x = 0; x < Body3.NumParticles; x++)
                {
                    if (Body3.IsContact[x])
                        Debug.Log("[INFO] Sphere: " + x + " is in contact: " + Body3.IsContact[x] + " - Body3.ExternalHit[x]: " + Body3.ExternalHit[x].point);
                    else
                        Debug.Log("[INFO] Sphere: " + x + " NOT in contact: " + Body3.IsContact[x] + " - Body3.ExternalHit[x]: " + Body3.ExternalHit[x].point);
                }
                Debug.Log("==================");

                // It draws multiple rays
                Debug.DrawRay(Body3.ExternalHit[int.Parse(children.name)].point, Body3.ExternalHit[int.Parse(children.name)].normal, Color.blue, 0.1f);
            }
        }
    } 
}
