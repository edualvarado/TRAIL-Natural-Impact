using System.Collections.Generic;
using UnityEngine;
using MxM;

namespace MxMEditor
{
    public interface IMxMAnim
    {
        //Properties
        MxMPreProcessData TargetPreProcess { get; }
        AnimationModule TargetAnimModule { get; }
        GameObject TargetModel { get; }
        float AnimLength { get; }
        EMxMAnimtype AnimType { get; }
        AnimationClip FinalClip { get; }
        AnimationClip TargetClip { get; }
        ETags AnimGlobalTags { get; }
        ETags AnimGlobalFavourTags { get; }
        List<PoseData> AnimPoseList { get; }
        float PlaybackSpeed { get; set; }
        List<EventMarker> EventMarkers { get; }
        List<EventMarker> FinalEventMarkers { get; }
        List<AnimationClip> AnimBeforeClips { get; }
        List<AnimationClip> AnimAfterClips { get; }
        MotionModifyData AnimMotionModifier { get; }
        List<TagTrack> AnimTagTracks { get; }
        List<TagTrack> AnimFavourTagTracks { get; }

        void CopyTagsAndEvents(IMxMAnim a_target, bool a_mirrored = false);

        bool IsLooping { get; }
        bool UseIgnoreEdges { get; }
        bool UseExtrapolateTrajectory { get; }
        bool UseFlattenTrajectory { get; }
        bool UseRuntimeSplicing { get; }
        bool UsingSpeedMods { get; set; }
        List<TagTrack> FinalTagTracks { get; }
        List<TagTrack> FinalFavourTagTracks { get; }
        AnimationClip AnimGeneratedClip { get; }
        List<TagTrackBase> GenericTagTracks { get; }
        List<TagTrackBase> FinalGenericTagTracks { get; }
        List<TagTrackBase> UserTagTracks { get; }
        List<TagTrackBase> FinalUserTagTracks { get; }
        List<MxMCurveTrack> CurveTracks { get; }


        //Functions
        (List<Vector3> posLookup, List<Quaternion> rotLookup) GetRootLookupTable();
        (Vector3 pos, Quaternion rot) GetRoot(float a_time);
        float GetAverageRootSpeed(float a_startTime, float a_endTime);
        void InitPoseDataList();
        void InitTagTracks();
        void VerifyData();

        void GenerateModifiedAnimation(MxMPreProcessData a_data, string a_folderName);
        void ScrapGeneratedClips();
        void AddToPoseList(ref PoseData newPose);
        void AddEvent(float a_time);

        void OnDeleteEventMarker(object a_eventObj);
        void OnDeleteTag(object a_tagObject);
        void OnDeleteMotionSection(object a_motionObj);

        bool CheckAnimationCompatibility(bool a_human);

        int GetTrackId();
        void SetTrackId(int a_trackId);
        
    }//End of interface IMxMAnim
}//End of namespace: MxMEditor
