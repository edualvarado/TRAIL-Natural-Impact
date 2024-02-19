using System.Collections.Generic;
using UnityEngine;

namespace MxM
{
    public enum ECurveBlendType
    {
        Dominant,
        Chosen,
        DominantAndChosen,
        All
    }


    public partial class MxMAnimator : MonoBehaviour
    {
        public Dictionary<int, float> CachedCurveValues { get; private set; }
        public ECurveBlendType TrackedCurveBlendType { get; set; }

        public void RegisterTrackedCurve(string a_curveName)
        {
            int curveHandle = CurrentAnimData.CurveIdFromName(a_curveName);

            if (curveHandle > -1)
            {
                RegisterTrackedCurve(curveHandle);
            }
        }

        public void RegisterTrackedCurve(int a_curveHandle)
        {
            if (CachedCurveValues == null)
                CachedCurveValues = new Dictionary<int, float>(3);

            CachedCurveValues.Add(a_curveHandle, 0f);
        }

        public void RegisterTrackedCurves(List<string> a_curveNames)
        {
            if (CachedCurveValues == null)
                CachedCurveValues = new Dictionary<int, float>(a_curveNames.Count);

            foreach(string curveName in a_curveNames)
            {
                RegisterTrackedCurve(curveName);
            }
        }

        public void RegisterTrackedCurves(string[] a_curveNames)
        {
            if (CachedCurveValues == null)
                CachedCurveValues = new Dictionary<int, float>(a_curveNames.Length);

            foreach(string curveName in a_curveNames)
            {
                RegisterTrackedCurve(curveName);
            }
        }

        public void RegisterTrackedCurves(int[] a_curveHandles)
        {
            if (CachedCurveValues == null)
                CachedCurveValues = new Dictionary<int, float>(a_curveHandles.Length);

            foreach(int curveHandle in a_curveHandles)
            {
                CachedCurveValues.Add(curveHandle, 0f);
            }
        }

        public void RegisterTrackedCurves(List<int> a_curveHandles)
        {
            if (CachedCurveValues == null)
                CachedCurveValues = new Dictionary<int, float>(a_curveHandles.Count);

            foreach (int curveHandle in a_curveHandles)
            {
                CachedCurveValues.Add(curveHandle, 0f);
            }
        }

        public float GetCurveValue(int a_curveHandle, ECurveBlendType a_blendType)
        {
            float curveValue = 0f;

            if(CachedCurveValues != null && CachedCurveValues.TryGetValue(a_curveHandle, out curveValue))
            {
                return curveValue;
            }

            switch(a_blendType)
            {
                case ECurveBlendType.Dominant: curveValue = GetCurveValueFromChannel(ref m_animationStates[m_dominantBlendChannel], a_curveHandle); break;
                case ECurveBlendType.Chosen: curveValue = GetCurveValueFromChannel(ref m_animationStates[m_primaryBlendChannel], a_curveHandle); break;
                case ECurveBlendType.DominantAndChosen:
                    {
                        if(m_dominantBlendChannel == m_primaryBlendChannel)
                        {
                            curveValue = GetCurveValueFromChannel(ref m_animationStates[m_primaryBlendChannel], a_curveHandle); break;
                        }

                        ref MxMPlayableState dominantPlayableState = ref m_animationStates[m_dominantBlendChannel];
                        ref MxMPlayableState chosenPlayableState = ref m_animationStates[m_primaryBlendChannel];

                        float dominantValue = GetCurveValueFromChannel(ref dominantPlayableState, a_curveHandle);
                        float chosenValue = GetCurveValueFromChannel(ref chosenPlayableState, a_curveHandle);

                        curveValue = (dominantValue * dominantPlayableState.Weight) + (chosenValue * chosenPlayableState.Weight);
                        curveValue /= (dominantPlayableState.Weight + chosenPlayableState.Weight);

                    } break;
                case ECurveBlendType.All:
                    {
                        float totalWeight = 0f;
                        for(int i = 0; i < m_animationStates.Length; ++i)
                        {
                            ref MxMPlayableState blendChannel = ref m_animationStates[i];

                            if (blendChannel.BlendStatus == EBlendStatus.None)
                                continue;

                            curveValue += GetCurveValueFromChannel(ref blendChannel, a_curveHandle) * blendChannel.Weight;
                            totalWeight += blendChannel.Weight;
                        }

                        curveValue /= totalWeight;
                    }
                    break;
            }

            return curveValue;
        }

        public float GetCurveValue(string a_curveName, ECurveBlendType a_curveBlendType)
        {
            return GetCurveValue(GetCurveHandle(a_curveName), a_curveBlendType);
        }

        public int GetCurveHandle(string a_curveName)
        {
            return CurrentAnimData.CurveIdFromName(a_curveName);
        }

        private float GetCurveValueFromChannel(ref MxMPlayableState a_curveChannel, int a_curveHandle)
        {
            float time = a_curveChannel.Time;

            switch (a_curveChannel.AnimType)
            {
                case EMxMAnimtype.Composite:
                        return CurrentAnimData.Composites[a_curveChannel.AnimId].CurveData.GetCurveValue(a_curveHandle, time);
                case EMxMAnimtype.BlendSpace:
                    {
                        ref BlendSpaceData blendSpaceData = ref CurrentAnimData.BlendSpaces[a_curveChannel.AnimId];
                        time = Mathf.Repeat(time, blendSpaceData.Length);

                        return blendSpaceData.CurveData.GetCurveValue(a_curveHandle, time);
                    }
                case EMxMAnimtype.Clip:
                    {
                        ref ClipData clipData = ref CurrentAnimData.ClipsData[a_curveChannel.AnimId];

                        time = clipData.IsLooping ? Mathf.Repeat(time, clipData.Length) : time;

                        return clipData.CurveData.GetCurveValue(a_curveHandle, time);
                    }
                case EMxMAnimtype.BlendClip:
                    {
                        ref BlendClipData blendClip = ref CurrentAnimData.BlendClips[a_curveChannel.AnimId];
                        time = Mathf.Repeat(time, blendClip.Length);

                        return blendClip.CurveData.GetCurveValue(a_curveHandle, time);
                    }
                default:
                    return 0f;
            }
        }



    }//End of partial class: MxMAnimator
} //End of namespace: MxM