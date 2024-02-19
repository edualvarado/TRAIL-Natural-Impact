using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MxM
{
    [System.Serializable]
    public struct TurnInPlaceProfile
    {
        [SerializeField]
        public string EventName; //Without the _Left or _Right prefix

        [SerializeField]
        public Vector2 TriggerRange;

        [SerializeField]
        public bool Warp;

        //[SerializeField]
        //public bool WarpTime;

        //[SerializeField]
        //public float DesiredTime;

        [System.NonSerialized]
        [HideInInspector]
        public int LeftEventHandle;

        [System.NonSerialized]
        [HideInInspector]
        public int RightEventHandle;

        public void FetchEventHandles(MxMAnimator a_mxmAnimator)
        {
            if (a_mxmAnimator == null)
                return;

            LeftEventHandle = a_mxmAnimator.CurrentAnimData.EventIdFromName(EventName + "_Left");
            RightEventHandle = a_mxmAnimator.CurrentAnimData.EventIdFromName(EventName + "_Right");
        }
    }


    [DisallowMultipleComponent]
    [RequireComponent(typeof(MxMAnimator))]
    public class MxMTIPExtension : MonoBehaviour, IMxMExtension
    {
        [SerializeField] 
        private bool m_useCustomTIPVector = false;
        
        [SerializeField]
        private float m_blendTime = 0.5f;

        [SerializeField]
        private TurnInPlaceProfile[] m_turnInPlaceProfiles = null;
        
        private float m_minAngleToTriggerTIP;

        private MxMAnimator m_mxmAnimator;
        private MxMTrajectoryGenerator m_trajectoryGenerator;

        private MxMEventDefinition m_eventDef;

        private float m_mxmBlendTime = 0.25f;

        public Vector3 TIPVector { get; set; } 

        private int CurrentTurnProfileId { get; set; }
        private int CurrentTurnEventHandle { get; set; }
        public bool IsEnabled { get { return enabled; } }
        public bool DoUpdatePhase1 { get { return true; } }
        public bool DoUpdatePhase2 { get { return false; } }
        public bool DoUpdatePost { get { return false; } }

        public void Initialize()
        {
            CurrentTurnProfileId = -1;

            //Validate that components and data is all setup correctly
            m_mxmAnimator = GetComponent<MxMAnimator>();
            if(m_mxmAnimator == null)
            {
                Debug.LogError("Could not find MxMAnimator component, MxM TIP Extension" +
                    "component disabled.");
                enabled = false;
                return;
            }

            m_mxmBlendTime = m_mxmAnimator.BlendTime;

            m_trajectoryGenerator = GetComponent<MxMTrajectoryGenerator>();
            if(m_trajectoryGenerator == null)
            {
                Debug.LogError("Could not find MxMTrajectoryGeneratorBase component, MxM TIP " +
                    "Extension component disabled");
                enabled = false;
                return;
            }

            if (m_turnInPlaceProfiles == null || m_turnInPlaceProfiles.Length == 0)
            {
                Debug.LogError("There are no turn in place profiles specified in the MxM TIP Extension. Component Disabled");
                enabled = false;
                return;
            }

            //Setup Event Definition
            m_eventDef = ScriptableObject.CreateInstance<MxMEventDefinition>();
            m_eventDef.Priority = -1;
            m_eventDef.ContactCountToMatch = 0;
            m_eventDef.ContactCountToWarp = 1;
            m_eventDef.EventType = EMxMEventType.Standard;
            m_eventDef.ExitWithMotion = true;
            m_eventDef.MatchPose = true;
            m_eventDef.MatchTrajectory = false;
            m_eventDef.PostEventTrajectoryMode = EPostEventTrajectoryMode.Reset;
            m_eventDef.MatchPosition = false;
            m_eventDef.MatchTiming = false;
            m_eventDef.MatchRotation = false;
            m_eventDef.RotationWarpType = EEventWarpType.Linear;
            m_eventDef.RotationWeight = 10f;

            //Go through each TIP profile, determine the minimum TIP trigger
            //And fetch Event Handles for each profile
            m_minAngleToTriggerTIP = float.MaxValue;
            for(int i = 0; i < m_turnInPlaceProfiles.Length; ++i)
            {
                ref TurnInPlaceProfile profile = ref m_turnInPlaceProfiles[i];

                if (profile.TriggerRange.x < m_minAngleToTriggerTIP)
                    m_minAngleToTriggerTIP = profile.TriggerRange.x;

                profile.FetchEventHandles(m_mxmAnimator);
            }
        }

        public void UpdatePhase1()
        {
            m_trajectoryGenerator.ExtractGoal();

            if(CurrentTurnProfileId > -1) //A turn-in-place event is already playing
            {
                //Cancel the turn-in-place immediately if the player wants to move
                if (m_trajectoryGenerator.InputVector.sqrMagnitude > 0.0001f) 
                {
                    m_mxmAnimator.ForceExitEvent();
                    CurrentTurnProfileId = -1;
                    m_mxmAnimator.BlendTime = m_mxmBlendTime;
                }
                else
                {
                    //Reset the current turn profile to -1 if the turn in place has finished playing
                    if (!m_mxmAnimator.CheckEventPlaying(CurrentTurnEventHandle))
                    {
                        CurrentTurnProfileId = -1;
                        m_mxmAnimator.BlendTime = m_mxmBlendTime;
                    }

                    //TIP can begin from the The FollowThrough stage of another turn-in-place event
                    if (m_mxmAnimator.CurrentEventState == EEventState.FollowThrough)
                        DetectAndTurn();
                }
            }
            else //TIP can begin from the Idle state of MxM
            {
                if (m_mxmAnimator.IsIdle)
                    DetectAndTurn();
            }
        }

        private void DetectAndTurn()
        {
            //We can only start turning if we are beyond the min turn threshold

            float rawAngle = 0f;

            if (m_useCustomTIPVector)
            {
                rawAngle = Vector3.SignedAngle(Vector3.forward, TIPVector, Vector3.up) -
                           transform.rotation.eulerAngles.y;
            }
            else
            {
                rawAngle = m_trajectoryGenerator.DesiredOrientation - transform.rotation.eulerAngles.y;
            }
            
            Quaternion rotation = Quaternion.AngleAxis(rawAngle, Vector3.up);
            rawAngle = rotation.eulerAngles.y;

            if (rawAngle > 180.0f)
                rawAngle -= 360f;

            bool left = rawAngle < 0.0f;
            float angle = Mathf.Abs(rawAngle);

            if (angle < m_minAngleToTriggerTIP)
                return; //We don't search for TIP if it doesn't meet minimum requirements

            //We can now search for the appropriate TIP profile to use
            for (int i = 0; i < m_turnInPlaceProfiles.Length; ++i)
            {
                ref TurnInPlaceProfile profile = ref m_turnInPlaceProfiles[i];

                if (angle > profile.TriggerRange.x
                    && angle < profile.TriggerRange.y)
                {
                    //A turn profile match was found. Trigger it appropriately
                    CurrentTurnProfileId = i;

                    CurrentTurnEventHandle = left ? profile.LeftEventHandle : profile.RightEventHandle;

                    m_eventDef.Id = CurrentTurnEventHandle;
                    //if (profile.WarpTime)
                    //    m_eventDef.TimingWarpType = EEventWarpType.Linear;
                    //else
                    //    m_eventDef.TimingWarpType = EEventWarpType.None;

                    //m_eventDef.DesiredDelay = profile.DesiredTime;

                    if (profile.Warp)
                        m_eventDef.RotationWarpType = EEventWarpType.Linear;
                    else
                        m_eventDef.RotationWarpType = EEventWarpType.None;

                    m_mxmAnimator.BlendTime = m_blendTime;

                    m_eventDef.ClearContacts();
                    m_eventDef.AddEventContact(transform.position, transform.rotation.eulerAngles.y + rawAngle);
                    m_mxmAnimator.BeginEvent(m_eventDef);

                    break;
                }
            }
        }

        public void Terminate() { }
        
        public void UpdatePhase2() { }

        public void UpdatePost() { }

    }//End of class: MxMTIPExtension
}//End of Namespace: MxM