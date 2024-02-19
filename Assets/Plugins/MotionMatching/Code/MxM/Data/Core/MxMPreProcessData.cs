// ============================================================================================
// File: MxMPreProcessData.cs
// 
// Authors:  Kenneth Claassen
// Date:     2017-11-05: Created this file.
// 
//     Contains a part of the 'MxMEditor' namespace for 'Unity Engine'.
// 
// Copyright (c) 2019 Kenneth Claassen. All rights reserved.
// ============================================================================================
using System.Collections.Generic;
using UnityEngine;
using MxM;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MxMEditor
{
    //============================================================================================
    /**
    *  @brief 
    *         
    *********************************************************************************************/
    [CreateAssetMenu(fileName = "MxMPreProcessData", menuName = "MxM/Core/Pre-Processor", order = 1)]
    public class MxMPreProcessData : ScriptableObject
    {
        [SerializeField] private GameObject m_targetPrefab = null;
        [SerializeField] private float m_poseInterval = 0.1f;
        [SerializeField] private List<float> m_trajectoryTimes = new List<float>();
        [SerializeField] private List<PoseJoint> m_poseJoints = new List<PoseJoint>();
        [SerializeField] private List<string> m_tagNames = null;
        [SerializeField] private List<string> m_favourTagNames = null;
        [SerializeField] private List<string> m_userTagNames = null;
        [SerializeField] private List<string> m_eventNames = null;
        [SerializeField] private bool m_getBonesByName = false;
        [SerializeField] private string m_lastSaveDirectory = "";
        [SerializeField] private MxMAnimData m_lastCreatedAnimData = null;
        [SerializeField] private bool m_hideSubAssets = true;
        [SerializeField] private EJointVelocityCalculationMethod m_jointVelocityGlobal = EJointVelocityCalculationMethod.BodyVelocityDependent;

        [SerializeField] private List<CompositeCategory> m_compositeCategories = null;

        //Override Modules
        [SerializeField] private MotionMatchConfigModule m_overrideConfigModule = null;
        [SerializeField] private TagNamingModule m_overrideTagModule = null;
        [SerializeField] private EventNamingModule m_overrideEventModule = null;
        [SerializeField] private List<AnimationModule> m_animModules = null;

        [SerializeField] private List<MxMAnimationIdleSet> m_animIdleSets = null;
        [SerializeField] private List<MxMBlendSpace> m_blendSpaces = null;

        [SerializeField] private MotionTimingPresets m_motionTimingPresets = null;

        [SerializeField] private bool m_embedAnimClipsInAnimData = false;
        [SerializeField] private bool m_generateModifiedClipsOnPreProcess = false;

        //Foldout states
        [SerializeField] private bool m_generalFoldout = true;
        [SerializeField] private bool m_trajectoryFoldout = true;
        [SerializeField] private bool m_poseFoldout = true;
        [SerializeField] private bool m_animationFoldout = true;
        [SerializeField] private bool m_animModuleFoldout = true;
        [SerializeField] private bool m_compositeFoldout = false;
        [SerializeField] private bool m_idleSetFoldout = false;
        [SerializeField] private bool m_blendSpaceFoldout = false;
        [SerializeField] private bool m_metaDataFoldout = true;
        [SerializeField] private bool m_tagsFoldout = false;
        [SerializeField] private bool m_favourTagsFoldout = false;
        [SerializeField] private bool m_userTagsFoldout = false;
        [SerializeField] private bool m_eventsFoldout = false;
        [SerializeField] private bool m_motionTimingFoldout = true;
        [SerializeField] private bool m_preProcessFoldout = true;

        public float PoseInterval { get { return m_poseInterval; } }
        public List<CompositeCategory> CompositeCategories { get { return m_compositeCategories; } }
        public List<MxMAnimationIdleSet> AnimationIdleSets { get { return m_animIdleSets; } }
        public List<MxMBlendSpace> BlendSpaces { get { return m_blendSpaces; } }
        public MotionTimingPresets MotionTimingPresets { get { return m_motionTimingPresets; } }
        public bool EmbedClips { get { return m_embedAnimClipsInAnimData; } }
        public bool GenerateModifiedClips { get { return m_generateModifiedClipsOnPreProcess; } }
        public EJointVelocityCalculationMethod UseGlobalJointVelocity { get { return m_jointVelocityGlobal; } }
        public MotionMatchConfigModule OverrideConfigModule { get { return m_overrideConfigModule; } set { m_overrideConfigModule = value; }}
        public TagNamingModule OverrideTagModule { get { return m_overrideTagModule; } set { m_overrideTagModule = value; }}
        public EventNamingModule OverrideEventModule { get { return m_overrideEventModule; } set { m_overrideEventModule = value; }}
        public List<AnimationModule> AnimationModules { get { return m_animModules; } }
        public MxMAnimData LastSavedAnimData { get { return m_lastCreatedAnimData; } set { m_lastCreatedAnimData = value; } }
        public string LastSaveDirectory { get { return m_lastSaveDirectory; } set { m_lastSaveDirectory = value; } }
        
        public bool GetBonesByName
        {
            get
            {
                if(m_overrideConfigModule == null)
                    return m_getBonesByName;

                return m_overrideConfigModule.GetBonesByName;
            }
        }

        public GameObject Prefab
        {
            get
            {
                if(m_overrideConfigModule == null)
                    return m_targetPrefab;

                return m_overrideConfigModule.Prefab;
            }

            set
            {
                m_targetPrefab = value;
            }
        }

        public List<PoseJoint> PoseJoints
        {
            get
            {
                if (m_overrideConfigModule == null)
                    return m_poseJoints;

                return m_overrideConfigModule.PoseJoints;
            }
        }

        public List<float> TrajectoryPoints
        {
            get
            {
                if (m_overrideConfigModule == null)
                    return m_trajectoryTimes;

                return m_overrideConfigModule.TrajectoryPoints;
            }
        }

        public List<string> TagNames
        {
            get
            {
                if(m_overrideTagModule == null)
                    return m_tagNames;

                return m_overrideTagModule.TagNames;
            }
        }

        public List<string> FavourTagNames
        {
            get
            {
                if (m_overrideTagModule == null)
                    return m_favourTagNames;

                return m_overrideTagModule.FavourTagNames;
            }
        }

        public List<string> UserTagNames
        {
            get
            {
                if (m_overrideTagModule == null)
                    return m_userTagNames;

                return m_overrideTagModule.UserTagNames;
            }
        }

        public List<string> EventNames
        {
            get
            {
                if(m_overrideEventModule == null)
                    return m_eventNames;

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

            if (m_tagNames == null || m_tagNames.Count != 32)
                InitTagList();

            if (m_favourTagNames == null || m_favourTagNames.Count != 32)
                InitFavourTagList();

            if (m_userTagNames == null || m_userTagNames.Count != 32)
                InitUserTagList();

            if (m_eventNames == null)
                m_eventNames = new List<string>();

            List<string> eventNames = m_eventNames;
            if (m_overrideEventModule != null)
                eventNames = m_overrideEventModule.EventNames;

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
        public string GetTagName(int a_id)
        {
            List<string> tagNames = m_tagNames;

            if (m_overrideTagModule != null)
                tagNames = m_overrideTagModule.TagNames;

            Mathf.Clamp(a_id, 0, tagNames.Count-1);

            if(tagNames.Count > 0)
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
            List<string> favourTagNames = m_favourTagNames;

            if (m_overrideTagModule != null)
                favourTagNames = m_overrideTagModule.FavourTagNames;

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
            List<string> userTagNames = m_userTagNames;

            if (m_overrideTagModule != null)
                userTagNames = m_overrideTagModule.UserTagNames;

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
            List<string> userTagNames = m_userTagNames;

            if (m_overrideTagModule != null)
                userTagNames = m_overrideTagModule.UserTagNames;

            for (int i = 0; i < userTagNames.Count; ++i)
            {
                if(userTagNames[i] == a_name)
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
        private void InitTagList()
        {
            if (m_tagNames == null)
                m_tagNames = new List<string>(33);

            m_tagNames.Clear();

            for (int i = 1; i < 31; ++i)
            {
                m_tagNames.Add("Tag " + 1.ToString());
            }

            m_tagNames.Add("DoNotUse");
            m_tagNames.Add("Reserved");
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void InitFavourTagList()
        {
            if (m_favourTagNames == null)
                m_favourTagNames = new List<string>(33);

            m_favourTagNames.Clear();

            for (int i = 1; i < 33; ++i)
            {
                m_favourTagNames.Add("Favour Tag " + i.ToString());
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void InitUserTagList()
        {
            if (m_userTagNames == null)
                m_userTagNames = new List<string>(33);

            m_userTagNames.Clear();

            for (int i = 1; i < 33; ++i)
            {
                m_userTagNames.Add("User Tag " + i.ToString());
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void GenerateModifiedAnimations(string _directory = "")
        {
            foreach(CompositeCategory category in m_compositeCategories)
            {
                foreach(MxMAnimationClipComposite composite in category.Composites)
                {
                    if(composite != null)
                    {
                        composite.GenerateModifiedAnimation(this, _directory);
                    }
                }
            }

            foreach(MxMBlendSpace blendSpace in m_blendSpaces)
            {
                if(blendSpace != null)
                    blendSpace.GenerateModifiedAnimation(this, _directory);
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
        public void ScrapModifiedAnimations()
        {
            foreach(CompositeCategory category in m_compositeCategories)
            {
                foreach(MxMAnimationClipComposite composite in category.Composites)
                {
                    if(composite != null)
                    {
                        composite.ScrapGeneratedClips();
                    }
                }
            }

            foreach (MxMBlendSpace blendSpace in m_blendSpaces)
            {
                if (blendSpace != null)
                    blendSpace.ScrapGeneratedClips();
            }
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
                    {
                        composite.VerifyData();
                    }
                }
            }

            foreach (MxMAnimationIdleSet idleSet in m_animIdleSets)
            {
                idleSet.VerifyData();
            }

            foreach (MxMBlendSpace blendSpace in m_blendSpaces)
            {
                blendSpace.VerifyData();
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void ValidateEventMarkers()
        {
            string[] eventNames;

            if(m_overrideEventModule == null)
                eventNames = m_eventNames.ToArray();
            else
                eventNames = m_overrideEventModule.EventNames.ToArray();

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
            foreach(CompositeCategory category in m_compositeCategories)
            {
                foreach(MxMAnimationClipComposite composite in category.Composites)
                {
                    if(!composite.CheckAnimationCompatibility(a_useGeneric))
                    {
                        return false;
                    }
                }
            }

            foreach(MxMAnimationIdleSet idleSet in m_animIdleSets)
            {
                if(!idleSet.CheckAnimationCompatibility(a_useGeneric))
                {
                    return false;
                }
            }

            foreach(MxMBlendSpace blendSpace in m_blendSpaces)
            {
                if(!blendSpace.CheckAnimationCompatibility(a_useGeneric))
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
            if(a_categoryId < m_compositeCategories.Count)
            {
                return m_compositeCategories[a_categoryId].Composites;
            }

            return null;
        }
        
        //============================================================================================
        /**
        *  @brief Adds an animation module to the pre-processor. This is used for pre-processing at
        * runtime. It will only be added if it is unique.
        *
        * @param [AnimationModule] a_animModule - the animation module to add
        * 
        *********************************************************************************************/
        public void AddAnimationModule(AnimationModule a_animModule)
        {
            if (a_animModule == null)
            {
                Debug.LogWarning("MxMPreProcessData [AddAnimationModule] - Trying to add a new animation" +
                                 "module but the module is null.");
                return;
            }
            
            if (m_animModules == null)
            {
                m_animModules = new List<AnimationModule>(5);
            }

            if (m_animModules.Contains(a_animModule))
            {
                Debug.Log("MxMPreProcessData [AddAnimationModule] - Trying to add an animation module" +
                          "but it has already been added. This module will not be added again.");
                return;
                
            }
            
            m_animModules.Add(a_animModule);
        }
        
        //============================================================================================
        /**
        *  @brief Adds animation modules to the pre-processor. This is used for pre-processing at
        * runtime. It will only be added if it is unique.
        *
        * @param [AnimationModule[]] a_animModules - an array animation modules to add
        * 
        *********************************************************************************************/
        public void AddAnimationModules(AnimationModule[] a_animModules)
        {
            if (a_animModules == null || a_animModules.Length == 0)
            {
                Debug.LogWarning("MxMPreProcessData [AddAnimationModules] - Trying to add a new animation " +
                                 "module list but the list is null or empty.");
                return;
            }

            if (m_animModules == null)
            {
                m_animModules = new List<AnimationModule>(5);
            }

            for (int i = 0; i < a_animModules.Length; ++i)
            {
                ref AnimationModule animModule = ref a_animModules[i];

                if (animModule == null)
                {
                    Debug.Log("MxMPreProcessData [AddAnimationModules] - Trying to add an animation module" +
                              "but it is null.");
                    continue;
                }

                if (m_animModules.Contains(animModule))
                {
                    Debug.Log("MxMPreProcessData [AddAnimationModule] - Trying to add an animation module" +
                              "but it has already been added. This module will not be added again.");
                    return;
                }
                
                m_animModules.Add(animModule);
            }
        }
        
         //============================================================================================
        /**
        *  @brief Pre-Processing animation data for motion matching at runtime. When doing this at
        *  runtime, generate modified clips is not supported and the anim data is not persistent (i.e.
        *  it is not saved)
        *
        *  @return MxMAnimData - the fully pre-processed animation data for runtime use 
        *         
        *********************************************************************************************/
        public MxMAnimData PreProcessAnimationDataRuntime()
        {
            MxMPreProcessor preProcessor = new MxMPreProcessor();
            preProcessor.SetupSceneForProcessing(this);

            MxMAnimData animData = ScriptableObject.CreateInstance<MxMAnimData>();
            preProcessor.PreProcessData(animData);

            //Todo: Set base calibration somehow
           // animData.InitializeCalibration(m_calibrationModule); 
           
           //Todo: Do we want to save the data somewhere/somehow?
           // AssetDatabase.CreateAsset(animData, _fileName + ".asset");

           return animData;
        }

        //============================================================================================
        /**
        *  @brief Used to stop 'Unused Warnings' where it is used for editor the editor
        *         
        *********************************************************************************************/
        private void StopUnusedWarnings()
        {
            if (m_hideSubAssets)
                m_hideSubAssets = true;

            if (m_generalFoldout)
                m_generalFoldout = true;

            if (m_trajectoryFoldout)
                m_trajectoryFoldout = true;

            if (m_poseFoldout)
                m_poseFoldout = true;

            if (m_animationFoldout)
                m_animationFoldout = true;

            if (m_compositeFoldout)
                m_compositeFoldout = true;

            if (m_idleSetFoldout)
                m_idleSetFoldout = true;

            if (m_blendSpaceFoldout)
                m_blendSpaceFoldout = true;

            if (m_metaDataFoldout)
                m_metaDataFoldout = true;

            if (m_tagsFoldout)
                m_tagsFoldout = true;

            if (m_favourTagsFoldout)
                m_favourTagsFoldout = true;

            if (m_userTagsFoldout)
                m_userTagsFoldout = true;

            if (m_eventsFoldout)
                m_eventsFoldout = true;

            if (m_motionTimingFoldout)
                m_motionTimingFoldout = true;

            if (m_preProcessFoldout)
                m_preProcessFoldout = true;

            if (m_animModuleFoldout)
                m_animModuleFoldout = true;
        }

    }//End of class: MxMPreProcessData
}//End of namespace: MxMEditor
