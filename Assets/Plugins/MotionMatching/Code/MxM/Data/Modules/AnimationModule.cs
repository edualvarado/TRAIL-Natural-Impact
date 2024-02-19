// ============================================================================================
// File: AnimationModule.cs
// 
// Authors:  Kenneth Claassen
// Date:     2020-02-25: Created this file.
// 
//     Contains a part of the 'MxMEditor' namespace for 'Unity Engine'.
// 
// Copyright (c) 2020 Kenneth Claassen. All rights reserved.
// ============================================================================================
using System.Collections.Generic;
using UnityEngine;
using MxM;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MxMEditor
{
    [CreateAssetMenu(fileName = "MxMAnimationModule", menuName = "MxM/Core/Modules/MxMAnimationModule", order = 1)]
    public class AnimationModule : ScriptableObject
    {
        [SerializeField] private bool m_hideSubAssets = true;
        [SerializeField] private MotionMatchConfigModule m_overrideConfigModule = null;
        [SerializeField] private TagNamingModule m_overrideTagModule = null;
        [SerializeField] private EventNamingModule m_overrideEventModule = null;
        [SerializeField] private AnimModuleDefaults m_animModuleDefaults = null;

        [SerializeField] private List<CompositeCategory> m_compositeCategories = null;
        [SerializeField] private List<MxMAnimationIdleSet> m_animIdleSets = null;
        [SerializeField] private List<MxMBlendSpace> m_blendSpaces = null;

        [SerializeField] private bool m_moduleFoldout = false;
        [SerializeField] private bool m_compositeFoldout = false;
        [SerializeField] private bool m_idleSetFoldout = false;
        [SerializeField] private bool m_blendSpaceFoldout = false;

        public List<CompositeCategory> CompositeCategories { get { return m_compositeCategories; } }
        public List<MxMAnimationIdleSet> AnimationIdleSets { get { return m_animIdleSets; } }
        public List<MxMBlendSpace> BlendSpaces { get { return m_blendSpaces; } }

        public MotionMatchConfigModule OverrideConfigModule { get { return m_overrideConfigModule; } }
        public TagNamingModule OverrideTagModule { get { return m_overrideTagModule; } }
        public EventNamingModule OverrideEventModule { get { return m_overrideEventModule; } }
        public AnimModuleDefaults DefaultSettings { get { return m_animModuleDefaults; } }

        public bool GetBonesByName
        {
            get
            {
                if (m_overrideConfigModule == null)
                    return false;

                return m_overrideConfigModule.GetBonesByName;
            }
        }

        public GameObject Prefab
        {
            get
            {
                if (m_overrideConfigModule == null)
                    return null;

                return m_overrideConfigModule.Prefab;
            }
        }

        public List<PoseJoint> PoseJoints
        {
            get
            {
                if (m_overrideConfigModule == null)
                    return null;

                return m_overrideConfigModule.PoseJoints;
            }
        }

        public List<float> TrajectoryPoints
        {
            get
            {
                if (m_overrideConfigModule == null)
                    return null;

                return m_overrideConfigModule.TrajectoryPoints;
            }
        }

        public List<string> TagNames
        {
            get
            {
                if (m_overrideTagModule == null)
                    return null;

                return m_overrideTagModule.TagNames;
            }
        }

        public List<string> FavourTagNames
        {
            get
            {
                if (m_overrideTagModule == null)
                    return null;

                return m_overrideTagModule.FavourTagNames;
            }
        }

        public List<string> UserTagNames
        {
            get
            {
                if (m_overrideTagModule == null)
                    return null;

                return m_overrideTagModule.UserTagNames;
            }
        }

        public List<string> EventNames
        {
            get
            {
                if (m_overrideEventModule == null)
                    return null;

                return m_overrideEventModule.EventNames;
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void OnEnable()
        {
            StopUnusedWarnings();

            if (m_compositeCategories == null)
                m_compositeCategories = new List<CompositeCategory>();

            if (m_compositeCategories.Count == 0)
                m_compositeCategories.Add(new CompositeCategory("Composite Category"));

            if (m_animIdleSets == null)
                m_animIdleSets = new List<MxMAnimationIdleSet>();

            if (m_blendSpaces == null)
                m_blendSpaces = new List<MxMBlendSpace>();
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void StopUnusedWarnings()
        {
            if (m_moduleFoldout)
                m_moduleFoldout = true;

            if (m_hideSubAssets)
                m_hideSubAssets = true;

            if (m_compositeFoldout)
                m_compositeFoldout = true;

            if (m_idleSetFoldout)
                m_idleSetFoldout = true;

            if (m_blendSpaceFoldout)
                m_blendSpaceFoldout = true;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public string GetTagName(int a_id)
        {
            if (m_overrideTagModule == null)
                return "Null";

            List<string> tagNames = m_overrideTagModule.TagNames;

            Mathf.Clamp(a_id, 0, tagNames.Count - 1);

            if (tagNames.Count > 0)
                return tagNames[a_id];
            else
                return "Null";
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public string GetFavourTagName(int a_id)
        {
            if (m_overrideTagModule != null)
                return "Null";

            List<string> favourTagNames = m_overrideTagModule.FavourTagNames;

            Mathf.Clamp(a_id, 0, favourTagNames.Count - 1);

            if (favourTagNames.Count > 0)
                return favourTagNames[a_id];
            else
                return "Null";
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public string GetUserTagName(int a_id)
        {
            if (m_overrideTagModule != null)
                return "Null";

            List<string> userTagNames = m_overrideTagModule.UserTagNames;

            Mathf.Clamp(a_id, 0, userTagNames.Count - 1);

            if (userTagNames.Count > 0)
                return userTagNames[a_id];
            else
                return "Null";
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public EUserTags GetUserTagId(string a_name)
        {
            if (m_overrideTagModule != null)
                return EUserTags.None;

            List<string> userTagNames = m_overrideTagModule.UserTagNames;

            for (int i = 0; i < userTagNames.Count; ++i)
            {
                if (userTagNames[i] == a_name)
                {
                    return (EUserTags)(1 << i);
                }
            }

            return EUserTags.None;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void ValidateData()
        {
            ValidateEventMarkers();

            foreach (CompositeCategory category in m_compositeCategories)
            {
                foreach (MxMAnimationClipComposite composite in category.Composites)
                {
                    if (composite != null)
                        composite.VerifyData();
                }
            }

            foreach (MxMAnimationIdleSet idleSet in m_animIdleSets)
                idleSet.VerifyData();

            foreach (MxMBlendSpace blendSpace in m_blendSpaces)
                blendSpace.VerifyData();
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void ValidateEventMarkers()
        {
            if (m_overrideEventModule == null)
                return;

           string[] eventNames = m_overrideEventModule.EventNames.ToArray();

            foreach (CompositeCategory category in m_compositeCategories)
            {
                foreach (MxMAnimationClipComposite composite in category.Composites)
                {
                    if (composite != null)
                    {
                        composite.VerifyEventMarkers(eventNames);
                    }
                }
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public bool CheckAnimationCompatibility(bool a_useGeneric)
        {
            foreach (CompositeCategory category in m_compositeCategories)
            {
                foreach (MxMAnimationClipComposite composite in category.Composites)
                {
                    if (!composite.CheckAnimationCompatibility(a_useGeneric))
                    {
                        return false;
                    }
                }
            }

            foreach (MxMAnimationIdleSet idleSet in m_animIdleSets)
            {
                if (!idleSet.CheckAnimationCompatibility(a_useGeneric))
                {
                    return false;
                }
            }

            foreach (MxMBlendSpace blendSpace in m_blendSpaces)
            {
                if (!blendSpace.CheckAnimationCompatibility(a_useGeneric))
                {
                    return false;
                }
            }

            return true;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public List<MxMAnimationClipComposite> GetCompositeCategoryList(int a_categoryId)
        {
            if (a_categoryId < m_compositeCategories.Count)
            {
                return m_compositeCategories[a_categoryId].Composites;
            }

            return null;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void CopyCompositeCategory(AnimationModule a_animModule, int a_compositeCategoryId)
        {
            if (a_animModule == null)
                return;

            m_overrideTagModule = a_animModule.OverrideTagModule;
            m_overrideEventModule = a_animModule.OverrideEventModule;
            m_overrideConfigModule = a_animModule.OverrideConfigModule;

            CompositeCategory sourceCategory = a_animModule.CompositeCategories[a_compositeCategoryId];

            CompositeCategory newCategory = new CompositeCategory(sourceCategory, this);
            m_compositeCategories.Add(newCategory);

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
        
        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void CopyModuleMirrored(AnimationModule a_animModule)
        {
            if (a_animModule == null)
                return;
            
            m_overrideTagModule = a_animModule.OverrideTagModule;
            m_overrideEventModule = a_animModule.OverrideEventModule;
            m_overrideConfigModule = a_animModule.OverrideConfigModule;

            //Copy composite categories
            for (int i = 0; i < a_animModule.CompositeCategories.Count; ++i)
            {
                CompositeCategory sourceCategory = a_animModule.CompositeCategories[i];

                if (sourceCategory == null)
                    continue;

                CompositeCategory newCategory = new CompositeCategory(sourceCategory, this, /*mirrored*/true);
                m_compositeCategories.Add(newCategory);
            }
            
            //Copy idle sets
            for (int i = 0; i < a_animModule.m_animIdleSets.Count; ++i)
            {
                MxMAnimationIdleSet sourceIdleSet = a_animModule.m_animIdleSets[i];

                if (sourceIdleSet == null)
                    continue;


                MxMAnimationIdleSet newIdleSet = ScriptableObject.CreateInstance<MxMAnimationIdleSet>();
                newIdleSet.CopyData(sourceIdleSet, /*mirrored*/true);
                m_animIdleSets.Add(newIdleSet);
            }
            
            //Copy blend spaces
            for (int i = 0; i < a_animModule.m_blendSpaces.Count; ++i)
            {
                MxMBlendSpace sourceBlendSpace = a_animModule.m_blendSpaces[i];

                if (sourceBlendSpace == null)
                    continue;

                MxMBlendSpace newBlendSpace = ScriptableObject.CreateInstance<MxMBlendSpace>();
                newBlendSpace.CopyData(sourceBlendSpace, /*mirrored*/true);
                m_blendSpaces.Add(newBlendSpace);
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void CopyCompositeCategory(MxMPreProcessData a_preProcessData, int a_compositeCategoryId)
        {
            if (a_preProcessData == null)
                return;

            m_overrideTagModule = a_preProcessData.OverrideTagModule;
            m_overrideEventModule = a_preProcessData.OverrideEventModule;
            m_overrideConfigModule = a_preProcessData.OverrideConfigModule;

            CompositeCategory sourceCategory = a_preProcessData.CompositeCategories[a_compositeCategoryId];

            CompositeCategory newCategory = new CompositeCategory(sourceCategory, this);
            m_compositeCategories.Add(newCategory);

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void CopyPreProcessData(MxMPreProcessData a_preProcessData)
        {
            if (a_preProcessData == null)
                return;

            m_overrideTagModule = a_preProcessData.OverrideTagModule;
            m_overrideEventModule = a_preProcessData.OverrideEventModule;
            m_overrideConfigModule = a_preProcessData.OverrideConfigModule;

            List<CompositeCategory> sourceCategories = a_preProcessData.CompositeCategories;
            m_compositeCategories = new List<CompositeCategory>(sourceCategories.Count + 1);

            foreach (CompositeCategory sourceCategory in sourceCategories)
            {
                CompositeCategory newCategory = new CompositeCategory(sourceCategory, this);
                m_compositeCategories.Add(newCategory);
            }

            List<MxMAnimationIdleSet> sourceIdleSets = a_preProcessData.AnimationIdleSets;
            m_animIdleSets = new List<MxMAnimationIdleSet>(sourceIdleSets.Count + 1);

            foreach(MxMAnimationIdleSet sourceIdleSet in sourceIdleSets)
            {
                MxMAnimationIdleSet newIdleSet = ScriptableObject.CreateInstance<MxMAnimationIdleSet>();
                newIdleSet.CopyData(sourceIdleSet);
                newIdleSet.name = sourceIdleSet.name;
                newIdleSet.hideFlags = HideFlags.HideInHierarchy;
                newIdleSet.TargetAnimModule = this;
                newIdleSet.TargetPreProcess = null;

#if UNITY_EDITOR
                EditorUtility.SetDirty(newIdleSet);
                AssetDatabase.AddObjectToAsset(newIdleSet, this);
#endif
                m_animIdleSets.Add(newIdleSet);
            }

            List<MxMBlendSpace> sourceBlendSpaces = a_preProcessData.BlendSpaces;
            m_blendSpaces = new List<MxMBlendSpace>(sourceBlendSpaces.Count + 1);

            foreach(MxMBlendSpace sourceBlendSpace in sourceBlendSpaces)
            {
                MxMBlendSpace newBlendSpace = ScriptableObject.CreateInstance<MxMBlendSpace>();
                newBlendSpace.CopyData(sourceBlendSpace);
                newBlendSpace.name = sourceBlendSpace.name;
                newBlendSpace.hideFlags = HideFlags.HideInHierarchy;
                newBlendSpace.TargetAnimModule = this;
                newBlendSpace.TargetPreProcess = null;

#if UNITY_EDITOR
                EditorUtility.SetDirty(newBlendSpace);
                AssetDatabase.AddObjectToAsset(newBlendSpace, this);
#endif

                m_blendSpaces.Add(newBlendSpace);
            }
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public bool AreMxMAnimsValid()
        {
            foreach (CompositeCategory category in m_compositeCategories)
            {
                if (category != null && category.Composites != null)
                {
                    foreach (MxMAnimationClipComposite composite in category.Composites)
                    {
                        if (composite == null)
                            return false;

                        if (composite.PrimaryClip == null)
                        {
#if UNITY_EDITOR
                            EditorUtility.DisplayDialog("Error: Empty Composite", "You have a composite with no animations in it. Anim Module: " 
                                + name + " Category: " + category.CatagoryName + " Composite Name: " + composite.CompositeName
                                + ". Please add an animation or remove the composite before pre-processing", "Ok");

                            EditorGUIUtility.PingObject(composite);
#endif
                            return false;
                        }
                    }
                }
            }

            if (m_animIdleSets != null)
            {
                foreach (MxMAnimationIdleSet idleSet in m_animIdleSets)
                {
                    if (idleSet == null)
                    {
                        return false;
                    }

                    if (idleSet.PrimaryClip == null)
                    {
#if UNITY_EDITOR
                        EditorUtility.DisplayDialog("Error: Empty Idle Set", "You have a IdleSet with no animations in it. Anim Module: " 
                            + name + ". Please add an animation or remove the idle set before pre-processing", "Ok");

                        EditorGUIUtility.PingObject(idleSet);
#endif
                        return false;
                    }
                }
            }

            if (m_blendSpaces != null)
            {
                foreach (MxMBlendSpace blendSpace in m_blendSpaces)
                {
                    if (blendSpace == null)
                        return false;

                    List<AnimationClip> clips = blendSpace.Clips;

                    if (clips == null || clips.Count == 0)
                    {
#if UNITY_EDITOR
                        EditorUtility.DisplayDialog("Error: Empty blend space", "You have a blendspace with no animations in it. Anim Module: " 
                            + name + " Blendspace Name: " + blendSpace.BlendSpaceName
                            + ". Please add an animation or remove the blendspace before pre-processing", "Ok");

                        EditorGUIUtility.PingObject(blendSpace);
#endif
                        return false;
                    }

                    if (clips[0] == null)
                    {
#if UNITY_EDITOR
                        EditorUtility.DisplayDialog("Error: Empty blend space", "You have a blendspace with no animations in it. Anim Module: " 
                            + name + " Blendspace Name: " + blendSpace.BlendSpaceName
                            + ". Please add an animation or remove the blendspace before pre-processing", "Ok");

                        EditorGUIUtility.PingObject(blendSpace);
#endif
                        return false;
                    }
                }
            }

            return true;
        }

    }//End of class: AnimationModule
}//End of namespace: MxM