using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MxMEditor
{

    [System.Serializable]
    public class MxMCurveTrack
    {
        public string CurveName;
        public AnimationCurve AnimCurve;

        public MxMCurveTrack()
        {
            CurveName = "";
            AnimCurve = new AnimationCurve();
        }

        public MxMCurveTrack(string a_curveName, float a_animLength)
        {
            CurveName = a_curveName;
            AnimCurve = new AnimationCurve();
            AnimCurve.AddKey(0f, 0f);
            AnimCurve.AddKey(a_animLength, 0f);
        }

        public MxMCurveTrack(string a_curveName, AnimationCurve a_copyCurve)
        {
            CurveName = a_curveName;
            AnimCurve = new AnimationCurve(a_copyCurve.keys);
        }
    }
}