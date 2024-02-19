using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MotionCurveData
{
    public AnimationCurve[] Curves;
    public int[] CurveHandles;

    public Dictionary<int, int> CurveHandleRemap;

    public void SetCurves(List<AnimationCurve> a_curves, List<int> a_curveHandles)
    {
        if(a_curves == null || a_curveHandles == null)
        {
            Debug.Log("MotionCurveData: Trying to set curves during pre-process but the curves and/or handle lists are null. No curves added.");
            return;
        }

        Curves = a_curves.ToArray();
        CurveHandles = a_curveHandles.ToArray();
    }

    public void InitializeCurvesRuntime()
    {
        if (Curves == null || CurveHandles == null)
        {
            return;
        }
        
        if(Curves.Length != CurveHandles.Length)
        {
            Debug.LogError("MotionCurveData: Mismatch in number of curves and curve handles. " +
                "MotionCurveData has become corrupted. Try pre-processing the data again");
            return;
        }

        CurveHandleRemap = new Dictionary<int, int>(Curves.Length + 1);

        for(int i = 0; i < CurveHandles.Length; ++i)
        {
            CurveHandleRemap[CurveHandles[i]] = i;
        }
    }

    public AnimationCurve GetCurve(int a_curveHandle)
    {
        if (CurveHandleRemap.TryGetValue(a_curveHandle, out var localCurveHandle))
        {
            return Curves[localCurveHandle];
        }
        else
        {
            return null;
        }
    }

    public float GetCurveValue(int a_curveHandle, float a_time)
    {
        if (CurveHandleRemap != null && CurveHandleRemap.TryGetValue(a_curveHandle, out var localCurveHandle))
        {
            return Curves[localCurveHandle].Evaluate(a_time);
        }

        return 0f;
    }
}
