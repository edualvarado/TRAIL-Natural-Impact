using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MxM;
using System.IO;
using EditorUtil;

namespace MxMEditor
{
    public class MxMAnimationIdleSetWindow
    {
        private static MxMAnimationIdleSet m_data;
        private static MxMAnimationIdleSetWindow m_inst;

        private static SerializedObject m_soData;
        private static SerializedProperty m_spPrimaryClip;
        private static SerializedProperty m_spSecondaryClips;
        private static SerializedProperty m_spTransitionClips;
        private static SerializedProperty m_spTags;
        private static SerializedProperty m_spFavourTags;
        private static SerializedProperty m_spMinLoops;
        private static SerializedProperty m_spMaxLoops;
        private static SerializedProperty m_spTraits;

        private static SerializedProperty m_spTargetPreProcessData;
        private static SerializedProperty m_spTargetAnimationModule;

        private const float s_animSlotSize = 100f;
        private const float s_animSlotSizeSmall = 75f;
        private const float s_animSlotSpacing = 30f;

        private EAnimType m_selectionType;
        private int m_selectionId;

        private Vector2 m_scrollPosition;

        private enum EAnimType
        {
            None,
            Primary,
            Secondary,
            Transition
        }

        public static MxMAnimationIdleSetWindow Inst()
        {
            if (m_inst == null)
            {
                m_inst = new MxMAnimationIdleSetWindow();
            }

            return m_inst;
        }

        public static void SetData(MxMAnimationIdleSet a_data)
        {
            if(a_data != null)
            {
                m_data = a_data;

                m_data.VerifyData();

                m_soData = new SerializedObject(m_data);

                m_spPrimaryClip = m_soData.FindProperty("PrimaryClip");
                m_spSecondaryClips = m_soData.FindProperty("SecondaryClips");
                m_spTransitionClips = m_soData.FindProperty("TransitionClips");
                m_spTags = m_soData.FindProperty("Tags");
                m_spFavourTags = m_soData.FindProperty("FavourTags");
                m_spMinLoops = m_soData.FindProperty("MinLoops");
                m_spMaxLoops = m_soData.FindProperty("MaxLoops");
                m_spTraits = m_soData.FindProperty("Traits");

                m_spTargetPreProcessData = m_soData.FindProperty("m_targetPreProcessData");
                m_spTargetAnimationModule = m_soData.FindProperty("m_targetAnimModule");

                MxMSettings settings = MxMSettings.Instance();
                if (settings != null)
                {
                    settings.ActiveIdleSet = a_data;
                }

                if (MxMTaggingWindow.Exists())
                {
                    MxMTaggingWindow.Inst().SetTarget(a_data);
                }
            }
        }

        public static MxMAnimationIdleSet GetIdleSet()
        {
            return m_data;
        }

        [MenuItem("CONTEXT/AnimationClip/Create MxM Idle Set")]
        public static void MakeFromAnimationClip(MenuCommand a_cmd)
        {
            AnimationClip clip = (AnimationClip)a_cmd.context;

            MxMAnimationIdleSet idleSet = ScriptableObject.CreateInstance<MxMAnimationIdleSet>();

            idleSet.SetPrimaryAnim(clip);

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if(path == "")
            {
                path = "Assets";
            }
            else if(Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + clip.name + "_MxMIdleSet.asset");

            AssetDatabase.CreateAsset(idleSet, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = idleSet;
        }

        public void OnGUI(Rect a_position)
        {
            if (m_data == null)
            {
                MxMSettings settings = MxMSettings.Instance();
                if (settings != null)
                {
                    m_data = settings.ActiveIdleSet;

                    if (m_data != null)
                        SetData(m_data);
                }
            }

            if (m_data != null)
            {
                Rect viewRect = new Rect(0f, 0f, a_position.width, a_position.height);
                float requiredWidth = (Mathf.Max(m_data.TransitionClips.Count, m_data.SecondaryClips.Count) + 1) * (s_animSlotSizeSmall + s_animSlotSpacing);

                if (requiredWidth > a_position.width)
                {
                    Rect scrollRect = new Rect((a_position.width - requiredWidth) / 2f, 18f, Mathf.Max(requiredWidth, a_position.width), a_position.height - 18f);
                    m_scrollPosition = GUI.BeginScrollView(viewRect, m_scrollPosition, scrollRect);
                }

                Rect baseAnimRect = new Rect(a_position.width / 2f - s_animSlotSize / 2f,
                                             a_position.height / 2.5f - s_animSlotSize / 2f,
                                             s_animSlotSize, s_animSlotSize);

                ManageSlot(baseAnimRect, m_data.PrimaryClip, "\n\n\nPrimary Anim");

                if (m_data.SecondaryClips == null)
                    m_data.SecondaryClips = new List<AnimationClip>();

                if (m_data.PrimaryClip != null)
                {
                    Rect slotRect = new Rect(a_position.width / 2f - s_animSlotSizeSmall / 2f - (m_data.TransitionClips.Count
                                             * (s_animSlotSizeSmall + s_animSlotSpacing)) / 2f,
                                             baseAnimRect.y  - s_animSlotSizeSmall - s_animSlotSpacing,
                                             s_animSlotSizeSmall, s_animSlotSizeSmall);


                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(Mathf.Max(m_data.TransitionClips.Count, m_data.SecondaryClips.Count) * (s_animSlotSizeSmall + s_animSlotSpacing));
                    EditorGUILayout.EndHorizontal();


                    for(int i = 0; i < m_data.TransitionClips.Count + 1; ++i)
                    {
                        ManageSlot(slotRect, i < m_data.TransitionClips.Count ? m_data.TransitionClips[i] : null,
                            "\n\nTransition\nAnim", EAnimType.Transition, i);

                        slotRect.x += s_animSlotSizeSmall + s_animSlotSpacing;
                    }


                    slotRect = new Rect(a_position.width / 2f - s_animSlotSizeSmall / 2f - (m_data.SecondaryClips.Count
                                             * (s_animSlotSizeSmall + s_animSlotSpacing)) / 2f,
                                             baseAnimRect.y + s_animSlotSize + s_animSlotSpacing,
                                             s_animSlotSizeSmall, s_animSlotSizeSmall);

                    for (int i = 0; i < m_data.SecondaryClips.Count + 1; ++i)
                    {
                        ManageSlot(slotRect, i < m_data.SecondaryClips.Count ? m_data.SecondaryClips[i] : null,
                            "\n\nSecondary\nAnim", EAnimType.Secondary, i);

                        slotRect.x += s_animSlotSizeSmall + s_animSlotSpacing;
                    }

                    Rect settingsRect = new Rect(baseAnimRect.x - s_animSlotSpacing / 2f,
                                             baseAnimRect.y + baseAnimRect.height + s_animSlotSpacing * 2f + s_animSlotSizeSmall,
                                             baseAnimRect.width + s_animSlotSpacing,
                                             baseAnimRect.height * 2f);

                    settingsRect.x -= settingsRect.width / 2f;
                    DrawSettings(settingsRect);
                    settingsRect.x += settingsRect.width + 10f;
                    DrawTraitBox(settingsRect);
                }


                Event evt = Event.current;

                if (evt.isKey && evt.keyCode == KeyCode.Delete)
                {
                    switch (m_selectionType)
                    {
                        case EAnimType.Primary:
                            {
                                m_spPrimaryClip.objectReferenceValue = null;
                            }
                            break;
                        case EAnimType.Secondary:
                            {
                                if (m_selectionId < m_spSecondaryClips.arraySize)
                                {
                                    if(m_spSecondaryClips.GetArrayElementAtIndex(m_selectionId).objectReferenceValue != null)
                                        m_spSecondaryClips.DeleteArrayElementAtIndex(m_selectionId);

                                    m_spSecondaryClips.DeleteArrayElementAtIndex(m_selectionId);
                                }
                            }
                            break;
                        case EAnimType.Transition:
                            {
                                if(m_selectionId < m_spTransitionClips.arraySize)
                                {
                                    if (m_spTransitionClips.GetArrayElementAtIndex(m_selectionId).objectReferenceValue != null)
                                        m_spTransitionClips.DeleteArrayElementAtIndex(m_selectionId);

                                    m_spTransitionClips.DeleteArrayElementAtIndex(m_selectionId);
                                }
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

                GUILayout.Space(3f);

                if (GUILayout.Button(new GUIContent("Locate Asset"), EditorStyles.toolbarButton))
                {
                    if (m_data != null)
                        EditorGUIUtility.PingObject(m_data);
                }

                GUILayout.Space(a_position.width - 190f);

                if (GUILayout.Button(new GUIContent("Locate Animation"), EditorStyles.toolbarButton))
                {
                    if (m_selectionId >= 0)
                    {
                        switch (m_selectionType)
                        {
                            case EAnimType.Primary:
                                {
                                    if (m_spPrimaryClip.objectReferenceValue != null)
                                        EditorGUIUtility.PingObject(m_spPrimaryClip.objectReferenceValue);

                                }
                                break;
                            case EAnimType.Secondary:
                                {
                                    if (m_spSecondaryClips.arraySize > m_selectionId)
                                    {
                                        SerializedProperty spAnim = m_spSecondaryClips.GetArrayElementAtIndex(m_selectionId);

                                        if (spAnim != null && spAnim.objectReferenceValue != null)
                                        {
                                            EditorGUIUtility.PingObject(spAnim.objectReferenceValue);
                                        }
                                        else
                                        {
                                            if (m_spPrimaryClip.objectReferenceValue != null)
                                                EditorGUIUtility.PingObject(m_spPrimaryClip.objectReferenceValue);
                                        }
                                    }
                                }
                                break;
                            case EAnimType.Transition:
                                {
                                    if(m_spTransitionClips.arraySize > m_selectionId)
                                    {
                                        SerializedProperty spAnim = m_spTransitionClips.GetArrayElementAtIndex(m_selectionId);

                                        if(spAnim != null && spAnim.objectReferenceValue != null)
                                        {
                                            EditorGUIUtility.PingObject(spAnim.objectReferenceValue);
                                        }
                                        else
                                        {
                                            if (m_spPrimaryClip.objectReferenceValue != null)
                                                EditorGUIUtility.PingObject(m_spPrimaryClip.objectReferenceValue);
                                        }
                                    }
                                }
                                break;
                        }
                    }
                    else
                    {
                        if (m_spPrimaryClip.objectReferenceValue != null)
                            EditorGUIUtility.PingObject(m_spPrimaryClip.objectReferenceValue);
                    }
                }


                if (m_soData != null)
                    m_soData.ApplyModifiedProperties();
                
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
            else
            {
                GUILayout.Space(18f);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("No Idle Set Selected.", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
        }

        private void ManageSlot(Rect a_rect, AnimationClip a_clip, string a_defaultName,
            EAnimType a_animType = EAnimType.Primary, int a_index = 0)
        {
            if (a_clip != null)
            {
                Texture2D assetPreview = AssetPreview.GetAssetPreview(a_clip);

                if (assetPreview != null)
                    GUI.DrawTexture(a_rect, assetPreview);

                GUI.Box(a_rect, "\n" + a_clip.name);

                if (m_selectionType == a_animType && a_index == m_selectionId)
                    Handles.color = Color.green;
                else
                    Handles.color = Color.cyan;

#if UNITY_2019_4_OR_NEWER
                Texture icon = EditorGUIUtility.IconContent("P4_DeletedLocal").image;
#else
                Texture icon = EditorGUIUtility.IconContent("d_P4_DeletedLocal").image;
#endif
                Rect deleteRect = new Rect(a_rect.x + a_rect.width - 20f, a_rect.y + a_rect.height - 20f, 18f, 18f);

                GUIStyle invisiButton = new GUIStyle(GUI.skin.label);

                if (GUI.Button(deleteRect, icon, invisiButton))
                {
                    if (EditorUtility.DisplayDialog("Delete Clip", "Are you sure you want to delete this clip from the idle set?", "Yes", "Cancel"))
                    {
                        switch(a_animType)
                        {
                            case EAnimType.Primary:
                                {
                                    m_spPrimaryClip.objectReferenceValue = null;
                                }
                                break;
                            case EAnimType.Secondary:
                                {
                                    if (a_index < m_spSecondaryClips.arraySize)
                                    {
                                        if (m_spSecondaryClips.GetArrayElementAtIndex(a_index).objectReferenceValue != null)
                                            m_spSecondaryClips.DeleteArrayElementAtIndex(a_index);

                                        m_spSecondaryClips.DeleteArrayElementAtIndex(a_index);
                                    }
                                }
                                break;
                            case EAnimType.Transition:
                                {
                                    if(a_index < m_spTransitionClips.arraySize)
                                    {
                                        if (m_spTransitionClips.GetArrayElementAtIndex(a_index).objectReferenceValue != null)
                                            m_spTransitionClips.DeleteArrayElementAtIndex(a_index);

                                        m_spTransitionClips.DeleteArrayElementAtIndex(a_index);
                                    }
                                }
                                break;
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
            MxMPreProcessData preProcessData = m_spTargetPreProcessData.objectReferenceValue as MxMPreProcessData;
            AnimationModule animModule = m_spTargetAnimationModule.objectReferenceValue as AnimationModule;

            GUILayout.BeginArea(a_rect);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Require", GUILayout.Width(50f));

            if(preProcessData != null)
            {
                EditorFunctions.DrawTagFlagFieldWithCustomNames(preProcessData.TagNames.ToArray(), m_spTags, 75f);
            }
            else if(animModule != null && animModule.TagNames != null)
            {
                EditorFunctions.DrawTagFlagFieldWithCustomNames(animModule.TagNames.ToArray(), m_spTags, 75f);
            }
            else
            {
                m_spTags.intValue = (int)(ETags)EditorGUILayout.EnumFlagsField((ETags)m_spTags.intValue);
            }
            

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Favour", GUILayout.Width(50f));

            if (preProcessData != null)
            {
                EditorFunctions.DrawTagFlagFieldWithCustomNames(preProcessData.FavourTagNames.ToArray(), m_spFavourTags, 75f);
            }
            else if(animModule != null && animModule.FavourTagNames != null)
            {
                EditorFunctions.DrawTagFlagFieldWithCustomNames(animModule.FavourTagNames.ToArray(), m_spFavourTags, 75f);
            }
            else
            {
                m_spFavourTags.intValue = (int)(ETags)EditorGUILayout.EnumFlagsField((ETags)m_spFavourTags.intValue);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PrefixLabel("Min Loops:");
            m_spMinLoops.intValue = EditorGUILayout.IntField(m_spMinLoops.intValue);
            EditorGUILayout.PrefixLabel("Max Loops:");
            m_spMaxLoops.intValue = EditorGUILayout.IntField(m_spMaxLoops.intValue);

            GUILayout.EndArea();
        }

        private void DrawTraitBox(Rect a_rect)
        {
            MxMPreProcessData preProcessData = m_spTargetPreProcessData.objectReferenceValue as MxMPreProcessData;
            AnimationModule animModule = m_spTargetAnimationModule.objectReferenceValue as AnimationModule;

            GUIStyle invisiButton = new GUIStyle(GUI.skin.label);

#if UNITY_2019_4_OR_NEWER
            Texture deleteicon = EditorGUIUtility.IconContent("P4_DeletedLocal").image;
#else
                Texture deleteicon = EditorGUIUtility.IconContent("d_P4_DeletedLocal").image;
#endif

            GUILayout.BeginArea(a_rect);

            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Traits", GUILayout.Width(30));
                    GUILayout.Space(20f);
                    if(GUILayout.Button("Clear"))
                    {
                        m_spTraits.ClearArray();
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10f);
                EditorGUILayout.BeginVertical();
                {
                    for (int i = 0; i < m_spTraits.arraySize; ++i)
                    {
                        EditorGUILayout.BeginHorizontal();

                        SerializedProperty spTraitName = m_spTraits.GetArrayElementAtIndex(i);
                        spTraitName.stringValue = EditorGUILayout.TextField(spTraitName.stringValue);

                        if(GUILayout.Button(deleteicon, invisiButton))
                        {
                            m_spTraits.DeleteArrayElementAtIndex(i);
                            MxMAnimConfigWindow.Inst().Repaint();
                            --i;
                        }

                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                if (m_spTraits.arraySize < 8)
                {
                    if (GUILayout.Button(new GUIContent("Add Trait", "Adds a new trait to the idle set. " +
                        "Please note you are limited to 32 traits for each AnimData.")))
                    {
                        m_spTraits.InsertArrayElementAtIndex(m_spTraits.arraySize);
                        MxMAnimConfigWindow.Inst().Repaint();
                    }
                }
            }
            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DragDropAnimations(Rect a_dropRect, EAnimType a_type, int a_index)
        {
            Event evt = Event.current;

            switch(evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    {
                        if (!a_dropRect.Contains(evt.mousePosition))
                            return;

                        if(DragAndDrop.objectReferences[0].GetType() == typeof(AnimationClip))
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                            if(evt.type == EventType.DragPerform)
                            {
                                DragAndDrop.AcceptDrag();

                                Object obj = DragAndDrop.objectReferences[0];

                                if(obj != null && obj.GetType() == typeof(AnimationClip))
                                {
                                    switch(a_type)
                                    {
                                        case EAnimType.Primary:
                                            {
                                                m_spPrimaryClip.objectReferenceValue = obj;

                                                m_data.name = (obj as AnimationClip).name + "_idleSet";

                                                m_selectionType = EAnimType.Primary;
                                                m_selectionId = 0;

                                            }
                                            break;
                                        case EAnimType.Secondary:
                                            {
                                                if (a_index == m_spSecondaryClips.arraySize)
                                                    m_spSecondaryClips.InsertArrayElementAtIndex(a_index);

                                                m_spSecondaryClips.GetArrayElementAtIndex(a_index).objectReferenceValue = obj;

                                                m_selectionType = EAnimType.Secondary;
                                                m_selectionId = a_index;
                                            }
                                            break;
                                        case EAnimType.Transition:
                                            {
                                                if (a_index == m_spTransitionClips.arraySize)
                                                    m_spTransitionClips.InsertArrayElementAtIndex(a_index);

                                                m_spTransitionClips.GetArrayElementAtIndex(a_index).objectReferenceValue = obj;

                                                m_selectionType = EAnimType.Transition;
                                                m_selectionId = a_index;
                                            }
                                            break;
                                    }
                                    MxMAnimConfigWindow.Inst().Repaint();
                                }
                            }
                        }


                    } break;
            }
        }

        private bool CheckHover(Rect a_dropRect)
        {
            return a_dropRect.Contains(Event.current.mousePosition);
        }

        private void ManageSelection(EAnimType a_type, int a_index)
        {
            Event evt = Event.current;
            if(evt.type == EventType.MouseDown && evt.button == 0)
            {
                m_selectionType = a_type;
                m_selectionId = a_index;
            }
        }

    }//End of class: MxMAnimationIdleSetWindow
}//End of namespace: MxMEditor
