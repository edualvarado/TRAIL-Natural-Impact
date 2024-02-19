// ================================================================================================
// File: MxMTrajectoryGenerator_AI.cs
// 
// Authors:  Kenneth Claassen
// Date:     11-10-2019: Created this file.
// 
//     Contains a part of the 'MxM' namespace for 'Unity Engine'. 
// ================================================================================================
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;
using Unity.Collections;
using Unity.Mathematics;

namespace MxM
{
    //===========================================================================================
    /**
    *  @brief This class / Unity component (Monobehaviour) is used to generat a trajecotry in
    *  3D space for an MxMAnimator (Motion Matching Animation System) that uses a navmesh for navigation.
    *  
    *  See MxMTrajectoryGenerator.cs for more details on trajectory generators in general.
    *         
    *********************************************************************************************/
    public class MxMTrajectoryGenerator_AI : MxMTrajectoryGeneratorBase
    {
        [Header("Motion Settings")]
        [SerializeField] private float m_maxSpeed = 4f;
        [SerializeField] private float m_moveRate = 15f;
        [SerializeField] private float m_turnRate = 10f;
        [SerializeField] private float m_stoppingDist = 1f;
        [SerializeField] private bool m_strafe = false;
        [SerializeField] private bool m_applyRootSpeedToNavAgent = true;
        [SerializeField] private bool m_faceDirectionOnIdle = false;

        private bool m_hasInputThisFrame;
        private NativeArray<float3> m_newTrajectoryPositions;

        private NavMeshAgent m_navAgent;

        private Vector3[] m_path;

        public bool FaceDirectiononIdle { get { return m_faceDirectionOnIdle; } set { m_faceDirectionOnIdle = value; } }
        public float MaxSpeed { get { return m_maxSpeed; } set { m_maxSpeed = value; } }
        public bool Strafing { get { return m_strafe; } set { m_strafe = value; } }
        public float StoppingDistance { get { return m_stoppingDist; } set { m_stoppingDist = value; } }
        public Vector3 StrafeLookVector { get; set; }

        protected override void Setup(float[] a_predictionTimes) { }

        //===========================================================================================
        /**
        *  @brief Monobehaviour Start function called once before the gameobject is updated.
        *  
        *  Ensures that the trajectory generator has all the handles it needs
        *         
        *********************************************************************************************/
        protected virtual void Start()
        {
            m_navAgent = GetComponentInChildren<NavMeshAgent>();
            Assert.IsNotNull(m_navAgent, "Error: MxMTrajectoryGEnerator_AI - cannot find NavMeshAgent component");

            m_path = new Vector3[10];
        }

        //===========================================================================================
        /**
        *  @brief Monobehaviour update function called every frame. Updates the past trajectory recording
        *         
        *********************************************************************************************/
        public void LateUpdate()
        {
            if (p_animator != null)
            {
                if (m_applyRootSpeedToNavAgent)
                {
                    //While not controlling the character by root motion, we can use the root motion delta
                    //to control the speed of the navAgent to reduce foot sliding somewhat
                    Vector3 v = m_navAgent.velocity * Time.deltaTime;
                    Vector3 animatorDelta = p_animator.deltaPosition;

                    float rootSpeed = 0f;
                    if (Vector3.Angle(animatorDelta, v) < 180)

                    {
                        float vMag = v.magnitude;

                        Vector3 projectedDelta = (Vector3.Dot(animatorDelta, v) / (vMag * vMag)) * v;
                        rootSpeed = projectedDelta.magnitude / Time.deltaTime;
                    }

                    if(!float.IsNaN(rootSpeed))
                        m_navAgent.speed = Mathf.Max(rootSpeed, 0.5f);
                }

            }
            
            UpdatePastTrajectory(Time.deltaTime);
        }

        //===========================================================================================
        /**
        *  @brief Updates the future prediction of the trajectory based on a path provided by a nav
        *  mesh.
        *         
        *********************************************************************************************/
        protected override void UpdatePrediction(float a_deltaTime)
        {
            Vector3 charPosition = transform.position;

            m_newTrajectoryPositions[0] = float3.zero;
            p_trajFacingAngles[0] = 0f;
            int iterations = m_newTrajectoryPositions.Length;

            if (m_navAgent.remainingDistance < m_stoppingDist)
            {
                for (int i = 1; i < iterations; ++i)
                {
                    float percentage = (float)i / (float)(iterations - 1);

                    m_newTrajectoryPositions[i] = math.lerp(p_trajPositions[i], float3.zero,
                        1f - math.exp(-m_moveRate * percentage * a_deltaTime));
                }

                m_hasInputThisFrame = false;
            }
            else
            {
                int pathPointCount = m_navAgent.path.GetCornersNonAlloc(m_path);

                int pathIndex = 1;
                float pathCumDisplacement = 0f;

                float3 from;
                float3 to;
                //float largestFacingAngle = 0f;
                float desiredOrientation = 0f;
                for (int i = 1; i < iterations; ++i)
                {
                    float percentage = (float)i / (float)(iterations - 1);
                    float desiredDisplacement = m_maxSpeed * percentage;

                    //find the desired point along the path.
                    float3 lastPoint = float3.zero;
                    float3 desiredPos = float3.zero;
                    desiredOrientation = 0f;
                    to = m_path[pathIndex - 1] - charPosition;
                    for (int k = pathIndex; k < pathPointCount; ++k)
                    {
                        from = to;
                        to = m_path[k] - charPosition;

                        float displacement = math.length(to - from);

                        if (pathCumDisplacement + displacement > desiredDisplacement)
                        {
                            float lerp = (desiredDisplacement - pathCumDisplacement) / displacement;
                            desiredPos = math.lerp(from, to, lerp);

                            break;
                        }

                        if (k == pathPointCount - 1)
                        {

                            desiredPos = to;
                            desiredOrientation = transform.rotation.eulerAngles.y;
                            break;
                        }

                        pathCumDisplacement += displacement;
                        ++pathIndex;
                    }

                    float3 lastPosition = p_trajPositions[i - 1];

                    float3 adjustedTrajectoryDisplacement = math.lerp(p_trajPositions[i] - lastPosition, desiredPos - lastPosition,
                        1f - math.exp(-m_moveRate * percentage * a_deltaTime));

                    m_newTrajectoryPositions[i] = m_newTrajectoryPositions[i - 1] + adjustedTrajectoryDisplacement;
                }

                if(m_strafe || (m_faceDirectionOnIdle && !m_hasInputThisFrame))
                    desiredOrientation = (Mathf.Atan2(StrafeLookVector.x, StrafeLookVector.z) * Mathf.Rad2Deg);

                //Rotation iteration
                to = m_newTrajectoryPositions[0];
                for (int i = 1; i < m_newTrajectoryPositions.Length; ++i)
                {
                    float percentage = (float)i / (float)(iterations - 1);

                    from = to;
                    to = m_newTrajectoryPositions[i];
                    float3 next = to + (to - from);

                    if (i < m_newTrajectoryPositions.Length - 1)
                        next = m_newTrajectoryPositions[i + 1];

                    if (!m_strafe)
                    {
                        var displacementVector = next - to;
                        desiredOrientation = Vector3.SignedAngle(Vector3.forward, displacementVector, Vector3.up);
                    }

                    if (Vector3.SqrMagnitude(to - from) > 0.05f)
                    {
                        float facingAngle = Mathf.LerpAngle(p_trajFacingAngles[i],
                            desiredOrientation, 1f - math.exp(-m_turnRate * percentage));

                        p_trajFacingAngles[i] = facingAngle;
                    }
                }

                m_hasInputThisFrame = true;
            }

            for(int i = 0; i < iterations; ++i)
            {
                p_trajPositions[i] = m_newTrajectoryPositions[i];
            }       
        }

        //===========================================================================================
        /**
        *  @brief Allocates and initializes any native arrays required for the trajectory generator
        *  to function.
        *         
        *********************************************************************************************/
        protected override void InitializeNativeData()
        {
            base.InitializeNativeData();

            m_newTrajectoryPositions = new NativeArray<float3>(p_trajectoryIterations,
                Allocator.Persistent, NativeArrayOptions.ClearMemory);
        }

        //===========================================================================================
        /**
        *  @brief Disposes any native data that has been created for jobs to avoid memory leaks.
        *         
        *********************************************************************************************/
        protected override void DisposeNativeData()
        {
            base.DisposeNativeData();

            if (m_newTrajectoryPositions.IsCreated)
                m_newTrajectoryPositions.Dispose();
        }

        //===========================================================================================
        /**
        *  @brief Checks if the trajectory generator has movement input or not. The movement input is
        *  usually cached because it is processed during the update and may need to be fetched at a 
        *  later point.
        *  
        *  @return bool - true if there is movement input, false if there is not.
        *         
        *********************************************************************************************/
        public override bool HasMovementInput()
        {
            return m_hasInputThisFrame;
        }


    }//End of class: MxMTrajectoryGenerator_AI
}//End of namespace: MxM