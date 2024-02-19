using UnityEngine;
using System;
using System.Collections.Generic;

namespace MxM
{
    [System.Serializable]
    public struct BlendClipData : IComplexAnimData
    {
        public int StartPoseId;
        public int EndPoseId;
        
        public bool NormalizeTime;

        public int[] ClipIds;
        public float[] Weightings;

        public float Length;

        public float PlaybackSpeed;

        public EComplexAnimType ComplexAnimType { get { return EComplexAnimType.BlendClip; } }

        public MotionCurveData CurveData;
        public MotionCurveData GetMotionCurveData() { return CurveData; }

        public BlendClipData(int a_startPoseId, int a_endPoseId, bool a_normalizeTime, int[] a_clipIds, 
            float[] a_weightings, float a_length, float a_playbackSpeed)
        {
            StartPoseId = a_startPoseId;
            EndPoseId = a_endPoseId;
            NormalizeTime = a_normalizeTime;
            CurveData = new MotionCurveData();
            Length = a_length;

            PlaybackSpeed = a_playbackSpeed;

            int actualWeightCount = 0;
            foreach (float weight in a_weightings)
            {
                if (weight > Mathf.Epsilon)
                {
                    ++actualWeightCount;
                }
            }

            ClipIds = new int[actualWeightCount];
            Weightings = new float[actualWeightCount];

            int index = 0;
            for (int i = 0; i < a_weightings.Length; ++i)
            {
                if(a_weightings[i] > Mathf.Epsilon)
                {
                    ClipIds[index] = a_clipIds[i];
                    Weightings[index] = a_weightings[i];
                    ++index;
                }

            }
        }
    }
}