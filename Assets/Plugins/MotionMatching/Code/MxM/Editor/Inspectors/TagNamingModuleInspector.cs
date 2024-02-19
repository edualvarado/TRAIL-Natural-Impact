using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MxMEditor
{
    //============================================================================================
    /**
    *  @brief
    *         
    *********************************************************************************************/
    [CustomEditor(typeof(TagNamingModule))]
    public class TagNamingModuleInspector : Editor
    {
        private SerializedProperty m_spTags;
        private SerializedProperty m_spFavourTags;
        private SerializedProperty m_spUserTags;

        private bool m_requireTagsFoldout = true;
        private bool m_favourTagsFoldout = true;
        private bool m_userTagsFoldout = true;

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void OnEnable()
        {
            m_spTags = serializedObject.FindProperty("m_tagNames");
            m_spFavourTags = serializedObject.FindProperty("m_favourTagNames");
            m_spUserTags = serializedObject.FindProperty("m_userTagNames");
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("");
            Rect lastRect = GUILayoutUtility.GetLastRect();

            float curHeight = lastRect.y + 9f;

            curHeight = EditorUtil.EditorFunctions.DrawTitle("MxM Tag Naming Module", curHeight);

            m_requireTagsFoldout = EditorUtil.EditorFunctions.DrawFoldout(
                "Tags", curHeight, EditorGUIUtility.currentViewWidth, m_requireTagsFoldout);

            lastRect = GUILayoutUtility.GetLastRect();
            curHeight = lastRect.y + lastRect.height;

            if (m_requireTagsFoldout)
            {
                SerializedProperty spTag;

                for (int i = 0; i < m_spTags.arraySize; ++i)
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

            m_favourTagsFoldout = EditorUtil.EditorFunctions.DrawFoldout(
                "Favour Tags", curHeight, EditorGUIUtility.currentViewWidth, m_favourTagsFoldout);

            lastRect = GUILayoutUtility.GetLastRect();
            curHeight = lastRect.y + lastRect.height;

            if (m_favourTagsFoldout)
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

            m_userTagsFoldout = EditorUtil.EditorFunctions.DrawFoldout(
                "User Tags", curHeight, EditorGUIUtility.currentViewWidth, m_userTagsFoldout);

            lastRect = GUILayoutUtility.GetLastRect();
            curHeight = lastRect.y + lastRect.height;

            if (m_userTagsFoldout)
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

            curHeight += 23f;

            serializedObject.ApplyModifiedProperties();
        }

    }//End of class: TagNamingModuleInspector
}//End of namespace: MxMEditor