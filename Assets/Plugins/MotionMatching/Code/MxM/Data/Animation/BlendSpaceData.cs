using System.Collections.Generic;
using UnityEngine;

namespace MxM
{
    [System.Serializable]
    public struct BlendSpaceData : IComplexAnimData
    {
        public int StartPoseId;
        public int EndPoseId;

        public Vector2 Magnitude;
        public Vector2 Smoothing;
        public bool NormalizeTime;

        public int[] ClipIds;      
        public Vector2[] Positions;

        public float Length;
        
        public float PlaybackSpeed;
        
        public EComplexAnimType ComplexAnimType { get { return EComplexAnimType.BlendSpace2D; } }

        public MotionCurveData CurveData;
        public MotionCurveData GetMotionCurveData() { return CurveData; }

        public BlendSpaceData(int a_startPoseId, int a_endPoseId, bool a_normalizeTime, Vector2 a_magnitude, 
            Vector2 a_smoothing, int[] a_clipIds, Vector2[] a_positions, float a_length, float a_playbackSpeed)
        {
            StartPoseId = a_startPoseId;
            EndPoseId = a_endPoseId;

            Magnitude = a_magnitude;
            Smoothing = a_smoothing;

            ClipIds = new int[a_clipIds.Length];
            Positions = new Vector2[a_positions.Length];
            NormalizeTime = a_normalizeTime;

            Length = a_length;
            
            PlaybackSpeed = a_playbackSpeed;

            CurveData = new MotionCurveData();

            //Ensure the clip that is closest to the center of the blend space is the first clip
            //in the blendspace clip list
            int closestIndex = 0;
            float closestDistance = float.MaxValue;
            for(int i=0; i < a_positions.Length; ++i)
            {
                float dist = a_positions[i].magnitude;

                if(dist < closestDistance)
                {
                    closestDistance = dist;
                    closestIndex = i;
                }
            }

            ClipIds[0] = a_clipIds[closestIndex];
            Positions[0] = a_positions[closestIndex];

            //Copy the rest of the clips and positions
            int targetIndex = 1;
            for(int i = 0; i < a_clipIds.Length; ++i)
            {
                if(i != closestIndex)
                {
                    ClipIds[targetIndex] = a_clipIds[i];
                    Positions[targetIndex] = a_positions[i];

                    ++targetIndex;
                }
            }
        }
    }//End of class: BlendSpace Data
}//ENd of namespace: MxM