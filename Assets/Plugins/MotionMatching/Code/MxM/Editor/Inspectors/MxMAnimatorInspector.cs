// ================================================================================================
// File: MxMAnimatorInspector.cs
// 
// Authors:  Kenneth Claassen
// Date:     2018-05-31: Created this file.
// 
//     Contains a part of the 'MxMEditor' namespace for 'Unity Engine 2018'.
// 
// Copyright (c) 2018 Kenneth Claassen. All rights reserved.
// ================================================================================================
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

using MxM;
using UnityEngine.Scripting;

namespace MxMEditor
{
    //============================================================================================
    /**
    *  @brief Inspector class for drawing and managing the MxMAnimator component in the Unity Editor
    *         
    *********************************************************************************************/
    [CustomEditor(typeof(MxMAnimator))]
    public class MxMAnimatorInspector : Editor
    {
        private SerializedProperty m_spDIYPlayableGraph;
        private SerializedProperty m_spAutoCreateAnimatorController;
        private SerializedProperty m_spRootMotionMode;
        private SerializedProperty m_spTransitionMethod;
        private SerializedProperty m_spUpdateInterval;
        private SerializedProperty m_spPlaybackSpeed;
        private SerializedProperty m_spPlaybackSpeedSmoothRate;
        private SerializedProperty m_spMatchBlendTime;
        private SerializedProperty m_spTurnInPlaceThreshold;
        private SerializedProperty m_spEnableIdle;
        
        private SerializedProperty m_spAnimationRootOverride;
        private SerializedProperty m_spAnimData;
        private SerializedProperty m_spCalibrationOverride;
        private SerializedProperty m_spMaxMixCount;

        private SerializedProperty m_spDebugGoal;
        private SerializedProperty m_spDebugChosenTrajectory;
        private SerializedProperty m_spDebugPoses;
        private SerializedProperty m_spDebugCurrentPose;
        private SerializedProperty m_spDebugAnimDataId;
        private SerializedProperty m_spDebugPoseId;

        private SerializedProperty m_spOverrideWarpSettings;
        private SerializedProperty m_spAngularWarpType;
        private SerializedProperty m_spAngularWarpMethod;
        private SerializedProperty m_spLongErrorWarpType;
        private SerializedProperty m_spSpeedWarpLimits;

        private SerializedProperty m_spPoseMatchMethod;
        private SerializedProperty m_spFavourTagMethod;
        private SerializedProperty m_spPastTrajectoryMode;
        private SerializedProperty m_spApplyHumanoidFootIK;
        private SerializedProperty m_spApplyPlayableIK;
        private SerializedProperty m_spMinFootstepInterval;
        
        private SerializedProperty m_spAngularErrorWarpRate;
        private SerializedProperty m_spAngularErrorWarpThreshold;
        private SerializedProperty m_spAngularErrorWarpAngleThreshold;
        private SerializedProperty m_spAngularErrorWarpMinAngleThreshold;
        private SerializedProperty m_spAngularErrorWarpSmoothing;
        private SerializedProperty m_spApplyTrajectoryBlending;
        private SerializedProperty m_spTrajectoryBlendingWeight;
        private SerializedProperty m_spFavourCurrentPose;
        private SerializedProperty m_spTransformGoal;
        private SerializedProperty m_spPoseFavourFactor;
        private SerializedProperty m_spTagBlendMethod;
        private SerializedProperty m_spBlendSpaceSmoothing;
        private SerializedProperty m_spBlendSpaceSmoothRate;
        private SerializedProperty m_spAnimatorControllerMask;
        private SerializedProperty m_spAnimControllerLayer;
        private SerializedProperty m_spNextPoseToleranceTest;
        private SerializedProperty m_spNextPoseToleranceDist;
        private SerializedProperty m_spNextPoseToleranceAngle;
        private SerializedProperty m_spBlendOutEarly;
        
        private SerializedProperty m_spOnSetupCompleteCallback;
        private SerializedProperty m_spOnIdleTriggeredCallback;
        private SerializedProperty m_spOnIdleEndCallback;
        private SerializedProperty m_spOnRequiredTagsChangedCallback;
        private SerializedProperty m_spOnLeftFootStepStartCallback;
        private SerializedProperty m_spOnRightFootStepStartCallback;
        private SerializedProperty m_spOnEventCompleteCallback;
        private SerializedProperty m_spOnEventContactCallback;
        private SerializedProperty m_spOnEventChangeStateCallback;
        private SerializedProperty m_spOnPoseChangedCallback;

        private SerializedProperty m_spPriorityUpdate;
        private SerializedProperty m_spMaxUpdateDelay;
       
        private SerializedProperty m_spGeneralFoldout;
        private SerializedProperty m_spAnimDataFoldout;
        private SerializedProperty m_spOptionsFoldout;
        private SerializedProperty m_spWarpingFoldout;
        private SerializedProperty m_spOptimisationFoldout;
        private SerializedProperty m_spCallbackFoldout;
        private SerializedProperty m_spDebugFoldout;
        
        private MxMAnimData m_animData;
        private MxMAnimator m_animator;


        private ReorderableList m_animDataReorderableList;

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void OnEnable()
        {
            m_animator = target as MxMAnimator;

            m_spDIYPlayableGraph = serializedObject.FindProperty("p_DIYPlayableGraph");
            m_spAutoCreateAnimatorController = serializedObject.FindProperty("m_autoCreateAnimatorController");
            m_spAnimatorControllerMask = serializedObject.FindProperty("m_animatorControllerMask");
            m_spAnimControllerLayer = serializedObject.FindProperty("m_animControllerLayer");
            m_spRootMotionMode = serializedObject.FindProperty("m_rootMotionMode");
            m_spTransitionMethod = serializedObject.FindProperty("m_transitionMethod");
            m_spUpdateInterval = serializedObject.FindProperty("m_updateInterval");
            m_spPlaybackSpeed = serializedObject.FindProperty("m_playbackSpeed");
            m_spPlaybackSpeedSmoothRate = serializedObject.FindProperty("m_playbackSpeedSmoothRate");
            m_spMatchBlendTime = serializedObject.FindProperty("m_matchBlendTime");
            m_spTurnInPlaceThreshold = serializedObject.FindProperty("m_turnInPlaceThreshold");
            m_spEnableIdle = serializedObject.FindProperty("m_enableIdle");
            
            m_spAnimData = serializedObject.FindProperty("m_animData");
            m_spCalibrationOverride = serializedObject.FindProperty("m_calibrationOverride");
            m_spAnimationRootOverride = serializedObject.FindProperty("m_animationRoot");
            m_spMaxMixCount = serializedObject.FindProperty("m_maxMixCount");

            m_spDebugGoal = serializedObject.FindProperty("m_debugGoal");
            m_spDebugChosenTrajectory = serializedObject.FindProperty("m_debugChosenTrajectory");
            m_spDebugPoses = serializedObject.FindProperty("m_debugPoses");
            m_spDebugCurrentPose = serializedObject.FindProperty("m_debugCurrentPose");
            m_spDebugAnimDataId = serializedObject.FindProperty("m_debugAnimDataId");
            m_spDebugPoseId = serializedObject.FindProperty("m_debugPoseId");

            m_spOverrideWarpSettings = serializedObject.FindProperty("m_overrideWarpSettings");
            m_spAngularWarpType = serializedObject.FindProperty("m_angularWarpType");
            m_spAngularWarpMethod = serializedObject.FindProperty("m_angularWarpMethod");
            m_spLongErrorWarpType = serializedObject.FindProperty("m_longErrorWarpType");
            m_spSpeedWarpLimits = serializedObject.FindProperty("m_speedWarpLimits");

            m_spBlendOutEarly = serializedObject.FindProperty("m_blendOutEarly");
            m_spPoseMatchMethod = serializedObject.FindProperty("m_poseMatchMethod");
            m_spFavourTagMethod = serializedObject.FindProperty("m_favourTagMethod");
            m_spPastTrajectoryMode = serializedObject.FindProperty("m_pastTrajectoryMode");
            m_spApplyHumanoidFootIK = serializedObject.FindProperty("m_applyHumanoidFootIK");
            m_spApplyPlayableIK = serializedObject.FindProperty("m_applyPlayableIK");
            m_spMinFootstepInterval = serializedObject.FindProperty("m_minFootstepInterval");
            
            m_spAngularErrorWarpRate = serializedObject.FindProperty("m_angularErrorWarpRate");
            m_spAngularErrorWarpThreshold = serializedObject.FindProperty("m_angularErrorWarpThreshold");
            m_spAngularErrorWarpAngleThreshold = serializedObject.FindProperty("m_angularErrorWarpAngleThreshold");
            m_spAngularErrorWarpMinAngleThreshold = serializedObject.FindProperty("m_angularErrorWarpMinAngleThreshold");
            m_spApplyTrajectoryBlending = serializedObject.FindProperty("m_applyTrajectoryBlending");
            m_spTrajectoryBlendingWeight = serializedObject.FindProperty("m_trajectoryBlendingWeight");
            m_spFavourCurrentPose = serializedObject.FindProperty("m_favourCurrentPose");
            m_spTransformGoal = serializedObject.FindProperty("m_transformGoal");
            m_spPoseFavourFactor = serializedObject.FindProperty("m_currentPoseFavour");
            m_spTagBlendMethod = serializedObject.FindProperty("m_tagBlendMethod");
            m_spBlendSpaceSmoothing = serializedObject.FindProperty("m_blendSpaceSmoothing");
            m_spBlendSpaceSmoothRate = serializedObject.FindProperty("m_blendSpaceSmoothRate");
            m_spNextPoseToleranceTest = serializedObject.FindProperty("m_nextPoseToleranceTest");
            m_spNextPoseToleranceDist = serializedObject.FindProperty("m_nextPoseToleranceDist");
            m_spNextPoseToleranceAngle = serializedObject.FindProperty("m_nextPoseToleranceAngle");
            
            m_spOnSetupCompleteCallback = serializedObject.FindProperty("m_onSetupComplete");
            m_spOnIdleTriggeredCallback = serializedObject.FindProperty("m_onIdleTriggered");
            m_spOnIdleEndCallback = serializedObject.FindProperty("m_onIdleEnd");
            m_spOnRequiredTagsChangedCallback = serializedObject.FindProperty("m_onRequireTagsChanged");
            m_spOnLeftFootStepStartCallback = serializedObject.FindProperty("m_onLeftFootStepStart");
            m_spOnRightFootStepStartCallback = serializedObject.FindProperty("m_onRightFootStepStart");
            m_spOnEventCompleteCallback = serializedObject.FindProperty("m_onEventComplete");
            m_spOnEventContactCallback = serializedObject.FindProperty("m_onEventContactReached");
            m_spOnEventChangeStateCallback = serializedObject.FindProperty("m_onEventStateChanged");
            m_spOnPoseChangedCallback = serializedObject.FindProperty("m_onPoseChanged");

            m_spPriorityUpdate = serializedObject.FindProperty("m_priorityUpdate");
            m_spMaxUpdateDelay = serializedObject.FindProperty("m_maxUpdateDelay");

            m_spGeneralFoldout = serializedObject.FindProperty("m_generalFoldout");
            m_spAnimDataFoldout = serializedObject.FindProperty("m_animDataFoldout");
            m_spOptionsFoldout = serializedObject.FindProperty("m_optionsFoldout");
            m_spWarpingFoldout = serializedObject.FindProperty("m_warpingFoldout");
            m_spOptimisationFoldout = serializedObject.FindProperty("m_optimisationFoldout");
            m_spCallbackFoldout = serializedObject.FindProperty("m_debugFoldout");
            m_spDebugFoldout = serializedObject.FindProperty("m_callbackFoldout");
            

            if (m_spAnimData.arraySize == 0)
                m_spAnimData.InsertArrayElementAtIndex(0);

            if (m_spAnimData.arraySize > 0)
            {
                SerializedProperty spAnimData = m_spAnimData.GetArrayElementAtIndex(0);
                if (spAnimData != null)
                {
                    m_animData = m_spAnimData.GetArrayElementAtIndex(0).objectReferenceValue as MxMAnimData;
                }
            }

            if (m_spDebugPoses.boolValue)
                m_animator.StartPoseDebug(m_spDebugAnimDataId.intValue);

            m_animDataReorderableList = new ReorderableList(serializedObject, m_spAnimData,
                true, true, true, true);

            m_animDataReorderableList.drawElementCallback =
                (Rect a_rect, int a_index, bool a_isActive, bool a_isFocused) =>
                {
                    var element = m_animDataReorderableList.serializedProperty.GetArrayElementAtIndex(a_index);

                    EditorGUI.LabelField(new Rect(a_rect.x, a_rect.y, 100f, EditorGUIUtility.singleLineHeight),
                        "Anim Data " + (a_index + 1).ToString());
                    EditorGUI.ObjectField(new Rect(a_rect.x + 100f, a_rect.y, EditorGUIUtility.currentViewWidth - 170f,
                        EditorGUIUtility.singleLineHeight), element, new GUIContent(""));
                };

            m_animDataReorderableList.drawHeaderCallback =
                (Rect a_rect) =>
                {
                    EditorGUI.LabelField(a_rect, "Anim Data");
                };

            m_animDataReorderableList.onRemoveCallback =
                (ReorderableList a_list) =>
                {
                    if(a_list.index >= 0 && a_list.index < a_list.serializedProperty.arraySize)
                    {
                        SerializedProperty spObject = a_list.serializedProperty.GetArrayElementAtIndex(a_list.index);

                        if(spObject.objectReferenceValue != null)
                        {
                            spObject.objectReferenceValue = null;
                        }
                    }

                    ReorderableList.defaultBehaviours.DoRemoveButton(a_list);
                };

            serializedObject.ApplyModifiedProperties();

            if(EditorApplication.isPlaying)
                SetDebuggerTarget();

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void OnDisable()
        {
            if (m_spDebugPoses.boolValue)
            {
                m_animator.StopPoseDebug();
                m_spDebugPoses.boolValue = false;

                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if(state == PlayModeStateChange.EnteredPlayMode)
                SetDebuggerTarget();
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void SetDebuggerTarget()
        {
            if (m_animator != null && MxMDebuggerWindow.Instance != null)
                MxMDebuggerWindow.SetTarget(m_animator, false);
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public override void OnInspectorGUI()
        {
            MxMSettings MxMSettings = MxMSettings.Instance();

            if (m_animData == null)
            {
                if (m_spAnimData.arraySize > 0)
                {
                    SerializedProperty spAnimData = m_spAnimData.GetArrayElementAtIndex(0);
                    if (spAnimData != null)
                    {
                        m_animData = m_spAnimData.GetArrayElementAtIndex(0).objectReferenceValue as MxMAnimData;
                    }
                }
            }

            MxMSettings.HelpActive = EditorGUILayout.ToggleLeft(new GUIContent("Help", "Displays help info and links for the pre-process properties"), MxMSettings.HelpActive);
            Rect lastRect = GUILayoutUtility.GetLastRect();

            float curHeight = lastRect.y + 9f;

            curHeight = EditorUtil.EditorFunctions.DrawTitle("MxM Animator", curHeight);

            if(MxMSettings.HelpActive)
            {
                EditorGUILayout.HelpBox("This component is used to run motion matching on your character model. It should be placed alongside the " +
                    "Animation component and it also requires a TrajectoryGenerator component attached along with it. Based on the generated trajectory, " +
                    "the slotted AnimData and settings on this component, animation will be dynamically synthesised each frame.", MessageType.Info);

                lastRect = GUILayoutUtility.GetLastRect();
                curHeight = lastRect.y + 9f;
                GUILayout.Space(9f);
            }

            m_spGeneralFoldout.boolValue = EditorUtil.EditorFunctions.DrawFoldout("General", curHeight, EditorGUIUtility.currentViewWidth, m_spGeneralFoldout.boolValue);
            if (m_spGeneralFoldout.boolValue)
            {
                EditorGUILayout.Slider(m_spPlaybackSpeed, 0f, 3f, new GUIContent("PlaybackSpeed"));

                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("The playback speed is how fast the animation should play. By default this is 1.0f but can be reduced " +
                        "to slow animation and increased to speed animation up. Please note that speed modification will degrade animation quality.", MessageType.Info);
                }

                EditorGUI.BeginChangeCheck();
                m_spPlaybackSpeedSmoothRate.floatValue = EditorGUILayout.FloatField(new GUIContent("Playback Smooth Rate"), m_spPlaybackSpeedSmoothRate.floatValue);
                if(EditorGUI.EndChangeCheck())
                {
                    if (m_spPlaybackSpeedSmoothRate.floatValue <= 0.01f)
                        m_spPlaybackSpeedSmoothRate.floatValue = 0.01f;
                }

                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("Abrupt changes in playback rate may be jarring. This setting causes any runtime change in the playback speed " +
                        "(by setting 'DesiredPlaybackSpeed') to be smoothed over time instead of instant.", MessageType.Info);
                }

                m_spMaxMixCount.intValue = EditorGUILayout.IntField("Max Blend Channels", m_spMaxMixCount.intValue);

                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("The maximum number of channels or animations that can be blending at any given time with " +
                        "motion matching. This is an advanced setting for optimization and should be left as default most of the time.", MessageType.Info);
                }

                EditorGUI.BeginChangeCheck();
                float updateRate = EditorGUILayout.Slider(new GUIContent("Update Rate (Hz)", "The rate at which motion matching searches take place (not the FPS!)"), 1f / m_spUpdateInterval.floatValue, 1f, 144f);
                if(EditorGUI.EndChangeCheck())
                {
                    m_spUpdateInterval.floatValue = 1f / updateRate;
                }

                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("The frequency of motion matching searches. Note this is not the frame rate of your animation, it is simply the " +
                        "rate at which MxM will search the pose database for a match. Performing this search every frame is unnecessary, bad for performance and does not alway improve results. " +
                        "An update rate of 15Hz - 30Hz is recommended for a normal LOD character. It could be reduced for characters far away.", MessageType.Info);
                }

                EditorGUILayout.Slider(m_spMatchBlendTime, 0f, 2f, new GUIContent("Blend Time"));

                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("The amount of time to blend between animations. A value between 0.2 - 0.5 is recommended. Longer blends can smooth out" +
                        " lacking animation coverage but reduce the animation quality. Faster blends could be abrupt but provide better results with dense animation data.", MessageType.Info);
                }

                EditorGUILayout.Slider(m_spTurnInPlaceThreshold, 0f, 360f, new GUIContent("Turn in place threshold"));

                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("An angle threshold for turn-in-place animation. Once the turn is greater than this amount (degrees), the idle state will " +
                        "be exited and motion matching will attempt to follow the trajectory.", MessageType.Info);
                }

                m_spEnableIdle.boolValue = EditorGUILayout.Toggle(new GUIContent("Enable Idle"), m_spEnableIdle.boolValue);

                lastRect = GUILayoutUtility.GetLastRect();
                curHeight = lastRect.y;

                //curHeight += 18f * 10f;
            }

            curHeight += 30f;
            GUILayout.Space(3f);

            m_spAnimDataFoldout.boolValue = EditorUtil.EditorFunctions.DrawFoldout("Animation Data", curHeight, EditorGUIUtility.currentViewWidth, m_spAnimDataFoldout.boolValue);

            

            bool greyOutData = false;
            if (m_spAnimDataFoldout.boolValue)
            {
                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("This is where all animation data (created via the pre-processor stage) should be slotted in. In most cases there should only " +
                        "be one MxMAnimData slotted unless there are certain 'states' where the character matches different bones. The MxMAnimator does not automatically use" +
                        "all MxMAnimData and a manual switch is required. (e.g. switch to a climbing set which matches hands instead of feet.", MessageType.Info);
                }

                m_animDataReorderableList.DoLayoutList();

                GUIStyle redBoldStyle = new GUIStyle(GUI.skin.label);
                redBoldStyle.fontStyle = FontStyle.Bold;
                redBoldStyle.normal.textColor = Color.red;

                for (int i=0; i < m_spAnimData.arraySize; ++i)
                {
                    SerializedProperty spAnimData = m_spAnimData.GetArrayElementAtIndex(i);

                    if(spAnimData.objectReferenceValue == null)
                    {
                        EditorGUILayout.HelpBox("You have NULL anim data in the list. Please remove or fill the null entry in the list.", MessageType.Error);

                        curHeight += 18f;
                        greyOutData = true;
                    }
                }

                if (m_spAnimData.arraySize == 0)
                {
                    EditorGUILayout.HelpBox("Please add anim data into the list above.", MessageType.Error);
                    curHeight += 18f;
                    greyOutData = false;
                }

                curHeight += 18f + 5f;
                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(m_spCalibrationOverride);

                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("This property allows for calibration to be done in a standalone module which overrides the calibration on " +
                        "the MxMAnimData. This workflow is recommended for ease of use and modularity.", MessageType.Info);
                }
            }

            curHeight += 31f;
            GUILayout.Space(3f);

            if (greyOutData)
            {
                GUI.enabled = false;
            }

            m_spOptionsFoldout.boolValue = EditorUtil.EditorFunctions.DrawFoldout("Options",
                curHeight, EditorGUIUtility.currentViewWidth, m_spOptionsFoldout.boolValue);

            if (m_spOptionsFoldout.boolValue)
            {
                EditorGUILayout.ObjectField(m_spAnimationRootOverride, typeof(Transform), new GUIContent("Animation Root Override",
                    "The root transform for animation. Leaveing this blank will use the transform of the MxMAnimator."));

                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("The root transform of the character which should be moved with root motion. Set this to your character's " +
                        "transform if the MxMAnimator is not palced on the root gameobject of your character.", MessageType.Info);
                }

                EditorGUILayout.ObjectField(m_spAnimatorControllerMask, typeof(AvatarMask), new GUIContent("Controller Mask",
                    "The mask that is placed here will be applied to the mecanim animator controller if you are using one."));

                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("If you are using a mecanim controller alongside MxM, this mask can be set to apply a mask to the controller" +
                        "layer, effectively enabling you to use mecanim for layered animation while MxM handles locomotion.", MessageType.Info);
                }

                EditorGUI.BeginChangeCheck();
                m_spAnimControllerLayer.intValue = EditorGUILayout.IntField(new GUIContent("Controller Layer",
                    "Use this to choose which layer the animator controller should be placed on."), m_spAnimControllerLayer.intValue);
                if(EditorGUI.EndChangeCheck())
                {
                    if (m_spAnimControllerLayer.intValue < 1)
                        m_spAnimControllerLayer.intValue = 1;
                }

                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("The layer Id to place mecanim. By default this is layer 1 (just over MxM). If you want to add your own layers " +
                        "beneach mecanim, you must set this value higher to allow the correct number of slots.", MessageType.Info);
                }

                GUILayout.Space(10f);
                curHeight += 10f;

                m_spPoseMatchMethod.enumValueIndex = (int)(EPoseMatchMethod)EditorGUILayout.EnumPopup(new GUIContent(
                    "Pose Match Method", "The method to use for pose matching"), (EPoseMatchMethod)m_spPoseMatchMethod.enumValueIndex);

                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("The method of pose matching. 'Velocity Costing' provides higher quality results but is less perforamnt.", MessageType.Info);
                }

                m_spFavourTagMethod.enumValueIndex = (int)(EFavourTagMethod)EditorGUILayout.EnumPopup(new GUIContent(
                    "Favour Tag Method", "The method to process favour tags with"), (EFavourTagMethod)m_spFavourTagMethod.enumValueIndex);

                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("The method to use for favour tagging. Please see the user manual for precise details.", MessageType.Info);
                }

                m_spRootMotionMode.enumValueIndex = (int)(EMxMRootMotion)EditorGUILayout.EnumPopup(new GUIContent(
                    "Root Motion", "How to handle root motion"), (EMxMRootMotion)m_spRootMotionMode.enumValueIndex);

                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("How root motion should be handled. It can be turned off (Off), applied directly to the transform (On), " +
                        "or delegated to a RootMotionAplicator (see MxMRootMotionApplicator component) to apply the movement to a capsule controller.", MessageType.Info);
                }

                m_spPastTrajectoryMode.intValue = (int)(EPastTrajectoryMode)EditorGUILayout.EnumPopup(
                    "Past Trajectory Mode", (EPastTrajectoryMode)m_spPastTrajectoryMode.intValue);

                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("How the past trajectory should be handled. Normally the past trajectory can be attained by recording positions, " +
                        "however, if root motion isn't being used or the speed is being warped, this should be set to 'Copy From Current Pose'.", MessageType.Info);
                }

                m_spTagBlendMethod.enumValueIndex = (int)(ETagBlendMethod)EditorGUILayout.EnumPopup(
                    new GUIContent("Tag Blend Method", "How tags should be decided during pose interpolation"),
                    (ETagBlendMethod) m_spTagBlendMethod.enumValueIndex);

                m_spBlendSpaceSmoothing.enumValueIndex = (int)(EBlendSpaceSmoothing)EditorGUILayout.EnumPopup(
                    new GUIContent("Blend Space Smoothing", "How blend space smoothing should operate"),
                    (EBlendSpaceSmoothing)m_spBlendSpaceSmoothing.enumValueIndex);

                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("The method to use for smoothing blend spaces. Note that this does not apply to scatter spaces, only blend spaces " +
                        "that are used explicitly with 'BeginBlendLoop(...).", MessageType.Info);
                }

                switch (m_spBlendSpaceSmoothing.intValue)
                {
                    case (int)EBlendSpaceSmoothing.Lerp:
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(15f);

                            EditorGUI.BeginChangeCheck();
                            float smoothRate = EditorGUILayout.FloatField(new GUIContent("Smooth Rate"),
                                m_spBlendSpaceSmoothRate.vector2Value.x);

                            if(EditorGUI.EndChangeCheck())
                            {
                                m_spBlendSpaceSmoothRate.vector2Value = new Vector2(smoothRate, smoothRate);
                            }
                            EditorGUILayout.EndHorizontal();

                            curHeight += 18f;
                        }
                        break;
                    case (int)EBlendSpaceSmoothing.Lerp2D:
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(15f);
                            m_spBlendSpaceSmoothRate.vector2Value = EditorGUILayout.Vector2Field(new GUIContent("Smooth Rate"),
                                m_spBlendSpaceSmoothRate.vector2Value);
                            EditorGUILayout.EndHorizontal();

                            
                        }
                        break;
                }

                EditorGUI.BeginChangeCheck();
                m_spTransitionMethod.intValue = (int)(ETransitionMethod)EditorGUILayout.EnumPopup(
                    "Transition Method", (ETransitionMethod)m_spTransitionMethod.intValue);
                if (EditorGUI.EndChangeCheck())
                {
                    if (m_spTransitionMethod.intValue == (int)ETransitionMethod.None)
                        m_spBlendOutEarly.boolValue = false;
                }

                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("The method of transitioning between animations. Currently the only options here are for blending and " +
                        "not blending so it is unlikely that you will need to change this.", MessageType.Info);
                }

                
                if (m_spTransitionMethod.intValue == (int)ETransitionMethod.Blend)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15f);
                    m_spBlendOutEarly.boolValue = EditorGUILayout.Toggle(new GUIContent("Blend Out Early", "If true, animations will be " +
                        "forced to change and blend out before they reach their end."), m_spBlendOutEarly.boolValue);
                    EditorGUILayout.EndHorizontal();

                    if (MxMSettings.HelpActive)
                    {
                        EditorGUILayout.HelpBox("Enabling 'Blend Out Early' will force the system to pick a new animation and start blending " +
                            "out the current one if it is nearing it's end. This has some impact on cut clips. Try with and without this setting " +
                            "and use the setting that works best for you.", MessageType.Info);
                    }

                    curHeight += 18f;
                }
                

                GUILayout.Space(10f);
                curHeight += 10f;

                m_spTransformGoal.boolValue = EditorGUILayout.Toggle(new GUIContent("Transform Goal"),
                    m_spTransformGoal.boolValue);

                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("Causes the MxMAnimator to transform the trajectory (goal) it receives from the MxMTrajectory into the " +
                        "space of the character. Most of the time this should remain checked. However, if you write your own custom trajectory " +
                        "generator that already transforms the trajectory into the character space then un-check this.", MessageType.Info);
                }

                //m_spApplyBlending.boolValue = EditorGUILayout.Toggle(new GUIContent("Apply Blending"), m_spApplyBlending.boolValue);
                m_spApplyTrajectoryBlending.boolValue = EditorGUILayout.Toggle(new
                    GUIContent("Apply Trajectory Blending"), m_spApplyTrajectoryBlending.boolValue);

                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("Blends the current desired trajectory with the trajectory of the current animation with a progressive " +
                        "fall-off with prediction time. This can give for smoother, more realistic trajectories but may result in less responsive " +
                        "animation in some cases.", MessageType.Info);
                }

                if (m_spApplyTrajectoryBlending.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15f);

                    m_spTrajectoryBlendingWeight.floatValue = EditorGUILayout.FloatField("Trajectory Blend Weight", 
                        m_spTrajectoryBlendingWeight.floatValue);

                    if (MxMSettings.HelpActive)
                    {
                        EditorGUILayout.HelpBox("A multiplier to the blending weights for trajectory blending. This can be used to tone down " +
                            "the effect to mitigate the less responsive animation while still getting a slightly more realistic trajectory. Values " +
                            "of 1 mean full blending, while lesser values (toward 0) tone down the trajectory blending progressively.", MessageType.Info);
                    }

                    EditorGUILayout.EndHorizontal();

                    curHeight += 18f;
                }

                m_spFavourCurrentPose.boolValue = EditorGUILayout.Toggle(
                    new GUIContent("Favour Current Pose"), m_spFavourCurrentPose.boolValue);

                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("Should the current pose be favoured? This helps prevent to much random jumping around in the" +
                        "animation data. It applies a multiplier (see below) to the cost of the current pose when doing the pose search. This can" +
                        "make for more realistic animation. However, if the multiplier is too low then it can reduce responsiveness. This settings " +
                        "is NOT recommended for a cut-clip setup.", MessageType.Info);
                }

                if (m_spFavourCurrentPose.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15f);
                    m_spPoseFavourFactor.floatValue = EditorGUILayout.FloatField(
                        new GUIContent("Pose Favour Factor"), m_spPoseFavourFactor.floatValue);
                    EditorGUILayout.EndHorizontal();

                    if (MxMSettings.HelpActive)
                    {
                        EditorGUILayout.HelpBox("The multiplier to use for current pose favouring. A value of 1 means no favouring, while lower " +
                            "value multipliers favour the pose more. Values greater than one make the current pose less likely to be picked again " +
                            "and this is not recommended.", MessageType.Info);
                    }

                    curHeight += 18f;
                }

                m_spNextPoseToleranceTest.boolValue = EditorGUILayout.Toggle(new GUIContent("Next Pose Tolerance Test"), m_spNextPoseToleranceTest.boolValue);

                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("Next pose tolerance is an optimization where it checks if the next pose in your current animation " +
                        "sequence is 'good enough'. If it is deemed good enough (based on the below 2 settings) then the pose search will not run" +
                        "saving on performance and potentially resulting in smoother animation (particularly on loops).", MessageType.Info);
                }

                if (m_spNextPoseToleranceTest.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15f);
                    EditorGUILayout.BeginVertical();

                    m_spNextPoseToleranceDist.floatValue = EditorGUILayout.FloatField("Distance Tolerance", m_spNextPoseToleranceDist.floatValue);
                    if (MxMSettings.HelpActive)
                    {
                        EditorGUILayout.HelpBox("The maximum allowable trajectory positional error tolerance for the next pose to be considered 'good enough' " +
                            "if the total trajectory error of the next pose exceeds this, a pose search will be triggered.", MessageType.Info);
                    }


                    m_spNextPoseToleranceAngle.floatValue = EditorGUILayout.FloatField("Angular Tolerance", m_spNextPoseToleranceAngle.floatValue);

                    if (MxMSettings.HelpActive)
                    {
                        EditorGUILayout.HelpBox("The maximum allowable trajectory angle facing error tolerance for the next pose to be considered 'good enough' " +
                            "if the total trajectory facing error of the next pose exceeds this, a pose search will be triggered.", MessageType.Info);
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();

                    curHeight += 18f * 2f;
                }

                m_spDIYPlayableGraph.boolValue = EditorGUILayout.Toggle(new GUIContent("DIY Playable Graph"), m_spDIYPlayableGraph.boolValue);

                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("If you want to create your own playable graph and add MxM to it then check this box. This is an advanced feature " +
                        "and requires additional knowledge on Playable Graphs. Please see the User manual for more details.", MessageType.Info);
                }

                if (m_spDIYPlayableGraph.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15f);
                    m_spAutoCreateAnimatorController.boolValue = EditorGUILayout.Toggle(
                        new GUIContent("Auto Create Controller"), m_spAutoCreateAnimatorController.boolValue);
                    EditorGUILayout.EndHorizontal();

                    if (MxMSettings.HelpActive)
                    {
                        EditorGUILayout.HelpBox("If using a DIY playable graph, it may not be desirable for MxM to create an animator controller within its " +
                            "own structure. Check this box to prevent it from doing so.", MessageType.Info);
                    }

                    curHeight += 18f;
                }

                m_spApplyHumanoidFootIK.boolValue = EditorGUILayout.Toggle(new GUIContent("Apply Humanoid Foot IK",
                    "Turns on/off Unity's humanoid foot IK for runtime fixing the feet on a humanoid pose."), m_spApplyHumanoidFootIK.boolValue);

                m_spApplyPlayableIK.boolValue = EditorGUILayout.Toggle(
                    new GUIContent("Apply Playable IK", "Turns on/off playable IK for this clip"),
                    m_spApplyPlayableIK.boolValue);

                m_spMinFootstepInterval.floatValue = EditorGUILayout.FloatField(new GUIContent("Min Footstep Interval",
                        "The minimum time interval between footstep triggers of the same foot."),
                    m_spMinFootstepInterval.floatValue);

                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("Check this box if you would like Unity's built in humanoid foot IK to be active for MxM animations. For " +
                        "most users, this should be checked.", MessageType.Info);
                }

                GUILayout.Space(5f);
                curHeight += 18f * 18f + 5f;
            }

            curHeight += 30f;
            GUILayout.Space(2f);

            m_spWarpingFoldout.boolValue = EditorUtil.EditorFunctions.DrawFoldout("Warping",
                curHeight, EditorGUIUtility.currentViewWidth, m_spWarpingFoldout.boolValue);


            if (m_spWarpingFoldout.boolValue)
            {
                EditorGUILayout.ObjectField(m_spOverrideWarpSettings, typeof(WarpModule), new GUIContent("Override Warp Module"));

                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("Warp settings can be overridden via an external asset if desired. To create that asset, right click in " +
                        "your project view and choose 'Create/MxM/Utility/MxMWarpDataModule.", MessageType.Info);
                }

                if (m_spOverrideWarpSettings.objectReferenceValue == null)
                {

                    m_spAngularWarpType.intValue = (int)(EAngularErrorWarp)EditorGUILayout.EnumPopup(
                        "Angular Error Warping", (EAngularErrorWarp)m_spAngularWarpType.intValue);

                    if (MxMSettings.HelpActive)
                    {
                        EditorGUILayout.HelpBox("Angular Error Warping can be turned On, Off, or set to 'managed external'. Most users will " +
                            "want this to be set to On if using Root motion. It procedurally rotates the character to compensate for errors in the " +
                            "root motion causing the character to run in a slightly different direction than desired.", MessageType.Info);
                    }

                    if (m_spAngularWarpType.intValue > 0)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(15f);
                        m_spAngularWarpMethod.intValue = (int)(EAngularErrorWarpMethod)EditorGUILayout.EnumPopup(
                            "Warp Method", (EAngularErrorWarpMethod)m_spAngularWarpMethod.intValue);
                        EditorGUILayout.EndHorizontal();

                        if (MxMSettings.HelpActive)
                        {
                            EditorGUILayout.HelpBox("This setting defines how angular error warping should be calculated. For normal locomotion where the character " +
                                "faces the direction they are moving use 'Current Heading' or 'Trajectory Heading'. For strafing type locomotion use Trajectory Facing. " +
                                "Note that angular error warping can be disabled for certain animations in the MxMTimeline 'Utility' section.", MessageType.Info);
                        }

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(15f);
                        m_spAngularErrorWarpRate.floatValue = EditorGUILayout.FloatField(new GUIContent("Warp Rate",
                            "How fast the error will be warped for in 'degrees per second'."),
                            m_spAngularErrorWarpRate.floatValue);
                        EditorGUILayout.EndHorizontal();

                        if (MxMSettings.HelpActive)
                        {
                            EditorGUILayout.HelpBox("The rate of procedural rotation to compensate for error (Degrees per Second)", MessageType.Info);
                        }

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(15f);
                        m_spAngularErrorWarpThreshold.floatValue = EditorGUILayout.FloatField(new GUIContent("Magnitude Threshold",
                            "Angular Error Warping will not activate unless the final trajectory point is greater than this value"),
                            m_spAngularErrorWarpThreshold.floatValue);
                        EditorGUILayout.EndHorizontal();

                        if (MxMSettings.HelpActive)
                        {
                            EditorGUILayout.HelpBox("It is not desirable for Angular Error Warping to occur while idle or moving very slowly. This setting " +
                                "stops it from occurring while the future trajectory is below a certain size. 0.5m - 1m is recommended.", MessageType.Info);
                        }

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(15f);

                        Vector2 angularErrorWarpRange = new Vector2(m_spAngularErrorWarpMinAngleThreshold.floatValue, m_spAngularErrorWarpAngleThreshold.floatValue);

                        EditorGUI.BeginChangeCheck();
                        angularErrorWarpRange = EditorGUILayout.Vector2Field(new GUIContent("Angle Range",
                            "The minimum and maximum angle under which angular error warping will be activated (degrees)."),
                            angularErrorWarpRange);
                        if (EditorGUI.EndChangeCheck())
                        {
                            m_spAngularErrorWarpAngleThreshold.floatValue = angularErrorWarpRange.y;
                            m_spAngularErrorWarpMinAngleThreshold.floatValue = angularErrorWarpRange.x;
                        }

                        EditorGUILayout.EndHorizontal();

                        if (MxMSettings.HelpActive)
                        {
                            EditorGUILayout.HelpBox("Unless strafing, it is usually desirable only to apply angular error warping when the error is small as it is " +
                                "likely that the motion matching algorithm will switch to a different animation to achieve larger rotations. Angular error warping " +
                                "will only occur when the error is between these two values (degrees).", MessageType.Info);
                        }

                        curHeight += 18f * 3f;
                    }

                    GUILayout.Space(10f);
                    curHeight += 10f;

                    m_spLongErrorWarpType.intValue = (int)(ELongitudinalErrorWarp)EditorGUILayout.EnumPopup(
                        "Longitudinal Error Warping", (ELongitudinalErrorWarp)m_spLongErrorWarpType.intValue);

                    if (m_spLongErrorWarpType.intValue > 0)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(15f);
                        EditorGUI.BeginChangeCheck();

                        if (m_spLongErrorWarpType.intValue == 1)
                        {

                            m_spSpeedWarpLimits.vector2Value = EditorGUILayout.Vector2Field(
                                "Speed Warp Limits", m_spSpeedWarpLimits.vector2Value);
                            if (EditorGUI.EndChangeCheck())
                            {
                                if (m_spSpeedWarpLimits.vector2Value.x > m_spSpeedWarpLimits.vector2Value.y)
                                {
                                    m_spSpeedWarpLimits.vector2Value = new Vector2(m_spSpeedWarpLimits.vector2Value.y,
                                        m_spSpeedWarpLimits.vector2Value.y);
                                }
                            }
                        }
                        else
                        {
                            GUIStyle redBoldStyle = new GUIStyle(GUI.skin.label);
                            redBoldStyle.fontStyle = FontStyle.Bold;
                            redBoldStyle.normal.textColor = Color.red;

                            EditorGUILayout.LabelField("Check Stride Warper Component Attached", redBoldStyle);
                        }

                        EditorGUILayout.EndHorizontal();
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Warp settings are being overridden by warp module", MessageType.Info);
                }

                GUILayout.Space(5f);
                curHeight += 5f;
            }

            curHeight += 30f;
            GUILayout.Space(2f);
            
            m_spOptimisationFoldout.boolValue = EditorUtil.EditorFunctions.DrawFoldout("Optimisation",
                curHeight, EditorGUIUtility.currentViewWidth, m_spOptimisationFoldout.boolValue);
            
            if (m_spOptimisationFoldout.boolValue)
            {
                m_spPriorityUpdate.boolValue =
                    EditorGUILayout.Toggle(new GUIContent("Priority Update", 
                    "If checked, this MxM animator will be guaranteed to be updated as per it's update " +
                    "interval. and never delayed"), m_spPriorityUpdate.boolValue);
                
                EditorGUI.BeginChangeCheck();
                m_spMaxUpdateDelay.floatValue = EditorGUILayout.FloatField(new GUIContent("Max Update Delay (s)", 
                        "The maximum amount of time that the update manager can delay an udpate for this animator"),
                    m_spMaxUpdateDelay.floatValue);
                if (EditorGUI.EndChangeCheck())
                {
                    if (m_spMaxUpdateDelay.floatValue < 0f)
                        m_spMaxUpdateDelay.floatValue = 0f;
                }
            }
            
            lastRect = GUILayoutUtility.GetLastRect();
            curHeight = lastRect.y + lastRect.height + 5f;
            GUILayout.Space(5f);

            m_spDebugFoldout.boolValue = EditorUtil.EditorFunctions.DrawFoldout("Debug",
                curHeight, EditorGUIUtility.currentViewWidth, m_spDebugFoldout.boolValue);

            if (m_spDebugFoldout.boolValue)
            {
                m_spDebugGoal.boolValue = EditorGUILayout.Toggle(new GUIContent("Debug Goal"), m_spDebugGoal.boolValue);
                m_spDebugChosenTrajectory.boolValue = EditorGUILayout.Toggle(new GUIContent("Debug AnimTrajectory"), m_spDebugChosenTrajectory.boolValue);
                m_spDebugCurrentPose.boolValue = EditorGUILayout.Toggle(new GUIContent("Debug Current Pose"), m_spDebugCurrentPose.boolValue);

                if (!Application.IsPlaying(m_animator))
                {

                    EditorGUI.BeginChangeCheck();
                    m_spDebugPoses.boolValue = EditorGUILayout.Toggle(new GUIContent("Debug Poses In Editor"), m_spDebugPoses.boolValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (m_spDebugPoses.boolValue)
                            m_animator.StartPoseDebug(m_spDebugAnimDataId.intValue);
                        else
                            m_animator.StopPoseDebug();
                    }
                }

                if (m_spDebugPoses.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15f);
                    EditorGUI.BeginChangeCheck();
                    m_spDebugAnimDataId.intValue = EditorGUILayout.IntField(new GUIContent("AnimData Id"), m_spDebugAnimDataId.intValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (m_spDebugAnimDataId.intValue < 0)
                            m_spDebugAnimDataId.intValue = 0;

                        if (m_spDebugAnimDataId.intValue >= m_spAnimData.arraySize - 1)
                            m_spDebugAnimDataId.intValue = m_spAnimData.arraySize - 2;

                        m_animator.StopPoseDebug();
                        m_animator.StartPoseDebug(m_spDebugAnimDataId.intValue);
                    }
                    EditorGUILayout.EndHorizontal();

                    if (m_spDebugAnimDataId.intValue < m_spAnimData.arraySize - 1)
                    {
                        m_animData = m_spAnimData.GetArrayElementAtIndex(m_spDebugAnimDataId.intValue).objectReferenceValue as MxMAnimData;
                    }

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15f);
                    m_spDebugPoseId.intValue = EditorGUILayout.IntField(new GUIContent("Pose Id"), m_spDebugPoseId.intValue);
                    EditorGUILayout.EndHorizontal();

                    if (m_spDebugPoseId.intValue < 0)
                        m_spDebugPoseId.intValue = 0;

                    if (m_spDebugPoseId.intValue >= m_animData.Poses.Length)
                        m_spDebugPoseId.intValue = m_animData.Poses.Length - 1;

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15f);

                    PoseData curPose = m_animData.Poses[m_spDebugPoseId.intValue];

                    EditorGUILayout.LabelField(m_animData.Clips[curPose.PrimaryClipId].name);
                    if (GUILayout.Button("Back"))
                    {
                        m_spDebugPoseId.intValue -= 1;

                        if (m_spDebugPoseId.intValue < 0)
                            m_spDebugPoseId.intValue = 0;
                    }

                    if (GUILayout.Button("Next"))
                    {
                        m_spDebugPoseId.intValue += 1;

                        if (m_spDebugPoseId.intValue >= m_animData.Poses.Length)
                            m_spDebugPoseId.intValue = m_animData.Poses.Length - 1;
                    }
                    EditorGUILayout.EndHorizontal();


                    curHeight += 36f;
                }

                if(GUILayout.Button("OpenDebugWindow"))
                {
                    MxMDebuggerWindow.SetTarget(m_animator);
                    MxMDebuggerWindow.ShowWindow();
                }

                if (Application.IsPlaying(m_animator))
                {

                    string pauseString = "Pause";

                    if (m_animator.IsPaused)
                        pauseString = "UnPause";


                    if (GUILayout.Button(new GUIContent(pauseString)))
                    {
                        m_animator.TogglePause();
                    }
                }

                if (GUILayout.Button(new GUIContent("Strip Pose Masks")))
                {
                    if (EditorUtility.DisplayDialog("Strip Pose Masks", "Are you sure you want to delete all pose masks attached " +
                         "to animData in this Animator? This is irreversible.", "Yes", "No"))
                    {
                        m_animator.ClearPoseMask();
                    }
                }
            }

            lastRect = GUILayoutUtility.GetLastRect();

            curHeight = lastRect.y + lastRect.height + 5f;
            GUILayout.Space(5f);

            m_spCallbackFoldout.boolValue = EditorUtil.EditorFunctions.DrawFoldout("Callbacks",
                curHeight, EditorGUIUtility.currentViewWidth, m_spCallbackFoldout.boolValue);

            if (m_spCallbackFoldout.boolValue)
            {
                EditorGUILayout.PropertyField(m_spOnSetupCompleteCallback);
                EditorGUILayout.PropertyField(m_spOnIdleTriggeredCallback);
                EditorGUILayout.PropertyField(m_spOnIdleEndCallback);
                EditorGUILayout.PropertyField(m_spOnRequiredTagsChangedCallback);
                EditorGUILayout.PropertyField(m_spOnLeftFootStepStartCallback);
                EditorGUILayout.PropertyField(m_spOnRightFootStepStartCallback);
                EditorGUILayout.PropertyField(m_spOnEventCompleteCallback);
                EditorGUILayout.PropertyField(m_spOnEventContactCallback);
                EditorGUILayout.PropertyField(m_spOnEventChangeStateCallback);
                EditorGUILayout.PropertyField(m_spOnPoseChangedCallback);
            }

            if (m_spAnimData.arraySize <= 1 || m_spAnimData.GetArrayElementAtIndex(0).objectReferenceValue == null)
            {
                GUI.enabled = true;
            }

            serializedObject.ApplyModifiedProperties();
        }

    }//End of class: MxMAnimatorInspector
}//End of namespace: MxM