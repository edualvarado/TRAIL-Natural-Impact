using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using UnityEditor;
using System.IO;
using System;
using MxM;
using EditorUtil;

namespace MxMEditor

{//===========================================================================================
 /**
 *  @brief
 *         
 *********************************************************************************************/
    public class MxMBlendSpaceWindow : IPreviewable
    {
        private static MxMBlendSpace m_data;
        private static MxMBlendSpaceWindow m_inst;

        private static SerializedObject m_soData;
        private static SerializedProperty m_spName;
        private static SerializedProperty m_spClips;
        private static SerializedProperty m_spPositions;
        private static SerializedProperty m_spNormalizeTime;
        private static SerializedProperty m_spTargetPreProcessData;
        private static SerializedProperty m_spTargetAnimModule;
        private static SerializedProperty m_spTargetPrefab;
        private static SerializedProperty m_spMagnitude;
        private static SerializedProperty m_spSmoothing;
        private static SerializedProperty m_spGlobalTags;
        private static SerializedProperty m_spGlobalFavourTags;
        
        private static SerializedProperty m_spScatterSpace;
        private static SerializedProperty m_spScatterSpacing;

        private static string[] m_tagNames;
        private static string[] m_favourTagNames;
        private static int m_tagIndex;
        private static int m_favourTagIndex;

        private static int m_selectId = -1;
        private static bool m_dragging;
        private static Vector2 m_cumulativeDrag;
        private static bool m_snapActive = true;
        private static float m_snapInterval = 0.1f;
        private static Vector2 m_previewPos;
        private static bool m_draggingPreview;
        private static bool m_showClipNames = true;
        //private static float m_previewProgress = 0f;
        private static float m_normalizedClipLength = 0f;

        private int m_queueDeleteIndex = -1;

        //Preview
        private static bool m_previewActive;
        private static bool m_queuePreview;
        private static MxMBlendSpace m_previewBlendSpace = null;
        private static float m_lastPlayIncTime;
        private static List<float> m_blendWeights;

        public static MxMBlendSpaceWindow Inst()
        {
            if (m_inst == null)
            {
                m_inst = new MxMBlendSpaceWindow();
            }

            return m_inst;
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

            m_data = a_data;
            m_data.VerifyData();

            m_soData = new SerializedObject(m_data);

            m_spName = m_soData.FindProperty("m_blendSpaceName");
            m_spClips = m_soData.FindProperty("m_clips");
            m_spPositions = m_soData.FindProperty("m_positions");
            m_spNormalizeTime = m_soData.FindProperty("m_normalizeTime");
            m_spTargetPreProcessData = m_soData.FindProperty("m_targetPreProcessData");
            m_spTargetAnimModule = m_soData.FindProperty("m_targetAnimModule");
            m_spTargetPrefab = m_soData.FindProperty("m_targetPrefab");
            m_spMagnitude = m_soData.FindProperty("m_magnitude");
            m_spSmoothing = m_soData.FindProperty("m_smoothing");
            m_spGlobalTags = m_soData.FindProperty("GlobalTags");
            m_spGlobalFavourTags = m_soData.FindProperty("GlobalFavourTags");

            m_spScatterSpace = m_soData.FindProperty("m_scatterSpace");
            m_spScatterSpacing = m_soData.FindProperty("m_scatterSpacing");

            if (m_spTargetPreProcessData.objectReferenceValue != null)
            {
                var preProcessor = m_spTargetPreProcessData.objectReferenceValue as MxMPreProcessData;
                var animModule = m_spTargetAnimModule.objectReferenceValue as AnimationModule;

                List<string> tagNames = null;

                if (preProcessor != null)
                {
                    tagNames = preProcessor.TagNames;
                }
                else if (animModule != null && animModule.TagNames != null)
                {
                    tagNames = animModule.TagNames;
                }
                else
                {
                    tagNames = new List<string>(Enum.GetNames(typeof(ETags)));
                }

                m_tagNames = new string[33];
                m_tagNames[0] = "None";


                for (int i = 0; i < tagNames.Count; ++i)
                    m_tagNames[i + 1] = tagNames[i];
                

                List<string> favourTagNames = null;

                if (preProcessor != null)
                {
                    favourTagNames = preProcessor.FavourTagNames;
                }
                else if(animModule != null && animModule.FavourTagNames != null)
                {
                    favourTagNames = animModule.FavourTagNames;
                }
                else
                {
                    favourTagNames = new List<string>(Enum.GetNames(typeof(ETags)));
                }

                m_favourTagNames = new string[33];
                m_favourTagNames[0] = "None";


                for (int i = 0; i < favourTagNames.Count; ++i)
                    m_favourTagNames[i + 1] = favourTagNames[i];
                

                m_tagIndex = Array.IndexOf(Enum.GetValues(typeof(ETags)), (ETags)m_spGlobalTags.intValue);
                m_favourTagIndex = Array.IndexOf(Enum.GetValues(typeof(ETags)), (ETags)m_spGlobalFavourTags.intValue);

                GameObject targetPrefab = null;
                if (preProcessor != null)
                {
                    targetPrefab = preProcessor.Prefab;
                }
                else if (animModule != null)
                {
                    targetPrefab = animModule.Prefab;
                }

                if (targetPrefab != null)
                {
                    m_spTargetPrefab.objectReferenceValue = targetPrefab;
                    m_soData.ApplyModifiedPropertiesWithoutUndo();
                }

            }
            else
            {
                m_tagIndex = 0;
                m_favourTagIndex = 0;
            }

            MxMSettings settings = MxMSettings.Instance();
            if (settings != null)
            {
                settings.ActiveBlendSpace = a_data;
            }

            if (MxMTaggingWindow.Exists())
            {
                MxMTaggingWindow.Inst().SetTarget(a_data);
            }
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        public static MxMBlendSpace GetBlendSpace()
        {
            return m_data;
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        [MenuItem("CONTEXT/AnimationClip/Create MxM BlendSpace")]
        public static void MakeFromAnimationClip(MenuCommand a_cmd)
        {
            AnimationClip clip = (AnimationClip)a_cmd.context;

            MxMBlendSpace mxmBlendSpace = ScriptableObject.CreateInstance<MxMBlendSpace>();

            mxmBlendSpace.Initialize(clip);

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if(path == "")
            {
                path = "Assets";
            }
            else if(Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + clip.name + "_MxMBlend.asset");

            AssetDatabase.CreateAsset(mxmBlendSpace, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = mxmBlendSpace;
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        public void OnLostFocus()
        {
            m_dragging = false;
            m_draggingPreview = false;
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        public void OnGUI(Rect a_position)
        {
            if (m_data == null)
            {
                MxMSettings settings = MxMSettings.Instance();
                if (settings != null)
                {
                    m_data = settings.ActiveBlendSpace;

                    if (m_data != null)
                        SetData(m_data);
                }
            }

            if (m_data != null)
            {
                Event evt = Event.current;

                float labelWidth = EditorGUIUtility.labelWidth;

                if (evt.type == EventType.Layout)
                {
                    if (m_queueDeleteIndex >= 0 && m_queueDeleteIndex < m_spClips.arraySize)
                    {
                        if (m_selectId == m_queueDeleteIndex)
                            m_selectId = -1;

                        if (m_spClips.GetArrayElementAtIndex(m_queueDeleteIndex).objectReferenceValue != null)
                            m_spClips.DeleteArrayElementAtIndex(m_queueDeleteIndex);

                        m_spClips.DeleteArrayElementAtIndex(m_queueDeleteIndex);
                        m_spPositions.DeleteArrayElementAtIndex(m_queueDeleteIndex);

                        m_soData.ApplyModifiedPropertiesWithoutUndo();

                        if (m_previewActive)
                        {
                            RemoveClipFromPreview(m_queueDeleteIndex);
                        }
                        
                        m_queueDeleteIndex = -1;
                    }

                    if(m_queuePreview != m_previewActive)
                    {
                        m_queuePreview = m_previewActive;

                        if (m_previewActive)
                            BeginPreview();
                        else
                            EndPreview();
                    }
                }

                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(20f), GUILayout.ExpandWidth(true));
                {
                    EditorGUI.BeginChangeCheck();
                    m_previewActive = GUILayout.Toggle(m_previewActive, "Preview", EditorStyles.toolbarButton, GUILayout.Width(60f));
                    if (EditorGUI.EndChangeCheck())
                    {
                        m_queuePreview = !m_previewActive;
                    }

                    if (m_previewActive && m_queuePreview == m_previewActive && MxMPreviewScene.IsSceneLoaded)
                    {
                        EditorGUIUtility.labelWidth = 20f;

                        EditorGUI.BeginChangeCheck();
                        m_previewPos.x = EditorGUILayout.FloatField("X: ", m_previewPos.x, EditorStyles.toolbarTextField);
                        m_previewPos.y = EditorGUILayout.FloatField("Y: ", m_previewPos.y, EditorStyles.toolbarTextField);
                        if (EditorGUI.EndChangeCheck())
                        {
                            m_previewPos.x = Mathf.Clamp(m_previewPos.x, -1f, 1f);
                            m_previewPos.y = Mathf.Clamp(m_previewPos.y, -1f, 1f);

                            CalculateBlendWeights();
                            ApplyBlendWeights();
                        }
                        EditorGUIUtility.labelWidth = labelWidth;

                        UpdatePreview();
                        MxMAnimConfigWindow.Inst().Repaint();
                    }

                    GUILayout.Space(5f);

                    if (GUILayout.Button(new GUIContent("Open Timeline"), EditorStyles.toolbarButton))
                    {
                        MxMTaggingWindow.ShowWindow();
                    }

                    GUILayout.Space(5f);


                    GUILayout.FlexibleSpace();

                    GUILayout.Space(2f);
                    m_showClipNames = GUILayout.Toggle(m_showClipNames, "Show Clips", EditorStyles.toolbarButton, GUILayout.Width(80f));
                    GUILayout.Space(5f);
                    m_snapActive = GUILayout.Toggle(m_snapActive, "Snap", EditorStyles.toolbarButton, GUILayout.Width(40f));

                    if (m_snapActive)
                    {
                        m_snapInterval = EditorGUILayout.FloatField(m_snapInterval, EditorStyles.toolbarTextField, GUILayout.Width(30f));
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(20f), GUILayout.ExpandWidth(true));
                {
                    EditorGUILayout.LabelField("Name:", GUILayout.Width(40f));
                    EditorGUI.BeginChangeCheck();
                    m_spName.stringValue = EditorGUILayout.TextField(m_spName.stringValue, GUILayout.Width(150f));
                    if (EditorGUI.EndChangeCheck())
                    {
                        m_data.name = m_spName.stringValue;
                        EditorUtility.SetDirty(m_data);
                    }

                    GUILayout.FlexibleSpace();

                    m_spNormalizeTime.boolValue = GUILayout.Toggle(m_spNormalizeTime.boolValue, "Normalize Time",
                    EditorStyles.toolbarButton, GUILayout.Width(90f));

                    EditorGUILayout.LabelField("Type:", GUILayout.Width(40f));
                    m_spScatterSpace.enumValueIndex = (int)(EBlendSpaceType)EditorGUILayout.EnumPopup(
                        (EBlendSpaceType)m_spScatterSpace.enumValueIndex, GUILayout.Width(70f));
                    GUILayout.Space(5f);

                    switch ((EBlendSpaceType)m_spScatterSpace.enumValueIndex)
                    {
                        case EBlendSpaceType.Standard:
                            {
                                EditorGUILayout.LabelField("Magnitude", GUILayout.Width(62f));
                                m_spMagnitude.vector2Value = EditorGUILayout.Vector2Field("", m_spMagnitude.vector2Value, GUILayout.Width(100f));
                                EditorGUILayout.LabelField("Smoothing", GUILayout.Width(65f));
                                m_spSmoothing.vector2Value = EditorGUILayout.Vector2Field("", m_spSmoothing.vector2Value, GUILayout.Width(100f));
                            }
                            break;
                        case EBlendSpaceType.Scatter:
                            {
                                EditorGUILayout.LabelField("Spacing", GUILayout.Width(50f));
                                m_spScatterSpacing.vector2Value = EditorGUILayout.Vector2Field("", m_spScatterSpacing.vector2Value, GUILayout.Width(100f));
                            }
                            break;
                        case EBlendSpaceType.ScatterX:
                            {
                                EditorGUILayout.LabelField("Spacing X", GUILayout.Width(60f));
                                float spacingX = EditorGUILayout.FloatField(m_spScatterSpacing.vector2Value.x, GUILayout.Width(35f));

                                m_spScatterSpacing.vector2Value = new Vector2(spacingX, m_spScatterSpacing.vector2Value.y);
                            }
                            break;
                        case EBlendSpaceType.ScatterY:
                            {
                                EditorGUILayout.LabelField("Spacing Y", GUILayout.Width(60f));
                                float spacingY = EditorGUILayout.FloatField(m_spScatterSpacing.vector2Value.y, GUILayout.Width(35f));

                                m_spScatterSpacing.vector2Value = new Vector2(m_spScatterSpacing.vector2Value.x, spacingY);
                            }
                            break;
                    }
                }
                EditorGUILayout.EndHorizontal();


                GUILayout.FlexibleSpace();

                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(20f), GUILayout.ExpandWidth(true));

                if(m_selectId >= 0 && m_selectId < m_spClips.arraySize)
                {
                    AnimationClip clip = m_spClips.GetArrayElementAtIndex(m_selectId).objectReferenceValue as AnimationClip;
                    SerializedProperty spPosition = m_spPositions.GetArrayElementAtIndex(m_selectId);

                    if (clip != null && spPosition != null)
                    {
                        EditorGUILayout.LabelField(clip.name, GUILayout.Width(GUI.skin.label.CalcSize(new GUIContent(clip.name)).x + 4f));

                        EditorGUI.BeginChangeCheck();
                        spPosition.vector2Value = EditorGUILayout.Vector2Field("", spPosition.vector2Value);
                        if(EditorGUI.EndChangeCheck())
                        {
                            if (m_previewActive && MxMPreviewScene.IsSceneLoaded)
                            {
                                CalculateBlendWeights();
                                ApplyBlendWeights();
                            }
                        }
                    }
                }

                EditorGUILayout.LabelField("Require", GUILayout.Width(50f));
                MxMPreProcessData preProcessData = m_spTargetPreProcessData.objectReferenceValue as MxMPreProcessData;
                AnimationModule animModule = m_spTargetAnimModule.objectReferenceValue as AnimationModule;

                if (preProcessData != null)
                {
                    EditorFunctions.DrawTagFlagFieldWithCustomNames(preProcessData.TagNames.ToArray(), m_spGlobalTags, 100f);
                }
                else if(animModule != null && animModule.TagNames != null)
                {
                    EditorFunctions.DrawTagFlagFieldWithCustomNames(animModule.TagNames.ToArray(), m_spGlobalTags, 100f);
                }
                else
                {
                    m_spGlobalTags.intValue = (int)(ETags)EditorGUILayout.EnumFlagsField((ETags)m_spGlobalTags.intValue);
                }

                EditorGUILayout.LabelField("Favour", GUILayout.Width(45f));

                if (preProcessData != null)
                {
                    EditorFunctions.DrawTagFlagFieldWithCustomNames(preProcessData.FavourTagNames.ToArray(), m_spGlobalFavourTags, 100f);
                }
                else if (animModule != null && animModule.FavourTagNames != null)
                {
                    EditorFunctions.DrawTagFlagFieldWithCustomNames(animModule.FavourTagNames.ToArray(), m_spGlobalFavourTags, 100f);
                }
                else
                {
                    m_spGlobalFavourTags.intValue = (int)(ETags)EditorGUILayout.EnumFlagsField((ETags)m_spGlobalFavourTags.intValue);
                }

                GUILayout.FlexibleSpace();

                if (m_spTargetPreProcessData.objectReferenceValue != null ||
                    m_spTargetAnimModule.objectReferenceValue != null)
                {
                    if (GUILayout.Button(EditorGUIUtility.IconContent("back").image, EditorStyles.toolbarButton))
                        LastBlendSpace();

                    if (GUILayout.Button(EditorGUIUtility.IconContent("forward").image, EditorStyles.toolbarButton))
                        NextBlendSpace();
                }

                bool independentBlendSpace = preProcessData == null && animModule == null;

                EditorGUI.BeginDisabledGroup(!independentBlendSpace);

                EditorGUIUtility.labelWidth = 110f;
                if (preProcessData != null)
                {
                    EditorGUILayout.ObjectField(m_spTargetPreProcessData, new GUIContent("Target PreProcess"));
                }
                else if(animModule != null)
                {
                    EditorGUILayout.ObjectField(m_spTargetAnimModule, new GUIContent("Target Anim Module"));
                }

                EditorGUIUtility.labelWidth = 95f;
                EditorGUILayout.ObjectField(m_spTargetPrefab, new GUIContent("Preview Prefab"));

                EditorGUI.EndDisabledGroup();

                EditorGUIUtility.labelWidth = labelWidth;

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                Rect blendSpaceRect = new Rect(30f, 40f, a_position.width - 60f, a_position.height - 90f);

                GUI.Box(blendSpaceRect, "");

                Rect labelRect = new Rect(blendSpaceRect.x - 18f, blendSpaceRect.y, 18f, 18f);

                GUI.Label(labelRect, "1");
                labelRect.y += blendSpaceRect.height / 2f - 9f;
                GUI.Label(labelRect, "0");
                labelRect.y = blendSpaceRect.y + blendSpaceRect.height - 18f;
                GUI.Label(labelRect, "-1");
                labelRect.y += labelRect.height;
                labelRect.x += labelRect.width;
                GUI.Label(labelRect, "-1");
                labelRect.x += blendSpaceRect.width / 2f - 9f;
                GUI.Label(labelRect, "0");
                labelRect.x = blendSpaceRect.x + blendSpaceRect.width - 18f;
                GUI.Label(labelRect, "1");

                float spacingH = blendSpaceRect.width / 10f;
                float spacingV = blendSpaceRect.height / 10f;

                float top = blendSpaceRect.y;
                float bottom = blendSpaceRect.y + blendSpaceRect.height;
                float left = blendSpaceRect.x;
                float right = blendSpaceRect.x + blendSpaceRect.width;

                Handles.color = Color.grey;
                for (int i = 1; i < 10; ++i)
                {
                    float horizontal = i * spacingH + blendSpaceRect.x;
                    float vertical = i * spacingV + blendSpaceRect.y;

                    Handles.DrawLine(new Vector3(horizontal, top), new Vector3(horizontal, bottom));
                    Handles.DrawLine(new Vector3(left, vertical), new Vector3(right, vertical));
                }

                Handles.color = Color.black;
                Handles.DrawLine(new Vector3(blendSpaceRect.x + blendSpaceRect.width / 2f, top),
                    new Vector3(blendSpaceRect.x + blendSpaceRect.width / 2f, bottom));

                Handles.DrawLine(new Vector3(left, blendSpaceRect.y + blendSpaceRect.height / 2f),
                    new Vector3(right, blendSpaceRect.y + blendSpaceRect.height / 2f));

                Rect animDrawRect = new Rect(0f, 0f, 18f, 18f);
                Vector2 blendSpaceRatio = new Vector2(2f / blendSpaceRect.width, 2f / blendSpaceRect.height);

                Texture blendKey = EditorGUIUtility.IconContent("blendKey").image;
                Texture blendKeySelected = EditorGUIUtility.IconContent("blendKeySelected").image;
#if UNITY_2019_4_OR_NEWER
                Texture previewPointTex = EditorGUIUtility.IconContent("P4_AddedLocal").image;
#else
                Texture previewPointTex = EditorGUIUtility.IconContent("d_P4_AddedLocal").image;
#endif

                Vector2 centerPos = blendSpaceRect.position;
                centerPos.x += blendSpaceRect.width / 2f;
                centerPos.y += blendSpaceRect.height / 2f;

                //Draw Points
                for (int i = 0; i < m_spClips.arraySize; ++i)
                {
                    Vector2 normalizedPos = m_spPositions.GetArrayElementAtIndex(i).vector2Value;
                    normalizedPos.y *= -1f;

                    animDrawRect.position = (normalizedPos / blendSpaceRatio) + centerPos;

                    animDrawRect.size = new Vector2(14f, 14f);

                    if(m_previewActive && MxMPreviewScene.IsSceneLoaded)
                    {
                        float blendWeight = 1f;
                        if (m_blendWeights.Count > i)
                        {
                            blendWeight = m_blendWeights[i];
                        }

                        float size = 9f + 5f * blendWeight;
                        animDrawRect.size = new Vector2(size, size);
                    }
                    else
                    {
                        animDrawRect.size = new Vector2(14f, 14f);
                    }

                    animDrawRect.position -= (animDrawRect.size / 2f);

                    if (m_selectId == i)
                    {
                        GUI.DrawTexture(animDrawRect, blendKeySelected);
                    }
                    else
                    {
                        GUI.DrawTexture(animDrawRect, blendKey);
                    }

                    if(m_showClipNames)
                    {
                        AnimationClip clip = m_spClips.GetArrayElementAtIndex(i).objectReferenceValue as AnimationClip;

                        Vector2 labelSize = GUI.skin.label.CalcSize(new GUIContent(clip.name));

                        Rect clipNameRect = new Rect(animDrawRect.x + (animDrawRect.width / 2f) - labelSize.x / 2f, 
                            animDrawRect.y - labelSize.y, labelSize.x, labelSize.y);

                        GUI.Label(clipNameRect, clip.name);
                    }

                    if (evt.type == EventType.MouseDown)
                    {
                        if (evt.button == 0)
                        {
                            if (animDrawRect.Contains(evt.mousePosition))
                            {
                                m_selectId = i;
                                m_dragging = true;
                                m_cumulativeDrag = m_spPositions.GetArrayElementAtIndex(i).vector2Value;

                                if (evt.clickCount >= 2)
                                {
                                    EditorGUIUtility.PingObject(m_spClips.GetArrayElementAtIndex(i).objectReferenceValue);
                                }

                                evt.Use();
                                MxMAnimConfigWindow.Inst().Repaint();
                            }
                        }
                        else if (evt.button == 1)
                        {
                            if (animDrawRect.Contains(evt.mousePosition))
                            {                             //Create context menu
                                GenericMenu menu = new GenericMenu();

                                menu.AddItem(new GUIContent("Delete"), false, OnDeleteClip, i);
                                menu.ShowAsContext();
                            }
                        }
                    }
                }

                //Draw Preview Point
                if (m_previewActive && MxMPreviewScene.IsSceneLoaded)
                {
                    Vector3 previewDrawPos = m_previewPos;
                    previewDrawPos.y *= -1f;

                    animDrawRect.size = new Vector2(18f, 18f);
                    animDrawRect.position = (previewDrawPos / blendSpaceRatio) + centerPos - (animDrawRect.size / 2f);

                    GUI.DrawTexture(animDrawRect, previewPointTex);
                }

                switch (evt.type)
                {
                    case EventType.MouseDown:
                        {
                            if (evt.button == 0)
                            {
                                if (m_previewActive && blendSpaceRect.Contains(evt.mousePosition) && MxMPreviewScene.IsSceneLoaded)
                                {
                                    Vector2 blendSpacePos = evt.mousePosition - (blendSpaceRect.position + (blendSpaceRect.size / 2f));
                                    m_previewPos = blendSpacePos * blendSpaceRatio;
                                    m_previewPos.y *= -1f;

                                    m_previewPos.x = Mathf.Clamp(m_previewPos.x, -1f, 1f);
                                    m_previewPos.y = Mathf.Clamp(m_previewPos.y, -1f, 1f);

                                    CalculateBlendWeights();
                                    ApplyBlendWeights();

                                    if (m_spNormalizeTime.boolValue)
                                        UpdateNormalizedClipSpeeds();

                                    m_draggingPreview = true;
                                }

                                m_dragging = false;
                                m_selectId = -1;
                                evt.Use();
                            }
                        }
                        break;
                    case EventType.MouseUp:
                        {
                            if (evt.button == 0)
                            {
                                if (m_dragging || m_draggingPreview)
                                {
                                    m_draggingPreview = false;
                                    m_dragging = false;
                                    m_cumulativeDrag = Vector2.zero;
                                    evt.Use();
                                }
                            }
                        }
                        break;
                    case EventType.MouseDrag:
                        {
                            if (m_dragging)
                            {
                                if (m_selectId >= 0 && m_selectId < m_spPositions.arraySize)
                                {
                                    SerializedProperty spPosition = m_spPositions.GetArrayElementAtIndex(m_selectId);
                                    Vector2 moveDelta = evt.delta;
                                    moveDelta.y *= -1f;

                                    if (m_snapActive)
                                    {
                                        m_cumulativeDrag += moveDelta * blendSpaceRatio;

                                        m_cumulativeDrag.x = Mathf.Clamp(m_cumulativeDrag.x, -1f, 1f);
                                        m_cumulativeDrag.y = Mathf.Clamp(m_cumulativeDrag.y, -1f, 1f);

                                        spPosition.vector2Value = m_cumulativeDrag;
                                        SnapClip(m_selectId);
                                    }
                                    else
                                    {
                                        Vector2 newPos = spPosition.vector2Value + moveDelta * blendSpaceRatio;

                                        newPos.x = Mathf.Clamp(newPos.x, -1f, 1f);
                                        newPos.y = Mathf.Clamp(newPos.y, -1f, 1f);
                                        spPosition.vector2Value = newPos;
                                    }

                                    if (m_previewActive && MxMPreviewScene.IsSceneLoaded)
                                    {
                                        CalculateBlendWeights();
                                        ApplyBlendWeights();
                                    }

                                    evt.Use();
                                }
                                else
                                {
                                    m_dragging = false;
                                }
                            }
                            else if (m_previewActive && m_draggingPreview && MxMPreviewScene.IsSceneLoaded)
                            {
                                Vector2 blendSpacePos = evt.mousePosition - (blendSpaceRect.position + (blendSpaceRect.size / 2f));
                                m_previewPos = blendSpacePos * blendSpaceRatio;
                                m_previewPos.y *= -1f;

                                m_previewPos.x = Mathf.Clamp(m_previewPos.x, -1f, 1f);
                                m_previewPos.y = Mathf.Clamp(m_previewPos.y, -1f, 1f);

                                CalculateBlendWeights();
                                ApplyBlendWeights();

                                if (m_snapActive)
                                    SnapPreview();

                                evt.Use();

                            }
                        }
                        break;
                    case EventType.KeyDown:
                        {
                            if (m_selectId >= 0 && m_selectId < m_spClips.arraySize)
                            {
                                if (evt.keyCode == KeyCode.Delete)
                                {
                                    m_queueDeleteIndex = m_selectId;
                                    MxMAnimConfigWindow.Inst().Repaint();
                                    evt.Use();
                                }
                            }
                        }
                        break;
                }

                DragDropAnimations(blendSpaceRect);

                if (m_soData != null)
                {
                    m_soData.ApplyModifiedProperties();
                }
            }
            else
            {
                GUILayout.Space(18f);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("No Blend Space Selected.", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        public void UpdatePreview()
        {
            if (!MxMPreviewScene.IsSceneLoaded)
                return;

            if (m_data == null || m_data.Clips.Count == 0 || !MxMPreviewScene.PlayableGraph.IsValid())
            {
                EndPreview();
                return;
            }

            if(m_previewBlendSpace != m_data)
            {
                m_previewBlendSpace = m_data;
                SetupPreviewGraph();
            }

            //Calculate and Set blending weights here
            float timePassed = (float)EditorApplication.timeSinceStartup - m_lastPlayIncTime;

            MxMPreviewScene.PlayableGraph.Evaluate(timePassed);
            MxMPreviewScene.PreviewObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            SceneView.RepaintAll();

            m_lastPlayIncTime = (float)EditorApplication.timeSinceStartup;

        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        public void BeginPreview()
        {
            List<AnimationClip> clips = m_data.Clips;
            GameObject prefab = m_data.TargetPrefab;

            if (clips == null || clips.Count == 0 || prefab == null)
            {
                m_previewActive = false;
                return;
            }

            if (MxMPreviewScene.BeginPreview(this))
            {
                MxMPreviewScene.SetPreviewObject(prefab);
                m_previewActive = true;
            }
            else
            {
                m_previewActive = false;
            }
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        public void EndPreview()
        {
            MxMPreviewScene.EndPreview();
            m_previewActive = false;
            m_previewBlendSpace = null;

            if (m_blendWeights != null)
                m_blendWeights.Clear();
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        public void EndPreviewLocal()
        {
            if (!m_previewActive)
                return;

            AnimationMixerPlayable mixer = MxMPreviewScene.Mixer;

            if (mixer.IsValid())
            {
                for(int i = 0; i < mixer.GetInputCount(); ++i)
                {
                    var clipPlayable = mixer.GetInput(0);

                    if (clipPlayable.IsValid())
                    {
                        mixer.DisconnectInput(0);
                        clipPlayable.Destroy();
                    }
                }
            }

            m_previewActive = false;
            m_previewBlendSpace = null;
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void SetupPreviewGraph()
        {
            List<AnimationClip> clips = m_data.Clips;

            m_blendWeights = new List<float>(clips.Count + 1);

            AnimationMixerPlayable mixer = MxMPreviewScene.Mixer;
            mixer.SetInputCount(clips.Count);
            
            for (int i = 0; i < clips.Count; ++i)
            {
                m_blendWeights.Add(0f);
            }
            
            m_previewPos = Vector2.zero;
            CalculateBlendWeights();

            for (int i = 0; i < clips.Count; ++i)
            {
                AnimationClip clip = clips[i];

                var clipPlayable = AnimationClipPlayable.Create(MxMPreviewScene.PlayableGraph, clip);
                clipPlayable.SetApplyFootIK(true);

                if (clipPlayable.IsValid())
                {
                    float normalizedClipSpeed = 1f;
                    if(m_spNormalizeTime.boolValue)
                    {
                        normalizedClipSpeed = clip.length / m_normalizedClipLength;
                    }

                    clipPlayable.SetTime(0.0);
                    clipPlayable.SetSpeed(normalizedClipSpeed);

                    var curPlayable = mixer.GetInput(i);
                    if(curPlayable.IsValid())
                    {
                        mixer.DisconnectInput(i);
                        curPlayable.Destroy();
                    }

                    mixer.ConnectInput(i, clipPlayable, 0);
                    mixer.SetInputWeight(i, m_blendWeights[i]);
                }
            }
            
            ApplyBlendWeights();

            m_lastPlayIncTime = (float)EditorApplication.timeSinceStartup;
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void CalculateBlendWeights()
        {
            if(m_blendWeights != null)
            {
                float totalWeight = 0f;
                List<Vector2> positions = m_data.Positions;

                for(int i=0; i < positions.Count; ++i)
                {
                    Vector2 positionI = positions[i];
                    Vector2 iToSample = m_previewPos - positionI;

                    float weight = 1f;

                    for(int j = 0; j < positions.Count; ++j)
                    {
                        if (j == i)
                            continue;

                        Vector2 positionJ = positions[j];
                        Vector2 iToJ = positionJ - positionI;

                        //Calc Weight
                        float lensq_ij = Vector2.Dot(iToJ, iToJ);
                        float newWeight = Vector2.Dot(iToSample, iToJ) / lensq_ij;
                        newWeight = 1f - newWeight;
                        newWeight = Mathf.Clamp01(newWeight);

                        weight = Mathf.Min(weight, newWeight);
                    }

                    m_blendWeights[i] = weight;
                    totalWeight += weight;
                }

                for(int i = 0; i < m_blendWeights.Count; ++i)
                {
                    m_blendWeights[i] = m_blendWeights[i] / totalWeight;
                }
                
                CalculateNormalizedClipLength();
            }
        }
        
        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void CalculateNormalizedClipLength()
        {
            m_normalizedClipLength = 0f;
            for (int i = 0; i < m_previewBlendSpace.Clips.Count; ++i)
            {
                m_normalizedClipLength += m_previewBlendSpace.Clips[i].length * m_blendWeights[i];
            }
        }
        
        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void UpdateNormalizedClipSpeeds()
        {
            for (int i = 0; i < m_previewBlendSpace.Clips.Count; ++i)
            {
                var clipPlayable = (AnimationClipPlayable)MxMPreviewScene.Mixer.GetInput(i);
                clipPlayable.SetSpeed(m_previewBlendSpace.Clips[i].length / m_normalizedClipLength);
            }
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void ApplyBlendWeights()
        {
            if (!MxMPreviewScene.Mixer.IsValid() || m_blendWeights == null)
                return;

            if(MxMPreviewScene.Mixer.GetInputCount() < m_blendWeights.Count)
                MxMPreviewScene.Mixer.SetInputCount(m_blendWeights.Count);

            for(int i=0; i < m_blendWeights.Count; ++i)
            {
                MxMPreviewScene.Mixer.SetInputWeight(i, m_blendWeights[i]);
            }
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void AddClipToPreview(AnimationClip a_clip)
        {
            if(m_previewActive && a_clip != null)
            {
                m_blendWeights.Add(0f);

                AnimationMixerPlayable mixer = MxMPreviewScene.Mixer;

                int inputId = mixer.GetInputCount();
                mixer.SetInputCount(inputId + 1);

                var clipPlayable = AnimationClipPlayable.Create(MxMPreviewScene.PlayableGraph, a_clip);
                clipPlayable.SetApplyFootIK(true);

                mixer.ConnectInput(inputId, clipPlayable, 0);
                
                CalculateBlendWeights();
                
                for(int i = 0; i < mixer.GetInputCount(); ++i)
                {
                    var playable = mixer.GetInput(i);

                    if(playable.IsValid())
                    {
                        float normalizedClipSpeed = 1.0f;
                        if (m_spNormalizeTime.boolValue)
                        {
                            normalizedClipSpeed *= m_previewBlendSpace.Clips[i].length / m_normalizedClipLength;
                        }

                        playable.SetTime(0f);
                        playable.SetSpeed(normalizedClipSpeed);
                    }
                }
                
                ApplyBlendWeights();
            }
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void RemoveClipFromPreview(int a_clipId)
        {
            if (a_clipId < 0 || a_clipId >= m_blendWeights.Count)
                return;
            
            m_blendWeights.RemoveAt(a_clipId);
            
            AnimationMixerPlayable mixer = MxMPreviewScene.Mixer;
            var clipPlayable = mixer.GetInput(a_clipId);
            
            mixer.DisconnectInput(a_clipId);
            clipPlayable.Destroy();

            for (int i = a_clipId + 1; i < m_data.Clips.Count; ++i)
            {
                var shiftPlayable = mixer.GetInput(i);

                mixer.DisconnectInput(i);
                if (shiftPlayable.IsValid())
                {
                    mixer.ConnectInput(i-1, shiftPlayable, 0);
                }
            }
            
            CalculateBlendWeights();
            
            for (int i = 0; i < mixer.GetInputCount(); ++i)
            {
                var playable = mixer.GetInput(i);

                if (playable.IsValid())
                {
                    float normalizedClipSpeed = 1.0f;
                    if (m_spNormalizeTime.boolValue)
                    {
                        normalizedClipSpeed *= m_previewBlendSpace.Clips[i].length / m_normalizedClipLength;
                    }

                    playable.SetTime(0f);
                    playable.SetSpeed(normalizedClipSpeed);
                }
            }

            ApplyBlendWeights();
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void DragDropAnimations(Rect a_dropRect)
        {
            Event evt = Event.current;

            switch(evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    {
                        if (!a_dropRect.Contains(evt.mousePosition))
                            return;

                        if (DragAndDrop.objectReferences[0].GetType() == typeof(AnimationClip))
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                            if (evt.type == EventType.DragPerform)
                            {
                                DragAndDrop.AcceptDrag();

                                UnityEngine.Object obj = DragAndDrop.objectReferences[0];

                                if (obj != null && obj.GetType() == typeof(AnimationClip))
                                {
                                    Vector2 blendSpacePos = evt.mousePosition - (a_dropRect.position + (a_dropRect.size / 2f));

                                    float blendSpaceRatioX = 1f / (a_dropRect.width / 2f);
                                    float blendSpaceRatioY = -1f / (a_dropRect.height / 2f);

                                    blendSpacePos *= new Vector2(blendSpaceRatioX, blendSpaceRatioY);

                                    m_spClips.InsertArrayElementAtIndex(m_spClips.arraySize);
                                    m_spPositions.InsertArrayElementAtIndex(m_spPositions.arraySize);

                                    m_spClips.GetArrayElementAtIndex(m_spClips.arraySize - 1).objectReferenceValue = obj as AnimationClip;
                                    m_spPositions.GetArrayElementAtIndex(m_spClips.arraySize - 1).vector2Value = blendSpacePos;

                                    if (m_snapActive)
                                        SnapClip(m_spPositions.arraySize - 1);

                                    if(m_previewActive)
                                    {
                                        AddClipToPreview(obj as AnimationClip);
                                    }

                                    MxMAnimConfigWindow.Inst().Repaint();
                                }
                            }
                        }
                    }
                    break;
            }
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void SnapClip(int a_clipId)
        {
            if (a_clipId >= 0 && a_clipId < m_spPositions.arraySize)
            {
                SerializedProperty spPosition = m_spPositions.GetArrayElementAtIndex(a_clipId);

                Vector2 position = spPosition.vector2Value;

                float snapX = Mathf.Round(position.x / m_snapInterval);
                float snapY = Mathf.Round(position.y / m_snapInterval);

                spPosition.vector2Value = new Vector2(snapX * m_snapInterval, snapY * m_snapInterval);
            }
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void SnapPreview()
        {
            float snapX = Mathf.Round(m_previewPos.x / m_snapInterval);
            float snapY = Mathf.Round(m_previewPos.y / m_snapInterval);

            m_previewPos = new Vector2(snapX * m_snapInterval, snapY * m_snapInterval);

            if(m_spNormalizeTime.boolValue)
                UpdateNormalizedClipSpeeds();
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void NextBlendSpace()
        {
            MxMPreProcessData preProcessData = m_spTargetPreProcessData.objectReferenceValue as MxMPreProcessData;
            AnimationModule animModule = m_spTargetAnimModule.objectReferenceValue as AnimationModule;

            List<MxMBlendSpace> blendSpaceList = null;


            if (preProcessData != null)
            {
                blendSpaceList = preProcessData.BlendSpaces;
            }
            else if(animModule != null)
            {
                blendSpaceList = animModule.BlendSpaces;
            }

            if (blendSpaceList != null)
            {
                for (int i = 0; i < blendSpaceList.Count; ++i)
                {
                    if (blendSpaceList[i] == m_data)
                    {
                        if (i < blendSpaceList.Count - 1)
                        {
                            blendSpaceList[i + 1].ValidateBaseData();
                            SetData(blendSpaceList[i + 1]);
                        }
                        else
                        {
                            blendSpaceList[0].ValidateBaseData();
                            SetData(blendSpaceList[0]);
                        }

                        break;
                    }
                }
            }
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void LastBlendSpace()
        {
            MxMPreProcessData preProcessData = m_spTargetPreProcessData.objectReferenceValue as MxMPreProcessData;
            AnimationModule animModule = m_spTargetAnimModule.objectReferenceValue as AnimationModule;

            List<MxMBlendSpace> blendSpaceList = null;

            if (preProcessData != null)
            {
                blendSpaceList = preProcessData.BlendSpaces;
            }
            else if(animModule != null)
            {
                blendSpaceList = animModule.BlendSpaces;
            }

            if (blendSpaceList != null)
            {
                for (int i = 0; i < blendSpaceList.Count; ++i)
                {
                    if (blendSpaceList[i] == m_data)
                    {
                        if (i == 0)
                        {
                            blendSpaceList[blendSpaceList.Count - 1].ValidateBaseData();
                            SetData(blendSpaceList[blendSpaceList.Count - 1]);
                        }
                        else
                        {
                            blendSpaceList[i - 1].ValidateBaseData();
                            SetData(blendSpaceList[i - 1]);
                        }

                        break;
                    }
                }
            }
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private bool CheckHover(Rect a_dropRect)
        {
            return a_dropRect.Contains(Event.current.mousePosition);
        }

        //===========================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        public void OnDeleteClip(object a_eventObj)
        {
            int clipId = (int)a_eventObj;

            if (clipId > -1 && clipId < m_spClips.arraySize)
            {
                m_queueDeleteIndex = clipId;

                //m_spClips.DeleteArrayElementAtIndex(clipId);
            }
        }

    }//End of class: MxmBlendSpaceWindow
}//End of namespace: MxMEditor
