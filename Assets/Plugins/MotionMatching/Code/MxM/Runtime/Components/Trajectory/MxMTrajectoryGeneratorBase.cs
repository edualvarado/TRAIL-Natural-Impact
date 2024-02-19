// ================================================================================================
// File: MxMTrajectoryGeneratorBase.cs
// 
// Authors:  Kenneth Claassen
// Date:     2019-07-2019: Created this file.
// 
//     Contains a part of the 'MxM' namespace for 'Unity Engine'.
// ================================================================================================
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;

namespace MxM
{
    //============================================================================================
    /**
    *  @brief The abstract class / Unity component (MonoBehaviour) should be used as a base class for
    *  creating trajectory generators for 'Motion Matching for Unity' (MxmAnimator). 
    *  
    *  It partially implements the IMxMTrjectory interface to allow an MxMAnimator to extract data
    *  from the trajectory whenever necessary. 
    *  
    *  The purpose of the MxMTrajectoryGeneratorBase is to provide a good starting point for creating
    *  a custom TrajectoryGenerator. It handles all the logic of recording past trajectory and also
    *  extracting and interpolating trajectories requested from the MxMAnimator. To achieve this 
    *  it provides some specific containers that must be filled with trajectory points by whatever
    *  trajectory generator inherits this class.
    *         
    *********************************************************************************************/
    public abstract class MxMTrajectoryGeneratorBase : MonoBehaviour, IMxMTrajectory
    {
        //Serialized
        [Header("Trajectory History")]
        [SerializeField] protected float p_recordingFrequency = 0f; //Time between recording past trajectory points.
        [SerializeField] protected float p_sampleRate = 20f; //Number of samples or steps in calculating the future trajectory (Can be more granular than MxM trajectory configuration)
        [SerializeField] protected bool p_flattenTrajectory = false; //If true, the trajectory will be flattened with no 'Y' component value

        //Tracking
        protected float p_timeHorizon;  //The time horizon is how far in the future to predict the trajectory. This is set by the MxMAnimator
        protected float p_timeStep;     //The amount of time that must pass between future trajectory prediction samples
        protected int p_trajectoryIterations; //The number of iterations to achieve p_sampleRate within p_timeHorizon
        protected float p_curFacingAngle; //The current facing angle of the trajectory
        private bool m_extractedThisFrame; //True if an MxMAnimator extracted a trajectory goal this frame

        //Trajectory
        protected TrajectoryPoint[] p_goal; //The goal, or rather the last extracted trajectory
        protected NativeArray<float3> p_trajPositions; //A native array containing all future predicted trajectory positions of length p_trajectoryIterations
        protected NativeArray<float> p_trajFacingAngles; //A native array containing all future predicted trajectory facing angles of length p_trajectoryIterations
        protected List<float> p_predictionTimes; //The a list of prediction times, past and future. These are the prediction times set in the trajectory config

        //Past
        protected float p_recordingTimer; //A time to ensure past recording frequency is adhered to
        protected float p_maxRecordTime; //The maximum amount of time to record past trajectory for
        protected List<Vector3> p_recordedPastPositions = new List<Vector3>(); //A list of recorded trajectory positions in the character's past (World Space)
        protected List<float> p_recordedPastFacingAngles = new List<float>(); //A list of recorded trajectory facing angles in the character's past (World Space)
        protected List<float> p_recordedPastTimes = new List<float>(); //A list of recorded past times that correlates to p_recordedPastPositions and p_recordedPastFacingAngles

        //Components
        protected MxMAnimator p_mxmAnimator; //A reference to the MxMAnimator that this trajectory generator is paired with
        protected Animator p_animator; //A reference to the animator component
        
        //Job handling
        protected JobHandle p_trajectoryGenerateJobHandle; //A job handle to the trajectory generator job
        protected static int s_curTrajectoryGeneratorId; //A static counter for trajectory generator updates to ensure that the jobs only get batch scheduled after s_curTrajectoryGeneratorId == s_trajectoryGeneratorCount
        protected static int s_trajectoryGeneratorCount; //The number of trajectory generators that are active

        //Properties
        public bool IsPaused { get; private set; } //Used to pause and un-pause the trajectory generator

        //Abstract functions
        protected abstract void UpdatePrediction(float a_deltaTime); //Abstract function to update the future prediction logic. This needs to be overriden with a custom implementation
        public abstract bool HasMovementInput(); //Returns true if there is movement input this frame.
        protected abstract void Setup(float[] a_predictionTimes); //Setup function to be implemented for any custom logic based on the prediction times. It is called at the end of SetGoalRequirements.

        //===========================================================================================
        /**
        *  @brief Monobehaviour Awake function called once when the game object is created and before
        *  Start. This implementation ensure the trajectory generator has a reference to all necessary
        *  components.
        *         
        *********************************************************************************************/
        protected virtual void Awake()
        {
            p_mxmAnimator = GetComponentInChildren<MxMAnimator>();

            Assert.IsNotNull(p_mxmAnimator, "Cannot initialize MxMTrajectoryGeneratorBase with null MxMAnimator" +
                "Add an MxMAnimator component");

            if (p_mxmAnimator.AnimationRoot != null)
            {
                p_animator = p_mxmAnimator.AnimationRoot.GetComponentInChildren<Animator>();
            }
            else
            {
                p_animator = p_mxmAnimator.GetComponent<Animator>();
            }
        }

        //===========================================================================================
        /**
        *  @brief Updates the trajectory logic
        *         
        *********************************************************************************************/
        protected void UpdatePastTrajectory(float a_deltaTime)
        {
            if (!IsPaused)
            {
                m_extractedThisFrame = false;

                RecordPastTrajectory();
                p_curFacingAngle = transform.rotation.eulerAngles.y;

                p_trajectoryGenerateJobHandle.Complete(); //Complete the job before it needs to be used again
                UpdatePrediction(a_deltaTime);

                ++s_curTrajectoryGeneratorId;
                if(s_curTrajectoryGeneratorId == s_trajectoryGeneratorCount)
                {
                    s_curTrajectoryGeneratorId = 0;
                    JobHandle.ScheduleBatchedJobs();
                }
            }
        }

        //===========================================================================================
        /**
        *  @brief Records the past trajectory if the time passed meets the recording frequency requirements
        *         
        *********************************************************************************************/
        protected void RecordPastTrajectory()
        {
            if (Time.time - p_recordingTimer >= p_recordingFrequency)
            {
                p_recordedPastPositions.Insert(0, transform.position);
                p_recordedPastFacingAngles.Insert(0, p_curFacingAngle);

                p_recordingTimer = Time.time;
                p_recordedPastTimes.Insert(0, p_recordingTimer);

                float totalRecordTime = p_recordedPastTimes[0] - p_recordedPastTimes[p_recordedPastTimes.Count - 1];

                if (totalRecordTime > p_maxRecordTime + p_recordingFrequency)
                {
                    p_recordedPastPositions.RemoveAt(p_recordedPastPositions.Count - 1);
                    p_recordedPastFacingAngles.RemoveAt(p_recordedPastFacingAngles.Count - 1);
                    p_recordedPastTimes.RemoveAt(p_recordedPastTimes.Count - 1);
                }
            }
        }

        //===========================================================================================
        /**
        *  @brief In order to generate and record an appropriate trajectory the Trajectory Generator
        *  needs to know what the MxMAnimator requires from it with it's trajectory configuration. This
        *  function is used to do this. It passes an array of floats specificying the trajectory times 
        *  that the Trajectory generator needs to account for. 
        *  
        *  The functoin is called automatically from the MxMAnimator and the array of prediction times
        *  (past and future) comes directly from the trajectory configuration defined in the pre-process
        *  stage.
        *  
        *  @param [float[]] a_predictionTimes -an array of prediction times (past and future) that this
        *  trajectory generator must account for.
        *         
        *********************************************************************************************/
        public void SetGoalRequirements(float[] a_predictionTimes)
        {
            if(a_predictionTimes == null)
            {
                Debug.LogError("MxM Error: Attempting to set goal requirements on IMxMTrajectory with null prediction times.");
                return;
            }

            int predictionLength = a_predictionTimes.Length;

            if (predictionLength == 0)
            {
                Debug.LogError("MxM Error: Attempting to set goal requirements on IMxMTrajectory with no prediction times set.");
                return;
            }

            //From the known prediction times we can determine a time horizon and a time step which is essential for trajectory prediction.
            p_timeHorizon = a_predictionTimes[predictionLength - 1];
            p_sampleRate = Mathf.Max(p_sampleRate, 0.001f);
            p_timeStep = p_timeHorizon / p_sampleRate;

            if (p_predictionTimes == null)
            {
                p_predictionTimes = new List<float>(a_predictionTimes);
            }
            else
            {
                p_predictionTimes.Clear();

                for(int i = 0; i < predictionLength; ++i)
                {
                    p_predictionTimes.Add(a_predictionTimes[i]);
                }
            }

            p_goal = new TrajectoryPoint[predictionLength];
            int pastPredictionCount = 0;

            for (int i = 0; i < predictionLength; ++i)
            {
                if (i == 0 & a_predictionTimes[i] < 0f)
                    p_maxRecordTime = Mathf.Abs(a_predictionTimes[i]);

                if (a_predictionTimes[i] < 0f)
                    ++pastPredictionCount;
            }

            p_trajectoryIterations = Mathf.FloorToInt(p_timeHorizon * p_sampleRate);
            InitializeNativeData();

            //Setup past recording
            if (p_recordingFrequency <= Mathf.Epsilon)
                p_recordingFrequency = 1f / 30f;

            p_recordedPastPositions.Clear();
            p_recordedPastFacingAngles.Clear();
            p_recordedPastTimes.Clear();
            p_recordingTimer = Time.time;
            p_recordedPastPositions.Add(transform.position);
            p_recordedPastFacingAngles.Add(transform.rotation.eulerAngles.y);
            p_recordedPastTimes.Add(p_recordingTimer);

            Setup(a_predictionTimes);
        }

        //===========================================================================================
        /**
        *  @brief This function is called by the MxMAnimator when it requires a goal to run an 
        *  animation search. Essentially it extracts a course set of trajectory points from the granular
        *  recorded past and predicted future. The goal extracted is based on the trajectory configuration
        *  specified in the Pre-Processing stage
        *  
        *  This function is an implementation of IMxMTrajectory
        *         
        *********************************************************************************************/
        public void ExtractGoal()
        {
            p_trajectoryGenerateJobHandle.Complete();

            Vector3 transformPosition = transform.position;
            //Extract data from the trajectory for the goal times
            for (int i = 0; i < p_goal.Length; ++i)
            {
                float timeDelay = p_predictionTimes[i];

                if (timeDelay < 0f) //Past trajectory search
                {
                    int curIndex = 1;
                    float lerp = 0f;
                    float curTime = Time.time;
                    for(int k = 1; k < p_recordedPastTimes.Count; ++k)
                    {
                        float time = p_recordedPastTimes[k];

                        if (time <  curTime + timeDelay)
                        {
                            curIndex = k;


                            float timeError = (curTime + timeDelay) - time;
                            float deltaTime = p_recordedPastTimes[k - 1] - time;

                            lerp = timeError / deltaTime;
                            break; 
                        }
                    }

                    if (curIndex < p_recordedPastTimes.Count)
                    {
                        Vector3 position = Vector3.Lerp(p_recordedPastPositions[curIndex], p_recordedPastPositions[curIndex - 1], lerp);
                        float facingAngle = Mathf.LerpAngle(p_recordedPastFacingAngles[curIndex], p_recordedPastFacingAngles[curIndex - 1], lerp);

                        if (p_flattenTrajectory)
                            position.y = transformPosition.y;

                        //p_goal[i] = new TrajectoryPoint(position - transformPosition, p_recordedPastFacingAngles[curIndex]);
                        p_goal[i] = new TrajectoryPoint(position - transformPosition, facingAngle);
                    }
                    else
                    {
                        p_goal[i] = new TrajectoryPoint();
                    }
                }
                else
                {
                    //int index = Mathf.RoundToInt(timeDelay / p_timeStep);
                    int index = Mathf.RoundToInt(timeDelay / p_timeHorizon * p_trajPositions.Length) - 1;

                    if (index >= p_trajPositions.Length)
                        index = p_trajPositions.Length - 1;

                    Vector3 position = p_trajPositions[index];

                    if (p_flattenTrajectory)
                        position.y = 0f;

                    p_goal[i] = new TrajectoryPoint(position, p_trajFacingAngles[index]);
                }
            }

            m_extractedThisFrame = true;
        }

        //===========================================================================================
        /**
        *  @brief Extracts motion from the trajectory at a specific time in the Trajectory. More 
        *  accurately, this return a tuple with the movement vector and angular delta from the 
        *  character's current transform to the trajectory point specified in the passed time
        *  
        *  @param [float] a_time - the time in the future to extract motion from
        *  
        *  @return Vector3 moveDelta - (Tuple) the delta position between the character and the extracted trajectory point
        *  @return float angleDelta - (Tuple) the delta angle between the character and the extracted trajectory point
        *         
        *********************************************************************************************/
        public (Vector3 moveDelta, float angleDelta) ExtractMotion(float a_time)
        {
            p_trajectoryGenerateJobHandle.Complete();

            a_time = Mathf.Clamp(a_time, 0f, p_predictionTimes[p_predictionTimes.Count - 1]);

            int startIndex = Mathf.FloorToInt(a_time / p_timeStep);
            int endIndex = Mathf.CeilToInt(a_time / p_timeStep);

            float lerp = (a_time - (startIndex * p_timeStep)) / p_timeStep;

            return (Vector3.Lerp(p_trajPositions[startIndex], p_trajPositions[endIndex], lerp),
                Mathf.LerpAngle(p_trajFacingAngles[startIndex], p_trajFacingAngles[endIndex], lerp));
        }

        //===========================================================================================
        /**
        *  @brief Monobehaviour OnDestroy function which ensures all native data has been disposed to
        *  avoid memory leaks.
        *         
        *********************************************************************************************/
        protected virtual void OnDestroy()
        {
            DisposeNativeData();
        }

        //===========================================================================================
        /**
        *  @brief Monobehaviour OnDisable function which is called everytime this component is disabled.
        *  It disposes any native allocated data to avoid memory leaks and removes a counter from the 
        *  trajectory generator count so that jobs will be batched appropriately
        *         
        *********************************************************************************************/
        protected virtual void OnDisable()
        {
            //DisposeNativeData();
            ResetMotion();

            if (!IsPaused)
            {
                --s_trajectoryGeneratorCount;
            }
        }

        //===========================================================================================
        /**
        *  @brief Monobehaviour OnEnable function which is called everytime this component is enabled.
        *  It initializes all native data and adds a count to trajectory generators to ensure that jobs
        *  from all trajectory generators will be batched
        *         
        *********************************************************************************************/
        protected virtual void OnEnable()
        {
            //InitializeNativeData();

            if(!IsPaused)
            {
                ++s_trajectoryGeneratorCount;
            }

            p_mxmAnimator.SetCurrentTrajectoryGenerator(this);
        }

        //===========================================================================================
        /**
        *  @brief Initializes native arrays for use with the MxMTrajectoryGenerator. 
        *         
        *********************************************************************************************/
        protected virtual void InitializeNativeData()
        {
            DisposeNativeData();

            p_trajPositions = new NativeArray<float3>(Mathf.Max(p_trajectoryIterations, 1),
                Allocator.Persistent, NativeArrayOptions.ClearMemory);

            p_trajFacingAngles = new NativeArray<float>(Mathf.Max(p_trajectoryIterations, 1),
                Allocator.Persistent, NativeArrayOptions.ClearMemory);
        }

        //===========================================================================================
        /**
        *  @brief This function disposes any native data that has been allocated to avoid memory leaks
        *         
        *********************************************************************************************/
        protected virtual void DisposeNativeData()
        {
            p_trajectoryGenerateJobHandle.Complete();

            if (p_trajFacingAngles.IsCreated)
                p_trajFacingAngles.Dispose();

            if (p_trajPositions.IsCreated)
                p_trajPositions.Dispose();
        }

        //===========================================================================================
        /**
        *  @brief Pauses the trajectory generator
        *         
        *********************************************************************************************/
        public virtual void Pause()
        {
            IsPaused = true;

            if (enabled)
            {
                --s_trajectoryGeneratorCount;
            }
        }

        //===========================================================================================
        /**
        *  @brief Unpauses the trajectory generator
        *         
        *********************************************************************************************/
        public virtual void UnPause()
        {
            IsPaused = false;

            if (enabled)
                ++s_trajectoryGeneratorCount;
        }

        //===========================================================================================
        /**
        *  @brief 
        *  
        *          *  This function is usually called automatically by the MxMAnimator following an action event 
        *  dependent on the 'PostEventTrajectoryHandling' property (see MxMAnimator.cs).
        *         
        *********************************************************************************************/
        public virtual void CopyGoalFromPose(ref PoseData a_poseData)
        {
            p_trajectoryGenerateJobHandle.Complete();

            if (a_poseData.Trajectory.Length != p_goal.Length)
                return;

            SetGoal(a_poseData.Trajectory);
        }

        //===========================================================================================
        /**
        *  @brief Usually a goal is extrated from the Trajectory Generator. However, it is also possible
        *  to set the goal inversly from the MxMAnimator, with this function. 
        *  
        *  Since the actual recorded and predicted trajectory points are stored in a more granular manner
        *  than the MxMAnimator deals with, these granular trajectories need to be interpolated from
        *  the passed trjaectory goal.
        *  
        *  This function is usually called automatically by the MxMAnimator following an action event 
        *  dependent on the 'PostEventTrajectoryHandling' property (see MxMAnimator.cs).
        *  
        *  This function is an implementation of IMxMTrajectory
        *  
        *  @param [TrajectoryPoint[]] a_trajectory - a list of trajectory points representing a trajectory
        *         
        *********************************************************************************************/
        public virtual void SetGoal(TrajectoryPoint[] a_trajectory)
        {
            int point = 0;
            ref readonly TrajectoryPoint startPoint = ref a_trajectory[point];
            ref readonly TrajectoryPoint nextPoint = ref a_trajectory[point + 1];

            float startTime = p_predictionTimes[point];
            float nextTime = p_predictionTimes[point + 1];

            float curTime = startTime;
            float timeDif = nextTime - startTime;

            bool useControllerPosition = false;

            if (nextTime > 0)
                useControllerPosition = true;

            //Copy over past recordings
            for (int i = 0; i < p_recordedPastPositions.Count; ++i)
            {
                float lerp = curTime / timeDif;

                //Todo: Check that this is in the right space
                if (useControllerPosition)
                {
                    p_recordedPastPositions[i] = Vector3.Lerp(startPoint.Position, Vector3.zero, lerp);
                    p_recordedPastFacingAngles[i] = Mathf.LerpAngle(startPoint.FacingAngle, 0f, lerp);
                }
                else
                {
                    p_recordedPastPositions[i] = Vector3.Lerp(startPoint.Position, nextPoint.Position, lerp);
                    p_recordedPastFacingAngles[i] = Mathf.LerpAngle(startPoint.FacingAngle, nextPoint.FacingAngle, lerp);
                }

                curTime += p_recordingFrequency;

                if (curTime > nextTime)
                {
                    ++point;
                    if (point + 1 < p_predictionTimes.Count)
                    {
                        startPoint = ref a_trajectory[point];
                        nextPoint = ref a_trajectory[point + 1];

                        startTime = p_predictionTimes[point];
                        nextTime = p_predictionTimes[point + 1];

                        timeDif = nextTime - startTime;

                        if (nextTime > 0f)
                        {
                            useControllerPosition = true;
                            timeDif = -startTime;
                        }

                        if (startTime > 0f)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (curTime > 0f)
                    break;
            }

            curTime = 0f;
            useControllerPosition = true;
            timeDif = nextTime;


            //Copy over future predictions
            //for (int i = 0; i < p_trajPositions.Count; ++i)
            for (int i = 0; i < p_trajPositions.Length; ++i)
            {
                float lerp = curTime / timeDif;

                if(useControllerPosition)
                {
                    p_trajPositions[i] = Vector3.Lerp(Vector3.zero, nextPoint.Position, lerp);
                    p_trajFacingAngles[i] = Mathf.LerpAngle(0f, nextPoint.FacingAngle, lerp);
                }
                else
                {
                    p_trajPositions[i] = Vector3.Lerp(startPoint.Position, nextPoint.Position, lerp);
                    p_trajFacingAngles[i] = Mathf.LerpAngle(startPoint.FacingAngle, nextPoint.FacingAngle, lerp);
                }

                curTime += p_timeStep;

                if (curTime > nextTime)
                {
                    ++point;

                    if (point + 1 < p_predictionTimes.Count)
                    {
                        startPoint = ref a_trajectory[point];
                        nextPoint = ref a_trajectory[point + 1];

                        startTime = p_predictionTimes[point];
                        nextTime = p_predictionTimes[point + 1];

                        timeDif = nextTime - startTime;

                        useControllerPosition = false;

                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        //===========================================================================================
        /**
        *  @brief This function resets the 'Motion' on the trajectory generator. Essentially it sets
        *  all past and future predicted points to zero.  It is also an implementation of IMxMTrajectory
        *  
        *  This function is normally called automatically from the MxMAnimator after an event dependent
        *  on the method used 'PostEventTrajectoryHandling' (See MxMAnimator.cs). However, it can also
        *  be used to stomp all trajectory for whatever reason (e.g. when teleporting the character, 
        *  it may be useful to stomp trajectory so they don't do a running stop after teleportation)
        *         
        *********************************************************************************************/
        public virtual void ResetMotion(float a_rotation=0f)
        {
            Vector3 zeroVector = Vector3.zero;
            m_extractedThisFrame = false;

            p_trajectoryGenerateJobHandle.Complete();
            for (int i = 0; i < p_trajFacingAngles.Length; ++i)
            {
                p_trajFacingAngles[i] = a_rotation;
                p_trajPositions[i] = zeroVector;
            }

            zeroVector = transform.position;

            for (int i = 0; i < p_recordedPastFacingAngles.Count; ++i)
            {
                p_recordedPastFacingAngles[i] = a_rotation;
                p_recordedPastPositions[i] = zeroVector;
            }
        }
        
        //===========================================================================================
        /**
        *  @brief
        * 
        *********************************************************************************************/
        public void ForcePastTrajectoryByVelocity(Vector3 a_velocity)
        {
            int pastRecordCount = Mathf.CeilToInt(p_maxRecordTime / p_recordingFrequency);

            p_recordedPastPositions.Capacity = pastRecordCount;
            p_recordedPastFacingAngles.Capacity = pastRecordCount;
            
            Vector3 trajectoryStartPosition = transform.position;
            Vector3 incrementVelocityVector = a_velocity * p_recordingFrequency * -1f;

            float facingAngle = Vector3.SignedAngle(Vector3.forward, a_velocity.normalized, Vector3.up);

            for (int i = 0; i < pastRecordCount; ++i)
            {
                p_recordedPastPositions[i] = trajectoryStartPosition + (incrementVelocityVector * i);
                p_recordedPastFacingAngles[i] = facingAngle;
            }

            p_recordingTimer = Time.time;
        }

        //===========================================================================================
        /**
        *  @brief IMxMTrajectory implemented function which returns the transform of the game object
        *  
        *  @return Transform - the transform component of the game object
        *         
        *********************************************************************************************/
        public Transform GetTransform()
        {
            return gameObject.transform;
        }

        //===========================================================================================
        /**
        *  @brief Returns a list of trajectory points for the current goal. If it has not yet been
        *  extracted, the current goal will be first extracted and then returned. 
        *  
        *  Extraction only occurs once per frame and is cached for performance.
        *  
        *  @return TrajectoryPoint[] - a list of trajectory points representing the goal
        *         
        *********************************************************************************************/
        public TrajectoryPoint[] GetCurrentGoal()
        {
            if(!m_extractedThisFrame)
                ExtractGoal();

            return p_goal;
        }

        //===========================================================================================
        /**
        *  @brief Returns the enabled value of the trajectory generator. This is an implementation of
        *  IMxMTrajectory so that the MxMAnimator can access the enabled status from the interface 
        *  alone.
        *  
        *  @return bool - whether or not the trajectory generator is enabled
        *         
        *********************************************************************************************/
        public bool IsEnabled()
        {
            return enabled;
        }

    }//End of class: MxMTrajectoryGeneratorBase
}//End of namespace: MxM
