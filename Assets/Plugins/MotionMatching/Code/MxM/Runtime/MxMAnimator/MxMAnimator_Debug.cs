// ================================================================================================
// File: MxMAnimator_Debug.cs
// 
// Authors:  Kenneth Claassen
// Date:     2019-09-12: Created this file.
// 
//     Contains a part of the 'MxM' namespace for 'Unity Engine'.
// ================================================================================================
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEditor;
using UTIL;

namespace MxM
{
    //============================================================================================
    /**
    *  @brief This is partial implementation of the MxMAnimator. This particular partial class 
    *  handles all debug system logic for the MxMAnimator.
    *         
    *********************************************************************************************/
    public partial class MxMAnimator : MonoBehaviour
    {
#if UNITY_EDITOR
        private bool m_updateThisFrame;
        private float m_lastChosenCost;
        private float m_lastPoseCost;
        private float m_lastTrajectoryCost;

        private Vector3 basePosition;
        private Quaternion baseRotation;
        private bool m_debugPosesActive;

        TrajectoryPoint[] m_debugDesiredGoal;

        private bool m_debugPreview;
        private Mesh m_debugArrowMesh;

        public float LastChosenCost => m_lastChosenCost;


        public MxMDebugger DebugData { get; private set; }

        //============================================================================================
        /**
        *  @brief Draws the debugging gizmo's for the MxMAnimator.
        *  
        *  This includes drawing of the desired trajectory, animation trajectory and pose joint data.
        *         
        *********************************************************************************************/
        private void OnDrawGizmos()
        {
            if (p_trajectoryGenerator != null && CurrentAnimData != null && m_animMixerConnected && enabled)
            {
                if (m_fsm.CurrentStateId == (uint)EMxMStates.Event)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(m_desiredEventRootWorld.Position, 0.1f);
                    Quaternion rotation = Quaternion.AngleAxis(m_desiredEventRootWorld.RotationY, Vector3.up);
                    DrawArrow.ForGizmo(m_desiredEventRootWorld.Position, rotation * Vector3.forward, 0.8f, 20f);
                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(m_currentEventRootWorld.Position, 0.1f);
                    rotation = Quaternion.AngleAxis(m_currentEventRootWorld.RotationY, Vector3.up);
                    DrawArrow.ForGizmo(m_currentEventRootWorld.Position, rotation * Vector3.forward, 0.8f, 20f);

                    Quaternion contactRotation = Quaternion.AngleAxis(m_desiredEventRootWorld.RotationY, Vector3.up);

                    if (CurEventContacts != null && CurEventContacts.Length > m_curEventContactIndex)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawSphere(CurEventContacts[m_curEventContactIndex].Position, 0.05f);
                    }

                    Gizmos.color = Color.cyan;
                    Gizmos.DrawSphere(transform.position, 0.05f);
                    DrawArrow.ForGizmo(transform.position, transform.rotation * Vector3.forward, 0.4f, 20f);

                }

                if (m_debugGoal)
                {
                    Array.Copy(m_desiredGoal, m_debugDesiredGoal, m_debugDesiredGoal.Length);
                    float rot = transform.rotation.eulerAngles.y;
                    for(int i = 0; i < m_debugDesiredGoal.Length; ++i)
                    {
  
                        Vector3 newPos = transform.TransformVector(m_debugDesiredGoal[i].Position);
                        float newRot = m_debugDesiredGoal[i].FacingAngle + rot;

                        m_debugDesiredGoal[i] = new TrajectoryPoint(newPos, newRot);
                    }

                    //Draw Desired Trajectory
                    int doneCenter = 0;
                    for (int i = 0; i < m_debugDesiredGoal.Length; ++i)
                    {
                        TrajectoryPoint curPoint = m_debugDesiredGoal[i];

                        if (CurrentAnimData.PosePredictionTimes[i] < 0f)
                            Gizmos.color = new Color(0f, 0.5f, 0f);
                        else
                        {
                            if (doneCenter == 0)
                            {
                                Gizmos.color = Color.cyan;
                                Handles.color = Color.cyan;
                                Handles.DrawWireDisc(transform.position, Vector3.up, 0.3f);
                                Gizmos.color = Color.blue;
                                Vector3 centerPos = transform.position;
                                Vector3 centerPosUp = centerPos + Vector3.up * 0.14f;

                                if (m_debugArrowMesh != null)
                                {
                                    Gizmos.DrawMesh(m_debugArrowMesh, centerPos, Quaternion.LookRotation(transform.forward, Vector3.up));
                                }
                                else
                                {
                                    Gizmos.DrawSphere(centerPos, 0.06f);
                                    Gizmos.DrawLine(centerPos, centerPosUp);
                                    DrawArrow.ForGizmo(centerPosUp, transform.forward * 0.2f, 0.05f, 20f);
                                }

                                doneCenter = 1;

                                Vector3 point1;
                                Vector3 point2;
                                if (m_transformGoal)
                                {
                                    point1 = transform.position + m_debugDesiredGoal[i - 1].Position;
                                    point2 = transform.position + m_debugDesiredGoal[i].Position;
                                }
                                else
                                {
                                    point1 = transform.TransformPoint(m_debugDesiredGoal[i - 1].Position);
                                    point2 = transform.TransformPoint(m_debugDesiredGoal[i].Position);
                                }

                                Gizmos.color = new Color(0f, 0.5f, 0f);
                                Gizmos.DrawLine(centerPos, point1);
                                Gizmos.color = Color.green;
                                Gizmos.DrawLine(centerPos, point2);
                            }

                            Gizmos.color = Color.green;
                        }

                        Vector3 pointPos;

                        float angle = curPoint.FacingAngle * Mathf.Deg2Rad;

                        var direction = new Vector3(0.2f * Mathf.Sin(angle), 0f, 0.2f * Mathf.Cos(angle));

                        if (m_transformGoal)
                        {
                            pointPos = transform.position + curPoint.Position;
                        }
                        else
                        {
                            pointPos = transform.TransformPoint(curPoint.Position);
                            direction = transform.TransformDirection(direction);
                        }

                        if (m_debugArrowMesh != null)
                        {
                            Gizmos.DrawMesh(m_debugArrowMesh, pointPos, Quaternion.LookRotation(direction, Vector3.up));
                        }
                        else
                        {
                            Gizmos.DrawSphere(pointPos, 0.06f);
                            Gizmos.DrawLine(pointPos, new Vector3(pointPos.x, pointPos.y + 0.14f, pointPos.z));
                            DrawArrow.ForGizmo(new Vector3(pointPos.x, pointPos.y + 0.14f, pointPos.z),
                                direction, 0.05f, 20f);
                        }

                        if (i != 0)
                        {
                            Vector3 pointPosPrev;

                            if (m_transformGoal)
                            {
                                pointPosPrev = transform.position + m_debugDesiredGoal[i - 1].Position;
                            }
                            else
                            {
                                pointPosPrev = transform.TransformPoint(m_debugDesiredGoal[i - 1].Position);
                            }

                            if (doneCenter != 1)
                            {
                                Gizmos.DrawLine(pointPos, pointPosPrev);
                            }
                            else
                            {
                                doneCenter = 2;
                            }
                        }
                    }
                }

                if (m_debugCurrentPose)
                {
                    Gizmos.color = Color.yellow;

                    for (int i = 0; i < m_curInterpolatedPose.JointsData.Length; ++i)
                    {
                        Vector3 pos = transform.TransformPoint(m_curInterpolatedPose.JointsData[i].Position);
                        Gizmos.DrawWireSphere(pos, 0.04f);
                        DrawArrow.ForGizmo(pos, transform.TransformVector(m_curInterpolatedPose.JointsData[i].Velocity), 0.05f);
                    }
                }

                if (m_debugChosenTrajectory)
                {
                    TrajectoryPoint[] curGoal = CurrentAnimData.Poses[m_curInterpolatedPose.PoseId].Trajectory;

                    int doneCenter = 0;
                    for (int i = 0; i < curGoal.Length; ++i)
                    {
                        TrajectoryPoint curPoint = curGoal[i];

                        if (CurrentAnimData.PosePredictionTimes[i] < 0f)
                            Gizmos.color = new Color(0.5f, 0f, 0f);
                        else
                        {
                            if (doneCenter == 0)
                            {
                                Gizmos.color = Color.cyan;
                                Handles.color = Color.cyan;
                                Handles.DrawWireDisc(transform.position, Vector3.up, 0.3f);

                                DrawArrow.ForGizmo(transform.position,
                                    transform.TransformVector(m_curInterpolatedPose.LocalVelocity), 0.4f, 20f);

                                Gizmos.color = Color.blue;
                                Vector3 centerPos = transform.position;
                                Vector3 centerPosUp = centerPos + Vector3.up * 0.14f;

                                if (m_debugArrowMesh != null)
                                {
                                    Gizmos.DrawMesh(m_debugArrowMesh, centerPos, Quaternion.LookRotation(transform.forward, Vector3.up));
                                }
                                else
                                {
                                    Gizmos.DrawSphere(centerPos, 0.06f);
                                    Gizmos.DrawLine(centerPos, centerPosUp);
                                    DrawArrow.ForGizmo(centerPosUp, transform.forward * 0.2f, 0.05f, 20f);
                                }

                                doneCenter = 1;

                                Gizmos.color = new Color(0.5f, 0f, 0f);
                                Gizmos.DrawLine(centerPos, transform.TransformPoint(curGoal[i - 1].Position));
                                Gizmos.color = Color.red;
                                Gizmos.DrawLine(centerPos, transform.TransformPoint(curGoal[i].Position));
                            }

                            Gizmos.color = Color.red;
                        }

                        Vector3 pointPos = transform.TransformPoint(curPoint.Position);
                        float angle = curPoint.FacingAngle * Mathf.Deg2Rad;

                        Vector3 direction = transform.TransformDirection(new Vector3(0.2f * Mathf.Sin(angle),
                                0f, 0.2f * Mathf.Cos(angle)));

                        if (m_debugArrowMesh != null)
                        {
                            Gizmos.DrawMesh(m_debugArrowMesh, pointPos, Quaternion.LookRotation(direction, Vector3.up));
                        }
                        else
                        {
                            Gizmos.DrawSphere(pointPos, 0.06f);
                            Gizmos.DrawLine(pointPos, new Vector3(pointPos.x, pointPos.y + 0.14f, pointPos.z));
                            DrawArrow.ForGizmo(new Vector3(pointPos.x, pointPos.y + 0.14f, pointPos.z), direction, 0.05f, 20f);
                        }

                        if (i != 0)
                        {
                            if (doneCenter != 1)
                                Gizmos.DrawLine(pointPos, transform.TransformPoint(curGoal[i - 1].Position));
                            else
                                doneCenter = 2;
                        }
                    }


                }
            }
            else
            {
                //Draw Chosen Pose Trajectory
                if (m_debugPoses && m_debugPosesActive && CurrentAnimData != null)
                {
                    if (m_debugPoseId >= CurrentAnimData.Poses.Length)
                        m_debugPoseId = CurrentAnimData.Poses.Length - 1;

                    PoseData pose = CurrentAnimData.Poses[m_debugPoseId];
                    UpdatePoseDebug();
                    Gizmos.color = Color.yellow;
                    for (int i = 0; i < pose.JointsData.Length; ++i)
                    {
                        Vector3 pos = transform.TransformPoint(pose.JointsData[i].Position);
                        Gizmos.DrawWireSphere(pos, 0.04f);
                        DrawArrow.ForGizmo(pos, transform.TransformVector(
                            pose.JointsData[i].Velocity), 0.05f);
                    }

                    if (m_debugPoseId < CurrentAnimData.Poses.Length)
                        m_chosenPose = CurrentAnimData.Poses[m_debugPoseId];
                    else
                        m_chosenPose = CurrentAnimData.Poses[CurrentAnimData.Poses.Length - 1];

                    TrajectoryPoint[] curGoal = m_chosenPose.Trajectory;

                    int doneCenter = 0;
                    for (int i = 0; i < curGoal.Length; ++i)
                    {
                        TrajectoryPoint curPoint = curGoal[i];

                        if (CurrentAnimData.PosePredictionTimes[i] < 0f)
                            Gizmos.color = new Color(0.5f, 0f, 0f);
                        else
                        {
                            if (doneCenter == 0)
                            {
                                Gizmos.color = Color.cyan;
                                Handles.color = Color.cyan;
                                Handles.DrawWireDisc(transform.position, Vector3.up, 0.3f);
                                DrawArrow.ForGizmo(transform.position,
                                    transform.TransformVector(m_chosenPose.LocalVelocity), 0.4f, 20f);

                                Gizmos.color = Color.blue;
                                Vector3 centerPos = transform.position;
                                Vector3 centerPosUp = centerPos + Vector3.up * 0.14f;

                                if (m_debugArrowMesh != null)
                                {
                                    Gizmos.DrawMesh(m_debugArrowMesh, centerPos, Quaternion.LookRotation(transform.forward, Vector3.up));
                                }
                                else
                                {
                                    Gizmos.DrawSphere(centerPos, 0.06f);
                                    Gizmos.DrawLine(centerPos, centerPosUp);
                                    DrawArrow.ForGizmo(centerPosUp, transform.forward * 0.2f, 0.05f, 20f);
                                }
                                doneCenter = 1;

                                Gizmos.color = new Color(0.5f, 0f, 0f);
                                Gizmos.DrawLine(centerPos, transform.TransformPoint(curGoal[i - 1].Position));
                                Gizmos.color = Color.red;
                                Gizmos.DrawLine(centerPos, transform.TransformPoint(curGoal[i].Position));
                            }

                            Gizmos.color = Color.red;
                        }

                        Vector3 pointPos = transform.TransformPoint(curPoint.Position);
                        float angle = curPoint.FacingAngle * Mathf.Deg2Rad;


                        Gizmos.DrawSphere(pointPos, 0.06f);
                        DrawArrow.ForGizmo(new Vector3(pointPos.x, pointPos.y + 0.14f, pointPos.z),
                            transform.TransformDirection(new Vector3(0.2f * Mathf.Sin(angle),
                            0f, 0.2f * Mathf.Cos(angle))), 0.05f, 20f);
                        Gizmos.DrawLine(pointPos, new Vector3(pointPos.x, pointPos.y + 0.14f, pointPos.z));

                        if (i != 0)
                        {
                            if (doneCenter != 1)
                                Gizmos.DrawLine(pointPos, transform.TransformPoint(curGoal[i - 1].Position));
                            else
                                doneCenter = 2;
                        }
                    }
                }
            }
        }

        //============================================================================================
        /**
        *  @brief Starts pose debugging in editor and creates the playable graph.
        *         
        *********************************************************************************************/
        public void StartPoseDebug(int a_animDataId)
        {
            if (m_animData.Length > 0 && a_animDataId < m_animData.Length)
                CurrentAnimData = m_animData[a_animDataId];

            if (CurrentAnimData != null)
            {
                p_animator = GetComponent<Animator>();

                m_animationStates = new MxMPlayableState[1];

                if (p_animator)
                {
                    MxMPlayableGraph = PlayableGraph.Create();
                    MxMPlayableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);

                    var playableOutput = AnimationPlayableOutput.Create(MxMPlayableGraph, "Animation", p_animator);
                    m_animationMixer = AnimationMixerPlayable.Create(MxMPlayableGraph, 1);
                    playableOutput.SetSourcePlayable(m_animationMixer);
                    m_animationMixer.SetInputWeight(0, 1f);

                    basePosition = transform.position;
                    baseRotation = transform.rotation;

                    m_debugPosesActive = true;
                }
                else
                {
                    m_debugPoses = false;
                    m_debugPosesActive = false;
                }
            }
        }

        //============================================================================================
        /**
        *  @brief Stops pose debugging and destroys the playable graph
        *         
        *********************************************************************************************/
        public void StopPoseDebug()
        {
            if (!m_debugPosesActive)
                return;

            if (m_animData.Length > 0 && m_animData[0] != null)
                CurrentAnimData = m_animData[0];

            if (CurrentAnimData != null)
            {

                m_debugPosesActive = false;

                PoseData pose = CurrentAnimData.Poses[0];
                AnimationClip clip = CurrentAnimData.Clips[pose.PrimaryClipId];

                var clipPlayable = m_animationMixer.GetInput(0);
                if (!clipPlayable.IsNull())
                {
                    m_animationMixer.DisconnectInput(0);
                    clipPlayable.Destroy();
                }

                clipPlayable = AnimationClipPlayable.Create(MxMPlayableGraph, clip);
                ((AnimationClipPlayable)clipPlayable).SetApplyFootIK(m_applyHumanoidFootIK);

                MxMPlayableGraph.Connect(clipPlayable, 0, m_animationMixer, 0);
                clipPlayable.SetTime(0.0);
                clipPlayable.SetTime(0.0);
                MxMPlayableGraph.Evaluate(0f);
                SceneView.RepaintAll();

                MxMPlayableGraph.Destroy();
                transform.SetPositionAndRotation(basePosition, baseRotation);
            }
        }

        //============================================================================================
        /**
        *  @brief Updates the pose debug data for the editor pose debugging functionality.
        *         
        *********************************************************************************************/
        public void UpdatePoseDebug()
        {
            var playable = m_animationMixer.GetInput(0);

            if(playable.IsValid())
            {
                MxMPlayableGraph.DestroySubgraph(playable);
            }

            transform.SetPositionAndRotation(basePosition, baseRotation);

            SetupPoseInSlot(ref CurrentAnimData.Poses[m_debugPoseId], 0, 1f);
            m_animationMixer.SetInputWeight(0, 1f);
            MxMPlayableGraph.Evaluate(0f);

            SceneView.RepaintAll();
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void BeginDebugPreview()
        {
            m_debugPreview = true;
            Pause();

            SetDebugPreviewFrame(DebugData.LastRecordedFrame);
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void EndDebugPreview()
        {
            m_debugPreview = false;
            UnPause();
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void SetDebugPreviewFrame(int a_debugFrame)
        {
            ref MxMDebugFrame debugFrame = ref DebugData.GetDebugFrame(a_debugFrame);

            PreviewDebugFrame(ref debugFrame);
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void PreviewDebugFrame(ref MxMDebugFrame a_debugFrame)
        {
            if (!a_debugFrame.Used)
                return;

            //Set Base Data State
            m_updateInterval = a_debugFrame.UpdateInterval;
            m_playbackSpeed = a_debugFrame.PlaybackSpeed;
            m_playbackSpeedSmoothRate = a_debugFrame.PlaybackSmoothRate;

            m_matchBlendTime = a_debugFrame.MatchBlendTime;
            m_applyTrajectoryBlending = a_debugFrame.ApplyTrajectoryBlending;
            m_trajectoryBlendingWeight = a_debugFrame.TrajectoryBlendingWeight;
            m_favourCurrentPose = a_debugFrame.FavourCurrentPose;
            m_currentPoseFavour = a_debugFrame.CurrentPoseFavour;
            m_angularWarpType = a_debugFrame.AngularWarpType;
            m_angularWarpMethod = a_debugFrame.AngularWarpMethod;
            m_angularErrorWarpRate = a_debugFrame.AngularWarpRate;
            m_angularErrorWarpThreshold = a_debugFrame.AngularWarpThreshold;
            m_angularErrorWarpAngleThreshold = a_debugFrame.AngularWarpAngleThreshold;
            m_longErrorWarpType = a_debugFrame.LongErrorWarpType;
            m_speedWarpLimits = a_debugFrame.SpeedWarpLimits;

            CurrentAnimData = m_animData[a_debugFrame.AnimDataId];

            if (m_calibrationOverride != null)
            {
                m_curCalibData = m_calibrationOverride.GetCalibrationSet(a_debugFrame.CalibrationDataId);
            }
            else
            {
                m_curCalibData = CurrentAnimData.CalibrationSets[a_debugFrame.CalibrationDataId];
            }
            m_primaryBlendChannel = a_debugFrame.PrimaryBlendChannel;
            m_dominantBlendChannel = a_debugFrame.DominantBlendChannel;

            Array.Copy(a_debugFrame.DesiredGoal, m_desiredGoal, m_desiredGoal.Length);
            Array.Copy(a_debugFrame.DesiredGoalBase, m_desiredGoalBase, m_desiredGoalBase.Length);

            m_timeSinceMotionUpdate = a_debugFrame.TimeSinceMotionUpdate;
            m_timeSinceMotionChosen = a_debugFrame.TimeSinceMotionChosen;
            DesiredPlaybackSpeed = a_debugFrame.DesiredPlaybackSpeed;
            m_enforcePoseSearch = a_debugFrame.EnforceClipChange;
            m_poseInterpolationValue = a_debugFrame.PoseInterpolationValue;

            MxMUtility.CopyPose(ref a_debugFrame.CurrentInterpolatedPose, ref m_curInterpolatedPose);

            LatErrorWarpAngle = a_debugFrame.LateralWarpAngle;
            LongErrorWarpScale = a_debugFrame.LongitudinalWarpScale;
            m_updateThisFrame = a_debugFrame.UpdateThisFrame;
            m_lastChosenCost = a_debugFrame.LastChosenCost;
            m_lastPoseCost = a_debugFrame.LastPoseCost;
            m_lastTrajectoryCost = a_debugFrame.LastTrajectoryCost;

            RequiredTags = a_debugFrame.RequiredTags;
            FavourTags = a_debugFrame.FavourTags;
            m_favourMultiplier = a_debugFrame.FavourTagMultiplier;

            m_curBlendSpaceChannel = a_debugFrame.CurrentBlendSpaceChannel;
            m_blendSpacePosition = a_debugFrame.BlendSpacePosition;
            m_desiredBlendSpacePosition = a_debugFrame.DesiredBlendSpacePosition;

            //Clear playable graph
            for (int i = 0; i < m_animationMixer.GetInputCount(); ++i)
            {
                Playable playable = m_animationMixer.GetInput(i);

                if (playable.IsValid())
                    PlayableUtils.DestroyPlayableRecursive(ref playable);
            }

            //Reconstruct playable graph
            float totalBlendPower = 0f;
            float totalBlendCount = 0;
            for(int i = 0; i < a_debugFrame.playableStates.Length; ++i)
            {
                ref MxMDebugFrame.PlayableStateDebugData debugPlayable = ref a_debugFrame.playableStates[i];
                ref MxMPlayableState animationState = ref m_animationStates[i];

                if (debugPlayable.BlendStatus != EBlendStatus.None)
                {
                    ref PoseData pose = ref CurrentAnimData.Poses[debugPlayable.StartPoseId];

                    SetupPoseInSlot(ref pose, i, debugPlayable.Speed);

                    Playable animPlayable = m_animationMixer.GetInput(i);

                    float animTime = debugPlayable.Age + pose.Time;
                    if (pose.AnimType == EMxMAnimtype.Composite)
                    {
                        ref readonly CompositeData compData = ref CurrentAnimData.Composites[pose.AnimId];

                        if(animTime > compData.ClipALength)
                            animTime -= compData.ClipALength;
                    }

                    MxMPlayableState.SetTimeRecursive(ref animationState.TargetPlayable, animTime);
                    
                    animationState.StartPoseId = debugPlayable.StartPoseId;
                    animationState.Age = debugPlayable.Age;
                    animationState.AnimId = debugPlayable.AnimId;
                    animationState.AnimType = debugPlayable.AnimType;
                    animationState.BlendStatus = debugPlayable.BlendStatus;
                    animationState.DecayAge = debugPlayable.DecayAge;
                    animationState.TargetPlayable = animPlayable;
                    animationState.Speed = debugPlayable.Speed;

                    ++totalBlendCount;
                    totalBlendPower += debugPlayable.Weight;
                }
                else
                {
                    animationState.BlendStatus = EBlendStatus.None;
                }
            }

            float blendNormalizeFactor = 1f / totalBlendPower;

            //Apply blend weights
            for(int i = 0; i < a_debugFrame.playableStates.Length; ++i)
            {
                ref MxMDebugFrame.PlayableStateDebugData debugPlayable = ref a_debugFrame.playableStates[i];

                if(debugPlayable.BlendStatus != EBlendStatus.None)
                    m_animationMixer.SetInputWeight(i, debugPlayable.Weight * blendNormalizeFactor);
            }

            //Events
            m_curEvent = a_debugFrame.CurrentEvent;
            m_eventType = a_debugFrame.EventType;
            m_curEventState = a_debugFrame.CurrentEventState;
            m_eventLength = a_debugFrame.EventLength;
            //m_timeSinceEventTriggered = a_debugFrame.TimeSinceEventTriggered;
            m_curEventPriority = a_debugFrame.CurrentEventPriority;
            m_exitEventWithMotion = a_debugFrame.ExitWithMotion;
            m_contactCountToWarp = a_debugFrame.ContactCountToWarp;
            WarpType = a_debugFrame.WarpType;
            RotWarpType = a_debugFrame.RotWarpType;
            TimeWarpType = a_debugFrame.TimeWarpType;
            m_eventStartTimeOffset = a_debugFrame.EventStartTimeOffset;
            m_curEventContactIndex = a_debugFrame.CurrentEventContactIndex;
            m_curEventContactTime = a_debugFrame.CurrentEventContactTime;
            m_linearWarpRate = a_debugFrame.LinearWarpRate;
            m_linearWarpRotRate = a_debugFrame.LinearWarpRotRate;
            m_currentEventRootWorld = a_debugFrame.CurrentEventRootWorld;
            m_desiredEventRootWorld = a_debugFrame.DesiredEventRootWorld;

            if(a_debugFrame.CurrentEventContacts == null)
            {
                CurEventContacts = null;
            }
            else
            {
                if(CurEventContacts == null ||
                    CurEventContacts.Length != a_debugFrame.CurrentEventContacts.Length)
                {
                    CurEventContacts = new EventContact[a_debugFrame.CurrentEventContacts.Length];
                }

                Array.Copy(a_debugFrame.CurrentEventContacts, CurEventContacts, a_debugFrame.CurrentEventContacts.Length);
            }

            PostEventTrajectoryMode = a_debugFrame.PostEventTrajectoryMode;
            m_cumActionDuration = a_debugFrame.CumulativeActionDuration;


            //Idle State
            m_curIdleSet = a_debugFrame.CurrentIdleSet;
            m_idleState = a_debugFrame.CurrentIdleState;
            m_curIdleSetId = a_debugFrame.CurrentIdleSetId;
            m_idleDetectTimer = a_debugFrame.IdleDetectTimer;
            m_lastSecondaryIdleClipId = a_debugFrame.LastSecondaryIdleClipId;
            m_timeSinceLastIdleStarted = a_debugFrame.TimeSinceLastIdleStarted;
            m_chosenIdleLoopCount = a_debugFrame.ChosenIdleLoopCount;

            //Set position of the character
            m_animationRoot.SetPositionAndRotation(a_debugFrame.Position, a_debugFrame.Rotation);
        }

        //============================================================================================
        /**
        *  @brief Records the current debug state to the MxMDebugger
        *         
        *********************************************************************************************/
        public void RecordDebugState()
        {
            ref MxMDebugFrame debugFrame = ref DebugData.GetNexDebugState();

            debugFrame.Used = true;
            debugFrame.Time = Time.time;
            debugFrame.DeltaTime = p_currentDeltaTime;
            debugFrame.Position = m_animationRoot.position;
            debugFrame.Rotation = m_animationRoot.rotation;

            debugFrame.AnimatorState = (EMxMStates)m_fsm.CurrentStateId;

            debugFrame.UpdateInterval = m_updateInterval;
            debugFrame.PlaybackSpeed = m_playbackSpeed;
            debugFrame.PlaybackSmoothRate = m_playbackSpeedSmoothRate;
            debugFrame.MatchBlendTime = m_matchBlendTime;
            debugFrame.ApplyTrajectoryBlending = m_applyTrajectoryBlending;
            debugFrame.TrajectoryBlendingWeight = m_trajectoryBlendingWeight;
            debugFrame.FavourCurrentPose = m_favourCurrentPose;
            debugFrame.CurrentPoseFavour = m_currentPoseFavour;
            debugFrame.AngularWarpType = m_angularWarpType;
            debugFrame.AngularWarpMethod = m_angularWarpMethod;
            debugFrame.AngularWarpRate = m_angularErrorWarpRate;
            debugFrame.AngularWarpThreshold = m_angularErrorWarpThreshold;
            debugFrame.AngularWarpAngleThreshold = m_angularErrorWarpAngleThreshold;
            debugFrame.LongErrorWarpType = m_longErrorWarpType;
            debugFrame.SpeedWarpLimits = m_speedWarpLimits;

            debugFrame.AnimDataId = 0;
            for (int i = 0; i < m_animData.Length; ++i)
            {
                if (CurrentAnimData == m_animData[i])
                {
                    debugFrame.AnimDataId = i;
                    break;
                }
            }

            debugFrame.CalibrationDataId = 0;

            if (m_calibrationOverride != null && m_calibrationOverride.IsCompatibleWith(CurrentAnimData))
            {
                debugFrame.CalibrationDataId = m_calibrationOverride.GetCalibrationHandle(m_curCalibData);
            }
            else
            { 
                for (int i = 0; i < CurrentAnimData.CalibrationSets.Length; ++i)
                {
                    if (m_curCalibData == CurrentAnimData.CalibrationSets[i])
                    {
                        debugFrame.CalibrationDataId = i;
                        break;
                    }
                }
            }

            debugFrame.PrimaryBlendChannel = m_primaryBlendChannel;
            debugFrame.DominantBlendChannel = m_dominantBlendChannel;

            for (int i = 0; i < m_animationStates.Length; ++i)
            {
                ref MxMPlayableState playableState = ref m_animationStates[i];

                ref MxMDebugFrame.PlayableStateDebugData debugPlayableState
                    = ref debugFrame.playableStates[i];

                debugPlayableState.BlendStatus = playableState.BlendStatus;

                if (playableState.BlendStatus != EBlendStatus.None)
                {
                    debugPlayableState.Weight = playableState.Weight;
                    debugPlayableState.StartPoseId = playableState.StartPoseId;
                    debugPlayableState.AnimId = playableState.AnimId;
                    debugPlayableState.AnimType = playableState.AnimType;
                    debugPlayableState.Age = playableState.Age;
                    debugPlayableState.DecayAge = playableState.DecayAge;
                    debugPlayableState.Speed = playableState.Speed;
                }
            }

            Array.Copy(m_desiredGoal, debugFrame.DesiredGoal, m_desiredGoal.Length);
            Array.Copy(m_desiredGoalBase, debugFrame.DesiredGoalBase, m_desiredGoalBase.Length);
            MxMUtility.CopyPose(ref m_curInterpolatedPose, ref debugFrame.CurrentInterpolatedPose);
            debugFrame.RequiredTags = RequiredTags;
            debugFrame.FavourTags = FavourTags;
            debugFrame.FavourTagMultiplier = m_favourMultiplier;

            debugFrame.TimeSinceMotionUpdate = m_timeSinceMotionUpdate;
            debugFrame.TimeSinceMotionChosen = m_timeSinceMotionChosen;
            debugFrame.DesiredPlaybackSpeed = DesiredPlaybackSpeed;
            debugFrame.EnforceClipChange = m_enforcePoseSearch;
            debugFrame.PoseInterpolationValue = m_poseInterpolationValue;

            debugFrame.LateralWarpAngle = LatErrorWarpAngle;
            debugFrame.LongitudinalWarpScale = LongErrorWarpScale;

            debugFrame.UpdateThisFrame = m_updateThisFrame;
            debugFrame.LastChosenCost = m_lastChosenCost;
            debugFrame.LastPoseCost = m_lastPoseCost;
            debugFrame.LastTrajectoryCost = m_lastTrajectoryCost;

            debugFrame.CurrentBlendSpaceChannel = m_curBlendSpaceChannel;
            debugFrame.BlendSpacePosition = m_blendSpacePosition;
            debugFrame.DesiredBlendSpacePosition = m_desiredBlendSpacePosition;

            //Events
            debugFrame.CurrentEvent = m_curEvent;
            debugFrame.EventType = m_eventType;
            debugFrame.CurrentEventState = m_curEventState;
            debugFrame.EventLength = m_eventLength;
            debugFrame.TimeSinceEventTriggered = m_timeSinceMotionChosen;
            debugFrame.CurrentEventPriority = m_curEventPriority;
            debugFrame.ExitWithMotion = m_exitEventWithMotion;
            debugFrame.ContactCountToWarp = m_contactCountToWarp;
            debugFrame.WarpType = WarpType;
            debugFrame.RotWarpType = RotWarpType;
            debugFrame.TimeWarpType = TimeWarpType;
            debugFrame.EventStartTimeOffset = m_eventStartTimeOffset;
            debugFrame.CurrentEventContactIndex = m_curEventContactIndex;
            debugFrame.CurrentEventContactTime = m_curEventContactTime;
            debugFrame.LinearWarpRate = m_linearWarpRate;
            debugFrame.LinearWarpRotRate = m_linearWarpRotRate;
            debugFrame.CurrentEventRootWorld = m_currentEventRootWorld;
            debugFrame.DesiredEventRootWorld = m_desiredEventRootWorld;

            if (CurEventContacts == null)
            {
                debugFrame.CurrentEventContacts = null;
            }
            else
            {
                if (debugFrame.CurrentEventContacts == null ||
                    debugFrame.CurrentEventContacts.Length != CurEventContacts.Length)
                {
                    debugFrame.CurrentEventContacts = new EventContact[CurEventContacts.Length];
                }

                Array.Copy(CurEventContacts, debugFrame.CurrentEventContacts, CurEventContacts.Length);
            }

            debugFrame.PostEventTrajectoryMode = PostEventTrajectoryMode;
            debugFrame.CumulativeActionDuration = m_cumActionDuration;


            //Idle State
            debugFrame.CurrentIdleSet = m_curIdleSet;
            debugFrame.CurrentIdleState = m_idleState;
            debugFrame.CurrentIdleSetId = m_curIdleSetId;
            debugFrame.IdleDetectTimer = m_idleDetectTimer;
            debugFrame.LastSecondaryIdleClipId = m_lastSecondaryIdleClipId;
            debugFrame.TimeSinceLastIdleStarted = m_timeSinceLastIdleStarted;
            debugFrame.ChosenIdleLoopCount = m_chosenIdleLoopCount;

            //Layers
            if (debugFrame.Layers == null || debugFrame.Layers.Length != m_layers.Count)
                debugFrame.Layers = new MxMDebugFrame.LayerData[m_layers.Count];

            int debugLayerIndex = 0;
            foreach(KeyValuePair<int, MxMLayer> layerPair in m_layers)
            {
                ref MxMDebugFrame.LayerData debugLayer = ref debugFrame.Layers[debugLayerIndex];
                MxMLayer layer = layerPair.Value;

                debugLayer.LayerId = layer.Id;
                debugLayer.Weight = layer.Weight;
                debugLayer.Additive = layer.Additive;
                debugLayer.Time = layer.PrimaryClipTime;
                debugLayer.Clip = layer.PrimaryClip;
                debugLayer.Mask = layer.Mask;
                debugLayer.MaxClips = layer.MaxClips;
                debugLayer.PrimaryInputId = layer.PrimaryInputId;

                if (debugLayer.SubLayerWeights == null || debugLayer.SubLayerWeights.Length != layer.SubLayerWeights.Length)
                    debugLayer.SubLayerWeights = new float[layer.SubLayerWeights.Length];

                Array.Copy(layer.SubLayerWeights, debugLayer.SubLayerWeights, layer.SubLayerWeights.Length);

                ++debugLayerIndex;
            }

            debugFrame.MecanimLayer = (p_animator.runtimeAnimatorController != null);
            debugFrame.MecanimLayerWeight = m_animationLayerMixer.GetInputWeight(1);
            debugFrame.MecanimMask = m_animatorControllerMask;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void StartRecordAnalytics()
        {
            m_recordAnalytics = true;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void StopRecordAnalytics()
        {
            m_recordAnalytics = false;
        }

        //============================================================================================
        /**
        *  @brief Generates a pose mask for the current animation data based on recorded pose utilisation
        *         
        *********************************************************************************************/
        public void GeneratePoseMask()
        {
            DebugData.GeneratePoseMask();
        }

        //=========================================================================m===================
        /**
        *  @brief Deletes the pose mask from all MxMAnimData currently connected to the MxMAnimator
        *         
        *********************************************************************************************/
        public void ClearPoseMask()
        {
            foreach (MxMAnimData animData in m_animData)
            {
                animData.StripPoseMask();
                EditorUtility.SetDirty(animData);
            }
        }

        //============================================================================================
        /**
        *  @brief Logs pose utilisation analytics to the Unity editor console
        *         
        *********************************************************************************************/
        public void DumpAnalytics()
        {
            DebugData.DumpUsedPoseAnalytics();
        }
#endif

    }//End of class: MxMAnimator
}//End of namespace: MxM
#endif
