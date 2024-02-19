using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MxMEditor
{
    [CustomEditor(typeof(MotionMatchConfigModule))]
    public class ConfigurationModuleInspector : Editor
    {
        private MotionMatchConfigModule m_data;

        private SerializedProperty m_spTargetPrefab;
        private SerializedProperty m_spTrajectoryTimes;
        private SerializedProperty m_spPoseProperties;
        private SerializedProperty m_spUseGenericRig;

        private List<string> m_jointNames;
        private List<int> m_poseJointIndexes;

        public void OnEnable()
        {
            m_data = (MotionMatchConfigModule)target;

            m_spTargetPrefab = serializedObject.FindProperty("m_targetPrefab");
            m_spTrajectoryTimes = serializedObject.FindProperty("m_trajectoryTimes");
            m_spPoseProperties = serializedObject.FindProperty("m_poseJoints");
            m_spUseGenericRig = serializedObject.FindProperty("m_getBonesByName");

            InitializeGenericRigData();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("");
            Rect lastRect = GUILayoutUtility.GetLastRect();

            float curHeight = lastRect.y + 9f;

            curHeight = EditorUtil.EditorFunctions.DrawTitle("MxM Config Module", curHeight);

            EditorUtil.EditorFunctions.DrawFoldout("General", curHeight, EditorGUIUtility.currentViewWidth, true);


            lastRect = GUILayoutUtility.GetLastRect();
            curHeight = lastRect.y + lastRect.height;


            EditorGUI.BeginChangeCheck();
            EditorGUILayout.ObjectField(m_spTargetPrefab, new GUIContent("Target Model", "This is the model that the pre-processing is based on."));
            if (EditorGUI.EndChangeCheck())
            {
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

            curHeight += 18f + 4f;
            GUILayout.Space(3f);


            EditorUtil.EditorFunctions.DrawFoldout("Trajectory Configuration", curHeight,
                 EditorGUIUtility.currentViewWidth, true);

            lastRect = GUILayoutUtility.GetLastRect();
            curHeight = lastRect.y + lastRect.height;


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

            curHeight += 27f;

            EditorUtil.EditorFunctions.DrawFoldout("Pose Configuration", curHeight,
                EditorGUIUtility.currentViewWidth, true);

            lastRect = GUILayoutUtility.GetLastRect();
            curHeight = lastRect.y + lastRect.height;

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

                    boneNameSP.stringValue = m_jointNames[m_poseJointIndexes[i]];
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

            serializedObject.ApplyModifiedProperties();
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

            if (m_data.Prefab != null)
            {
                List<string> tempJointNames = null;
                if (m_spUseGenericRig.boolValue)
                {
                    tempJointNames = GetAllChildPaths(m_data.Prefab.transform, "");

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

        private List<string> GetAllChildPaths(Transform a_transform, string a_startPath)
        {
            List<string> childPathList = new List<string>();
            List<Transform> childTFormList = new List<Transform>();

            if (a_transform != null)
            {
                int childCount = a_transform.childCount;

                for (int i = 0; i < childCount; ++i)
                {
                    Transform child = a_transform.GetChild(i);

                    childPathList.Add(a_startPath + "/" + child.name);
                    childTFormList.Add(child);
                }

                for (int i = 0; i < childTFormList.Count; ++i)
                {
                    childPathList.AddRange(GetAllChildPaths(childTFormList[i], childPathList[i]));
                }
            }

            return childPathList;
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

    }//End of class: ConfigurationModuleInspector
}//End of namespace: MxMEditor
