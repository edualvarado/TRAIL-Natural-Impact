using UnityEngine;
using UnityEditor;
using MxM;

namespace MxMEditor
{
    [CustomEditor(typeof(MxMEventDefinition))]
    public class MxMEventDefinitionInspector : Editor
    {
        private SerializedProperty m_spId;
        private SerializedProperty m_spEventName;
        private SerializedProperty m_spEventType;
        private SerializedProperty m_spPriority;
        private SerializedProperty m_spContactCountToMatch;
        private SerializedProperty m_spContactCountToWarp;
        private SerializedProperty m_spExitWithMotion;
        private SerializedProperty m_spMatchPose;
        private SerializedProperty m_spMatchTrajectory;
        private SerializedProperty m_spMatchRequireTags;
        private SerializedProperty m_spFavourTagMethod;
        private SerializedProperty m_spPostEventTrajectoryMode;

        private SerializedProperty m_spMatchTiming;
        private SerializedProperty m_spExactTimeMatch;
        private SerializedProperty m_spTimingWeight;
        private SerializedProperty m_spTimingWarpType;

        private SerializedProperty m_spMatchPosition;
        private SerializedProperty m_spPositionWeight;
        private SerializedProperty m_spMotionWarpType;
        private SerializedProperty m_spWarpTimeScaling;
        private SerializedProperty m_spContactCountToTimeScale;
        private SerializedProperty m_spMinWarpTimeScale;
        private SerializedProperty m_spMaxWarpTimeScale;

        private SerializedProperty m_spMatchRotation;
        private SerializedProperty m_spRotationWeight;
        private SerializedProperty m_spRotationWarpType;
        
        private bool m_generalFoldout = true;
        private bool m_timingFoldout = true;
        private bool m_positionFoldout = true;
        private bool m_rotationFoldout = true;

        private SerializedProperty m_spEventNamingModule;
        private SerializedProperty m_spTargetAnimData;

        private void OnEnable()
        {
            m_spId = serializedObject.FindProperty("Id");
            m_spEventName = serializedObject.FindProperty("EventName");
            m_spEventType = serializedObject.FindProperty("EventType");
            m_spPriority = serializedObject.FindProperty("Priority");
            m_spContactCountToMatch = serializedObject.FindProperty("ContactCountToMatch");
            m_spContactCountToWarp = serializedObject.FindProperty("ContactCountToWarp");
            m_spExitWithMotion = serializedObject.FindProperty("ExitWithMotion");
            m_spMatchPose = serializedObject.FindProperty("MatchPose");
            m_spMatchTrajectory = serializedObject.FindProperty("MatchTrajectory");
            m_spMatchRequireTags = serializedObject.FindProperty("MatchRequireTags");
            m_spFavourTagMethod = serializedObject.FindProperty("FavourTagMethod");
            m_spPostEventTrajectoryMode = serializedObject.FindProperty("PostEventTrajectoryMode");

            m_spMatchTiming = serializedObject.FindProperty("MatchTiming");
            m_spExactTimeMatch = serializedObject.FindProperty("ExactTimeMatch");
            m_spTimingWeight = serializedObject.FindProperty("TimingWeight");
            m_spTimingWarpType = serializedObject.FindProperty("TimingWarpType");

            m_spMatchPosition = serializedObject.FindProperty("MatchPosition");
            m_spPositionWeight = serializedObject.FindProperty("PositionWeight");
            m_spMotionWarpType = serializedObject.FindProperty("MotionWarpType");

            m_spMatchRotation = serializedObject.FindProperty("MatchRotation");
            m_spRotationWeight = serializedObject.FindProperty("RotationWeight");
            m_spRotationWarpType = serializedObject.FindProperty("RotationWarpType");

            m_spWarpTimeScaling = serializedObject.FindProperty("WarpTimeScaling");
            m_spContactCountToTimeScale = serializedObject.FindProperty("ContactCountToTimeScale");
            m_spMinWarpTimeScale = serializedObject.FindProperty("MinWarpTimeScale");
            m_spMaxWarpTimeScale = serializedObject.FindProperty("MaxWarpTimeScale");

            m_spEventNamingModule = serializedObject.FindProperty("m_targetEventNamingModule");
            m_spTargetAnimData = serializedObject.FindProperty("m_targetAnimData");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("");
            Rect lastRect = GUILayoutUtility.GetLastRect();

            float curHeight = lastRect.y + 9f;

            curHeight = EditorUtil.EditorFunctions.DrawTitle("MxM Event Definition", curHeight);

            m_generalFoldout = EditorUtil.EditorFunctions.DrawFoldout("General", curHeight, EditorGUIUtility.currentViewWidth, m_generalFoldout);

            if (m_generalFoldout)
            {

                MxMAnimData animData = m_spTargetAnimData.objectReferenceValue as MxMAnimData;
                EventNamingModule eventNameData = m_spEventNamingModule.objectReferenceValue as EventNamingModule;

                if (eventNameData != null)
                {
                    EditorGUI.BeginChangeCheck();
                    m_spId.intValue = EditorGUILayout.Popup("Event Name", m_spId.intValue,
                        eventNameData.EventNames.ToArray());
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (m_spId.intValue > -1 && m_spId.intValue < eventNameData.EventNames.Count)
                        {
                            m_spEventName.stringValue = eventNameData.EventNames[m_spId.intValue];
                        }
                    }
                }
                else if (animData != null)
                {
                    EditorGUI.BeginChangeCheck();
                    m_spId.intValue = EditorGUILayout.Popup("Event Name", m_spId.intValue, animData.EventNames);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (m_spId.intValue > -1 && m_spId.intValue < animData.EventNames.Length)
                        {
                            m_spEventName.stringValue = animData.EventNames[m_spId.intValue];
                        }
                    }
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    m_spId.intValue = EditorGUILayout.IntField("Event Id", m_spId.intValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (m_spId.intValue < 0)
                            m_spId.intValue = 0;
                    }
                }

                m_spEventType.intValue =
                    (int)(EMxMEventType)EditorGUILayout.EnumPopup("Event Type", (EMxMEventType)m_spEventType.intValue);

                EditorGUI.BeginChangeCheck();
                m_spPriority.intValue = EditorGUILayout.IntField("Priority", m_spPriority.intValue);
                if (EditorGUI.EndChangeCheck())
                {
                    if (m_spPriority.intValue < -1)
                        m_spPriority.intValue = -1;
                }

                EditorGUI.BeginChangeCheck();
                m_spContactCountToMatch.intValue =
                    EditorGUILayout.IntField("Num Contacts to Match", m_spContactCountToMatch.intValue);
                if (EditorGUI.EndChangeCheck())
                {
                    if (m_spContactCountToMatch.intValue < 0)
                        m_spContactCountToMatch.intValue = 0;
                }

                EditorGUI.BeginChangeCheck();
                m_spContactCountToWarp.intValue =
                    EditorGUILayout.IntField("Num Contacts to Warp", m_spContactCountToWarp.intValue);
                if (EditorGUI.EndChangeCheck())
                {
                    if (m_spContactCountToWarp.intValue < 0)
                        m_spContactCountToWarp.intValue = 0;
                }

                m_spExitWithMotion.boolValue = EditorGUILayout.Toggle("Exit With Motion", m_spExitWithMotion.boolValue);
                m_spMatchPose.boolValue = EditorGUILayout.Toggle("Match Pose", m_spMatchPose.boolValue);
                m_spMatchTrajectory.boolValue =
                    EditorGUILayout.Toggle("Match Trajectory", m_spMatchTrajectory.boolValue);
                m_spMatchRequireTags.boolValue =
                    EditorGUILayout.Toggle("Match Require Tags", m_spMatchRequireTags.boolValue);
                m_spFavourTagMethod.intValue = (int)(EFavourTagMethod)EditorGUILayout.EnumPopup(
                    "Favour Tag Method", (EFavourTagMethod)m_spFavourTagMethod.intValue);
                m_spPostEventTrajectoryMode.intValue = (int)(EPostEventTrajectoryMode)EditorGUILayout.EnumPopup(
                    "Post Event Trajectory Mode", (EPostEventTrajectoryMode)m_spPostEventTrajectoryMode.intValue);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.ObjectField(m_spEventNamingModule,
                    typeof(EventNamingModule), new GUIContent("Event Module"));
                if (EditorGUI.EndChangeCheck())
                {
                    eventNameData = m_spEventNamingModule.objectReferenceValue as EventNamingModule;

                    if (eventNameData != null)
                    {
                        (target as MxMEventDefinition).ValidateEventId(eventNameData);
                    }
                }

                if (m_spEventNamingModule.objectReferenceValue == null)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.ObjectField(m_spTargetAnimData, typeof(MxMAnimData),
                        new GUIContent("Ref AnimData (Legacy)",
                            "Use event naming module instead to avoid needing to re-process to see updated event list"));
                    if (EditorGUI.EndChangeCheck())
                    {
                        animData = m_spTargetAnimData.objectReferenceValue as MxMAnimData;

                        if (animData != null)
                        {
                            (target as MxMEventDefinition).ValidateEventId(animData);
                        }
                    }

                    curHeight += 18f;
                }

                curHeight += 18f * 11f;
            }

            curHeight += 30f;
            GUILayout.Space(3f);

            m_timingFoldout = EditorUtil.EditorFunctions.DrawFoldout("Time Warping", curHeight, EditorGUIUtility.currentViewWidth, m_timingFoldout);

            if (m_timingFoldout)
            {

                EditorGUI.BeginChangeCheck();
                m_spMatchTiming.boolValue = EditorGUILayout.Toggle("Match Timing", m_spMatchTiming.boolValue);
                if(EditorGUI.EndChangeCheck())
                {
                    if (m_spMatchTiming.boolValue)
                    {
                        m_spWarpTimeScaling.boolValue = false;
                    }
                }

                m_spExactTimeMatch.boolValue = EditorGUILayout.Toggle("Exact Time Match", m_spExactTimeMatch.boolValue);
                m_spTimingWeight.floatValue = EditorGUILayout.FloatField("Timing Weight", m_spTimingWeight.floatValue);
                m_spTimingWarpType.intValue = (int)(EEventWarpType)EditorGUILayout.EnumPopup("Timing Warp Type", (EEventWarpType)m_spTimingWarpType.intValue);
                
                curHeight += 18f * 3f;
            }

            curHeight += 30f;
            GUILayout.Space(3f);

            m_positionFoldout = EditorUtil.EditorFunctions.DrawFoldout("Position Warping", curHeight, EditorGUIUtility.currentViewWidth, m_positionFoldout);

            if (m_positionFoldout)
            {

                m_spMatchPosition.boolValue = EditorGUILayout.Toggle("Match Position", m_spMatchPosition.boolValue);
                m_spPositionWeight.floatValue = EditorGUILayout.FloatField("Position Weight", m_spPositionWeight.floatValue);
                m_spMotionWarpType.intValue = (int)(EEventWarpType)EditorGUILayout.EnumPopup("Position Warp Type", (EEventWarpType)m_spMotionWarpType.intValue);

                EditorGUI.BeginChangeCheck();
                m_spWarpTimeScaling.boolValue = EditorGUILayout.Toggle("Time Scaling", m_spWarpTimeScaling.boolValue);
                if(EditorGUI.EndChangeCheck())
                {
                    if(m_spWarpTimeScaling.boolValue)
                    {
                        m_spMatchTiming.boolValue = false;
                    }
                }

                if (m_spWarpTimeScaling.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20f);
                    EditorGUILayout.BeginVertical();

                    m_spContactCountToTimeScale.intValue = EditorGUILayout.IntField("Contact Count", m_spContactCountToTimeScale.intValue);
                    m_spMinWarpTimeScale.floatValue = EditorGUILayout.FloatField("Min", m_spMinWarpTimeScale.floatValue);
                    m_spMaxWarpTimeScale.floatValue = EditorGUILayout.FloatField("Max", m_spMaxWarpTimeScale.floatValue);

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();

                    curHeight += 18f * 3f;
                }

                curHeight += 18f * 4f;
            }

            curHeight += 30f;
            GUILayout.Space(3f);

            m_rotationFoldout = EditorUtil.EditorFunctions.DrawFoldout("Rotation Warping", curHeight, EditorGUIUtility.currentViewWidth, m_rotationFoldout);

            if (m_rotationFoldout)
            {
                m_spMatchRotation.boolValue = EditorGUILayout.Toggle("Match Rotation", m_spMatchRotation.boolValue);
                m_spRotationWeight.floatValue = EditorGUILayout.FloatField("Rotation Weight", m_spRotationWeight.floatValue);
                m_spRotationWarpType.intValue = (int)(EEventWarpType)EditorGUILayout.EnumPopup("Rotation Warp Type", (EEventWarpType)m_spRotationWarpType.intValue);

                curHeight += 18f * 3f;
            }

            curHeight += 30f;
            GUILayout.Space(3f);

            serializedObject.ApplyModifiedProperties();
        }

    }//End of class: MxMEventDefinitionInspector
}//End of namespace: MxMEditor
