using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace MxMEditor
{
    [CustomEditor(typeof(EventNamingModule))]
    public class EventNamingModuleInspector : Editor
    {
        private SerializedProperty m_spEvents;

        public void OnEnable()
        {
            m_spEvents = serializedObject.FindProperty("m_eventNames");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("");
            Rect lastRect = GUILayoutUtility.GetLastRect();

            float curHeight = lastRect.y + 9f;

            curHeight = EditorUtil.EditorFunctions.DrawTitle(" MxM Event Naming Module", curHeight);

            EditorUtil.EditorFunctions.DrawFoldout(
                    "Events", curHeight, EditorGUIUtility.currentViewWidth, true);

            lastRect = GUILayoutUtility.GetLastRect();
            curHeight = lastRect.y + lastRect.height;


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
                    continue;
                }

                spEvent.stringValue = EditorGUILayout.TextField(new GUIContent("Event " + (i + 1).ToString()), spEvent.stringValue);
                EditorGUILayout.EndHorizontal();

#if UNITY_2019_3_OR_NEWER
                curHeight += 20f;
#else
                curHeight += 18f;
#endif
            }

            if (GUI.Button(new Rect(EditorGUIUtility.currentViewWidth - 175f, curHeight + 5f, 150f, 18f),
                new GUIContent("New Event Id")))
            {
                m_spEvents.InsertArrayElementAtIndex(m_spEvents.arraySize);
                spEvent = m_spEvents.GetArrayElementAtIndex(m_spEvents.arraySize - 1);
                spEvent.stringValue = "Event " + m_spEvents.arraySize.ToString();
            }
            curHeight += 27f;
            GUILayout.Space(25f);

            serializedObject.ApplyModifiedProperties();
        }

    }//End of class: EventNamingModuleInspector
}//End of namespace: MxMEditor