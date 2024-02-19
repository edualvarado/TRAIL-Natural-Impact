using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MxM
{
    public interface ILongitudinalWarper
    {
        void ApplySpeedScale(float a_speedScale);
        float RootMotionScale();

    }//End of interface: ILongitudinalWarper
}//End of namespace: MxM
