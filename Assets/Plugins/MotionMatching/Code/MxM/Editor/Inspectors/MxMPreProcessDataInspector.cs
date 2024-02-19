// ============================================================================================
// File: MxMPreProcessDataInspector.cs
// 
// Authors:  Kenneth Claassen
// Date:     2017-11-05: Created this file.
// 
//     Contains a part of the 'MxMEditor' namespace for 'Unity Engine 5'.
// 
// Copyright (c) 2019 Kenneth Claassen. All rights reserved.
// ============================================================================================
using UnityEngine;
using UnityEditor;
using MxM;
using System.Collections.Generic;
using System.IO;
using UnityEditorInternal;

namespace MxMEditor
{
//============================================================================================
/**
*  @brief
*         
*********************************************************************************************/ 
    [CustomEditor(typeof(MxMPreProcessData))]
    public class AnimationPreProcessDataInspector : Editor
    {
        private MxMPreProcessData m_data;

        private string m_fileName;

        //Serialized Properties
        private SerializedProperty m_spTargetPrefab;
        private SerializedProperty m_spPoseInterval;
        private SerializedProperty m_spTrajectoryTimes;
        private SerializedProperty m_spPoseProperties;
        private SerializedProperty m_spTags;
        private SerializedProperty m_spFavourTags;
        private SerializedProperty m_spUserTags;
        private SerializedProperty m_spEvents;
        private SerializedProperty m_spUseGenericRig;
        private SerializedProperty m_spLastSaveDirectory;
        private SerializedProperty m_spAnimModules;
        private SerializedProperty m_spCompositeCategories;
        private SerializedProperty m_spAnimIdleSets;
        private SerializedProperty m_spBlendSpaces;
        private SerializedProperty m_spMotionTimingPresets;
        private SerializedProperty m_spEmbedAnimClipsInAnimData;
        private SerializedProperty m_spGenerateModifiedClipsOnPreProcess;
        private SerializedProperty m_spHideSubAssets;
        private SerializedProperty m_spJointVelocityGlobal;

        private SerializedProperty m_spOverrideConfigModule;
        private SerializedProperty m_spOverrideTagModule;
        private SerializedProperty m_spOverrideEventModule;

        private SerializedProperty m_spLastSaveLocation;
        private SerializedProperty m_spLastCreatedAnimData;

        private ReorderableList m_animModuleReorderableList;
        private List<ReorderableList> m_compositeReorderableLists;
        private ReorderableList m_idleSetReorderableList;
        private ReorderableList m_blendSpaceReorderableList;

        private List<string> m_jointNames;
        private List<int> m_poseJointIndexes;

        private SerializedProperty m_spGeneralFoldout;
        private SerializedProperty m_spTrajectoryFoldout;
        private SerializedProperty m_spPoseFoldout;
        private SerializedProperty m_spAnimationFoldout;
        private SerializedProperty m_spAnimModuleFoldout;
        private SerializedProperty m_spCompositeFoldout;
        private SerializedProperty m_spIdleSetFoldout;
        private SerializedProperty m_spBlendSpaceFoldout;
        private SerializedProperty m_spMetaDataFoldout;
        private SerializedProperty m_spTagsFoldout;
        private SerializedProperty m_spFavourTagsFoldout;
        private SerializedProperty m_spUserTagsFoldout;
        private SerializedProperty m_spEventsFoldout;
        private SerializedProperty m_spMotionTimingFoldout;
        private SerializedProperty m_spPreProcessFoldout;


        private int m_currentCompositeCategory;
        private int m_queueDeleteCompositeCategory = -1;
        private int m_queueShiftCompositeCategoryUp = -1;
        private int m_queueShiftCompositeCategoryDown = -1;

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void OnEnable()
        {
            m_data = (MxMPreProcessData)target;

            m_spTargetPrefab = serializedObject.FindProperty("m_targetPrefab");
            m_spPoseInterval = serializedObject.FindProperty("m_poseInterval");
            m_spEmbedAnimClipsInAnimData = serializedObject.FindProperty("m_embedAnimClipsInAnimData");
            m_spTrajectoryTimes = serializedObject.FindProperty("m_trajectoryTimes");
            m_spPoseProperties = serializedObject.FindProperty("m_poseJoints");
            m_spTags = serializedObject.FindProperty("m_tagNames");
            m_spFavourTags = serializedObject.FindProperty("m_favourTagNames");
            m_spUserTags = serializedObject.FindProperty("m_userTagNames");
            m_spEvents = serializedObject.FindProperty("m_eventNames");
            m_spUseGenericRig = serializedObject.FindProperty("m_getBonesByName");
            m_spLastSaveDirectory = serializedObject.FindProperty("m_lastSaveDirectory");
            m_spLastCreatedAnimData = serializedObject.FindProperty("m_lastCreatedAnimData");
            m_spAnimModules = serializedObject.FindProperty("m_animModules");
            m_spCompositeCategories = serializedObject.FindProperty("m_compositeCategories");
            m_spAnimIdleSets = serializedObject.FindProperty("m_animIdleSets");
            m_spBlendSpaces = serializedObject.FindProperty("m_blendSpaces");
            m_spMotionTimingPresets = serializedObject.FindProperty("m_motionTimingPresets");
            m_spGenerateModifiedClipsOnPreProcess = serializedObject.FindProperty("m_generateModifiedClipsOnPreProcess");
            m_spHideSubAssets = serializedObject.FindProperty("m_hideSubAssets");
            m_spJointVelocityGlobal = serializedObject.FindProperty("m_jointVelocityGlobal");

            m_spOverrideConfigModule = serializedObject.FindProperty("m_overrideConfigModule");
            m_spOverrideTagModule = serializedObject.FindProperty("m_overrideTagModule");
            m_spOverrideEventModule = serializedObject.FindProperty("m_overrideEventModule");

            m_spGeneralFoldout = serializedObject.FindProperty("m_generalFoldout");
            m_spTrajectoryFoldout = serializedObject.FindProperty("m_trajectoryFoldout");
            m_spPoseFoldout = serializedObject.FindProperty("m_poseFoldout");
            m_spAnimationFoldout = serializedObject.FindProperty("m_animationFoldout");
            m_spAnimModuleFoldout = serializedObject.FindProperty("m_animModuleFoldout");
            m_spCompositeFoldout = serializedObject.FindProperty("m_compositeFoldout");
            m_spIdleSetFoldout = serializedObject.FindProperty("m_idleSetFoldout");
            m_spBlendSpaceFoldout = serializedObject.FindProperty("m_blendSpaceFoldout");
            m_spMetaDataFoldout = serializedObject.FindProperty("m_metaDataFoldout");
            m_spTagsFoldout = serializedObject.FindProperty("m_tagsFoldout");
            m_spFavourTagsFoldout = serializedObject.FindProperty("m_favourTagsFoldout");
            m_spUserTagsFoldout = serializedObject.FindProperty("m_userTagsFoldout");
            m_spEventsFoldout = serializedObject.FindProperty("m_eventsFoldout");
            m_spMotionTimingFoldout = serializedObject.FindProperty("m_motionTimingFoldout");
            m_spPreProcessFoldout = serializedObject.FindProperty("m_preProcessFoldout");

            InitializeGenericRigData();
            SetupReorderableLists();

            UpdateTargetModelForAnimData();

            m_data.ValidateData();

            foreach (MxMAnimationIdleSet idleSet in m_data.AnimationIdleSets)
            {
                idleSet.TargetPreProcess = m_data;
                EditorUtility.SetDirty(idleSet);
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void InitializeGenericRigData()
        {
            if (m_jointNames == null)
                m_jointNames = new List<string>();

            if (m_poseJointIndexes == null)
                m_poseJointIndexes = new List<int>();

            m_jointNames.Clear();
            m_poseJointIndexes.Clear();

            var prefab = m_spTargetPrefab.objectReferenceValue as GameObject;

            if (prefab != null)
            {
                List<string> tempJointNames = null;
                if (m_spUseGenericRig.boolValue)
                {
                    tempJointNames = GetAllChildPaths(prefab.transform, "");

                    for (int i = 0; i < tempJointNames.Count; ++i)
                    {
                        m_jointNames.Add(tempJointNames[i].Remove(0, 1));
                    }
                }
            }

            for (int i = 0; i < m_spPoseProperties.arraySize; ++i)
            {
                SerializedProperty spProperty = m_spPoseProperties.GetArrayElementAtIndex(i);
                SerializedProperty spBoneName = spProperty.FindPropertyRelative("BoneName");

                m_poseJointIndexes.Add(0);

                for (int k = 0; k < m_jointNames.Count; ++k)
                {
                    if (m_jointNames[k] == spBoneName.stringValue)
                    {
                        m_poseJointIndexes[i] = k;
                        break;
                    }
                }
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public override void OnInspectorGUI()
        {
            MxMSettings MxMSettings = MxMSettings.Instance();

            ManageQueuedActions();

            MxMSettings.HelpActive = EditorGUILayout.ToggleLeft(new GUIContent("Help", "Displays help info and links for the pre-process properties"), MxMSettings.HelpActive);
            Rect lastRect = GUILayoutUtility.GetLastRect();

            float curHeight = lastRect.y + 9f;

            curHeight = EditorUtil.EditorFunctions.DrawTitle("MxM Pre-Processor", curHeight);

            if(MxMSettings.HelpActive)
            {
                EditorGUILayout.HelpBox("The PreProcessor is used to create animation data that can be used at runtime for 'Motion Matching for Unity'. " +
                    "This is where you set baked parameters, add your animations and configure them. Once ready, the pre-processor will " +
                    "process all the data into a MxMAnimData asset for runtime use in the MxMAnimator component", MessageType.Info);

                lastRect = GUILayoutUtility.GetLastRect();
                curHeight = lastRect.y + 9f;
                GUILayout.Space(9f);
            }

            m_spGeneralFoldout.boolValue = EditorUtil.EditorFunctions.DrawFoldout(
                "General", curHeight, EditorGUIUtility.currentViewWidth, m_spGeneralFoldout.boolValue);

            lastRect  = GUILayoutUtility.GetLastRect();
            curHeight = lastRect.y + lastRect.height;

            if (m_spGeneralFoldout.boolValue)
            {
                if (m_spOverrideConfigModule.objectReferenceValue == null)
                {

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.ObjectField(m_spTargetPrefab, new GUIContent("Target Model", "This is the model that the pre-processing is based on."));
                    if (EditorGUI.EndChangeCheck())
                    {
                        UpdateTargetModelForAnimData();
                        if (IsTargetPrefabHuman())
                        {
                            m_spUseGenericRig.boolValue = false;
                        }
                        else
                        {
                            m_spUseGenericRig.boolValue = true;
                            InitializeGenericRigData();
                        }
                    }

                    if (MxMSettings.HelpActive)
                    {
                        EditorGUILayout.HelpBox("Target Model - This is the target model that will be used to pre-process and analyse your animations on. Preferably it should be the model you " +
                            "plan to use in the game or something of relatively similar proportions.", MessageType.Info);
                    }
                }
                else
                {
                    EditorGUI.BeginDisabledGroup(true);
                    var configModule =  m_spOverrideConfigModule.objectReferenceValue as MotionMatchConfigModule;
                    EditorGUILayout.ObjectField(new GUIContent("Target Model", "The target model is being used from the config override"), configModule.Prefab, typeof(GameObject), false);
                    EditorGUI.EndDisabledGroup();

                    if (MxMSettings.HelpActive)
                    {
                        EditorGUILayout.HelpBox("Target Model - This is the target model that will be used to pre-process and analyse your animations on. It is currently being overriden by " +
                            "the target model set in the Configuration Module referenced in the 'Config Override' parameter below.", MessageType.Info);
                    }
                }

                EditorGUI.BeginChangeCheck();
                m_spPoseInterval.floatValue = EditorGUILayout.FloatField(new GUIContent("Pose Interval", "The time interval between each recorded pose"), m_spPoseInterval.floatValue);
                if(EditorGUI.EndChangeCheck())
                {
                    m_spPoseInterval.floatValue = Mathf.Clamp(m_spPoseInterval.floatValue, 0.01f, 0.2f);
                }

                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("Pose Interval - This is the time interval (in seconds) between poses recorded in each animation. Lower values mean more poses which " +
                        "does tend to improve quality but also reduce performance. Values between 0.05s - 0.1s is recommended.", MessageType.Info);
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.ObjectField(m_spOverrideConfigModule, new GUIContent("Config Override (Optional)", "This is an optional field to " +
                    "override all config data in this pre-processor from a separate config module asset"));
                if(EditorGUI.EndChangeCheck())
                {
                    if (m_spOverrideConfigModule.objectReferenceValue != null)
                    {
                        m_spTrajectoryFoldout.boolValue = false;
                        m_spPoseFoldout.boolValue = false;
                    }
                    else
                    {
                        m_spTrajectoryFoldout.boolValue = true;
                        m_spPoseFoldout.boolValue = true;
                    }

                    UpdateTargetModelForAnimData();
                }

                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("Config Override - The pre-processor configuration can be overriden by an external 'Configuration Module' asset. This" +
                        "allows for multiple pre-processors to re-use the same configuration without repeating the data in each one. Use of modules like this is highly " +
                        "recommended for more flexible and robust workflow", MessageType.Info);
                }

                GUILayout.Space(3f);
                curHeight += 3f;
            }

            if (m_spOverrideConfigModule.objectReferenceValue != null)
            {
                EditorGUI.BeginDisabledGroup(true);
                m_spTrajectoryFoldout.boolValue = EditorUtil.EditorFunctions.DrawFoldout("Trajectory Configuration (Override)", curHeight, EditorGUIUtility.currentViewWidth, false);

                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("The trajectory configuration is currently being overriden by the 'Config Override' module set in the 'General' foldout. It can only be" +
                        "modified in the module (recommended), unless the module is removed (not recommended)", MessageType.Info);

                    GUILayout.Space(9f);
                }
            }
            else
            {
                m_spTrajectoryFoldout.boolValue = EditorUtil.EditorFunctions.DrawFoldout(
                    "Trajectory Configuration", curHeight, EditorGUIUtility.currentViewWidth, m_spTrajectoryFoldout.boolValue);
            }

            lastRect = GUILayoutUtility.GetLastRect();
            curHeight = lastRect.y + lastRect.height;

            GUIStyle italicLabel = new GUIStyle(GUI.skin.label);
            italicLabel.fontStyle = FontStyle.Italic;

            if (m_spTrajectoryFoldout.boolValue)
            {
                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("This section is for defining the trajectory that MxM should use for matching user input. Typically you will need " +
                        "4 - 5 trajectory points with at least 1 in the past. Usually trajectories should be within -1s and 1s. Negative values represent " +
                        "trajectory points in the past, while positive values represent points in the future. Trajectory points must be ordered chronologically " +
                        "from past to future.", MessageType.Info);
                }


                float lastTrajTime = -1000f;

                for (int i = 0; i < m_spTrajectoryTimes.arraySize; ++i)
                {
                    SerializedProperty trajTime = m_spTrajectoryTimes.GetArrayElementAtIndex(i);

                    EditorGUILayout.BeginHorizontal();
                    trajTime.floatValue = EditorGUILayout.FloatField(new GUIContent("Trajectory " + (1 + i).ToString() + " (sec)"), trajTime.floatValue);

                    GUILayout.Space(20f);

                    lastRect = GUILayoutUtility.GetLastRect();

                    lastRect.x += 2f;
                    lastRect.y += 1f;
                    lastRect.height = 16f;
                    lastRect.width = 16f;

                    if (m_spTrajectoryTimes.arraySize > 1)
                    {
                        if (GUI.Button(lastRect, new GUIContent("-")))
                        {
                            m_spTrajectoryTimes.DeleteArrayElementAtIndex(i);
                            --i;
                            continue;
                        }
                    }

                    EditorGUILayout.EndHorizontal();

                    if (trajTime.floatValue <= lastTrajTime)
                        trajTime.floatValue = lastTrajTime + 0.1f;
                    else if (trajTime.floatValue == 0f)
                        trajTime.floatValue = 0.1f;

                    lastTrajTime = trajTime.floatValue;

                    curHeight += 18f;
                }

                lastRect = GUILayoutUtility.GetLastRect();
                curHeight = lastRect.y + lastRect.height + 4f;

                if (m_spTrajectoryTimes.arraySize < 12)
                {
                    if (GUI.Button(new Rect(EditorGUIUtility.currentViewWidth - 175f, curHeight, 150f, 18f), new GUIContent("Add Trajectory Point")))
                    {
                        m_spTrajectoryTimes.InsertArrayElementAtIndex(m_spTrajectoryTimes.arraySize);

                        if (m_spTrajectoryTimes.arraySize == 1)
                        {
                            m_spTrajectoryTimes.GetArrayElementAtIndex(0).floatValue = -0.2f;
                        }
                        else
                        {
                            m_spTrajectoryTimes.GetArrayElementAtIndex(m_spTrajectoryTimes.arraySize - 1).floatValue
                                = m_spTrajectoryTimes.GetArrayElementAtIndex(m_spTrajectoryTimes.arraySize - 2).floatValue + 0.1f;
                        }
                    }
                }

                curHeight += 10f;
                GUILayout.Space(29f);
            }

            curHeight += 27f;

            if (m_spOverrideConfigModule.objectReferenceValue != null)
            {
                m_spPoseFoldout.boolValue = EditorUtil.EditorFunctions.DrawFoldout("Pose Configuration (Override)", curHeight, EditorGUIUtility.currentViewWidth, false);

                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("The pose configuration is currently being overridden by the 'Config Override' module set in the 'General' foldout. It can only be" +
                        "modified in the module (recommended), unless the module is removed (not recommended)", MessageType.Info);

                    GUILayout.Space(9f);
                }
            }
            else
            {
                m_spPoseFoldout.boolValue = EditorUtil.EditorFunctions.DrawFoldout(
                    "Pose Configuration", curHeight, EditorGUIUtility.currentViewWidth, m_spPoseFoldout.boolValue);
            }

            lastRect = GUILayoutUtility.GetLastRect();
            curHeight = lastRect.y + lastRect.height;

            if (m_spPoseFoldout.boolValue)
            {
                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("The 'Joint Velocity Method' defines how the velocity of joints should be calculated for each pose. Whether body velocity should" +
                        "be included in the calculation or not. This will impact calibration but results tend to be similar for both methods. The default 'Body Velocity " +
                        "Dependent' mode is recommended in most cases.", MessageType.Info);
                }

                m_spJointVelocityGlobal.intValue = (int)(EJointVelocityCalculationMethod)EditorGUILayout.EnumPopup(
                    "Joint Velocity Method (Advanced)", (EJointVelocityCalculationMethod)m_spJointVelocityGlobal.intValue);

                

                curHeight += 23f;
                GUILayout.Space(5f);

                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("This section is for defining the pose or rather set of joints that MxM should use for pose matching. It may be " +
                        "tempting to pick many joints to match but usually 3 is sufficient. Left foot, right foot and a central stabilizing bone like the neck, " +
                        "or hips. The hands can also be added for additional stability if required. The number of joints has a direct impact on performance so try " +
                        "to pick as few bones as possible to get the results that you desire.", MessageType.Info);
                }

                if (m_spOverrideConfigModule.objectReferenceValue == null)
                {

                    GUIStyle boldText = new GUIStyle(GUI.skin.label);
                    boldText.fontStyle = FontStyle.Bold;

                    curHeight += 20f;
                    GUILayout.Space(2f);

                    for (int i = 0; i < m_spPoseProperties.arraySize; ++i)
                    {
                        SerializedProperty propertySP = m_spPoseProperties.GetArrayElementAtIndex(i);
                        SerializedProperty boneIdSP = propertySP.FindPropertyRelative("BoneId");
                        SerializedProperty boneNameSP = propertySP.FindPropertyRelative("BoneName");

                        EditorGUILayout.LabelField("Pose Property " + (i + 1).ToString(), boldText);
                        GUILayout.Space(2f);

                        lastRect = GUILayoutUtility.GetLastRect();
                        curHeight = lastRect.y + lastRect.height;

                        GUI.Box(new Rect(10f, curHeight - 2f, EditorGUIUtility.currentViewWidth - 33f, 24f), "");

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(20f);
                        lastRect = GUILayoutUtility.GetLastRect();

                        if (m_spPoseProperties.arraySize > 1)
                        {
                            if (GUI.Button(new Rect(lastRect.x, lastRect.y + 2, 16f, 16f), "-"))
                            {
                                m_poseJointIndexes.RemoveAt(i);
                                m_spPoseProperties.DeleteArrayElementAtIndex(i);
                                --i;
                                continue;
                            }
                        }



                        if (m_spUseGenericRig.boolValue)
                        {
                            m_poseJointIndexes[i] = EditorGUILayout.Popup(new GUIContent("Joint Name",
                                "The name of the joint to include in the matching process"),
                                m_poseJointIndexes[i], m_jointNames.ToArray());

                            int index = m_poseJointIndexes[i];
                            if (m_jointNames.Count > index)
                                boneNameSP.stringValue = m_jointNames[m_poseJointIndexes[i]];
                            else
                                boneNameSP.stringValue = "";
                        }
                        else
                        {
                            EditorGUILayout.PropertyField(boneIdSP, new GUIContent("Joint Id",
                                "The joint to include in the matching process"));
                        }

                        GUILayout.Space(20f);
                        EditorGUILayout.EndHorizontal();

                        curHeight += 22f;
                        GUILayout.Space(4f);
                    }

                    GUILayout.Space(5f);

                    lastRect = GUILayoutUtility.GetLastRect();
                    curHeight = lastRect.y + lastRect.height;

                    if (m_poseJointIndexes == null)
                        m_poseJointIndexes = new List<int>();

                    if (m_spPoseProperties.arraySize < 8)
                    {
                        if (GUI.Button(new Rect(EditorGUIUtility.currentViewWidth - 175f, curHeight, 150f, 18f),
                            new GUIContent("Add Pose Property")))
                        {
                            m_spPoseProperties.InsertArrayElementAtIndex(m_spPoseProperties.arraySize);
                            SerializedProperty propertySP = m_spPoseProperties.GetArrayElementAtIndex(m_spPoseProperties.arraySize - 1);
                            SerializedProperty boneIdSP = propertySP.FindPropertyRelative("BoneId");

                            m_poseJointIndexes.Add(0);

                            boneIdSP.enumValueIndex = (int)HumanBodyBones.Head;
                        }
                    }

                    curHeight += 10f;
                    GUILayout.Space(27f);
                }
                else
                {
                    EditorGUILayout.LabelField(new GUIContent("Defined by override configuration module"), italicLabel);
                    GUILayout.Space(19f);
                }
            }

            if (m_spOverrideConfigModule.objectReferenceValue != null)
            {
                EditorGUI.EndDisabledGroup();
            }

            curHeight += 27f;

            m_spAnimationFoldout.boolValue = EditorUtil.EditorFunctions.DrawFoldout(
                "Animation Data", curHeight, EditorGUIUtility.currentViewWidth, m_spAnimationFoldout.boolValue);

            lastRect = GUILayoutUtility.GetLastRect();
            curHeight = lastRect.y + lastRect.height;

            if (m_spAnimationFoldout.boolValue)
            {
                EditorGUI.BeginChangeCheck();
                m_spHideSubAssets.boolValue = EditorGUILayout.Toggle("Hide Sub Assets", m_spHideSubAssets.boolValue);
                if (EditorGUI.EndChangeCheck())
                {
                    HideFlags hideFlag = HideFlags.None;

                    if (m_spHideSubAssets.boolValue)
                    {
                        hideFlag = HideFlags.HideInHierarchy;
                    }

                    foreach (CompositeCategory compCategory in m_data.CompositeCategories)
                    {
                        foreach (MxMAnimationClipComposite composite in compCategory.Composites)
                        {
                            if (composite != null)
                            {
                                composite.hideFlags = hideFlag;
                                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(composite));
                            }
                        }
                    }

                    foreach (MxMAnimationIdleSet idleSet in m_data.AnimationIdleSets)
                    {
                        idleSet.hideFlags = hideFlag;
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(idleSet));
                    }

                    foreach (MxMBlendSpace blendSpace in m_data.BlendSpaces)
                    {
                        blendSpace.hideFlags = hideFlag;
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(blendSpace));
                    }

                    if(m_data.MotionTimingPresets != null)
                    {
                        m_data.MotionTimingPresets.hideFlags = hideFlag;
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(m_data.MotionTimingPresets));
                    }
                }

                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("Hide Sub Assets - Sub assets created for each composite, idle set or blend space are hidden in the project hierarchy for performance and " +
                        "project tidiness. Only un-check this if you desire to make a copy of these sub-assets to put in another pre-processor.\n\n (NOTE: This is only recommended for " +
                        "advanced users. Most users should keep this toggle checked)", MessageType.Info);

                    lastRect = GUILayoutUtility.GetLastRect();
                    curHeight = lastRect.y + lastRect.height;
                }


                GUILayout.Space(5f);
                curHeight += 5f;

                m_spAnimModuleFoldout.boolValue = EditorUtil.EditorFunctions.DrawFoldout(
                    "Modules", curHeight, EditorGUIUtility.currentViewWidth, m_spAnimModuleFoldout.boolValue, 1, true);

                lastRect = GUILayoutUtility.GetLastRect();
                curHeight = lastRect.y + lastRect.height;

                Rect dropRect;
                if(m_spAnimModuleFoldout.boolValue)
                {
                    if (MxMSettings.HelpActive)
                    {
                        EditorGUILayout.HelpBox("Instead of adding animation directly to the pre-processor, they can be added to external module assets to help modularise work flows " +
                            "and enable reuse without having to re-configure them each time. It is recommended to use Animation Modules " +
                            "instead of adding the animations directly to the pre-processor.", MessageType.Info);

                        lastRect = GUILayoutUtility.GetLastRect();
                        curHeight = lastRect.y + lastRect.height;
                    }

                    curHeight += 2f;

                    m_animModuleReorderableList.DoLayoutList();

                    GUILayout.Space(4f);

                    lastRect = GUILayoutUtility.GetLastRect();
                    curHeight = lastRect.y + lastRect.height;

#if UNITY_2019_3_OR_NEWER
                    dropRect = new Rect(18f, curHeight - 20f, EditorGUIUtility.currentViewWidth - 107f, 20f);
#else
                    dropRect = new Rect(15f, curHeight - 18f, EditorGUIUtility.currentViewWidth - 95f, 20f);
#endif
                    GUI.Box(dropRect, new GUIContent("Drag Animation Module Here"));
                    DragDropAnimationModules(dropRect);

                    GUILayout.Space(18f);

                    lastRect = GUILayoutUtility.GetLastRect();
                    curHeight = lastRect.y + lastRect.height;

                    curHeight += 4f;
                    GUILayout.Space(4f);
                }

                m_spCompositeFoldout.boolValue = EditorUtil.EditorFunctions.DrawFoldout(
                    "Composites", curHeight, EditorGUIUtility.currentViewWidth, m_spCompositeFoldout.boolValue, 1, true);

                lastRect = GUILayoutUtility.GetLastRect();
                curHeight = lastRect.y + lastRect.height;

                if (m_spCompositeFoldout.boolValue)
                {
                    if (MxMSettings.HelpActive)
                    {
                        EditorGUILayout.HelpBox("Composites are your normal animations and mocap takes that you want to use with motion matching. It is not " +
                            "recommended to add them directly to the pre-processor here but rather in a separate 'AnimModule'.", MessageType.Info);
                    }

                    for (int i = 0; i < m_spCompositeCategories.arraySize; ++i)
                    {
                        m_currentCompositeCategory = i;

                        m_compositeReorderableLists[i].DoLayoutList();

                        GUILayout.Space(4f);

                        lastRect = GUILayoutUtility.GetLastRect();
                        curHeight = lastRect.y + lastRect.height;

                        //Draw Animation Composites

#if UNITY_2019_3_OR_NEWER
                        dropRect = new Rect(18f, curHeight - 20f, EditorGUIUtility.currentViewWidth - 107f, 20f);
#else
                        dropRect = new Rect(15f, curHeight - 18f, EditorGUIUtility.currentViewWidth - 95f, 20f);
#endif
                        GUI.Box(dropRect, new GUIContent("Drag Anim or Composite Here"));
                        DragDropComposites(dropRect, i);

                        GUILayout.Space(10f);
                        lastRect = GUILayoutUtility.GetLastRect();
                        curHeight = lastRect.y + lastRect.height;
                    }

                    lastRect = GUILayoutUtility.GetLastRect();
                    curHeight = lastRect.y + lastRect.height;

                    if (GUI.Button(new Rect(EditorGUIUtility.currentViewWidth 
                        - 175f, curHeight, 150f, 18f), "Add New Category"))
                    {
                        m_spCompositeCategories.InsertArrayElementAtIndex(m_spCompositeCategories.arraySize);
                        SerializedProperty spCategory = m_spCompositeCategories.GetArrayElementAtIndex(m_spCompositeCategories.arraySize - 1);

                        SerializedProperty spCategoryName = spCategory.FindPropertyRelative("CatagoryName");
                        SerializedProperty spCompositeList = spCategory.FindPropertyRelative("Composites");
                        SerializedProperty spIgnoreEdges = spCategory.FindPropertyRelative("IgnoreEdges_default");
                        SerializedProperty spExtrapolate = spCategory.FindPropertyRelative("Extrapolate_default");
                        SerializedProperty spFlattenTrajectory = spCategory.FindPropertyRelative("FlattenTrajectory_default");
                        SerializedProperty spRuntimeSplicing = spCategory.FindPropertyRelative("RuntimeSplicing_default");
                        SerializedProperty spRequireTags_default = spCategory.FindPropertyRelative("RequireTags_default");
                        SerializedProperty spFavourTags_default = spCategory.FindPropertyRelative("FavourTags_default");


                        spCategoryName.stringValue = "New Category";
                        spCompositeList.ClearArray();
                        spIgnoreEdges.boolValue = false;
                        spExtrapolate.boolValue = true;
                        spFlattenTrajectory.boolValue = true;
                        spRuntimeSplicing.boolValue = false;
                        spRequireTags_default.intValue = 0;
                        spFavourTags_default.intValue = 0;

                        SetupReorderableLists();
                    }

                    GUILayout.Space(25f);
                }

                m_spIdleSetFoldout.boolValue = EditorUtil.EditorFunctions.DrawFoldout(
                    "Idle Sets", curHeight, EditorGUIUtility.currentViewWidth, m_spIdleSetFoldout.boolValue, 1, true);

                lastRect = GUILayoutUtility.GetLastRect();
                curHeight = lastRect.y + lastRect.height;

                if (m_spIdleSetFoldout.boolValue)
                {
                    if (MxMSettings.HelpActive)
                    {
                        EditorGUILayout.HelpBox("This is where you add your idle type animations to utilize MxM's dedicated idle system. It is not recommended " +
                            "to add idle sets directly here in the pre-processor. Add them to a standalone Animation Module instead", MessageType.Info);
                    }

                    curHeight += 2f;

                    m_idleSetReorderableList.DoLayoutList();

                    GUILayout.Space(4f);

                    lastRect = GUILayoutUtility.GetLastRect();
                    curHeight = lastRect.y + lastRect.height;

#if UNITY_2019_3_OR_NEWER
                        dropRect = new Rect(18f, curHeight - 20f, EditorGUIUtility.currentViewWidth - 107f, 20f);
#else
                    dropRect = new Rect(15f, curHeight - 18f, EditorGUIUtility.currentViewWidth - 95f, 20f);
#endif
                    GUI.Box(dropRect, new GUIContent("Drag Idle Anim or IdleSet Here"));
                    DragDropIdles(dropRect);

                    GUILayout.Space(18f);

                    lastRect = GUILayoutUtility.GetLastRect();
                    curHeight = lastRect.y + lastRect.height;

                    curHeight += 4f;
                    GUILayout.Space(4f);
                }

                m_spBlendSpaceFoldout.boolValue = EditorUtil.EditorFunctions.DrawFoldout(
                    "Blend Spaces", curHeight, EditorGUIUtility.currentViewWidth, m_spBlendSpaceFoldout.boolValue, 1, true);

                lastRect = GUILayoutUtility.GetLastRect();
                curHeight = lastRect.y + lastRect.height;

                if (m_spBlendSpaceFoldout.boolValue)
                {
                    if (MxMSettings.HelpActive)
                    {
                        EditorGUILayout.HelpBox("This is where you add Blend Spaces for motion matching. Blend spaces are a relatively advanced topic so please consult " +
                            "the 'User Manual' and 'Tutorial Videos' for more information. It is not recommended " +
                            "to add blend spaces directly here in the pre-processor. Add them to a standalone Animation Module instead", MessageType.Info);
                    }

                    curHeight += 2f;

                    m_blendSpaceReorderableList.DoLayoutList();

                    GUILayout.Space(4f);

                    lastRect = GUILayoutUtility.GetLastRect();
                    curHeight = lastRect.y + lastRect.height;

#if UNITY_2019_3_OR_NEWER
                    dropRect = new Rect(18f, curHeight - 20f, EditorGUIUtility.currentViewWidth - 107f, 20f);
#else
                    dropRect = new Rect(15f, curHeight - 18f, EditorGUIUtility.currentViewWidth - 95f, 20f);
#endif
                    GUI.Box(dropRect, new GUIContent("Drag Anim or BlendSpace here"));
                    DragDropBlendSpaces(dropRect);

                    GUILayout.Space(18f);

                    lastRect = GUILayoutUtility.GetLastRect();
                    curHeight = lastRect.y + lastRect.height;

                    //curHeight += 13f;
                    //GUILayout.Space(4f);
                }

                GUILayout.Space(2f);

                if (GUI.Button(new Rect(EditorGUIUtility.currentViewWidth
                        - 175f, curHeight, 150f, 18f), "Export To Module"))
                {
                    ExportAnimDataToModule();
                }

                curHeight += 20f;
                GUILayout.Space(20f);

                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("Export To Module - If you have animations placed directly in the Pre-Processor (i.e. composites, idle sets and blend spaces) you " +
                        "can export them to an animation module and slot that into the 'Animation Modules' foldout for more flexible use.", MessageType.Info);

                    lastRect = GUILayoutUtility.GetLastRect();
                    curHeight = lastRect.y + lastRect.height +4f;
                    GUILayout.Space(4f);
                }
            }

            if (m_spCompositeFoldout.boolValue && m_spAnimationFoldout.boolValue)
                curHeight += 1f;

            m_spMetaDataFoldout.boolValue = EditorUtil.EditorFunctions.DrawFoldout(
                "Tags and Events", curHeight, EditorGUIUtility.currentViewWidth, m_spMetaDataFoldout.boolValue);

            lastRect = GUILayoutUtility.GetLastRect();
            curHeight = lastRect.y + lastRect.height;

            if (m_spMetaDataFoldout.boolValue)
            {
                curHeight += 28f;

                EditorGUILayout.ObjectField(m_spOverrideTagModule, typeof(TagNamingModule), new GUIContent("Override Tag Module"));
                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("Override Tag Module - Set this to override the tags defined in the pre-processor with the tags defined in an " +
                        "external 'Tag Naming Module' asset.", MessageType.Info);

                    lastRect = GUILayoutUtility.GetLastRect();
                    curHeight = lastRect.y + lastRect.height + 4f;
                }

                EditorGUILayout.ObjectField(m_spOverrideEventModule, typeof(EventNamingModule), new GUIContent("Override Event Module"));
                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("Override Event Module - Set this to override the event ids defined in the pre-processor with the event ids " +
                        "defined in an external 'Event Naming Module' asset.", MessageType.Info);

                    lastRect = GUILayoutUtility.GetLastRect();
                    curHeight = lastRect.y + lastRect.height + 4f;
                }

                curHeight += 5f;
                GUILayout.Space(5f);

                if (m_spOverrideTagModule.objectReferenceValue != null)
                {
                    EditorGUI.BeginDisabledGroup(true);

                    m_spTagsFoldout.boolValue = EditorUtil.EditorFunctions.DrawFoldout(
                        "Require Tags (Override)", curHeight, EditorGUIUtility.currentViewWidth, false, 1, true);

                    if (MxMSettings.HelpActive)
                    {
                        EditorGUILayout.HelpBox("The Required Tags configuration is currently being overriden by an external 'Tag Naming Module' asset. Tags can be edited within " +
                            "the module or by removing the referenced tagging module.", MessageType.Info);

                        GUILayout.Space(4f);
                        lastRect = GUILayoutUtility.GetLastRect();
                        curHeight = lastRect.y + lastRect.height + 4f;
                    }
                }
                else
                {
                    m_spTagsFoldout.boolValue = EditorUtil.EditorFunctions.DrawFoldout(
                        "Require Tags", curHeight, EditorGUIUtility.currentViewWidth, m_spTagsFoldout.boolValue, 1, true);
                }

                lastRect = GUILayoutUtility.GetLastRect();
                curHeight = lastRect.y + lastRect.height;

                if (m_spTagsFoldout.boolValue)
                {
                    SerializedProperty spTag;

                    for (int i=0; i < m_spTags.arraySize; ++i)
                    {
                        spTag = m_spTags.GetArrayElementAtIndex(i);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(23f);

                        lastRect = GUILayoutUtility.GetLastRect();
                        lastRect.height = 17f;
                        lastRect.width = 17f;

                        if (i < 30)
                        {
                            spTag.stringValue = EditorGUILayout.TextField(new GUIContent("Tag " + (i + 1).ToString() + ": "), spTag.stringValue); 
                        }
                        else
                        {
                            EditorGUILayout.LabelField(new GUIContent("Tag " + (i + 1).ToString() + ": " + spTag.stringValue));
                        }
                        EditorGUILayout.EndHorizontal();

                        curHeight += 18f;
                    }

                   
                    curHeight += 5f;
                    GUILayout.Space(4f);
                }

                curHeight += 23f;

                if (m_spOverrideTagModule.objectReferenceValue != null)
                {
                    m_spFavourTagsFoldout.boolValue = EditorUtil.EditorFunctions.DrawFoldout(
                        "Favour Tags (Override)", curHeight, EditorGUIUtility.currentViewWidth, false, 1, true);

                    if (MxMSettings.HelpActive)
                    {
                        EditorGUILayout.HelpBox("The Favour Tags configuration is currently being overriden by an external 'Tag Naming Module' asset. Tags can be edited within " +
                            "the module or by removing the referenced tagging module.", MessageType.Info);

                        GUILayout.Space(4f);
                        lastRect = GUILayoutUtility.GetLastRect();
                        curHeight = lastRect.y + lastRect.height + 4f;
                    }
                }
                else
                {
                    m_spFavourTagsFoldout.boolValue = EditorUtil.EditorFunctions.DrawFoldout(
                        "Favour Tags", curHeight, EditorGUIUtility.currentViewWidth, m_spFavourTagsFoldout.boolValue, 1, true);
                }

                lastRect = GUILayoutUtility.GetLastRect();
                curHeight = lastRect.y + lastRect.height;

                if(m_spFavourTagsFoldout.boolValue)
                {
                    SerializedProperty spFavourTag;

                    for (int i = 0; i < m_spFavourTags.arraySize; ++i)
                    {
                        spFavourTag = m_spFavourTags.GetArrayElementAtIndex(i);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(23f);

                        lastRect = GUILayoutUtility.GetLastRect();
                        lastRect.height = 17f;
                        lastRect.width = 17f;

                        spFavourTag.stringValue = EditorGUILayout.TextField(new GUIContent("Favour Tag " + 
                            (i + 1).ToString() + ": "), spFavourTag.stringValue);

                        EditorGUILayout.EndHorizontal();

                        curHeight += 18f;
                    }


                    curHeight += 5f;
                    GUILayout.Space(4f);
                }

                curHeight += 23f;

                if (m_spOverrideTagModule.objectReferenceValue != null)
                {
                    m_spUserTagsFoldout.boolValue = EditorUtil.EditorFunctions.DrawFoldout(
                        "User Tags (Override)", curHeight, EditorGUIUtility.currentViewWidth, false, 1, true);

                    if (MxMSettings.HelpActive)
                    {
                        EditorGUILayout.HelpBox("The Required Tags configuration is currently being overriden by an external 'Tag Naming Module' asset. Tags can be edited within " +
                            "the module or by removing the referenced tagging module.", MessageType.Info);

                        GUILayout.Space(4f);
                        lastRect = GUILayoutUtility.GetLastRect();
                        curHeight = lastRect.y + lastRect.height + 4f;
                    }
                }
                else
                {
                    m_spUserTagsFoldout.boolValue = EditorUtil.EditorFunctions.DrawFoldout(
                        "User Tags", curHeight, EditorGUIUtility.currentViewWidth, m_spUserTagsFoldout.boolValue, 1, true);
                }

                lastRect = GUILayoutUtility.GetLastRect();
                curHeight = lastRect.y + lastRect.height;

                if (m_spUserTagsFoldout.boolValue)
                {
                    SerializedProperty spUserTag;

                    for (int i = 0; i < m_spUserTags.arraySize; ++i)
                    {
                        spUserTag = m_spUserTags.GetArrayElementAtIndex(i);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(23f);

                        lastRect = GUILayoutUtility.GetLastRect();
                        lastRect.height = 17f;
                        lastRect.width = 17f;

                        spUserTag.stringValue = EditorGUILayout.TextField(new GUIContent("User Tag " +
                            (i + 1).ToString() + ": "), spUserTag.stringValue);

                        EditorGUILayout.EndHorizontal();

                        curHeight += 18f;
                    }


                    curHeight += 5f;
                    GUILayout.Space(4f);
                }

                if (m_spOverrideTagModule.objectReferenceValue != null)
                {
                    EditorGUI.EndDisabledGroup();
                }

                    curHeight += 23f;

                if (m_spOverrideEventModule.objectReferenceValue != null)
                {
                    EditorGUI.BeginDisabledGroup(true);

                    m_spEventsFoldout.boolValue = EditorUtil.EditorFunctions.DrawFoldout(
                        "Events Ids (Override)", curHeight, EditorGUIUtility.currentViewWidth, false, 1, true);

                    if (MxMSettings.HelpActive)
                    {
                        EditorGUILayout.HelpBox("The Event Id configuration is currently being overriden by an external 'Event Naming Module' asset. EventIds can be edited within " +
                            "the module or by removing the referenced event module.", MessageType.Info);

                        GUILayout.Space(4f);
                        lastRect = GUILayoutUtility.GetLastRect();
                        curHeight = lastRect.y + lastRect.height + 4f;
                    }
                }
                else
                {
                    m_spEventsFoldout.boolValue = EditorUtil.EditorFunctions.DrawFoldout(
                        "Events Ids", curHeight, EditorGUIUtility.currentViewWidth, m_spEventsFoldout.boolValue, 1, true);
                }

                lastRect = GUILayoutUtility.GetLastRect();
                curHeight = lastRect.y + lastRect.height;

                if (m_spEventsFoldout.boolValue)
                {
                    SerializedProperty spEvent;

                    for (int i = 0; i < m_spEvents.arraySize; ++i)
                    {
                        spEvent = m_spEvents.GetArrayElementAtIndex(i);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(23f);

                        lastRect = GUILayoutUtility.GetLastRect();
                        lastRect.height = 17f;
                        lastRect.width = 17f;

                        if (GUI.Button(lastRect, "-"))
                        {
                            m_spEvents.DeleteArrayElementAtIndex(i);
                            --i;

                            m_data.ValidateEventMarkers();
                            continue;
                        }

                        spEvent.stringValue = EditorGUILayout.TextField(new GUIContent("Event " + (i + 1).ToString()), spEvent.stringValue);
                        EditorGUILayout.EndHorizontal();

                        curHeight += 18f;
                    }

                    lastRect = GUILayoutUtility.GetLastRect();
                    curHeight = lastRect.y + lastRect.height + 5f;

                    if (GUI.Button(new Rect(EditorGUIUtility.currentViewWidth - 175f, curHeight, 150f, 18f), 
                        new GUIContent("New Event Id")))
                    {
                        m_spEvents.InsertArrayElementAtIndex(m_spEvents.arraySize);
                        spEvent = m_spEvents.GetArrayElementAtIndex(m_spEvents.arraySize - 1);
                        spEvent.stringValue = "Event " + m_spEvents.arraySize.ToString();
                    }
                    curHeight += 27f;
                    GUILayout.Space(25f);
                }

                if (m_spOverrideEventModule.objectReferenceValue != null)
                {
                    EditorGUI.EndDisabledGroup();
                }

                curHeight -= 4f;
                GUILayout.Space(1f);
            }

            GUILayout.Space(1f);
            curHeight += 28f;

//             m_spMotionTimingFoldout.boolValue = EditorUtil.EditorFunctions.DrawFoldout(
//                 "Motion Speed & Timing", curHeight, EditorGUIUtility.currentViewWidth, m_spMotionTimingFoldout.boolValue);
//
//             lastRect = GUILayoutUtility.GetLastRect();
//             curHeight = lastRect.y + lastRect.height;
//
//             if (m_spMotionTimingFoldout.boolValue)
//             {
//                 EditorGUI.BeginDisabledGroup(true);
//                 EditorGUILayout.ObjectField(m_spMotionTimingPresets, new GUIContent("Motion Timing Presets"));
//                 EditorGUI.EndDisabledGroup();
//
//                 curHeight += 22f;
//                 if (m_spMotionTimingPresets.objectReferenceValue == null)
//                 {
//                     if (GUI.Button(new Rect(EditorGUIUtility.currentViewWidth - 175f, curHeight, 150f, 18f),
//                         new GUIContent("New Timing Presets")))
//                     {
//                         MotionTimingPresets motionTimingPresets = ScriptableObject.CreateInstance<MotionTimingPresets>();
//                         motionTimingPresets.name = "MxMMotionTimingPreset";
//                         AssetDatabase.AddObjectToAsset(motionTimingPresets, m_data);
//                         motionTimingPresets.hideFlags = m_spHideSubAssets.boolValue ? HideFlags.HideInHierarchy : HideFlags.None;
// #if UNITY_2020_2_OR_NEWER
//                         AssetDatabase.Refresh();
// #else
//                         AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(motionTimingPresets));
// #endif
//                         m_spMotionTimingPresets.objectReferenceValue = motionTimingPresets;
//                     }
//                 }
//                 else
//                 {
//                     lastRect = GUILayoutUtility.GetLastRect();
//                     curHeight = lastRect.y + lastRect.height + 3f;
//
//                     if (GUI.Button(new Rect(EditorGUIUtility.currentViewWidth - 175f, curHeight, 150f, 18f),
//                         new GUIContent("Delete Timing Presets")))
//                     {
//                         if(EditorUtility.DisplayDialog("Delete Motion Timing Presets", 
//                             "Are you sure you want to delete the motion timing presets", "Ok"))
//                         {
//                             AssetDatabase.RemoveObjectFromAsset(m_spMotionTimingPresets.objectReferenceValue);
// #if UNITY_2020_2_OR_NEWER
//                             AssetDatabase.Refresh();
// #else
//                             AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(m_data));
// #endif
//                             m_spMotionTimingPresets.objectReferenceValue = null;
//                         }
//                     }
//                 }
//
//                 curHeight += 11f;
//                 GUILayout.Space(28f);
//
//                 if (MxMSettings.HelpActive)
//                 {
//                     EditorGUILayout.HelpBox("Motion Speed & Timing presets allow you to set values that can be used in the MxMTimeline to modify the speed of animations." +
//                         "\n\nPlease note that this feature is for testing only and should not be used for production.", MessageType.Info);
//
//                     GUILayout.Space(4f);
//                     lastRect = GUILayoutUtility.GetLastRect();
//                     curHeight = lastRect.y + lastRect.height + 4f;
//                 }
//             }
//
//             curHeight += 27f;

            m_spPreProcessFoldout.boolValue = EditorUtil.EditorFunctions.DrawFoldout(
                "Pre-Process", curHeight, EditorGUIUtility.currentViewWidth, m_spPreProcessFoldout.boolValue);

            lastRect = GUILayoutUtility.GetLastRect();
            curHeight = lastRect.y + lastRect.height;

            if (m_spPreProcessFoldout.boolValue)
            {
                // m_spGenerateModifiedClipsOnPreProcess.boolValue = EditorGUILayout.Toggle("Generate Modified Clips", m_spGenerateModifiedClipsOnPreProcess.boolValue);
                //
                // if (MxMSettings.HelpActive)
                // {
                //     EditorGUILayout.HelpBox("Generate Modified Clips - If the animation speed / timing of any clips has been modified in the MxMTimeline then " +
                //         "checking this box will cause those clips to be generated with modified speeds. \n\nPlease note that this is a feature for testing and shouldn't" +
                //         "be relied on for production.", MessageType.Info);
                // }
                //
                // lastRect = GUILayoutUtility.GetLastRect();
                // curHeight = lastRect.y + lastRect.height + 3f;

                GUIStyle btnBoldStyle = new GUIStyle(GUI.skin.button);
                btnBoldStyle.fontSize = 16;
                btnBoldStyle.fontStyle = FontStyle.Bold;

                curHeight += 5f;
                GUILayout.Space(5f);

                if (GUI.Button(new Rect(50f, curHeight, Mathf.Max(EditorGUIUtility.currentViewWidth - 100f, 270f), 30f),
                    new GUIContent("Pre-Process Animation Data"), btnBoldStyle))
                {
                    bool shouldContinue = true;

                    int animCount = m_spBlendSpaces.arraySize;

                    for (int i = 0; i < m_spCompositeCategories.arraySize; ++i)
                    {
                        SerializedProperty spCategoryList = m_spCompositeCategories.GetArrayElementAtIndex(i);

                        if(spCategoryList != null)
                        {
                            SerializedProperty spCompositeList = spCategoryList.FindPropertyRelative("Composites");

                            if(spCompositeList != null)
                            {
                                animCount += spCompositeList.arraySize;
                            }
                        }
                    }

                    GameObject targetModel = m_data.Prefab;
                    MotionMatchConfigModule configModule = m_data.OverrideConfigModule;

                    if (configModule != null && configModule.Prefab != null)
                    {
                        targetModel = configModule.Prefab;
                    }

                    if (targetModel == null)
                    {
                        EditorUtility.DisplayDialog("Error: No Target Model!", "There is no target model specified " +
                            "in the 'General' foldout 'Target Model' parameter. Please add a model (not a prefab) and " +
                            "try again", "Ok");

                        shouldContinue = false;
                    }
                    else if (m_spTrajectoryTimes.arraySize == 0 && (configModule == null || configModule.TrajectoryPoints.Count == 0))
                    {
                        EditorUtility.DisplayDialog("Error: No Trajectory Configuration", "There are not trajectory" +
                            "points specified in the 'Trajectory Configuration foldout. Please add some. Four to five" +
                            "points are recommended with at least one set in the past.", "Ok");

                        shouldContinue = false;
                    }
                    else if (m_spPoseProperties.arraySize == 0 && (configModule == null || configModule.PoseJoints.Count == 0))
                    {
                        EditorUtility.DisplayDialog("Error: No Pose Configuration", "There are no joints specified" +
                            "in the pose configuration foldout. Please add some. Left foot, right foot and neck are " +
                            "recommended for basic locomotion.", "Ok");

                        shouldContinue = false;
                    }
                    else if (animCount == 0 && (m_data.AnimationModules.Count == 0 || m_data.AnimationModules[0] == null))
                    {
                        EditorUtility.DisplayDialog("Error: No animation added", "Please add some animations to the" +
                            "'Composites' or 'Blend Spaces' sub foldouts in the 'Animation Data' foldout to continue", "Ok");

                        shouldContinue = false;
                    }
                    else if (m_spAnimIdleSets.arraySize == 0 && (m_data.AnimationModules.Count == 0 || m_data.AnimationModules[0] == null))
                    {
                        EditorUtility.DisplayDialog("Error: No Idle Sets added", "Please add at least one idle animation" +
                            "to the 'Idle Sets' sub foldout in the 'Animation Data' foldout to continue", "Ok");
                    }
                    else if (!AreMxMAnimsValid())
                    {
                        EditorUtility.DisplayDialog("Error: Invalid animation data", "You have invalid animation data in your" +
                            "pre-processor. Pre-Processing cancelled.", "Ok");
                    }

                    //if (shouldContinue)
                    //{
                    //    //Check animation data generic or humanoid
                    //    if (!m_data.CheckAnimationCompatibility(m_spUseGenericRig.boolValue))
                    //    {
                    //        shouldContinue = false;
                    //        EditorUtility.DisplayDialog("Error: Incompatible Clips", "You are trying to use animation clips that are not compatible with" +
                    //            "your target model. If your character is humanoid please only use humanoid animation clips, likewise for generic rigs", "Ok");
                    //    }
                    //}

                    if (shouldContinue && m_spGenerateModifiedClipsOnPreProcess.boolValue)
                    {
                        shouldContinue = EditorUtility.DisplayDialog("Generating Clips", "You have chosen to generate modified clips on pre-process. This may take some time", "Ok", "Cancel");
                    }

                    if (shouldContinue && m_spEmbedAnimClipsInAnimData.boolValue && shouldContinue)
                    {
                        shouldContinue = EditorUtility.DisplayDialog("Embed Clips", "You have chosen to embed animation clips in AnimData. This may take some time and will result" +
                            "in a very large file", "Ok", "Cancel");
                    }

                    if (shouldContinue)
                    {
                        m_data.ValidateData();
                        EditorUtility.SetDirty(m_data);
                        AssetDatabase.SaveAssets();

                        serializedObject.ApplyModifiedProperties();

                        string startLocation;
                        string fileName;

                        if(m_data.LastSavedAnimData != null)
                        {
                            startLocation = AssetDatabase.GetAssetPath(m_data.LastSavedAnimData).Replace(m_data.LastSavedAnimData.name + ".asset", "");

                            fileName = EditorUtility.SaveFilePanelInProject("PreProcess Data",
                            m_data.LastSavedAnimData.name, "asset", "Process Animation Data as", startLocation).Replace(".asset", "");
                        }
                        else
                        {
                            startLocation = m_spLastSaveDirectory.stringValue;

                            if (startLocation == "")
                            {
                                startLocation = AssetDatabase.GetAssetPath(m_data).Replace(m_data.name + ".asset", "");
                            }

                            fileName = EditorUtility.SaveFilePanelInProject("PreProcess Data",
                            "MxMAnimData", "asset", "Process Animation Data as", startLocation).Replace(".asset", "");
                        }

                        

                        if (!string.IsNullOrEmpty(fileName))
                        {

#if UNITY_2020_2_OR_NEWER
                            m_data.LastSaveDirectory = fileName;
                            EditorUtility.SetDirty(m_data);
#else
                            m_spLastSaveDirectory.stringValue = fileName;
#endif



                            Object assetAtPath = AssetDatabase.LoadAssetAtPath(fileName + ".asset", typeof(Object));
                            if(assetAtPath != null)
                            {
                                MxMAnimData animDataAtPath  = AssetDatabase.LoadAssetAtPath<MxMAnimData>(fileName + ".asset");

                                if(animDataAtPath == null)
                                {
                                    EditorUtility.DisplayDialog("Error: Overwriting different asset type.",
                                        "You are trying to overwrite an asset that is not an MxMAnimData. This is not allowed", "Ok");

                                    shouldContinue = false;
                                }
                            }

                            if (shouldContinue)
                            {
                                PreProcessAnimationData(fileName);
                            }
                        }
                    }
                }

                curHeight += 35f;
                GUILayout.Space(35f);

                if (MxMSettings.HelpActive)
                {
                    EditorGUILayout.HelpBox("PreProcess Animatin Data - Once all animations are added and correctly configured, click this button to pre-process" +
                        "the data into a baked 'MxMAnimaData' asset which can be used in your MxMAnimator component. You will be prompted for a directory to " +
                        "place the MxMAnimData. \n\n Note: Overriding existing MxMAnimData will preserve any 'calibration sets' currently present on that MxMAnimData.", MessageType.Info);

                    lastRect = GUILayoutUtility.GetLastRect();
                    curHeight = lastRect.y + lastRect.height + 4f;
                }
            }

            curHeight += 83f;
            GUILayout.Space(12f);

            serializedObject.ApplyModifiedProperties();
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void ExportAnimDataToModule()
        {
            serializedObject.ApplyModifiedProperties();

            m_data.ValidateData();
            EditorUtility.SetDirty(m_data);
            AssetDatabase.SaveAssets();

            string startLocation;

            if (m_data.LastSavedAnimData != null)
            {
                startLocation = AssetDatabase.GetAssetPath(m_data.LastSavedAnimData).Replace(m_data.LastSavedAnimData.name + ".asset", "");
            }
            else
            {
                startLocation = m_spLastSaveDirectory.stringValue;

                if (startLocation == "")
                {
                    startLocation = AssetDatabase.GetAssetPath(m_data).Replace(m_data.name + ".asset", "");
                }
            }

            string fileName = EditorUtility.SaveFilePanelInProject("PreProcess Data",
                "MxMAnimationModule", "asset", "Export animation module as", startLocation).Replace(".asset", "");

            if (!string.IsNullOrEmpty(fileName))
            {
                m_spLastSaveDirectory.stringValue = fileName;

                bool shouldContinue = true;

                Object assetAtPath = AssetDatabase.LoadAssetAtPath(fileName + ".asset", typeof(Object));
                if (assetAtPath != null)
                {
                    AnimationModule animModuleAtPath = AssetDatabase.LoadAssetAtPath<AnimationModule>(fileName + ".asset");

                    if (animModuleAtPath == null)
                    {
                        EditorUtility.DisplayDialog("Error: Overwriting different asset type.",
                            "You are trying to overwrite an asset that is not an AnimationModule. This is not allowed", "Ok");

                        shouldContinue = false;
                    }
                }

                if (shouldContinue)
                {
                    AnimationModule animModule = ScriptableObject.CreateInstance<AnimationModule>();
                    AssetDatabase.CreateAsset(animModule, fileName + ".asset");
#if UNITY_2020_2_OR_NEWER
                    AssetDatabase.Refresh();
#else
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(animModule));
#endif

                    animModule.CopyPreProcessData(m_data);
                }
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void ManageQueuedActions()
        {
            if (m_queueDeleteCompositeCategory > -1)
            {
                if (m_queueDeleteCompositeCategory < m_spCompositeCategories.arraySize)
                {
                    SerializedProperty spCategory = m_spCompositeCategories.GetArrayElementAtIndex(m_queueDeleteCompositeCategory);

                    SerializedProperty spCompositeList = spCategory.FindPropertyRelative("Composites");

                    for (int i = 0; i < spCompositeList.arraySize; ++i)
                    {
                        SerializedProperty spObject = spCompositeList.GetArrayElementAtIndex(i);

                        AssetDatabase.RemoveObjectFromAsset(spObject.objectReferenceValue);
                        spObject.objectReferenceValue = null;
                    }
                    m_spCompositeCategories.DeleteArrayElementAtIndex(m_queueDeleteCompositeCategory);

#if UNITY_2020_2_OR_NEWER
                    AssetDatabase.Refresh();
#else
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(m_data));
#endif

                    m_compositeReorderableLists.RemoveAt(m_queueDeleteCompositeCategory);

                    SetupReorderableLists();

                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }

                m_queueDeleteCompositeCategory = -1;
            }

            if (m_queueShiftCompositeCategoryUp > 0) //Can't shift the first list up
            {
                if (m_queueShiftCompositeCategoryUp < m_spCompositeCategories.arraySize)
                {
                    m_spCompositeCategories.MoveArrayElement(m_queueShiftCompositeCategoryUp, m_queueShiftCompositeCategoryUp - 1);
                    SetupReorderableLists();
                }

                m_queueShiftCompositeCategoryUp = -1;
            }

            if(m_queueShiftCompositeCategoryDown > -1)
            {
                if (m_queueShiftCompositeCategoryDown + 1 < m_spCompositeCategories.arraySize)
                {
                    m_spCompositeCategories.MoveArrayElement(m_queueShiftCompositeCategoryDown, m_queueShiftCompositeCategoryDown + 1);
                    SetupReorderableLists();
                }

                m_queueShiftCompositeCategoryDown = -1;
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void PreProcessAnimationData(string _fileName)
        {
#if UNITY_2020_2_OR_NEWER
            if (m_data.GenerateModifiedClips)
#else
            if (m_spGenerateModifiedClipsOnPreProcess.boolValue )
#endif
            {
                if (!m_spEmbedAnimClipsInAnimData.boolValue)
                    Directory.CreateDirectory(_fileName);

                m_data.GenerateModifiedAnimations(_fileName);
            }

            MxMPreProcessor preProcessor = new MxMPreProcessor();
            preProcessor.SetupSceneForProcessing(m_data);

            MxMAnimData animData = (MxMAnimData)AssetDatabase.LoadAssetAtPath(_fileName + ".asset", typeof(MxMAnimData));

            bool existing = true;
            if(animData == null)
            {
                animData = ScriptableObject.CreateInstance<MxMAnimData>();
                existing = false;
            }

            preProcessor.PreProcessData(animData);

            MxMAnimData existingData = (MxMAnimData)AssetDatabase.LoadAssetAtPath(_fileName + ".asset", typeof(MxMAnimData));
            List<CalibrationData> copyOverCalibrationData = new List<CalibrationData>();
            if(existingData != null)
            {
                foreach(CalibrationData calibration in existingData.CalibrationSets)
                {
                    copyOverCalibrationData.Add(new CalibrationData(calibration));
                }
            }

            animData.InitializeCalibration(copyOverCalibrationData);

            EditorUtility.SetDirty(animData);

            if (!existing)
            {
                AssetDatabase.CreateAsset(animData, _fileName + ".asset");

#if UNITY_2020_2_OR_NEWER
                AssetDatabase.Refresh();
#else
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(animData));
#endif
            }

#if UNITY_2020_2_OR_NEWER           
            if(m_data.EmbedClips)
#else
            if (m_spEmbedAnimClipsInAnimData.boolValue)
#endif
            {
                EditorUtility.DisplayProgressBar("Embeded Clips", "Creating Embeded Clips", 0f);
                List<AnimationClip> baseAnims = new List<AnimationClip>(animData.Clips);

                for(int i=0; i < baseAnims.Count; ++i)
                {
                    AnimationClip clip = baseAnims[i];

                    EditorUtility.DisplayProgressBar("Embeded Clips", "Creating Embeded Clip: " + clip.name, ((float)i) / ((float)baseAnims.Count));
                    AnimationClip newClip = new AnimationClip();
                    EditorUtility.CopySerialized(clip, newClip);
                    AssetDatabase.AddObjectToAsset(newClip, animData);

#if UNITY_2020_2_OR_NEWER
                    AssetDatabase.Refresh();
#else
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newClip));
#endif
                    animData.Clips[i] = newClip;
                }
            }

            EditorUtility.ClearProgressBar();

            EditorUtility.SetDirty(animData);

#if UNITY_2020_2_OR_NEWER
            m_data.LastSavedAnimData = animData;
            EditorUtility.SetDirty(m_data);
#else
            m_spLastCreatedAnimData.objectReferenceValue = animData;
#endif
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void DragDropAnimationModules(Rect a_dropRect)
        {
            Event evt = Event.current;

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    {
                        if (!a_dropRect.Contains(evt.mousePosition))
                            return;

                        if (DragAndDrop.objectReferences[0].GetType() == typeof(AnimationModule))
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                            if (evt.type == EventType.DragPerform)
                            {
                                for (int i = 0; i < DragAndDrop.objectReferences.Length; ++i)
                                {
                                    DragAndDrop.AcceptDrag();

                                    Object obj = DragAndDrop.objectReferences[i];

                                    if (obj != null)
                                    {
                                        if (obj.GetType() == typeof(AnimationModule))
                                        {
                                            m_spAnimModules.InsertArrayElementAtIndex(m_spAnimModules.arraySize);
                                            SerializedProperty spAnimModule = m_spAnimModules.GetArrayElementAtIndex(m_spAnimModules.arraySize - 1);
                                            spAnimModule.objectReferenceValue = obj as AnimationModule;
                                        }
                                    }
                                }
                                Repaint();
                            }
                        }

                    }
                    break;
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void DragDropComposites(Rect a_dropRect, int a_categoryId)
        {
            Event evt = Event.current;

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    {
                        if (!a_dropRect.Contains(evt.mousePosition))
                            return;

                        if (DragAndDrop.objectReferences[0].GetType() == typeof(AnimationClip)
                            || DragAndDrop.objectReferences[0].GetType() == typeof(MxMAnimationClipComposite))
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                            if (evt.type == EventType.DragPerform)
                            {
                                for (int i = 0; i < DragAndDrop.objectReferences.Length; ++i)
                                {
                                    DragAndDrop.AcceptDrag();

                                    Object obj = DragAndDrop.objectReferences[i];

                                    if (obj != null)
                                    {
                                        if (obj.GetType() == typeof(AnimationClip))
                                        {
                                            CreateNewAnimationCompositeFromDrag(obj as AnimationClip, a_categoryId);
                                        }
                                        else if (obj.GetType() == typeof(MxMAnimationClipComposite))
                                        {
                                            CreateNewAnimationCompositeFromDrag(obj as MxMAnimationClipComposite, a_categoryId);
                                        }
                                    }
                                }
                                
                            }
                        }

                    }
                    break;
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void DragDropIdles(Rect a_dropRect)
        {
            Event evt = Event.current;

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    {
                        if (!a_dropRect.Contains(evt.mousePosition))
                            return;

                        if (DragAndDrop.objectReferences[0].GetType() == typeof(AnimationClip)
                            || DragAndDrop.objectReferences[0].GetType() == typeof(MxMAnimationIdleSet))
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                            if (evt.type == EventType.DragPerform)
                            {
                                for (int i = 0; i < DragAndDrop.objectReferences.Length; ++i)
                                {
                                    DragAndDrop.AcceptDrag();

                                    Object obj = DragAndDrop.objectReferences[i];

                                    if (obj != null)
                                    {
                                        if (obj.GetType() == typeof(AnimationClip))
                                        {
                                            CreateNewIdleSetFromDrag(obj as AnimationClip);
                                        }
                                        else if (obj.GetType() == typeof(MxMAnimationIdleSet))
                                        {
                                            CreateNewIdleSetFromDrag(obj as MxMAnimationIdleSet);
                                        }
                                    }
                                }
                                Repaint();
                            }
                        }

                    }
                    break;
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void DragDropBlendSpaces(Rect a_dropRect)
        {
            Event evt = Event.current;

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    {
                        if (!a_dropRect.Contains(evt.mousePosition))
                            return;

                        if (DragAndDrop.objectReferences[0].GetType() == typeof(AnimationClip)
                            || DragAndDrop.objectReferences[0].GetType() == typeof(MxMBlendSpace))
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                            if (evt.type == EventType.DragPerform)
                            {
                                for (int i = 0; i < DragAndDrop.objectReferences.Length; ++i)
                                {
                                    DragAndDrop.AcceptDrag();

                                    Object obj = DragAndDrop.objectReferences[i];

                                    if (obj != null)
                                    {
                                        if (obj.GetType() == typeof(AnimationClip))
                                        {
                                            CreateNewBlendSpaceFromDrag(obj as AnimationClip);
                                        }
                                        else if (obj.GetType() == typeof(MxMBlendSpace))
                                        {
                                            CreateNewBlendSpaceFromDrag(obj as MxMBlendSpace);
                                        }
                                    }
                                }
                                Repaint();
                            }
                        }

                    }
                    break;
            }
        }
        

        //===========================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void CreateNewAnimationComposite(int a_categoryId)
        {
            if (a_categoryId > m_spCompositeCategories.arraySize)
            {
                Debug.LogError("Trying to create a new animation composite, but the category Id is out of range. Aborting operation.");
                return;
            }

            MxMAnimationClipComposite newComposite = ScriptableObject.CreateInstance<MxMAnimationClipComposite>();
            newComposite.name = "MxMAnimComposite";

            if (newComposite != null)
            {
                AssetDatabase.AddObjectToAsset(newComposite, m_data);
                newComposite.hideFlags = m_spHideSubAssets.boolValue ? HideFlags.HideInHierarchy : HideFlags.None;
#if UNITY_2020_2_OR_NEWER
                AssetDatabase.Refresh();
#else
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newComposite));
#endif

                SerializedProperty spCategory = m_spCompositeCategories.GetArrayElementAtIndex(a_categoryId);
                SerializedProperty spCompositeList = spCategory.FindPropertyRelative("Composites");

                spCompositeList.InsertArrayElementAtIndex(spCompositeList.arraySize);

                SerializedProperty spComposite = spCompositeList.GetArrayElementAtIndex(spCompositeList.arraySize - 1);
                spComposite.objectReferenceValue = newComposite;

                CompositeCategory category = m_data.CompositeCategories[a_categoryId];

                newComposite.IgnoreEdges = category.IgnoreEdges_default;
                newComposite.ExtrapolateTrajectory = category.Extrapolate_default;
                newComposite.FlattenTrajectory = category.FlattenTrajectory_default;
                newComposite.RuntimeSplicing = category.RuntimeSplicing_default;
                newComposite.GlobalTags = category.RequireTags_default;
                newComposite.GlobalFavourTags = category.FavourTags_default;
                newComposite.TargetPreProcess = m_data;
                newComposite.TargetAnimModule = null;
                newComposite.TargetPrefab = m_data.Prefab;
                newComposite.CategoryId = a_categoryId;
                
                newComposite.ValidateBaseData();

                EditorUtility.SetDirty(newComposite);
            }
        }

        //===========================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public bool CreateNewAnimationCompositeFromDrag(AnimationClip a_clip, int a_categoryId)
        {
            if (a_categoryId > m_spCompositeCategories.arraySize)
            {
                Debug.LogError("Trying to create a new animation composite, but the category Id is out of range. Aborting operation.");
                return false;
            }

            bool success = false;

            if (a_clip != null)
            {
                MxMAnimationClipComposite newComposite =
                    ScriptableObject.CreateInstance<MxMAnimationClipComposite>();

                newComposite.name = a_clip.name + "_comp";

                if (newComposite != null)
                {
                    newComposite.SetPrimaryAnim(a_clip);

                    AssetDatabase.AddObjectToAsset(newComposite, m_data);
                    newComposite.hideFlags = m_spHideSubAssets.boolValue ? HideFlags.HideInHierarchy : HideFlags.None;
#if UNITY_2020_2_OR_NEWER
                    AssetDatabase.Refresh();
#else
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newComposite));
#endif

                    SerializedProperty spCategory = m_spCompositeCategories.GetArrayElementAtIndex(a_categoryId);
                    SerializedProperty spCompositeList = spCategory.FindPropertyRelative("Composites");

                    spCompositeList.InsertArrayElementAtIndex(spCompositeList.arraySize);

                    SerializedProperty spComposite = spCompositeList.GetArrayElementAtIndex(spCompositeList.arraySize - 1);
                    spComposite.objectReferenceValue = newComposite;

                    CompositeCategory category = m_data.CompositeCategories[a_categoryId];

                    newComposite.IgnoreEdges = category.IgnoreEdges_default;
                    newComposite.ExtrapolateTrajectory = category.Extrapolate_default;
                    newComposite.FlattenTrajectory = category.FlattenTrajectory_default;
                    newComposite.RuntimeSplicing = category.RuntimeSplicing_default;
                    newComposite.GlobalTags = category.RequireTags_default;
                    newComposite.GlobalFavourTags = category.FavourTags_default;
                    newComposite.TargetPreProcess = m_data;
                    newComposite.TargetPrefab = m_data.Prefab;
                    newComposite.CategoryId = a_categoryId;
                    
                    newComposite.ValidateBaseData();

                    EditorUtility.SetDirty(newComposite);

                    success = true;
                }
            }

            return success;
        }

        //===========================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public bool CreateNewAnimationCompositeFromDrag(MxMAnimationClipComposite a_composite, int a_categoryId)
        {
            if (a_categoryId > m_spCompositeCategories.arraySize)
            {
                Debug.LogError("Trying to create a new animation composite, but the category Id is out of range. Aborting operation.");
                return false;
            }

            bool success = false;

            if (a_composite != null)
            {
                MxMAnimationClipComposite newComposite = ScriptableObject.CreateInstance<MxMAnimationClipComposite>();
                EditorUtility.CopySerialized(a_composite, newComposite);
                newComposite.TargetPreProcess = m_data;
                newComposite.TargetPrefab = m_data.Prefab;
                newComposite.CategoryId = a_categoryId;
                newComposite.name = a_composite.name;

                AssetDatabase.AddObjectToAsset(newComposite, m_data);
                newComposite.hideFlags = m_spHideSubAssets.boolValue ? HideFlags.HideInHierarchy : HideFlags.None;
#if UNITY_2020_2_OR_NEWER
                AssetDatabase.Refresh();
#else
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newComposite));
#endif

                SerializedProperty spCategory = m_spCompositeCategories.GetArrayElementAtIndex(a_categoryId);
                SerializedProperty spCompositeList = spCategory.FindPropertyRelative("Composites");

                spCompositeList.InsertArrayElementAtIndex(spCompositeList.arraySize);
                SerializedProperty spComposite = spCompositeList.GetArrayElementAtIndex(spCompositeList.arraySize - 1);
                spComposite.objectReferenceValue = newComposite;

                newComposite.ValidateBaseData();

                EditorUtility.SetDirty(newComposite);

                success = true;
            }

            return success;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void CreateNewIdleSet()
        {
            MxMAnimationIdleSet newIdleSet = ScriptableObject.CreateInstance<MxMAnimationIdleSet>();
            if (newIdleSet != null)
            {
                newIdleSet.name = "MxMIdleSet";
                newIdleSet.TargetPreProcess = m_data;

                AssetDatabase.AddObjectToAsset(newIdleSet, m_data);
                newIdleSet.hideFlags = m_spHideSubAssets.boolValue ? HideFlags.HideInHierarchy : HideFlags.None;
#if UNITY_2020_2_OR_NEWER
                AssetDatabase.Refresh();
#else
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newIdleSet));
#endif

                m_spAnimIdleSets.InsertArrayElementAtIndex(m_spAnimIdleSets.arraySize);
                SerializedProperty spIdleSet = m_spAnimIdleSets.GetArrayElementAtIndex(m_spAnimIdleSets.arraySize - 1);
                spIdleSet.objectReferenceValue = newIdleSet;
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public bool CreateNewIdleSetFromDrag(AnimationClip a_primaryIdleAnim)
        {
            bool success = false;

            if (a_primaryIdleAnim != null)
            {
                MxMAnimationIdleSet newIdleSet =
                    ScriptableObject.CreateInstance<MxMAnimationIdleSet>();
                newIdleSet.name = a_primaryIdleAnim.name;

                if (newIdleSet != null)
                {
                    newIdleSet.SetPrimaryAnim(a_primaryIdleAnim);
                    newIdleSet.MinLoops = 1;
                    newIdleSet.MaxLoops = 2;
                    newIdleSet.TargetPreProcess = m_data;

                    AssetDatabase.AddObjectToAsset(newIdleSet, m_data);
                    newIdleSet.hideFlags = m_spHideSubAssets.boolValue ? HideFlags.HideInHierarchy : HideFlags.None;

#if UNITY_2020_2_OR_NEWER
                    AssetDatabase.Refresh();
#else
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newIdleSet));
#endif

                    m_spAnimIdleSets.InsertArrayElementAtIndex(m_spAnimIdleSets.arraySize);
                    SerializedProperty spIdleSet = m_spAnimIdleSets.GetArrayElementAtIndex(m_spAnimIdleSets.arraySize - 1);
                    spIdleSet.objectReferenceValue = newIdleSet;

                    

                    success = true;
                }
            }

            return success;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public bool CreateNewIdleSetFromDrag(MxMAnimationIdleSet a_idleSet)
        {
            bool success = false;

            if (a_idleSet != null)
            {
                MxMAnimationIdleSet newIdleSet = ScriptableObject.CreateInstance<MxMAnimationIdleSet>();
                newIdleSet.CopyData(a_idleSet);
                newIdleSet.name = a_idleSet.name;

                AssetDatabase.AddObjectToAsset(newIdleSet, m_data);
                newIdleSet.hideFlags = m_spHideSubAssets.boolValue ? HideFlags.HideInHierarchy : HideFlags.None;
#if UNITY_2020_2_OR_NEWER
                AssetDatabase.Refresh();
#else
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newIdleSet));
#endif

                m_spAnimIdleSets.InsertArrayElementAtIndex(m_spAnimIdleSets.arraySize);
                SerializedProperty spIdleSet = m_spAnimIdleSets.GetArrayElementAtIndex(m_spAnimIdleSets.arraySize - 1);
                spIdleSet.objectReferenceValue = newIdleSet;

                success = true;
            }
            return success;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void CreateNewBlendSpace()
        {
            MxMBlendSpace newBlendSpace = ScriptableObject.CreateInstance<MxMBlendSpace>();
            newBlendSpace.name = "MxMBlendSpace";

            if (newBlendSpace != null)
            {
                newBlendSpace.SetTargetPreProcessor(m_data);

                AssetDatabase.AddObjectToAsset(newBlendSpace, m_data);
                newBlendSpace.hideFlags = m_spHideSubAssets.boolValue ? HideFlags.HideInHierarchy : HideFlags.None;
#if UNITY_2020_2_OR_NEWER
                AssetDatabase.Refresh();
#else
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newBlendSpace));
#endif

                m_spBlendSpaces.InsertArrayElementAtIndex(m_spBlendSpaces.arraySize);
                SerializedProperty spBlendSpace = m_spBlendSpaces.GetArrayElementAtIndex(m_spBlendSpaces.arraySize - 1);
                spBlendSpace.objectReferenceValue = newBlendSpace;
                
                newBlendSpace.ValidateBaseData();
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public bool CreateNewBlendSpaceFromDrag(AnimationClip a_primaryBlendAnim)
        {
            bool success = false;

            if (a_primaryBlendAnim != null)
            {
                MxMBlendSpace newBlendSpace =
                    ScriptableObject.CreateInstance<MxMBlendSpace>();
                newBlendSpace.name = a_primaryBlendAnim.name;

                if (newBlendSpace != null)
                {
                    newBlendSpace.SetPrimaryAnim(a_primaryBlendAnim);
                    newBlendSpace.SetTargetPreProcessor(m_data);

                    AssetDatabase.AddObjectToAsset(newBlendSpace, m_data);
                    newBlendSpace.hideFlags = m_spHideSubAssets.boolValue ? HideFlags.HideInHierarchy : HideFlags.None;
#if UNITY_2020_2_OR_NEWER
                    AssetDatabase.Refresh();
#else
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newBlendSpace));
#endif

                    m_spBlendSpaces.InsertArrayElementAtIndex(m_spBlendSpaces.arraySize);
                    SerializedProperty spBlendSpace = m_spBlendSpaces.GetArrayElementAtIndex(m_spBlendSpaces.arraySize - 1);
                    spBlendSpace.objectReferenceValue = newBlendSpace;
                    
                    newBlendSpace.ValidateBaseData();

                    success = true;
                }
            }

            return success;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public bool CreateNewBlendSpaceFromDrag(MxMBlendSpace a_blendSpace)
        {
            bool success = false;

            if (a_blendSpace != null)
            {
                MxMBlendSpace newBlendSpace = ScriptableObject.CreateInstance<MxMBlendSpace>();
                newBlendSpace.TargetPreProcess = m_data;
                newBlendSpace.TargetPrefab = m_data.Prefab;
                newBlendSpace.CopyData(a_blendSpace);
                newBlendSpace.name = a_blendSpace.name;


                AssetDatabase.AddObjectToAsset(newBlendSpace, m_data);
                newBlendSpace.hideFlags = m_spHideSubAssets.boolValue ? HideFlags.HideInHierarchy : HideFlags.None;
#if UNITY_2020_2_OR_NEWER
                AssetDatabase.Refresh();
#else
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newBlendSpace));
#endif

                m_spBlendSpaces.InsertArrayElementAtIndex(m_spBlendSpaces.arraySize);
                SerializedProperty spBlendSpace = m_spBlendSpaces.GetArrayElementAtIndex(m_spBlendSpaces.arraySize - 1);
                spBlendSpace.objectReferenceValue = newBlendSpace;
                
                newBlendSpace.ValidateBaseData();

                success = true;
            }
            return success;
        }

        private List<string> GetAllChildPaths(Transform a_transform, string a_startPath)
        {
            List<string> childPathList = new List<string>();
            List<Transform> childTFormList = new List<Transform>();

            if (a_transform != null)
            {
                int childCount = a_transform.childCount;

                for(int i=0; i < childCount; ++i)
                {
                    Transform child = a_transform.GetChild(i);

                    childPathList.Add(a_startPath + "/" + child.name);
                    childTFormList.Add(child);
                }

                for(int i=0; i < childTFormList.Count; ++i)
                {
                    childPathList.AddRange(GetAllChildPaths(childTFormList[i], childPathList[i]));
                }
            }

            return childPathList;
        }


        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private bool AreMxMAnimsValid()
        {
            if (m_data != null)
            {
                foreach(AnimationModule animModule in m_data.AnimationModules)
                {
                    if(!animModule.AreMxMAnimsValid())
                    {
                        return false;
                    }
                }


                List<MxMAnimationIdleSet> idleSets = m_data.AnimationIdleSets;
                List<MxMBlendSpace> blendSpaces = m_data.BlendSpaces;

                foreach(CompositeCategory category in m_data.CompositeCategories)
                {
                    if(category != null && category.Composites != null)
                    {
                        foreach(MxMAnimationClipComposite composite in category.Composites)
                        {
                            if (composite == null)
                                return false;

                            if(composite.PrimaryClip == null)
                            {
                                EditorUtility.DisplayDialog("Error: Empty Composite", "You have a composite with no animations in it. Main PreProcessor, Category: " 
                                    + category.CatagoryName + " Composite: " + composite.CompositeName 
                                    + ". Please add an animation or remove the composite before pre-processing", "Ok");

                                EditorGUIUtility.PingObject(composite);

                                return false;
                            }
                        }
                    }
                }

                if(idleSets != null)
                {
                    foreach(MxMAnimationIdleSet idleSet in idleSets)
                    {
                        if (idleSet == null)
                        {
                            return false;
                        }

                        if (idleSet.PrimaryClip == null)
                        {
                            EditorUtility.DisplayDialog("Error: Empty Idle Set", "You have a IdleSet with no animations in it. Main PreProcessor." 
                                + "Please add an animation or remove the idle set before pre-processing", "Ok");

                            EditorGUIUtility.PingObject(idleSet);

                            return false;
                        }
                    }
                }

                if(blendSpaces != null)
                {
                    foreach(MxMBlendSpace blendSpace in blendSpaces)
                    {
                        if (blendSpace == null)
                            return false;

                        List<AnimationClip> clips = blendSpace.Clips;

                        if(clips == null || clips.Count == 0)
                        {
                            EditorUtility.DisplayDialog("Error: Empty blend space", "You have a blendspace with no animations in it. Main PreProcessor." 
                                + " BlendSpace: " + blendSpace.BlendSpaceName + ". Please add an animation or remove the blendspace before pre-processing", "Ok");

                            EditorGUIUtility.PingObject(blendSpace);

                            return false;
                        }

                        if (clips[0] == null)
                        {
                            EditorUtility.DisplayDialog("Error: Empty blend space", "You have a blendspace with no animations in it. Main PreProcessor." 
                                + " BlendSpace: " + blendSpace.BlendSpaceName + ". Please add an animation or remove the blendspace before pre-processing", "Ok");

                            EditorGUIUtility.PingObject(blendSpace);

                            return false;
                        }
                    }
                }

            }
            else
            {
                return false;
            }

            return true;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void UpdateTargetModelForAnimData()
        {
            if (m_spTargetPrefab.objectReferenceValue == null)
            {
                if(m_spOverrideConfigModule.objectReferenceValue == null)
                {
                    return;
                }
                else
                {
                    var configModule = m_spOverrideConfigModule.objectReferenceValue as MotionMatchConfigModule;
                    if (configModule.Prefab == null)
                        return;
                }
            }

            for (int i = 0; i < m_spCompositeCategories.arraySize; ++i)
            {
                SerializedProperty spCategory = m_spCompositeCategories.GetArrayElementAtIndex(i);
                SerializedProperty spCompositeList = spCategory.FindPropertyRelative("Composites");

                for(int k = 0; k < spCompositeList.arraySize; ++k)
                {
                    SerializedProperty spComposite = spCompositeList.GetArrayElementAtIndex(k);

                    if (spComposite.objectReferenceValue != null)
                    {
                        var composite = spComposite.objectReferenceValue as MxMAnimationClipComposite;

                        if (m_spOverrideConfigModule.objectReferenceValue != null)
                        {
                            var configModule = m_spOverrideConfigModule.objectReferenceValue as MotionMatchConfigModule;
                            composite.TargetPrefab = configModule.Prefab;
                        }
                        else
                        {
                            composite.TargetPrefab = m_spTargetPrefab.objectReferenceValue as GameObject;
                        }
                        composite.TargetPreProcess = m_data;

                        EditorUtility.SetDirty(spComposite.objectReferenceValue);
                    }
                }
            }

            for(int i=0; i < m_spBlendSpaces.arraySize; ++i)
            {
                SerializedProperty spBlendSpace = m_spBlendSpaces.GetArrayElementAtIndex(i);

                if(spBlendSpace.objectReferenceValue != null)
                {
                    MxMBlendSpace blendSpace = spBlendSpace.objectReferenceValue as MxMBlendSpace;

                    if (m_spOverrideConfigModule.objectReferenceValue != null)
                    {
                        var configModule = m_spOverrideConfigModule.objectReferenceValue as MotionMatchConfigModule;
                        blendSpace.TargetPrefab = configModule.Prefab;
                    }
                    else
                    {
                        blendSpace.TargetPrefab = m_spTargetPrefab.objectReferenceValue as GameObject;
                    }

                    blendSpace.SetTargetPreProcessor(m_data);

                    EditorUtility.SetDirty(spBlendSpace.objectReferenceValue);
                }
            }
        }

        private bool IsTargetPrefabHuman()
        {
            bool ret = true;

            if (m_spTargetPrefab.objectReferenceValue != null)
            {
                GameObject targetPrefab = m_spTargetPrefab.objectReferenceValue as GameObject;

                Animator animator = targetPrefab.GetComponent<Animator>();

                bool animatorAdded = false;
                if (animator == null)
                {
                    animator = targetPrefab.AddComponent<Animator>();
                    animatorAdded = true;
                }

                if (!animator.isHuman)
                {
                    ret = false;
                }

                if (animatorAdded)
                {
                    GameObject.Destroy(animator);
                }

            }

            return ret;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void SetupReorderableLists()
        {
            m_compositeReorderableLists = new List<ReorderableList>();

            for (int i = 0; i < m_spCompositeCategories.arraySize; ++i)
            {
                SerializedProperty spCategory = m_spCompositeCategories.GetArrayElementAtIndex(i);
                SerializedProperty spCompositeList = spCategory.FindPropertyRelative("Composites");

                ReorderableList compositeReorderableList = new ReorderableList(serializedObject,
                    spCompositeList, true, true, true, true);

                m_compositeReorderableLists.Add(compositeReorderableList);

                compositeReorderableList.drawElementCallback =
                    (Rect a_rect, int a_index, bool a_isActive, bool a_isFocused) =>
                    {
                        var element = compositeReorderableList.serializedProperty.GetArrayElementAtIndex(a_index);

                        EditorGUI.BeginDisabledGroup(true);
                        
                        string elementName = "Anim " + (a_index + 1).ToString();

                        if (element.objectReferenceValue != null)
                        {
                            string testName = ((MxMAnimationClipComposite) element.objectReferenceValue).CompositeName;
                            if (testName != "")
                            {
                                elementName = testName;
                            }
                        }
                        
                        EditorGUI.LabelField(new Rect(a_rect.x, a_rect.y, 100f, EditorGUIUtility.singleLineHeight), elementName);
                        
                        EditorGUI.ObjectField(new Rect(a_rect.x + 100f, a_rect.y, EditorGUIUtility.currentViewWidth - 170f,
                            EditorGUIUtility.singleLineHeight), element, new GUIContent(""));

                        EditorGUI.EndDisabledGroup();
                    };

                compositeReorderableList.drawHeaderCallback =
                    (Rect a_rect) =>
                    {
                        Rect deleteRect = a_rect;
                        deleteRect.x = a_rect.width - 186f;
                        deleteRect.width = 50f;
                        deleteRect.y += 1f;
                        deleteRect.height -= 2f;

                        a_rect.x += 10f;
                        a_rect.width /= 2f;
                        a_rect.y += 1f;

                        GUIStyle smallTitleStyle = new GUIStyle(GUI.skin.label);
                        smallTitleStyle.fontSize = 10;
                        smallTitleStyle.fontStyle = FontStyle.Bold;

                        SerializedProperty category = m_spCompositeCategories.GetArrayElementAtIndex(m_currentCompositeCategory);
                        SerializedProperty categoryName = category.FindPropertyRelative("CatagoryName");

                        a_rect.width = smallTitleStyle.CalcSize(new GUIContent(categoryName.stringValue)).x + 10f;
                        
                        categoryName.stringValue = EditorGUI.TextField(a_rect, categoryName.stringValue, smallTitleStyle);

                        GUIStyle redButtonStyle = new GUIStyle(GUI.skin.button);
                        redButtonStyle.normal.textColor = Color.red;

                        if (GUI.Button(deleteRect, "Delete", redButtonStyle))
                        {
                            if (EditorUtility.DisplayDialog("WARNING! Delete Composite List?",
                                "STOP! WAIT! HOLD UP! \n\nYOU ARE ABOUT TO DELETE AN ENTIRE COMPOSITE LIST! \n\nAre you sure?", "Yes", "Cancel"))
                            {
                                m_queueDeleteCompositeCategory = m_currentCompositeCategory;
                            }
                        }

                        if (GUI.Button(new Rect(deleteRect.x - deleteRect.width - 15f, deleteRect.y,
                            deleteRect.width, deleteRect.height), "Export"))
                        {
                            ExportCategoryToModule(m_currentCompositeCategory);
                        }

                        if (GUI.Button(new Rect(deleteRect.x + deleteRect.width + 30f, deleteRect.y,
                            deleteRect.width, deleteRect.height), "Up"))
                        {
                            m_queueShiftCompositeCategoryUp = m_currentCompositeCategory;
                        }

                        if (GUI.Button(new Rect(deleteRect.x + deleteRect.width * 2f + 35f, deleteRect.y, 
                            deleteRect.width, deleteRect.height), "Down"))
                        {
                            m_queueShiftCompositeCategoryDown = m_currentCompositeCategory;
                        }

                        GUIStyle invisiButton = new GUIStyle(GUI.skin.label);

                        Rect settingsBtnRect = new Rect(deleteRect.x + deleteRect.width * 3f + 40f,
                            deleteRect.y, 15f, 14f);

                        if (GUI.Button(settingsBtnRect, "", invisiButton))
                        {
                            CompositeCategorySettingsWindow.SetData(serializedObject, m_data, m_currentCompositeCategory);
                            CompositeCategorySettingsWindow.ShowWindow();
                        }

                        Texture icon = EditorGUIUtility.IconContent("_Popup").image;
                        GUI.DrawTexture(settingsBtnRect, icon);
                        
                    };

                int id = i;
                compositeReorderableList.onAddCallback =
                   (ReorderableList a_list) =>
                   {
                       CreateNewAnimationComposite(id);
                   };

                // compositeReorderableList.onSelectCallback =
                //     (ReorderableList a_list) =>
                //     {
                //         PingSelectedAnimation(id);
                //     };

                compositeReorderableList.onRemoveCallback =
                (ReorderableList a_list) =>
                {
                    if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete this composite", "Yes", "No"))
                    {
                        if (a_list.index >= 0 && a_list.index < a_list.serializedProperty.arraySize)
                        {
                            SerializedProperty spObject = a_list.serializedProperty.GetArrayElementAtIndex(a_list.index);

                            if (spObject.objectReferenceValue != null)
                            {
                                AssetDatabase.RemoveObjectFromAsset(spObject.objectReferenceValue);
#if UNITY_2020_2_OR_NEWER
                                AssetDatabase.Refresh();
#else
                                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(m_data));
#endif
                                spObject.objectReferenceValue = null;
                            }
                        }

                        ReorderableList.defaultBehaviours.DoRemoveButton(a_list);
                    }
                };
            }

            m_idleSetReorderableList = new ReorderableList(serializedObject,
                m_spAnimIdleSets, true, true, true, true);

            m_blendSpaceReorderableList = new ReorderableList(serializedObject,
                m_spBlendSpaces, true, true, true, true);

            m_animModuleReorderableList = new ReorderableList(serializedObject,
                m_spAnimModules, true, true, true, true);

            m_idleSetReorderableList.drawElementCallback =
                (Rect a_rect, int a_index, bool a_isActive, bool a_isFocused) =>
                {
                    var element = m_idleSetReorderableList.serializedProperty.GetArrayElementAtIndex(a_index);

                    EditorGUI.BeginDisabledGroup(true);

                    EditorGUI.LabelField(new Rect(a_rect.x, a_rect.y, 100f, EditorGUIUtility.singleLineHeight), "Idle Set " + (a_index + 1).ToString());
                    EditorGUI.ObjectField(new Rect(a_rect.x + 100f, a_rect.y, EditorGUIUtility.currentViewWidth - 170f,
                        EditorGUIUtility.singleLineHeight), element, new GUIContent(""));

                    EditorGUI.EndDisabledGroup();
                };

            m_blendSpaceReorderableList.drawElementCallback =
                (Rect a_rect, int a_index, bool a_isActive, bool a_isFocused) =>
                {
                    var element = m_blendSpaceReorderableList.serializedProperty.GetArrayElementAtIndex(a_index);

                    EditorGUI.BeginDisabledGroup(true);

                    MxMBlendSpace blendSpace = null;
                    
                    if (m_data.BlendSpaces.Count > a_index)
                    {
                        blendSpace = m_data.BlendSpaces[a_index];
                        EditorGUI.LabelField(new Rect(a_rect.x, a_rect.y, 150f, EditorGUIUtility.singleLineHeight), m_data.BlendSpaces[a_index].BlendSpaceName);
                    }
                    else
                    {
                        EditorGUI.LabelField(new Rect(a_rect.x, a_rect.y, 150f, EditorGUIUtility.singleLineHeight), "Blend Space " + (a_index + 1).ToString());
                    }

                    EditorGUI.ObjectField(new Rect(a_rect.x + 150f, a_rect.y, EditorGUIUtility.currentViewWidth - 220f,
                        EditorGUIUtility.singleLineHeight), element, new GUIContent(""));

                    EditorGUI.EndDisabledGroup();
                };

                m_animModuleReorderableList.drawElementCallback =
                (Rect a_rect, int a_index, bool a_isActive, bool a_isFocused) =>
                {
                    var element = m_animModuleReorderableList.serializedProperty.GetArrayElementAtIndex(a_index);

                    EditorGUI.BeginDisabledGroup(true);

                    EditorGUI.LabelField(new Rect(a_rect.x, a_rect.y, 100f, EditorGUIUtility.singleLineHeight), "Anim Module " + (a_index + 1).ToString());
                    EditorGUI.ObjectField(new Rect(a_rect.x + 100f, a_rect.y, EditorGUIUtility.currentViewWidth - 170f,
                        EditorGUIUtility.singleLineHeight), element, new GUIContent(""));

                    EditorGUI.EndDisabledGroup();
                };

            m_idleSetReorderableList.drawHeaderCallback =
                (Rect a_rect) =>
                {
                    EditorGUI.LabelField(a_rect, "Idle Sets");
                };

            m_blendSpaceReorderableList.drawHeaderCallback =
                (Rect a_rect) =>
                {
                    EditorGUI.LabelField(a_rect, "Blend Spaces");
                };

            m_animModuleReorderableList.drawHeaderCallback =
                (Rect a_rect) =>
                {
                    EditorGUI.LabelField(a_rect, "Animation Modules");
                };

            m_idleSetReorderableList.onAddCallback =
                (ReorderableList a_list) =>
                {
                    CreateNewIdleSet();
                };

            m_blendSpaceReorderableList.onAddCallback =
                (ReorderableList a_list) =>
                {
                    CreateNewBlendSpace();
                };

            m_idleSetReorderableList.onRemoveCallback =
                (ReorderableList a_list) =>
                {
                    if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete this idle set", "Yes", "No"))
                    {
                        if (a_list.index >= 0 && a_list.index < a_list.serializedProperty.arraySize)
                        {
                            SerializedProperty spObject = a_list.serializedProperty.GetArrayElementAtIndex(a_list.index);

                            if (spObject.objectReferenceValue != null)
                            {
                                AssetDatabase.RemoveObjectFromAsset(spObject.objectReferenceValue);
#if UNITY_2020_2_OR_NEWER
                                AssetDatabase.Refresh();
#else
                                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(m_data));
#endif
                                spObject.objectReferenceValue = null;
                            }
                        }

                        ReorderableList.defaultBehaviours.DoRemoveButton(a_list);
                    }
                };

            m_blendSpaceReorderableList.onRemoveCallback =
                (ReorderableList a_list) =>
                {
                    if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete this blend space", "Yes", "No"))
                    {
                        if (a_list.index >= 0 && a_list.index < a_list.serializedProperty.arraySize)
                        {
                            SerializedProperty spObject = a_list.serializedProperty.GetArrayElementAtIndex(a_list.index);

                            if (spObject.objectReferenceValue != null)
                            {
                                AssetDatabase.RemoveObjectFromAsset(spObject.objectReferenceValue);
#if UNITY_2020_2_OR_NEWER
                                AssetDatabase.Refresh();
#else
                                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(m_data));
#endif
                                spObject.objectReferenceValue = null;
                            }
                        }

                        ReorderableList.defaultBehaviours.DoRemoveButton(a_list);
                    }
                };

            m_animModuleReorderableList.onRemoveCallback =
                (ReorderableList a_list) =>
                {
                    if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to remove this animation module.", "Yes", "No"))
                    {
                        if (a_list.index >= 0 && a_list.index < a_list.serializedProperty.arraySize)
                        {
                            SerializedProperty spObject = a_list.serializedProperty.GetArrayElementAtIndex(a_list.index);

                            if (spObject.objectReferenceValue != null)
                            {
                                spObject.objectReferenceValue = null;
                            }
                        }

                        ReorderableList.defaultBehaviours.DoRemoveButton(a_list);
                    }
                };

            for (int i = 0; i < m_data.CompositeCategories.Count; ++i)
            {
                foreach(MxMAnimationClipComposite composite in m_data.CompositeCategories[i].Composites)
                {
                    composite.CategoryId = i;
                }
            }
        }

        public void ExportCategoryToModule(int a_categoryId)
        {
            serializedObject.ApplyModifiedProperties();

            m_data.ValidateData();
            EditorUtility.SetDirty(m_data);
            AssetDatabase.SaveAssets();

            string startLocation = AssetDatabase.GetAssetPath(m_data).Replace(name + ".asset", "");

            string fileName = EditorUtility.SaveFilePanelInProject("Export Composite Category",
                "MxMAnimationModule", "asset", "Export composite category as", startLocation).Replace(".asset", "");

            if (!string.IsNullOrEmpty(fileName))
            {
                bool shouldContinue = true;

                Object assetAtPath = AssetDatabase.LoadAssetAtPath(fileName + ".asset", typeof(Object));
                if (assetAtPath != null)
                {
                    EditorUtility.DisplayDialog("Error: Export will overwrite another asset.",
                            "You are trying to overwrite another asset. This is not allowed with exporting composite categories", "Ok");

                    shouldContinue = false;
                }

                if (shouldContinue)
                {
                    AnimationModule animModule = ScriptableObject.CreateInstance<AnimationModule>();
                    AssetDatabase.CreateAsset(animModule, fileName + ".asset");
#if UNITY_2020_2_OR_NEWER
                    AssetDatabase.Refresh();
#else
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(animModule));
#endif

                    animModule.CopyCompositeCategory(m_data, a_categoryId);
                }
            }
        }

    }//End of class: PreProcessDataInspector
}//End of namespace: MxMEditor