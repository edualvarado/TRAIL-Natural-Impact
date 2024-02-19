using System;
using System.Collections.Generic;
using UnityEngine;

namespace MxM
{
    //============================================================================================
    /**
    *  @brief Data structure used to define an idle animation set
    *         
    *********************************************************************************************/
    [System.Serializable]
    public struct IdleSetData : IComplexAnimData
    {
        public ETags Tags;

        public int PrimaryClipId;
        public int[] TransitionClipIds;
        public int[] SecondaryClipIds;

        public int[] PrimaryPoseIds;
        public int[] TransitionPoseIds;
        public int[] SecondaryPoseIds;

        public int MinLoops;
        public int MaxLoops;

        public ETags Traits;
        
        public float PlaybackSpeed;

        public EComplexAnimType ComplexAnimType { get { return EComplexAnimType.IdleSet; } }

        public MotionCurveData GetMotionCurveData() { return null; }

        public IdleSetData(ETags a_tags, int a_primaryClipId, int a_primaryPoseCount,
            int a_transitionClipCount, int a_secondaryClipCount, int a_minLoops, int a_maxLoops,
            float a_playbackSpeed, ETags a_traits = ETags.None)
        {
            Tags = a_tags;
            Traits = a_traits;
            MinLoops = a_minLoops;
            MaxLoops = a_maxLoops;

            PlaybackSpeed = a_playbackSpeed;

            PrimaryClipId = a_primaryClipId;
            PrimaryPoseIds = new int[a_primaryPoseCount];

            TransitionClipIds = new int[a_transitionClipCount];
            TransitionPoseIds = new int[a_transitionClipCount];

            SecondaryClipIds = new int[a_secondaryClipCount];
            SecondaryPoseIds = new int[a_secondaryClipCount];
        }

        public IdleSetData(ETags a_tags, int a_primaryClipId, int[] a_primaryPoseIds,
            int[] a_transitionClipsIds, int[] a_transitionPoseIds, int[] a_secondaryClipIds,
            int[] a_secondaryPoseIds, int a_minLoops, int a_maxLoops, float a_playbackSpeed, ETags a_traits = ETags.None)
        {
            Tags = a_tags;
            Traits = a_traits;
            MinLoops = a_minLoops;
            MaxLoops = a_maxLoops;

            PlaybackSpeed = a_playbackSpeed;

            PrimaryClipId = a_primaryClipId;

            PrimaryPoseIds = new int[a_primaryPoseIds.Length];
            Array.Copy(a_primaryPoseIds, PrimaryPoseIds, a_primaryPoseIds.Length);

            TransitionClipIds = new int[a_transitionClipsIds.Length];
            Array.Copy(a_transitionClipsIds, TransitionClipIds, a_transitionClipsIds.Length);

            TransitionPoseIds = new int[a_transitionPoseIds.Length];
            Array.Copy(a_transitionPoseIds, TransitionPoseIds, a_transitionPoseIds.Length);

            SecondaryClipIds = new int[a_secondaryClipIds.Length];
            Array.Copy(a_secondaryClipIds, SecondaryClipIds, a_secondaryClipIds.Length);

            SecondaryPoseIds = new int[a_secondaryPoseIds.Length];
            Array.Copy(a_secondaryPoseIds, SecondaryPoseIds, a_secondaryPoseIds.Length);
        }

    }//End of class: IdleSet
}//End of namespace: MxM