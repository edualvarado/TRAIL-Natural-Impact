using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MxM
{
    [System.Serializable]
    public struct ClipData : IComplexAnimData
    {
        public int StartPoseId;
        public int EndPoseId;

        public int ClipId;
        public bool IsLooping;

        public float Length;
        
        public float PlaybackSpeed;

        public EComplexAnimType ComplexAnimType { get { return EComplexAnimType.Clip; } }

        public MotionCurveData CurveData;
        public MotionCurveData GetMotionCurveData() { return CurveData; }
        public ClipData(int a_startPoseId, int a_endPoseId, int a_clipId, bool a_isLooping, 
            float a_length, float a_playbackSpeed)
        {
            StartPoseId = a_startPoseId;
            EndPoseId = a_endPoseId;
            ClipId = a_clipId;
            IsLooping = a_isLooping;
            Length = a_length;
            PlaybackSpeed = a_playbackSpeed;

            CurveData = new MotionCurveData();
        }
      
    }//End of struct: ClipData
}//End of namespace: MxM