using System;
using UnityEngine;
using UnityEditor;
using MxM;

namespace EditorUtil
{

    public static class EditorFunctions
    {
        public static Texture2D HighlightTex;
        public static Texture2D SelectTex;
        public static Texture2D TimelineTexActive;
        public static Texture2D TimelineTexInActive;
        public static Texture2D TimelineTexEvent;

        private  static Texture2D m_foldoutTexOpen;

        //============================================================================================
        /**
         *  @brief 
        *         
        *********************************************************************************************/
        static EditorFunctions()
        {
            HighlightTex = MakeTex(1, 1, new Color(1f, 1f, 0f, 0.3f));

            if (EditorGUIUtility.isProSkin)
                m_foldoutTexOpen = MakeTex(1, 1, new Color(0.25f, 0.25f, 0.25f, 1.0f));
            else
                m_foldoutTexOpen = MakeTex(1, 1, new Color(0.6f, 0.6f, 0.6f, 1.0f));
        }

        //============================================================================================
        /**
         *  @brief 
        *         
        *********************************************************************************************/
        public static Texture2D GetHighlightTex()
        {
            if(HighlightTex == null)
                HighlightTex = MakeTex(1, 1, new Color(1f, 0.5f, 0f, 0.3f));

            return HighlightTex;
        }

        //============================================================================================
        /**
         *  @brief 
        *         
        *********************************************************************************************/
        public static Texture2D GetTimelineTexActive()
        {
            if (TimelineTexActive == null)
                TimelineTexActive = MakeTex(1, 1, new Color(0.825f, 0.825f, 0.825f, 1f));

            return TimelineTexActive;
        }

        //============================================================================================
        /**
         *  @brief 
        *         
        *********************************************************************************************/
        public static Texture2D GetTimelineTexEvent()
        {
            if (TimelineTexEvent == null)
                TimelineTexEvent = MakeTex(1, 1, new Color(0.4f, 0.4f, 0.4f, 1f));

            return TimelineTexEvent;
        }

        //============================================================================================
        /**
         *  @brief 
        *         
        *********************************************************************************************/
        public static Texture2D GetTimelineTexInActive()
        {
            if (TimelineTexInActive == null)
                TimelineTexInActive = MakeTex(1, 1, new Color(0.725f, 0.725f, 0.725f, 1f));

            return TimelineTexInActive;
        }

        //============================================================================================
        /**
         *  @brief Makes a 2D texture of a single color.
        *         
        *********************************************************************************************/
        public static Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public static float DrawTitle(string _title, float _height)
        {
            GUIStyle titleTxtStyle = new GUIStyle(GUI.skin.label);
            titleTxtStyle.fontSize = 22;
            titleTxtStyle.fontStyle = FontStyle.Bold;

            float txtWidth = titleTxtStyle.CalcSize(new GUIContent(_title)).x;

            EditorGUI.LabelField(new Rect((EditorGUIUtility.currentViewWidth - txtWidth) / 2f, _height, txtWidth, 32f),
                                 new GUIContent(_title), titleTxtStyle);

            GUILayout.Space(37f);
            _height += 46f;

            return _height;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public static void DrawSelectionBox(Rect _rect, int _borderThickness = 5)
        {
            if (SelectTex == null)
                SelectTex = MakeTex(2, 2, new Color(0f, 0.3f, 0.7f, 0.35f));

            GUI.DrawTexture(_rect, SelectTex, ScaleMode.StretchToFill);

            Handles.BeginGUI();

            Handles.color = new Color(0f, 0f, 1f);

            Vector3[] p = new Vector3[5];

            p[0] = new Vector3(_rect.position.x - Mathf.Floor(_borderThickness / 3), _rect.position.y, 0f);
            p[1] = new Vector3(_rect.position.x + _rect.size.x, _rect.position.y, 0f);
            p[2] = new Vector3(_rect.position.x + _rect.size.x, _rect.position.y + _rect.size.y, 0f);
            p[3] = new Vector3(_rect.position.x, _rect.position.y + _rect.size.y, 0f);
            p[4] = new Vector3(_rect.position.x, _rect.position.y - Mathf.Floor(_borderThickness / 3), 0f);

            Handles.DrawAAPolyLine(_borderThickness, p);

            Handles.EndGUI();
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public static bool DrawFoldout(string _title, float _curHeight, float _width, bool _foldoutBool, int _indent = 0, bool _thin=false)
        {
            GUIStyle myBoxStyle = new GUIStyle(GUI.skin.box);
            GUIStyle foldoutStyle = new GUIStyle(GUI.skin.label);

            _width = _width - (_indent * 10f);

            Rect lastRect = GUILayoutUtility.GetLastRect();
            Rect boxRect;



#if UNITY_2019_3_OR_NEWER
            float labelY = lastRect.y + lastRect.height + 3f;
#else
            float labelY = lastRect.y + lastRect.height + 1f;
#endif

            if (!_thin)
            {
                boxRect = new Rect(_indent * 10f, labelY, _width, 22f);
                GUI.Box(boxRect, "", myBoxStyle);
                foldoutStyle.fontSize = 14;
                foldoutStyle.fixedHeight = 22f;
            }
            else
            {
                boxRect = new Rect(_indent * 10f, labelY, _width, 18f);
                GUI.Box(boxRect, "", myBoxStyle);
                foldoutStyle.fontSize = 12;
                foldoutStyle.fixedHeight = 20f;
            }

            if (_foldoutBool)
            {
                myBoxStyle.normal.background = m_foldoutTexOpen;

                if(!_thin)
                    GUI.Box(new Rect(_indent * 10f, labelY + 1, _width, 20f), "", myBoxStyle);
                else
                    GUI.Box(new Rect(_indent * 10f, labelY + 1, _width, 16f), "", myBoxStyle);
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(_indent * 10f);

            bool ret = _foldoutBool;
            if(GUI.Button(boxRect, "", GUI.skin.label))
            {
                ret = !_foldoutBool;
            }

            EditorGUILayout.LabelField(_title, foldoutStyle);
            foldoutStyle.fixedHeight = 18f;

            EditorGUILayout.EndHorizontal();


            if (!_thin)
                GUILayout.Space(10);
            else
                GUILayout.Space(6);

            return ret;
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        public static void DisplayInstructionText(string _text)
        {
            GUIStyle instructionStyle = new GUIStyle(GUI.skin.label);
            instructionStyle.fontSize = 11;
            instructionStyle.fontStyle = FontStyle.Bold;
            instructionStyle.normal.textColor = Color.red;

            Vector2 instructSize = instructionStyle.CalcSize(new GUIContent(_text));

            Rect messageRect = new Rect(5f, 5f, instructSize.x + 10f, instructSize.y + 10f);
            GUI.Box(messageRect, "");

            GUILayout.BeginArea(new Rect(messageRect.x + 5f, messageRect.y + 5f, messageRect.width - 10f, messageRect.height - 10f));
            EditorGUILayout.LabelField(_text, instructionStyle);
            GUILayout.EndArea();
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        public static void DisplaySuccessText(string _text)
        {
            GUIStyle successStyle = new GUIStyle(GUI.skin.label);
            successStyle.fontSize = 11;
            successStyle.fontStyle = FontStyle.Bold;
            successStyle.normal.textColor = Color.blue;

            Vector2 successSize = successStyle.CalcSize(new GUIContent(_text));

            Camera cam = SceneView.currentDrawingSceneView.camera;

            Rect messageRect = new Rect(cam.pixelWidth - successSize.x - 15f, cam.pixelHeight - 35f,
                                        successSize.x + 10f, successSize.y + 10f);

            GUI.Box(messageRect, "");

            GUILayout.BeginArea(new Rect(messageRect.x + 5f, messageRect.y + 5f, messageRect.width - 10f, messageRect.height - 10f));
            EditorGUILayout.LabelField(_text, successStyle);
            GUILayout.EndArea();
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        public static Rect GetRectInRect(Rect _innerRect, Rect _outerRect)
        {
            return new Rect(_outerRect.x + _innerRect.x, _outerRect.y + _innerRect.y, _innerRect.width, _innerRect.height);
        }

        //============================================================================================
        /**
         *  @brief 
        *         
        *********************************************************************************************/
        public static void DrawTagFlagFieldWithCustomNames(string[] a_customNames, SerializedProperty a_spTags, float a_width)
        {
            //Figure out the name fo the enum
            ETags enumValue = (ETags)a_spTags.intValue;

            string enumName = "Mixed";

            if(enumValue == 0)
            {
                enumName = "None";
            }
            else if ((enumValue & (enumValue - 1)) == 0)
            {
                int enumIndex =  Array.IndexOf(Enum.GetValues(typeof(ETags)), (ETags)a_spTags.intValue);
                enumName = a_customNames[enumIndex - 1];
            }

            if (GUILayout.Button(enumName, EditorStyles.popup, GUILayout.Width(a_width)))
            {
                GenericMenu menu = new GenericMenu();

                menu.AddItem(new GUIContent("None"), enumValue == ETags.None ? true : false, OnNoneSelected, a_spTags);
                menu.AddItem(new GUIContent("Everything"), false, OnEverythingSelected, a_spTags);

                int currentTags = a_spTags.intValue;

                for (int i = 0; i < a_customNames.Length; ++i)
                {
                    bool isSelected = ((int)currentTags & (1 << i)) == (1 << i);
                    EnumFlagIndexPair enumFlagIndexPair = new EnumFlagIndexPair(i, a_spTags, isSelected);

                    menu.AddItem(new GUIContent(a_customNames[i]), isSelected, OnFlagSelected, enumFlagIndexPair);
                }

                menu.ShowAsContext();
            }
        }

        //============================================================================================
        /**
         *  @brief 
        *         
        *********************************************************************************************/
        private static void OnNoneSelected(object a_context)
        {
            var spTags = a_context as SerializedProperty;
            spTags.intValue = 0;

            ////spTags.serializedObject.ApplyModifiedProperties();
        }

        //============================================================================================
        /**
         *  @brief 
        *         
        *********************************************************************************************/
        private static void OnEverythingSelected(object a_context)
        {
            var spTags = a_context as SerializedProperty;
            spTags.intValue = -1;

            spTags.serializedObject.ApplyModifiedProperties();
        }

        //============================================================================================
        /**
         *  @brief 
        *         
        *********************************************************************************************/
        private static void OnFlagSelected(object a_context)
        {
            var flagIndexPair = a_context as EnumFlagIndexPair;

            if (flagIndexPair == null)
                return;

            ETags currentTags = (ETags)flagIndexPair.SpTags.intValue;
            ETags chosenTag = (ETags)(1 << flagIndexPair.EnumIndex);

            if ((currentTags & chosenTag) == chosenTag)
            {//Its already tagged, untag it
                currentTags = currentTags & (~chosenTag);
            }
            else
            { //It's not already tagged, tag it
                currentTags = currentTags | chosenTag;
            }

            flagIndexPair.SpTags.intValue = (int)currentTags;

            flagIndexPair.SpTags.serializedObject.ApplyModifiedProperties();
        }

        //============================================================================================
        /**
         *  @brief 
        *         
        *********************************************************************************************/
        private class EnumFlagIndexPair
        {
            public int EnumIndex;
            public SerializedProperty SpTags;
            public bool IsSelected;

            public EnumFlagIndexPair(int a_enumIndex, SerializedProperty a_spTags, bool a_isSelected)
            {
                EnumIndex = a_enumIndex;
                SpTags = a_spTags;
                IsSelected = a_isSelected;
            }
        }


    }//End of class: EditorFunctions
}//End of namespace: EditorUtil
