// ============================================================================================
// File: AnimationModuleInspector.cs
// 
// Authors:  Kenneth Claassen
// Date:     2020-07-05: Created this file.
// 
//     Contains a part of the 'MxMEditor' namespace for 'Unity Engine'.
// 
// Copyright (c) 2019 - 2020 Kenneth Claassen. All rights reserved.
// ============================================================================================
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;
using MxM;

namespace MxMEditor
{
    [CustomEditor(typeof(AnimationModule))]
    public class AnimationModuleInspector : Editor
    {
        private AnimationModule m_data;

        private SerializedProperty m_spHideSubAssets;
        private SerializedProperty m_spCompositeCategories;
        private SerializedProperty m_spAnimIdleSets;
        private SerializedProperty m_spBlendSpaces;

        private SerializedProperty m_spOverrideConfigModule;
        private SerializedProperty m_spOverrideTagModule;
        private SerializedProperty m_spOverrideEventModule;

        private SerializedProperty m_spModuleFoldout;
        private SerializedProperty m_spCompositeFoldout;
        private SerializedProperty m_spIdleSetFoldout;
        private SerializedProperty m_spBlendSpaceFoldout;

        private List<ReorderableList> m_compositeReorderableLists;
        private ReorderableList m_idleSetReorderableList;
        private ReorderableList m_blendSpaceReorderableList;

        
        private int m_currentCompositeCategory;
        private int m_queueDeleteCompositeCategory = -1;
        private int m_queueShiftCompositeCategoryUp = -1;
        private int m_queueShiftCompositeCategoryDown = -1;

        //============================================================================================
        /**
        *  @brief Called just before the inspector is to be shown. Initializes serialized properties.
        *         
        *********************************************************************************************/
        public void OnEnable()
        {
            m_data = (AnimationModule)target;

            m_spCompositeCategories = serializedObject.FindProperty("m_compositeCategories");
            m_spAnimIdleSets = serializedObject.FindProperty("m_animIdleSets");
            m_spBlendSpaces = serializedObject.FindProperty("m_blendSpaces");
            m_spHideSubAssets = serializedObject.FindProperty("m_hideSubAssets");

            m_spOverrideConfigModule = serializedObject.FindProperty("m_overrideConfigModule");
            m_spOverrideTagModule = serializedObject.FindProperty("m_overrideTagModule");
            m_spOverrideEventModule = serializedObject.FindProperty("m_overrideEventModule");

            m_spModuleFoldout = serializedObject.FindProperty("m_moduleFoldout");
            m_spCompositeFoldout = serializedObject.FindProperty("m_compositeFoldout");
            m_spIdleSetFoldout = serializedObject.FindProperty("m_idleSetFoldout");
            m_spBlendSpaceFoldout = serializedObject.FindProperty("m_blendSpaceFoldout");


            SetupReorderableLists();
            UpdateTargetModelForAnimData();

            m_data.ValidateData();
        }

        //============================================================================================
        /**
        *  @brief Draws and manages the inspector GUI
        *         
        *********************************************************************************************/
        public override void OnInspectorGUI()
        {
            ManageQueuedActions();

            EditorGUILayout.LabelField("");
            Rect lastRect = GUILayoutUtility.GetLastRect();

            float curHeight = lastRect.y + 9f;

            curHeight = EditorUtil.EditorFunctions.DrawTitle("MxM Animation Module", curHeight);

            m_spModuleFoldout.boolValue = EditorUtil.EditorFunctions.DrawFoldout(
                "Modules", curHeight, EditorGUIUtility.currentViewWidth, m_spModuleFoldout.boolValue);

            lastRect = GUILayoutUtility.GetLastRect();
            curHeight = lastRect.y + lastRect.height;

            if (m_spModuleFoldout.boolValue)
            {
                EditorGUILayout.ObjectField(m_spOverrideConfigModule,
                    typeof(MotionMatchConfigModule), new GUIContent("Config Module"));

                EditorGUILayout.ObjectField(m_spOverrideTagModule, 
                    typeof(TagNamingModule), new GUIContent("Tag Module"));

                EditorGUILayout.ObjectField(m_spOverrideEventModule, 
                    typeof(EventNamingModule), new GUIContent("Event Module"));

                GUILayout.Space(5f);

                GUIStyle boldRedLabel = new GUIStyle(EditorStyles.wordWrappedLabel);
                boldRedLabel.fontStyle = FontStyle.Bold;
                boldRedLabel.normal.textColor = Color.red;

                EditorGUILayout.HelpBox("These modules are for reference only. The modules in " +
                    "the MxMPreProcessor will be used for pre-processing", MessageType.Info);

                GUILayout.Space(5f);
            }

            EditorUtil.EditorFunctions.DrawFoldout("Animation Data", curHeight, 
                EditorGUIUtility.currentViewWidth, true);


            lastRect = GUILayoutUtility.GetLastRect();
            curHeight = lastRect.y + lastRect.height;

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            m_spHideSubAssets.boolValue = EditorGUILayout.Toggle("Hide Sub Assets", m_spHideSubAssets.boolValue);
            if (EditorGUI.EndChangeCheck())
            {
                HideFlags hideFlag = HideFlags.None;

                if (m_spHideSubAssets.boolValue)
                {
                    hideFlag = HideFlags.HideInHierarchy;
                }

                foreach (CompositeCategory compCategory in m_data.CompositeCategories)
                {
                    foreach (MxMAnimationClipComposite composite in compCategory.Composites)
                    {
                        if (composite != null)
                        {
                            composite.hideFlags = hideFlag;
                            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(composite));
                        }
                    }
                }

                foreach (MxMAnimationIdleSet idleSet in m_data.AnimationIdleSets)
                {
                    idleSet.hideFlags = hideFlag;
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(idleSet));
                }

                foreach (MxMBlendSpace blendSpace in m_data.BlendSpaces)
                {
                    blendSpace.hideFlags = hideFlag;
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(blendSpace));
                }
            }

            GUILayout.FlexibleSpace();

            Texture cogIcon = EditorGUIUtility.IconContent("_Popup").image;
            GUIStyle invisiButton = new GUIStyle(GUI.skin.label);
            if(GUILayout.Button(cogIcon, invisiButton))
            {
                AnimModuleSettingsWindow.SetData(serializedObject, m_data);
                AnimModuleSettingsWindow.ShowWindow();
            }

            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(5f);
            curHeight += 5f;

            //curHeight += 2f;
            m_spCompositeFoldout.boolValue = EditorUtil.EditorFunctions.DrawFoldout(
                "Composites", curHeight, EditorGUIUtility.currentViewWidth, m_spCompositeFoldout.boolValue, 1, true);

            lastRect = GUILayoutUtility.GetLastRect();
            curHeight = lastRect.y + lastRect.height;

            Rect dropRect;
            if (m_spCompositeFoldout.boolValue)
            {
                for (int i = 0; i < m_spCompositeCategories.arraySize; ++i)
                {
                    m_currentCompositeCategory = i;

                    m_compositeReorderableLists[i].DoLayoutList();

                    GUILayout.Space(4f);

                    lastRect = GUILayoutUtility.GetLastRect();
                    curHeight = lastRect.y + lastRect.height;

                    //Draw Animation Composites

#if UNITY_2019_3_OR_NEWER
                    dropRect = new Rect(18f, curHeight - 20f, EditorGUIUtility.currentViewWidth - 107f, 20f);
#else
                    dropRect = new Rect(15f, curHeight - 18f, EditorGUIUtility.currentViewWidth - 95f, 20f);
#endif
                    GUI.Box(dropRect, new GUIContent("Drag Anim or Composite Here"));
                    DragDropComposites(dropRect, i);

                    GUILayout.Space(10f);
                    lastRect = GUILayoutUtility.GetLastRect();
                    curHeight = lastRect.y + lastRect.height;
                }

                lastRect = GUILayoutUtility.GetLastRect();
                curHeight = lastRect.y + lastRect.height;

                if (GUI.Button(new Rect(EditorGUIUtility.currentViewWidth
                    - 175f, curHeight, 150f, 18f), "Add New Category"))
                {
                    m_spCompositeCategories.InsertArrayElementAtIndex(m_spCompositeCategories.arraySize);
                    SerializedProperty spCategory = m_spCompositeCategories.GetArrayElementAtIndex(m_spCompositeCategories.arraySize - 1);

                    SerializedProperty spCategoryName = spCategory.FindPropertyRelative("CatagoryName");
                    SerializedProperty spCompositeList = spCategory.FindPropertyRelative("Composites");
                    SerializedProperty spIgnoreEdges = spCategory.FindPropertyRelative("IgnoreEdges_default");
                    SerializedProperty spExtrapolate = spCategory.FindPropertyRelative("Extrapolate_default");
                    SerializedProperty spFlattenTrajectory = spCategory.FindPropertyRelative("FlattenTrajectory_default");
                    SerializedProperty spRuntimeSplicing = spCategory.FindPropertyRelative("RuntimeSplicing_default");
                    SerializedProperty spRequireTags_default = spCategory.FindPropertyRelative("RequireTags_default");
                    SerializedProperty spFavourTags_default = spCategory.FindPropertyRelative("FavourTags_default");

                    AnimModuleDefaults defaultSettings = m_data.DefaultSettings;

                    spCategoryName.stringValue = "New Category";
                    spCompositeList.ClearArray();
                    spIgnoreEdges.boolValue = defaultSettings.IgnoreEdges;
                    spExtrapolate.boolValue = defaultSettings.Extrapolate;
                    spFlattenTrajectory.boolValue = defaultSettings.FlattenTrajectory;
                    spRuntimeSplicing.boolValue = defaultSettings.RuntimeSplicing;
                    spRequireTags_default.intValue = (int)defaultSettings.RequireTags;
                    spFavourTags_default.intValue = (int)defaultSettings.FavourTags;

                    SetupReorderableLists();
                }

                GUILayout.Space(25f);
            }

            m_spIdleSetFoldout.boolValue = EditorUtil.EditorFunctions.DrawFoldout(
                "Idle Sets", curHeight, EditorGUIUtility.currentViewWidth, m_spIdleSetFoldout.boolValue, 1, true);

            lastRect = GUILayoutUtility.GetLastRect();
            curHeight = lastRect.y + lastRect.height;

            if (m_spIdleSetFoldout.boolValue)
            {
                curHeight += 2f;

                m_idleSetReorderableList.DoLayoutList();

                GUILayout.Space(4f);

                lastRect = GUILayoutUtility.GetLastRect();
                curHeight = lastRect.y + lastRect.height;

#if UNITY_2019_3_OR_NEWER
                dropRect = new Rect(18f, curHeight - 20f, EditorGUIUtility.currentViewWidth - 107f, 20f);
#else
                dropRect = new Rect(15f, curHeight - 18f, EditorGUIUtility.currentViewWidth - 95f, 20f);
#endif
                GUI.Box(dropRect, new GUIContent("Drag Idle Anim or IdleSet Here"));
                DragDropIdles(dropRect);

                GUILayout.Space(18f);

                lastRect = GUILayoutUtility.GetLastRect();
                curHeight = lastRect.y + lastRect.height;

                curHeight += 4f;
                GUILayout.Space(4f);
            }

            m_spBlendSpaceFoldout.boolValue = EditorUtil.EditorFunctions.DrawFoldout(
                "Blend Spaces", curHeight, EditorGUIUtility.currentViewWidth, m_spBlendSpaceFoldout.boolValue, 1, true);

            lastRect = GUILayoutUtility.GetLastRect();
            curHeight = lastRect.y + lastRect.height;

            if (m_spBlendSpaceFoldout.boolValue)
            {
                curHeight += 2f;

                m_blendSpaceReorderableList.DoLayoutList();

                GUILayout.Space(4f);

                lastRect = GUILayoutUtility.GetLastRect();
                curHeight = lastRect.y + lastRect.height;

#if UNITY_2019_3_OR_NEWER
                dropRect = new Rect(18f, curHeight - 20f, EditorGUIUtility.currentViewWidth - 107f, 20f);
#else
                dropRect = new Rect(15f, curHeight - 18f, EditorGUIUtility.currentViewWidth - 95f, 20f);
#endif
                GUI.Box(dropRect, new GUIContent("Drag Anim or BlendSpace Here"));
                DragDropBlendSpaces(dropRect);

                curHeight += 31f;
                GUILayout.Space(4f);
            }

            if (GUI.Button(new Rect(EditorGUIUtility.currentViewWidth
                                    - 175f, curHeight, 150f, 18f), "Export Mirrored"))
            {
                ExportToMirroredModule();
            }

            curHeight += 35f;
            GUILayout.Space(35f);

            curHeight += 28f;

            curHeight += 55f;
            GUILayout.Space(12f);

            serializedObject.ApplyModifiedProperties();
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void ManageQueuedActions()
        {
            if (m_queueDeleteCompositeCategory > -1)
            {
                if (m_queueDeleteCompositeCategory < m_spCompositeCategories.arraySize)
                {
                    SerializedProperty spCategory = m_spCompositeCategories.GetArrayElementAtIndex(m_queueDeleteCompositeCategory);

                    SerializedProperty spCompositeList = spCategory.FindPropertyRelative("Composites");

                    for (int i = 0; i < spCompositeList.arraySize; ++i)
                    {
                        SerializedProperty spObject = spCompositeList.GetArrayElementAtIndex(i);

                        AssetDatabase.RemoveObjectFromAsset(spObject.objectReferenceValue);
                        spObject.objectReferenceValue = null;
                    }
                    m_spCompositeCategories.DeleteArrayElementAtIndex(m_queueDeleteCompositeCategory);

                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(m_data));

                    m_compositeReorderableLists.RemoveAt(m_queueDeleteCompositeCategory);

                    SetupReorderableLists();

                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }

                m_queueDeleteCompositeCategory = -1;
            }

            if (m_queueShiftCompositeCategoryUp > 0) //Can't shift the first list up
            {
                if (m_queueShiftCompositeCategoryUp < m_spCompositeCategories.arraySize)
                {
                    m_spCompositeCategories.MoveArrayElement(m_queueShiftCompositeCategoryUp, m_queueShiftCompositeCategoryUp - 1);
                    SetupReorderableLists();
                }

                m_queueShiftCompositeCategoryUp = -1;
            }

            if (m_queueShiftCompositeCategoryDown > -1)
            {
                if (m_queueShiftCompositeCategoryDown + 1 < m_spCompositeCategories.arraySize)
                {
                    m_spCompositeCategories.MoveArrayElement(m_queueShiftCompositeCategoryDown, m_queueShiftCompositeCategoryDown + 1);
                    SetupReorderableLists();
                }

                m_queueShiftCompositeCategoryDown = -1;
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void DragDropComposites(Rect a_dropRect, int a_categoryId)
        {
            Event evt = Event.current;

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    {
                        if (!a_dropRect.Contains(evt.mousePosition))
                            return;

                        if (DragAndDrop.objectReferences[0].GetType() == typeof(AnimationClip)
                            || DragAndDrop.objectReferences[0].GetType() == typeof(MxMAnimationClipComposite))
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                            if (evt.type == EventType.DragPerform)
                            {
                                for (int i = 0; i < DragAndDrop.objectReferences.Length; ++i)
                                {
                                    DragAndDrop.AcceptDrag();

                                    Object obj = DragAndDrop.objectReferences[i];

                                    if (obj != null)
                                    {
                                        if (obj.GetType() == typeof(AnimationClip))
                                        {
                                            CreateNewAnimationCompositeFromDrag(obj as AnimationClip, a_categoryId);
                                        }
                                        else if (obj.GetType() == typeof(MxMAnimationClipComposite))
                                        {
                                            CreateNewAnimationCompositeFromDrag(obj as MxMAnimationClipComposite, a_categoryId);
                                        }
                                    }
                                }

                            }
                        }

                    }
                    break;
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void DragDropIdles(Rect a_dropRect)
        {
            Event evt = Event.current;

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    {
                        if (!a_dropRect.Contains(evt.mousePosition))
                            return;

                        if (DragAndDrop.objectReferences[0].GetType() == typeof(AnimationClip)
                            || DragAndDrop.objectReferences[0].GetType() == typeof(MxMAnimationIdleSet))
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                            if (evt.type == EventType.DragPerform)
                            {
                                for (int i = 0; i < DragAndDrop.objectReferences.Length; ++i)
                                {
                                    DragAndDrop.AcceptDrag();

                                    Object obj = DragAndDrop.objectReferences[i];

                                    if (obj != null)
                                    {
                                        if (obj.GetType() == typeof(AnimationClip))
                                        {
                                            CreateNewIdleSetFromDrag(obj as AnimationClip);
                                        }
                                        else if (obj.GetType() == typeof(MxMAnimationClipComposite))
                                        {
                                            CreateNewIdleSetFromDrag(obj as MxMAnimationIdleSet);
                                        }
                                    }
                                }
                                Repaint();
                            }
                        }

                    }
                    break;
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void DragDropBlendSpaces(Rect a_dropRect)
        {
            Event evt = Event.current;

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    {
                        if (!a_dropRect.Contains(evt.mousePosition))
                            return;

                        if (DragAndDrop.objectReferences[0].GetType() == typeof(AnimationClip)
                            || DragAndDrop.objectReferences[0].GetType() == typeof(MxMBlendSpace))
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                            if (evt.type == EventType.DragPerform)
                            {
                                for (int i = 0; i < DragAndDrop.objectReferences.Length; ++i)
                                {
                                    DragAndDrop.AcceptDrag();

                                    Object obj = DragAndDrop.objectReferences[i];

                                    if (obj != null)
                                    {
                                        if (obj.GetType() == typeof(AnimationClip))
                                        {
                                            CreateNewBlendSpaceFromDrag(obj as AnimationClip);
                                        }
                                        else if (obj.GetType() == typeof(MxMBlendSpace))
                                        {
                                            CreateNewBlendSpaceFromDrag(obj as MxMBlendSpace);
                                        }
                                    }
                                }
                                Repaint();
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
        public void CreateNewAnimationComposite(int a_categoryId)
        {
            if (a_categoryId > m_spCompositeCategories.arraySize)
            {
                Debug.LogError("Trying to create a new animation composite, but the category Id is out of range. Aborting operation.");
                return;
            }

            MxMAnimationClipComposite newComposite = ScriptableObject.CreateInstance<MxMAnimationClipComposite>();
            newComposite.name = "MxMAnimComposite";

            if (newComposite != null)
            {
                AssetDatabase.AddObjectToAsset(newComposite, m_data);
                newComposite.hideFlags = m_spHideSubAssets.boolValue ? HideFlags.HideInHierarchy : HideFlags.None;
#if UNITY_2020_2_OR_NEWER
                AssetDatabase.Refresh();
#else
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newComposite));
#endif

                SerializedProperty spCategory = m_spCompositeCategories.GetArrayElementAtIndex(a_categoryId);
                SerializedProperty spCompositeList = spCategory.FindPropertyRelative("Composites");

                spCompositeList.InsertArrayElementAtIndex(spCompositeList.arraySize);

                SerializedProperty spComposite = spCompositeList.GetArrayElementAtIndex(spCompositeList.arraySize - 1);
                spComposite.objectReferenceValue = newComposite;

                CompositeCategory category = m_data.CompositeCategories[a_categoryId];

                newComposite.IgnoreEdges = category.IgnoreEdges_default;
                newComposite.ExtrapolateTrajectory = category.Extrapolate_default;
                newComposite.FlattenTrajectory = category.FlattenTrajectory_default;
                newComposite.RuntimeSplicing = category.RuntimeSplicing_default;
                newComposite.GlobalTags = category.RequireTags_default;
                newComposite.GlobalFavourTags = category.FavourTags_default;
                newComposite.TargetAnimModule = m_data;
                newComposite.TargetPreProcess = null;
                newComposite.TargetPrefab = m_data.Prefab;
                newComposite.CategoryId = a_categoryId;
                
                newComposite.ValidateBaseData();

                EditorUtility.SetDirty(newComposite);
            }
        }

        //===========================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public bool CreateNewAnimationCompositeFromDrag(AnimationClip a_clip, int a_categoryId)
        {
            if (a_categoryId > m_spCompositeCategories.arraySize)
            {
                Debug.LogError("Trying to create a new animation composite, but the category Id is out of range. Aborting operation.");
                return false;
            }

            bool success = false;

            if (a_clip != null)
            {
                MxMAnimationClipComposite newComposite =
                    ScriptableObject.CreateInstance<MxMAnimationClipComposite>();

                newComposite.name = a_clip.name + "_comp";

                if (newComposite != null)
                {
                    newComposite.SetPrimaryAnim(a_clip);

                    AssetDatabase.AddObjectToAsset(newComposite, m_data);
                    newComposite.hideFlags = m_spHideSubAssets.boolValue ? HideFlags.HideInHierarchy : HideFlags.None;

#if UNITY_2020_2_OR_NEWER
                    AssetDatabase.Refresh();
#else
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newComposite));
#endif

                    SerializedProperty spCategory = m_spCompositeCategories.GetArrayElementAtIndex(a_categoryId);
                    SerializedProperty spCompositeList = spCategory.FindPropertyRelative("Composites");

                    spCompositeList.InsertArrayElementAtIndex(spCompositeList.arraySize);

                    SerializedProperty spComposite = spCompositeList.GetArrayElementAtIndex(spCompositeList.arraySize - 1);
                    spComposite.objectReferenceValue = newComposite;

                    CompositeCategory category = m_data.CompositeCategories[a_categoryId];

                    newComposite.IgnoreEdges = category.IgnoreEdges_default;
                    newComposite.ExtrapolateTrajectory = category.Extrapolate_default;
                    newComposite.FlattenTrajectory = category.FlattenTrajectory_default;
                    newComposite.RuntimeSplicing = category.RuntimeSplicing_default;
                    newComposite.GlobalTags = category.RequireTags_default;
                    newComposite.GlobalFavourTags = category.FavourTags_default;
                    newComposite.TargetPreProcess = null;
                    newComposite.TargetAnimModule = m_data;
                    newComposite.TargetPrefab = m_data.Prefab;
                    newComposite.CategoryId = a_categoryId;
                    
                    newComposite.ValidateBaseData();

                    EditorUtility.SetDirty(newComposite);

                    success = true;
                }
            }

            return success;
        }

        //===========================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public bool CreateNewAnimationCompositeFromDrag(MxMAnimationClipComposite a_composite, int a_categoryId)
        {
            if (a_categoryId > m_spCompositeCategories.arraySize)
            {
                Debug.LogError("Trying to create a new animation composite, but the category Id is out of range. Aborting operation.");
                return false;
            }

            bool success = false;

            if (a_composite != null)
            {
                MxMAnimationClipComposite newComposite = ScriptableObject.CreateInstance<MxMAnimationClipComposite>();
                EditorUtility.CopySerialized(a_composite, newComposite);
                newComposite.TargetPreProcess = null;
                newComposite.TargetAnimModule = m_data;
                newComposite.TargetPrefab = m_data.Prefab;
                newComposite.CategoryId = a_categoryId;
                newComposite.name = a_composite.name;

                AssetDatabase.AddObjectToAsset(newComposite, m_data);
                newComposite.hideFlags = m_spHideSubAssets.boolValue ? HideFlags.HideInHierarchy : HideFlags.None;
#if UNITY_2020_2_OR_NEWER
                AssetDatabase.Refresh();
#else
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newComposite));
#endif

                SerializedProperty spCategory = m_spCompositeCategories.GetArrayElementAtIndex(a_categoryId);
                SerializedProperty spCompositeList = spCategory.FindPropertyRelative("Composites");

                spCompositeList.InsertArrayElementAtIndex(spCompositeList.arraySize);
                SerializedProperty spComposite = spCompositeList.GetArrayElementAtIndex(spCompositeList.arraySize - 1);
                spComposite.objectReferenceValue = newComposite;

                newComposite.ValidateBaseData();

                EditorUtility.SetDirty(newComposite);

                success = true;
            }

            return success;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void CreateNewIdleSet()
        {
            MxMAnimationIdleSet newIdleSet = ScriptableObject.CreateInstance<MxMAnimationIdleSet>();
            if (newIdleSet != null)
            {
                newIdleSet.name = "MxMIdleSet";
                newIdleSet.TargetPreProcess = null;
                newIdleSet.TargetAnimModule = m_data;

                AssetDatabase.AddObjectToAsset(newIdleSet, m_data);
                newIdleSet.hideFlags = m_spHideSubAssets.boolValue ? HideFlags.HideInHierarchy : HideFlags.None;
#if UNITY_2020_2_OR_NEWER
                AssetDatabase.Refresh();
#else
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newIdleSet));
#endif

                m_spAnimIdleSets.InsertArrayElementAtIndex(m_spAnimIdleSets.arraySize);
                SerializedProperty spIdleSet = m_spAnimIdleSets.GetArrayElementAtIndex(m_spAnimIdleSets.arraySize - 1);
                spIdleSet.objectReferenceValue = newIdleSet;

                AnimModuleDefaults defaultSettings = m_data.DefaultSettings;

                newIdleSet.MinLoops = defaultSettings.MinLoops;
                newIdleSet.MaxLoops = defaultSettings.MaxLoops;
                newIdleSet.Tags = defaultSettings.RequireTags;
                newIdleSet.FavourTags = defaultSettings.FavourTags;
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public bool CreateNewIdleSetFromDrag(AnimationClip a_primaryIdleAnim)
        {
            bool success = false;

            if (a_primaryIdleAnim != null)
            {
                MxMAnimationIdleSet newIdleSet =
                    ScriptableObject.CreateInstance<MxMAnimationIdleSet>();
                newIdleSet.name = a_primaryIdleAnim.name;

                if (newIdleSet != null)
                {
                    newIdleSet.SetPrimaryAnim(a_primaryIdleAnim);
                    newIdleSet.MinLoops = 1;
                    newIdleSet.MaxLoops = 2;
                    newIdleSet.TargetPreProcess = null;
                    newIdleSet.TargetAnimModule = m_data;

                    AssetDatabase.AddObjectToAsset(newIdleSet, m_data);
                    newIdleSet.hideFlags = m_spHideSubAssets.boolValue ? HideFlags.HideInHierarchy : HideFlags.None;
#if UNITY_2020_2_OR_NEWER
                    AssetDatabase.Refresh();
#else
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newIdleSet));
#endif

                    m_spAnimIdleSets.InsertArrayElementAtIndex(m_spAnimIdleSets.arraySize);
                    SerializedProperty spIdleSet = m_spAnimIdleSets.GetArrayElementAtIndex(m_spAnimIdleSets.arraySize - 1);
                    spIdleSet.objectReferenceValue = newIdleSet;

                    AnimModuleDefaults defaultSettings = m_data.DefaultSettings;

                    newIdleSet.MinLoops = defaultSettings.MinLoops;
                    newIdleSet.MaxLoops = defaultSettings.MaxLoops;
                    newIdleSet.Tags = defaultSettings.RequireTags;
                    newIdleSet.FavourTags = defaultSettings.FavourTags;

                    success = true;
                }
            }

            return success;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public bool CreateNewIdleSetFromDrag(MxMAnimationIdleSet a_idleSet)
        {
            bool success = false;

            if (a_idleSet != null)
            {
                MxMAnimationIdleSet newIdleSet = ScriptableObject.CreateInstance<MxMAnimationIdleSet>();
                newIdleSet.CopyData(a_idleSet);
                newIdleSet.name = a_idleSet.name;
                newIdleSet.TargetPreProcess = null;
                newIdleSet.TargetAnimModule = m_data;

                AssetDatabase.AddObjectToAsset(newIdleSet, m_data);
                newIdleSet.hideFlags = m_spHideSubAssets.boolValue ? HideFlags.HideInHierarchy : HideFlags.None;
#if UNITY_2020_2_OR_NEWER
                AssetDatabase.Refresh();
#else
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newIdleSet));
#endif

                m_spAnimIdleSets.InsertArrayElementAtIndex(m_spAnimIdleSets.arraySize);
                SerializedProperty spIdleSet = m_spAnimIdleSets.GetArrayElementAtIndex(m_spAnimIdleSets.arraySize - 1);
                spIdleSet.objectReferenceValue = newIdleSet;

                AnimModuleDefaults defaultSettings = m_data.DefaultSettings;

                newIdleSet.MinLoops = defaultSettings.MinLoops;
                newIdleSet.MaxLoops = defaultSettings.MaxLoops;
                newIdleSet.Tags = defaultSettings.RequireTags;
                newIdleSet.FavourTags = defaultSettings.FavourTags;

                success = true;
            }
            return success;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void CreateNewBlendSpace()
        {
            MxMBlendSpace newBlendSpace = ScriptableObject.CreateInstance<MxMBlendSpace>();
            newBlendSpace.name = "MxMBlendSpace";

            if (newBlendSpace != null)
            {
                newBlendSpace.TargetPreProcess = null;
                newBlendSpace.TargetAnimModule = m_data;
                newBlendSpace.TargetPrefab = m_data.Prefab;

                AssetDatabase.AddObjectToAsset(newBlendSpace, m_data);
                newBlendSpace.hideFlags = m_spHideSubAssets.boolValue ? HideFlags.HideInHierarchy : HideFlags.None;
#if UNITY_2020_2_OR_NEWER
                AssetDatabase.Refresh();
#else
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newBlendSpace));
#endif

                m_spBlendSpaces.InsertArrayElementAtIndex(m_spBlendSpaces.arraySize);
                SerializedProperty spBlendSpace = m_spBlendSpaces.GetArrayElementAtIndex(m_spBlendSpaces.arraySize - 1);
                spBlendSpace.objectReferenceValue = newBlendSpace;

                AnimModuleDefaults defaultSettings = m_data.DefaultSettings;

                newBlendSpace.ScatterSpace = defaultSettings.BlendSpaceType;
                newBlendSpace.ScatterSpacing = defaultSettings.ScatterSpacing;
                newBlendSpace.NormalizeTime = defaultSettings.NormalizeBlendSpace;
                newBlendSpace.Magnitude = defaultSettings.BlendSpaceMagnitude;
                newBlendSpace.Smoothing = defaultSettings.BlendSpaceSmoothing;
                newBlendSpace.GlobalTags = defaultSettings.RequireTags;
                newBlendSpace.GlobalFavourTags = defaultSettings.FavourTags;
                
                newBlendSpace.ValidateBaseData();
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public bool CreateNewBlendSpaceFromDrag(AnimationClip a_primaryBlendAnim)
        {
            bool success = false;

            if (a_primaryBlendAnim != null)
            {
                MxMBlendSpace newBlendSpace =
                    ScriptableObject.CreateInstance<MxMBlendSpace>();
                newBlendSpace.name = a_primaryBlendAnim.name;

                if (newBlendSpace != null)
                {
                    newBlendSpace.SetPrimaryAnim(a_primaryBlendAnim);
                    newBlendSpace.TargetPreProcess = null;
                    newBlendSpace.TargetAnimModule = m_data;
                    newBlendSpace.TargetPrefab = m_data.Prefab;

                    AssetDatabase.AddObjectToAsset(newBlendSpace, m_data);
                    newBlendSpace.hideFlags = m_spHideSubAssets.boolValue ? HideFlags.HideInHierarchy : HideFlags.None;
#if UNITY_2020_2_OR_NEWER
                    AssetDatabase.Refresh();
#else
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newBlendSpace));
#endif

                    m_spBlendSpaces.InsertArrayElementAtIndex(m_spBlendSpaces.arraySize);
                    SerializedProperty spBlendSpace = m_spBlendSpaces.GetArrayElementAtIndex(m_spBlendSpaces.arraySize - 1);
                    spBlendSpace.objectReferenceValue = newBlendSpace;

                    AnimModuleDefaults defaultSettings = m_data.DefaultSettings;

                    newBlendSpace.ScatterSpace = defaultSettings.BlendSpaceType;
                    newBlendSpace.ScatterSpacing = defaultSettings.ScatterSpacing;
                    newBlendSpace.NormalizeTime = defaultSettings.NormalizeBlendSpace;
                    newBlendSpace.Magnitude = defaultSettings.BlendSpaceMagnitude;
                    newBlendSpace.Smoothing = defaultSettings.BlendSpaceSmoothing;
                    newBlendSpace.GlobalTags = defaultSettings.RequireTags;
                    newBlendSpace.GlobalFavourTags = defaultSettings.FavourTags;
                    
                    newBlendSpace.ValidateBaseData();

                    success = true;
                }
            }

            return success;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public bool CreateNewBlendSpaceFromDrag(MxMBlendSpace a_blendSpace)
        {
            bool success = false;

            if (a_blendSpace != null)
            {
                MxMBlendSpace newBlendSpace = ScriptableObject.CreateInstance<MxMBlendSpace>();
                newBlendSpace.TargetPreProcess = null;
                newBlendSpace.TargetAnimModule = m_data;
                newBlendSpace.TargetPrefab = m_data.Prefab;
                newBlendSpace.CopyData(a_blendSpace);
                newBlendSpace.name = a_blendSpace.name;


                AssetDatabase.AddObjectToAsset(newBlendSpace, m_data);
                newBlendSpace.hideFlags = m_spHideSubAssets.boolValue ? HideFlags.HideInHierarchy : HideFlags.None;
#if UNITY_2020_2_OR_NEWER
                AssetDatabase.Refresh();
#else
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newBlendSpace));
#endif

                m_spBlendSpaces.InsertArrayElementAtIndex(m_spBlendSpaces.arraySize);
                SerializedProperty spBlendSpace = m_spBlendSpaces.GetArrayElementAtIndex(m_spBlendSpaces.arraySize - 1);
                spBlendSpace.objectReferenceValue = newBlendSpace;

                AnimModuleDefaults defaultSettings = m_data.DefaultSettings;

                newBlendSpace.ScatterSpace = defaultSettings.BlendSpaceType;
                newBlendSpace.ScatterSpacing = defaultSettings.ScatterSpacing;
                newBlendSpace.NormalizeTime = defaultSettings.NormalizeBlendSpace;
                newBlendSpace.Magnitude = defaultSettings.BlendSpaceMagnitude;
                newBlendSpace.Smoothing = defaultSettings.BlendSpaceSmoothing;
                newBlendSpace.GlobalTags = defaultSettings.RequireTags;
                newBlendSpace.GlobalFavourTags = defaultSettings.FavourTags;
                
                newBlendSpace.ValidateBaseData();

                success = true;
            }
            return success;
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


        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private bool AreMxMAnimsValid()
        {
            if (m_data != null)
            {
                List<MxMAnimationIdleSet> idleSets = m_data.AnimationIdleSets;
                List<MxMBlendSpace> blendSpaces = m_data.BlendSpaces;

                foreach (CompositeCategory category in m_data.CompositeCategories)
                {
                    if (category != null && category.Composites != null)
                    {
                        foreach (MxMAnimationClipComposite composite in category.Composites)
                        {
                            if (composite == null)
                                return false;

                            if (composite.PrimaryClip == null)
                            {
                                EditorUtility.DisplayDialog("Error: Empty Composite", "You have a composite with no animations in it. Anim Module: " 
                                    + m_data.name + " Category: " + category.CatagoryName + "Composite: " + composite.CompositeName
                                    + ". Please add an animation or remove the composite before pre-processing", "Ok");

                                EditorGUIUtility.PingObject(composite);

                                return false;
                            }
                        }
                    }
                }

                if (idleSets != null)
                {
                    foreach (MxMAnimationIdleSet idleSet in idleSets)
                    {
                        if (idleSet == null)
                        {
                            return false;
                        }

                        if (idleSet.PrimaryClip == null)
                        {
                            EditorUtility.DisplayDialog("Error: Empty Idle Set", "You have a IdleSet with no animations in it. Anim Module: " 
                                + m_data.name
                                + ". Please add an animation or remove the idle set before pre-processing", "Ok");

                            EditorGUIUtility.PingObject(idleSet);

                            return false;
                        }
                    }
                }

                if (blendSpaces != null)
                {
                    foreach (MxMBlendSpace blendSpace in blendSpaces)
                    {
                        if (blendSpace == null)
                            return false;

                        List<AnimationClip> clips = blendSpace.Clips;

                        if (clips == null || clips.Count == 0)
                        {
                            EditorUtility.DisplayDialog("Error: Empty blend space", "You have a blendspace with no animations in it. Anim Module: " 
                                + m_data.name + " Blendspace: " + blendSpace.BlendSpaceName
                                + ". Please add an animation or remove the blendspace before pre-processing", "Ok");

                            EditorGUIUtility.PingObject(blendSpace);

                            return false;
                        }

                        if (clips[0] == null)
                        {
                            EditorUtility.DisplayDialog("Error: Empty blend space", "You have a blendspace with no animations in it. Anim Module: " 
                                + m_data.name + " Blendspace: " + blendSpace.BlendSpaceName
                                + ". Please add an animation or remove the blendspace before pre-processing", "Ok");

                            EditorGUIUtility.PingObject(blendSpace);

                            return false;
                        }
                    }
                }

            }
            else
            {
                return false;
            }

            return true;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void UpdateTargetModelForAnimData()
        {
            if (m_spOverrideConfigModule.objectReferenceValue == null)
            {
                return;
            }
            else
            {
                var configModule = m_spOverrideConfigModule.objectReferenceValue as MotionMatchConfigModule;
                if (configModule.Prefab == null)
                    return;
            }


            for (int i = 0; i < m_spCompositeCategories.arraySize; ++i)
            {
                SerializedProperty spCategory = m_spCompositeCategories.GetArrayElementAtIndex(i);
                SerializedProperty spCompositeList = spCategory.FindPropertyRelative("Composites");

                for (int k = 0; k < spCompositeList.arraySize; ++k)
                {
                    SerializedProperty spComposite = spCompositeList.GetArrayElementAtIndex(k);

                    if (spComposite.objectReferenceValue != null)
                    {
                        var composite = spComposite.objectReferenceValue as MxMAnimationClipComposite;

                        if (m_spOverrideConfigModule.objectReferenceValue != null)
                        {
                            var configModule = m_spOverrideConfigModule.objectReferenceValue as MotionMatchConfigModule;

                            if(composite.TargetPrefab != configModule.Prefab)
                            {
                                composite.TargetPrefab = configModule.Prefab;
                                EditorUtility.SetDirty(spComposite.objectReferenceValue);
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < m_spBlendSpaces.arraySize; ++i)
            {
                SerializedProperty spBlendSpace = m_spBlendSpaces.GetArrayElementAtIndex(i);

                if (spBlendSpace.objectReferenceValue != null)
                {
                    MxMBlendSpace blendSpace = spBlendSpace.objectReferenceValue as MxMBlendSpace;

                    if (m_spOverrideConfigModule.objectReferenceValue != null)
                    {
                        var configModule = m_spOverrideConfigModule.objectReferenceValue as MotionMatchConfigModule;

                        if (blendSpace.TargetPrefab != configModule.Prefab)
                        {
                            blendSpace.TargetPrefab = configModule.Prefab;
                            EditorUtility.SetDirty(spBlendSpace.objectReferenceValue);
                        }
                    }
                }
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void SetupReorderableLists()
        {
            m_compositeReorderableLists = new List<ReorderableList>();

            for (int i = 0; i < m_spCompositeCategories.arraySize; ++i)
            {
                SerializedProperty spCategory = m_spCompositeCategories.GetArrayElementAtIndex(i);
                SerializedProperty spCompositeList = spCategory.FindPropertyRelative("Composites");

                ReorderableList compositeReorderableList = new ReorderableList(serializedObject,
                    spCompositeList, true, true, true, true);

                m_compositeReorderableLists.Add(compositeReorderableList);

                compositeReorderableList.drawElementCallback =
                    (Rect a_rect, int a_index, bool a_isActive, bool a_isFocused) =>
                    {
                        var element = compositeReorderableList.serializedProperty.GetArrayElementAtIndex(a_index);

                        EditorGUI.BeginDisabledGroup(true);

                        string elementName = "Anim " + (a_index + 1).ToString();

                        if (element.objectReferenceValue != null)
                        {
                            string testName = ((MxMAnimationClipComposite) element.objectReferenceValue).CompositeName;
                            if (testName != "")
                            {
                                elementName = testName;
                            }
                        }
                        
                        EditorGUI.LabelField(new Rect(a_rect.x, a_rect.y, 100f, EditorGUIUtility.singleLineHeight), elementName);
                        
                        EditorGUI.ObjectField(new Rect(a_rect.x + 100f, a_rect.y, EditorGUIUtility.currentViewWidth - 170f,
                            EditorGUIUtility.singleLineHeight), element, new GUIContent(""));

                        EditorGUI.EndDisabledGroup();
                    };

                compositeReorderableList.drawHeaderCallback =
                    (Rect a_rect) =>
                    {
                        Rect deleteRect = a_rect;
                        deleteRect.x = a_rect.width - 186f;
                        deleteRect.width = 50f;
                        deleteRect.y += 1f;
                        deleteRect.height -= 2f;

                        a_rect.x += 10f;
                        a_rect.width /= 2f;
                        a_rect.y += 1f;

                        GUIStyle smallTitleStyle = new GUIStyle(GUI.skin.label);
                        smallTitleStyle.fontSize = 10;
                        smallTitleStyle.fontStyle = FontStyle.Bold;

                        SerializedProperty category = m_spCompositeCategories.GetArrayElementAtIndex(m_currentCompositeCategory);
                        SerializedProperty categoryName = category.FindPropertyRelative("CatagoryName");

                        a_rect.width = smallTitleStyle.CalcSize(new GUIContent(categoryName.stringValue)).x + 10f;

                        categoryName.stringValue = EditorGUI.TextField(a_rect, categoryName.stringValue, smallTitleStyle);



                        GUIStyle redButtonStyle = new GUIStyle(GUI.skin.button);
                        redButtonStyle.normal.textColor = Color.red;

                        if (GUI.Button(deleteRect, "Delete", redButtonStyle))
                        {
                            if (EditorUtility.DisplayDialog("WARNING! Delete Composite List?",
                                "STOP! WAIT! HOLD UP! \n\nYOU ARE ABOUT TO DELETE AN ENTIRE COMPOSITE LIST! \n\nAre you sure?", "Yes", "Cancel"))
                            {
                                m_queueDeleteCompositeCategory = m_currentCompositeCategory;
                            }
                        }

                        if(GUI.Button(new Rect(deleteRect.x - deleteRect.width - 15f, deleteRect.y,
                            deleteRect.width, deleteRect.height), "Export"))
                        {
                            ExportCategoryToModule(m_currentCompositeCategory);
                        }

                        if (GUI.Button(new Rect(deleteRect.x + deleteRect.width + 30f, deleteRect.y,
                            deleteRect.width, deleteRect.height), "Up"))
                        {
                            m_queueShiftCompositeCategoryUp = m_currentCompositeCategory;
                        }

                        if (GUI.Button(new Rect(deleteRect.x + deleteRect.width * 2f + 35f, deleteRect.y,
                            deleteRect.width, deleteRect.height), "Down"))
                        {
                            m_queueShiftCompositeCategoryDown = m_currentCompositeCategory;
                        }

                        GUIStyle invisiButton = new GUIStyle(GUI.skin.label);

                        Rect settingsBtnRect = new Rect(deleteRect.x + deleteRect.width * 3f + 40f,
                            deleteRect.y, 15f, 14f);


                        Texture cogIcon = EditorGUIUtility.IconContent("_Popup").image;
                        if (GUI.Button(settingsBtnRect, cogIcon, invisiButton))
                        {
                            CompositeCategorySettingsWindow.SetData(serializedObject, m_data, 
                                m_currentCompositeCategory);
                            CompositeCategorySettingsWindow.ShowWindow();
                        }
                    };

                int id = i;
                compositeReorderableList.onAddCallback =
                   (ReorderableList a_list) =>
                   {
                       CreateNewAnimationComposite(id);
                   };

                compositeReorderableList.onRemoveCallback =
                (ReorderableList a_list) =>
                {
                    if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete this composite", "Yes", "No"))
                    {
                        if (a_list.index >= 0 && a_list.index < a_list.serializedProperty.arraySize)
                        {
                            SerializedProperty spObject = a_list.serializedProperty.GetArrayElementAtIndex(a_list.index);

                            if (spObject.objectReferenceValue != null)
                            {
                                AssetDatabase.RemoveObjectFromAsset(spObject.objectReferenceValue);
#if UNITY_2020_2_OR_NEWER
                                AssetDatabase.Refresh();
#else
                                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(m_data));
#endif
                                spObject.objectReferenceValue = null;
                            }
                        }

                        ReorderableList.defaultBehaviours.DoRemoveButton(a_list);
                    }
                };
            }

            m_idleSetReorderableList = new ReorderableList(serializedObject,
                m_spAnimIdleSets, true, true, true, true);

            m_blendSpaceReorderableList = new ReorderableList(serializedObject,
                m_spBlendSpaces, true, true, true, true);

            m_idleSetReorderableList.drawElementCallback =
                (Rect a_rect, int a_index, bool a_isActive, bool a_isFocused) =>
                {
                    var element = m_idleSetReorderableList.serializedProperty.GetArrayElementAtIndex(a_index);

                    EditorGUI.BeginDisabledGroup(true);

                    EditorGUI.LabelField(new Rect(a_rect.x, a_rect.y, 100f, EditorGUIUtility.singleLineHeight), "Idle Set " + (a_index + 1).ToString());
                    EditorGUI.ObjectField(new Rect(a_rect.x + 100f, a_rect.y, EditorGUIUtility.currentViewWidth - 170f,
                        EditorGUIUtility.singleLineHeight), element, new GUIContent(""));

                    EditorGUI.EndDisabledGroup();
                };

            m_blendSpaceReorderableList.drawElementCallback =
                (Rect a_rect, int a_index, bool a_isActive, bool a_isFocused) =>
                {
                    var element = m_blendSpaceReorderableList.serializedProperty.GetArrayElementAtIndex(a_index);

                    EditorGUI.BeginDisabledGroup(true);
                    
                    MxMBlendSpace blendSpace = null;
                    
                    if (m_data.BlendSpaces.Count > a_index)
                    {
                        blendSpace = m_data.BlendSpaces[a_index];
                        EditorGUI.LabelField(new Rect(a_rect.x, a_rect.y, 150f, EditorGUIUtility.singleLineHeight), m_data.BlendSpaces[a_index].BlendSpaceName);
                    }
                    else
                    {
                        EditorGUI.LabelField(new Rect(a_rect.x, a_rect.y, 150f, EditorGUIUtility.singleLineHeight), "Blend Space " + (a_index + 1).ToString());
                    }
                    
                    EditorGUI.ObjectField(new Rect(a_rect.x + 150f, a_rect.y, EditorGUIUtility.currentViewWidth - 170f,
                        EditorGUIUtility.singleLineHeight), element, new GUIContent(""));

                    EditorGUI.EndDisabledGroup();
                };

            m_idleSetReorderableList.drawHeaderCallback =
                (Rect a_rect) =>
                {
                    EditorGUI.LabelField(a_rect, "Idle Sets");
                };

            m_blendSpaceReorderableList.drawHeaderCallback =
                (Rect a_rect) =>
                {
                    EditorGUI.LabelField(a_rect, "Blend Spaces");
                };

            m_idleSetReorderableList.onAddCallback =
                (ReorderableList a_list) =>
                {
                    CreateNewIdleSet();
                };

            m_blendSpaceReorderableList.onAddCallback =
                (ReorderableList a_list) =>
                {
                    CreateNewBlendSpace();
                };

            m_idleSetReorderableList.onRemoveCallback =
                (ReorderableList a_list) =>
                {
                    if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete this idle set", "Yes", "No"))
                    {
                        if (a_list.index >= 0 && a_list.index < a_list.serializedProperty.arraySize)
                        {
                            SerializedProperty spObject = a_list.serializedProperty.GetArrayElementAtIndex(a_list.index);

                            if (spObject.objectReferenceValue != null)
                            {
                                AssetDatabase.RemoveObjectFromAsset(spObject.objectReferenceValue);
#if UNITY_2020_2_OR_NEWER
                                AssetDatabase.Refresh();
#else
                                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(m_data));
#endif
                                spObject.objectReferenceValue = null;
                            }
                        }

                        ReorderableList.defaultBehaviours.DoRemoveButton(a_list);
                    }
                };

            m_blendSpaceReorderableList.onRemoveCallback =
                (ReorderableList a_list) =>
                {
                    if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete this blend space", "Yes", "No"))
                    {
                        if (a_list.index >= 0 && a_list.index < a_list.serializedProperty.arraySize)
                        {
                            SerializedProperty spObject = a_list.serializedProperty.GetArrayElementAtIndex(a_list.index);

                            if (spObject.objectReferenceValue != null)
                            {
                                AssetDatabase.RemoveObjectFromAsset(spObject.objectReferenceValue);
#if UNITY_2020_2_OR_NEWER
                                AssetDatabase.Refresh();
#else
                                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(m_data));
#endif
                                spObject.objectReferenceValue = null;
                            }
                        }

                        ReorderableList.defaultBehaviours.DoRemoveButton(a_list);
                    }
                };

            for (int i = 0; i < m_data.CompositeCategories.Count; ++i)
            {
                foreach (MxMAnimationClipComposite composite in m_data.CompositeCategories[i].Composites)
                {
                    composite.CategoryId = i;
                }
            }
        }

        public void ExportCategoryToModule(int a_categoryId)
        {
            serializedObject.ApplyModifiedProperties();

            m_data.ValidateData();
            EditorUtility.SetDirty(m_data);
            AssetDatabase.SaveAssets();

            string startLocation = AssetDatabase.GetAssetPath(m_data).Replace(name + ".asset", "");

            string fileName = EditorUtility.SaveFilePanelInProject("Export Composite Category", 
                "MxMAnimationModule", "asset", "Export composite category as", startLocation).Replace(".asset", "");

            if(!string.IsNullOrEmpty(fileName))
            {
                bool shouldContinue = true;

                Object assetAtPath = AssetDatabase.LoadAssetAtPath(fileName + ".asset", typeof(Object));
                if (assetAtPath != null)
                {
                    EditorUtility.DisplayDialog("Error: Export will overwriting another asset.",
                            "You are trying to overwrite another asset. This is not allowed with exporting composite categories", "Ok");

                    shouldContinue = false;
                }

                if(shouldContinue)
                {
                    AnimationModule animModule = ScriptableObject.CreateInstance<AnimationModule>();
                    AssetDatabase.CreateAsset(animModule, fileName + ".asset");
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(animModule));

                    animModule.CopyCompositeCategory(m_data, a_categoryId);
                }
            }
        }

        private void ExportToMirroredModule()
        {
            serializedObject.ApplyModifiedProperties();

            m_data.ValidateData();
            EditorUtility.SetDirty(m_data);
            AssetDatabase.SaveAssets();

            string startLocation = AssetDatabase.GetAssetPath(this).Replace(name + ".asset", "");
            
            string fileName = EditorUtility.SaveFilePanelInProject("Export Mirrored Module", 
                "MxMAnimationModule", "asset", "Export mirrored module as", startLocation).Replace(".asset", "");

            if (!string.IsNullOrEmpty(fileName))
            {
                bool shouldContinue = true;

                Object assetAtPath = AssetDatabase.LoadAssetAtPath(fileName + ".asset", typeof(Object));
                if (assetAtPath != null)
                {
                    if (EditorUtility.DisplayDialogComplex("Export will overwrite another asset.",
                            "Are you sure you want to do this? It cannot be undone!", "Yes", "No", "Cancel") != 0)
                    {
                        shouldContinue = false;
                    }
                }

                if (shouldContinue)
                {
                    AnimationModule animModule = ScriptableObject.CreateInstance<AnimationModule>();
                    AssetDatabase.CreateAsset(animModule, fileName + ".asset");
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(animModule));
                    
                    animModule.CopyModuleMirrored(m_data);
                }
            }
        }


    }//End of class: AnimationModuleInspector
}//End of namespace: MxMEditor
