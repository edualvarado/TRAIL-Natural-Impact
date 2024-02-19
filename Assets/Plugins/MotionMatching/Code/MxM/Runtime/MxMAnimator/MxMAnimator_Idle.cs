// ================================================================================================
// File: MxMAnimator_Idle.cs
// 
// Authors:  Kenneth Claassen
// Date:     2019-10-10: Created this file.
// 
//     Contains a part of the 'MxM' namespace for 'Unity Engine'.
// ================================================================================================
using UnityEngine;

namespace MxM
{
    //============================================================================================
    /**
    *  @brief This is partial implementation of the MxMAnimator. This particular partial class 
    *  handles all idle state logic for the of the MxMAnimator.
    *         
    *********************************************************************************************/
    public partial class MxMAnimator : MonoBehaviour
    {
        //Idles
        private IdleSetData m_curIdleSet; //The current idle set being used.
        private EIdleState m_idleState;
        private int m_curIdleSetId; //The id of the current Idle set being used (based on tag)
        private float m_idleDetectTimer; //Used to keep track of the time so that idle can be initiated.
        private int m_lastSecondaryIdleClipId; //The id of the last secondary idle clip (clip id within the idle set)
        private int m_lastIdleGlobalClipId; //The id of the last idle clip from the pose data (clip id from pose data not idle set)
        private float m_timeSinceLastIdleStarted;
        private int m_chosenIdleLoopCount; //The chosen number of loops that the current idle will play.

        [SerializeField]
        private bool m_enableIdle = true; //Whether the idle mode state should be enabled or not

        public bool EnableIdle
        {
            get { return m_enableIdle; }
            set { m_enableIdle = true; }
        }

        //Returns true if the character is idle.
        public bool IsIdle 
        {
            get
            {
                if ((EMxMStates)m_fsm.CurrentStateId == EMxMStates.Idle)
                    return true;

                return false;
            }
        }

        //============================================================================================
        /**
        *  @brief Triggers the idle state in the MxMAnimator.
        *  
        *  The idle chosen is dependent on the currently required tags in the MxMAnimator.
        *         
        *********************************************************************************************/
        public bool BeginIdle()
        {
            if (m_fsm.CurrentStateId == (uint)EMxMStates.Event)
                return false;

            IdleSetData[] idleSets = CurrentAnimData.IdleSets;
            int idleSetCount = CurrentAnimData.IdleSets.Length;

            if (idleSets == null || idleSetCount == 0)
                return false;

            StopJobs();

            ref readonly IdleSetData chosenIdle = ref idleSets[0];

            //Pick the best Idle Set
            float bestCost = float.MaxValue;
            int bestPoseId = chosenIdle.PrimaryPoseIds[0];
            bool isTransition = false;
            for (int i = 0; i < idleSetCount; ++i)
            {
                ref readonly IdleSetData thisIdle = ref idleSets[i];

                if (thisIdle.Tags == RequiredTags && thisIdle.Traits == IdleTraits)
                {
                    for (int k = 0; k < thisIdle.TransitionPoseIds.Length; ++k)
                    {
                        int poseId = thisIdle.TransitionPoseIds[k];
                        float cost = ComputePoseCost(ref CurrentAnimData.Poses[poseId]);

                        if (cost < bestCost)
                        {
                            bestCost = cost;
                            bestPoseId = poseId;
                            chosenIdle = ref thisIdle;
                            isTransition = true;
                        }
                    }

                    for (int k = 0; k < thisIdle.PrimaryPoseIds.Length; ++k)
                    {
                        int poseId = thisIdle.PrimaryPoseIds[k];

                        float cost = ComputePoseCost(ref CurrentAnimData.Poses[poseId]);

                        if (cost < bestCost)
                        {
                            bestCost = cost;
                            bestPoseId = poseId;
                            chosenIdle = ref thisIdle;
                            isTransition = false;
                        }
                    }
                }
            }

#if UNITY_EDITOR
            m_lastChosenCost = m_lastPoseCost = bestCost;
            m_lastTrajectoryCost = 0f;
#endif

            m_curIdleSet = chosenIdle;

            m_timeSinceLastIdleStarted = 0f;
            m_timeSinceMotionChosen = 0f;
            m_curChosenPoseId = bestPoseId;
            m_chosenPose = CurrentAnimData.Poses[m_curChosenPoseId];
            m_chosenIdleLoopCount = m_curIdleSet.MaxLoops;

            if (isTransition)
            {
                m_lastIdleGlobalClipId = m_chosenPose.PrimaryClipId;
                m_idleState = EIdleState.Transition;
            }
            else
            {
                m_lastIdleGlobalClipId = m_curIdleSet.PrimaryClipId;
                m_idleState = EIdleState.Primary;
            }

            TransitionToPose(ref m_chosenPose);

            m_fsm.GoToState((uint)EMxMStates.Idle, true);

            m_onIdleTriggered.Invoke();

            return true;
        }

        //============================================================================================
        /**
        *  @brief Updates the idle state of the MxMAnimator
        *         
        *********************************************************************************************/
        private void UpdateIdle()
        {
            foreach (IMxMExtension extension in m_phase1Extensions)
            {
                if (extension.IsEnabled)
                    extension.UpdatePhase1();
            }

            //if (m_debugCurrentPose)
            ComputeCurrentPose();

            GenerateGoalTrajectory(p_trajectoryGenerator.GetCurrentGoal());

            if (DetectIdle())
            {
                m_timeSinceLastIdleStarted += p_currentDeltaTime * m_playbackSpeed;

                float clipLength = CurrentAnimData.Clips[m_lastIdleGlobalClipId].length;

                switch (m_idleState)
                {
                    case EIdleState.Primary:
                        {
                            if (m_curIdleSet.SecondaryClipIds != null && m_curIdleSet.SecondaryClipIds.Length > 0)
                            {
                                int numLoops = Mathf.FloorToInt(m_timeSinceLastIdleStarted / clipLength);

                                if (numLoops > m_chosenIdleLoopCount)
                                {
                                    int chosenClipId = Random.Range(0, m_curIdleSet.SecondaryClipIds.Length);
                                    if (chosenClipId == m_lastSecondaryIdleClipId)
                                    {
                                        ++chosenClipId;
                                        if (chosenClipId > m_curIdleSet.SecondaryClipIds.Length - 1)
                                            chosenClipId = 0;
                                    }

                                    m_lastSecondaryIdleClipId = chosenClipId;
                                    m_lastIdleGlobalClipId = m_curIdleSet.SecondaryClipIds[chosenClipId];
                                    m_chosenIdleLoopCount = 1;
                                    m_timeSinceLastIdleStarted = 0f;
                                    m_timeSinceMotionChosen = 0f;

                                    m_curChosenPoseId = m_curIdleSet.SecondaryPoseIds[chosenClipId];
                                    m_chosenPose = CurrentAnimData.Poses[m_curChosenPoseId];

                                    TransitionToPose(ref m_chosenPose);

                                    m_idleState = EIdleState.Secondary;
                                }
                            }
                        }
                        break;
                    case EIdleState.Secondary:
                    case EIdleState.Transition:
                        {
                            float remaining = clipLength - m_chosenPose.Time - m_timeSinceLastIdleStarted;

                            if (remaining * m_playbackSpeed <= m_matchBlendTime / 10f)
                            {
                                m_chosenIdleLoopCount = Random.Range(m_curIdleSet.MinLoops, m_curIdleSet.MaxLoops + 1);

                                m_timeSinceLastIdleStarted = 0f;
                                m_timeSinceMotionChosen = 0f;

                                float bestCost = float.MaxValue;
                                int bestPoseId = 0;
                                for (int i = 0; i < m_curIdleSet.PrimaryPoseIds.Length; ++i)
                                {
                                    ref PoseData pose = ref CurrentAnimData.Poses[m_curIdleSet.PrimaryPoseIds[i]];

                                    float cost = ComputePoseCost(ref pose);

                                    if (cost < bestCost)
                                    {
                                        bestCost = cost;
                                        bestPoseId = i;
                                    }
                                }

#if UNITY_EDITOR
                                m_lastPoseCost = bestCost;
                                m_lastTrajectoryCost = 0f;
#endif

                                m_curChosenPoseId = m_curIdleSet.PrimaryPoseIds[bestPoseId];
                                m_chosenPose = CurrentAnimData.Poses[m_curChosenPoseId];

                                TransitionToPose(ref m_chosenPose);

                                m_idleState = EIdleState.Primary;
                                m_lastIdleGlobalClipId = m_curIdleSet.PrimaryClipId;
                            }
                        }
                        break;
                }
            }
            else if(CurrentNativeAnimData.Tags == RequiredTags)
            {
                m_fsm.GoToState((uint)EMxMStates.Matching, true);
            }
        }

        //============================================================================================
        /**
        *  @brief Detects if the character has gone to an idel state
        *  
        *  @return bool - true if idle is detected, otherwise false
        *         
        *********************************************************************************************/
        public bool DetectIdle()
        {
            if (!m_enableIdle)
                return false;
            
            //If the player has no input and the current velocity is very low then transition to idle
            if (!p_trajectoryGenerator.HasMovementInput())
            {
                if (m_curInterpolatedPose.LocalVelocity.sqrMagnitude < 0.05f)
                {
                    float tipThreshold = m_turnInPlaceThreshold;

                    if (m_fsm.CurrentStateId != (uint)EMxMStates.Idle)
                        tipThreshold /= 2f;


                    if (!DetectAngularMovement(tipThreshold))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        //============================================================================================
        /**
        *  @brief Forces the character into an idle state. 
        *  
        *  Note that if the trajectory is not reset (ResetMotion()) and paused, any user input next
        *  frame can take the user out of idle immediately.
        *         
        *********************************************************************************************/
        public void ForceBeginIdle()
        {
            BeginIdle();
        }

        //============================================================================================
        /**
        *  @brief Checks if the currently playing idle is suitable for the tags currently set on the
        *  MxMAnimator. If they are not, it will reset the idle state and blend in the new idle 
        *  appropriate for the current required tags.
        *         
        *********************************************************************************************/
        private void CheckIdleSuitability()
        {
            if (m_fsm.CurrentStateId == (uint)EMxMStates.Idle)
            {
                if (m_curIdleSet.Tags != RequiredTags)
                {
                    BeginIdle();
                }
            }
        }

    }//End of partial class: MxMAnimator
}//End of namespace: MxM
