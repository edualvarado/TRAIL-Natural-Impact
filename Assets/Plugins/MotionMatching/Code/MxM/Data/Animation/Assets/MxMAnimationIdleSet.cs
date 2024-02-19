using System.Collections.Generic;
using UnityEngine;
using MxM;

namespace MxMEditor
{
    public class MxMAnimationIdleSet : ScriptableObject, IMxMAnim
    {
        //Clips
        [SerializeField] public AnimationClip PrimaryClip;
        [SerializeField] public List<AnimationClip> SecondaryClips = new List<AnimationClip>();
        [SerializeField] public List<AnimationClip> TransitionClips = new List<AnimationClip>();

        [SerializeField] public ETags Tags = ETags.None;
        [SerializeField] public ETags FavourTags = ETags.None;
        [SerializeField] public int MinLoops = 1;
        [SerializeField] public int MaxLoops = 2;

        [SerializeField] public string[] Traits;

        [SerializeField] public float RuntimePlaybackSpeed = 1f;

        //Reference Data
        [SerializeField] private MxMPreProcessData m_targetPreProcessData;
        [SerializeField] private AnimationModule m_targetAnimModule;

        [System.NonSerialized] public List<PoseData> PoseList = new List<PoseData>();

        public MxMPreProcessData TargetPreProcess
        {
            get { return m_targetPreProcessData; }
            set { m_targetPreProcessData = value; }
        }

        public AnimationModule TargetAnimModule
        {
            get { return m_targetAnimModule; }
            set { m_targetAnimModule = value; }
        }

        public void CopyData(MxMAnimationIdleSet a_copy, bool a_mirrored=false)
        {
#if UNITY_EDITOR
            if (a_mirrored)
            {
                PrimaryClip = MxMUtility.FindMirroredClip(PrimaryClip);

                SecondaryClips = new List<AnimationClip>(a_copy.SecondaryClips.Count + 1);
                foreach (AnimationClip clip in a_copy.SecondaryClips)
                {
                    SecondaryClips.Add(MxMUtility.FindMirroredClip(clip));
                }

                TransitionClips = new List<AnimationClip>(a_copy.TransitionClips.Count + 1);
                foreach (AnimationClip clip in a_copy.TransitionClips)
                {
                    TransitionClips.Add(MxMUtility.FindMirroredClip(clip));
                }
            }
            else
#endif
            {
                PrimaryClip = a_copy.PrimaryClip;
                SecondaryClips = new List<AnimationClip>(a_copy.SecondaryClips);
                TransitionClips = new List<AnimationClip>(a_copy.TransitionClips);
            }
            
            Tags = a_copy.Tags;
            FavourTags = a_copy.FavourTags;
            MinLoops = a_copy.MinLoops;
            MaxLoops = a_copy.MaxLoops;

            m_targetPreProcessData = a_copy.m_targetPreProcessData;
            m_targetAnimModule = a_copy.m_targetAnimModule;

            PoseList = null;
        }

        public void SetPrimaryAnim(AnimationClip a_clip)
        {
            if (a_clip != null)
            {
                PrimaryClip = a_clip;
                Tags = ETags.None;
                FavourTags = ETags.None;
            }
        }

        public void AddSecondaryAnim(AnimationClip a_clip)
        {
            if(a_clip != null)
                SecondaryClips.Add(a_clip);
        }

        public void AddTransitionAnim(AnimationClip a_clip)
        {
            if (a_clip != null)
                TransitionClips.Add(a_clip);
        }

        public void VerifyData()
        {
            for(int i = SecondaryClips.Count - 1; i >= 0; --i)
            {
                if (SecondaryClips[i] == null)
                    SecondaryClips.RemoveAt(i);
            }

            for(int i = TransitionClips.Count - 1; i >= 0; --i)
            {
                if (TransitionClips[i] == null)
                    TransitionClips.RemoveAt(i);
            }
        }

        #region IMxMAnim
        public float AnimLength { get { return PrimaryClip.length; } }
        public EMxMAnimtype AnimType { get { return EMxMAnimtype.IdleSet; } }
        public AnimationClip FinalClip { get { return PrimaryClip; } }
        public AnimationClip TargetClip { get { return PrimaryClip; } }
        public ETags AnimGlobalTags { get { return Tags; } }
        public ETags AnimGlobalFavourTags { get { return FavourTags; } }
        public List<PoseData> AnimPoseList { get { return PoseList; } }
        public float PlaybackSpeed { get { return RuntimePlaybackSpeed; } set { RuntimePlaybackSpeed = value; }}
        public List<EventMarker> FinalEventMarkers { get { return null; } }
        public List<EventMarker> EventMarkers { get { return null; } }
        public List<TagTrack> FinalTagTracks { get { return null; } }
        public List<TagTrack> FinalFavourTagTracks { get { return null; } }
        public List<TagTrack> AnimTagTracks { get { return null; } }
        public List<TagTrack> AnimFavourTagTracks { get { return null; } }
        public List<TagTrackBase> GenericTagTracks { get { return null; } }
        public List<TagTrackBase> FinalGenericTagTracks { get { return null; } }
        public List<TagTrackBase> UserTagTracks { get { return null; } }
        public List<TagTrackBase> FinalUserTagTracks { get { return null; } }
        public void CopyTagsAndEvents(IMxMAnim a_target, bool a_mirrored) { }
        public List<AnimationClip> AnimBeforeClips { get { return null; } }
        public List<AnimationClip> AnimAfterClips { get { return null; } }
        public MotionModifyData AnimMotionModifier { get { return null; } }
        public AnimationClip AnimGeneratedClip { get { return null; } }
        public GameObject TargetModel { get { return null; } }
        public bool IsLooping { get { return true; } }
        public bool UseIgnoreEdges { get { return false; } }
        public bool UseExtrapolateTrajectory { get { return false; } }
        public bool UseFlattenTrajectory { get { return true; } }
        public bool UseRuntimeSplicing { get { return false; } }
        public bool UsingSpeedMods { get { return false; } set { } }

        public void OnDeleteEventMarker(object a_eventObj) { }
        public void OnDeleteTag(object a_tagObj) { }
        public void OnDeleteMotionSection(object a_motionObj) { }
        public void InitTagTracks() { }
        public void AddEvent(float _time) { }
        public void ScrapGeneratedClips() { }
        public void GenerateModifiedAnimation(MxMPreProcessData a_preProcessData, string a_directory) { }
        public (List<Vector3> posLookup, List<Quaternion> rotLookup) GetRootLookupTable() { return (null, null); }
        public (Vector3 pos, Quaternion rot) GetRoot(float a_time) { return (Vector3.zero, Quaternion.identity); }
        public float GetAverageRootSpeed(float a_startTime, float a_endTime) { return 0f; }
        public void InitPoseDataList() { PoseList = new List<PoseData>(); }
        public void SetTrackId(int a_trackId) { }
        public int GetTrackId() { return -1; }
        public List<MxMCurveTrack> CurveTracks { get { return null; } }

        public void AddToPoseList(ref PoseData a_newPose)
        {
            if (PoseList == null)
                PoseList = new List<PoseData>();

            PoseList.Add(a_newPose);
        }

        public bool CheckAnimationCompatibility(bool a_useGeneric)
        {

            if (PrimaryClip.isHumanMotion == a_useGeneric)
                return false;

            foreach (AnimationClip clip in SecondaryClips)
            {
                if (clip.isHumanMotion == a_useGeneric)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

    }//End of class: MxMAnimationIdleSet
}//End of namespace: MxM