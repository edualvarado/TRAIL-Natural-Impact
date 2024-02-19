using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MxM
{
    [System.Serializable]
    public struct SequenceData : IComplexAnimData
    {
        public int StartPoseId;
        public int EndPoseId;

        public int[] ClipIds;
        public AnimationCurve[] ClipWeights;

        public EComplexAnimType ComplexAnimType { get { return EComplexAnimType.Sequence; } }

        public MotionCurveData CurveData;
        public MotionCurveData GetMotionCurveData() { return CurveData; }

        public SequenceData(int a_startPoseId, int a_endPoseId, int[] a_clipIds, 
            AnimationCurve[] a_clipWeights)
        {
            StartPoseId = a_startPoseId;
            EndPoseId = a_endPoseId;
            ClipIds = a_clipIds;
            ClipWeights = a_clipWeights;

            CurveData = new MotionCurveData();
        }

        public void Setup(ref MxMPlayableState a_state, MxMAnimator m_mxmAnimator)
        {
            ref AnimationMixerPlayable mixer = ref m_mxmAnimator.MixerPlayable;


        }

        public void Update(ref MxMPlayableState a_state, MxMAnimator m_mxmAnimator)
        {

        }

        public void Destroy(ref MxMPlayableState a_state, MxMAnimator m_mxmAnimator)
        {

        }

    }//End of struct: SequenceData
}//End of namespace: MxM