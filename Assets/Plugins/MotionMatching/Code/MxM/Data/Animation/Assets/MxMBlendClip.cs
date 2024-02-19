using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using MxM;
using UnityEngine.Serialization;

namespace MxMEditor
{
    [System.Serializable]
    public class MxMBlendClip : IMxMAnim
    {
        public MxMBlendSpace SourceBlendSpace;
        public Vector2 Position;
        public float[] Weightings;
        public bool NormalizeTime;

        public float RuntimePlaybackSpeed = 1f;

        public float NormalizedLength = 0f;

        [System.NonSerialized]
        public List<PoseData> PoseList = new List<PoseData>();

        public List<AnimationClip> FinalClips { get { return SourceBlendSpace.FinalClips; } }

        public MxMBlendClip(MxMBlendSpace a_sourceBlendSpace, Vector2 a_position)
        {
            SourceBlendSpace = a_sourceBlendSpace;
            Position = a_position;
            NormalizeTime = a_sourceBlendSpace.NormalizeTime;
            RuntimePlaybackSpeed = SourceBlendSpace.RuntimePlaybackSpeed;

            Weightings = SourceBlendSpace.CalculateWeightings(Position);
            
            List<AnimationClip> clips = SourceBlendSpace.FinalClips;
            
            NormalizedLength = 0f;
            for (int i = 0; i < Weightings.Length; ++i)
            {
                NormalizedLength += clips[i].length * Weightings[i];
            }
        }

        public void SetTrackId(int a_trackId) { }

        public int GetTrackId()
        {
            return SourceBlendSpace.GetTrackId();
        }

        public void SetupPlayables(ref PlayableGraph a_playableGraph, ref AnimationMixerPlayable a_mixer, float a_startTime)
        {
            List<AnimationClip> clips = SourceBlendSpace.FinalClips;

            a_mixer.SetInputCount(clips.Count);
            
            for (int i = 0; i < clips.Count; ++i)
            {
                AnimationClip clip = clips[i];
                var clipPlayable = AnimationClipPlayable.Create(a_playableGraph, clip);
                clipPlayable.SetApplyFootIK(true);

                float normalizedClipSpeed;
                if (SourceBlendSpace.NormalizeTime)
                {
                    normalizedClipSpeed = clip.length / NormalizedLength;
                }
                else
                {
                    normalizedClipSpeed = 1f;
                }

                clipPlayable.SetTime(a_startTime * normalizedClipSpeed);
                clipPlayable.SetTime(a_startTime * normalizedClipSpeed);
                a_mixer.ConnectInput(i, clipPlayable, 0, Weightings[i]);
                clipPlayable.SetSpeed(normalizedClipSpeed);
            }
        }

        #region IMxMAnim
        //IMxMAnim Basic Properties
        public MxMPreProcessData TargetPreProcess { get { return SourceBlendSpace.TargetPreProcess; } }
        public AnimationModule TargetAnimModule { get { return SourceBlendSpace.TargetAnimModule; } }
        public GameObject TargetModel { get { return SourceBlendSpace.TargetModel; } }
        public EMxMAnimtype AnimType { get { return EMxMAnimtype.BlendClip; } }
        public ETags AnimGlobalTags { get { return SourceBlendSpace.GlobalTags; } }
        public ETags AnimGlobalFavourTags { get { return SourceBlendSpace.GlobalFavourTags; } }
        public List<PoseData> AnimPoseList { get { return PoseList; } }
        public float PlaybackSpeed { get { return RuntimePlaybackSpeed; } set { RuntimePlaybackSpeed = value; }}
        public List<EventMarker> FinalEventMarkers { get { return null; } }
        public List<EventMarker> EventMarkers { get { return null; } }
        public List<AnimationClip> AnimBeforeClips { get { return null; } }
        public List<AnimationClip> AnimAfterClips { get { return null; } }
        public MotionModifyData AnimMotionModifier { get { return null; } }
        public float AnimLength { get { return SourceBlendSpace.NormalizeTime ? NormalizedLength : SourceBlendSpace.AnimLength; } }
        public bool IsLooping { get { return true; } }
        public bool UseIgnoreEdges { get { return false; } }
        public bool UseExtrapolateTrajectory { get { return false; } }
        public bool UseFlattenTrajectory { get { return false; } }
        public bool UseRuntimeSplicing { get { return false; } }
        public bool UsingSpeedMods { get { return false; } set { } }
        public List<TagTrack> AnimTagTracks { get { return SourceBlendSpace.AnimTagTracks; } }
        public List<TagTrack> AnimFavourTagTracks { get { return SourceBlendSpace.AnimFavourTagTracks; } }
        public AnimationClip FinalClip { get { return SourceBlendSpace.TargetClip; } }
        public AnimationClip TargetClip { get { return SourceBlendSpace.TargetClip; } }
        public List<TagTrack> FinalTagTracks { get { return SourceBlendSpace.AnimTagTracks; } }
        public List<TagTrack> FinalFavourTagTracks { get { return SourceBlendSpace.AnimFavourTagTracks; } }
        public List<TagTrackBase> GenericTagTracks { get { return SourceBlendSpace.GenericTagTracks; } }
        public List<TagTrackBase> FinalGenericTagTracks { get { return SourceBlendSpace.GenericTagTracks; } }
        public List<TagTrackBase> UserTagTracks { get { return SourceBlendSpace.UserTagTracks; } }
        public List<TagTrackBase> FinalUserTagTracks { get { return SourceBlendSpace.FinalGenericTagTracks; } }
        public List<MxMCurveTrack> CurveTracks {  get { return SourceBlendSpace.Curves; } }

        //Unused IMxMAnim Functions
        public void CopyTagsAndEvents(IMxMAnim a_target, bool a_mirrored) { }
        public void OnDeleteEventMarker(object a_eventObj) { }
        public void OnDeleteTag(object a_tagObj) { }
        public void OnDeleteMotionSection(object a_motionObj) { }
        public void AddEvent(float _time) { }
        public void ScrapGeneratedClips() { }
        public void GenerateModifiedAnimation(MxMPreProcessData a_preProcessData, string a_directory) { }
        public (List<Vector3> posLookup, List<Quaternion> rotLookup) GetRootLookupTable() { return (null, null); }
        public (Vector3 pos, Quaternion rot) GetRoot(float a_time) { return (Vector3.zero, Quaternion.identity); }
        public float GetAverageRootSpeed(float a_startTime, float a_endTime) { return 0f; }
        public AnimationClip AnimGeneratedClip { get { return null; } }
        public void VerifyData() { }

        //IMxMAnim Functions
        public void InitPoseDataList() { PoseList = new List<PoseData>(); }
        public void InitTagTracks() { SourceBlendSpace.InitTagTracks(); }
        public bool CheckAnimationCompatibility(bool a_useGeneric) { return SourceBlendSpace.CheckAnimationCompatibility(a_useGeneric); }

        public void AddToPoseList(ref PoseData a_newPose)
        {
            if (PoseList == null)
                PoseList = new List<PoseData>();

            PoseList.Add(a_newPose);
        }

        #endregion
    }
}
