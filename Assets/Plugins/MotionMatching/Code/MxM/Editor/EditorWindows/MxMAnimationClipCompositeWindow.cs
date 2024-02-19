using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MxM;
using System.IO;
using EditorUtil;


namespace MxMEditor
{
    public class MxMAnimationClipCompositeWindow
    {
        //Static Fields
        private static MxMAnimationClipComposite m_data;
        private static MxMAnimationClipCompositeWindow m_inst;

        private static SerializedObject m_soData;
        private static SerializedProperty m_spName;
        private static SerializedProperty m_spPrimaryClip;
        private static SerializedProperty m_spBeforeClips;
        private static SerializedProperty m_spAfterClips;

        private static SerializedProperty m_spLooping;
        private static SerializedProperty m_spIgnoreEdges;
        private static SerializedProperty m_spExtrapolateTrajectory;
        private static SerializedProperty m_spFlattenTrajectory;
        private static SerializedProperty m_spRuntimeSplicing;

        private static SerializedProperty m_spGlobalTags;
        private static SerializedProperty m_spGlobalFavourTags;
        private static SerializedProperty m_spTagTracks;
        private static SerializedProperty m_spEvents;

        private static SerializedProperty m_spTargetPreProcessData;
        private static SerializedProperty m_spTargetAnimModule;
        private static SerializedProperty m_spTargetPrefab;

        private const float s_animSlotSize = 100f;
        private const float s_animSlotSizeSmall = 75f;
        private const float s_animSlotSpacing = 30f;

        private EAnimType m_selectionType;
        private int m_selectionId;

        private Vector2 m_scrollPosition;

        private enum EAnimType
        {
            None,
            Current,
            Future,
            Past
        }

        //Static Functions
        public static MxMAnimationClipCompositeWindow Inst()
        {
            if (m_inst == null)
            {
                m_inst = new MxMAnimationClipCompositeWindow();
            }

            return m_inst;
        }

        public static void SetData(MxMAnimationClipComposite a_data)
        {
            if (a_data != null)
            {
                m_data = a_data;
                m_data.VerifyData();

                m_data.GenerateRootLookupTable();

                m_soData = new SerializedObject(m_data);

                m_spName = m_soData.FindProperty("CompositeName");
                m_spPrimaryClip = m_soData.FindProperty("PrimaryClip");
                m_spBeforeClips = m_soData.FindProperty("BeforeClips");
                m_spAfterClips = m_soData.FindProperty("AfterClips");

                m_spLooping = m_soData.FindProperty("Looping");
                m_spIgnoreEdges = m_soData.FindProperty("IgnoreEdges");
                m_spExtrapolateTrajectory = m_soData.FindProperty("ExtrapolateTrajectory");
                m_spFlattenTrajectory = m_soData.FindProperty("FlattenTrajectory");
                m_spRuntimeSplicing = m_soData.FindProperty("RuntimeSplicing");

                m_spGlobalTags = m_soData.FindProperty("GlobalTags");
                m_spGlobalFavourTags = m_soData.FindProperty("GlobalFavourTags");
                m_spTagTracks = m_soData.FindProperty("TagTracks");
                m_spEvents = m_soData.FindProperty("Events");

                m_spTargetPreProcessData = m_soData.FindProperty("m_targetPreProcessData");
                m_spTargetAnimModule = m_soData.FindProperty("m_targetAnimModule");
                m_spTargetPrefab = m_soData.FindProperty("m_targetPrefab");

                if(m_spTargetPreProcessData.objectReferenceValue != null)
                {
                    if (m_spTargetPrefab.objectReferenceValue == null)
                    {
                        SerializedProperty spTargetPrefab = m_spTargetPreProcessData.FindPropertyRelative("m_targetPrefab");

                        if (spTargetPrefab != null && spTargetPrefab.objectReferenceValue != null)
                        {
                            m_spTargetPrefab.objectReferenceValue = spTargetPrefab.objectReferenceValue;
                            m_soData.ApplyModifiedPropertiesWithoutUndo();
                        }
                    }
                }

                MxMSettings settings = MxMSettings.Instance();
                if(settings != null)
                {
                    settings.ActiveComposite = a_data;
                }

                if(MxMTaggingWindow.Exists())
                {
                    MxMTaggingWindow.Inst().SetTarget(a_data);
                }
            }
        }

        public static MxMAnimationClipComposite GetComposite()
        {
            return m_data;
        }

        [MenuItem("CONTEXT/AnimationClip/Create MxM Composite")]
        public static void MakeFromAnimationClip(MenuCommand a_cmd)
        {
            AnimationClip clip = (AnimationClip)a_cmd.context;

            MxMAnimationClipComposite mxmClip = ScriptableObject.CreateInstance<MxMAnimationClipComposite>();

            mxmClip.SetPrimaryAnim(clip);

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if(path == "")
            {
                path = "Assets";
            }
            else if(Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + clip.name + "_MxMComp.asset");

            AssetDatabase.CreateAsset(mxmClip, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = mxmClip;
        }

        public void OnGUI(Rect a_position)
        {
            if(m_data == null)
            {
                MxMSettings settings = MxMSettings.Instance();
                if (settings != null)
                {
                    m_data = settings.ActiveComposite;

                    if (m_data != null)
                        SetData(m_data);
                }
            }

            if (m_data != null)
            {
                Rect viewRect = new Rect(0f, 18f, a_position.width, a_position.height-36f);
                float requiredWidth = (m_data.AnimAfterClips.Count +  m_data.AnimBeforeClips.Count + 2) * (s_animSlotSizeSmall + s_animSlotSpacing) + (s_animSlotSize + s_animSlotSpacing);

                if (requiredWidth > a_position.width)
                {
                    Rect scrollRect = new Rect((a_position.width - requiredWidth) / 2f, 18f, Mathf.Max(requiredWidth, a_position.width), a_position.height - 36f);
                    m_scrollPosition = GUI.BeginScrollView(viewRect, m_scrollPosition, scrollRect);
                }

                Rect baseAnimRect = new Rect(a_position.width / 2f - s_animSlotSize / 2f, 35f,
                                             //a_position.height / 2f - s_animSlotSize / 2f,
                                             s_animSlotSize, s_animSlotSize);

                ManageSlot(baseAnimRect, m_data.PrimaryClip, "\n\n\nPrimary Anim");

                if (m_data.BeforeClips == null)
                    m_data.BeforeClips = new List<AnimationClip>();

                if (m_data.AfterClips == null)
                    m_data.AfterClips = new List<AnimationClip>();

                if (m_data.PrimaryClip != null)
                {
                    if (!m_spLooping.boolValue)
                    {
                        Rect slotRect = new Rect(a_position.width / 2f + s_animSlotSize / 2f + s_animSlotSpacing,
                                                 35f + (s_animSlotSize / 2.0f) - (s_animSlotSizeSmall / 2f),
                                                 s_animSlotSizeSmall, s_animSlotSizeSmall);

                        for (int i = 0; i < m_data.AfterClips.Count + 1; ++i)
                        {
                            ManageSlot(slotRect, i < m_data.AfterClips.Count ? m_data.AfterClips[i] : null,
                                "\n\nAfter\nAnim", EAnimType.Future, i);

                            slotRect.x += s_animSlotSizeSmall + s_animSlotSpacing;
                        }

                        slotRect = new Rect(a_position.width / 2f - s_animSlotSize / 2f - s_animSlotSpacing - s_animSlotSizeSmall,
                                            35f + (s_animSlotSize / 2.0f) - (s_animSlotSizeSmall / 2f),
                                            s_animSlotSizeSmall, s_animSlotSizeSmall);

                        for (int i = 0; i < m_data.BeforeClips.Count + 1; ++i)
                        {
                            ManageSlot(slotRect, i < m_data.BeforeClips.Count ? m_data.BeforeClips[i] : null,
                                "\n\nBefore\nAnim", EAnimType.Past, i);

                            slotRect.x -= s_animSlotSizeSmall + s_animSlotSpacing;
                        }
                    }

                    Rect settingsRect = new Rect(baseAnimRect.x - s_animSlotSpacing / 2f,
                                                     baseAnimRect.y + baseAnimRect.height + 10f,
                                                     baseAnimRect.width + s_animSlotSpacing,
                                                     baseAnimRect.height * 2f);

                    DrawSettings(settingsRect);
                }

                Event evt = Event.current;

                if (evt.isKey && evt.keyCode == KeyCode.Delete)
                {

                    switch (evt.keyCode)
                    {
                        case KeyCode.Delete:
                            {
                                //Manage delete key
                                switch (m_selectionType)
                                {
                                    case EAnimType.Current:
                                        {
                                            m_spPrimaryClip.objectReferenceValue = null;

                                            m_data.ClearRootLookupTable();
                                        }
                                        break;
                                    case EAnimType.Future:
                                        {
                                            if (m_selectionId < m_spAfterClips.arraySize)
                                            {
                                                if (m_spAfterClips.GetArrayElementAtIndex(m_selectionId) != null)
                                                    m_spAfterClips.DeleteArrayElementAtIndex(m_selectionId);

                                                m_spAfterClips.DeleteArrayElementAtIndex(m_selectionId);
                                            }
                                        }
                                        break;
                                    case EAnimType.Past:
                                        {
                                            if (m_selectionId < m_spBeforeClips.arraySize)
                                            {
                                                if (m_spBeforeClips.GetArrayElementAtIndex(m_selectionId) != null)
                                                    m_spBeforeClips.DeleteArrayElementAtIndex(m_selectionId);

                                                m_spBeforeClips.DeleteArrayElementAtIndex(m_selectionId);
                                            }
                                        }
                                        break;
                                }
                            }
                            break;
                        case KeyCode.RightArrow:
                            {
                                NextComposite();

                            }
                            break;
                        case KeyCode.LeftArrow:
                            {
                                LastComposite();

                            }
                            break;
                    }



                    m_selectionType = EAnimType.None;
                    m_selectionId = 0;

                    MxMAnimConfigWindow.Inst().Repaint();
                }
                if (requiredWidth > a_position.width)
                {
                    GUI.EndScrollView();
                }


                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(20f), GUILayout.ExpandWidth(true));
                if (GUILayout.Button(new GUIContent("Open Timeline"), EditorStyles.toolbarButton))
                {
                    MxMTaggingWindow.ShowWindow();
                }

                if (GUILayout.Button(new GUIContent("Locate Asset"), EditorStyles.toolbarButton))
                {
                    if (m_spTargetPreProcessData.objectReferenceValue != null)
                        EditorGUIUtility.PingObject(m_spTargetPreProcessData.objectReferenceValue);

                    if (m_spTargetAnimModule.objectReferenceValue != null)
                        EditorGUIUtility.PingObject(m_spTargetAnimModule.objectReferenceValue);
                }
                
                GUILayout.Label("Name: ");
                m_spName.stringValue = GUILayout.TextField(m_spName.stringValue, EditorStyles.textField, GUILayout.MinWidth(150f));

                GUILayout.FlexibleSpace();

                if (GUILayout.Button(EditorGUIUtility.IconContent("back").image, EditorStyles.toolbarButton))
                {
                    LastComposite();
                }

                if (GUILayout.Button(EditorGUIUtility.IconContent("forward").image, EditorStyles.toolbarButton))
                {
                    NextComposite();
                }

                GUILayout.Space(5f);

                if (GUILayout.Button(new GUIContent("Locate Animation"), EditorStyles.toolbarButton))
                {
                    switch (m_selectionType)
                    {
                        case EAnimType.None:
                        case EAnimType.Current:
                            {
                                if (m_spPrimaryClip.objectReferenceValue != null)
                                    EditorGUIUtility.PingObject(m_spPrimaryClip.objectReferenceValue);

                            }
                            break;
                        case EAnimType.Future:
                            {
                                if (m_selectionId < m_spAfterClips.arraySize)
                                {
                                    SerializedProperty spCLip = m_spAfterClips.GetArrayElementAtIndex(m_selectionId);

                                    if (spCLip != null && spCLip.objectReferenceValue != null)
                                    {
                                        EditorGUIUtility.PingObject(spCLip.objectReferenceValue);
                                    }
                                }

                            }
                            break;
                        case EAnimType.Past:
                            {
                                if (m_selectionId < m_spBeforeClips.arraySize)
                                {
                                    SerializedProperty spCLip = m_spBeforeClips.GetArrayElementAtIndex(m_selectionId);

                                    if (spCLip != null && spCLip.objectReferenceValue != null)
                                    {
                                        EditorGUIUtility.PingObject(spCLip.objectReferenceValue);
                                    }
                                }
                            }
                            break;
                    }
                }

                EditorGUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();

                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(20f), GUILayout.ExpandWidth(true));
                EditorGUI.BeginDisabledGroup(true);
                if (m_spTargetPreProcessData.objectReferenceValue != null)
                {
                    EditorGUILayout.ObjectField(m_spTargetPreProcessData, GUILayout.Width(300f));
                }
                else
                {
                    EditorGUILayout.ObjectField(m_spTargetAnimModule, GUILayout.Width(300f));
                }
                
                GUILayout.FlexibleSpace();

                EditorGUILayout.ObjectField(m_spTargetPrefab, GUILayout.Width(300f));

                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();


                if (m_soData != null)
                    m_soData.ApplyModifiedProperties();
            }
            else
            {
                GUILayout.Space(18f);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("No Composite Selected.", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
        }

        private void ManageSlot(Rect a_rect, AnimationClip a_clip, string a_defaultName,
            EAnimType a_animType = EAnimType.Current, int a_index = 0)
        {
            if (a_clip != null)
            {
                Texture2D assetPreview = AssetPreview.GetAssetPreview(a_clip);

                if (assetPreview != null)
                    GUI.DrawTexture(a_rect, assetPreview);

                GUI.Box(a_rect, "\n" + a_clip.name);

                if (m_selectionType == a_animType && a_index == m_selectionId)
                {
                    Handles.color = Color.green;
                }
                else
                {
                    Handles.color = Color.cyan;
                }
#if UNITY_2019_4_OR_NEWER
                Texture icon = EditorGUIUtility.IconContent("P4_DeletedLocal").image;
#else
                Texture icon = EditorGUIUtility.IconContent("d_P4_DeletedLocal").image;
#endif
                Rect deleteRect = new Rect(a_rect.x + a_rect.width - 20f, a_rect.y + a_rect.height - 20f, 18f, 18f);

                GUIStyle invisiButton = new GUIStyle(GUI.skin.label);

                if (GUI.Button(deleteRect, icon, invisiButton))
                {
                    if (EditorUtility.DisplayDialog("Delete Clip", "Are you sure you want to delete this clip from the composite?", "Yes", "Cancel"))
                    {
                        if (a_animType == EAnimType.Current)
                        {
                            m_spPrimaryClip.objectReferenceValue = null;
                        }
                        else if (a_animType == EAnimType.Future)
                        {
                            if (a_index < m_spAfterClips.arraySize)
                            {
                                if (m_spAfterClips.GetArrayElementAtIndex(a_index).objectReferenceValue != null)
                                    m_spAfterClips.DeleteArrayElementAtIndex(a_index);

                                m_spAfterClips.DeleteArrayElementAtIndex(a_index);
                            }
                        }
                        else if(a_animType == EAnimType.Past)
                        {
                            if (a_index < m_spBeforeClips.arraySize)
                            {
                                if (m_spBeforeClips.GetArrayElementAtIndex(a_index).objectReferenceValue != null)
                                    m_spBeforeClips.DeleteArrayElementAtIndex(a_index);

                                m_spBeforeClips.DeleteArrayElementAtIndex(a_index);
                            }
                        }
                    }
                }

                if (CheckHover(a_rect))
                {
                    ManageSelection(a_animType, a_index);
                    Handles.color = Color.yellow;

                    MxMAnimConfigWindow.Inst().Repaint();
                }
            }
            else
            {
                GUI.Box(a_rect, a_defaultName);
                Handles.color = Color.red;
            }


            //Draw highlight box
            Handles.DrawLine(new Vector3(a_rect.xMin, a_rect.yMin), new Vector3(a_rect.xMax, a_rect.yMin));
            Handles.DrawLine(new Vector3(a_rect.xMax, a_rect.yMin), new Vector3(a_rect.xMax, a_rect.yMax));
            Handles.DrawLine(new Vector3(a_rect.xMax, a_rect.yMax), new Vector3(a_rect.xMin, a_rect.yMax));
            Handles.DrawLine(new Vector3(a_rect.xMin, a_rect.yMax), new Vector3(a_rect.xMin, a_rect.yMin));

            
            DragDropAnimations(a_rect, a_animType, a_index);
        }

        private void DrawSettings(Rect a_rect)
        {
            GUILayout.BeginArea(a_rect);

            m_spLooping.boolValue = EditorGUILayout.ToggleLeft(new GUIContent(
                "Loop", "Check if this animation loops"), m_spLooping.boolValue);

            if (!m_spLooping.boolValue)
            {
                m_spIgnoreEdges.boolValue = EditorGUILayout.ToggleLeft(new GUIContent(
                    "Ignore Edges", "Clip edges will be marked as DoNotUse"), m_spIgnoreEdges.boolValue);

                if (!m_spIgnoreEdges.boolValue)
                {
                    m_spExtrapolateTrajectory.boolValue = EditorGUILayout.ToggleLeft(new GUIContent(
                        "Extrapolate", "Trajectory will be extrapolated"), m_spExtrapolateTrajectory.boolValue);
                }
            }

            m_spFlattenTrajectory.boolValue = EditorGUILayout.ToggleLeft(new GUIContent(
                "Flatten Trajectory", "Flatten the trejectory so that the 'y' axis component is always zero"),
                m_spFlattenTrajectory.boolValue);

            if (m_spAfterClips.arraySize > 0)
            {
                m_spRuntimeSplicing.boolValue = EditorGUILayout.ToggleLeft(new GUIContent(
                    "Runtime Splicing", "Splice together the primary clip and first after clip at runtime to ensure continuity"),
                    m_spRuntimeSplicing.boolValue);
            }

            MxMPreProcessData preProcessData = m_spTargetPreProcessData.objectReferenceValue as MxMPreProcessData;
            AnimationModule animModule = m_spTargetAnimModule.objectReferenceValue as AnimationModule;


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Require", GUILayout.Width(50f));
            if (preProcessData != null)
            {
                EditorFunctions.DrawTagFlagFieldWithCustomNames(preProcessData.TagNames.ToArray(), m_spGlobalTags, 75f);
            }
            else if (animModule != null && animModule.FavourTagNames != null)
            {
                EditorFunctions.DrawTagFlagFieldWithCustomNames(animModule.TagNames.ToArray(), m_spGlobalTags, 75f);
            }
            else
            {
                m_spGlobalTags.intValue = (int)(ETags)EditorGUILayout.EnumFlagsField((ETags)m_spGlobalTags.intValue);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Favour", GUILayout.Width(50f));
            if (preProcessData != null)
            {
                EditorFunctions.DrawTagFlagFieldWithCustomNames(preProcessData.FavourTagNames.ToArray(), m_spGlobalFavourTags, 75f);
            }
            else if(animModule != null && animModule.FavourTagNames != null)
            {
                EditorFunctions.DrawTagFlagFieldWithCustomNames(animModule.FavourTagNames.ToArray(), m_spGlobalFavourTags, 75f);
            }
            else
            {
                m_spGlobalFavourTags.intValue = (int)(ETags)EditorGUILayout.EnumFlagsField((ETags)m_spGlobalFavourTags.intValue);
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        private void DragDropAnimations(Rect a_dropRect, EAnimType a_type, int a_index)
        {
            Event evt = Event.current;

            switch (evt.type)
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
                                    switch (a_type)
                                    {
                                        case EAnimType.Current:
                                            {
                                                m_spPrimaryClip.objectReferenceValue = obj;

                                                m_data.name = (obj as AnimationClip).name + "_comp";

                                                if ((obj as AnimationClip).isLooping)
                                                    m_spLooping.boolValue = true;

                                                m_selectionType = EAnimType.Current;
                                                m_selectionId = 0;
                                                
                                                m_data.GenerateRootLookupTable();
                                                m_data.ValidateBaseData();
                                            }
                                            break;
                                        case EAnimType.Future:
                                            {
                                                if (a_index == m_spAfterClips.arraySize)
                                                    m_spAfterClips.InsertArrayElementAtIndex(a_index);

                                                m_spAfterClips.GetArrayElementAtIndex(a_index).objectReferenceValue = obj;

                                                m_selectionType = EAnimType.Future;
                                                m_selectionId = a_index;
                                            }
                                            break;
                                        case EAnimType.Past:
                                            {
                                                if (a_index == m_spBeforeClips.arraySize)
                                                    m_spBeforeClips.InsertArrayElementAtIndex(a_index);

                                                m_spBeforeClips.GetArrayElementAtIndex(a_index).objectReferenceValue = obj;

                                                m_selectionType = EAnimType.Past;
                                                m_selectionId = a_index;
                                            }
                                            break;
                                    }
                                    MxMAnimConfigWindow.Inst().Repaint();
                                }
                            }
                        }
                    }
                    break;
            }
        }

        private void NextComposite()
        {
            MxMPreProcessData preProcessData = m_spTargetPreProcessData.objectReferenceValue as MxMPreProcessData;
            AnimationModule animModule = m_spTargetAnimModule.objectReferenceValue as AnimationModule;

            List<MxMAnimationClipComposite> compList = null;

            if (preProcessData != null)
            {
                compList = preProcessData.GetCompositeCategoryList(m_data.CategoryId);
            }
            else if(animModule != null)
            {
                compList = animModule.GetCompositeCategoryList(m_data.CategoryId);
            }

            if (compList != null)
            {
                for (int i = 0; i < compList.Count; ++i)
                {
                    if (compList[i] == m_data)
                    {
                        if (i < compList.Count - 1)
                        {
                            compList[i + 1].ValidateBaseData();
                            SetData(compList[i + 1]);
                        }
                        else
                        {
                            compList[0].ValidateBaseData();
                            SetData(compList[0]);
                        }

                        if (MxMTaggingWindow.Exists())
                            MxMTaggingWindow.Inst().ClipChanged();

                        break;
                    }
                }
            }
        }

        private void LastComposite()
        {
            MxMPreProcessData preProcessData = m_spTargetPreProcessData.objectReferenceValue as MxMPreProcessData;
            AnimationModule animModule = m_spTargetAnimModule.objectReferenceValue as AnimationModule;

            List<MxMAnimationClipComposite> compList = null;

            if (preProcessData != null)
            {
                compList = preProcessData.GetCompositeCategoryList(m_data.CategoryId);
            }
            else if (animModule != null)
            {
                compList = animModule.GetCompositeCategoryList(m_data.CategoryId);
            }

            if (compList != null)
            {
                for (int i = 0; i < compList.Count; ++i)
                {
                    if (compList[i] == m_data)
                    {
                        if (i == 0)
                        {
                            compList[compList.Count - 1].ValidateBaseData();
                            SetData(compList[compList.Count - 1]);
                        }
                        else
                        {
                            compList[i - 1].ValidateBaseData();
                            SetData(compList[i - 1]);
                        }

                        break;
                    }
                }
            }
        }

        private bool CheckHover(Rect a_dropRect)
        {
            return a_dropRect.Contains(Event.current.mousePosition);
        }

        private void ManageSelection(EAnimType a_type, int a_index)
        {
            Event evt = Event.current;
            if (evt.type == EventType.MouseDown && evt.button == 0)
            {
                m_selectionType = a_type;
                m_selectionId = a_index;
            }
        }

    }//End of class: MxMAnimationClipCompositeWindow
}//End of namespace: MxMEditor
