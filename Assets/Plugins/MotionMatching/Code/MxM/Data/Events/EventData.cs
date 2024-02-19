// ================================================================================================
// File: EventData.cs
// 
// Authors:  Kenneth Claassen
// Date:     2019-02-27: Created this file.
// 
//     Contains a part of the 'MxMEditor' namespace for 'Unity Engine'.
// 
// Copyright (c) 2019 Kenneth Claassen. All rights reserved.
// ================================================================================================
using UnityEngine;

namespace MxM
{
    //============================================================================================
    /**
    *  @brief 
    *         
    *********************************************************************************************/
    [System.Serializable]
    public struct EventData
    {
        public int EventId;
        
        public float Windup;
        public float[] Actions; //Time between all contact points (for the first point its the time between end of windup to the first contact point
        public float FollowThrough;
        public float Recovery;

        public float Length; //Length of the event from start of windup to 
        public float TimeToHit;
        public float TotalActionDuration;

        public int StartPoseId;

        public EventContact[] WindupPoseContactOffsets; //Offset of the root to the contact point of the first subEvent from all poses within the windup phase (Count == poseCount)
        public EventContact[] SubEventContactOffsets; //Offset from the root of this contact event point to the contact point of the next sub event (Count == subEventCount - 1)
        public EventContact[] RootContactOffset; //Offset of the root from the contact point at the contact time one for each contact point (Count == subEventCount)

        //Event warping lookup table
        public EventFrameData[] WarpingLookupTable;
        
        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public EventData(int _eventId, float _windup, float[] _actions, float _followThrough, float _recovery,
            int _poseId, EventContact[] _rootOffset, EventContact[] _offsets, EventFrameData[] _warpLookupTable,
            EventContact[] _subEventOffsets = null)
        {
            EventId = _eventId;
            Windup = _windup;
            Actions = _actions;
            FollowThrough = _followThrough;
            Recovery = _recovery;
            StartPoseId = _poseId;
            WindupPoseContactOffsets = _offsets;
            SubEventContactOffsets = _subEventOffsets;
            RootContactOffset = _rootOffset;

            TimeToHit = Windup + Actions[0];

            TotalActionDuration = 0f;

            foreach(float action in Actions)
                TotalActionDuration += action;

            Length = Windup + TotalActionDuration + FollowThrough + Recovery;

            WarpingLookupTable = _warpLookupTable;
        }

    }//End of class: EventData
}//End of namespace: MxM