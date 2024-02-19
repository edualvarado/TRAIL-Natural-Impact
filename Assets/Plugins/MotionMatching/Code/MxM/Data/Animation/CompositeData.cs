using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MxM
{
    [System.Serializable]
    public struct CompositeData : IComplexAnimData
    {
        public int StartPoseId;
        public int EndPoseId;

        public float Length;
        public float ClipALength;
        public float ClipBLength;
        
        public float PlaybackSpeed;

        public int ClipIdA;
        public int ClipIdB;

        public EComplexAnimType ComplexAnimType { get { return EComplexAnimType.Composite; } }

        public MotionCurveData CurveData;
        public MotionCurveData GetMotionCurveData() { return CurveData; }

        public CompositeData(int a_startPoseId, int a_endPoseId, int a_clipIdA, int a_clipIdB, float a_clipALength, 
            float a_clipBLength, float a_playbackSpeed)
        {
            StartPoseId = a_startPoseId;
            EndPoseId = a_endPoseId;
            ClipIdA = a_clipIdA;
            ClipIdB = a_clipIdB;
            ClipALength = a_clipALength;
            ClipBLength = a_clipBLength;
            Length = ClipALength + ClipBLength;
            PlaybackSpeed = a_playbackSpeed;

            CurveData = new MotionCurveData();
        }
    }//End of struct: CompositeData
}//End of namespace: MxM
