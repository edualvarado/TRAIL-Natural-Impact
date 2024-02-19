using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MxM;

namespace MxMEditor
{
    public class MxMAnimConfigWindow : EditorWindow
    {
        //Windows
        private static MxMAnimationClipCompositeWindow m_compositeWindow;
        private static MxMAnimationIdleSetWindow m_idleSetWindow;
        private static MxMBlendSpaceWindow m_blendSpaceWindow;

        private static MxMAnimConfigWindow m_inst;

        //Data
        private static MxMAnimationClipComposite m_compositeData;
        private static MxMAnimationIdleSet m_idleSetData;
        private static MxMBlendSpace m_blendSpaceData;

        //Inst
        private static EMxMAnimtype m_curAnimType;
        private static EMxMAnimtype m_nextAnimType;

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        public static bool IsOpen()
        {
            return m_inst != null;
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        public static MxMAnimConfigWindow Inst()
        {
            if(m_inst == null)
            {
                ShowWindow();
            }

            return m_inst;
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        [MenuItem("Window/MxM/Animation Config")]
        public static void ShowWindow()
        {
            m_compositeWindow = MxMAnimationClipCompositeWindow.Inst();
            m_idleSetWindow = MxMAnimationIdleSetWindow.Inst();
            m_blendSpaceWindow = MxMBlendSpaceWindow.Inst();

            System.Type sceneType = System.Type.GetType("UnityEditor.SceneView, UnityEditor.dll");
            System.Type gameType = System.Type.GetType("UnityEditor.GameView, UnityEditor.dll");

            var dockTypes = new System.Type[] { typeof(MxMAnimationIdleSetWindow), typeof(MxMBlendSpaceWindow),
                sceneType, gameType };

            EditorWindow editorWindow = EditorWindow.GetWindow<MxMAnimConfigWindow>("MxM Anim Config",
                true, dockTypes);

            editorWindow.minSize = new Vector2(100f, 50f);
            editorWindow.Show();

            m_inst = (MxMAnimConfigWindow)editorWindow;

            MxMTaggingWindow.ShowWindow();
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        public static void SetData(MxMAnimationClipComposite a_data)
        {
            if (a_data == null)
                return;

            m_compositeData = a_data;
            MxMAnimationClipCompositeWindow.SetData(m_compositeData);
            m_nextAnimType = EMxMAnimtype.Composite;
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        public static void SetData(MxMAnimationIdleSet a_data)
        {
            if (a_data == null)
                return;

            m_idleSetData = a_data;
            MxMAnimationIdleSetWindow.SetData(m_idleSetData);
            m_nextAnimType = EMxMAnimtype.IdleSet;
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        public static void SetData(MxMBlendSpace a_data)
        {
            if (a_data == null)
                return;

            m_blendSpaceData = a_data;
            MxMBlendSpaceWindow.SetData(m_blendSpaceData);
            m_nextAnimType = EMxMAnimtype.BlendSpace;
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void OnFocus()
        {
            switch(m_curAnimType)
            {
                case EMxMAnimtype.Composite: { SetData(m_compositeData); } break;
                case EMxMAnimtype.IdleSet: { SetData(m_idleSetData); } break;
                case EMxMAnimtype.BlendSpace: { SetData(m_blendSpaceData); } break;
            }
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void OnLostFocus()
        {
            switch (m_curAnimType)
            {
                case EMxMAnimtype.Composite: { } break;
                case EMxMAnimtype.IdleSet: {  } break;
                case EMxMAnimtype.BlendSpace: { m_blendSpaceWindow.OnLostFocus();  } break;
            }
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void OnGUI()
        {
            if(Event.current.type != EventType.Repaint)
            {
                if (m_nextAnimType != m_curAnimType)
                {
                    m_curAnimType = m_nextAnimType;
                }
            }
            
            switch(m_curAnimType)
            {
                case EMxMAnimtype.Composite:
                    {
                        if (m_compositeWindow != null)
                            m_compositeWindow.OnGUI(position);
                        else
                            DrawNoAnimMessage();
                    }
                    break;
                case EMxMAnimtype.IdleSet:
                    {
                        if (m_idleSetWindow != null)
                            m_idleSetWindow.OnGUI(position);
                        else
                            DrawNoAnimMessage();
                    }
                    break;
                case EMxMAnimtype.BlendSpace:
                    {
                        if (m_blendSpaceWindow != null)
                            m_blendSpaceWindow.OnGUI(position);
                        else
                            DrawNoAnimMessage();
                    }
                    break;
            }
        }

        private void DrawNoAnimMessage()
        {
            GUILayout.Space(18f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("No Anim Selected", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
    }   
}