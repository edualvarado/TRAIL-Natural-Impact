using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using MxMEditor;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MxM
{
    [CreateAssetMenu(fileName = "MxMBlendSpace", menuName = "MxM/Extensions/BlendSpace", order = 1)]
    public class MxMBlendSpace : ScriptableObject, IMxMAnim
    {
        //Clips
        [SerializeField] private string m_blendSpaceName = null;
        [SerializeField] private List<AnimationClip> m_clips = null;
        [SerializeField] private List<Vector2> m_positions = null;

        [SerializeField] private Vector2 m_magnitude = Vector2.one;
        [SerializeField] private Vector2 m_smoothing = new Vector2(0.25f, 0.25f);
        [SerializeField] private bool m_normalizeTime = true;

        [SerializeField] private EBlendSpaceType m_scatterSpace = EBlendSpaceType.Standard;
        [SerializeField] private Vector2 m_scatterSpacing = new Vector2(0.05f, 1f);
        [SerializeField] private List<Vector2> m_scatterPositions = null;

        //Tags (BlendSpaces can't have events)
        [SerializeField] public ETags GlobalTags = ETags.None;
        [SerializeField] public ETags GlobalFavourTags = ETags.None;
        [SerializeField] public List<TagTrack> TagTracks;
        [SerializeField] public List<TagTrack> FavourTagTracks;

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

        //Curves
        [SerializeField] public List<MxMCurveTrack> Curves;

        [SerializeField] public float RuntimePlaybackSpeed = 1f;

        //Speed ModificationData
        [SerializeField] public bool UseSpeedMods;
        [SerializeField] public MotionModifyData MotionModifier;
        [SerializeField] public List<AnimationClip> GeneratedClips;

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

        //Reference Data
        [SerializeField] private MxMPreProcessData m_targetPreProcessData;
        [SerializeField] private AnimationModule m_targetAnimModule;
        [SerializeField] private GameObject m_targetPrefab;

        //Pre-process Working Data
        [System.NonSerialized] public List<PoseData> PoseList = new List<PoseData>();

        private int m_trackId = -1;

        //Preview Data
        private List<Vector3> m_rootPosLookupTable;
        private List<Quaternion> m_rootRotLookupTable;
        private List<float> m_rootSpeedLookupTable;

        public string BlendSpaceName { get { return m_blendSpaceName; } set { m_blendSpaceName = value ;} }
        public Vector2 Magnitude { get { return m_magnitude; } set { m_magnitude = value; } }
        public Vector2 Smoothing { get { return m_smoothing; } set { m_smoothing = value; } }
        public List<AnimationClip> Clips { get { return m_clips; } }
        public List<Vector2> Positions { get { return m_positions; } }
        public GameObject TargetPrefab { get { return m_targetPrefab; } set { m_targetPrefab = value; } }
        public MxMPreProcessData PreProcessor { get { return m_targetPreProcessData; } }

        [System.Obsolete("Obsolete property. Please use AnimModule instead or your project may not compile in future updates.")]
        public AnimationModule AnimMOdule { get { return m_targetAnimModule; } }
        public AnimationModule AnimModule { get { return m_targetAnimModule; } }
        public EBlendSpaceType ScatterSpace { get { return m_scatterSpace; } set { m_scatterSpace = value; } }
        public Vector2 ScatterSpacing { get { return m_scatterSpacing; } set { m_scatterSpacing = value; } }
        public List<Vector2> ScatterPositions { get { return m_scatterPositions; } }
        public bool NormalizeTime { get { return m_normalizeTime; } set { m_normalizeTime = value; } }

        public List<AnimationClip> FinalClips
        {
            get
            {
                if (GeneratedClips != null && GeneratedClips.Count == m_clips.Count)
                {
                    return GeneratedClips;
                }
                else
                {
                    return m_clips;
                }
            }
        }

        public void CopyData(MxMBlendSpace a_copy, bool a_mirrored=false)
        {
            ValidateBaseData();

            m_magnitude = a_copy.m_magnitude;
            m_smoothing = a_copy.m_smoothing;

            m_scatterSpace = a_copy.m_scatterSpace;
            m_scatterSpacing = a_copy.m_scatterSpacing;
            m_scatterPositions = null;

            UseSpeedMods = a_copy.UseSpeedMods;

            GlobalTags = a_copy.GlobalTags;
            GlobalFavourTags = a_copy.GlobalFavourTags;

#if UNITY_EDITOR
            if (a_mirrored)
            {
                BlendSpaceName = a_copy.BlendSpaceName + "_MIRROR";
                
                m_clips = new List<AnimationClip>(a_copy.m_clips.Count + 1);
                foreach (AnimationClip clip in a_copy.m_clips)
                {
                    m_clips.Add(MxMUtility.FindMirroredClip(clip));
                }
            }
            else
#endif
            {
                BlendSpaceName = a_copy.BlendSpaceName;
                m_clips = new List<AnimationClip>(a_copy.m_clips);
            }

            m_positions = new List<Vector2>(a_copy.Positions);

            if(m_targetPrefab == null)
                m_targetPrefab = a_copy.m_targetPrefab;

            TagTracks = new List<TagTrack>(a_copy.TagTracks.Count + 1);
            foreach(TagTrack track in a_copy.TagTracks)
            {
                TagTracks.Add(new TagTrack(track));
            }

            FavourTagTracks = new List<TagTrack>(a_copy.FavourTagTracks.Count + 1);
            foreach(TagTrack track in a_copy.FavourTagTracks)
            {
                FavourTagTracks.Add(new TagTrack(track));
            }

            UserBoolTracks = new List<TagTrackBase>(a_copy.UserBoolTracks.Count + 1);
            foreach(TagTrackBase track in a_copy.UserBoolTracks)
            {
                UserBoolTracks.Add(new TagTrackBase(track));
            }

            if (a_mirrored)
            {
                LeftFootStepTrack = new FootStepTagTrack(a_copy.RightFootStepTrack);
                RightFootStepTrack = new FootStepTagTrack(a_copy.LeftFootStepTrack);
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
            
            //Todo: mirror events
        }

        public void Initialize(AnimationClip a_clip)
        {
            m_clips = new List<AnimationClip>();
            m_positions = new List<Vector2>();

            if (a_clip != null)
            {
                m_clips.Add(a_clip);
                m_positions.Add(Vector2.zero);
            }
        }

        public void OnEnable()
        {
            VerifyData();

            if(Curves == null)
                Curves = new List<MxMCurveTrack>(3);
        }

        public void ValidateBaseData()
        {
            bool modified = false;

            if (MotionModifier == null)
                MotionModifier = new MotionModifyData();

            if (TagTracks == null)
                TagTracks = new List<TagTrack>();

            if (FavourTagTracks == null)
                FavourTagTracks = new List<TagTrack>();

            if (GeneratedTagTracks == null)
                GeneratedTagTracks = new List<TagTrack>();

            if (GeneratedFavourTagTracks == null)
                GeneratedFavourTagTracks = new List<TagTrack>();

            if (m_clips == null)
                m_clips = new List<AnimationClip>();

            if (m_positions == null)
                m_positions = new List<Vector2>();

            if (m_clips != null && m_clips.Count > 0)
            {
                float clipLength = m_clips[0].length;

                if (LeftFootStepTrack == null || LeftFootStepTrack.Name == "" || LeftFootStepTrack.Name == null)
                {
                    LeftFootStepTrack = new FootStepTagTrack(0, "Footstep Left", clipLength);
                    modified = true;
                }

                if (RightFootStepTrack == null || RightFootStepTrack.Name == "" || RightFootStepTrack.Name == null)
                {
                    RightFootStepTrack = new FootStepTagTrack(1, "Footstep Right", clipLength);
                    modified = true;
                }

                if (WarpPositionTrack == null || WarpPositionTrack.Name == "" || WarpPositionTrack.Name == null)
                {
                    WarpPositionTrack = new TagTrackBase(2, "Disable Warp (Position)", clipLength);
                    modified = true;
                }

                if (WarpRotationTrack == null || WarpRotationTrack.Name == "" || WarpRotationTrack.Name == null)
                {
                    WarpRotationTrack = new TagTrackBase(3, "Disable Warp (Rotation)", clipLength);
                    modified = true;
                }

                if (EnableRootMotionTrack == null || EnableRootMotionTrack.Name == "" || EnableRootMotionTrack.Name == null)
                {
                    EnableRootMotionTrack = new TagTrackBase(4, "Enable Root Motion", clipLength);
                    modified = true;
                }

                if (PoseFavourTrack == null || PoseFavourTrack.Name == "" || PoseFavourTrack.Name == null)
                {
                    PoseFavourTrack = new FloatTagTrack(5, "Pose Favour", clipLength);
                    modified = true;
                }

                if (WarpTrajLatTrack == null || WarpTrajLatTrack.Name == null || WarpTrajLatTrack.Name == "")
                {
                    WarpTrajLatTrack = new TagTrackBase(6, "Disable Trajectory Warp (Angular)", clipLength);
                    modified = true;
                }

                if (WarpTrajLongTrack == null || WarpTrajLongTrack.Name == null || WarpTrajLongTrack.Name == "")
                {
                    WarpTrajLongTrack = new TagTrackBase(7, "Disable Trajectory Warp (Long)", clipLength);
                    modified = true;
                }

                PoseFavourTrack.SetDefaultTagValue(1f);
            }
            
            //Fix potential corrupted data
            if (m_positions.Count > m_clips.Count)
            {
                m_positions.RemoveRange(m_clips.Count, m_positions.Count - m_clips.Count);
                modified = true;
            }
            else if (m_clips.Count > m_positions.Count)
            {
                m_clips.RemoveRange(m_positions.Count, m_clips.Count - m_positions.Count);
                modified = true;
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

        public void VerifyData()
        {
            ValidateBaseData();

            if (m_clips == null || m_clips.Count == 0)
                return;

            AnimationClip clip = m_clips[0];

            for (int i = m_clips.Count - 1; i >= 0; --i)
            {
                if (m_clips[i] == null)
                {
                    m_clips.RemoveAt(i);
                    m_positions.RemoveAt(i);
                }
            }

            foreach (TagTrack tagTrack in TagTracks)
                tagTrack.VerifyData(clip);

            foreach (TagTrack favourTrack in FavourTagTracks)
                favourTrack.VerifyData(clip);

            LeftFootStepTrack.VerifyData(clip);
            RightFootStepTrack.VerifyData(clip);
            WarpPositionTrack.VerifyData(clip);
            WarpRotationTrack.VerifyData(clip);
            EnableRootMotionTrack.VerifyData(clip);
            PoseFavourTrack.VerifyData(clip);
            WarpTrajLatTrack.VerifyData(clip);
            WarpTrajLongTrack.VerifyData(clip);
        }

        public void SetPrimaryAnim(AnimationClip a_clip)
        {
            if (a_clip != null)
            {
                ValidateBaseData();

                m_clips.Add(a_clip);
                m_positions.Add(Vector2.zero);

                GlobalTags = ETags.None;
                GlobalFavourTags = ETags.None;
            }
        }

        public void SetTargetPreProcessor(MxMPreProcessData a_preProcessor)
        {
            if (a_preProcessor != null)
            {
                m_targetPreProcessData = a_preProcessor;

                if (a_preProcessor.Prefab != null)
                {
                    m_targetPrefab = a_preProcessor.Prefab;
                }

                m_targetAnimModule = null;
            }
        }

        public void SetTargetAnimModule(AnimationModule a_animModule)
        {
            if(a_animModule != null)
            {
                m_targetAnimModule = a_animModule;

                if(a_animModule.Prefab != null)
                {
                    m_targetPrefab = a_animModule.Prefab;
                }

                m_targetPreProcessData = null;
            }
        }

        public void GenerateRootLookupTable()
        {
            AnimationClip primaryClip = this.TargetClip;

            if (primaryClip != null)
            {
                if (m_targetPrefab == null)
                {
                    if (m_targetPreProcessData != null)
                    {
                        m_targetPrefab = m_targetPreProcessData.Prefab;
                    }
                    else if(m_targetAnimModule != null)
                    {
                        m_targetPrefab = m_targetPreProcessData.Prefab;
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
                    var playableClip = AnimationClipPlayable.Create(playableGraph, primaryClip);
                    playableClip.SetApplyFootIK(true);
                    animationMixer.ConnectInput(0, playableClip, 0);
                    animationMixer.SetInputWeight(0, 1f);
                    playableClip.SetTime(0.0);
                    playableClip.SetTime(0.0);

                    previewPrefab.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                    playableGraph.Evaluate(0f);

                    double sampleRate = 1.0 / primaryClip.frameRate;

                    //Fill the lookup table with root positions
                    for (double animTime = 0.0; animTime <= primaryClip.length; animTime += sampleRate)
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

        private void GenerateRootLookupTableIfRequired()
        {
            if (m_clips != null && m_clips.Count > 0)
            {
                if (m_rootPosLookupTable == null || m_rootRotLookupTable == null
                        || m_rootPosLookupTable.Count == 0 || m_rootRotLookupTable.Count == 0)
                {
                    GenerateRootLookupTable();
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

        public List<Vector2> CalculateScatterPositions()
        {
            m_scatterPositions = new List<Vector2>();

            List<float> xPos = new List<float>();
            List<float> yPos = new List<float>();

            if(m_scatterSpace == EBlendSpaceType.Scatter || m_scatterSpace == EBlendSpaceType.ScatterX)
            {
                int xCount = Mathf.CeilToInt(1f / m_scatterSpacing.x) * 2 + 1;

                float xEnd = Mathf.Ceil(1f / m_scatterSpacing.x) * m_scatterSpacing.x;
                float xStart = xEnd * -1f;

                for (int i = 0; i < xCount; ++i)
                {
                    xPos.Add(Mathf.Clamp(xStart + i * m_scatterSpacing.x, -1f, 1f));
                }
            }
            else
            {
                xPos.Add(0f);
            }

            if (m_scatterSpace == EBlendSpaceType.Scatter || m_scatterSpace == EBlendSpaceType.ScatterY)
            {
                int yCount = Mathf.CeilToInt(1f / m_scatterSpacing.y) * 2 + 1;

                float yEnd = Mathf.Ceil(1f / m_scatterSpacing.y) * m_scatterSpacing.y;
                float yStart = yEnd * -1f;

                for(int i = 0; i < yCount; ++i)
                {
                    yPos.Add(Mathf.Clamp(yStart + i * m_scatterSpacing.y, -1f, 1f));
                }
            }
            else
            {
                yPos.Add(0f);
            }

            foreach(float y in yPos)
            {
                foreach(float x in xPos)
                {
                    m_scatterPositions.Add(new Vector2(x, y));
                }
            }

            return m_scatterPositions;
        }

        public float[] CalculateWeightings(Vector2 a_position)
        {
            float[] weightings = new float[m_clips.Count];

            float totalWeight = 0f;

            for (int i = 0; i < m_positions.Count; ++i)
            {
                Vector2 positionI = m_positions[i];
                Vector2 iToSample = a_position - positionI;

                float weight = 1f;

                for (int j = 0; j < m_positions.Count; ++j)
                {
                    if (j == i)
                        continue;

                    Vector2 positionJ = m_positions[j];
                    Vector2 iToJ = positionJ - positionI;

                    //Calc Weight
                    float lensq_ij = Vector2.Dot(iToJ, iToJ);
                    float newWeight = Vector2.Dot(iToSample, iToJ) / lensq_ij;
                    newWeight = 1f - newWeight;
                    newWeight = Mathf.Clamp01(newWeight);

                    weight = Mathf.Min(weight, newWeight);
                }

                weightings[i] = weight;
                totalWeight += weight;
            }

            for (int i = 0; i < weightings.Length; ++i)
            {
                weightings[i] = weightings[i] / totalWeight;
            }


            return weightings;
        }

        #region IMxMAnim
        //IMxMAnim Basic Properties
        public MxMPreProcessData TargetPreProcess { get { return m_targetPreProcessData; } set { m_targetPreProcessData = value; } }
        public AnimationModule TargetAnimModule { get { return m_targetAnimModule; } set { m_targetAnimModule = value; } }
        public GameObject TargetModel { get { return m_targetPrefab; } set { m_targetPrefab = value; } }
        public EMxMAnimtype AnimType { get { return EMxMAnimtype.BlendSpace; } } 
        public ETags AnimGlobalTags { get { return GlobalTags; } }
        public ETags AnimGlobalFavourTags { get { return GlobalFavourTags; } }
        public List<PoseData> AnimPoseList { get { return PoseList; } }
        public float PlaybackSpeed { get { return RuntimePlaybackSpeed; } set { RuntimePlaybackSpeed = value; }}
        public List<EventMarker> FinalEventMarkers { get { return null; } }
        public List<EventMarker> EventMarkers { get { return null; } }
        public List<TagTrackBase> UserTagTracks { get { return UserBoolTracks; } }
        public List<AnimationClip> AnimBeforeClips { get { return null; } }
        public List<AnimationClip> AnimAfterClips { get { return null; } }
        public MotionModifyData AnimMotionModifier { get { return MotionModifier; } }
        public bool IsLooping { get { return true; } }
        public bool UseIgnoreEdges { get { return false; } }
        public bool UseExtrapolateTrajectory { get { return false; } }
        public bool UseFlattenTrajectory { get { return false; } }
        public bool UseRuntimeSplicing { get { return false; } }
        public bool UsingSpeedMods { get { return UseSpeedMods; } set { UseSpeedMods = value; } }
        
        //Unused IMxMAnim Functions
        public void OnDeleteEventMarker(object a_eventObj) { }
        public void OnDeleteTag(object a_tagObj) { }
        public void OnDeleteMotionSection(object a_motionObj) { }
        public void AddEvent(float _time) { }

        public void InitPoseDataList() { PoseList = new List<PoseData>(); }

        public void SetTrackId(int a_trackId) { m_trackId = a_trackId; }
        public int GetTrackId() { return m_trackId; }

        public List<MxMCurveTrack> CurveTracks { get { return Curves; } }

        //IMxMAnim Complex Properties
        public List<TagTrack> AnimTagTracks
        {
            get
            {
                if (TagTracks == null)
                    FavourTagTracks = new List<TagTrack>();

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


        public float AnimLength
        {
            get
            {
                if (UseSpeedMods && GeneratedClips != null)
                    return GeneratedClips[0].length;

                return Clips[0].length;
            }
        }

        public AnimationClip FinalClip
        {
            get
            { 
                List<AnimationClip> clipList = m_clips;
                if (UseSpeedMods && GeneratedClips != null && GeneratedClips.Count > 0)
                {
                    clipList = GeneratedClips;
                }

                float closestDist = float.MaxValue;
                int closestClipId = -1;

                for (int i = 0; i < clipList.Count; ++i)
                {
                    float dist = m_positions[i].magnitude;

                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestClipId = i;
                    }
                }

                return clipList[closestClipId];
            }
        }

        public AnimationClip TargetClip
        {
            get
            {
                if (m_clips == null || m_clips.Count == 0)
                    return null;

                float closestDist = float.MaxValue;
                int closestClipId = -1;

                for (int i = 0; i < m_clips.Count; ++i)
                {
                    float dist = m_positions[i].magnitude;

                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestClipId = i;
                    }
                }

                return m_clips[closestClipId];
            }
        }

        public List<TagTrack> FinalTagTracks
        {
            get
            {
                if (GeneratedClips != null && GeneratedClips.Count > 0 && UseSpeedMods)
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
                if (GeneratedClips != null && GeneratedClips.Count > 0 && UseSpeedMods)
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

                if (GeneratedClips != null && GeneratedClips.Count > 0 && UseSpeedMods)
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
                if (GeneratedClips != null && GeneratedClips.Count > 0 && UseSpeedMods)
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
                    if (track != null)
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
                    if (track != null)
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

        public AnimationClip AnimGeneratedClip
        {
            get
            {
                if (GeneratedClips != null && GeneratedClips.Count > 0)
                    return GeneratedClips[0];

                if (m_clips != null && m_clips.Count > 0)
                    return m_clips[0];

                return null;
            }
        }

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
            if(PoseList == null)
                PoseList = new List<PoseData>();

            PoseList.Add(a_newPose);
        }

        public void ScrapGeneratedClips()
        {
#if UNITY_EDITOR
            if(GeneratedClips != null)
            {
                foreach(AnimationClip clip in GeneratedClips)
                {
                    if(clip != null)
                    {
                        AssetDatabase.RemoveObjectFromAsset(clip);
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(this));
                    }
                }

                GeneratedClips.Clear();

                GeneratedTagTracks = new List<TagTrack>();
            }
            else
            {
                GeneratedClips = new List<AnimationClip>();
                GeneratedTagTracks = new List<TagTrack>();
            }
#endif
        }

        public (List<Vector3> posLookup, List<Quaternion> rotLookup) GetRootLookupTable()
        {
            AnimationClip primaryClip = this.TargetClip;

            if (primaryClip != null)
            {
                GenerateRootLookupTableIfRequired();

                return (m_rootPosLookupTable, m_rootRotLookupTable);
            }

            return (null, null);
        }

        public (Vector3 pos, Quaternion rot) GetRoot(float a_time)
        {
            AnimationClip primaryClip = this.TargetClip;

            if (primaryClip != null)
            {
                GenerateRootLookupTableIfRequired();

                a_time = Mathf.Clamp(a_time, 0f, primaryClip.length);

                float sampleRate = 1f / primaryClip.frameRate;

                int startIndex = Mathf.FloorToInt(a_time / sampleRate);
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
            AnimationClip primaryClip = this.TargetClip;

            if (primaryClip != null)
            {
                GenerateRootLookupTableIfRequired();

                a_startTime = Mathf.Clamp(a_startTime, 0f, primaryClip.length);
                a_endTime = Mathf.Clamp(a_endTime, 0f, primaryClip.length);

                if (a_endTime < a_startTime)
                {
                    float endTime = a_endTime;
                    a_endTime = a_startTime;
                    a_startTime = endTime;
                }

                int startIndex = Mathf.FloorToInt(a_startTime * primaryClip.frameRate);
                int endIndex = Mathf.FloorToInt(a_endTime * primaryClip.frameRate);

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
            EditorUtility.DisplayProgressBar("Generate Modified Animations", "Scraping Old Anims", 0f);

            if (a_preProcessData != null)
                m_targetPreProcessData = a_preProcessData;

            ScrapGeneratedClips();

            List<MotionSection> motionList = MotionModifier.MotionSections;
            MotionTimingPresets presets = m_targetPreProcessData.MotionTimingPresets;



            if (UseSpeedMods && m_clips != null && m_targetPreProcessData != null)
            {
                int blendSpaceId = 0;
                for (int i = 0; i < m_targetPreProcessData.BlendSpaces.Count; ++i)
                {
                    if(this == m_targetPreProcessData.BlendSpaces[i])
                    {
                        blendSpaceId = i;
                        break;
                    }
                }

                float cumTimeShift = 0f; //This is the cumulative amount of time shift at the point of modification;
                float curStartTime = 0f; //The start time for the current motion section
                //Loop clips and generate ndes ones
                foreach (AnimationClip clip in m_clips)
                {
                    EditorUtility.DisplayProgressBar("Generate Modified Animation", "Copying Clip " + clip.name, 0f);

                    var generatedClip = new AnimationClip();
                    EditorUtility.CopySerialized(clip, generatedClip);
                    generatedClip.name = clip.name + "_MxM_MOD_" + blendSpaceId;

                    var curveBindings = AnimationUtility.GetCurveBindings(generatedClip);
                    List<AnimationCurve> workingCurves = new List<AnimationCurve>(curveBindings.Length + 1);
                    List<AnimationEvent> newEvents = new List<AnimationEvent>();

                    //Create curves but don't add keys
                    for (int i = 0; i < curveBindings.Length; ++i)
                    {
                        workingCurves.Add(new AnimationCurve());
                    }

                    cumTimeShift = 0f;
                    curStartTime = 0f;
                    int[] startKeyIndex = new int[workingCurves.Count]; //The start key for the current motion section
                    for (int i = 0; i < motionList.Count; ++i)
                    {
                        EditorUtility.DisplayProgressBar("Generate Modified Animation",
                            "Modifying Section " + i + " of " + clip.name, ((float)i) / ((float)motionList.Count));

                        MotionSection motionSection = motionList[i];

                        float startWarpTime = curStartTime;
                        float endWarpTime = motionSection.EndTime;
                        float warpScale = motionSection.GetSpeedMod(curStartTime, presets, this);

                        float localTimeShift = (endWarpTime - startWarpTime) - (endWarpTime - startWarpTime) * warpScale;

                        //Shift Curve Keys
                        for (int k = 0; k < curveBindings.Length; ++k)
                        {
                            EditorCurveBinding binding = curveBindings[k];
                            AnimationCurve originalCurve = AnimationUtility.GetEditorCurve(clip, binding);
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

                        //Shift Animation Clip EventsEvents
                        foreach (AnimationEvent evt in generatedClip.events)
                        {
                            if (evt.time > startWarpTime && evt.time < endWarpTime)
                            {
                                //Scale & Shift
                                evt.time = startWarpTime + ((evt.time - startWarpTime) * warpScale) - cumTimeShift;
                            }

                            newEvents.Add(evt);
                        }
                    }

                    for (int i = 0; i < workingCurves.Count; ++i)
                    {
                        EditorUtility.DisplayProgressBar("Generate Modified Animation",
                            "Generating Curves for clip: " + clip.name, ((float)i) / ((float)workingCurves.Count));

                        AnimationUtility.SetEditorCurve(generatedClip, curveBindings[i], workingCurves[i]);
                    }

                    AnimationUtility.SetAnimationEvents(generatedClip, newEvents.ToArray());
                    EditorUtility.SetDirty(generatedClip);

                    AssetDatabase.CreateAsset(generatedClip, a_directory + "/" + generatedClip.name + ".anim");

                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(generatedClip));
                    GeneratedClips.Add(generatedClip);
                }
                //End loop clips. Generations is complete

                //Go through Motion Sections again and modify tag tracks and events
                cumTimeShift = 0f; //This is the cumulative amount of time shift at the point of modification;
                curStartTime = 0f; //The start time for the current motion section
                for (int i = 0; i < motionList.Count; ++i)
                {
                    MotionSection motionSection = motionList[i];

                    float startWarpTime = curStartTime;
                    float endWarpTime = motionSection.EndTime;
                    float warpScale = motionSection.GetSpeedMod(curStartTime, presets, this);

                    float localTimeShift = (endWarpTime - startWarpTime) - (endWarpTime - startWarpTime) * warpScale;

                    //Shift MXM Tag Points
                    foreach (TagTrack track in TagTracks)
                    {
                        TagTrack newTrack = new TagTrack(track);
                        List<Vector2> tagList = newTrack.Tags;

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

                        GeneratedTagTracks.Add(newTrack);
                    }

                    cumTimeShift += localTimeShift;
                    curStartTime = endWarpTime;
                }  
            }

            EditorUtility.SetDirty(this);
            EditorUtility.ClearProgressBar();
#endif
        }

        public bool CheckAnimationCompatibility(bool a_useGeneric)
        {
            foreach (AnimationClip clip in m_clips)
            {
                if (clip.isHumanMotion == a_useGeneric)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

    }//End of class: MxMBlendSpace
}//End of class: MxMBlendSpace