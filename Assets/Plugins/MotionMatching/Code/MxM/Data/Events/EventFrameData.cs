using UnityEngine;

namespace MxM
{
    [System.Serializable]
    public struct EventFrameData
    {
        //public Vector3 remainingDeltaSum;
        //public float remainingRotDeltaSum;

        public Vector3 relativeContactRoot;
        public float relativeContactRootRotY;

        public float RemainingWarpTime;
        public float RemainingRotWarpTime;

        public float Time;
        public bool WarpPosThisFrame;
        public bool WarpRotThisFrame;

        public EventFrameData(ref EventFrameData a_start, ref EventFrameData a_end, float a_lerp)
        {
            //remainingDeltaSum = Vector3.Lerp(a_start.remainingDeltaSum, a_end.remainingDeltaSum, a_lerp);
            //remainingRotDeltaSum = Mathf.Lerp(a_start.remainingRotDeltaSum, a_end.remainingRotDeltaSum, a_lerp);

            relativeContactRoot = Vector3.Lerp(a_start.relativeContactRoot, a_end.relativeContactRoot, a_lerp);
            relativeContactRootRotY = Mathf.LerpAngle(a_start.relativeContactRootRotY, a_end.relativeContactRootRotY, a_lerp);
            Time = Mathf.Lerp(a_start.Time, a_end.Time, a_lerp);

            RemainingWarpTime = Mathf.Lerp(a_start.RemainingWarpTime, a_end.RemainingWarpTime, a_lerp);
            RemainingRotWarpTime = Mathf.Lerp(a_start.RemainingRotWarpTime, a_end.RemainingRotWarpTime, a_lerp);
            
            if(a_lerp < 0.5f)
            {
                WarpPosThisFrame = a_start.WarpPosThisFrame;
                WarpRotThisFrame = a_start.WarpRotThisFrame;
            }
            else
            {
                WarpPosThisFrame = a_end.WarpPosThisFrame;
                WarpRotThisFrame = a_end.WarpRotThisFrame;
            }
        }
    }
}