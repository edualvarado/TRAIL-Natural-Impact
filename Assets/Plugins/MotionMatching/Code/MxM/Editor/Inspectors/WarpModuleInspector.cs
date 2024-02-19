// ============================================================================================
// File: WarpModuleInspector.cs
// 
// Authors:  Kenneth Claassen
// Date:     2020-11-13: Created this file.
// 
//     Contains a part of the 'MxMEditor' namespace for 'Unity Engine'.
// 
// Copyright (c) 2019 - 2020 Kenneth Claassen. All rights reserved.
// ============================================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MxM;

namespace MxMEditor
{
    [CustomEditor(typeof(WarpModule))]
    public class WarpModuleInspector : Editor
    {
        private WarpModule m_data;

        private SerializedProperty m_spAngularWarpType;
        private SerializedProperty m_spAngularWarpMethod;
        private SerializedProperty m_spAngularErrorWarpRate;
        private SerializedProperty m_spDistanceThreshold;
        private SerializedProperty m_spAngleRange;
        private SerializedProperty m_spLongErrorWarpType;
        private SerializedProperty m_spLongWarpSpeedRange;

        //============================================================================================
        /**
        *  @brief Called just before the inspector is to be shown. Initializes serialized properties.
        *         
        *********************************************************************************************/
        public void OnEnable()
        {
            m_data = (WarpModule)target;

            m_spAngularWarpType = serializedObject.FindProperty("AngularErrorWarpType");
            m_spAngularWarpMethod = serializedObject.FindProperty("AngularErrorWarpMethod");
            m_spAngularErrorWarpRate = serializedObject.FindProperty("WarpRate");
            m_spDistanceThreshold = serializedObject.FindProperty("DistanceThreshold");
            m_spAngleRange = serializedObject.FindProperty("AngleRange");
            m_spLongErrorWarpType = serializedObject.FindProperty("LongErrorWarpType");
            m_spLongWarpSpeedRange = serializedObject.FindProperty("LongWarpSpeedRange");
        }

        //============================================================================================
        /**
        *  @brief Draws and manages the inspector GUI
        *         
        *********************************************************************************************/
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("");
            Rect lastRect = GUILayoutUtility.GetLastRect();

            float curHeight = lastRect.y + 9f;

            curHeight = EditorUtil.EditorFunctions.DrawTitle("Warp Module", curHeight);

            EditorUtil.EditorFunctions.DrawFoldout("Warping", curHeight, EditorGUIUtility.currentViewWidth, true);

            m_spAngularWarpType.intValue = (int)(EAngularErrorWarp)EditorGUILayout.EnumPopup(
                "Angular Error Warping", (EAngularErrorWarp)m_spAngularWarpType.intValue);

            if (m_spAngularWarpType.intValue > 0)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15f);
                m_spAngularWarpMethod.intValue = (int)(EAngularErrorWarpMethod)EditorGUILayout.EnumPopup(
                    "Warp Method", (EAngularErrorWarpMethod)m_spAngularWarpMethod.intValue);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15f);
                m_spAngularErrorWarpRate.floatValue = EditorGUILayout.FloatField(new GUIContent("Warp Rate",
                    "How fast the error will be warped for in 'degrees per second'."),
                    m_spAngularErrorWarpRate.floatValue);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15f);
                m_spDistanceThreshold.floatValue = EditorGUILayout.FloatField(new GUIContent("Distance Threshold",
                    "Discrepancy compensation will not activate unless the final trajectory point is greater than this value"),
                     m_spDistanceThreshold.floatValue);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15f);

                m_spAngleRange.vector2Value = EditorGUILayout.Vector2Field(new GUIContent("Angle Range",
                    "The minimum and maximum angle under which angular error warping will be activated (degrees)."),
                     m_spAngleRange.vector2Value);


                EditorGUILayout.EndHorizontal();
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

                    m_spLongWarpSpeedRange.vector2Value = EditorGUILayout.Vector2Field(
                        "Speed Warp Limits", m_spLongWarpSpeedRange.vector2Value);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (m_spLongWarpSpeedRange.vector2Value.x > m_spLongWarpSpeedRange.vector2Value.y)
                        {
                            m_spLongWarpSpeedRange.vector2Value = new Vector2(m_spLongWarpSpeedRange.vector2Value.y,
                                m_spLongWarpSpeedRange.vector2Value.y);
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

            GUILayout.Space(5f);
            curHeight += 5f;

            serializedObject.ApplyModifiedProperties();
        }
    }
}