using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using MxM;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MxMEditor
{
    public class MxMAnimationClipComposite : ScriptableObject, IMxMAnim
    {
        //Name
        [SerializeField] public string CompositeName;
        
        //Clips
        [SerializeField] public AnimationClip PrimaryClip;
        [SerializeField] public List<AnimationClip> BeforeClips;
        [SerializeField] public List<AnimationClip> AfterClips;

        //Settings
        [SerializeField] public bool Looping;
        [SerializeField] public bool IgnoreEdges = false;
        [SerializeField] public bool ExtrapolateTrajectory = true;
        [SerializeField] public bool FlattenTrajectory = false;
        [SerializeField] public bool RuntimeSplicing = false;

        //Tags and events
        [SerializeField] public ETags GlobalTags = ETags.None;
        [SerializeField] public ETags GlobalFavourTags = ETags.None;
        [SerializeField] public List<TagTrack> TagTracks;
        [SerializeField] public List<TagTrack> FavourTagTracks;
        [SerializeField] public List<EventMarker> Events;

        //Generic Tags
        [SerializeField] public FootStepTagTrack LeftFootStepTrack;
        [SerializeField] public FootStepTagTrack RightFootStepTrack;
        [SerializeField] public TagTrackBase WarpPositionTrack;
        [SerializeField] public TagTrackBase WarpRotationTrack;
        [SerializeField] public TagTrackBase EnableRootMotionTrack;
        [SerializeField] public FloatTagTrack PoseFavourTrack;
        [SerializeField] public TagTrackBase WarpTrajLatTrack;
        [SerializeField] public TagTrackBase WarpTrajLongTrack;
        [SerializeField] public List<TagTrackBase> UserBoolTracks;
        
        //Runtime Playback Speed
        [SerializeField] public float RuntimePlaybackSpeed = 1f;
        
        //Speed Modification Data
        [SerializeField] public bool UseSpeedMods;
        [SerializeField] public MotionModifyData MotionModifier;
        [SerializeField] public AnimationClip GeneratedClip;

        [SerializeField] public List<TagTrack> GeneratedTagTracks;
        [SerializeField] public List<TagTrack> GeneratedFavourTagTracks;
        [SerializeField] public List<EventMarker> GeneratedEvents;

        [SerializeField] public FootStepTagTrack GeneratedLeftFootStepTrack;
        [SerializeField] public FootStepTagTrack GeneratedRightFootStepTrack;
        [SerializeField] public TagTrackBase GeneratedWarpPositionTrack;
        [SerializeField] public TagTrackBase GeneratedWarpRotationTrack;
        [SerializeField] public TagTrackBase GeneratedEnableRootMotionTrack;
        [SerializeField] public FloatTagTrack GeneratedPoseFavourTrack;
        [SerializeField] public TagTrackBase GeneratedWarpTrajLatTrack;
        [SerializeField] public TagTrackBase GeneratedWarpTrajLongTrack;
        [SerializeField] public List<TagTrackBase> GeneratedUserBoolTracks;

        //Curves
        [SerializeField] public List<MxMCurveTrack> Curves;

        //Reference Data
        [SerializeField] private MxMPreProcessData m_targetPreProcessData;
        [SerializeField] private AnimationModule m_targetAnimModule;
        [SerializeField] private GameObject m_targetPrefab;

        //Pre-process Working Data
        [System.NonSerialized] public List<PoseData> PoseList = new List<PoseData>();

        //Preview Data
        private List<Vector3> m_rootPosLookupTable;
        private List<Quaternion> m_rootRotLookupTable;
        private List<float> m_rootSpeedLookupTable;

        private int m_trackId = -1;

        public int CategoryId { get; set; }

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

        public GameObject TargetPrefab
        {
            get { return m_targetPrefab; }
            set { m_targetPrefab = value; }
        }

        public void CopyData(MxMAnimationClipComposite a_copy, bool a_mirrored = false)
        {
 #if UNITY_EDITOR           
            if (a_mirrored)
            {
                PrimaryClip = MxMUtility.FindMirroredClip(a_copy.PrimaryClip);
                
                BeforeClips = new List<AnimationClip>(a_copy.BeforeClips.Count + 1);
                foreach (AnimationClip clip in a_copy.BeforeClips)
                {
                    if (clip == null)
                        continue;
                    
                    BeforeClips.Add(MxMUtility.FindMirroredClip(clip));
                }
                
                AfterClips = new List<AnimationClip>(a_copy.AfterClips.Count + 1);
                foreach (AnimationClip clip in a_copy.AfterClips)
                {
                    if (clip == null)
                        continue;
                    
                    AfterClips.Add(MxMUtility.FindMirroredClip(clip));
                }
            }
            else
#endif
            {
                PrimaryClip = a_copy.PrimaryClip;
                BeforeClips = new List<AnimationClip>(a_copy.BeforeClips);
                AfterClips = new List<AnimationClip>(a_copy.AfterClips);
            }

            CompositeName = a_copy.CompositeName;
            Looping= a_copy.Looping;
            IgnoreEdges = a_copy.IgnoreEdges;
            ExtrapolateTrajectory = a_copy.ExtrapolateTrajectory;
            FlattenTrajectory = a_copy.FlattenTrajectory;
            RuntimeSplicing = a_copy.RuntimeSplicing;
            UseSpeedMods = a_copy.UseSpeedMods;

            GlobalTags = a_copy.GlobalTags;
            GlobalFavourTags = a_copy.GlobalFavourTags;

            TagTracks = new List<TagTrack>(a_copy.TagTracks.Count + 1);
            foreach(TagTrack track in a_copy.TagTracks)
            {
                TagTracks.Add(new TagTrack(track));
            }

            FavourTagTracks = new List<TagTrack>(a_copy.FavourTagTracks.Count + 1);
            foreach(TagTrack track  in a_copy.FavourTagTracks)
            {
                FavourTagTracks.Add(new TagTrack(track));
            }
            
            UserBoolTracks = new List<TagTrackBase>(a_copy.UserBoolTracks.Count + 1);
            foreach(TagTrackBase track in a_copy.UserBoolTracks)
            {
                UserBoolTracks.Add(new TagTrackBase(track));
            }

            Events = new List<EventMarker>(a_copy.Events.Count + 1);
            foreach(EventMarker marker in a_copy.Events)
            {
                Events.Add(new EventMarker(marker));
            }

            m_targetPreProcessData = a_copy.m_targetPreProcessData;
            m_targetAnimModule = a_copy.m_targetAnimModule;
            m_targetPrefab = a_copy.m_targetPrefab;

            PoseList = null;

            if (a_mirrored)
            {
                LeftFootStepTrack = new FootStepTagTrack(a_copy.RightFootStepTrack);
                LeftFootStepTrack.Name = a_copy.LeftFootStepTrack.Name;
                RightFootStepTrack = new FootStepTagTrack(a_copy.LeftFootStepTrack);
                RightFootStepTrack.Name = a_copy.RightFootStepTrack.Name;
            }
            else
            {
                LeftFootStepTrack = new FootStepTagTrack(a_copy.LeftFootStepTrack);
                RightFootStepTrack = new FootStepTagTrack(a_copy.RightFootStepTrack);
            }
            WarpPositionTrack = new TagTrackBase(a_copy.WarpPositionTrack);
            WarpRotationTrack = new TagTrackBase(a_copy.WarpRotationTrack);
            EnableRootMotionTrack = new TagTrackBase(a_copy.EnableRootMotionTrack);
            PoseFavourTrack = new FloatTagTrack(a_copy.PoseFavourTrack);
            WarpTrajLatTrack = new TagTrackBase(a_copy.WarpTrajLatTrack);
            WarpTrajLongTrack = new TagTrackBase(a_copy.WarpTrajLongTrack);

            MotionModifier = new MotionModifyData(a_copy.MotionModifier, this);
            
            //Todo: Mirror events
        }

        public void OnEnable()
        {
            ValidateBaseData();

            if (Curves == null)
                Curves = new List<MxMCurveTrack>(3);
        }

        public void ValidateBaseData()
        {
            bool modified = false;

            if (BeforeClips == null)
                BeforeClips = new List<AnimationClip>();

            if (AfterClips == null)
                AfterClips = new List<AnimationClip>();

            if (MotionModifier == null)
                MotionModifier = new MotionModifyData();

            if (Events == null)
                Events = new List<EventMarker>();

            if (TagTracks == null)
                TagTracks = new List<TagTrack>();

            if (FavourTagTracks == null)
                FavourTagTracks = new List<TagTrack>();

            if (UserBoolTracks == null)
                UserBoolTracks = new List<TagTrackBase>();
            
            if (GeneratedEvents == null)
                GeneratedEvents = new List<EventMarker>();

            if (GeneratedTagTracks == null)
                GeneratedTagTracks = new List<TagTrack>();

            if (GeneratedFavourTagTracks == null)
                GeneratedFavourTagTracks = new List<TagTrack>();

            if (GeneratedUserBoolTracks == null)
                GeneratedUserBoolTracks = new List<TagTrackBase>();

            if (PrimaryClip != null)
            {
                if (LeftFootStepTrack == null || LeftFootStepTrack.Name == null || LeftFootStepTrack.Name == "")
                {
                    LeftFootStepTrack = new FootStepTagTrack(0, "Footstep Left", PrimaryClip.length);
                    modified = true;
                }

                if (RightFootStepTrack == null || RightFootStepTrack.Name == null || RightFootStepTrack.Name == "")
                {
                    RightFootStepTrack = new FootStepTagTrack(1, "Footstep Right", PrimaryClip.length);
                    modified = true;
                }

                if (WarpPositionTrack == null || WarpPositionTrack.Name == null || WarpPositionTrack.Name == "")
                {
                    WarpPositionTrack = new TagTrackBase(2, "Disable Warp (Position)", PrimaryClip.length);
                    modified = true;
                }

                if (WarpRotationTrack == null || WarpRotationTrack.Name == null || WarpRotationTrack.Name == "")
                {
                    WarpRotationTrack = new TagTrackBase(3, "Disable Warp (Rotation)", PrimaryClip.length);
                    modified = true;
                }

                if (EnableRootMotionTrack == null || EnableRootMotionTrack.Name == null || EnableRootMotionTrack.Name != "Disable Matching")
                {
                    EnableRootMotionTrack = new TagTrackBase(4, "Disable Matching", PrimaryClip.length);
                    modified = true;
                }

                if (PoseFavourTrack == null || PoseFavourTrack.Name == null || PoseFavourTrack.Name == "")
                {
                    PoseFavourTrack = new FloatTagTrack(5, "Pose Favour", PrimaryClip.length);
                    modified = true;
                }

                if (WarpTrajLatTrack == null || WarpTrajLatTrack.Name == null || WarpTrajLatTrack.Name == "")
                {
                    WarpTrajLatTrack = new TagTrackBase(6, "Disable Trajectory Warp (Angular)", PrimaryClip.length);
                    modified = true;
                }

                if (WarpTrajLongTrack == null || WarpTrajLongTrack.Name == null || WarpTrajLongTrack.Name == "")
                {
                    WarpTrajLongTrack = new TagTrackBase(7, "Disable Trajectory Warp (Long)", PrimaryClip.length);
                    modified = true;
                }

                PoseFavourTrack.DefaultValue = 1f;
            }

            if (MotionModifier.OnEnable(this))
                modified = true;

            if (modified)
            {
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

        public void SetPrimaryAnim(AnimationClip a_clip)
        {
#if UNITY_EDITOR
            Undo.RecordObject(this, "Primary Clip Changed");

            PrimaryClip = a_clip;

            if (a_clip != null)
            {
                if (a_clip.isLooping)
                    Looping = true;
                else
                    IgnoreEdges = true;

                GlobalTags = ETags.None;
                GlobalFavourTags = ETags.None;
            }
#endif
        }

        public void GenerateRootLookupTable()
        {
            if (PrimaryClip != null)
            {
                if (m_targetPrefab == null)
                {
                    if (m_targetPreProcessData != null)
                    {
                        m_targetPrefab = m_targetPreProcessData.Prefab;
                    }
                    else if(m_targetAnimModule != null)
                    {
                        m_targetPrefab = m_targetAnimModule.Prefab;
                    }
                }

                if (m_targetPrefab != null)
                {
                    if (m_rootPosLookupTable == null)
                        m_rootPosLookupTable = new List<Vector3>();

                    if (m_rootRotLookupTable == null)
                        m_rootRotLookupTable = new List<Quaternion>();

                    if (m_rootSpeedLookupTable == null)
                        m_rootSpeedLookupTable = new List<float>();

                    m_rootPosLookupTable.Clear();
                    m_rootRotLookupTable.Clear();
                    m_rootSpeedLookupTable.Clear();

                    //Instantiate prefab and setup playable graph
                    GameObject previewPrefab = Instantiate(m_targetPrefab);
                    Animator previewAnimator = previewPrefab.GetComponent<Animator>();

                    if (previewAnimator == null)
                    {
                        previewAnimator = previewPrefab.AddComponent<Animator>();
                        previewAnimator.applyRootMotion = true;
                    }

                    PlayableGraph playableGraph = PlayableGraph.Create();
                    playableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);

                    var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", previewAnimator);
                    AnimationMixerPlayable animationMixer = AnimationMixerPlayable.Create(playableGraph, 1);
                    playableOutput.SetSourcePlayable(animationMixer);
                    var playableClip = AnimationClipPlayable.Create(playableGraph, PrimaryClip);
                    playableClip.SetApplyFootIK(true);
                    animationMixer.ConnectInput(0, playableClip, 0);
                    animationMixer.SetInputWeight(0, 1f);
                    playableClip.SetTime(0.0);
                    playableClip.SetTime(0.0);

                    previewPrefab.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                    playableGraph.Evaluate(0f);

                    //double sampleRate = 1.0 / PrimaryClip.frameRate;
                    double sampleRate = 1f / 60f;

                    //Fill the lookup table with root positions
                    for (double animTime = 0.0; animTime <= PrimaryClip.length; animTime += sampleRate)
                    {
                        m_rootPosLookupTable.Add(previewPrefab.transform.position);
                        m_rootRotLookupTable.Add(previewPrefab.transform.rotation);
                        playableGraph.Evaluate((float)sampleRate);
                    }

                    playableGraph.Destroy();
                    GameObject.DestroyImmediate(previewPrefab);


                    if (m_rootPosLookupTable.Count > 1)
                    {
                        //Generate the linear speed lookup table
                        for (int i = 1; i < m_rootPosLookupTable.Count; ++i)
                        {
                            Vector3 startPos = m_rootPosLookupTable[i - 1];
                            Vector3 endPos = m_rootPosLookupTable[i];

                            m_rootSpeedLookupTable.Add(Vector3.Distance(endPos, startPos) / (float)sampleRate);
                        }
                    }
                    else
                    {
                        m_rootSpeedLookupTable.Add(0f);
                    }

                    m_rootSpeedLookupTable.Insert(0, m_rootSpeedLookupTable[0]);
                }
            }
        }

        public void ClearRootLookupTable()
        {
            if (m_rootPosLookupTable != null)
                m_rootPosLookupTable.Clear();

            if (m_rootRotLookupTable != null)
                m_rootRotLookupTable.Clear();

            if (m_rootSpeedLookupTable != null)
                m_rootSpeedLookupTable.Clear();
        }

        private void GenerateRootLookupTableIfRequired()
        {
            if (PrimaryClip != null)
            {
                if (m_rootPosLookupTable == null || m_rootRotLookupTable == null
                        || m_rootPosLookupTable.Count == 0 || m_rootRotLookupTable.Count == 0)
                {
                    GenerateRootLookupTable();
                }
            }
        }

        public float GetRootSpeed(float a_time)
        {
            if (PrimaryClip != null)
            {
                GenerateRootLookupTableIfRequired();

                a_time = Mathf.Clamp(a_time, 0f, PrimaryClip.length);

                //float sampleRate = 1f / PrimaryClip.frameRate;
                float sampleRate = 1f / 60f;

                int startIndex = Mathf.Min(Mathf.FloorToInt(a_time / sampleRate), m_rootPosLookupTable.Count - 1);
                int nextIndex = Mathf.Min(startIndex + 1, m_rootPosLookupTable.Count - 1);

                float lerpVal = (a_time % sampleRate) / sampleRate;

                return Mathf.Lerp(m_rootSpeedLookupTable[startIndex], 
                    m_rootSpeedLookupTable[nextIndex], lerpVal);
            }

            return 0f;
        }

        public void VerifyData()
        {
            if (PrimaryClip != null)
            {
                ValidateBaseData();
                
                foreach (EventMarker evtMarker in Events)
                {
                    if (evtMarker.EventTime > PrimaryClip.length)
                    {
                        evtMarker.EventTime = PrimaryClip.length;
                    }
                }

                foreach (TagTrack tagTrack in TagTracks)
                {
                    tagTrack.VerifyData(PrimaryClip);
                }

                foreach (TagTrack favourTrack in FavourTagTracks)
                {
                    favourTrack.VerifyData(PrimaryClip);
                }

                LeftFootStepTrack.VerifyData(PrimaryClip);
                RightFootStepTrack.VerifyData(PrimaryClip);
                WarpPositionTrack.VerifyData(PrimaryClip);
                WarpRotationTrack.VerifyData(PrimaryClip);
                EnableRootMotionTrack.VerifyData(PrimaryClip);
                PoseFavourTrack.VerifyData(PrimaryClip);
                WarpTrajLatTrack.VerifyData(PrimaryClip);
                WarpTrajLongTrack.VerifyData(PrimaryClip);

                MotionModifier.VerifyData();
            }
        }

        public void VerifyEventMarkers(string[] a_eventNames)
        {
            foreach(EventMarker eventMarker in Events)
            {
                eventMarker.Validate(a_eventNames);
            }
        }

        public bool CheckAnimationCompatibility(bool a_useGeneric)
        {

            if (PrimaryClip.isHumanMotion == a_useGeneric)
                return false;

            foreach (AnimationClip clip in BeforeClips)
            {
                if (clip.isHumanMotion == a_useGeneric)
                {
                    return false;
                }
            }

            foreach (AnimationClip clip in AfterClips)
            {
                if (clip.isHumanMotion == a_useGeneric)
                {
                    return false;
                }
            }

            return true;
        }

        #region IMxMAnim
        public GameObject TargetModel { get { return m_targetPrefab; } }
        public ETags AnimGlobalTags { get { return GlobalTags; } }
        public ETags AnimGlobalFavourTags { get { return GlobalFavourTags; } }
        public AnimationClip TargetClip { get { return PrimaryClip; } }
        public List<PoseData> AnimPoseList { get { return PoseList; } }
        public float PlaybackSpeed { get { return RuntimePlaybackSpeed; } set { RuntimePlaybackSpeed = value; }}
        public MotionModifyData AnimMotionModifier { get { return MotionModifier; } }
        public AnimationClip AnimGeneratedClip { get { return GeneratedClip; } }
        public bool IsLooping { get { return Looping; } }
        public bool UseIgnoreEdges { get { return IgnoreEdges; } }
        public bool UseExtrapolateTrajectory { get { return ExtrapolateTrajectory; } }
        public bool UseFlattenTrajectory { get { return FlattenTrajectory; } }
        public bool UseRuntimeSplicing { get { return RuntimeSplicing; } }
        public bool UsingSpeedMods { get { return UseSpeedMods; } set { UseSpeedMods = value; } }
        public List<TagTrackBase> UserTagTracks { get { return UserBoolTracks; } }
        public void SetTrackId(int a_trackId) { m_trackId = a_trackId; }
        public int GetTrackId() { return m_trackId; }
        public List<MxMCurveTrack> CurveTracks { get { return Curves; } }

        public float AnimLength 
        {
            get
            {
                if (AfterClips == null || AfterClips.Count == 0)
                {
                    if (UseSpeedMods && GeneratedClip != null)
                        return GeneratedClip.length;

                    return PrimaryClip.length;

                }
                else
                {
                    if (UseSpeedMods && GeneratedClip != null)
                        return GeneratedClip.length + AfterClips[0].length;

                    return PrimaryClip.length + AfterClips[0].length;
                }
            }
        }

        public EMxMAnimtype AnimType
        {
            get
            {
                if (AfterClips == null || AfterClips.Count == 0)
                {
                    return EMxMAnimtype.Clip;
                }

                if (!RuntimeSplicing)
                {
                    return EMxMAnimtype.Clip;
                }

                return EMxMAnimtype.Composite;
            }
        }

        public AnimationClip FinalClip
        {
            get
            {
                if (GeneratedClip != null && UseSpeedMods)
                {
                    return GeneratedClip;
                }
                else
                {
                    return PrimaryClip;
                }
            }
        }

        public void OnDeleteEventMarker(object a_eventObj)
        {
#if UNITY_EDITOR
            if (a_eventObj == null)
                return;

            EventMarker eventMarker = a_eventObj as EventMarker;

            for (int i = 0; i < Events.Count; ++i)
            {
                if (eventMarker == Events[i])
                {
                    Undo.RecordObject(this, "Delete Event");
                    Events.Remove(eventMarker);
                    //MxMTaggingWindow.Inst().Repaint();
                    //MxMTaggingWindow.Inst().QueueDeselectEventAction();
                    break;
                }
            }
#endif
        }

        public void OnDeleteTag(object a_tagObj)
        {
#if UNITY_EDITOR
            if (a_tagObj == null)
                return;

            //Not Implemented
#endif
        }

        public void OnDeleteMotionSection(object a_motionObj)
        {
#if UNITY_EDITOR
            if (a_motionObj == null)
                return;

            //Not Implemented
#endif
        }


        public List<EventMarker> FinalEventMarkers 
        {
            get
            {
                if (GeneratedClip != null && UseSpeedMods)
                {
                    return GeneratedEvents;
                }
                else
                {
                    if (Events == null)
                        Events = new List<EventMarker>();

                    return Events;
                }
            }
        }

        public List<EventMarker> EventMarkers
        {
            get
            {
                if (Events == null)
                    Events = new List<EventMarker>();

                return Events;
            }
        }

        public List<TagTrack> FinalTagTracks
        {
            get
            {
                if (GeneratedClip != null && UseSpeedMods)
                {
                    if (GeneratedTagTracks == null)
                        GeneratedTagTracks = new List<TagTrack>();

                    return GeneratedTagTracks;
                }
                else
                {
                    if (TagTracks == null)
                        TagTracks = new List<TagTrack>();

                    return TagTracks;
                }
            }
        }

        public List<TagTrack> FinalFavourTagTracks
        {
            get
            {
                if (GeneratedClip != null && UseSpeedMods)
                {
                    if (GeneratedFavourTagTracks == null)
                        GeneratedFavourTagTracks = new List<TagTrack>();

                    return GeneratedFavourTagTracks;
                }
                else
                {
                    if (FavourTagTracks == null)
                        FavourTagTracks = new List<TagTrack>();

                    return FavourTagTracks;
                }
            }
        }

        public List<TagTrack> AnimTagTracks
        {
            get
            {
                if (TagTracks == null)
                    TagTracks = new List<TagTrack>();

                return TagTracks;
            }
        }

        public List<TagTrack> AnimFavourTagTracks
        {
            get
            {
                if (FavourTagTracks == null)
                    FavourTagTracks = new List<TagTrack>();

                return FavourTagTracks;
            }
        }

        public List<TagTrackBase> GenericTagTracks
        {
            get
            {
                List<TagTrackBase> genericTagTracks = new List<TagTrackBase>();

                genericTagTracks.Add(LeftFootStepTrack);
                genericTagTracks.Add(RightFootStepTrack);
                genericTagTracks.Add(WarpPositionTrack);
                genericTagTracks.Add(WarpRotationTrack);
                genericTagTracks.Add(EnableRootMotionTrack);
                genericTagTracks.Add(PoseFavourTrack);
                genericTagTracks.Add(WarpTrajLatTrack);
                genericTagTracks.Add(WarpTrajLongTrack);

                return genericTagTracks;
            }
        }

        public List<TagTrackBase> FinalGenericTagTracks
        {
            get
            {
                List<TagTrackBase> genericTagTracks = new List<TagTrackBase>();

                if (GeneratedClip != null && UseSpeedMods)
                {
                    genericTagTracks.Add(GeneratedLeftFootStepTrack);
                    genericTagTracks.Add(GeneratedRightFootStepTrack);
                    genericTagTracks.Add(GeneratedWarpPositionTrack);
                    genericTagTracks.Add(GeneratedWarpRotationTrack);
                    genericTagTracks.Add(GeneratedEnableRootMotionTrack);
                    genericTagTracks.Add(GeneratedPoseFavourTrack);
                    genericTagTracks.Add(GeneratedWarpTrajLatTrack);
                    genericTagTracks.Add(GeneratedWarpTrajLongTrack);
                }
                else
                {
                    genericTagTracks.Add(LeftFootStepTrack);
                    genericTagTracks.Add(RightFootStepTrack);
                    genericTagTracks.Add(WarpPositionTrack);
                    genericTagTracks.Add(WarpRotationTrack);
                    genericTagTracks.Add(EnableRootMotionTrack);
                    genericTagTracks.Add(PoseFavourTrack);
                    genericTagTracks.Add(WarpTrajLatTrack);
                    genericTagTracks.Add(WarpTrajLongTrack);
                }

                return genericTagTracks;
            }
        }

        public List<TagTrackBase> FinalUserTagTracks
        {
            get
            {
                if (GeneratedClip != null && UseSpeedMods)
                {
                    return GeneratedUserBoolTracks;
                }
                else
                {
                    return UserBoolTracks;
                }
            }
        }

        public void CopyTagsAndEvents(IMxMAnim a_target, bool a_mirrored)
        {
            //Copy Events
            List<EventMarker> targetEvents = a_target.EventMarkers;

            if (targetEvents != null)
            {
                Events = new List<EventMarker>(targetEvents.Count + 1);

                foreach (EventMarker evt in targetEvents)
                {
                    if(evt != null)
                        Events.Add(new EventMarker(evt));
                }
            }

            //Copy Require tag tracks
            List<TagTrack> targetTagTracks = a_target.AnimTagTracks;
            if (targetTagTracks != null)
            {
                TagTracks = new List<TagTrack>(targetTagTracks.Count + 1);
                foreach (TagTrack track in targetTagTracks)
                {
                    if (track != null)
                    {
                        TagTracks.Add(new TagTrack(track));
                    }
                }
            }

            //Copy Favour tag tracks
            List<TagTrack> targetFavourTracks = a_target.AnimFavourTagTracks;
            if (FavourTagTracks != null)
            {
                FavourTagTracks = new List<TagTrack>(targetFavourTracks.Count + 1);
                foreach (TagTrack track in targetFavourTracks)
                {
                    if(track != null)
                        FavourTagTracks.Add(new TagTrack(track));
                }
            }

            //Copy User Tags
            List<TagTrackBase> userTagTracks = a_target.UserTagTracks;
            if (userTagTracks != null)
            {
                UserBoolTracks = new List<TagTrackBase>(userTagTracks.Count + 1);
                foreach (TagTrackBase track in userTagTracks)
                {
                    if(track != null)
                        UserBoolTracks.Add(track);
                }
            }

            //Copy Utility Tags
            List<TagTrackBase> utilityTagTracks = a_target.GenericTagTracks;

            if (utilityTagTracks != null)
            {
                if (a_mirrored)
                {
                    if (utilityTagTracks.Count > 0)
                    {
                        RightFootStepTrack = new FootStepTagTrack(utilityTagTracks[0] as FootStepTagTrack);
                        RightFootStepTrack.Name = "Footstep Right";
                    }

                    if (utilityTagTracks.Count > 1)
                    {
                        LeftFootStepTrack = new FootStepTagTrack(utilityTagTracks[1] as FootStepTagTrack);
                        LeftFootStepTrack.Name = "Footstep Left";
                    }
                }
                else
                {
                    if (utilityTagTracks.Count > 0)
                        LeftFootStepTrack = new FootStepTagTrack(utilityTagTracks[0] as FootStepTagTrack);

                    if (utilityTagTracks.Count > 1)
                        RightFootStepTrack = new FootStepTagTrack(utilityTagTracks[1] as FootStepTagTrack);
                }

                if (utilityTagTracks.Count > 2)
                    WarpPositionTrack = new TagTrackBase(utilityTagTracks[2]);

                if (utilityTagTracks.Count > 3)
                    WarpRotationTrack = new TagTrackBase(utilityTagTracks[3]);

                if (utilityTagTracks.Count > 4)
                    EnableRootMotionTrack = new TagTrackBase(utilityTagTracks[4]);

                if (utilityTagTracks.Count > 5)
                    PoseFavourTrack = new FloatTagTrack(utilityTagTracks[5] as FloatTagTrack);

                if (utilityTagTracks.Count > 6)
                    WarpTrajLatTrack = new TagTrackBase(utilityTagTracks[6]);

                if (utilityTagTracks.Count > 7)
                    WarpTrajLongTrack = new TagTrackBase(utilityTagTracks[7]);
            }
        }

        public List<AnimationClip> AnimBeforeClips
        {
            get
            {
                if (BeforeClips == null)
                    BeforeClips = new List<AnimationClip>();

                return BeforeClips;
            }
        }

        public List<AnimationClip> AnimAfterClips
        {
            get
            {
                if (AfterClips == null)
                    AfterClips = new List<AnimationClip>();

                return AfterClips;
            }
        }

        

        public void InitPoseDataList() { PoseList = new List<PoseData>(); }

        public void InitTagTracks()
        {
            if (TagTracks == null)
                TagTracks = new List<TagTrack>();

            if (FavourTagTracks == null)
                FavourTagTracks = new List<TagTrack>();

            if (UserBoolTracks == null)
                UserBoolTracks = new List<TagTrackBase>();
        }

        public void AddToPoseList(ref PoseData a_newPose)
        {
            if (PoseList == null)
                PoseList = new List<PoseData>();

            PoseList.Add(a_newPose);
        }


        public void AddEvent(float _time)
        {
#if UNITY_EDITOR
            Undo.RecordObject(this, "Event Added");

            if (Events == null)
                Events = new List<EventMarker>();

            _time = Mathf.Clamp(_time, 0f, PrimaryClip.length);
            Events.Add(new EventMarker(-1, _time));
#endif
        }

        public void ScrapGeneratedClips()
        {
#if UNITY_EDITOR
            if (GeneratedClip != null)
            {
                AssetDatabase.RemoveObjectFromAsset(GeneratedClip);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(this));
                GeneratedClip = null;
            }

            GeneratedTagTracks = new List<TagTrack>();
            GeneratedEvents = new List<EventMarker>();
            GeneratedUserBoolTracks = new List<TagTrackBase>();

            if (PrimaryClip != null)
            {

                GeneratedLeftFootStepTrack = new FootStepTagTrack(0, "FootStep Left", PrimaryClip.length);
                GeneratedRightFootStepTrack = new FootStepTagTrack(1, "FootStep Right", PrimaryClip.length);
                GeneratedWarpPositionTrack = new TagTrackBase(2, "Disable Warp (Position)", PrimaryClip.length);
                GeneratedWarpRotationTrack = new TagTrackBase(3, "Disable Warp (Rotation)", PrimaryClip.length);
                GeneratedEnableRootMotionTrack = new TagTrackBase(4, "Disable Matching", PrimaryClip.length);
                GeneratedPoseFavourTrack = new FloatTagTrack(5, "Pose Favour", PrimaryClip.length);
                GeneratedWarpTrajLatTrack = new TagTrackBase(6, "Disable Trajectory Warp (Angular)", PrimaryClip.length);
                GeneratedWarpTrajLongTrack = new TagTrackBase(7, "Disable Trajectory Warp (Long)", PrimaryClip.length);

                GeneratedPoseFavourTrack.DefaultValue = PoseFavourTrack.DefaultValue;
            }
#endif
        }


        public (List<Vector3> posLookup, List<Quaternion> rotLookup) GetRootLookupTable()
        {
            if(PrimaryClip != null)
            {
                GenerateRootLookupTableIfRequired();

                return (m_rootPosLookupTable, m_rootRotLookupTable);
            }

            return (null, null);
        }

        public (Vector3 pos, Quaternion rot) GetRoot(float a_time)
        {
            if (PrimaryClip != null)
            {
                GenerateRootLookupTableIfRequired();

                a_time = Mathf.Clamp(a_time, 0f, PrimaryClip.length);

                //float sampleRate = 1f / PrimaryClip.frameRate;
                float sampleRate = 1f / 60f;

                int startIndex = Mathf.Min(Mathf.FloorToInt(a_time / sampleRate), m_rootPosLookupTable.Count - 1);
                int nextIndex = Mathf.Min(startIndex + 1, m_rootPosLookupTable.Count - 1);

                float lerpVal = Mathf.Clamp01((a_time % sampleRate) / sampleRate);

                Vector3 rootPos = Vector3.Lerp(m_rootPosLookupTable[startIndex], m_rootPosLookupTable[nextIndex], lerpVal);
                Quaternion rootRot = Quaternion.Slerp(m_rootRotLookupTable[startIndex], m_rootRotLookupTable[nextIndex], lerpVal);

                return (rootPos, rootRot);
            }

            return (Vector3.zero, Quaternion.identity);
        }

        public float GetAverageRootSpeed(float a_startTime, float a_endTime)
        {
            if (PrimaryClip != null && m_rootPosLookupTable != null && m_rootPosLookupTable.Count > 0)
            {
                GenerateRootLookupTableIfRequired();

                a_startTime = Mathf.Clamp(a_startTime, 0f, PrimaryClip.length);
                a_endTime = Mathf.Clamp(a_endTime, 0f, PrimaryClip.length);

                if (a_endTime < a_startTime)
                {
                    float endTime = a_endTime;
                    a_endTime = a_startTime;
                    a_startTime = endTime;
                }

                int startIndex = Mathf.FloorToInt(a_startTime * PrimaryClip.frameRate);
                int endIndex = Mathf.FloorToInt(a_endTime * PrimaryClip.frameRate);

                float cumulativeSpeed = 0f;
                for (int i = startIndex; i < endIndex; ++i)
                {
                    cumulativeSpeed += m_rootSpeedLookupTable[i];
                }

                return cumulativeSpeed / (endIndex - startIndex);
            }

            return 0f;
        }

        public void GenerateModifiedAnimation(MxMPreProcessData a_preProcessData, string a_directory)
        {
#if UNITY_EDITOR
            EditorUtility.DisplayProgressBar("Generate Modified Animation", "Scraping Old Anims", 0f);

            if (a_preProcessData != null)
                m_targetPreProcessData = a_preProcessData;

            ScrapGeneratedClips();

            if (UseSpeedMods)
            {
                if (PrimaryClip != null && m_targetPreProcessData != null)
                {

                    EditorUtility.DisplayProgressBar("Generate Modified Animation", "Copying Clip " + PrimaryClip.name, 0f);

                    GeneratedClip = new AnimationClip();
                    EditorUtility.CopySerialized(PrimaryClip, GeneratedClip);
                    GeneratedClip.name = PrimaryClip.name + "_MxM_MOD";

                    var curveBindings = AnimationUtility.GetCurveBindings(GeneratedClip);
                    List<AnimationCurve> workingCurves = new List<AnimationCurve>(curveBindings.Length + 1);
                    List<AnimationEvent> newEvents = new List<AnimationEvent>();
                    List<MotionSection> motionList = MotionModifier.MotionSections;
                    MotionTimingPresets presets = m_targetPreProcessData.MotionTimingPresets;

                    GeneratedTagTracks = new List<TagTrack>(TagTracks);
                    GeneratedFavourTagTracks = new List<TagTrack>(TagTracks);
                    GeneratedUserBoolTracks = new List<TagTrackBase>(UserBoolTracks);
                    GeneratedLeftFootStepTrack = new FootStepTagTrack(LeftFootStepTrack);
                    GeneratedRightFootStepTrack = new FootStepTagTrack(RightFootStepTrack);
                    GeneratedWarpPositionTrack = new TagTrackBase(WarpPositionTrack);
                    GeneratedWarpRotationTrack = new TagTrackBase(WarpRotationTrack);
                    GeneratedEnableRootMotionTrack = new TagTrackBase(EnableRootMotionTrack);
                    GeneratedPoseFavourTrack = new FloatTagTrack(PoseFavourTrack);
                    GeneratedWarpTrajLatTrack = new TagTrackBase(WarpTrajLatTrack);
                    GeneratedWarpTrajLongTrack = new TagTrackBase(WarpTrajLongTrack);

                    //Create curves but don't add keys
                    for (int i = 0; i < curveBindings.Length; ++i)
                    {
                        workingCurves.Add(new AnimationCurve());
                    }

                    float cumTimeShift = 0f; //This is the cumulative amount of time shift at the point of modification;
                    float curStartTime = 0f; //The start time for the current motion section
                    int[] startKeyIndex = new int[workingCurves.Count]; //The start key for the current motion section
                    for (int i = 0; i < motionList.Count; ++i)
                    {
                        EditorUtility.DisplayProgressBar("Generate Modified Animation",
                            "Modifying Section " + i + " of " + PrimaryClip.name, ((float)i) / ((float)motionList.Count));

                        MotionSection motionSection = motionList[i];

                        float startWarpTime = curStartTime;
                        float endWarpTime = motionSection.EndTime;
                        float warpScale = motionSection.GetSpeedMod(curStartTime, presets, this);

                        float localTimeShift = (endWarpTime - startWarpTime) - (endWarpTime - startWarpTime) * warpScale;

                        //Shift Curve Keys
                        for (int k = 0; k < curveBindings.Length; ++k)
                        {
                            EditorCurveBinding binding = curveBindings[k];
                            AnimationCurve originalCurve = AnimationUtility.GetEditorCurve(PrimaryClip, binding);
                            AnimationCurve workingCurve = workingCurves[k];

                            //Make a cut at the end only
                            int endKeyIndex = originalCurve.AddKey(endWarpTime, originalCurve.Evaluate(endWarpTime));

                            if (endKeyIndex == -1)
                                endKeyIndex = originalCurve.keys.Length - 1;

                            //Add in the intermediate keys scaled relative to the start and shifted by the cumulative time shift
                            for (int keyIndex = startKeyIndex[k]; i < motionList.Count - 1 ? keyIndex < endKeyIndex : keyIndex <= endKeyIndex; ++keyIndex)
                            {
                                Keyframe key = originalCurve.keys[keyIndex];
                                key.time = startWarpTime + ((key.time - startWarpTime) * warpScale) - cumTimeShift;
                                key.inTangent /= warpScale;
                                key.outTangent /= warpScale;

                                workingCurve.AddKey(key);
                            }

                            startKeyIndex[k] = endKeyIndex;
                        }

                        //Shift Events
                        foreach (AnimationEvent evt in GeneratedClip.events)
                        {
                            if (evt.time > startWarpTime && evt.time < endWarpTime)
                            {
                                //Scale & Shift
                                evt.time = startWarpTime + ((evt.time - startWarpTime) * warpScale) - cumTimeShift;
                            }

                            newEvents.Add(evt);
                        }

                        //Shift MxM Events
                        foreach (EventMarker evt in Events)
                        {
                            EventMarker newMarker = new EventMarker(evt);

                            if (newMarker.EventTime > startWarpTime && evt.EventTime < endWarpTime)
                            {
                                evt.EventTime = startWarpTime + ((evt.EventTime - startWarpTime) * warpScale) - cumTimeShift;
                            }

                            GeneratedEvents.Add(newMarker);
                        }

                        //Shift MXM Tag Points
                        foreach (TagTrack track in GeneratedTagTracks)
                        {
                            List<Vector2> tagList = track.Tags;

                            for (int k = 0; k < tagList.Count; ++k)
                            {
                                Vector2 newTag = tagList[k];

                                if (newTag.x > startWarpTime && newTag.x < endWarpTime)
                                {
                                    newTag.x = startWarpTime + ((newTag.x - startWarpTime) * warpScale) - cumTimeShift;
                                }

                                if (newTag.y > startWarpTime && newTag.y < endWarpTime)
                                {
                                    newTag.y = startWarpTime + ((newTag.y - startWarpTime) * warpScale) - cumTimeShift;
                                }

                                tagList[k] = newTag;
                            }
                        }

                        //Shift MXM FavourTag Points
                        foreach (TagTrack track in GeneratedFavourTagTracks)
                        {
                            List<Vector2> tagList = track.Tags;

                            for (int k = 0; k < tagList.Count; ++k)
                            {
                                Vector2 newTag = tagList[k];

                                if (newTag.x > startWarpTime && newTag.x < endWarpTime)
                                {
                                    newTag.x = startWarpTime + ((newTag.x - startWarpTime) * warpScale) - cumTimeShift;
                                }

                                if (newTag.y > startWarpTime && newTag.y < endWarpTime)
                                {
                                    newTag.y = startWarpTime + ((newTag.y - startWarpTime) * warpScale) - cumTimeShift;
                                }

                                tagList[k] = newTag;
                            }
                        }

                        //Shift MxM User Tags
                        foreach (TagTrackBase track in GeneratedUserBoolTracks)
                        {
                            ShiftTrackTags(track, startWarpTime, endWarpTime, warpScale, cumTimeShift);
                        }

                        //Shift MxM Utility Tags
                        ShiftTrackTags(GeneratedLeftFootStepTrack, startWarpTime, endWarpTime, warpScale, cumTimeShift);
                        ShiftTrackTags(GeneratedRightFootStepTrack, startWarpTime, endWarpTime, warpScale, cumTimeShift);
                        ShiftTrackTags(GeneratedWarpPositionTrack, startWarpTime, endWarpTime, warpScale, cumTimeShift);
                        ShiftTrackTags(GeneratedWarpRotationTrack, startWarpTime, endWarpTime, warpScale, cumTimeShift);
                        ShiftTrackTags(GeneratedEnableRootMotionTrack, startWarpTime, endWarpTime, warpScale, cumTimeShift);
                        ShiftTrackTags(GeneratedPoseFavourTrack, startWarpTime, endWarpTime, warpScale, cumTimeShift);
                        ShiftTrackTags(GeneratedWarpTrajLatTrack, startWarpTime, endWarpTime, warpScale, cumTimeShift);
                        ShiftTrackTags(GeneratedWarpTrajLongTrack, startWarpTime, endWarpTime, warpScale, cumTimeShift);

                        cumTimeShift += localTimeShift;
                        curStartTime = endWarpTime;
                    }

                    for (int i = 0; i < workingCurves.Count; ++i)
                    {
                        EditorUtility.DisplayProgressBar("Generate Modified Animation",
                            "Generating Curves for clip: " + PrimaryClip.name, ((float)i) / ((float)workingCurves.Count));

                        AnimationUtility.SetEditorCurve(GeneratedClip, curveBindings[i], workingCurves[i]);
                    }

                    AnimationUtility.SetAnimationEvents(GeneratedClip, newEvents.ToArray());
                    EditorUtility.SetDirty(GeneratedClip);

                    AssetDatabase.CreateAsset(GeneratedClip, a_directory + "/" + GeneratedClip.name + ".anim");

                    //AssetDatabase.AddObjectToAsset(GeneratedClip, m_targetPreProcessData);
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(GeneratedClip));

                    EditorUtility.SetDirty(this);

                    EditorUtility.ClearProgressBar();

                }
                else
                {
                    Debug.LogWarning("Warning: Cannot generate modified animation with no PrimaryClip in MxMAnimationClipComposite");
                }
            }
#endif
        }

        public void ShiftTrackTags(TagTrackBase a_track, float a_startWarpTime,
            float a_endWarpTime, float a_warpScale, float a_cumTimeShift)
        {
            //Shift MxM User Tags
            List<Vector2> tagList = a_track.TagPositions;

            for (int k = 0; k < tagList.Count; ++k)
            {
                Vector2 newTag = tagList[k];

                if (newTag.x > a_startWarpTime && newTag.x < a_endWarpTime)
                {
                    newTag.x = a_startWarpTime + ((newTag.x - a_startWarpTime) * a_warpScale) - a_cumTimeShift;
                }

                if (newTag.y > a_startWarpTime && newTag.y < a_endWarpTime)
                {
                    newTag.y = a_startWarpTime + ((newTag.y - a_startWarpTime) * a_warpScale) - a_cumTimeShift;
                }

                tagList[k] = newTag;
            }
        }

        #endregion

    }//End of class: MxMAnimationClipComposite
}//End of namespace: MxMEditor