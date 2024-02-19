// ============================================================================================
// File: MxMPreProcessor.cs
// 
// Authors:  Kenneth Claassen
// Date:     2019-02-17: Created this file.
// 
//     Contains a part of the 'MxMEditor' namespace for 'Unity Engine'.
// 
// ============================================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Playables;
using UnityEngine.Animations;
using MxM;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MxMEditor
{
    //============================================================================================
    /**
    *  @brief This class is dedicated to running the pre-processing logic of the MxMPreProcessData
    *  asset when 'the Pre-Process Anim Data' button is pressed in the MxMPreProcessDataInspector
    *  
    *********************************************************************************************/
    public class MxMPreProcessor
    {
        private MxMPreProcessData m_data;
        private MxMAnimData m_processedData;

        private GameObject m_target;
        private Animator m_animator;

        private PlayableGraph m_playableGraph;
        private AnimationMixerPlayable m_animationMixer;

        private List<Transform> m_matchPropertyBones;

        private List<MxMAnimationClipComposite> m_animCompositeList;
        private List<MxMAnimationClipComposite> m_singleCompositeList;
        private List<MxMAnimationIdleSet> m_animIdleSetList;
        private List<MxMBlendSpace> m_animBlendSpaceList;
        private List<MxMBlendClip> m_animBlendClipList;
        private List<AnimationClip> m_clips = new List<AnimationClip>();
        private List<FootstepTagTrackData> m_leftFootStepTrackData;
        private List<FootstepTagTrackData> m_rightFootStepTrackData;
        private List<PoseData> m_poseList;
        private List<EventData> m_eventList;
        private List<IdleSetData> m_idleSetList;
        private List<BlendSpaceData> m_blendSpaceList;
        private List<ClipData> m_clipDataList;
        private List<BlendClipData> m_blendClipDataList;
        private List<CompositeData> m_compositeDataList;
        private List<IMxMAnim> m_animList;
        private List<string> m_curveBindings;

        //Modules
        private List<CompositeCategory> m_allCompositeCategories;
        private List<MxMBlendSpace> m_allBlendSpaces;

        //Events and Tags
        private List<string> m_combinedEventNames;
        private List<string> m_combinedIdleTraits;

        private Transform m_helperTransform;

        //private const float ThirtyHz = 1f / 30f;
        private const float SixtyHz = 1f / 60f;

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void SetupSceneForProcessing(MxMPreProcessData _data)
        {
            m_data = _data;
            m_target = GameObject.Instantiate(m_data.Prefab);
            m_animator = m_target.GetComponent<Animator>();
            m_target.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            m_target.name = "MxMTargetPrefab";

            Assert.IsNotNull(m_animator, "Cannot setup MxMPreProcessor with null Animator (m_animator)");
            m_animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            m_helperTransform = new GameObject().transform;
        }

        //============================================================================================
        // ReSharper disable Unity.PerformanceAnalysis
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void PreProcessData(MxMAnimData _animData)
        {
#if UNITY_EDITOR
            EditorUtility.DisplayProgressBar("Pre-Processing", "Start", 0f);
#endif
            Initialize(_animData);
            SetupPlayableGraph();
            PrepareDataContainers();
            RunSimulation();
            GeneratePoseSequencing();


#if UNITY_EDITOR
            EditorUtility.DisplayProgressBar("Pre-Processing", "Start", 1f);
#endif
            m_playableGraph.Destroy();
            GameObject.DestroyImmediate(m_target);
            GameObject.DestroyImmediate(m_helperTransform.gameObject);

#if UNITY_EDITOR
            EditorUtility.ClearProgressBar();
#endif
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void Initialize(MxMAnimData a_animData)
        {
            Assert.IsNotNull(a_animData, "Cannot Pre-Process MxM data with null MxMAnimData (_animData)");
            CombineIdleSetTraits();

            m_processedData = a_animData;

            m_processedData.ResetAllData();

            m_processedData.PoseInterval = m_data.PoseInterval;
            m_processedData.PosePredictionTimes = m_data.TrajectoryPoints.ToArray();
            m_processedData.GetBonesByName = m_data.GetBonesByName;

            m_processedData.MatchBones = new HumanBodyBones[m_data.PoseJoints.Count];
            m_processedData.MatchBonesGeneric = new string[m_data.PoseJoints.Count];
            m_processedData.IdleTraits = m_combinedIdleTraits.ToArray();   //new string[m_combinedIdleTraits.Count];

            for (int i = 0; i < m_data.PoseJoints.Count; ++i)
            {
                m_processedData.MatchBones[i] = m_data.PoseJoints[i].BoneId;

                string boneName = m_data.PoseJoints[i].BoneName;

               while (true)
                {
                    int index = boneName.IndexOf('/');
                    if (index == -1)
                    {
                        break;
                    }
                    else
                    {
                        boneName = boneName.Substring(index + 1 );
                    }
                }

                m_processedData.MatchBonesGeneric[i] = boneName;
            }

            //Find all the bones for match joints
            m_matchPropertyBones = new List<Transform>();
            List<PoseJoint> poseProperties = m_data.PoseJoints;
            for (int i = 0; i < poseProperties.Count; ++i)
            {
                if (m_data.GetBonesByName)
                {
                    Transform bone = m_target.transform.Find(poseProperties[i].BoneName);

                    if (bone != null)
                    {
                        m_matchPropertyBones.Add(m_target.transform.Find(poseProperties[i].BoneName));
                    }
                    else
                    {
#if UNITY_EDITOR
                        EditorUtility.DisplayDialog("Error", "Could not find bone: " + poseProperties[i].BoneName, "Ok");
#endif
                        Assert.IsTrue(true, "Could not find bone: " + poseProperties[i].BoneName);
                    }
                }
                else
                {
                    m_matchPropertyBones.Add(m_animator.GetBoneTransform(poseProperties[i].BoneId));
                }
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void SetupPlayableGraph()
        {
            m_playableGraph = PlayableGraph.Create();
            m_playableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);

            var playableOutput = AnimationPlayableOutput.Create(m_playableGraph, "Animation", m_animator);
            m_animationMixer = AnimationMixerPlayable.Create(m_playableGraph, 1);
            playableOutput.SetSourcePlayable(m_animationMixer);
            m_animationMixer.SetInputWeight(0, 1f);
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void PrepareDataContainers()
        {
            CombineEventNamingModules();

            //First combine all animation modules.
            m_allCompositeCategories = new List<CompositeCategory>(10);
            m_allBlendSpaces = new List<MxMBlendSpace>(10);
            List<MxMAnimationIdleSet> allIdleSets = new List<MxMAnimationIdleSet>(3);

            m_allCompositeCategories.AddRange(m_data.CompositeCategories);
            m_allBlendSpaces.AddRange(m_data.BlendSpaces);
            allIdleSets.AddRange(m_data.AnimationIdleSets);
            foreach(AnimationModule animModule in m_data.AnimationModules)
            {
                if (animModule == null)
                    continue;

                m_allCompositeCategories.AddRange(animModule.CompositeCategories);
                m_allBlendSpaces.AddRange(animModule.BlendSpaces);
                allIdleSets.AddRange(animModule.AnimationIdleSets);
            }

            m_animCompositeList = new List<MxMAnimationClipComposite>();
            m_singleCompositeList = new List<MxMAnimationClipComposite>();
            m_curveBindings = new List<string>(5);

            int mxmTrackId = 0;
            foreach(CompositeCategory category in m_allCompositeCategories)
            {
                foreach (MxMAnimationClipComposite composite in category.Composites)
                {
                    if (composite.AfterClips == null || composite.AfterClips.Count == 0)
                    {
                        m_singleCompositeList.Add(composite);
                    }
                    else if (!composite.RuntimeSplicing)
                    {
                        m_singleCompositeList.Add(composite);
                    }
                    else
                    {
                        m_animCompositeList.Add(composite);
                    }

                    //Register curve bindings for composite
                    foreach (MxMCurveTrack curveTrack in composite.Curves)
                    {
                        AddCurveBindingIfItDoesNotExist(curveTrack.CurveName);
                    }

                    composite.SetTrackId(mxmTrackId);
                    ++mxmTrackId;
                }
            }

            m_animBlendSpaceList = new List<MxMBlendSpace>();
            m_animBlendClipList = new List<MxMBlendClip>();

            foreach(MxMBlendSpace blendSpace in m_allBlendSpaces)
            {
                switch(blendSpace.ScatterSpace)
                {
                    case EBlendSpaceType.Standard: { m_animBlendSpaceList.Add(blendSpace); } break;
                    case EBlendSpaceType.Scatter:
                    case EBlendSpaceType.ScatterX:
                    case EBlendSpaceType.ScatterY:
                        {
                            List<Vector2> scatterPositions = blendSpace.CalculateScatterPositions();

                            foreach (Vector2 position in scatterPositions)
                            {
                                m_animBlendClipList.Add(new MxMBlendClip(blendSpace, position));
                            }
                        }
                        break;
                }

                //Register curve bindings for blend space
                foreach (MxMCurveTrack curveTrack in blendSpace.Curves)
                {
                    AddCurveBindingIfItDoesNotExist(curveTrack.CurveName);
                }

                blendSpace.SetTrackId(mxmTrackId);
                ++mxmTrackId;
            }

            m_animIdleSetList = allIdleSets;
            m_clips = new List<AnimationClip>(20);
            m_leftFootStepTrackData = new List<FootstepTagTrackData>(25);
            m_rightFootStepTrackData = new List<FootstepTagTrackData>(25);
            m_poseList = new List<PoseData>(1000);
            m_eventList = new List<EventData>(20);
            m_idleSetList = new List<IdleSetData>(5);
            m_blendSpaceList = new List<BlendSpaceData>(10);
            m_clipDataList = new List<ClipData>(20);
            m_blendClipDataList = new List<BlendClipData>(15);
            m_compositeDataList = new List<CompositeData>(15);
            m_animList = new List<IMxMAnim>(30);

            foreach (MxMAnimationClipComposite comp in m_singleCompositeList)
                m_animList.Add(comp);

            foreach (MxMAnimationClipComposite comp in m_animCompositeList)
                m_animList.Add(comp);

            m_processedData.BlendSpaceNames = new string[m_animBlendSpaceList.Count];
            for(int i = 0; i < m_animBlendSpaceList.Count; ++i)
            {
                MxMBlendSpace blendSpace = m_animBlendSpaceList[i];

                m_processedData.BlendSpaceNames[i] = blendSpace.BlendSpaceName;
                m_animList.Add(blendSpace);
            }

            foreach (MxMBlendClip blendClip in m_animBlendClipList)
                m_animList.Add(blendClip);

            foreach(IMxMAnim anim in m_animList)
                anim.VerifyData();
            
            Assert.IsTrue(m_animList.Count > 0, "Cannot pre-process animations as there are no animation");
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void RunSimulation()
        {
            float firstTrajtime = m_data.TrajectoryPoints[0];
            float lastTrajTime = m_data.TrajectoryPoints[m_data.TrajectoryPoints.Count - 1];

            int count = 0;
            for(int i=0; i < m_animList.Count; ++i)
            {
                m_target.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

                IMxMAnim curMxMAnim = m_animList[i];

                if (curMxMAnim == null)
                    continue;

                AnimationClip targetClip = curMxMAnim.FinalClip;

                if (targetClip == null)
                    continue;

#if UNITY_EDITOR
                EditorUtility.DisplayProgressBar("Pre-Processing", "Processing Clip: "
                    + targetClip.name, (float)i / (float)m_animList.Count);
#endif

                //Add clip to clip list if necessary 
                bool addClipToList = true;
                int curClipId = 0;
                for (int k = 0; k < m_clips.Count; ++k)
                {
                    if (ReferenceEquals(targetClip, m_clips[k]))
                    {
                        curClipId = k;
                        addClipToList = false;
                        break;
                    }
                }

                if (addClipToList)
                {
                    curClipId = m_clips.Count;
                    m_clips.Add(targetClip);
                }

                List<AnimationClip> beforeClips = curMxMAnim.AnimBeforeClips;
                List<AnimationClip> afterClips = curMxMAnim.AnimAfterClips;

                if (beforeClips == null)
                    beforeClips = new List<AnimationClip>();

                if (afterClips == null)
                    afterClips = new List<AnimationClip>();

                //Generate a list of clips to play for trajectories of this node
                var playlistData = GenerateClipPlaylist(curMxMAnim);

                List<AnimationClip> contClipList = playlistData.Item1;
                float startTime = playlistData.Item2;
                float endTime = playlistData.Item3;
                int targetClipId = playlistData.Item4;
                
                if (contClipList.Count == 0)
                    continue;

                curMxMAnim.InitPoseDataList();
                List<GlobalSpacePose> globalPoses = new List<GlobalSpacePose>();

                //GLOBAL PASS
                float globalPoseInterval = m_data.PoseInterval / 2f;
                float cumTime = 0f;
                float lastClipTimeRemainder = 0f;

                if (curMxMAnim.IsLooping || beforeClips.Count > 0)
                    cumTime = firstTrajtime;
                
                float clipSpeed = Mathf.Max(0.05f, curMxMAnim.PlaybackSpeed);

                for (int k=0; k < contClipList.Count; ++k)
                {
                    AnimationClip curClip = contClipList[k];
                    float thisClipEndTime = curClip.length;
                    float thisClipStartTime = 0f;

                    if (k == 0)
                    {
                        thisClipStartTime = startTime;
                    }
                    else
                    {
                        if (k == contClipList.Count - 1)
                            thisClipEndTime = endTime;

                        thisClipStartTime = (globalPoseInterval * clipSpeed) - lastClipTimeRemainder;
                    }

                    UTIL.PlayableUtils.DestroyChildPlayables(m_animationMixer);

                    if (curMxMAnim.AnimType == EMxMAnimtype.BlendClip)
                    {
                        MxMBlendClip blendClip = curMxMAnim as MxMBlendClip;
                        blendClip.SetupPlayables(ref m_playableGraph, ref m_animationMixer, thisClipStartTime);
                        thisClipEndTime = blendClip.NormalizedLength;
                    }
                    else
                    {
                        var clipPlayable = m_animationMixer.GetInput(0);
                        clipPlayable = AnimationClipPlayable.Create(m_playableGraph, curClip);
                        ((AnimationClipPlayable)clipPlayable).SetApplyFootIK(true);
                        clipPlayable.SetTime(Mathf.Clamp(thisClipStartTime, 0f, curClip.length));
                        clipPlayable.SetTime(Mathf.Clamp(thisClipStartTime, 0f, curClip.length));

                        m_playableGraph.Connect(clipPlayable, 0, m_animationMixer, 0);
                        m_animationMixer.SetInputWeight(0, 1f);
                    }

                    m_playableGraph.Evaluate(0f);

                    lastClipTimeRemainder = thisClipEndTime;
                    for (float time = thisClipStartTime; time <= thisClipEndTime; time += globalPoseInterval * clipSpeed)
                    {
                        GlobalSpacePose newPose = new GlobalSpacePose();

                        newPose.Position = m_target.transform.position;
                        newPose.Rotation = m_target.transform.rotation;
                        newPose.Forward = m_target.transform.forward;

                        newPose.ClipId = curClipId;
                        newPose.Time = cumTime;
                        cumTime += globalPoseInterval * clipSpeed;

                        
                        if (!curMxMAnim.IsLooping && curMxMAnim.AnimType == EMxMAnimtype.Composite)
                        {
                            if (k < targetClipId || k > targetClipId + 1)
                            {
                                newPose.TrajectoryOnly = true;
                            }
                        }
                        else
                        {
                            if (k != targetClipId)
                                newPose.TrajectoryOnly = true;
                        }

                        foreach (Transform joint in m_matchPropertyBones)
                            newPose.JointPositions.Add(joint.position);

                        globalPoses.Add(newPose);
                        m_playableGraph.Evaluate(globalPoseInterval * clipSpeed);

                        lastClipTimeRemainder = thisClipEndTime - time;
                    }
                }

                //LOCAL PASS
                for (int k = 0; k < globalPoses.Count; k += 2)
                {
                    if (globalPoses[k].TrajectoryOnly)
                        continue;

                    if (k >= globalPoses.Count)
                        k = globalPoses.Count - 1;

                    GlobalSpacePose globalPose = globalPoses[k];
                    GlobalSpacePose globalPoseMinus = globalPoses[Mathf.Max(0, k - 1)];
                    GlobalSpacePose globalPosePlus = globalPoses[Mathf.Min(k + 1, globalPoses.Count - 1)];
                    float timeDelta = (globalPosePlus.Time - globalPose.Time) / clipSpeed;

                    //Mark fringe poses as DoNotUse
                    ETags tags = ETags.None;
                    if (curMxMAnim.UseIgnoreEdges && !curMxMAnim.IsLooping)
                    {
                        if (beforeClips.Count == 0)
                        {
                            if (globalPose.Time < Mathf.Abs(firstTrajtime))
                                tags = ETags.DoNotUse;
                        }

                        if (afterClips.Count == 0)//Todo: What about composites?
                        {
                            if (globalPose.Time > targetClip.length - lastTrajTime) 
                                tags = ETags.DoNotUse;
                        }
                    }

                    if (!curMxMAnim.IsLooping)
                    {
                        float timeCuttoff = m_data.PoseInterval * 3f;

                        if (curMxMAnim.AnimType == EMxMAnimtype.Composite)
                        {
                            if (afterClips != null && afterClips.Count > 0)
                            {
                                timeCuttoff -= afterClips[0].length;
                            }
                        }

                        if (globalPose.Time > targetClip.length - timeCuttoff) //The last 2 poses in a clip that is not looping should not be used.
                            tags = ETags.DoNotUse;
                    }

                    m_helperTransform.SetPositionAndRotation(globalPose.Position, globalPose.Rotation);

                    Vector3 localVelocity = Vector3.zero;

                    if (timeDelta > Mathf.Epsilon)
                    {
                        localVelocity = m_helperTransform.InverseTransformVector(
                        globalPosePlus.Position - globalPose.Position) / timeDelta;
                    }

                    if(localVelocity.magnitude < 0.0001f)
                        localVelocity = m_helperTransform.forward * 0.0001f;


                    //PreProcess tags for this pose
                    ETags favourTags = ETags.None;
                    if (!Tags.HasTags(tags, ETags.DoNotUse))
                    {
                        tags = Tags.SetTag(tags, curMxMAnim.AnimGlobalTags);

                        List<TagTrack> tagTracks = curMxMAnim.FinalTagTracks;

                        if (tagTracks != null)
                        {
                            foreach (TagTrack track in tagTracks)
                            {
                                if (track.IsTimeTagged(globalPose.Time))
                                {
                                    tags = Tags.SetTag(tags, track.TagId);
                                }
                            }
                        }

                        //PreProcess favour tags for this pose
                        List<TagTrack> favourTagTracks = curMxMAnim.FinalFavourTagTracks;

                        favourTags = Tags.SetTag(favourTags, curMxMAnim.AnimGlobalFavourTags);

                        if (favourTagTracks != null)
                        {
                            foreach(TagTrack favourTrack in favourTagTracks)
                            {
                                if(favourTrack.IsTimeTagged(globalPose.Time))
                                {
                                    favourTags = Tags.SetTag(favourTags, favourTrack.TagId);
                                }
                            }
                        }
                    }

					//PreProcess root motion tags
                    List<TagTrackBase> genericTagTracks = curMxMAnim.FinalGenericTagTracks;
                    TagTrackBase rootMotionTrack = genericTagTracks[4];
                    EGenericTags genericTags = EGenericTags.None;
                    if (rootMotionTrack != null)
                    {
                        if(rootMotionTrack.IsTimeTagged(globalPose.Time))
                            genericTags = genericTags | EGenericTags.DisableMatching;
                    }

                    //PreProcess angular trajectory warping tags
                    TagTrackBase trajWarpLat = genericTagTracks[6];
                    if(trajWarpLat != null)
                    {
                        if (trajWarpLat.IsTimeTagged(globalPose.Time))
                            genericTags = genericTags | EGenericTags.DisableWarp_TrajLat;
                    }

                    //PreProcess longitudinal trajectory warping tags
                    TagTrackBase trajWarpLong = genericTagTracks[7];
                    if(trajWarpLong != null)
                    {
                        if (trajWarpLong.IsTimeTagged(globalPose.Time))
                            genericTags = genericTags | EGenericTags.DisableWarp_TrajLong;
                    }

                    //PreProcess favour tags
                    FloatTagTrack poseFavourTrack = genericTagTracks[5] as FloatTagTrack;
                    float poseFavour = 1f;
                    if(poseFavourTrack != null)
                    {
                        if(poseFavourTrack.IsTimeTagged(globalPose.Time))
                        {
                            poseFavour = poseFavourTrack.GetTagValue(globalPose.Time);
                        }
                    }

                    //PreProcess user tags
                    List<TagTrackBase> userTagTracks = curMxMAnim.FinalUserTagTracks;
                    EUserTags userTags = EUserTags.None;

                    if (userTagTracks != null)
                    {
                        for (int j = 0; j < userTagTracks.Count; ++j)
                        {
                            TagTrackBase userTrack = userTagTracks[j];

                            if (userTrack.IsTimeTagged(globalPose.Time))
                            {
                                userTags = userTags | m_data.GetUserTagId(userTrack.Name);
                            }
                        }
                    }

                    PoseData newPose = new PoseData(count, globalPose.ClipId, curMxMAnim.GetTrackId(), globalPose.Time,
                         localVelocity, m_data.PoseJoints.Count, m_data.TrajectoryPoints.Count,
                          poseFavour, tags, favourTags, genericTags, userTags, curMxMAnim.AnimType); ;

                    switch(curMxMAnim.AnimType)
                    {
                        //Todo: Search generic IMxMAnim lists instead and avoid this switch
                        case EMxMAnimtype.Composite: //Choose between clip and composite
                            {
                                MxMAnimationClipComposite composite = curMxMAnim as MxMAnimationClipComposite;

                                for (int j = 0; j < m_animCompositeList.Count; ++j)
                                {
                                    if (composite == m_animCompositeList[j])
                                    {
                                        newPose.AnimId = j;
                                        break;
                                    }
                                }
                            }
                            break;
                        case EMxMAnimtype.BlendSpace:
                            {
                                MxMBlendSpace blendSpace = curMxMAnim as MxMBlendSpace;

                                for (int j = 0; j < m_animBlendSpaceList.Count; ++j)
                                {
                                    if (blendSpace == m_animBlendSpaceList[j])
                                    {
                                        newPose.AnimId = j;
                                        break;
                                    }
                                }
                            }
                            break;
                        case EMxMAnimtype.Clip:
                            {
                                MxMAnimationClipComposite composite = curMxMAnim as MxMAnimationClipComposite;

                                for (int j = 0; j < m_singleCompositeList.Count; ++j)
                                {
                                    if (composite == m_singleCompositeList[j])
                                    {
                                        newPose.AnimId = j;
                                        break;
                                    }
                                }
                            }
                            break;
                        case EMxMAnimtype.BlendClip:
                            {
                                MxMBlendClip blendClip = curMxMAnim as MxMBlendClip;

                                for(int j = 0; j < m_animBlendClipList.Count; ++j)
                                {
                                    if(blendClip == m_animBlendClipList[j])
                                    {
                                        newPose.AnimId = j;
                                        break;
                                    }
                                }
                            }
                            break;
                    }

                    //Pose Pass
                    for (int j=0; j < globalPose.JointPositions.Count; ++j)
                    {
                        Vector3 jointPos = globalPose.JointPositions[j];
                        Vector3 jointPosMinus = globalPoseMinus.JointPositions[j];
                        Vector3 jointPosPlus = globalPosePlus.JointPositions[j];

                        Vector3 jointVelocity = Vector3.zero;

                        if(timeDelta > Mathf.Epsilon)
                        {
                            jointVelocity = m_helperTransform.InverseTransformVector(
                                jointPosPlus - jointPos) / timeDelta;

                            if (m_data.UseGlobalJointVelocity == EJointVelocityCalculationMethod.BodyVelocityDependent)
                            {
                                jointVelocity -= newPose.LocalVelocity;
                            }
                        }

                        jointPos = m_helperTransform.InverseTransformPoint(jointPos);

                        newPose.JointsData[j] = new JointData(jointPos, jointVelocity);
                    }

                    //TrajectoryPass
                    for (int j = 0; j <  m_data.TrajectoryPoints.Count; ++j)
                    {
                        float time = m_data.TrajectoryPoints[j] + globalPose.Time;
                        Vector3 trajPos = Vector3.zero;
                        float facingAngle = 0f;

                        if (curMxMAnim.IsLooping || (time > 0f && time < targetClip.length) 
                            || (time < 0f && beforeClips.Count > 0)
                            || (time > targetClip.length && afterClips.Count > 0))
                        {//Get the future or past from the current clip, whether it is looped, linked or within the target clip
                            float adjustedTime = time;

                            if (curMxMAnim.IsLooping || beforeClips.Count > 0) 
                            {
                                adjustedTime = time - m_data.TrajectoryPoints[0];
                            }

                            int trajPosIdMinus = Mathf.FloorToInt(adjustedTime / globalPoseInterval);
                            int trajPosIdPlus = trajPosIdMinus + 1;

                            float lerp = (adjustedTime / globalPoseInterval) - (float)trajPosIdMinus;

                            if (trajPosIdPlus >= globalPoses.Count)
                            {
                                trajPosIdPlus = globalPoses.Count - 1;
                                trajPosIdMinus = trajPosIdPlus - 1;
                            }

                            if (trajPosIdMinus < 0)
                            {
                                trajPosIdMinus = 0;
                                trajPosIdPlus = 1;
                            }

                            trajPos = Vector3.Lerp(globalPoses[trajPosIdMinus].Position,
                            globalPoses[trajPosIdPlus].Position, lerp);


                            float baseAngle = globalPose.Rotation.eulerAngles.y;
                            float angleMinus = globalPoses[trajPosIdMinus].Rotation.eulerAngles.y;
                            float anglePlus = globalPoses[trajPosIdPlus].Rotation.eulerAngles.y;

                            facingAngle = Mathf.LerpAngle(angleMinus, anglePlus, lerp) - baseAngle;
                        }
                        else if(time < 0f) 
                        {
                            if (curMxMAnim.UseExtrapolateTrajectory)//Extrapolate the past
                            {
                                float extrapTime = time * -1f;

                                GlobalSpacePose firstPose = globalPoses[0];
                                GlobalSpacePose secondPose = globalPoses[2];

                                Vector3 curVelocity = (firstPose.Position - secondPose.Position) / m_data.PoseInterval;
                                trajPos = firstPose.Position + curVelocity * extrapTime;
                                facingAngle = firstPose.Rotation.eulerAngles.y;
                            }
                            else //Clamp the past
                            {
                                trajPos = globalPoses[0].Position;
                                facingAngle = globalPoses[0].Rotation.eulerAngles.y;
                            }
                        }
                        else if(time > targetClip.length) 
                        {
                            if (curMxMAnim.UseExtrapolateTrajectory)//Extrapolate the future
                            {
                                float extrapTime = time - targetClip.length;

                                GlobalSpacePose lastPose = globalPoses[globalPoses.Count - 1];
                                GlobalSpacePose secondLastPose = globalPoses[globalPoses.Count - 3];

                                Vector3 curVelocity = (lastPose.Position - secondLastPose.Position) / m_data.PoseInterval;
                                trajPos = lastPose.Position + curVelocity * extrapTime;
                                facingAngle = lastPose.Rotation.eulerAngles.y;
                            }
                            else //Clamp the future
                            {
                                trajPos = globalPoses[globalPoses.Count - 1].Position;
                                facingAngle = globalPoses[globalPoses.Count - 1].Rotation.eulerAngles.y;
                            }
                        }

                        if(curMxMAnim.UseFlattenTrajectory)
                        {
                            trajPos.y = 0f;
                        }

                        newPose.Trajectory[j] = new TrajectoryPoint(m_helperTransform.InverseTransformPoint(trajPos), facingAngle);
                    }

                    m_poseList.Add(newPose);
                    curMxMAnim.AddToPoseList(ref newPose);
                    ++count;
                }

                //Process complex anims
                switch (curMxMAnim.AnimType)
                {
                    
                    case EMxMAnimtype.Composite://Process Clip Data or Composite
                        {
                            MxMAnimationClipComposite composite = curMxMAnim as MxMAnimationClipComposite;
                            m_compositeDataList.Add(ProcessCompositeData(curMxMAnim));
                        }
                        break;
                    case EMxMAnimtype.BlendSpace: //Process Blend Space Data
                        {
                            m_blendSpaceList.Add(ProcessBlendSpaceData(curMxMAnim));
                        }
                        break;
                    case EMxMAnimtype.Clip:
                        {
                            MxMAnimationClipComposite composite = curMxMAnim as MxMAnimationClipComposite;
                            m_clipDataList.Add(ProcessClipData(curMxMAnim));

                        }
                        break;
                    case EMxMAnimtype.BlendClip:
                        {
                            MxMBlendClip blendClip = curMxMAnim as MxMBlendClip;
                            m_blendClipDataList.Add(ProcessBlendClipData(curMxMAnim));
                        }
                        break;
                }

                ProcessEventData(curMxMAnim, targetClip, curClipId);
            }

            ProcessIdleSets();
            ProcessFootStepTracks();
            //RemoveEmbededDoNotUsePoses();
            FinalizeAnimData();
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void GeneratePoseSequencing()
        {
            for(int i=0; i < m_processedData.Poses.Length; ++i)
            {
                ref PoseData pose = ref m_processedData.Poses[i];
                ref PoseData beforePose = ref m_processedData.Poses[Mathf.Max(0, i - 1)];
                ref PoseData afterPose = ref m_processedData.Poses[Mathf.Min(i + 1, m_processedData.Poses.Length-1)];

                if(beforePose.AnimId == pose.AnimId && beforePose.AnimType == pose.AnimType)
                {
                    pose.LastPoseId = beforePose.PoseId;
                }
                else
                {
                    int clipId = GetPoseClipId(ref pose);
                    AnimationClip clip = m_processedData.Clips[clipId];

                    if (clip.isLooping)
                    {
                        int posesToEnd = Mathf.FloorToInt((clip.length - pose.Time) / m_data.PoseInterval);
                        pose.LastPoseId = pose.PoseId + posesToEnd;
                    }
                    else
                    {
                        pose.LastPoseId = pose.PoseId;
                    }
                }

                if(afterPose.AnimId == pose.AnimId && afterPose.AnimType == pose.AnimType)
                {
                    pose.NextPoseId = afterPose.PoseId;
                }
                else
                {
                    int clipId = GetPoseClipId(ref pose);
                    AnimationClip clip = m_processedData.Clips[clipId];

                    if (clip.isLooping)
                    {
                        int posesToBeginning = Mathf.FloorToInt(pose.Time / m_data.PoseInterval);
                        pose.NextPoseId = pose.PoseId - posesToBeginning;
                    }
                    else
                    {
                        pose.NextPoseId = pose.PoseId;
                    }
                }

                m_processedData.Poses[i] = pose;
            }

            //If the pose at the beginning of the database is looping, we need to fix its before pose reference
            ref PoseData startPose = ref m_processedData.Poses[0];
            AnimationClip startClip = m_processedData.Clips[GetPoseClipId(ref startPose)];
            if(startClip.isLooping)
            {
                int posesToEnd = Mathf.FloorToInt((startClip.length - startPose.Time) / m_data.PoseInterval);
                startPose.LastPoseId = startPose.PoseId + posesToEnd;
            }

            //If the pose at the end of the database is looping, we need to fix its after pose reference
            ref PoseData endPose = ref m_processedData.Poses[m_processedData.Poses.Length - 1];
            AnimationClip endClip = m_processedData.Clips[GetPoseClipId(ref endPose)]; 
            if(endClip.isLooping)
            {
                int posesToBeginning = Mathf.FloorToInt(endPose.Time / m_data.PoseInterval);
                endPose.NextPoseId = endPose.PoseId - posesToBeginning;
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void ProcessFootStepTracks()
        {
            foreach (CompositeCategory category in m_allCompositeCategories)
            {
                foreach (MxMAnimationClipComposite composite in category.Composites)
                {
                    AddFootstepTrack(composite);
                }
            }

            foreach (MxMBlendSpace blendSpace in m_allBlendSpaces)
            {
                AddFootstepTrack(blendSpace);
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void AddFootstepTrack(IMxMAnim a_mxmAnim)
        { 
            if (a_mxmAnim == null)
                return;

            List<TagTrackBase> genericTagTracks = a_mxmAnim.FinalGenericTagTracks;

            if (genericTagTracks == null || genericTagTracks.Count <= 2)
                return;

            FootStepTagTrack leftFootTagTrack = genericTagTracks[0] as FootStepTagTrack;
            FootStepTagTrack rightFootTagTrack = genericTagTracks[1] as FootStepTagTrack;

            FootstepTagTrackData leftStepData = new FootstepTagTrackData(0, leftFootTagTrack.StepCount);
            FootstepTagTrackData rightStepData = new FootstepTagTrackData(0, rightFootTagTrack.StepCount);

            for (int i = 0; i < leftFootTagTrack.StepCount; ++i)
            {
                var data = leftFootTagTrack.GetFootStepData(i);

                leftStepData.SetFootStep(i, data.range, data.step.Pace, data.step.Type);
            }
            
            for(int i = 0; i < rightFootTagTrack.StepCount; ++i)
            {
                var data = rightFootTagTrack.GetFootStepData(i);

                rightStepData.SetFootStep(i, data.range, data.step.Pace, data.step.Type);
            }
            
            //Sort footstep data by start time of each footstep
            Array.Sort(leftStepData.Tags, leftStepData.FootSteps, new TagComparer());
            Array.Sort(rightStepData.Tags, rightStepData.FootSteps, new TagComparer());

            m_leftFootStepTrackData.Add(leftStepData);
            m_rightFootStepTrackData.Add(rightStepData);
        }

        //============================================================================================
        /**
        *  @brief Comparer for sorting tags
        *         
        *********************************************************************************************/
        private class TagComparer : IComparer<Vector2>
        {
            public int Compare(Vector2 TagA, Vector2 TagB)
            {
                if (TagA.x < TagB.x)
                    return -1;

                if (TagA.x > TagB.x)
                    return 1;

                return 0;
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private float WrapAnimationTime(float _time, AnimationClip _clip)
        {
            if(_time < -Mathf.Epsilon)
            {
                float wholeNumbers = Mathf.FloorToInt(Mathf.Abs(_time) / _clip.length);
                _time = _clip.length + (_time + wholeNumbers * _clip.length);
            }
            else if(_time > _clip.length)
            {
                float wholeNumbers = Mathf.FloorToInt(_time / _clip.length);
                _time = _time - (wholeNumbers * _clip.length);
            }

            return _time;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private (List<AnimationClip>, float, float, int) GenerateClipPlaylist(IMxMAnim a_mxmAnim)
        {
            List<AnimationClip> contClipList = new List<AnimationClip>();
            float startTime = 0f;
            float endTime = 0f;
            int targetIndex = 0;

            AnimationClip finalClip = a_mxmAnim.FinalClip;

            if (a_mxmAnim != null && finalClip != null)
            {
                //AnimationNode curNode = a_mxmAnim;
                AnimationClip curClip = finalClip;
                endTime = curClip.length;
                
                contClipList.Add(curClip);

                List<AnimationClip> beforeClips = a_mxmAnim.AnimBeforeClips;
                List<AnimationClip> afterClips = a_mxmAnim.AnimAfterClips;

                if (beforeClips == null)
                    beforeClips = new List<AnimationClip>();

                if (afterClips == null)
                    afterClips = new List<AnimationClip>();

                //Fetch past clips
                float timeReq = Mathf.Abs(m_data.TrajectoryPoints[0]);

                int index = 0;
                while(true)
                {
                    if (beforeClips.Count > index || a_mxmAnim.IsLooping)
                    {
                        AnimationClip prevClip = finalClip;

                        if(!a_mxmAnim.IsLooping)
                            prevClip = beforeClips[index];

                        ++index;

                        if (prevClip != null)
                        {
                            contClipList.Insert(0, prevClip);
                            ++targetIndex;

                            timeReq -= Mathf.Max(SixtyHz, prevClip.length);
                            
                            if (timeReq < 0f)
                            {
                                startTime = -timeReq;
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                //Fetch future clips
                timeReq = Mathf.Abs(m_data.TrajectoryPoints[m_data.TrajectoryPoints.Count - 1]);
                if (a_mxmAnim.AnimType == EMxMAnimtype.Composite && afterClips.Count > 0)
                {
                    timeReq += afterClips[0].length;
                }

                index = 0;
                while(true)
                {
                    if (afterClips.Count > index || a_mxmAnim.IsLooping)
                    {
                        AnimationClip nextClip = finalClip;

                        if (!a_mxmAnim.IsLooping)
                            nextClip = afterClips[index];

                        ++index;

                        if (nextClip != null)
                        {
                            if (nextClip.isLooping)
                            {
                                int loops = Mathf.CeilToInt(timeReq / Mathf.Max(SixtyHz, nextClip.length));

                                for(int i = 0; i < loops; ++i)
                                {
                                    contClipList.Add(nextClip);
                                    timeReq -= nextClip.length;
                                }

                                endTime = nextClip.length + timeReq;
                                break;
                            }
                            else
                            {
                                contClipList.Add(nextClip);

                                timeReq -= Mathf.Max(SixtyHz, nextClip.length);
                                if (timeReq < 0f)
                                {
                                    endTime = nextClip.length + timeReq;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return (contClipList, startTime, endTime, targetIndex);
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void ProcessEventData(IMxMAnim a_mxmAnim, AnimationClip a_targetClip, int a_clipId)
        {
            //PreProcess Events for this animation
            List<EventMarker> events = a_mxmAnim.FinalEventMarkers;

            if (events == null)
                events = new List<EventMarker>();

            foreach (EventMarker evt in events)
            {
                evt.EventTime = Mathf.Clamp(evt.EventTime, 0f, a_targetClip.length);

                if (evt.EventTime - evt.Actions[0] < 0f)
                    evt.Actions[0] = evt.EventTime;

                if (evt.EventTime - evt.Actions[0] - evt.Windup < 0f)
                    evt.Windup = evt.EventTime - evt.Actions[0];


                float cumActionTime = evt.EventTime;
                for (int k = 1; k < evt.Actions.Count; ++k)
                {
                    if (cumActionTime + evt.Actions[k] > a_targetClip.length)
                    {
                        evt.Actions[k] = a_targetClip.length - cumActionTime;
                        cumActionTime = a_targetClip.length;

                        for (int j = k + 1; j < evt.Actions.Count; ++j)
                            evt.Actions[j] = 0;

                        break;
                    }
                    else
                    {
                        cumActionTime += evt.Actions[k];
                    }
                }

                if (cumActionTime + evt.FollowThrough > a_targetClip.length)
                    evt.FollowThrough = a_targetClip.length - cumActionTime;

                cumActionTime += evt.FollowThrough;

                if (cumActionTime + evt.Recovery > a_targetClip.length)
                    evt.Recovery = a_targetClip.length - cumActionTime;

                //Find the pose that starts this event
                float eventStartTime = Mathf.Clamp(evt.EventTime - evt.Windup - evt.Actions[0], 0f, a_targetClip.length);

                int closestPoseId = 0;
                float closestPoseCost = float.MaxValue;
                float signedCost = closestPoseCost;
                for (int k = 0; k < m_poseList.Count; ++k)
                {
                    PoseData pose = m_poseList[k];

                    if(GetPoseClipIdPreProcess(ref pose) == a_clipId)
                    {
                        float cost = m_poseList[k].Time - eventStartTime;

                        if (Mathf.Abs(cost) < closestPoseCost)
                        {
                            closestPoseCost = Mathf.Abs(cost);
                            signedCost = cost;
                            closestPoseId = k;
                        }
                    }
                }

                PoseData eventPose = m_poseList[closestPoseId];
                evt.Windup -= eventPose.Time - eventStartTime;

                int totalWindupPoses = Mathf.CeilToInt(evt.Windup / m_data.PoseInterval);
                if (totalWindupPoses == 0)
                    totalWindupPoses = 1;

                EventContact[] windupContactOffsets = new EventContact[totalWindupPoses];
                EventContact[] rootContactOffsets = new EventContact[evt.Contacts.Count];
                EventContact[] secondaryContactOffsets = new EventContact[evt.Contacts.Count - 1];

                UTIL.PlayableUtils.DestroyChildPlayables(m_animationMixer);

                var clipPlayable = m_animationMixer.GetInput(0);
                clipPlayable = AnimationClipPlayable.Create(m_playableGraph, a_targetClip);
                ((AnimationClipPlayable)clipPlayable).SetApplyFootIK(true);
                m_playableGraph.Connect(clipPlayable, 0, m_animationMixer, 0);
                m_animationMixer.SetInputWeight(0, 1f);

                //Reset transform and animate to the target pose
                for (int k = 0; k < totalWindupPoses; ++k)
                {
                    clipPlayable.SetTime(0.0);
                    clipPlayable.SetTime(0.0);
                    m_playableGraph.Evaluate(0f);
                    m_target.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

                    float timeToSimulate = Mathf.Clamp(eventPose.Time + k * m_data.PoseInterval, 0f, a_targetClip.length);
                    for (float time = 0f; time < timeToSimulate; time += SixtyHz)
                    {
                        m_playableGraph.Evaluate(SixtyHz);
                    }

                    clipPlayable.SetTime(timeToSimulate);
                    clipPlayable.SetTime(timeToSimulate);
                    m_playableGraph.Evaluate(0f);

                    //Calculate the offset of the windup pose root to the contact point of the event
                    windupContactOffsets[k].Position = evt.Contacts[0].Position - m_target.transform.position;
                    windupContactOffsets[k].RotationY = evt.Contacts[0].RotationY - m_target.transform.rotation.eulerAngles.y;
                }

                //Calculate the root contact offsets
                float cumContactTime = evt.EventTime;
                for (int k = 0; k < evt.Contacts.Count; ++k)
                {
                    clipPlayable.SetTime(0.0);
                    clipPlayable.SetTime(0.0);
                    m_playableGraph.Evaluate(0f);
                    m_target.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

                    for (float time = 0f; time < cumContactTime; time += SixtyHz)
                    {
                        m_playableGraph.Evaluate(SixtyHz);

                        if(time + SixtyHz >= cumContactTime)
                        {
                            m_playableGraph.Evaluate(cumContactTime - time);
                            break;
                        }
                    }

                    m_helperTransform.SetPositionAndRotation(evt.Contacts[k].Position, Quaternion.AngleAxis(evt.Contacts[k].RotationY, Vector3.up));

                    rootContactOffsets[k].Position = m_helperTransform.InverseTransformPoint(m_target.transform.position);
                    rootContactOffsets[k].RotationY = Mathf.DeltaAngle(evt.Contacts[k].RotationY, m_target.transform.rotation.eulerAngles.y);

                    if (k < evt.Actions.Count - 1)
                        cumContactTime += evt.Actions[k + 1];
                }

                //Calculate the offset between contacts
                float startEvtTime = evt.EventTime;
                for (int k = 0; k < secondaryContactOffsets.Length; ++k)
                {
                    float endEvtTime = startEvtTime + evt.Actions[k + 1];
                    clipPlayable.SetTime(startEvtTime);
                    clipPlayable.SetTime(startEvtTime);
                    m_playableGraph.Evaluate(0f);               
                    m_target.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

                    //Simulate
                    for (float time = startEvtTime; time < endEvtTime; time += SixtyHz)
                    {
                        m_playableGraph.Evaluate(SixtyHz);

                        if(time + SixtyHz >= endEvtTime)
                        {
                            m_playableGraph.Evaluate(endEvtTime - time);
                            break;
                        }
                    }

                    secondaryContactOffsets[k].Position = m_target.transform.position;
                    secondaryContactOffsets[k].RotationY = m_target.transform.rotation.eulerAngles.y;

                    startEvtTime = endEvtTime;
                }

                //Calculate the lookup table
                
                float actionsDuration = 0f;
                foreach (float duration in evt.Actions)
                {
                    actionsDuration += duration;
                }
                float actionsEndTime = evt.EventTime - evt.Actions[0] + actionsDuration;

                EventFrameData[] warpLookupTable = new EventFrameData[Mathf.CeilToInt(60f * (evt.Windup + actionsDuration))];

                //Vector3[] rootDeltaLookupTemp = new Vector3[warpLookupTable.Length];
                //float[] rootRotDeltaLookupTemp = new float[warpLookupTable.Length];

                float startEventTime = evt.EventTime - evt.Actions[0] - evt.Windup;
                clipPlayable.SetTime(0.0);
                clipPlayable.SetTime(0.0f);
                m_playableGraph.Evaluate(0f);
                m_target.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

                //Simulate to the start of the event
                for(float time = 0f; time < startEventTime; time += SixtyHz)
                {
                    m_playableGraph.Evaluate(SixtyHz);

                    if(time + SixtyHz >= startEventTime)
                    {
                        m_playableGraph.Evaluate(startEventTime - time);
                        break;
                    }
                }

                int contactIndex = 0;
                float contactTime = evt.EventTime;
                //Vector3 startPosition = m_target.transform.position;
                //Vector3 currentTotalDisplacement = Vector3.zero;
                //float startRotation = m_target.transform.rotation.eulerAngles.y;
                //float currentTotalRotationY = 0f;
                float lookupTime = startEventTime;
                List<TagTrackBase> genericTagTracks = a_mxmAnim.FinalGenericTagTracks;
                TagTrackBase warpPosTagTrack = genericTagTracks[2];
                TagTrackBase warpRotTagTrack = genericTagTracks[3];
                for (int k = 0; k < warpLookupTable.Length; ++k)
                {
                    EventFrameData newFrameData = new EventFrameData();
                    ref readonly EventContact rootContactOffset = ref rootContactOffsets[contactIndex];

                    Quaternion contactOrient = Quaternion.AngleAxis(evt.Contacts[contactIndex].RotationY, Vector3.up);

                    newFrameData.relativeContactRoot = m_target.transform.InverseTransformPoint(evt.Contacts[contactIndex].Position
                        + (contactOrient * rootContactOffset.Position));

                    newFrameData.relativeContactRootRotY = Mathf.DeltaAngle(m_target.transform.rotation.eulerAngles.y,
                        evt.Contacts[contactIndex].RotationY + rootContactOffset.RotationY);

                    if (warpPosTagTrack.IsTimeTagged(startEventTime + ((float)k * SixtyHz)))
                    {
                        newFrameData.WarpPosThisFrame = false;
                    }
                    else
                    {
                        newFrameData.WarpPosThisFrame = true;
                        //rootDeltaLookupTemp[k] = m_target.transform.position - currentTotalDisplacement;
                        //currentTotalDisplacement = m_target.transform.position - startPosition;
                    }

                    if (warpRotTagTrack.IsTimeTagged(startEventTime + ((float)k * SixtyHz)))
                    {
                        newFrameData.WarpRotThisFrame = false;
                    }
                    else
                    {
                        newFrameData.WarpRotThisFrame = true;
                        //float rotationY = m_target.transform.rotation.eulerAngles.y - startRotation;
                        //rootRotDeltaLookupTemp[k] = Mathf.DeltaAngle(currentTotalRotationY, rotationY);
                        //currentTotalRotationY = rotationY;
                    }

                    newFrameData.Time = startEventTime + ((float)k * SixtyHz);

                    warpLookupTable[k] = newFrameData;

                    lookupTime += SixtyHz;

                    if (lookupTime > contactTime)
                    {
                        ++contactIndex;

                        if (contactIndex < evt.Actions.Count)
                            contactTime += evt.Actions[contactIndex];
                        else
                            break;
                    }

                    m_playableGraph.Evaluate(SixtyHz);
                }

                //Calculate remaining warp time
                float cumAvailableRotTime = 0f;
                float cumAvailablePosTime = 0f;
                //Vector3 cumDelta = Vector3.zero;
                //float cumRotDelta = 0f;
                contactIndex = evt.Actions.Count - 1;
                contactTime = evt.Windup + actionsDuration - evt.Actions[contactIndex];

                if (evt.Actions.Count == 1)
                    contactTime = 0f;

                for (int k = warpLookupTable.Length - 1; k >= 0; --k)
                {
                    ref EventFrameData frameData = ref warpLookupTable[k];
                   // Vector3 delta = rootDeltaLookupTemp[k];
                    //float rotDelta = rootRotDeltaLookupTemp[k];

                    //cumDelta += new Vector3(Mathf.Abs(delta.x), Mathf.Abs(delta.y), Mathf.Abs(delta.z));
                    //cumRotDelta += Mathf.Abs(rotDelta);

                    if (frameData.Time < contactTime)
                    {
                        cumAvailablePosTime = contactTime - frameData.Time;
                        cumAvailableRotTime = contactTime - frameData.Time;

                        //cumDelta = Vector3.zero;
                        //cumRotDelta = 0f;

                        --contactIndex;

                        if (contactIndex > 0)
                        {
                            contactTime -= evt.Actions[contactIndex];
                        }
                        else if (contactIndex == 0)
                        {
                            contactTime = 0f;
                        }
                    }

                    frameData.RemainingWarpTime = cumAvailablePosTime;
                    frameData.RemainingRotWarpTime = cumAvailableRotTime;
                    //frameData.remainingDeltaSum = cumDelta;
                    //frameData.remainingRotDeltaSum = cumRotDelta;

                    if (frameData.WarpPosThisFrame)
                        cumAvailablePosTime += SixtyHz;

                    if (frameData.WarpRotThisFrame)
                        cumAvailableRotTime += SixtyHz;
                }
                
                int eventId = GetEventIdByName(evt.EventName);

                if(eventId == -1)
                {
                    Debug.LogWarning("MxM Pre Processing - An event was processed. However, the event name: '" + evt.EventName +
                        "' does not match any registered events in the pre-process data or any of the animation modules. This event" +
                        "won't be able to run. Check your event Ids on your MxM animations.");
                }

                EventData newEventData =  new EventData(eventId, evt.Windup, evt.Actions.ToArray(),
                    evt.FollowThrough, evt.Recovery, eventPose.PoseId, rootContactOffsets, windupContactOffsets,
                    warpLookupTable, secondaryContactOffsets);

                m_eventList.Add(newEventData);
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public ClipData ProcessClipData(IMxMAnim a_mxmAnim)
        {
            MxMAnimationClipComposite composite = a_mxmAnim as MxMAnimationClipComposite;

            //Determine start and end poses
            int startPoseId = composite.PoseList[0].PoseId;
            int endPoseId = composite.PoseList[composite.PoseList.Count - 1].PoseId;

            //Determine clip Id
            int clipId = -1;

            AnimationClip clip = a_mxmAnim.FinalClip;

            for (int i = 0; i < m_clips.Count; ++i)
            {
                if (clip == m_clips[i])
                {
                    clipId = i;
                    break;
                }
            }

            if (clipId < 0)
            {
                m_clips.Add(clip);
                clipId = m_clips.Count - 1;
            }

            ClipData clipData = new ClipData(startPoseId, endPoseId, clipId, clip.isLooping, 
                clip.length, composite.PlaybackSpeed);

            //Pre-proces curves for this clip data
            List<AnimationCurve> animCurves = new List<AnimationCurve>(composite.Curves.Count + 1);
            List<int> animCurveIds = new List<int>(composite.Curves.Count + 1);
            foreach(MxMCurveTrack curveTrack in composite.Curves)
            {
                if (curveTrack == null || curveTrack.CurveName == null || curveTrack.AnimCurve == null)
                    continue; //Curve data is invalid

                int curveId = GetCurveBindingId(curveTrack.CurveName);

                if (curveId == -1)
                    continue; //Could not find the curve binding

                animCurveIds.Add(curveId);
                animCurves.Add(curveTrack.AnimCurve);
            }

            clipData.CurveData.SetCurves(animCurves, animCurveIds);

            return clipData;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public CompositeData ProcessCompositeData(IMxMAnim a_mxmAnim)
        {
            MxMAnimationClipComposite composite = a_mxmAnim as MxMAnimationClipComposite;

            //Determine start and end poses
            int startPoseId = composite.PoseList[0].PoseId;
            int endPoseId = composite.PoseList[composite.PoseList.Count - 1].PoseId;

            List<int> clipIds = new List<int>();

            //Determine primary clip Id

            AnimationClip primaryClip = a_mxmAnim.FinalClip;
            List<AnimationClip> afterClips = a_mxmAnim.AnimAfterClips;

            List<AnimationClip> compositeClips = new List<AnimationClip>(afterClips);
            compositeClips.Insert(0, primaryClip);

            for (int i = 0; i < compositeClips.Count; ++i)
            {
                AnimationClip clip = compositeClips[i];

                int clipId = -1;
                for (int n = 0; n < m_clips.Count; ++n)
                {
                    if (clip == m_clips[n])
                    {
                        clipId = n;
                        break;
                    }
                }

                if (clipId < 0)
                {
                    m_clips.Add(clip);
                    clipId = m_clips.Count - 1;

                }

                clipIds.Add(clipId);
            }

            CompositeData compositeData = new CompositeData(startPoseId, endPoseId, clipIds[0], clipIds[1], 
                primaryClip.length, afterClips[0].length, composite.PlaybackSpeed);

            //Pre-proces curves for this composite data
            List<AnimationCurve> animCurves = new List<AnimationCurve>(composite.Curves.Count + 1);
            List<int> animCurveIds = new List<int>(composite.Curves.Count + 1);
            foreach (MxMCurveTrack curveTrack in composite.Curves)
            {
                if (curveTrack == null || curveTrack.CurveName == null || curveTrack.AnimCurve == null)
                    continue; //Curve data is invalid

                int curveId = GetCurveBindingId(curveTrack.CurveName);

                if (curveId == -1)
                    continue; //Could not find the curve binding

                animCurveIds.Add(curveId);
                animCurves.Add(curveTrack.AnimCurve);
            }

            compositeData.CurveData.SetCurves(animCurves, animCurveIds);

            return compositeData;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public BlendSpaceData ProcessBlendSpaceData(IMxMAnim a_mxmAnim)
        {
            MxMBlendSpace blendSpace = a_mxmAnim as MxMBlendSpace;

            int startPoseId = blendSpace.PoseList[0].PoseId;
            int endPoseId = blendSpace.PoseList[blendSpace.PoseList.Count - 1].PoseId;

            //Determine clip Ids
            List<AnimationClip> blendSpaceClips = blendSpace.FinalClips;
            int[] clipIds = new int[blendSpaceClips.Count];
            float blendSpaceLength = blendSpaceClips[0].length;
            for (int i = 0; i < blendSpaceClips.Count; ++i)
            {
                AnimationClip clip = blendSpaceClips[i];

                clipIds[i] = -1;
                for (int k = 0; k < m_clips.Count; ++k)
                {
                    if (clip == m_clips[k])
                    {
                        clipIds[i] = k;
                        break;
                    }
                }

                if (clipIds[i] < 0)
                {
                    m_clips.Add(blendSpaceClips[i]);
                    clipIds[i] = m_clips.Count - 1;

                    
                }
            }

            BlendSpaceData blendSpaceData =  new BlendSpaceData(startPoseId, endPoseId, blendSpace.NormalizeTime, blendSpace.Magnitude,
                 blendSpace.Smoothing, clipIds, blendSpace.Positions.ToArray(), blendSpaceLength, blendSpace.PlaybackSpeed);

            //Pre-proces curves for this blend space data
            List<AnimationCurve> animCurves = new List<AnimationCurve>(blendSpace.Curves.Count + 1);
            List<int> animCurveIds = new List<int>(blendSpace.Curves.Count + 1);
            foreach (MxMCurveTrack curveTrack in blendSpace.Curves)
            {
                if (curveTrack == null || curveTrack.CurveName == null || curveTrack.AnimCurve == null)
                    continue; //Curve data is invalid

                int curveId = GetCurveBindingId(curveTrack.CurveName);

                if (curveId == -1)
                    continue; //Could not find the curve binding

                animCurveIds.Add(curveId);
                animCurves.Add(curveTrack.AnimCurve);
            }

            blendSpaceData.CurveData.SetCurves(animCurves, animCurveIds);

            return blendSpaceData;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private BlendClipData ProcessBlendClipData(IMxMAnim a_mxmAnim)
        {
            MxMBlendClip blendClip = a_mxmAnim as MxMBlendClip;

            int startPoseId = blendClip.PoseList[0].PoseId;
            int endPoseId = blendClip.PoseList[blendClip.PoseList.Count - 1].PoseId;

            List<AnimationClip> blendClips = blendClip.FinalClips;
            int[] clipIds = new int[blendClips.Count];
            for(int i = 0; i < blendClips.Count; ++i)
            {
                AnimationClip clip = blendClips[i];

                clipIds[i] = -1;
                for(int k = 0; k < m_clips.Count; ++k)
                {
                    if (clip == m_clips[k])
                    {
                        clipIds[i] = k;
                        break;
                    }
                }

                if (clipIds[i] < 0)
                {
                    m_clips.Add(blendClips[i]);
                    clipIds[i] = m_clips.Count - 1;
                }
            }
            
            BlendClipData blendClipData = new BlendClipData(startPoseId, endPoseId, blendClip.NormalizeTime, clipIds, 
                blendClip.Weightings, blendClip.NormalizedLength, blendClip.PlaybackSpeed);

            //Pre-proces curves for this blend clip Data
            List<AnimationCurve> animCurves = new List<AnimationCurve>(blendClip.CurveTracks.Count + 1);
            List<int> animCurveIds = new List<int>(blendClip.CurveTracks.Count + 1);
            foreach (MxMCurveTrack curveTrack in blendClip.CurveTracks)
            {
                if (curveTrack == null || curveTrack.CurveName == null || curveTrack.AnimCurve == null)
                    continue; //Curve data is invalid

                int curveId = GetCurveBindingId(curveTrack.CurveName);

                if (curveId == -1)
                    continue; //Could not find the curve binding

                animCurveIds.Add(curveId);
                animCurves.Add(curveTrack.AnimCurve);
            }

            blendClipData.CurveData.SetCurves(animCurves, animCurveIds);

            return blendClipData;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void ProcessIdleSets()
        {
            //Process Idle sets
            for (int i = 0; i < m_animIdleSetList.Count; ++i)
            {
                if (m_animIdleSetList[i] == null)
                    return;

                MxMAnimationIdleSet idleSet = m_animIdleSetList[i];

                if (idleSet.PrimaryClip == null)
                    return;

                if (idleSet.SecondaryClips == null)
                    idleSet.SecondaryClips = new List<AnimationClip>();

                if (idleSet.TransitionClips == null)
                    idleSet.TransitionClips = new List<AnimationClip>();

                idleSet.PoseList = new List<PoseData>();

                //Setup Primary Clip
                bool addClipToList = true;
                int primaryClipId = m_clips.Count;
                for (int k = 0; k < m_clips.Count; ++k)
                {
                    if (ReferenceEquals(idleSet.PrimaryClip, m_clips[k]))
                    {
                        primaryClipId = k;
                        addClipToList = false;
                        break;
                    }
                }

                if(addClipToList)
                {
                    m_clips.Add(idleSet.PrimaryClip);
                }


                UTIL.PlayableUtils.DestroyChildPlayables(m_animationMixer);

                //Generate Primary Poses
                var clipPlayable = m_animationMixer.GetInput(0);
                clipPlayable = AnimationClipPlayable.Create(m_playableGraph, idleSet.PrimaryClip);
                ((AnimationClipPlayable)clipPlayable).SetApplyFootIK(true);
                clipPlayable.SetTime(0.0);
                clipPlayable.SetTime(0.0);
                m_playableGraph.Connect(clipPlayable, 0, m_animationMixer, 0);
                m_animationMixer.SetInputWeight(0, 1f);
                m_playableGraph.Evaluate(0f);

                Vector3[] lastJointPositions = new Vector3[m_matchPropertyBones.Count];
                
                for (float clipTime = 0; clipTime < idleSet.PrimaryClip.length + Mathf.Epsilon; clipTime += m_data.PoseInterval * idleSet.PlaybackSpeed)
                {
                    PoseData newPose = new PoseData(m_poseList.Count, primaryClipId, -1, clipTime, Vector3.zero,
                        m_data.PoseJoints.Count, m_data.TrajectoryPoints.Count, 1f, (idleSet.Tags | ETags.DoNotUse),
                        idleSet.FavourTags, EGenericTags.None, EUserTags.None, EMxMAnimtype.IdleSet);

                    //newPose.AnimId = m_idleSetList.Count;
                    
                    //Joints
                    for (int k = 0; k < m_matchPropertyBones.Count; ++k)
                    {
                        Transform joint = m_matchPropertyBones[k];

                        Vector3 jointPos = m_target.transform.InverseTransformPoint(joint.position);
                        Vector3 jointVel = Vector3.zero;

                        if (k > 0)
                        {
                            jointVel = (jointPos - lastJointPositions[k]) / m_data.PoseInterval;
                        }

                        lastJointPositions[k] = jointPos;
                        newPose.JointsData[k] = new JointData(jointPos, jointVel);
                    }

                    m_poseList.Add(newPose);
                    idleSet.PoseList.Add(newPose);

                    m_playableGraph.Evaluate(m_data.PoseInterval * idleSet.PlaybackSpeed);
                }

                IdleSetData idleSetData = new IdleSetData(idleSet.Tags, primaryClipId, idleSet.PoseList.Count, 
                    idleSet.TransitionClips.Count, idleSet.SecondaryClips.Count, idleSet.MinLoops, idleSet.MaxLoops, 
                    idleSet.PlaybackSpeed, m_processedData.IdleTraitFromName(idleSet.Traits));

                for (int k = 0; k < idleSet.PoseList.Count; ++k)
                {
                    idleSetData.PrimaryPoseIds[k] = idleSet.PoseList[k].PoseId;
                }

                idleSet.PoseList.Clear();

                //TRANSITIONS
                for (int k = 0; k < idleSet.TransitionClips.Count; ++k)
                {
                    AnimationClip transitionClip = idleSet.TransitionClips[k];

                    //Setup Transition Clip
                    addClipToList = true;
                    int transitionClipId = m_clips.Count;
                    for (int j = 0; j < m_clips.Count; ++j)
                    {
                        if (ReferenceEquals(transitionClip, m_clips[j]))
                        {
                            transitionClipId = j;
                            addClipToList = false;
                            break;
                        }
                    }

                    if (addClipToList)
                    {
                        m_clips.Add(transitionClip);
                    }

                    idleSetData.TransitionClipIds[k] = transitionClipId;

                    UTIL.PlayableUtils.DestroyChildPlayables(m_animationMixer);

                    //Generate Transition Poses
                    clipPlayable = m_animationMixer.GetInput(0);
                    clipPlayable = AnimationClipPlayable.Create(m_playableGraph, transitionClip);
                    ((AnimationClipPlayable)clipPlayable).SetApplyFootIK(true);
                    clipPlayable.SetTime(0.0);
                    clipPlayable.SetTime(0.0);
                    m_playableGraph.Connect(clipPlayable, 0, m_animationMixer, 0);
                    m_animationMixer.SetInputWeight(0, 1f);
                    m_playableGraph.Evaluate(0f);

                    lastJointPositions = new Vector3[m_matchPropertyBones.Count];

                    for (float clipTime = 0; clipTime < transitionClip.length + Mathf.Epsilon; clipTime += m_data.PoseInterval * idleSet.PlaybackSpeed)
                    {
                        PoseData newPose = new PoseData(m_poseList.Count, transitionClipId, -1, clipTime, Vector3.zero,
                            m_data.PoseJoints.Count, m_data.TrajectoryPoints.Count, 1f, (idleSet.Tags | ETags.DoNotUse),
                            idleSet.FavourTags, EGenericTags.None, EUserTags.None, EMxMAnimtype.IdleSet);

                        //newPose.AnimId = m_idleSetList.Count;
                        
                        //Joints
                        for (int j = 0; j < m_matchPropertyBones.Count; ++j)
                        {
                            Transform joint = m_matchPropertyBones[j];

                            Vector3 jointPos = m_target.transform.InverseTransformPoint(joint.position);
                            Vector3 jointVel = Vector3.zero;

                            if (j > 0)
                            {
                                jointVel = (jointPos - lastJointPositions[j]) / m_data.PoseInterval;
                            }

                            lastJointPositions[j] = jointPos;
                            newPose.JointsData[j] = new JointData(jointPos, jointVel);
                        }

                        m_poseList.Add(newPose);
                        idleSet.PoseList.Add(newPose);

                        m_playableGraph.Evaluate(m_data.PoseInterval * idleSet.PlaybackSpeed);
                    }

                    idleSetData.TransitionPoseIds = new int[idleSet.PoseList.Count];
                    for (int j = 0; j < idleSet.PoseList.Count; ++j)
                    {
                        idleSetData.TransitionPoseIds[j] = idleSet.PoseList[j].PoseId;
                    }

                }

                idleSet.PoseList.Clear();

                //SECONDARY IDLES
                for (int k = 0; k < idleSet.SecondaryClips.Count; ++k)
                {
                    AnimationClip secondaryClip = idleSet.SecondaryClips[k];

                    //Setup Secondary Clips
                    addClipToList = true;
                    int secondaryClipId = m_clips.Count;
                    for (int j = 0; j < m_clips.Count; ++j)
                    {
                        if (ReferenceEquals(secondaryClip, m_clips[j]))
                        {
                            secondaryClipId = j;
                            addClipToList = false;
                            break;
                        }
                    }

                    if (addClipToList)
                    {
                        m_clips.Add(secondaryClip);
                    }

                    idleSetData.SecondaryClipIds[k] = secondaryClipId;

                    UTIL.PlayableUtils.DestroyChildPlayables(m_animationMixer);

                    //Generate Secondary Poses
                    clipPlayable = m_animationMixer.GetInput(0);
                    clipPlayable = AnimationClipPlayable.Create(m_playableGraph, secondaryClip);
                    ((AnimationClipPlayable)clipPlayable).SetApplyFootIK(true);
                    clipPlayable.SetTime(0.0);
                    clipPlayable.SetTime(0.0);
                    m_playableGraph.Connect(clipPlayable, 0, m_animationMixer, 0);
                    m_animationMixer.SetInputWeight(0, 1f);
                    m_playableGraph.Evaluate(0f);

                    lastJointPositions = new Vector3[m_matchPropertyBones.Count];

                    for (float clipTime = 0; clipTime < secondaryClip.length + Mathf.Epsilon; clipTime += m_data.PoseInterval * idleSet.PlaybackSpeed)
                    {
                        int lastPoseId = m_poseList.Count;

                        PoseData newPose = new PoseData(lastPoseId, secondaryClipId, -1, clipTime, Vector3.zero,
                            m_data.PoseJoints.Count, m_data.TrajectoryPoints.Count, 1f, (idleSet.Tags | ETags.DoNotUse),
                            idleSet.FavourTags, EGenericTags.None, EUserTags.None, EMxMAnimtype.IdleSet);

                        //newPose.AnimId = m_idleSetList.Count;
                        
                        //Joints
                        for (int j = 0; j < m_matchPropertyBones.Count; ++j)
                        {
                            Transform joint = m_matchPropertyBones[j];

                            Vector3 jointPos = m_target.transform.InverseTransformPoint(joint.position);
                            Vector3 jointVel = Vector3.zero;

                            if (j > 0)
                            {
                                jointVel = (jointPos - lastJointPositions[j]) / m_data.PoseInterval;
                            }

                            lastJointPositions[j] = jointPos;
                            newPose.JointsData[j] = new JointData(jointPos, jointVel);
                        }

                        m_poseList.Add(newPose);

                        if (clipTime < m_data.PoseInterval / 2f)
                            idleSet.PoseList.Add(newPose);

                        m_playableGraph.Evaluate(m_data.PoseInterval * idleSet.PlaybackSpeed);
                    }

                    for (int j = 0; j < idleSet.PoseList.Count; ++j)
                    {
                        idleSetData.SecondaryPoseIds[j] = idleSet.PoseList[j].PoseId;
                    }

                }

                m_idleSetList.Add(idleSetData);
            }
        }

        private void RemoveEmbededDoNotUsePoses()
        {
            int cullDepth = Mathf.CeilToInt(0.3f / m_processedData.PoseInterval);

            int curDoNotUseDepth = 0;
            bool cullActive = false;
            int startCullId = 0;
            int endCullId = 0;

            for(int i = 0; i < m_poseList.Count; ++i)
            {
                ETags tags = m_poseList[i].Tags;

                if((tags & ETags.DoNotUse) == ETags.DoNotUse)
                {
                    if(curDoNotUseDepth >= cullDepth)
                    {
                        if(!cullActive)
                        {
                            cullActive = true;
                            startCullId = i;
                        }
                    }

                    ++curDoNotUseDepth;
                }
                else
                {
                    if(cullActive)
                    {
                        endCullId = i - cullDepth;

                        if(endCullId > startCullId)
                        {
                            int count = endCullId - startCullId;

                            m_poseList.RemoveRange(startCullId, count);
                            i -= count;
                        }

                        cullActive = false;
                    }

                    curDoNotUseDepth = 0;
                }
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void FinalizeAnimData()
        {
            //Add all clips
            m_processedData.Clips = new AnimationClip[m_clips.Count];
            for (int i = 0; i < m_clips.Count; ++i)
                m_processedData.Clips[i] = m_clips[i];

            //Add poses to anim data
            m_processedData.Poses = new PoseData[m_poseList.Count];
            for (int i = 0; i < m_poseList.Count; ++i)
                m_processedData.Poses[i] = m_poseList[i];

            //Add events to anim data
            m_processedData.Events = new EventData[m_eventList.Count];
            for (int i = 0; i < m_eventList.Count; ++i)
                m_processedData.Events[i] = m_eventList[i];

            m_processedData.EventNames = m_combinedEventNames.ToArray();

            if (m_data.OverrideTagModule != null)
            {
                m_processedData.TagNames = m_data.OverrideTagModule.TagNames.ToArray();
                m_processedData.FavourTagNames = m_data.OverrideTagModule.FavourTagNames.ToArray();
                m_processedData.UserTagNames = m_data.OverrideTagModule.UserTagNames.ToArray();
            }
            else
            {
                m_processedData.TagNames = m_data.TagNames.ToArray();
                m_processedData.FavourTagNames = m_data.FavourTagNames.ToArray();
                m_processedData.UserTagNames = m_data.UserTagNames.ToArray();
            }

            //Add idle sets to anim data
            m_processedData.IdleSets = new IdleSetData[m_idleSetList.Count];
            for (int i = 0; i < m_idleSetList.Count; ++i)
                m_processedData.IdleSets[i] = m_idleSetList[i];

            //Add blend spaces to anim data
            m_processedData.BlendSpaces = new BlendSpaceData[m_blendSpaceList.Count];
            for (int i = 0; i < m_blendSpaceList.Count; ++i)
                m_processedData.BlendSpaces[i] = m_blendSpaceList[i];

            //Add all clips data to anim data
            m_processedData.ClipsData = new ClipData[m_clipDataList.Count];
            for (int i = 0; i < m_clipDataList.Count; ++i)
                m_processedData.ClipsData[i] = m_clipDataList[i];

            //Add all blend blips data to anim data
            m_processedData.BlendClips = new BlendClipData[m_blendClipDataList.Count];
            for (int i = 0; i < m_blendClipDataList.Count; ++i)
                m_processedData.BlendClips[i] = m_blendClipDataList[i];

            //Add all composites data to anim data
            m_processedData.Composites = new CompositeData[m_compositeDataList.Count];
            for (int i = 0; i < m_compositeDataList.Count; ++i)
                m_processedData.Composites[i] = m_compositeDataList[i];

            //Set the default start Pose Id to the first idle set pose
            if (m_processedData.IdleSets.Length > 0)
            {
                ref readonly IdleSetData firstIdleSet = ref m_processedData.IdleSets[0];

                if (firstIdleSet.PrimaryPoseIds.Length > 0)
                    m_processedData.StartPoseId = firstIdleSet.PrimaryPoseIds[0];
            }

            //Add footstep tracks data
            m_processedData.LeftFootSteps = new FootstepTagTrackData[m_leftFootStepTrackData.Count];
            for (int i = 0; i < m_leftFootStepTrackData.Count; ++i)
                m_processedData.LeftFootSteps[i] = m_leftFootStepTrackData[i];

            m_processedData.RightFootSteps = new FootstepTagTrackData[m_rightFootStepTrackData.Count];
            for (int i = 0; i < m_rightFootStepTrackData.Count; ++i)
                m_processedData.RightFootSteps[i] = m_rightFootStepTrackData[i];

            int count = 0;
            for(int i = 0; i < m_processedData.Poses.Length; ++i)
            {
                ref readonly PoseData poseData = ref m_processedData.Poses[i];

                if((poseData.Tags & ETags.DoNotUse) == ETags.DoNotUse)
                {
                    ++count;
                }
            }
            
            //Cache pose clip Ids
            for(int i = 0; i < m_processedData.Poses.Length; ++i)
            {
                ref PoseData pose = ref m_processedData.Poses[i];
                pose.PrimaryClipId = GetPoseClipId(ref pose);
            }

            //Add Curve Bindings
            m_processedData.CurveNames = m_curveBindings.ToArray();
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private int GetPoseClipId(ref PoseData a_poseData)
        {
            switch (a_poseData.AnimType)
            {
                case EMxMAnimtype.Composite: { return m_processedData.Composites[a_poseData.AnimId].ClipIdA; }
                case EMxMAnimtype.IdleSet: { return a_poseData.AnimId; }
                case EMxMAnimtype.BlendSpace: { return m_processedData.BlendSpaces[a_poseData.AnimId].ClipIds[0]; }
                case EMxMAnimtype.Clip: { return m_processedData.ClipsData[a_poseData.AnimId].ClipId; }
                case EMxMAnimtype.BlendClip: { return m_processedData.BlendClips[a_poseData.AnimId].ClipIds[0]; }
                default: { return 0; }
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private int GetPoseClipIdPreProcess(ref PoseData a_poseData)
        {

            switch (a_poseData.AnimType)
            {
                case EMxMAnimtype.Composite: { return m_compositeDataList[a_poseData.AnimId].ClipIdA; }
                case EMxMAnimtype.IdleSet: { return a_poseData.AnimId; }
                case EMxMAnimtype.BlendSpace: { return m_blendSpaceList[a_poseData.AnimId].ClipIds[0]; }
                case EMxMAnimtype.Clip: { return m_clipDataList[a_poseData.AnimId].ClipId; }
                case EMxMAnimtype.BlendClip: { return m_blendClipDataList[a_poseData.AnimId].ClipIds[0]; }
                default: { return 0; }
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void CombineEventNamingModules()
        {
            //Get the event names list from the pre-processor
            if(m_data.OverrideEventModule == null)
            {
                m_combinedEventNames = new List<string>(m_data.EventNames);
            }
            else
            {
                m_combinedEventNames = new List<string>(m_data.OverrideEventModule.EventNames);
            }

            //Get the list event names from each module and add them into the list 
            //but only if they don't already exist
            foreach(AnimationModule animModule in m_data.AnimationModules)
            {
                if (animModule == null || animModule.OverrideEventModule == null
                    || animModule.OverrideEventModule.EventNames == null)
                {
                    continue;
                }

                List<string> moduleEvents = animModule.OverrideEventModule.EventNames;

                foreach(string modEvtName in moduleEvents)
                {
                    if (!m_combinedEventNames.Contains(modEvtName))
                    {
                        m_combinedEventNames.Add(modEvtName);
                    }
                }    
            }
        }

        //============================================================================================
        /**
        *  @brief Compiles a list of curve names across all MxMAnims
        *         
        *********************************************************************************************/
        private void AddCurveBindingIfItDoesNotExist(string a_curveName)
        {
            if (a_curveName == null)
                return;

            if (m_curveBindings == null || m_curveBindings.Contains(a_curveName))
                return;

            m_curveBindings.Add(a_curveName);
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private int GetCurveBindingId(string a_curveName)
        {
            for (int i = 0; i < m_curveBindings.Count; ++i)
            { 
                if(a_curveName == m_curveBindings[i])
                {
                    return i;
                }
            }

            return -1;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void CombineIdleSetTraits()
        {
            m_combinedIdleTraits = new List<string>(5);

            foreach(AnimationModule animModule in m_data.AnimationModules)
            {
                if (animModule == null 
                    || animModule.AnimationIdleSets == null
                    || animModule.AnimationIdleSets.Count == 0)
                {
                    continue;
                }

                List<MxMAnimationIdleSet> idleSets = animModule.AnimationIdleSets;

                foreach(MxMAnimationIdleSet idleSet in idleSets)
                {
                    if (idleSet.Traits == null || idleSet.Traits.Length == 0)
                        continue;

                    for(int i = 0; i < idleSet.Traits.Length; ++i)
                    {
                        string traitName = idleSet.Traits[i];

                        if (traitName == "")
                            continue;

                        if (m_combinedIdleTraits.Contains(traitName))
                            continue;

                        m_combinedIdleTraits.Add(traitName);
                    }
                }
            }

            if(m_combinedIdleTraits.Count > 32)
            {
                Debug.LogWarning("Pre-Process: You have too many idle set traits. There is a limit of 32. Please" +
                    "rationalize your idle traits and ensure you don't have any duplicates with spelling mistakes.");
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private int GetEventIdByName(string a_eventName)
        {
            for(int i = 0; i < m_combinedEventNames.Count; ++i)
            {
                if(a_eventName == m_combinedEventNames[i])
                {
                    return i;
                }
            }

            return -1;
        }

    }//End of class: MxMPreProcessor
}//End of namespace: MxMEditor