using System.Collections.Generic;
using UnityEngine;
using MxM;
using UnityEditor;


[CustomEditor(typeof(MxMAnimData))]
public class MxMAnimDataInspector : Editor
{
    private SerializedProperty m_spAnimClipList;
    private SerializedProperty m_spCalibrationSets;
    private SerializedProperty m_spPoseMask;
    private SerializedProperty m_spStartPoseId;

    private bool m_generalFoldout = true;
    private bool m_animClipsFoldout = false;
    private bool m_poseMaskFoldout = true;
    private bool m_calibrationFoldout = true;

    private List<bool> m_calibDataFoldouts;

    private MxMAnimData m_animData;

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
        m_animData = target as MxMAnimData;

        m_spAnimClipList = serializedObject.FindProperty("Clips");
        m_spCalibrationSets = serializedObject.FindProperty("CalibrationSets");
        m_spPoseMask = serializedObject.FindProperty("poseMask");
        m_spStartPoseId = serializedObject.FindProperty("StartPoseId");

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

    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("");
        Rect lastRect = GUILayoutUtility.GetLastRect();

        float curHeight = lastRect.y + 9f;

        curHeight = EditorUtil.EditorFunctions.DrawTitle("MxM Animation Data", curHeight);

        m_generalFoldout = EditorUtil.EditorFunctions.DrawFoldout("General", curHeight, EditorGUIUtility.currentViewWidth, m_generalFoldout);
        if (m_generalFoldout)
        {
            EditorGUI.BeginChangeCheck();
            m_spStartPoseId.intValue = EditorGUILayout.IntField("Start Pose Id: ", m_spStartPoseId.intValue);
            if(EditorGUI.EndChangeCheck())
            {
                if (m_spStartPoseId.intValue < 0)
                    m_spStartPoseId.intValue = 0;

                if (m_spStartPoseId.intValue >= m_animData.Poses.Length)
                    m_spStartPoseId.intValue = m_animData.Poses.Length - 1;
            }
            
            EditorGUILayout.LabelField("Pose Interval: " + m_animData.PoseInterval + " seconds");
            EditorGUILayout.LabelField("Clip Count: " + m_animData.Clips.Length);
            EditorGUILayout.LabelField("Pose Count: " + m_animData.Poses.Length);
            EditorGUILayout.LabelField("Event Count: " + m_animData.Events.Length);
            GUILayout.Space(9f);

            EditorGUILayout.LabelField("Complex Anims:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.LabelField("Single Composite Count " + m_animData.ClipsData.Length);
                EditorGUILayout.LabelField("Spliced Composite Count " + m_animData.Composites.Length);
                EditorGUILayout.LabelField("IdleSet Count: " + m_animData.IdleSets.Length);
                EditorGUILayout.LabelField("BlendSpace Count: " + m_animData.BlendSpaces.Length);
                EditorGUILayout.LabelField("BlendClip Count: " + m_animData.BlendClips.Length);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(9f);

            EditorGUILayout.LabelField("Match Joints:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            EditorGUILayout.BeginVertical();

            if(m_animData.GetBonesByName)
            {
                for (int i = 0; i < m_animData.MatchBonesGeneric.Length; ++i)
                {
                    EditorGUILayout.LabelField((i + 1) + ". " + m_animData.MatchBonesGeneric[i]);
                    curHeight += 18f;
                }
            }
            else
            {
                for (int i = 0; i < m_animData.MatchBones.Length; ++i)
                {
                    EditorGUILayout.LabelField((i + 1) + ". " + m_animData.MatchBones[i].ToString());
                    curHeight += 18f;
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(9f);
            EditorGUILayout.LabelField("Trajectory Points:", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            EditorGUILayout.BeginVertical();

            for (int i=0; i < m_animData.PosePredictionTimes.Length; ++i)
            {
                EditorGUILayout.LabelField("Point " + i + ": " + m_animData.PosePredictionTimes[i].ToString());
                curHeight += 18f;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            curHeight += 18f * 8f + 1f;
        }

        curHeight += 30f;
        GUILayout.Space(3f);

        m_animClipsFoldout = EditorUtil.EditorFunctions.DrawFoldout("Animation Clips", curHeight, EditorGUIUtility.currentViewWidth, m_animClipsFoldout);

        if(m_animClipsFoldout)
        {

            EditorGUI.BeginDisabledGroup(true);
            for(int i=0; i < m_spAnimClipList.arraySize; ++i)
            {
                EditorGUILayout.ObjectField(m_spAnimClipList.GetArrayElementAtIndex(i), new GUIContent("Clip " + i));
                curHeight += 18f;
            }
            EditorGUI.EndDisabledGroup();

        }

        curHeight += 30f;
        GUILayout.Space(3f);


        m_calibrationFoldout = EditorUtil.EditorFunctions.DrawFoldout("Calibration Sets", curHeight, EditorGUIUtility.currentViewWidth, m_calibrationFoldout);
        curHeight += 28f;

        if(m_calibrationFoldout)
        {
            for(int i=0; i < m_spCalibrationSets.arraySize; ++i)
            {
                SerializedProperty spCalibData = m_spCalibrationSets.GetArrayElementAtIndex(i);
                SerializedProperty spCalibrationName = spCalibData.FindPropertyRelative("CalibrationName");

                m_calibDataFoldouts[i] = EditorUtil.EditorFunctions.DrawFoldout(spCalibrationName.stringValue, curHeight, EditorGUIUtility.currentViewWidth,
                    m_calibDataFoldouts[i], 1, true);

                curHeight += 24f;

                if (m_calibDataFoldouts[i])
                {
                    MxMAnimData animData = target as MxMAnimData;
                    SerializedProperty spPoseTrajectoryRatio = spCalibData.FindPropertyRelative("PoseTrajectoryRatio");
                    SerializedProperty spPoseVelocityWeight = spCalibData.FindPropertyRelative("PoseVelocityWeight");
                    SerializedProperty spPoseAspectMultiplier = spCalibData.FindPropertyRelative("PoseAspectMultiplier");
                    SerializedProperty spPoseResultantVelocityMultiplier = spCalibData.FindPropertyRelative("PoseResultantVelocityMultiplier");
                    SerializedProperty spTrajPosMultiplier = spCalibData.FindPropertyRelative("TrajPosMultiplier");
                    SerializedProperty spTrajFAngleMultiplier = spCalibData.FindPropertyRelative("TrajFAngleMultiplier");
                    SerializedProperty spJointPositionWeights = spCalibData.FindPropertyRelative("JointPositionWeights");
                    SerializedProperty spJointVelocityWeights = spCalibData.FindPropertyRelative("JointVelocityWeights");
                    
                    spCalibrationName.stringValue = EditorGUILayout.TextField(new GUIContent("Name"), spCalibrationName.stringValue);

                    if(GUILayout.Button(new GUIContent("Delete")))
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

                    for (int n = 0; n < spJointPositionWeights.arraySize; ++n)
                    {
                        SerializedProperty spJointPositionWeight = spJointPositionWeights.GetArrayElementAtIndex(n);
                        SerializedProperty spJointVelocityWeight = spJointVelocityWeights.GetArrayElementAtIndex(n);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(10f);

                        string boneName = "";

                        if(m_animData.GetBonesByName)
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
                if (m_animData != null)
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

                    for (int k = 0; k < m_animData.MatchBones.Length; ++k)
                    {
                        spJointPositionWeights.InsertArrayElementAtIndex(k);
                        spJointPositionWeights.GetArrayElementAtIndex(k).floatValue = 3f;

                        spJointVelocityWeights.InsertArrayElementAtIndex(k);
                        spJointVelocityWeights.GetArrayElementAtIndex(k).floatValue = 1f;
                    }

                    m_calibDataFoldouts.Add(true);
                }
            }
            curHeight += 22f;
        }

        curHeight += 1f;
        GUILayout.Space(2f);

        m_poseMaskFoldout = EditorUtil.EditorFunctions.DrawFoldout("Pose Mask", curHeight, EditorGUIUtility.currentViewWidth, m_poseMaskFoldout);

        if(m_poseMaskFoldout)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(m_spPoseMask, new GUIContent("Pose Mask"));
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button(new GUIContent("Strip Pose Mask")))
            {
                if (EditorUtility.DisplayDialog("Srip Pose Mask", "Are you sure you want to strip the pose mask? " +
                    "This operation cannot be reversed", "Yes", "No"))
                {
                    m_animData.StripPoseMask();
                }
            }

            curHeight += 18f * 2f;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
