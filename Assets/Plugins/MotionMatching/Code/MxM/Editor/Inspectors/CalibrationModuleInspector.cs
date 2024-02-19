using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MxM;

namespace MxMEditor
{
    [CustomEditor(typeof(CalibrationModule))]
    public class CalibrationModuleInspector : Editor
    {
        private SerializedProperty m_spTargetAnimData;
        private SerializedProperty m_spCalibrationSets;

        private SerializedObject m_serializedObject;

        private List<bool> m_calibDataFoldouts;

        private CalibrationModule m_calibrationModule;

        private GUIContent m_poseTrajGUIContent;
        private GUIContent m_poseWeightGUIContent;
        private GUIContent m_bodyVelocityWeightGUIContent;
        private GUIContent m_resultantVelocityCostingGUIContent;
        private GUIContent m_trajPosWeightGUIContent;
        private GUIContent m_trajDirWeightGUIContent;

        private string m_jointPosTooltip;
        private string m_jointVelTooltip;

        private void OnEnable()
        {
            m_calibrationModule = target as CalibrationModule;
            m_calibrationModule.ValidateCalibrationSets();
            EditorUtility.SetDirty(m_calibrationModule);

            GetSerializedProperties();

            if (m_calibDataFoldouts == null)
            {
                m_calibDataFoldouts = new List<bool>(m_spCalibrationSets.arraySize + 1);

                for (int i = 0; i < m_spCalibrationSets.arraySize; ++i)
                {
                    m_calibDataFoldouts.Add(true);
                }
            }

            m_poseTrajGUIContent = new GUIContent("Pose-Traj Ratio",
            "A slider to balance the relationship between Pose Cost and Trajectory Cost. Setting the" +
            "value closer to '0' will make pose more weighted meaning more fluid motion. However, " +
            "values closer to '1' will make the trajectory more weighted meaning higher responsiveness." +
            "Treat this as a 'fluidity' - 'responsiveness' slider. User Manual: Section 6.3");

            m_poseWeightGUIContent = new GUIContent("Pose Weight", "This is an overall multiplier to pose joint" +
                "costs both for position and velocity. Each joint's position and velocity weighting is multiplied by this value" +
                "before to get their final weightings. Essentially, higher values make the pose more important as " +
                "a whole excluding body velocity while lower values do the opposite. User Manual: Section 6.1");

            m_bodyVelocityWeightGUIContent = new GUIContent("Body Velocity Weight", "For motion matching to work it is important" +
                "to match the overall body velocity to ensure it does not snap between different movement speeds too quickly. Higher" +
                "values for body velocity weighting will ensure more inertia is retained but it may be less responsive. " +
                "User Manual: Section 6.1");


            m_resultantVelocityCostingGUIContent = new GUIContent("Resultant Velocity Weight", "MxM doesn't just compare the velocity between" +
                "pose joints. It also checks to see if the resulting velocity of a joint, if a certain pose is picked' will match closesly to " +
                "the current velocity. This stops animation running backwards. This weighting is a multiplier to those costs. It is a very sensitive" +
                "setting and it is recommended to keep it around a value of 0.1f - 0.3f. User Manual: Section 6.1 ");

            m_trajPosWeightGUIContent = new GUIContent("Position Weight", "This value determines the importance of the trajectory points' position" +
                "when comparing to the desired trajectory. Higher values will make the trajectory position more important making for more responsive" +
                "animation. Keep in mind this multiplier does not affect trajectory angle. Typically the trajectory should have a higher weighting than" +
                "pose joints, around 5 - 10 depending on your game. User Manual: Section 6.2");


            m_trajDirWeightGUIContent = new GUIContent("Angle Weight", "This value determines the importance of the trajectory points' facing angle" +
                "when comparing to the desired trajectory. Higher values will make the trajectory facing angle more important ensuring that character" +
                "faces the direction you want them to face. This value does not affect trajectory position weighting. Facing angle differences are very " +
                "sensitive compared positions so a weighting around 0.03 - 0.1 is often used. User Manual: Section 6.2");

            m_jointPosTooltip = "Importance of this joint's position for pose comparison. Values of '1' " +
                "make the joint position cost unchanged. Lesser values make the joint position less important and higher " +
                "values make the joint position more important. Keep in mind that joint velocity values are much higher than" +
                "position values. So to normalize them, position weightings generaly need to be lower than velocity values." +
                "User Manual: Section 6.1";

            m_jointVelTooltip = "Importance of this joint's velocity for pose comparison. Values of '1' make the joint velocity" +
                "cost unchanged. Lesser values make the joint velocity less important and higher values make the joint velocity more" +
                "important. Keep in mind that joint velocity values are much higher than position values. So to normalize them, velocity" +
                "weighting generaly needs to be higher. User Manual: Section 6.1";
        }

        private void GetSerializedProperties()
        {
            m_serializedObject = new SerializedObject(m_calibrationModule);

            m_spTargetAnimData = m_serializedObject.FindProperty("m_targetAnimData");
            m_spCalibrationSets = m_serializedObject.FindProperty("m_calibrationSets");
        }

        public override void OnInspectorGUI()
        {
            GetSerializedProperties();

            EditorGUILayout.LabelField("");
            Rect lastRect = GUILayoutUtility.GetLastRect();

            float curHeight = lastRect.y + 9f;

            curHeight = EditorUtil.EditorFunctions.DrawTitle("MxM Calibration Module", curHeight);

            bool targetChanged = false;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_spTargetAnimData);
            if(EditorGUI.EndChangeCheck())
            {
                targetChanged = true;
                Repaint();
            }

            curHeight += 10f;
            GUILayout.Space(10f);

            MxMAnimData animData = m_spTargetAnimData.objectReferenceValue as MxMAnimData;

            if (animData == null)
                return;

            for (int i = 0; i < m_spCalibrationSets.arraySize; ++i)
            {
                SerializedProperty spCalibData = m_spCalibrationSets.GetArrayElementAtIndex(i);
                SerializedProperty spCalibrationName = spCalibData.FindPropertyRelative("CalibrationName");

                m_calibDataFoldouts[i] = EditorUtil.EditorFunctions.DrawFoldout(spCalibrationName.stringValue, curHeight, EditorGUIUtility.currentViewWidth,
                    m_calibDataFoldouts[i], 1, true);

                curHeight += 24f;

                if (m_calibDataFoldouts[i])
                {
                    SerializedProperty spPoseTrajectoryRatio = spCalibData.FindPropertyRelative("PoseTrajectoryRatio");
                    SerializedProperty spPoseVelocityWeight = spCalibData.FindPropertyRelative("PoseVelocityWeight");
                    SerializedProperty spPoseAspectMultiplier = spCalibData.FindPropertyRelative("PoseAspectMultiplier");
                    SerializedProperty spPoseResultantVelocityMultiplier = spCalibData.FindPropertyRelative("PoseResultantVelocityMultiplier");
                    SerializedProperty spTrajPosMultiplier = spCalibData.FindPropertyRelative("TrajPosMultiplier");
                    SerializedProperty spTrajFAngleMultiplier = spCalibData.FindPropertyRelative("TrajFAngleMultiplier");
                    SerializedProperty spJointPositionWeights = spCalibData.FindPropertyRelative("JointPositionWeights");
                    SerializedProperty spJointVelocityWeights = spCalibData.FindPropertyRelative("JointVelocityWeights");

                    spCalibrationName.stringValue = EditorGUILayout.TextField(new GUIContent("Name"), spCalibrationName.stringValue);

                    if (GUILayout.Button(new GUIContent("Delete")))
                    {
                        if (EditorUtility.DisplayDialog("Delete Calibration Data", "Are you sure you want to delete calibration data?", "Yes", "No"))
                        {
                            m_spCalibrationSets.DeleteArrayElementAtIndex(i);
                            --i;
                            Repaint();
                            continue;
                        }
                    }

                    EditorGUILayout.Slider(spPoseTrajectoryRatio, 0f, 1f, m_poseTrajGUIContent);

                    GUILayout.Space(5f);
                    EditorGUILayout.LabelField(new GUIContent("Pose", "Contains all weightings related to pose"), EditorStyles.boldLabel);

                    spPoseVelocityWeight.floatValue = EditorGUILayout.FloatField(m_bodyVelocityWeightGUIContent, spPoseVelocityWeight.floatValue);

                    spPoseAspectMultiplier.floatValue = EditorGUILayout.FloatField(m_poseWeightGUIContent, spPoseAspectMultiplier.floatValue);


                    spPoseResultantVelocityMultiplier.floatValue = EditorGUILayout.FloatField(m_resultantVelocityCostingGUIContent,
                        spPoseResultantVelocityMultiplier.floatValue);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(10f);
                    EditorGUILayout.LabelField("Joints:", EditorStyles.boldLabel);
                    EditorGUILayout.EndHorizontal();

                    int iterations = Mathf.Min(spJointPositionWeights.arraySize, animData.MatchBones.Length);
                    for (int n = 0; n < iterations; ++n)
                    {
                        SerializedProperty spJointPositionWeight = spJointPositionWeights.GetArrayElementAtIndex(n);
                        SerializedProperty spJointVelocityWeight = spJointVelocityWeights.GetArrayElementAtIndex(n);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(10f);

                        string boneName = "";

                        if (animData.GetBonesByName)
                        {
                            boneName = animData.MatchBonesGeneric[n];
                        }
                        else
                        {
                            boneName = animData.MatchBones[n].ToString();
                        }

                        spJointPositionWeight.floatValue = EditorGUILayout.FloatField(new GUIContent(
                            boneName + " Position", m_jointPosTooltip),
                            spJointPositionWeight.floatValue);
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(10f);
                        spJointVelocityWeight.floatValue = EditorGUILayout.FloatField(new GUIContent(
                            boneName + " Velocity", m_jointVelTooltip),
                            spJointVelocityWeight.floatValue);
                        EditorGUILayout.EndHorizontal();

                        curHeight += 18f * 2f;
                    }

                    GUILayout.Space(5f);
                    EditorGUILayout.LabelField(new GUIContent("Trajectory: ",
                        "Contains all weighting related to trajectory"), EditorStyles.boldLabel);

                    spTrajPosMultiplier.floatValue = EditorGUILayout.FloatField(m_trajPosWeightGUIContent, spTrajPosMultiplier.floatValue);

                    spTrajFAngleMultiplier.floatValue = EditorGUILayout.FloatField(m_trajDirWeightGUIContent, spTrajFAngleMultiplier.floatValue);

                    curHeight += 18f * 11f;
                    curHeight += 15f + 10f;
                    GUILayout.Space(10f);
                }
            }

            if (GUILayout.Button(new GUIContent("Add New Calibration")))
            {
                if (animData != null)
                {

                    m_spCalibrationSets.InsertArrayElementAtIndex(m_spCalibrationSets.arraySize);
                    SerializedProperty spCalibData = m_spCalibrationSets.GetArrayElementAtIndex(m_spCalibrationSets.arraySize - 1);

                    SerializedProperty spCalibrationName = spCalibData.FindPropertyRelative("CalibrationName");
                    spCalibrationName.stringValue = "Calibration " + (m_spCalibrationSets.arraySize - 1);

                    SerializedProperty spPoseTrajectoryRatio = spCalibData.FindPropertyRelative("PoseTrajectoryRatio");
                    SerializedProperty spPoseVelocityWeight = spCalibData.FindPropertyRelative("PoseVelocityWeight");
                    SerializedProperty spPoseAspectMultiplier = spCalibData.FindPropertyRelative("PoseAspectMultiplier");
                    SerializedProperty spTrajPosMultiplier = spCalibData.FindPropertyRelative("TrajPosMultiplier");
                    SerializedProperty spTrajFAngleMultiplier = spCalibData.FindPropertyRelative("TrajFAngleMultiplier");
                    SerializedProperty spJointPositionWeights = spCalibData.FindPropertyRelative("JointPositionWeights");
                    SerializedProperty spJointVelocityWeights = spCalibData.FindPropertyRelative("JointVelocityWeights");

                    spPoseTrajectoryRatio.floatValue = 0.6f;
                    spPoseAspectMultiplier.floatValue = 1f;
                    spPoseVelocityWeight.floatValue = 3f;
                    spTrajPosMultiplier.floatValue = 5f;
                    spTrajFAngleMultiplier.floatValue = 0.04f;

                    spJointPositionWeights.ClearArray();
                    spJointVelocityWeights.ClearArray();

                    for (int k = 0; k < animData.MatchBones.Length; ++k)
                    {
                        spJointPositionWeights.InsertArrayElementAtIndex(k);
                        spJointPositionWeights.GetArrayElementAtIndex(k).floatValue = 3f;

                        spJointVelocityWeights.InsertArrayElementAtIndex(k);
                        spJointVelocityWeights.GetArrayElementAtIndex(k).floatValue = 1f;
                    }

                    m_calibDataFoldouts.Add(true);
                }
            }

            m_serializedObject.ApplyModifiedProperties();

            curHeight += 22f;

            if (targetChanged)
            {
                m_calibrationModule.ValidateCalibrationSets();

                if (EditorUtility.DisplayDialog("Changing Source Anim Data",
                    "Do you want to copy calibration data from the new source anim Data?",
                    "Yes", "No"))
                {
                    MxMAnimData newAnimData = m_spTargetAnimData.objectReferenceValue as MxMAnimData;
                    m_calibrationModule.InitializeCalibration(newAnimData.CalibrationSets);

                    int CalibSetCount = m_calibrationModule.CalibrationSetCount;
                    m_calibDataFoldouts = new List<bool>(CalibSetCount + 1);

                    for (int i = 0; i < CalibSetCount; ++i)
                    {
                        m_calibDataFoldouts.Add(true);
                    }
                }

                EditorUtility.SetDirty(m_calibrationModule);
                Repaint();
            }
        }

    }//End of class: CalibrationModuleInspector
}//End of namespace: MxMEditor