using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MxM
{

    public struct BlendSpace1DData : IComplexAnimData
    {
        public int StartPoseId;
        public int EndPoseId;

        public float Magnitude;
        public float Smoothing;

        public int[] ClipIds;
        public float[] Positions;

        public EComplexAnimType ComplexAnimType { get { return EComplexAnimType.BlendSpace1D; } }

        public MotionCurveData CurveData;
        public MotionCurveData GetMotionCurveData() { return CurveData; }

        public BlendSpace1DData(int a_startPoseId, int a_endPoseId, float a_magnitude,
            float a_smoothing, int[] a_clipIds, float[] a_positions)
        {
            StartPoseId = a_startPoseId;
            EndPoseId = a_endPoseId;

            Magnitude = a_magnitude;
            Smoothing = a_smoothing;

            ClipIds = a_clipIds;
            Positions = a_positions;

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
    }//End of struct: BlendSpace1DData
}//End of namespace: MxM