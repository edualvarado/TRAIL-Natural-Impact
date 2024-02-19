using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MxM;

namespace MxMEditor
{
    [CustomEditor(typeof(MxMRootMotionApplicator))]
    public class MxMRootMotionApplicatorInspector : Editor
    {
        private SerializedProperty m_spCharController;
        private SerializedProperty m_spEnableGravity;
        private SerializedProperty m_spRotationOnly;
        private SerializedProperty m_spAxisLock;

        private void OnEnable()
        {
            m_spCharController = serializedObject.FindProperty("m_charController");
            m_spEnableGravity = serializedObject.FindProperty("m_enableGravity");
            m_spRotationOnly = serializedObject.FindProperty("m_rotationOnly");
            m_spAxisLock = serializedObject.FindProperty("m_axisLock");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.ObjectField(m_spCharController, new GUIContent("Controller Wrapper", 
                "A reference to the character controller wrapper to communicate movement to"));

            m_spEnableGravity.boolValue = EditorGUILayout.Toggle(new GUIContent("Enable Gravity",
                "Should the root motion applicator apply gravity to movement?"), m_spEnableGravity.boolValue);

            m_spRotationOnly.boolValue = EditorGUILayout.Toggle(new GUIContent("Rotation Only",
                "If checked, only root rotation will be applied and not movement"), m_spRotationOnly.boolValue);

            Vector3 axisLock = m_spAxisLock.vector3Value;

            bool axisLockX = axisLock.x > 0.5f ? false : true;
            bool axisLockY = axisLock.y > 0.5f ? false : true;
            bool axisLockZ = axisLock.z > 0.5f ? false : true;

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Axis Lock", GUILayout.Width(120f));

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("x", GUILayout.Width(18f));
                axisLockX = EditorGUILayout.Toggle(axisLockX, GUILayout.Width(20f));
                EditorGUILayout.LabelField("y", GUILayout.Width(18f));
                axisLockY = EditorGUILayout.Toggle(axisLockY, GUILayout.Width(20f));
                EditorGUILayout.LabelField("z", GUILayout.Width(18f));
                axisLockZ = EditorGUILayout.Toggle(axisLockZ, GUILayout.Width(20f));
                if (EditorGUI.EndChangeCheck())
                {
                    if (axisLockX) axisLock.x = 0f; else axisLock.x = 1f;
                    if (axisLockY) axisLock.y = 0f; else axisLock.y = 1f;
                    if (axisLockZ) axisLock.z = 0f; else axisLock.z = 1f;

                    m_spAxisLock.vector3Value = axisLock;
                }

                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }


    }//End of class: MxMRootMotionApplicatorInspector
} //End of namespace: MxMEditor