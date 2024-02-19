// ================================================================================================
// File: MxMAnimator_Events.cs
// 
// Authors:  Kenneth Claassen
// Date:     2019-10-10: Created this file.
// 
//     Contains a part of the 'MxM' namespace for 'Unity Engine'.
// ================================================================================================
using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Playables;

namespace MxM
{
    //============================================================================================
    /**
    *  @brief This is partial implementation of the MxMAnimator. This particular partial class 
    *  handles all event system logic for the MxMAnimator.
    *         
    *********************************************************************************************/
    public partial class MxMAnimator : MonoBehaviour
    {
        //Events
        private EventData m_curEvent; //The pre-processed data for the current event
        private EMxMEventType m_eventType; //The type or behavior of this event
        private EEventState m_curEventState; //The 'state' or 'phase' of the current event (i.e. windup, action, followthrough, recovery)
        private float m_timeSinceEventTriggered; //The amount of time that has passed since the event triggerd (modified by speed mods)
        private float m_eventLength; //The length of the current event
        private int m_curEventPriority; //The priority of the current event. Events with higher priority will not get cancelled by lower priority events
        private bool m_exitEventWithMotion; //Whether the event can automatically exit during the recovery phase with motion input
        private int m_contactCountToWarp = 1; //The number of contacts to warp to
        private EventContact m_currentEventRootWorld; //The character root position in world space at which the animation will contact if no warping is to take place this frame
        private EventContact m_desiredEventRootWorld; //The desired character root position in world space for the next contact point.
        public EventContact[] CurEventContacts { get; private set; }
        private float m_eventStartTimeOffset; //Timing offset of the event starting pose from the start of the event windup
        private int m_curEventContactIndex = 0; //The index of the event contact that is currently being warped to
        private float m_curEventContactTime = 0f;
        //private float m_timeWarpFactor = 1f; //The amount of time warping used as a multiplier for animation playback
        private Vector3 m_linearWarpRate; //The amount of linear warping required this frame to reach the contact point in the remaining warp time
        private float m_linearWarpRotRate; //The amount of linear rotational warping required this frame to reach the contact point rotation in the remaining warp time
        private float m_cumActionDuration = 0f; //The cumulative action duration of the event (sum of the time for all action phases)
        public EPostEventTrajectoryMode PostEventTrajectoryMode { get; set; } //The method by which trajectory should be handled immediately follwing event.
        private bool m_warpTimeScaling; //Should playback speed be scaled during event animation warping.
        private int m_contactCountToTimeScale; //The number of contacts to time scale with warping
        private float m_minWarpTimeScaling; //The minimum limit of time scale warping
        private float m_maxWarpTimeScaling; //The maximum limit of time scale warping
        private float m_eventSpeedMod = 1f; //The current speed modification to events to account for time warping
        

        public ref readonly EventData CurrentEvent { get { return ref m_curEvent; } } //Get a handle to the current event data
        public EEventState CurrentEventState { get { return m_curEventState; } } //Returns the current state of a playing event if there is one.
        
        [Obsolete("This property is deprecated and will be removed in a future release. Please use NextEventContactRoot_Actual_World instead.")]
        public ref readonly EventContact NextEventContact_Actual_World { get { return ref m_currentEventRootWorld; } } //Returns the current EventContact point in world space if no warping occurs
        
        [Obsolete("This property is deprecated and will be removed in a future release. Please use NextEventContactRoot_Desired_World instead.")]
        public ref readonly EventContact NextEventContact_Desired_World { get { return ref m_desiredEventRootWorld; } } //Returns the desired EventContact point in world space
        
        public ref readonly EventContact NextEventContactRoot_Actual_World { get { return ref m_currentEventRootWorld; }}//Returns the current EventContact point in world space if no warping occurs
        public ref readonly EventContact NextEventContactRoot_Desired_World { get { return ref m_desiredEventRootWorld; } } //Returns the desired EventContact point in world space
        
        public EEventWarpType WarpType { get; set; } //The type / method of positional warping to use
        public EEventWarpType RotWarpType { get; set; }//The type / method of rotation warping to use
        public EEventWarpType TimeWarpType { get; set; }//The type / method of time warping to use
        public float EventStartTimeOffset { get { return m_eventStartTimeOffset; } } //Returns the chosen start time offset within the Windup phase (i.e. the time from the start of the windup phase to the point that was chosen to start)

        /** Allows user to extract animation warping information */
        public Vector3 PosWarpThisFrame { get; private set; }
        public float RotWarpThisFrame { get; private set; }

        //Returns true if the current event is complete. Basically if no in the event state.
        public bool IsEventComplete
        {
            get
            {
                if ((EMxMStates)m_fsm.CurrentStateId != EMxMStates.Event)
                    return true;

                return false;
            }
        }

        public bool IsEventPlaying
        {
            get
            {
                if ((EMxMStates)m_fsm.CurrentStateId == EMxMStates.Event)
                    return true;

                return false;
            }
        }

        //Returns the duration of the current event
        public float CurrentEventDuration
        {
            get
            {
                if ((EMxMStates)m_fsm.CurrentStateId == EMxMStates.Event)
                {
                    float eventDuration = m_curEvent.Windup + m_curEvent.FollowThrough
                        + m_curEvent.Recovery - m_eventStartTimeOffset;

                    foreach (float actionTime in m_curEvent.Actions)
                    {
                        eventDuration += actionTime;
                    }

                    return eventDuration;
                }

                return 0f;
            }
        }

        //Returns the length of the recovery phase of this event
        public float CurrentEventRecovery
        {
            get
            {
                if ((EMxMStates)m_fsm.CurrentStateId == EMxMStates.Event)
                {
                    return m_curEvent.Recovery;
                }

                return 0f;
            }
        }

        //Returns the time from the start of the event to the last contact
        public float TimeToLastContact
        {
            get
            {
                if (m_fsm.CurrentStateId == (uint)EMxMStates.Event)
                {
                    return m_curEvent.Length - m_eventStartTimeOffset - m_curEvent.Recovery - m_curEvent.FollowThrough;
                }
                else
                {
                    return 0f;
                }
            }
        }

        //============================================================================================
        /**
        *  @brief Updates the event state of the MxMAnimator
        *         
        *********************************************************************************************/
        private void UpdateEvent()
        {
#if UNITY_EDITOR
            m_updateThisFrame = true;
#endif

            //Debug Pose
           // if (m_debugCurrentPose)
            ComputeCurrentPose();

            if (m_curEvent.Actions == null)
            {
                ForceExitEvent();
                return;
            }

            //Track and change event state
            switch (m_curEventState)
            {
                case EEventState.Windup: //Update the windup phase of the event
                    {
                        if (m_timeSinceEventTriggered >= m_curEvent.Windup - m_eventStartTimeOffset)
                        {
                            m_curEventState = EEventState.Action;
                            m_curEventContactIndex = 0;
                            m_curEventContactTime = m_curEvent.TimeToHit - m_eventStartTimeOffset;
                            m_onEventStateChanged.Invoke(m_curEventState);
                        }
                    }
                    break;
                case EEventState.Action: //Update the action phase of the event
                    {
                        if (m_timeSinceEventTriggered >= m_curEventContactTime)
                        {
                            ++m_curEventContactIndex;
                            TimeWarpType = EEventWarpType.None;

                            SnapToContact();

                            //Turn off warping if we are passed the contact count to warp
                            if (m_curEventContactIndex >= m_contactCountToWarp)
                            {
                                WarpType = EEventWarpType.None;
                                TimeWarpType = EEventWarpType.None;
                                RotWarpType = EEventWarpType.None;
                            }

                            //Move onto the next contact if there are multiple contacts in the event
                            if (m_curEventContactIndex < m_curEvent.Actions.Length && m_curEventContactIndex < m_curEvent.RootContactOffset.Length)
                            {
                                m_cumActionDuration += m_curEvent.Actions[m_curEventContactIndex];

                                if (WarpType != EEventWarpType.None && m_curEventContactIndex < CurEventContacts.Length)
                                {
                                    if (CurEventContacts == null)
                                    {
                                        WarpType = EEventWarpType.None;
                                        RotWarpType = EEventWarpType.None;
                                    }
                                    else
                                    {
                                        ref readonly EventContact contactLocal = ref CurEventContacts[m_curEventContactIndex];
                                        ref readonly EventContact rootContactOffsetLocal = ref m_curEvent.RootContactOffset[m_curEventContactIndex];
                                        ref readonly EventContact subEventContactLocal = ref m_curEvent.SubEventContactOffsets[m_curEventContactIndex - 1];
                                        Quaternion contactOrient = Quaternion.AngleAxis(contactLocal.RotationY, Vector3.up);

                                        m_desiredEventRootWorld.RotationY = contactLocal.RotationY + rootContactOffsetLocal.RotationY;
                                        m_currentEventRootWorld.RotationY = subEventContactLocal.RotationY + m_animationRoot.rotation.eulerAngles.y + rootContactOffsetLocal.RotationY;

                                        if (RotWarpType == EEventWarpType.Snap)
                                        {
                                            float error = Mathf.DeltaAngle(m_currentEventRootWorld.RotationY, m_desiredEventRootWorld.RotationY);
                                            m_animationRoot.Rotate(Vector3.up, error);
                                        }

                                        m_desiredEventRootWorld.Position = contactLocal.Position + (contactOrient * rootContactOffsetLocal.Position);
                                        m_currentEventRootWorld.Position = m_animationRoot.TransformPoint(subEventContactLocal.Position + rootContactOffsetLocal.Position);

                                        if (WarpType == EEventWarpType.Snap)
                                        {
                                            m_animationRoot.Translate(m_desiredEventRootWorld.Position - m_currentEventRootWorld.Position);
                                        }

                                        if (m_warpTimeScaling && m_contactCountToTimeScale > m_curEventContactIndex)
                                        {
                                            Vector3 rootPosition = m_animationRoot.position;

                                            //determine the scale and apply it.
                                            float vectorToCurrentContactMagnitude_Local = m_animationRoot.InverseTransformPoint(m_currentEventRootWorld.Position - rootPosition).magnitude;
                                            float vectorToDesiredContactMagnitude_Local = m_animationRoot.InverseTransformPoint(m_desiredEventRootWorld.Position - rootPosition).magnitude;

                                            if (vectorToDesiredContactMagnitude_Local > Mathf.Epsilon)
                                            {
                                                DesiredPlaybackSpeed = Mathf.Clamp(Mathf.Abs(vectorToCurrentContactMagnitude_Local / vectorToDesiredContactMagnitude_Local),
                                                    m_minWarpTimeScaling, m_maxWarpTimeScaling);
                                            }
                                        }
                                    }
                                }

                                m_curEventContactTime += m_curEvent.Actions[m_curEventContactIndex];

                                m_onEventContactReached.Invoke(m_curEventContactIndex);
                            }
                            else
                            {
                                m_curEventContactIndex = 0;

                                m_curEventState = EEventState.FollowThrough;
                                WarpType = EEventWarpType.None;
                                RotWarpType = EEventWarpType.None;
                                TimeWarpType = EEventWarpType.None;

                                m_onEventContactReached.Invoke(m_curEventContactIndex);
                                m_onEventStateChanged.Invoke(m_curEventState);
                            }
                        }
                    }
                    break;
                case EEventState.FollowThrough: //Update the follow through phase of the event
                    {
                        float timePassed = m_curEventContactTime + m_curEvent.FollowThrough;

                        if (m_timeSinceEventTriggered >= timePassed)
                        {
                            if (m_eventType == EMxMEventType.Sequence)
                            {
                                float rewindAmount = m_timeSinceEventTriggered - timePassed + m_curEvent.FollowThrough;
                                for (int i = 0; i < m_curEvent.Actions.Length; ++i)
                                {
                                    m_timeSinceEventTriggered += m_curEvent.Actions[i];
                                }
                                
                                var playableClip = m_animationMixer.GetInput(m_dominantBlendChannel);

                                float desiredTime = (float)playableClip.GetTime() - rewindAmount;
                                playableClip.SetTime(desiredTime);
                                playableClip.SetTime(desiredTime);

                                m_timeSinceEventTriggered -= rewindAmount;

                                m_curEventState = EEventState.Action;
                                m_curEventContactIndex = 0;
                                m_curEventContactTime = m_curEvent.TimeToHit - m_eventStartTimeOffset;
                                m_onEventStateChanged.Invoke(m_curEventState);
                            }
                            else
                            {
                                --m_curEventPriority;
                                m_curEventState = EEventState.Recovery;
                                m_onEventStateChanged.Invoke(m_curEventState);
                            }
                        }
                    }
                    break;
                case EEventState.Recovery: //Update the recovery phase of the event
                    {
                        float timePassed = m_eventLength - m_eventStartTimeOffset;

                        if (m_timeSinceEventTriggered >= timePassed)
                        {
                            if (m_eventType == EMxMEventType.Loop)
                            {
                                ref PoseData startPose = ref CurrentAnimData.Poses[m_curEvent.StartPoseId];

                                AnimationClip clip = CurrentAnimData.Clips[startPose.PrimaryClipId];

                                m_chosenPose = startPose;
                                m_timeSinceEventTriggered -= timePassed;

                                m_eventStartTimeOffset = 0f;

                                if (!clip.isLooping)
                                {
                                    JumpToPose(ref m_chosenPose);
                                }

                                ++m_curEventPriority;
                                m_curEventState = EEventState.Windup;
                            }
                            else
                            {
                                m_onEventComplete.Invoke();
                                ComputeCurrentPose();
                                PostEventTrajectoryHandling();
                                ExitEvent();
                            }
                        }
                        else if (m_exitEventWithMotion && p_trajectoryGenerator.HasMovementInput())
                        {
                            m_onEventComplete.Invoke();
                            ComputeCurrentPose();
                            PostEventTrajectoryHandling();
                            ExitEvent();
                            
                        }
                    }
                    break;
            }

            int currentAnimId = CurrentInterpolatedPose.AnimId;
            float clipSpeed = 1.0f;
            if (currentAnimId > -1 && currentAnimId < CurrentAnimData.ClipsData.Length)
            {
                clipSpeed = CurrentAnimData.ClipsData[CurrentInterpolatedPose.AnimId].PlaybackSpeed;
            }

            m_timeSinceEventTriggered += p_currentDeltaTime * m_playbackSpeed * m_eventSpeedMod * clipSpeed;

        }

        //============================================================================================
        /**
        *  @brief Snaps the current animation instantly at the current frame so that the contact point
        *  in the event data matches with the world event contact instantly.
        *         
        *********************************************************************************************/
        private void SnapToContact()
        {
            //Snap character position to ensure 100% precise contact at this frame
            if (WarpType == EEventWarpType.Linear || WarpType == EEventWarpType.Dynamic)
            {
                if (m_rootMotion != null)
                {
                    m_rootMotion.SetPosition(m_desiredEventRootWorld.Position);
                }
                else
                {
                    m_animationRoot.position = m_desiredEventRootWorld.Position;
                }
            }

            //Snap character rotation to ensure 100% precise contact at this frame
            if (RotWarpType == EEventWarpType.Linear || RotWarpType == EEventWarpType.Dynamic)
            {
                if (m_rootMotion != null)
                {
                    m_rootMotion.SetRotation(Quaternion.AngleAxis(m_desiredEventRootWorld.RotationY, Vector3.up));
                }
                else
                {
                    m_animationRoot.rotation = Quaternion.AngleAxis(m_desiredEventRootWorld.RotationY, Vector3.up);
                }
            }
        }

        //============================================================================================
        /**
        *  @brief Updates event warping logic for events. This function is called from OnAnimatorMove
        *  so that it can modify root motion.
        *  
        *  @param [out Vector3] warp - the output warp value;
        *  @param [out Quaternion] warpRot - the output rotational warp value;
        *         
        *********************************************************************************************/
        private void UpdateEventWarping(out Vector3 a_warp, ref EventFrameData a_lookupData)
        {
            a_warp = Vector3.zero;

            //Recalculate the current root position at the next contact (without warping) 
            
            m_currentEventRootWorld.Position = m_animationRoot.TransformPoint(a_lookupData.relativeContactRoot);

            //Positional Warping
            if (a_lookupData.WarpPosThisFrame)
            {
                float remainingWarpTime = a_lookupData.RemainingWarpTime / (m_playbackSpeed * m_eventSpeedMod);

                switch (WarpType)
                {
                    case EEventWarpType.Linear: { a_warp = ComputeLinearEventWarping(remainingWarpTime); } break;
                    case EEventWarpType.Dynamic: { a_warp = ComputeSimpleDynamicEventWarping(remainingWarpTime); } break;
                }
            }
        }

        private void UpdateEventRotWarping(out Quaternion a_warpRot, ref EventFrameData a_lookupData)
        {
            a_warpRot = Quaternion.identity;

            //Recalculate the current root rotation at the next contact (Without warping)
            m_currentEventRootWorld.RotationY = m_animationRoot.rotation.eulerAngles.y + a_lookupData.relativeContactRootRotY;

            //Rotational warping
            if (a_lookupData.WarpRotThisFrame)
            {
                float remainingWarpTime = a_lookupData.RemainingRotWarpTime / (m_playbackSpeed * m_eventSpeedMod);

                //Rotational Warping
                switch (RotWarpType)
                {
                    case EEventWarpType.Linear: { a_warpRot = ComputeLinearEventRotationWarping(remainingWarpTime); } break;
                    case EEventWarpType.Dynamic: { a_warpRot = ComputeSimpleDynamicEventRotationWarping(remainingWarpTime); } break;
                }
            }
        }

        //============================================================================================
        /**
        *  @brief Searches the events lookup table for relevent frame data, interpolating it if
        *  necessary.
        *  
        *  @param [out EventFrameData] lookupData - rerference to the lookup data to output.
        *         
        *********************************************************************************************/
        private void FetchEventLookupData(out EventFrameData lookupData)
        {
            //Find warping lookup table data within the current event based on event timing
            float lookupTableFloatIndex = (m_timeSinceEventTriggered - (p_currentDeltaTime * m_playbackSpeed * m_eventSpeedMod) + m_eventStartTimeOffset) / (1f / 60f);
            int lookupTableIndex = Mathf.Clamp(Mathf.FloorToInt(lookupTableFloatIndex), 0, m_curEvent.WarpingLookupTable.Length - 1);

            lookupData = new EventFrameData();

            if (m_curEvent.WarpingLookupTable.Length > 0)
            {
                lookupData = m_curEvent.WarpingLookupTable[lookupTableIndex];

                if (lookupTableIndex < m_curEvent.WarpingLookupTable.Length - 1)
                {
                    //Interpolate the lookup table data between the two closest data points
                    float lookupLerp = (lookupTableFloatIndex - (float)lookupTableIndex);
                    lookupData = new EventFrameData(ref m_curEvent.WarpingLookupTable[lookupTableIndex],
                        ref m_curEvent.WarpingLookupTable[lookupTableIndex + 1], lookupLerp);
                }
            }
        }

        //============================================================================================
        /**
        *  @brief Computes the amount of linear rotational warping required for event warping this frame
        *  
        *  The linear method warps the root rotation linearly to reach contact rotations. It does this 
        *  by calculating the total rotation error between the current animation contact point and the
        *  desired. This error is then divided over the 'warpable' time remaining to the contact point 
        *  and applied. Note that the error, and hence warp, is re-calculated every frame to compensate
        *  for positional warping and other factors that might move / affect the transform of the 
        *  character.
        *  
        *  @param [float] remainingWarpTime - the remaining allowable time to warp to the next contact
        *         
        *********************************************************************************************/
        private Quaternion ComputeLinearEventRotationWarping(float a_remainingWarpTime)
        {
            float error = Mathf.DeltaAngle(m_currentEventRootWorld.RotationY, m_desiredEventRootWorld.RotationY);
            float warpRotY = 0f;

            if (a_remainingWarpTime > Mathf.Epsilon)
            {
                m_linearWarpRotRate = error / a_remainingWarpTime;
                
                float remainingWarp = Mathf.DeltaAngle(m_currentEventRootWorld.RotationY, m_desiredEventRootWorld.RotationY);

                //This stops the warp from overshooting the target
                if (Mathf.Abs(m_linearWarpRotRate * p_currentDeltaTime) > Mathf.Abs(remainingWarp))
                {
                    warpRotY = remainingWarp;
                }
                else
                {
                    warpRotY = m_linearWarpRotRate * p_currentDeltaTime;
                }
            }
            else
            {
                //Avoid divide by zero. If the remaining warp time is 0 then just warp the entire error.
                warpRotY = error;
            }

            m_currentEventRootWorld.RotationY += warpRotY;
            return Quaternion.AngleAxis(warpRotY, Vector3.up);
        }

        //============================================================================================
        /**
        *  @brief Computes the amount of dynami rotational warping required for event warping this frame.
        *  
        *  The dynamic warp works differently to the linear warp in the way that warping is based purely
        *  by scaling up or down the motion that already exists in the current frame. This is dependent
        *  on the remaining motion and the desired remaining motion. This gives a more realistic warp. In this
        *  case warping operates on a 'per-axis' basis. If there is 0 remaining motion on any given axis 
        *  then this method reverts to linear warping.
        *  
        *  @param [float] remainingWarpTime - the remaining allowable time to warp to the next contact
        *         
        *********************************************************************************************/
        private Quaternion ComputeSimpleDynamicEventRotationWarping(float a_remainingWarpTime)
        {
            float rotationY = m_animationRoot.rotation.eulerAngles.y;
            float curRotationY = Mathf.DeltaAngle(rotationY, p_animator.rootRotation.eulerAngles.y);
            float desiredRemainingRotationY = Mathf.DeltaAngle(rotationY, m_desiredEventRootWorld.RotationY);
            float remainingRotationY = Mathf.DeltaAngle(rotationY, m_currentEventRootWorld.RotationY);

            float warpRotY = 0f;

            if (a_remainingWarpTime > Mathf.Epsilon)
            {
                if (Mathf.Abs(remainingRotationY) > 0.0001f)
                {
                    warpRotY = (curRotationY / remainingRotationY) * desiredRemainingRotationY - warpRotY;
                }
                else //Revert to linear
                {
                    warpRotY = (desiredRemainingRotationY / a_remainingWarpTime) * p_currentDeltaTime;
                }


                //The warp must always be in the direction of the desired contact rotation. This statement ensures that is the case
                if (Mathf.Abs(Mathf.DeltaAngle(m_currentEventRootWorld.RotationY + warpRotY, m_desiredEventRootWorld.RotationY))
                    > Mathf.Abs(Mathf.DeltaAngle(m_currentEventRootWorld.RotationY, m_desiredEventRootWorld.RotationY)))
                {
                    warpRotY *= -1f;
                }
            }
            else
            {
                //Simply warp the entire error if there is no remaining warp time
                warpRotY = m_desiredEventRootWorld.RotationY - m_currentEventRootWorld.RotationY;
            }

            return Quaternion.AngleAxis(warpRotY, Vector3.up);
        }

        //============================================================================================
        /**
        *  @brief Computes the amount of dynami rotational warping required for event warping this frame.
        *  
        *  The dynamic warp works differently to the linear warp in the way that warping is based purely
        *  by scaling up or down the motion that already exists in the current frame. This is dependent
        *  on the remaining motion and the desired remaining motion. This gives a more realistic warp. In this
        *  case warping operates on a 'per-axis' basis. If there is 0 remaining motion on any given axis 
        *  then this method reverts to linear warping.
        *  
        *  @param [float] remainingWarpTime - the remaining allowable time to warp to the next contact
        *         
        *********************************************************************************************/
        private Quaternion ComputeDynamicEventRotationWarping(float a_remainingWarpTime, float a_remaininRotDeltaSum)
        {
            float rotationY = m_animationRoot.rotation.eulerAngles.y;
            float curRotationY = Mathf.DeltaAngle(rotationY, p_animator.rootRotation.eulerAngles.y);
            float desiredRemainingRotationY = Mathf.DeltaAngle(rotationY, m_desiredEventRootWorld.RotationY);
            float remainingRotationY = a_remaininRotDeltaSum;

            float warpRotY = 0f;

            if (a_remainingWarpTime > Mathf.Epsilon)
            {
                if (Mathf.Abs(remainingRotationY) > 0.0001f)
                    warpRotY = (curRotationY / remainingRotationY) * desiredRemainingRotationY - warpRotY;
                else //Revert to linear
                    warpRotY = (desiredRemainingRotationY / a_remainingWarpTime) * p_currentDeltaTime;


                //The warp must always be in the direction of the desired contact rotation. This statement ensures that is the case
                if (Mathf.Abs(Mathf.DeltaAngle(m_currentEventRootWorld.RotationY + warpRotY, m_desiredEventRootWorld.RotationY))
                    > Mathf.Abs(Mathf.DeltaAngle(m_currentEventRootWorld.RotationY, m_desiredEventRootWorld.RotationY)))
                {
                    warpRotY *= -1f;
                }
            }
            else
            {
                //Simply warp the entire error if there is no remaining warp time
                warpRotY = m_desiredEventRootWorld.RotationY - m_currentEventRootWorld.RotationY;
            }

            return Quaternion.AngleAxis(warpRotY, Vector3.up);
        }

        //============================================================================================
        /**
        *  @brief Computes the amount of linear warping required for event warping this frame
        *  
        *  The linear method warps the root position linearly to reach contact points. It does this 
        *  by calculating the total positional error between the current animation contact point and the
        *  desired. This error is then divided over the 'warpable' time remaining to the contact point 
        *  and applied. Note that the error, and hence warp, is re-calculated every frame to compensate
        *  for rotational warping and other factors that might move / affect the transform of the 
        *  character (e.g. blending).
        *  
        *  @param [float] remainingWarpTime - the remaining allowable time to warp to the next contact
        *         
        *********************************************************************************************/
        private Vector3 ComputeLinearEventWarping(float a_remainingWarpTime)
        {
            Vector3 error = m_desiredEventRootWorld.Position - m_currentEventRootWorld.Position;
            Vector3 warp;

            if (a_remainingWarpTime > Mathf.Epsilon)
            {
                m_linearWarpRate = error / a_remainingWarpTime;
                warp = m_linearWarpRate * p_currentDeltaTime;

                if (warp.sqrMagnitude > error.sqrMagnitude)
                    warp = error;
            }
            else
            {
                warp = error;
            }

            m_currentEventRootWorld.Position += warp;
            return warp;
        }

        //============================================================================================
        /**
        *  @brief Computes the amount of dynamic position warping required for event warping this frame.
        *  
        *  The dynamic warp works differently to the linear warp in the way that warping is based purely
        *  by scaling up or down the motion that already exists in the current frame. This is dependent
        *  on the remaining motion and the desired remaining motion. This gives a more realistic warp. In this
        *  case warping operates on a 'per-axis' basis. If there is 0 remaining motion on any given axis 
        *  then this method reverts to linear warping.
        *  
        *  @param [float] remainingWarpTime - the remaining allowable time to warp to the next contact
        *         
        *********************************************************************************************/
        private Vector3 ComputeSimpleDynamicEventWarping(float a_remainingWarpTime)
        {
            Vector3 position = m_animationRoot.position;
            Vector3 curDisplacement = p_animator.deltaPosition;
            Vector3 desiredRemainingDisplacement = m_desiredEventRootWorld.Position - position;
            Vector3 remainingDisplacement = m_currentEventRootWorld.Position - position;

            Vector3 warp;
            if (a_remainingWarpTime > Mathf.Epsilon)
            {
                if (Mathf.Abs(remainingDisplacement.x) > 0.01f)
                    warp.x = (curDisplacement.x / remainingDisplacement.x) * desiredRemainingDisplacement.x - curDisplacement.x;
                else
                    warp.x = desiredRemainingDisplacement.x / a_remainingWarpTime * p_currentDeltaTime; //Revert to linear

                if (Mathf.Abs(remainingDisplacement.y) > 0.01f)
                    warp.y = (curDisplacement.y / remainingDisplacement.y) * desiredRemainingDisplacement.y - curDisplacement.y;
                else
                    warp.y = desiredRemainingDisplacement.y / a_remainingWarpTime * p_currentDeltaTime; //Revert to linear

                if (Mathf.Abs(remainingDisplacement.z) > 0.01f)
                    warp.z = (curDisplacement.z / remainingDisplacement.z) * desiredRemainingDisplacement.z - curDisplacement.z;
                else
                    warp.z = desiredRemainingDisplacement.z / a_remainingWarpTime * p_currentDeltaTime; //Revert to linear

                Vector3 desiredDirection = (m_desiredEventRootWorld.Position - m_currentEventRootWorld.Position).normalized;

                warp.x = Mathf.Abs(warp.x) * desiredDirection.x;
                warp.y = Mathf.Abs(warp.y) * desiredDirection.y;
                warp.z = Mathf.Abs(warp.z) * desiredDirection.z;
            }
            else
            {
                //Simply warp the entire error if there is no remaining warp time left
                warp = m_desiredEventRootWorld.Position - m_currentEventRootWorld.Position;
            }

            m_currentEventRootWorld.Position += warp;
            return warp;
        }

        //============================================================================================
        /**
        *  @brief Computes the amount of dynamic position warping required for event warping this frame.
        *  
        *  The dynamic warp works differently to the linear warp in the way that warping is based purely
        *  by scaling up or down the motion that already exists in the current frame. This is dependent
        *  on the remaining motion and the desired remaining motion. This gives a more realistic warp. In this
        *  case warping operates on a 'per-axis' basis. If there is 0 remaining motion on any given axis 
        *  then this method reverts to linear warping.
        *  
        *  @param [float] remainingWarpTime - the remaining allowable time to warp to the next contact
        *         
        *********************************************************************************************/
        private Vector3 ComputeDynamicEventWarping(float a_remainingWarpTime, Vector3 a_remainingDeltaSum)
        {
            Vector3 position = m_animationRoot.position;
            Vector3 curDisplacement = p_animator.deltaPosition;
            Vector3 desiredRemainingDisplacement = m_desiredEventRootWorld.Position - position;
            Vector3 remainingDisplacement = a_remainingDeltaSum;

            Vector3 warp;
            if (a_remainingWarpTime > Mathf.Epsilon)
            {
                if (Mathf.Abs(remainingDisplacement.x) > 0.01f)
                    warp.x = (curDisplacement.x / remainingDisplacement.x) * desiredRemainingDisplacement.x - curDisplacement.x;
                else
                    warp.x = desiredRemainingDisplacement.x / a_remainingWarpTime * p_currentDeltaTime; //Revert to linear

                if (Mathf.Abs(remainingDisplacement.y) > 0.01f)
                    warp.y = (curDisplacement.y / remainingDisplacement.y) * desiredRemainingDisplacement.y - curDisplacement.y;
                else
                    warp.y = desiredRemainingDisplacement.y / a_remainingWarpTime * p_currentDeltaTime; //Revert to linear

                if (Mathf.Abs(remainingDisplacement.z) > 0.01f)
                    warp.z = (curDisplacement.z / remainingDisplacement.z) * desiredRemainingDisplacement.z - curDisplacement.z;
                else
                    warp.z = desiredRemainingDisplacement.z / a_remainingWarpTime * p_currentDeltaTime; //Revert to linear

                Vector3 desiredDirection = (m_desiredEventRootWorld.Position - m_currentEventRootWorld.Position).normalized;

                warp.x = Mathf.Abs(warp.x) * desiredDirection.x;
                warp.y = Mathf.Abs(warp.y) * desiredDirection.y;
                warp.z = Mathf.Abs(warp.z) * desiredDirection.z;
            }
            else
            {
                //Simply warp the entire error if there is no remaining warp time left
                warp = m_desiredEventRootWorld.Position - m_currentEventRootWorld.Position;
            }

            m_currentEventRootWorld.Position += warp;
            return warp;
        }
        
        //============================================================================================
        /**
        *  @brief Begins an event via an MxMEventDefinition
        *  
        *  @param [MxMEventDefinition] a_eventDefinition - event definition data to use to begin an event
        *         
        *********************************************************************************************/
        public void BeginEvent(MxMEventDefinition a_eventDefinition, ETags a_overrideRequireTags = ETags.DoNotUse)
        {
            if (a_eventDefinition == null)
                return;

            if (a_eventDefinition.Priority < 0 || a_eventDefinition.Priority > m_curEventPriority || m_fsm.CurrentStateId != (uint)EMxMStates.Event)
            {
#if UNITY_2019_1_OR_NEWER && RIGGING_INTEGRATION
                if (p_riggingIntegration != null)
                    p_riggingIntegration.CacheTransforms();
#endif

                //Reset event data
                m_curEventPriority = a_eventDefinition.Priority;
                WarpType = a_eventDefinition.MotionWarpType;
                TimeWarpType = a_eventDefinition.TimingWarpType;
                RotWarpType = a_eventDefinition.RotationWarpType;
                m_exitEventWithMotion = a_eventDefinition.ExitWithMotion;
                m_eventType = a_eventDefinition.EventType;
                m_contactCountToWarp = a_eventDefinition.ContactCountToWarp;
                m_cumActionDuration = 0f;
                m_curEventContactIndex = 0;
                m_curEventState = EEventState.Windup;
                m_timeSinceMotionChosen = 0f;
                m_timeSinceEventTriggered = 0f;
                m_warpTimeScaling = a_eventDefinition.WarpTimeScaling;
                m_contactCountToTimeScale = a_eventDefinition.ContactCountToTimeScale;
                m_minWarpTimeScaling = a_eventDefinition.MinWarpTimeScale;
                m_maxWarpTimeScaling = a_eventDefinition.MaxWarpTimeScale;
                PostEventTrajectoryMode = a_eventDefinition.PostEventTrajectoryMode;

                CurEventContacts = a_eventDefinition.EventContacts.ToArray();

                //Get the current pose
                ComputeCurrentPose(); //Todo: Only do this once per frame

                float bestCost = float.MaxValue;
                int bestWindupPoseId = 0;
                EventData bestEvent = new EventData(); //Un-necessary copying
                Vector3 localPosition = Vector3.zero;

                if (CurEventContacts != null && CurEventContacts.Length > 0)
                    localPosition = m_animationRoot.InverseTransformPoint(CurEventContacts[0].Position);

                float playerRotationY = m_animationRoot.rotation.eulerAngles.y;
                float desiredDelay = a_eventDefinition.DesiredDelay;
                bool eventFound = false;
                
                for (int index = 0; index < CurrentAnimData.Events.Length; ++index)
                {
                    ref EventData evt = ref CurrentAnimData.Events[index];

                    if (evt.EventId != a_eventDefinition.Id)
                        continue;

                    int startTimeOffsetIndex = 0;
                    int iterationEnd = evt.WindupPoseContactOffsets.Length;
                    if (a_eventDefinition.ExactTimeMatch && desiredDelay <= evt.TimeToHit)
                    {
                        startTimeOffsetIndex = Mathf.RoundToInt(Mathf.Max(evt.TimeToHit - desiredDelay, 0) / CurrentAnimData.PoseInterval);
                        iterationEnd = Mathf.Min(iterationEnd, startTimeOffsetIndex + 1);
                    }
                    
                    for (int i = startTimeOffsetIndex; i < iterationEnd; ++i)
                    {
                        ref PoseData pose = ref CurrentAnimData.Poses[evt.StartPoseId + i];

                        if (a_eventDefinition.MatchRequireTags)
                        {
                            if (a_overrideRequireTags == ETags.DoNotUse)
                            {
                                ETags evtTags = (pose.Tags & (~ETags.DoNotUse));
                                
                                if (evtTags != m_desireRequiredTags)
                                    continue;
                            }
                            else
                            {
                                ETags evtTags = (pose.Tags & (~ETags.DoNotUse));
                                
                                if (evtTags != a_overrideRequireTags)
                                    continue;
                            }
                        }

                        float cost = 0f;

                        if (a_eventDefinition.MatchPose)
                        {
                            cost += ComputePoseCost(ref pose);
                        }

                        if (a_eventDefinition.MatchTrajectory)
                        {
                            cost += ComputeTrajectoryCost(ref pose);
                        }

                        cost *= pose.Favour;

                        if (a_eventDefinition.MatchTiming)
                        {
                            float timeWarp = Mathf.Abs(desiredDelay - evt.TimeToHit + CurrentAnimData.PoseInterval * i);
                            cost += timeWarp * a_eventDefinition.TimingWeight;
                        }

                        if (a_eventDefinition.MatchPosition && CurEventContacts.Length > 0)
                        {
                            cost += Vector3.Distance(m_animationRoot.InverseTransformPoint(CurEventContacts[0].Position),
                                evt.WindupPoseContactOffsets[i].Position) * a_eventDefinition.PositionWeight;
                            
                            for (int k = 0; k < a_eventDefinition.ContactCountToMatch - 1 
                                && k < evt.SubEventContactOffsets.Length 
                                && k < CurEventContacts.Length - 1; ++k)
                            {
                                cost += Vector3.Distance(m_animationRoot.InverseTransformPoint(CurEventContacts[k + 1].Position),
                                    evt.SubEventContactOffsets[k].Position) * a_eventDefinition.PositionWeight;
                            }
                        }

                        if (a_eventDefinition.MatchRotation && CurEventContacts.Length > 0)
                        {
                            cost += Mathf.Abs(Mathf.DeltaAngle(CurEventContacts[0].RotationY,
                                playerRotationY + evt.WindupPoseContactOffsets[i].RotationY) * a_eventDefinition.RotationWeight);

                            for (int k = 0; k < a_eventDefinition.ContactCountToMatch - 1 && k < evt.SubEventContactOffsets.Length; ++k)
                            {
                                cost += Mathf.Abs(Mathf.DeltaAngle(CurEventContacts[k + 1].RotationY,
                                    playerRotationY + evt.SubEventContactOffsets[k].RotationY) * a_eventDefinition.RotationWeight);
                            }
                        }

                        switch (a_eventDefinition.FavourTagMethod)
                        {
                            case EFavourTagMethod.Exclusive:
                            {
                                if(pose.FavourTags == FavourTags)
                                    cost *= m_favourMultiplier;
                                
                            } break;
                            case EFavourTagMethod.Inclusive:
                            {
                                if ((pose.FavourTags & FavourTags) != 0)
                                    cost *= m_favourMultiplier;
                                
                            } break;
                            case EFavourTagMethod.Stacking:
                            {
                                ETags activeTags = pose.FavourTags & FavourTags;
                                uint activeTagCount = MxMUtility.CountFlags(activeTags);
                                if (activeTagCount > 0)
                                {
                                    cost *= math.pow(FavourMultiplier, activeTagCount);
                                }
                            } break;
                            default:
                            {
                                //Nothing to do here
                            } break;
                        }
                        
                        if (cost < bestCost)
                        {
                            m_eventLength = evt.Length - CurrentAnimData.PoseInterval * i;
                            bestWindupPoseId = i;
                            bestCost = cost;
                            bestEvent = evt;

                            eventFound = true;
                        }
                    }
                }

                if (!eventFound)
                {
                    Debug.LogWarning("Could not find an event to match event definition: " + a_eventDefinition.ToString());
                    return;
                }
                    

                m_curEvent = bestEvent;

                m_eventStartTimeOffset = ((float)bestWindupPoseId) * CurrentAnimData.PoseInterval;
                m_chosenPose = CurrentAnimData.Poses[bestEvent.StartPoseId + bestWindupPoseId];
                m_curEventContactTime = bestEvent.TimeToHit - m_eventStartTimeOffset;

                ref readonly EventContact rootContactOffsetLocal = ref bestEvent.RootContactOffset[0];
                ref readonly EventContact windupContactOffsetLocal = ref bestEvent.WindupPoseContactOffsets[bestWindupPoseId];

                m_currentEventRootWorld.RotationY = windupContactOffsetLocal.RotationY + playerRotationY + rootContactOffsetLocal.RotationY;
                m_currentEventRootWorld.Position = m_animationRoot.TransformPoint(windupContactOffsetLocal.Position + rootContactOffsetLocal.Position);

                if (CurEventContacts != null && CurEventContacts.Length > 0)
                {
                    //Calculate warping data
                    ref readonly EventContact desiredContact = ref CurEventContacts[0];
                    Quaternion contactRotation = Quaternion.AngleAxis(desiredContact.RotationY, Vector3.up);

                    //Rotational warping calculations
                    m_desiredEventRootWorld.RotationY = desiredContact.RotationY + rootContactOffsetLocal.RotationY;

                    if (RotWarpType == EEventWarpType.Snap)
                    {
                        float error = Mathf.DeltaAngle(m_currentEventRootWorld.RotationY, m_desiredEventRootWorld.RotationY);

                        if (m_rootMotion != null)
                        {
                            m_rootMotion.Rotate(Vector3.up, error);
                        }
                        else
                        {
                            m_animationRoot.Rotate(Vector3.up, error);
                        }
                    }

                    //Positional warping calculations
                    m_desiredEventRootWorld.Position = desiredContact.Position + (contactRotation * rootContactOffsetLocal.Position);

                    contactRotation = Quaternion.AngleAxis(windupContactOffsetLocal.RotationY + m_animationRoot.rotation.eulerAngles.y, Vector3.up);
                    m_currentEventRootWorld.Position = m_animationRoot.TransformPoint(windupContactOffsetLocal.Position)
                        + contactRotation * rootContactOffsetLocal.Position;

                    if (WarpType == EEventWarpType.Snap)
                    {
                        if (m_rootMotion != null)
                        {
                            m_rootMotion.Translate(m_desiredEventRootWorld.Position - m_currentEventRootWorld.Position);
                        }
                        else
                        {
                            m_animationRoot.Translate(m_desiredEventRootWorld.Position - m_currentEventRootWorld.Position, Space.World);
                        }
                    }
                }

                m_eventSpeedMod = 1f;

                //Time warping calculations
                if (TimeWarpType != EEventWarpType.None)
                {
                    m_eventSpeedMod = m_curEventContactTime / desiredDelay;
                }
                else if (m_warpTimeScaling)
                {
                    Vector3 rootPosition = m_animationRoot.position;

                    //determine the scale and apply it.
                    float vectorToCurrentContactMagnitudeLocal = m_animationRoot.InverseTransformPoint(m_currentEventRootWorld.Position - rootPosition).magnitude;
                    float vectorToDesiredContactMagnitudeLocal = m_animationRoot.InverseTransformPoint(m_desiredEventRootWorld.Position - rootPosition).magnitude;

                    if (vectorToDesiredContactMagnitudeLocal > Mathf.Epsilon)
                    {
                        m_eventSpeedMod = Mathf.Clamp(Mathf.Abs(vectorToCurrentContactMagnitudeLocal / vectorToDesiredContactMagnitudeLocal),
                            m_minWarpTimeScaling, m_maxWarpTimeScaling);
                    }
                }

                TransitionToPose(ref m_chosenPose, m_eventSpeedMod);

                //Change State
                if (m_fsm.CurrentStateId != (uint)EMxMStates.Event)
                    m_fsm.GoToState((uint)EMxMStates.Event, true);

                StopJobs();
            }
        }
        
        public (AnimationClip, float) SourceAnimationFromMxMEvent(MxMEventDefinition a_eventDefinition, ETags a_overrideRequireTags = ETags.DoNotUse)
        {
            if (a_eventDefinition == null)
                return (null, 0.0f);
            
#if UNITY_2019_1_OR_NEWER && RIGGING_INTEGRATION
            if (p_riggingIntegration != null)
                p_riggingIntegration.CacheTransforms();
#endif
            CurEventContacts = a_eventDefinition.EventContacts.ToArray();

            //Get the current pose
            ComputeCurrentPose(); //Todo: Only do this once per frame

            float bestCost = float.MaxValue;
            int bestWindupPoseId = 0;
            EventData bestEvent = new EventData(); //Un-necessary copying
            
            float playerRotationY = m_animationRoot.rotation.eulerAngles.y;
            float desiredDelay = a_eventDefinition.DesiredDelay;
            bool eventFound = false;

            for (int index = 0; index < CurrentAnimData.Events.Length; ++index)
            {
                ref EventData evt = ref CurrentAnimData.Events[index];

                if (evt.EventId != a_eventDefinition.Id)
                    continue;

                for (int i = 0; i < evt.WindupPoseContactOffsets.Length; ++i)
                {
                    ref PoseData pose = ref CurrentAnimData.Poses[evt.StartPoseId + i];

                    if (a_eventDefinition.MatchRequireTags)
                    {
                        ETags evtTags = a_overrideRequireTags;
                        if (a_overrideRequireTags == ETags.DoNotUse)
                        {
                            evtTags = (pose.Tags & (~ETags.DoNotUse));
                        }
                            
                        if (evtTags != m_desireRequiredTags)
                            continue;
                    }

                    float cost = 0f;

                    if (a_eventDefinition.MatchPose)
                    {
                        cost += ComputePoseCost(ref pose);
                    }

                    if (a_eventDefinition.MatchTrajectory)
                    {
                        cost += ComputeTrajectoryCost(ref pose);
                    }

                    cost *= pose.Favour;

                    if (a_eventDefinition.MatchTiming)
                    {
                        float timeWarp = Mathf.Abs(desiredDelay - evt.TimeToHit + CurrentAnimData.PoseInterval * i);
                        cost += timeWarp * a_eventDefinition.TimingWeight;
                    }

                    if (a_eventDefinition.MatchPosition && CurEventContacts.Length > 0)
                    {
                        cost += Vector3.Distance(m_animationRoot.InverseTransformPoint(CurEventContacts[0].Position),
                            evt.WindupPoseContactOffsets[i].Position) * a_eventDefinition.PositionWeight;
                            
                        for (int k = 0; k < a_eventDefinition.ContactCountToMatch - 1 && k < evt.SubEventContactOffsets.Length 
                                && k < CurEventContacts.Length - 1; ++k)
                        {
                            cost += Vector3.Distance(m_animationRoot.InverseTransformPoint(CurEventContacts[k + 1].Position),
                                evt.SubEventContactOffsets[k].Position) * a_eventDefinition.PositionWeight;
                        }
                    }

                    if (a_eventDefinition.MatchRotation && CurEventContacts.Length > 0)
                    {
                        cost += Mathf.Abs(Mathf.DeltaAngle(CurEventContacts[0].RotationY,
                            playerRotationY + evt.WindupPoseContactOffsets[i].RotationY)) * a_eventDefinition.RotationWeight;

                        for (int k = 0; k < a_eventDefinition.ContactCountToMatch - 1 && k < evt.SubEventContactOffsets.Length; ++k)
                        {
                            cost += Mathf.Abs(Mathf.DeltaAngle(CurEventContacts[k + 1].RotationY,
                                playerRotationY + evt.SubEventContactOffsets[k].RotationY)) * a_eventDefinition.RotationWeight;
                        }
                    }

                    switch (a_eventDefinition.FavourTagMethod)
                    {
                        case EFavourTagMethod.Exclusive:
                        {
                            if(pose.FavourTags == FavourTags)
                                cost *= m_favourMultiplier;
                                
                        } break;
                        case EFavourTagMethod.Inclusive:
                        {
                            if ((pose.FavourTags & FavourTags) != 0)
                                cost *= m_favourMultiplier;
                                
                        } break;
                        case EFavourTagMethod.Stacking:
                        {
                            ETags activeTags = pose.FavourTags & FavourTags;
                            uint activeTagCount = MxMUtility.CountFlags(activeTags);
                            if (activeTagCount > 0)
                            {
                                cost *= math.pow(FavourMultiplier, activeTagCount);
                            }
                        } break;
                        default:
                        {
                            //Nothing to do here
                        } break;
                    }
                        
                    if (cost < bestCost)
                    {
                        m_eventLength = evt.Length - CurrentAnimData.PoseInterval * i;
                        bestWindupPoseId = i;
                        bestCost = cost;
                        bestEvent = evt;

                        eventFound = true;
                    }
                }
            }

            if (!eventFound)
            {
                Debug.LogWarning("Could not find an event to match event definition: " + a_eventDefinition.ToString());
                return (null, 0.0f);
            }
            
            m_curEvent = bestEvent;
            m_eventStartTimeOffset = ((float)bestWindupPoseId) * CurrentAnimData.PoseInterval;
            ref PoseData eventPose = ref CurrentAnimData.Poses[bestEvent.StartPoseId + bestWindupPoseId];

            if (eventPose.AnimType != EMxMAnimtype.Clip || eventPose.AnimType != EMxMAnimtype.Composite)
                return (null, 0.0f);
            
            return (CurrentAnimData.Clips[eventPose.AnimId], eventPose.Time);
        }
        
        //============================================================================================
        /**
        *  @brief Begin's an event explicitely by passing it an animation and an event Id. The function
        *  will search all event with the given event id and play the one that has the matching animation.
        *  
        *  Note - this is slower than starting an event with an event Id.
        *  
        *  @param [string] a_eventName - the name of the event to begin
        *  @param [int] a_priority - the priority of the event. Higher priority overrides lower priority events
        *  @param [bool] a_exitWithMotion - whether to allow event to exit with motion in the recovery
        *  phase.
        *         
        *********************************************************************************************/
        public void BeginEvent(MxMEventDefinition a_eventDefinition, AnimationClip a_eventClip, ETags a_overrideRequireTags = ETags.DoNotUse)
        {
              if (a_eventDefinition == null)
                return; 
              
              if (a_eventDefinition.Priority < 0 || a_eventDefinition.Priority > m_curEventPriority || m_fsm.CurrentStateId != (uint)EMxMStates.Event)
              {
#if UNITY_2019_1_OR_NEWER && RIGGING_INTEGRATION
                if (p_riggingIntegration != null)
                    p_riggingIntegration.CacheTransforms();
#endif

                //Reset event data
                m_curEventPriority = a_eventDefinition.Priority;
                WarpType = a_eventDefinition.MotionWarpType;
                TimeWarpType = a_eventDefinition.TimingWarpType;
                RotWarpType = a_eventDefinition.RotationWarpType;
                m_exitEventWithMotion = a_eventDefinition.ExitWithMotion;
                m_eventType = a_eventDefinition.EventType;
                m_contactCountToWarp = a_eventDefinition.ContactCountToWarp;
                m_cumActionDuration = 0f;
                m_curEventContactIndex = 0;
                m_curEventState = EEventState.Windup;
                m_timeSinceMotionChosen = 0f;
                m_timeSinceEventTriggered = 0f;
                m_warpTimeScaling = a_eventDefinition.WarpTimeScaling;
                m_contactCountToTimeScale = a_eventDefinition.ContactCountToTimeScale;
                m_minWarpTimeScaling = a_eventDefinition.MinWarpTimeScale;
                m_maxWarpTimeScaling = a_eventDefinition.MaxWarpTimeScale;
                PostEventTrajectoryMode = a_eventDefinition.PostEventTrajectoryMode;

                CurEventContacts = a_eventDefinition.EventContacts.ToArray();

                //Get the current pose
                ComputeCurrentPose(); //Todo: Only do this once per frame
                
                Vector3 localPosition = Vector3.zero;

                if (CurEventContacts != null && CurEventContacts.Length > 0)
                    localPosition = m_animationRoot.InverseTransformPoint(CurEventContacts[0].Position);

                float playerRotationY = m_animationRoot.rotation.eulerAngles.y;
                float desiredDelay = a_eventDefinition.DesiredDelay;
                bool eventFound = false;
                EventData bestEvent = new EventData(); //Un-necessary copying
                
                int bestWindupPoseId = 0;
                
                for (int index = 0; index < CurrentAnimData.Events.Length; ++index)
                {
                    ref EventData evt = ref CurrentAnimData.Events[index];
                    ref PoseData startPose = ref CurrentAnimData.Poses[evt.StartPoseId];
                    
                    if (evt.EventId != a_eventDefinition.Id)
                        continue;
                    
                    AnimationClip clip = CurrentAnimData.Clips[startPose.PrimaryClipId];

                    if (clip != a_eventClip)
                        continue;
                    
                    if (a_eventDefinition.MatchRequireTags)
                    {
                        if (a_overrideRequireTags == ETags.DoNotUse)
                        {
                            ETags evtTags = (startPose.Tags & (~ETags.DoNotUse));
                                
                            if (evtTags != m_desireRequiredTags)
                                continue;
                        }
                        else
                        {
                            ETags evtTags = (startPose.Tags & (~ETags.DoNotUse));
                                
                            if (evtTags != a_overrideRequireTags)
                                continue;
                        }
                    }
                    
                    bestEvent = evt;
                    eventFound = true;
                    
                    int startTimeOffsetIndex = 0;
                    int iterationEnd = evt.WindupPoseContactOffsets.Length;
                    if (a_eventDefinition.ExactTimeMatch && desiredDelay <= evt.TimeToHit)
                    {
                        startTimeOffsetIndex = Mathf.RoundToInt(Mathf.Max(evt.TimeToHit - desiredDelay, 0) / CurrentAnimData.PoseInterval);
                        iterationEnd = Mathf.Min(iterationEnd, startTimeOffsetIndex + 1);
                    }
                    
                    float bestCost = float.MaxValue;
                    
                    for (int i = startTimeOffsetIndex; i < iterationEnd; ++i)
                    {
                        ref PoseData pose = ref CurrentAnimData.Poses[evt.StartPoseId + i];
                        
                        float cost = 0f;

                        if (a_eventDefinition.MatchPose)
                        {
                            cost += ComputePoseCost(ref pose);
                        }

                        if (a_eventDefinition.MatchTrajectory)
                        {
                            cost += ComputeTrajectoryCost(ref pose);
                        }

                        cost *= pose.Favour;

                        if (a_eventDefinition.MatchTiming)
                        {
                            float timeWarp = Mathf.Abs(desiredDelay - evt.TimeToHit + CurrentAnimData.PoseInterval * i);
                            cost += timeWarp * a_eventDefinition.TimingWeight;
                        }

                        if (a_eventDefinition.MatchPosition && CurEventContacts.Length > 0)
                        {
                            cost += Vector3.Distance(m_animationRoot.InverseTransformPoint(CurEventContacts[0].Position),
                                evt.WindupPoseContactOffsets[i].Position) * a_eventDefinition.PositionWeight;
                            
                            for (int k = 0; k < a_eventDefinition.ContactCountToMatch - 1 
                                && k < evt.SubEventContactOffsets.Length 
                                && k < CurEventContacts.Length - 1; ++k)
                            {
                                cost += Vector3.Distance(m_animationRoot.InverseTransformPoint(CurEventContacts[k + 1].Position),
                                    evt.SubEventContactOffsets[k].Position) * a_eventDefinition.PositionWeight;
                            }
                        }

                        if (a_eventDefinition.MatchRotation && CurEventContacts.Length > 0)
                        {
                            cost += Mathf.Abs(Mathf.DeltaAngle(CurEventContacts[0].RotationY,
                                playerRotationY + evt.WindupPoseContactOffsets[i].RotationY) * a_eventDefinition.RotationWeight);

                            for (int k = 0; k < a_eventDefinition.ContactCountToMatch - 1 && k < evt.SubEventContactOffsets.Length; ++k)
                            {
                                cost += Mathf.Abs(Mathf.DeltaAngle(CurEventContacts[k + 1].RotationY,
                                    playerRotationY + evt.SubEventContactOffsets[k].RotationY) * a_eventDefinition.RotationWeight);
                            }
                        }

                        switch (a_eventDefinition.FavourTagMethod)
                        {
                            case EFavourTagMethod.Exclusive:
                            {
                                if(pose.FavourTags == FavourTags)
                                    cost *= m_favourMultiplier;
                                
                            } break;
                            case EFavourTagMethod.Inclusive:
                            {
                                if ((pose.FavourTags & FavourTags) != 0)
                                    cost *= m_favourMultiplier;
                                
                            } break;
                            case EFavourTagMethod.Stacking:
                            {
                                ETags activeTags = pose.FavourTags & FavourTags;
                                uint activeTagCount = MxMUtility.CountFlags(activeTags);
                                if (activeTagCount > 0)
                                {
                                    cost *= math.pow(FavourMultiplier, activeTagCount);
                                }
                            } break;
                            default:
                            {
                                //Nothing to do here
                            } break;
                        }
                        
                        if (cost < bestCost)
                        {
                            m_eventLength = evt.Length - CurrentAnimData.PoseInterval * i;
                            bestWindupPoseId = i;
                            bestCost = cost;
                        }
                    }
                }

                if (!eventFound)
                {
                    Debug.LogWarning("Could not find an event to match event definition: " + a_eventDefinition.ToString());
                    return;
                }
                    

                m_curEvent = bestEvent;

                m_eventStartTimeOffset = ((float)bestWindupPoseId) * CurrentAnimData.PoseInterval;
                m_chosenPose = CurrentAnimData.Poses[bestEvent.StartPoseId + bestWindupPoseId];
                m_curEventContactTime = bestEvent.TimeToHit - m_eventStartTimeOffset;

                ref readonly EventContact rootContactOffsetLocal = ref bestEvent.RootContactOffset[0];
                ref readonly EventContact windupContactOffsetLocal = ref bestEvent.WindupPoseContactOffsets[bestWindupPoseId];

                m_currentEventRootWorld.RotationY = windupContactOffsetLocal.RotationY + playerRotationY + rootContactOffsetLocal.RotationY;
                m_currentEventRootWorld.Position = m_animationRoot.TransformPoint(windupContactOffsetLocal.Position + rootContactOffsetLocal.Position);

                if (CurEventContacts != null && CurEventContacts.Length > 0)
                {
                    //Calculate warping data
                    ref readonly EventContact desiredContact = ref CurEventContacts[0];
                    Quaternion contactRotation = Quaternion.AngleAxis(desiredContact.RotationY, Vector3.up);

                    //Rotational warping calculations
                    m_desiredEventRootWorld.RotationY = desiredContact.RotationY + rootContactOffsetLocal.RotationY;

                    if (RotWarpType == EEventWarpType.Snap)
                    {
                        float error = Mathf.DeltaAngle(m_currentEventRootWorld.RotationY, m_desiredEventRootWorld.RotationY);

                        if (m_rootMotion != null)
                        {
                            m_rootMotion.Rotate(Vector3.up, error);
                        }
                        else
                        {
                            m_animationRoot.Rotate(Vector3.up, error);
                        }
                    }

                    //Positional warping calculations
                    m_desiredEventRootWorld.Position = desiredContact.Position + (contactRotation * rootContactOffsetLocal.Position);

                    contactRotation = Quaternion.AngleAxis(windupContactOffsetLocal.RotationY + m_animationRoot.rotation.eulerAngles.y, Vector3.up);
                    m_currentEventRootWorld.Position = m_animationRoot.TransformPoint(windupContactOffsetLocal.Position)
                        + contactRotation * rootContactOffsetLocal.Position;

                    if (WarpType == EEventWarpType.Snap)
                    {
                        if (m_rootMotion != null)
                        {
                            m_rootMotion.Translate(m_desiredEventRootWorld.Position - m_currentEventRootWorld.Position);
                        }
                        else
                        {
                            m_animationRoot.Translate(m_desiredEventRootWorld.Position - m_currentEventRootWorld.Position, Space.World);
                        }
                    }
                }

                m_eventSpeedMod = 1f;

                //Time warping calculations
                if (TimeWarpType != EEventWarpType.None)
                {
                    m_eventSpeedMod = m_curEventContactTime / desiredDelay;
                }
                else if (m_warpTimeScaling)
                {
                    Vector3 rootPosition = m_animationRoot.position;

                    //determine the scale and apply it.
                    float vectorToCurrentContactMagnitudeLocal = m_animationRoot.InverseTransformPoint(m_currentEventRootWorld.Position - rootPosition).magnitude;
                    float vectorToDesiredContactMagnitudeLocal = m_animationRoot.InverseTransformPoint(m_desiredEventRootWorld.Position - rootPosition).magnitude;

                    if (vectorToDesiredContactMagnitudeLocal > Mathf.Epsilon)
                    {
                        m_eventSpeedMod = Mathf.Clamp(Mathf.Abs(vectorToCurrentContactMagnitudeLocal / vectorToDesiredContactMagnitudeLocal),
                            m_minWarpTimeScaling, m_maxWarpTimeScaling);
                    }
                }

                TransitionToPose(ref m_chosenPose, m_eventSpeedMod);

                //Change State
                if (m_fsm.CurrentStateId != (uint)EMxMStates.Event)
                    m_fsm.GoToState((uint)EMxMStates.Event, true);

                StopJobs();
              }
        }

        //============================================================================================
        /**
        *  @brief Begins an event state in the MxM animator with event name only and no time or offset
        *  parameters. 
        *  
        *  Note - this is slower than starting an event with an event Id.
        *  
        *  @param [string] a_eventName - the name of the event to begin
        *  @param [int] a_priority - the priority of the event. Higher priority overrides lower priority events
        *  @param [bool] a_exitWithMotion - whether to allow event to exit with motion in the recovery
        *  phase.
        *         
        *********************************************************************************************/
        [Obsolete("This method is deprecated, please use the MxMEventDefinition version of 'BeginEvent'.")]
        public void BeginEvent(string a_eventName, int a_priority = -1, bool a_exitWithMotion = false)
        {
            if (CurrentAnimData == null)
                return;

            BeginEvent(CurrentAnimData.EventIdFromName(a_eventName), a_priority, a_exitWithMotion);
        }

        //============================================================================================
        /**
        *  @brief Begins a looping event state in the MxM animator with event name only and no time or offset
        *  parameters. 
        *  
        *  @param [string] a_eventName - the name of the event to begin
        *  @param [int] a_priority - the priority of the event. Higher priority overrides lower priority events
        *  @param [bool] a_exitWithMotion - whether to allow event to exit with motion in the recovery
        *  phase.
        *         
        *********************************************************************************************/
        public void BeginLoopedEvent(string a_eventName, int a_priority = -1, bool a_exitWithMotion = false)
        {
            if (CurrentAnimData == null)
                return;

            m_eventType = EMxMEventType.Loop;

            BeginEvent(CurrentAnimData.EventIdFromName(a_eventName), a_priority, a_exitWithMotion);
        }

        //============================================================================================
        /**
        *  @brief Begins a looping event state in the MxM animator with event id only and no time or offset
        *  parameters. 
        *  
        *  @param [int] a_eventID - the Id of the event to begin
        *  @param [int] a_priority - the priority of the event. Higher priority overrides lower priority events
        *  @param [bool] a_exitWithMotion - whether to allow event to exit with motion in the recovery
        *  phase.
        *         
        *********************************************************************************************/
        public void BeginLoopedEvent(int a_eventId, int a_priority = -1, bool a_exitWithMotion = false)
        {
            m_eventType = EMxMEventType.Loop;

            BeginEvent(a_eventId, a_priority, a_exitWithMotion);
        }

        //============================================================================================
        /**
        *  @brief Begins a looping sequence event state in the MxM animator with event name only and 
        *  no time or offset parameters. 
        *  
        *  This is different to a looping event in the way that it only loops the action and follow
        *  through portions of the event
        *  
        *  @param [string] a_eventName - the name of the event to begin
        *  @param [int] a_priority - the priority of the event. Higher priority overrides lower priority events
        *  @param [bool] a_exitWithMotion - whether to allow event to exit with motion in the recovery
        *  phase.
        *         
        *********************************************************************************************/
        public void BeginLoopedSequence(string a_eventName, int a_priority = -1, bool a_exitWithMotion = false)
        {
            if (CurrentAnimData == null)
                return;

            m_eventType = EMxMEventType.Sequence;

            BeginEvent(CurrentAnimData.EventIdFromName(a_eventName), a_priority, a_exitWithMotion);
        }

        //============================================================================================
        /**
        *  @brief Begins a looping sequence event state in the MxM animator with event id and 
        *  no time or offset parameters. 
        *  
        *  This is different to a looping event in the way that it only loops the action and follow
        *  through portions of the event
        *  
        *  @param [string] a_eventName - the name of the event to begin
        *  @param [int] a_priority - the priority of the event. Higher priority overrides lower priority events
        *  @param [bool] a_exitWithMotion - whether to allow event to exit with motion in the recovery
        *  phase.
        *         
        *********************************************************************************************/
        public void BeginLoopedSequence(int a_eventId, int a_priority = -1, bool a_exitWithMotion = false)
        {
            m_eventType = EMxMEventType.Sequence;

            BeginEvent(a_eventId, a_priority, a_exitWithMotion);
        }

        //============================================================================================
        /**
        *  @brief Legacy begin event function. Will be depractated
        *         
        *********************************************************************************************/
        [Obsolete("This method is deprecated, please use the MxMEventDefinition version of 'BeginEvent'.")]
        public void BeginEvent(string a_eventName, float a_desiredDelay, float a_delayWeight,
            int a_priority = -1, EEventWarpType a_warpTime = EEventWarpType.None)
        {
            if (CurrentAnimData == null)
                return;

            BeginEvent(CurrentAnimData.EventIdFromName(a_eventName), a_desiredDelay, a_delayWeight,
                a_priority, a_warpTime);
        }

        //============================================================================================
        /**
        *  @brief Legacy begin event function. Will be depractated
        *         
        *********************************************************************************************/
        [Obsolete("This method is deprecated, please use the MxMEventDefinition version of 'BeginEvent'.")]
        public void BeginEvent(string a_eventName, EventContact a_desiredContact, float a_contactWeight,
            int a_priority = -1, EEventWarpType a_warp = EEventWarpType.None, EEventWarpType a_warpAngle = EEventWarpType.None)
        {
            if (CurrentAnimData == null)
                return;

            BeginEvent(CurrentAnimData.EventIdFromName(a_eventName), a_desiredContact, a_contactWeight,
                a_priority, a_warp);
        }

        //============================================================================================
        /**
        *  @brief Legacy begin event function. Will be depractated
        *         
        *********************************************************************************************/
        [Obsolete("This method is deprecated, please use the MxMEventDefinition version of 'BeginEvent'.")]
        public void BeginEvent(string a_eventName, float a_desiredDelay, float a_delayWeight, EventContact a_desiredContact,
            float a_contactWeight, int a_priority = -0, EEventWarpType a_warp = EEventWarpType.None, EEventWarpType a_warpTime = EEventWarpType.None)
        {
            if (CurrentAnimData == null)
                return;

            BeginEvent(CurrentAnimData.EventIdFromName(a_eventName), a_desiredDelay, a_delayWeight, a_desiredContact,
                a_contactWeight, a_priority, a_warp, a_warpTime);
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void BeginEvent(int a_eventId, int a_priority = 0, bool a_exitWithMotion = false)
        {
            if (CurrentAnimData == null)
                return;

            if (m_fsm.CurrentStateId != (uint)EMxMStates.Event || a_priority >= m_curEventPriority)
            {
                //Reset Event Data
                m_curEventPriority = a_priority;
                WarpType = EEventWarpType.None;
                RotWarpType = EEventWarpType.None;
                TimeWarpType = EEventWarpType.None;
                m_exitEventWithMotion = a_exitWithMotion;
                m_curEventContactIndex = 0;
                m_curEventState = EEventState.Windup;
                m_timeSinceMotionChosen = 0f;
                m_timeSinceEventTriggered = 0f;
                m_cumActionDuration = 0f;

                //Get the current Pose
                ComputeCurrentPose();

                //Determine Best Pose
                float bestCost = float.MaxValue;
                int bestWindupPoseId = 0;
                EventData bestEvent = new EventData();
                foreach (EventData evt in CurrentAnimData.Events)
                {
                    if (evt.EventId != a_eventId)
                        continue;

                    for (int i = 0; i < evt.WindupPoseContactOffsets.Length; ++i)
                    {
                        float cost = ComputePoseCost(ref CurrentAnimData.Poses[evt.StartPoseId + i]);

                        if (cost < bestCost)
                        {
                            m_eventLength = evt.Length - CurrentAnimData.PoseInterval * i;
                            bestWindupPoseId = i;
                            bestCost = cost;
                            bestEvent = evt;
                        }
                    }
                }

                m_curEvent = bestEvent;

                m_eventStartTimeOffset = ((float)bestWindupPoseId) * CurrentAnimData.PoseInterval;
                m_chosenPose = CurrentAnimData.Poses[bestEvent.StartPoseId + bestWindupPoseId];

                TransitionToPose(ref m_chosenPose);

                //Change State
                if (m_fsm.CurrentStateId != (uint)EMxMStates.Event)
                    m_fsm.GoToState((uint)EMxMStates.Event, true);

                StopJobs();
            }
        }

        //============================================================================================
        /**
        *  @brief Legacy begin event function. Will be depractated
        *         
        *********************************************************************************************/
        [Obsolete("This method is deprecated, please use the MxMEventDefinition version of 'BeginEvent'.")]
        public void BeginEvent(int a_eventId, float a_desiredDelay, float a_delayWeight,
            int a_priority = 0, EEventWarpType a_warpTime = EEventWarpType.None, bool a_exitWithMotion = false)
        {
            if (CurrentAnimData == null)
                return;

            if (a_priority < 0 || a_priority > m_curEventPriority || m_fsm.CurrentStateId != (uint)EMxMStates.Event)
            {
                //Reset Data
                m_curEventPriority = a_priority;
                WarpType = EEventWarpType.None;
                RotWarpType = EEventWarpType.None;
                TimeWarpType = a_warpTime;
                m_curEventState = EEventState.Windup;
                m_exitEventWithMotion = a_exitWithMotion;
                m_curEventContactIndex = 0;
                m_timeSinceMotionChosen = 0f;
                m_timeSinceEventTriggered = 0f;
                m_cumActionDuration = 0f;

                //Get Current Pose
                ComputeCurrentPose(); //TODO: Mabye not required if the current state is "Matching"

                //Find the best Pose for the Event ID
                float bestCost = float.MaxValue;
                int bestWindupPoseId = 0;
                EventData bestEvent = new EventData();
                foreach (EventData evt in CurrentAnimData.Events)
                {
                    if (evt.EventId != a_eventId)
                        continue;

                    for (int i = 0; i < evt.WindupPoseContactOffsets.Length; ++i)
                    {
                        float cost = ComputePoseCost(ref CurrentAnimData.Poses[evt.StartPoseId + i]);

                        float timeWarp = Mathf.Abs(a_desiredDelay - evt.TimeToHit + CurrentAnimData.PoseInterval * i);
                        cost += timeWarp * a_delayWeight;

                        if (cost < bestCost)
                        {
                            m_eventLength = evt.Length - CurrentAnimData.PoseInterval * i;
                            bestWindupPoseId = i;
                            bestCost = cost;
                            bestEvent = evt;
                        }
                    }
                }

                m_curEvent = bestEvent;

                m_eventStartTimeOffset = ((float)bestWindupPoseId) * CurrentAnimData.PoseInterval;
                m_chosenPose = CurrentAnimData.Poses[bestEvent.StartPoseId + bestWindupPoseId];

                //Calculate time warping parameters
                if (a_warpTime != EEventWarpType.None)
                {
                    PlaybackSpeed = (bestEvent.TimeToHit - (CurrentAnimData.PoseInterval * bestWindupPoseId)) / a_desiredDelay;
                }

                TransitionToPose(ref m_chosenPose, m_playbackSpeed);

                //Change State
                if (m_fsm.CurrentStateId != (uint)EMxMStates.Event)
                    m_fsm.GoToState((uint)EMxMStates.Event, true);

                StopJobs();
            }
        }

        //============================================================================================
        /**
        *  @brief Legacy begin event function. Will be depractated
        *         
        *********************************************************************************************/
        [Obsolete("This method is deprecated, please use the MxMEventDefinition version of 'BeginEvent'.")]
        public void BeginEvent(int a_eventId, EventContact a_desiredContact, float a_contactWeight, int a_priority = -1,
             EEventWarpType a_warp = EEventWarpType.None, EEventWarpType a_warpAngle = EEventWarpType.None, bool a_exitWithMotion = false)
        {
            if (CurrentAnimData == null)
                return;

            if (a_priority < 0 || a_priority > m_curEventPriority || m_fsm.CurrentStateId != (uint)EMxMStates.Event)
            {
                //Reset Data
                m_curEventPriority = a_priority;
                WarpType = a_warp;
                RotWarpType = a_warpAngle;
                TimeWarpType = EEventWarpType.None;
                m_curEventContactIndex = 0;
                m_curEventState = EEventState.Windup;
                m_timeSinceMotionChosen = 0f;
                m_timeSinceEventTriggered = 0f;
                m_exitEventWithMotion = a_exitWithMotion;
                m_cumActionDuration = 0f;

                //Get the current Pose
                ComputeCurrentPose(); //Todo: Check to see if this has already been done this frame

                //Calculate the best pose for the event Id
                float bestCost = float.MaxValue;
                int bestWindupPoseId = 0;
                EventData bestEvent = new EventData();
                bool eventFound = false;
                Vector3 localPosition = m_animationRoot.InverseTransformPoint(a_desiredContact.Position);
                foreach (EventData evt in CurrentAnimData.Events)
                {
                    if (evt.EventId != a_eventId)
                        continue;

                    for (int i = 0; i < evt.WindupPoseContactOffsets.Length; ++i)
                    {
                        float cost = ComputePoseCost(ref CurrentAnimData.Poses[evt.StartPoseId + i]);

                        cost += Vector3.Distance(localPosition, evt.WindupPoseContactOffsets[i].Position) * a_contactWeight;
                        //TODO: COST FOR ANGLE?

                        if (cost < bestCost)
                        {
                            m_eventLength = evt.Length - CurrentAnimData.PoseInterval * i;
                            bestWindupPoseId = i;
                            bestCost = cost;
                            bestEvent = evt;

                            eventFound = true;
                        }
                    }
                }

                if (!eventFound)
                    return;

                m_curEvent = bestEvent;

                m_eventStartTimeOffset = ((float)bestWindupPoseId) * CurrentAnimData.PoseInterval;
                m_chosenPose = CurrentAnimData.Poses[bestEvent.StartPoseId + bestWindupPoseId];

                //Warping parameters
                Quaternion contactRotation = Quaternion.AngleAxis(a_desiredContact.RotationY, Vector3.up);
                ref readonly EventContact rootContactOffsetLocal = ref bestEvent.RootContactOffset[0];
                ref readonly EventContact windupContactOffsetLocal = ref bestEvent.WindupPoseContactOffsets[bestWindupPoseId];
                Vector3 rootContactOffsetWorld = contactRotation * rootContactOffsetLocal.Position;

                //Positional warping calculations
                m_desiredEventRootWorld.Position = a_desiredContact.Position + rootContactOffsetWorld;
                m_currentEventRootWorld.Position = m_animationRoot.TransformPoint(windupContactOffsetLocal.Position + rootContactOffsetLocal.Position);

                if (WarpType == EEventWarpType.Snap)
                {
                    m_animationRoot.Translate(m_desiredEventRootWorld.Position - m_currentEventRootWorld.Position);
                }

                //Rotational warping calculations
                m_desiredEventRootWorld.RotationY = a_desiredContact.RotationY + rootContactOffsetLocal.RotationY;
                m_currentEventRootWorld.RotationY = windupContactOffsetLocal.RotationY + m_animationRoot.rotation.eulerAngles.y + rootContactOffsetLocal.RotationY;

                if (RotWarpType == EEventWarpType.Snap)
                {
                    float error = Mathf.DeltaAngle(m_currentEventRootWorld.RotationY, m_desiredEventRootWorld.RotationY);
                    m_animationRoot.Rotate(Vector3.up, error);
                }

                TransitionToPose(ref m_chosenPose);

                //Change state
                if (m_fsm.CurrentStateId != (uint)EMxMStates.Event)
                    m_fsm.GoToState((uint)EMxMStates.Event, true);

                StopJobs();
            }
        }

        //============================================================================================
        /**
        *  @brief Legacy begin event function. Will be depractated
        *         
        *********************************************************************************************/
        [Obsolete("This method is deprecated, please use the MxMEventDefinition version of 'BeginEvent'.")]
        public void BeginEvent(int a_eventId, EventContact[] a_desiredContacts, float a_contactWeight,
            int a_priority = -1, EEventWarpType a_warp = EEventWarpType.None,
            EEventWarpType a_rotWarp = EEventWarpType.None, bool a_exitWithMotion = false)
        {

            if (CurrentAnimData == null || a_desiredContacts == null || a_desiredContacts.Length == 0)
                return;

            if (a_priority < 0 || a_priority > m_curEventPriority || m_fsm.CurrentStateId != (uint)EMxMStates.Event)
            {
                //Reset Event Data
                m_curEventPriority = a_priority;
                WarpType = a_warp;
                RotWarpType = a_rotWarp;
                TimeWarpType = EEventWarpType.None;
                m_cumActionDuration = 0f;
                m_curEventContactIndex = 0;
                CurEventContacts = a_desiredContacts;
                m_exitEventWithMotion = a_exitWithMotion;
                m_curEventState = EEventState.Windup;
                m_timeSinceMotionChosen = 0f;
                m_timeSinceEventTriggered = 0f;
                m_contactCountToWarp = a_desiredContacts.Length;

                //Compute current pose
                ComputeCurrentPose(); //TODO: Check if this has already been done this frame

                //Determine the best pose for the event ID
                float bestCost = float.MaxValue;
                int bestWindupPoseId = 0;
                EventData bestEvent = new EventData();
                bool eventFound = false;
                foreach (EventData evt in CurrentAnimData.Events)
                {
                    if (evt.EventId != a_eventId)
                        continue;

                    //TODO: Reconsider calculating the cost of secondary offsets

                    for (int i = 0; i < evt.WindupPoseContactOffsets.Length; ++i)
                    {
                        float cost = ComputePoseCost(ref CurrentAnimData.Poses[evt.StartPoseId + i]);

                        cost += Vector3.Distance(m_animationRoot.InverseTransformPoint(a_desiredContacts[0].Position),
                            evt.WindupPoseContactOffsets[i].Position) * a_contactWeight;
                        //Cost of roation?

                        if (cost < bestCost)
                        {
                            m_eventLength = evt.Length - CurrentAnimData.PoseInterval * i;
                            bestWindupPoseId = i;
                            bestCost = cost;
                            bestEvent = evt;

                            eventFound = true;
                        }
                    }
                }

                if (!eventFound)
                    return;

                m_curEvent = bestEvent;

                m_eventStartTimeOffset = ((float)bestWindupPoseId) * CurrentAnimData.PoseInterval;
                m_chosenPose = CurrentAnimData.Poses[bestEvent.StartPoseId + bestWindupPoseId];

                //Calculate warping parameters
                ref readonly EventContact desiredContactWorld = ref a_desiredContacts[0];
                ref readonly EventContact rootContactOffsetLocal = ref bestEvent.RootContactOffset[0];
                ref readonly EventContact windupContactOffsetLocal = ref bestEvent.WindupPoseContactOffsets[bestWindupPoseId];
                Quaternion contactRotation = Quaternion.AngleAxis(desiredContactWorld.RotationY, Vector3.up);
                Vector3 rootContactOffsetWorld = contactRotation * rootContactOffsetLocal.Position;

                //Calculate rotation warping
                m_desiredEventRootWorld.RotationY = desiredContactWorld.RotationY + rootContactOffsetLocal.RotationY;
                m_currentEventRootWorld.RotationY = windupContactOffsetLocal.RotationY + m_animationRoot.rotation.eulerAngles.y + rootContactOffsetLocal.RotationY;

                if (RotWarpType == EEventWarpType.Snap)
                {
                    float error = Mathf.DeltaAngle(m_currentEventRootWorld.RotationY, m_desiredEventRootWorld.RotationY);
                    m_animationRoot.Rotate(Vector3.up, error);
                }

                //Calculation position warping
                m_desiredEventRootWorld.Position = desiredContactWorld.Position + rootContactOffsetWorld;

                contactRotation = Quaternion.AngleAxis(windupContactOffsetLocal.RotationY + m_animationRoot.rotation.eulerAngles.y, Vector3.up);
                m_currentEventRootWorld.Position = m_animationRoot.TransformPoint(windupContactOffsetLocal.Position)
                    + contactRotation * rootContactOffsetLocal.Position;

                if (WarpType == EEventWarpType.Snap)
                {
                    m_animationRoot.Translate(m_desiredEventRootWorld.Position - m_currentEventRootWorld.Position);
                }

                TransitionToPose(ref m_chosenPose);

                //Change state
                if (m_fsm.CurrentStateId != (uint)EMxMStates.Event)
                    m_fsm.GoToState((uint)EMxMStates.Event, true);

                StopJobs();
            }
        }



        //============================================================================================
        /**
        *  @brief Legacy begin event function. Will be depractated
        *         
        *********************************************************************************************/
        [Obsolete("This method is deprecated, please use the MxMEventDefinition version of 'BeginEvent'.")]
        public void BeginEvent(int a_eventId, float a_desiredDelay, float a_delayWeight, EventContact a_desiredContact,
            float a_contactWeight, int a_priority = 0, EEventWarpType a_warp = EEventWarpType.None,
            EEventWarpType a_warpAngle = EEventWarpType.None, EEventWarpType a_warpTime = EEventWarpType.None, bool a_exitWithMotion = false)
        {
            if (CurrentAnimData == null)
                return;

            if (a_priority < 0 || a_priority > m_curEventPriority || m_fsm.CurrentStateId != (uint)EMxMStates.Event)
            {
                //Reset event data
                m_curEventPriority = a_priority;
                WarpType = a_warp;
                RotWarpType = a_warpAngle;
                TimeWarpType = a_warpTime;
                m_cumActionDuration = 0f;
                m_curEventContactIndex = 0;
                m_exitEventWithMotion = a_exitWithMotion;
                m_curEventState = EEventState.Windup;
                m_timeSinceMotionChosen = 0f;
                m_timeSinceEventTriggered = 0f;

                //Get the current Pose
                ComputeCurrentPose(); //Todo: Only do this once per frame

                //Find the best pose for the desired event
                float bestCost = float.MaxValue;
                int bestWindupPoseId = 0;
                EventData bestEvent = new EventData();
                Vector3 localPosition = m_animationRoot.InverseTransformPoint(a_desiredContact.Position);
                foreach (EventData evt in CurrentAnimData.Events)
                {
                    if (evt.EventId != a_eventId)
                        continue;

                    for (int i = 0; i < evt.WindupPoseContactOffsets.Length; ++i)
                    {
                        float cost = ComputePoseCost(ref CurrentAnimData.Poses[evt.StartPoseId + i]);

                        float timeWarp = Mathf.Abs(a_desiredDelay - evt.TimeToHit + CurrentAnimData.PoseInterval * i);

                        cost += timeWarp * a_delayWeight;
                        cost += Vector3.Distance(localPosition, evt.WindupPoseContactOffsets[i].Position) * a_contactWeight;
                        //TODO: COST FOR ANGLE?

                        if (cost < bestCost)
                        {
                            m_eventLength = evt.Length - CurrentAnimData.PoseInterval * i;
                            bestWindupPoseId = i;
                            bestCost = cost;
                            bestEvent = evt;
                        }
                    }
                }

                m_curEvent = bestEvent;

                m_eventStartTimeOffset = ((float)bestWindupPoseId) * CurrentAnimData.PoseInterval;
                m_chosenPose = CurrentAnimData.Poses[bestEvent.StartPoseId + bestWindupPoseId];

                //Calculate warping data
                Quaternion contactRotation = Quaternion.AngleAxis(a_desiredContact.RotationY, Vector3.up);
                ref readonly EventContact rootContactOffsetLocal = ref bestEvent.RootContactOffset[0];
                ref readonly EventContact windupContactOffsetLocal = ref bestEvent.WindupPoseContactOffsets[bestWindupPoseId];
                Vector3 rootContactOffsetWorld = contactRotation * rootContactOffsetLocal.Position;

                //Positional warping calculations
                m_desiredEventRootWorld.Position = a_desiredContact.Position + rootContactOffsetWorld;
                m_currentEventRootWorld.Position = m_animationRoot.TransformPoint(windupContactOffsetLocal.Position + rootContactOffsetLocal.Position);

                if (WarpType == EEventWarpType.Snap)
                {
                    m_animationRoot.Translate(m_desiredEventRootWorld.Position - m_currentEventRootWorld.Position);
                }

                //Rotational warping calculations
                m_desiredEventRootWorld.RotationY = a_desiredContact.RotationY + rootContactOffsetLocal.RotationY;
                m_currentEventRootWorld.RotationY = windupContactOffsetLocal.RotationY + m_animationRoot.rotation.eulerAngles.y + rootContactOffsetLocal.RotationY;

                if (RotWarpType == EEventWarpType.Snap)
                {
                    float error = Mathf.DeltaAngle(m_currentEventRootWorld.RotationY, m_desiredEventRootWorld.RotationY);
                    m_animationRoot.Rotate(Vector3.up, error);
                }

                //Time warping calculations
                if (a_warpTime != EEventWarpType.None)
                {
                    PlaybackSpeed = (bestEvent.TimeToHit - (CurrentAnimData.PoseInterval * bestWindupPoseId)) / a_desiredDelay;
                }

                TransitionToPose(ref m_chosenPose, m_playbackSpeed);

                //Change State
                if (m_fsm.CurrentStateId != (uint)EMxMStates.Event)
                    m_fsm.GoToState((uint)EMxMStates.Event, true);

                StopJobs();
            }
        }

        //============================================================================================
        /**
        *  @brief This function is called immediately after an event is complete (on DoExit of the 
        *  event state. It handles the trajectory immediately following the event dependent on the 
        *  user defined PoseEventTrajectoryMode.
        *         
        *********************************************************************************************/
        private void PostEventTrajectoryHandling()
        {
            switch (PostEventTrajectoryMode)
            {
                case EPostEventTrajectoryMode.Reset:
                    {
                        p_trajectoryGenerator.ResetMotion();
                    }
                    break;
                case EPostEventTrajectoryMode.InheritEvent:
                    {
                        p_trajectoryGenerator.CopyGoalFromPose(ref m_curInterpolatedPose);
                    }
                    break;
            }

            p_trajectoryGenerator.UnPause();
        }

        //============================================================================================
        /**
        *  @brief Forces the current event to be exited regardless of event conditions
        *         
        *********************************************************************************************/
        public void ForceExitEvent()
        {
            if (m_fsm.CurrentStateId == (uint)EMxMStates.Event)
            {
                ExitEvent();
            }
        }

        //============================================================================================
        /**
        *  @brief Forces the current event to be exited regardless of event conditions. In this case
        * idle will be enforced immediately after the event
        *         
        *********************************************************************************************/
        public void ForceExitEventToIdle()
        {
            if (m_fsm.CurrentStateId == (uint) EMxMStates.Event)
            {
                ResetMotion(m_trajectoryGenerators.Length > 1);
                BeginIdle();
            }
        }

        //============================================================================================
        /**
        *  @brief Exits from the current event
        *         
        *********************************************************************************************/
        private void ExitEvent()
        {
            ResetEventData();

            if (CurrentNativeAnimData.Tags == RequiredTags)
            {
                if (DetectIdle())
                {
                    m_fsm.GoToState((uint)EMxMStates.Idle, true);
                    BeginIdle();
                }
                else
                {
                    m_fsm.GoToState((uint)EMxMStates.Matching, true);
                }
            }
            else
            {
                BeginIdle();
            }
        }

        //============================================================================================
        /**
        *  @brief Resets all event data after every event is complete or exited
        *         
        *********************************************************************************************/
        public void ResetEventData()
        {
            m_curEventContactIndex = 0;
            m_curEventContactTime = 0f;
            m_curEventPriority = -1;
            m_eventType = EMxMEventType.Standard;
            WarpType = EEventWarpType.None;
            TimeWarpType = EEventWarpType.None;
            RotWarpType = EEventWarpType.None;
            m_exitEventWithMotion = false;
            m_curEventState = EEventState.Windup;
            m_eventLength = 0f;
            //m_timeSinceMotionChosen = 0f;
            m_timeSinceEventTriggered = 0f;
            m_contactCountToWarp = 0;
            CurEventContacts = null;
            m_eventStartTimeOffset = 0;
            m_curEventContactIndex = 0;
            m_curEventContactTime = 0f;
            m_linearWarpRate = Vector3.zero;
            m_linearWarpRotRate = 0f;
            m_cumActionDuration = 0f;
            m_warpTimeScaling = false;
            m_contactCountToTimeScale = 1;
            m_minWarpTimeScaling = 1f;
            m_maxWarpTimeScaling = 1f;
    }

        //============================================================================================
        /**
        *  @brief Set's whether the current event should loop. This should only be used on specific
        *  events where the clip loops continuously. In this case the event bounds should cover the
        *  entire clip 
        *         
        *********************************************************************************************/
        public void LoopCurrentEvent()
        {
            m_eventType = EMxMEventType.Loop;
        }

        //============================================================================================
        /**
        *  @brief Stops looping the current event.
        *         
        *********************************************************************************************/
        public void EndLoopEvent()
        {
            m_eventType = EMxMEventType.Standard;
        }

        //============================================================================================
        /**
        *  @brief Begins a loop event sequence. This should only be used with clips that have a looping
        *  region within them. The looping region should be encapsulated by the action and followthrough
        *  phase of the event.
        *         
        *********************************************************************************************/
        public void LoopEventSequence()
        {
            m_eventType = EMxMEventType.Sequence;
        }

        //============================================================================================
        /**
        *  @brief Ends a loop event sequence. Note that it will play through the current loop all the 
        *  way through to the end of the recover (unless ExitWithMotion is used)
        *         
        *********************************************************************************************/
        public void EndEventLoopSequence()
        {
            m_eventType = EMxMEventType.Standard;
        }

        //============================================================================================
        /**
        *  @brief Modifies an existing 'desired event contact'. This can be very useful for updating 
        *  a contact point after an event is chosen to avoid random and un-necessary warping.
        *  
        *  Example: Start a jump event. Once the event has been matched and chosen, retrieve current
        *  event contact from the MxMAnimator and raycast down to find the surface level at that point. 
        *  Then use this function to modify the contact point with the raycast point.
        *  
        *  @param [EventContact] a_desiredContact - new desired contact point for the current contact.
        *         
        *********************************************************************************************/
        public void ModifyDesiredEventContact(EventContact a_desiredContact)
        {
            if (m_fsm.CurrentStateId == (int)EMxMStates.Event)
            {
                CurEventContacts[m_curEventContactIndex] = a_desiredContact;

                Quaternion contactRotation = Quaternion.AngleAxis(a_desiredContact.RotationY, Vector3.up);
                ref readonly EventContact rootContactOffsetLocal = ref m_curEvent.RootContactOffset[m_curEventContactIndex];

                m_desiredEventRootWorld.RotationY = a_desiredContact.RotationY + rootContactOffsetLocal.RotationY;
                m_desiredEventRootWorld.Position = a_desiredContact.Position + (contactRotation * rootContactOffsetLocal.Position);

            }
        }

        //============================================================================================
        /**
        *  @brief Similar to ModifyDesiredEventContact by only modifies the desired position and not
        *  the facing angle.
        *  
        *  @param [Vector3] a_desiredPosition - new desired position for the current contact.
        *         
        *********************************************************************************************/
        public void ModifyDesiredEventContactPosition(Vector3 a_desiredPosition)
        {
            if (m_fsm.CurrentStateId != (int)EMxMStates.Event)
                return;

            if (m_curEventContactIndex >= CurEventContacts.Length)
                return;

            ref EventContact desiredContact = ref CurEventContacts[m_curEventContactIndex];
            desiredContact.Position = a_desiredPosition;

            Quaternion contactRotation = Quaternion.AngleAxis(desiredContact.RotationY, Vector3.up);
            ref readonly EventContact rootContactOffsetLocal = ref m_curEvent.RootContactOffset[m_curEventContactIndex];


            m_desiredEventRootWorld.Position = desiredContact.Position + (contactRotation * rootContactOffsetLocal.Position);

        }

        //============================================================================================
        /**
        *  @brief Similar to ModifyDesiredEventContact by only modifies the desired facing angle and not
        *  the fpositio.
        *  
        *  @param [float] a_desiredRotationY - new desired rotation for the current contact.
        *         
        *********************************************************************************************/
        public void ModifyDesiredEventContactRotation(float a_desiredRotationY)
        {
            if (m_fsm.CurrentStateId == (int)EMxMStates.Event)
            {
                ref EventContact desiredContact = ref CurEventContacts[m_curEventContactIndex];
                desiredContact.RotationY = a_desiredRotationY;

                ref readonly EventContact rootContactOffsetLocal = ref m_curEvent.RootContactOffset[m_curEventContactIndex];

                m_desiredEventRootWorld.RotationY = desiredContact.RotationY + rootContactOffsetLocal.RotationY;
            }
        }

        //============================================================================================
        /**
        *  @brief Checks if a specific event id is currently playing on the MxMAnimator
        *  
        *  @param [int] a_eventId - the id of the event to check.
        *  
        *  @param bool - true if the event is playing otherwise false
        *         
        *********************************************************************************************/
        public bool CheckEventPlaying(int a_eventId)
        {
            if ((EMxMStates)m_fsm.CurrentStateId != EMxMStates.Event)
                return false;

            if (CurrentEvent.EventId == a_eventId)
                return true;

            return false;
        }

        //============================================================================================
        /**
        *  @brief Checks if a specific event name is currently playing on the MxMAnimator
        *  
        *  @param [string] a_eventName - The name of the currently playing event
        *  
        *  @param bool - true if the event is playing otherwise false
        *         
        *********************************************************************************************/
        public bool CheckEventPlaying(string a_eventName)
        {
            if ((EMxMStates)m_fsm.CurrentStateId != EMxMStates.Event)
                return false;

            int eventId = CurrentAnimData.EventIdFromName(a_eventName);

            if (CurrentEvent.EventId == eventId)
                return true;

            return false;
        }

        //============================================================================================
        /**
        *  @brief Returns the first EventData of the passed name. If the event cannot be found the first
        *  event in the anim data list will be returned.
        *  
        *  @param [string] a_eventName - the name of the event to get data from
        *  
        *  @return ref readonly EventData - the event data 
        *         
        *********************************************************************************************/
        public ref readonly EventData GetEventData(string a_eventName)
        {
            //ref readonly EventData sampleEvent = ref GetEventData("myEvent");

            return ref GetEventData(CurrentAnimData.EventIdFromName(a_eventName));
        }

        //============================================================================================
        /**
        *  @brief Returns the first EventData of the passed id. If the event cannot be found the first
        *  event in the anim data list will be returned.
        *  
        *  @param [id] a_eventId - the id of the event to to get the data from
        *  
        *  @return ref readonly EventData - the event data 
        *         
        *********************************************************************************************/
        public ref readonly EventData GetEventData(int a_eventId)
        {
            for (int i = 0; i < CurrentAnimData.Events.Length; ++i)
            {
                ref readonly EventData eventData = ref CurrentAnimData.Events[i];
                if (eventData.EventId == a_eventId)
                    return ref eventData;
            }

            return ref CurrentAnimData.Events[0];
        }

        //============================================================================================
        /**
        *  @brief Samples the first contact point of an event if it was played from this frame without
        *  warping. If the event cannot be found, the characters current position will be returned.
        *  
        *  @param [string] a_eventName - the name of the event to sample from
        *  
        *  @return Vector3 - world position contact point of the event
        *         
        *********************************************************************************************/
        public Vector3 SampleEventContactPoint(string a_eventName)
        {
            return SampleEventContactPoint(CurrentAnimData.EventIdFromName(a_eventName));
        }

        //============================================================================================
        /**
        *  @brief Samples the first contact point of an event if it was played from this frame without
        *  warping. If the event cannot be found, the characters current position will be returned.
        *  
        *  @param [int] a_eventId - the Id of the event to sample from
        *  
        *  @return Vector3 - world position contact point of the event
        *         
        *********************************************************************************************/
        public Vector3 SampleEventContactPoint(int a_eventId)
        {
            ref readonly EventData eventData = ref GetEventData(a_eventId);

            if (eventData.WindupPoseContactOffsets == null || eventData.WindupPoseContactOffsets.Length == 0)
                return m_animationRoot.position;

            return m_animationRoot.TransformPoint(eventData.WindupPoseContactOffsets[0].Position);
        }

    }//End of partial class: MxMAnimator
}//End of namespace: MxM