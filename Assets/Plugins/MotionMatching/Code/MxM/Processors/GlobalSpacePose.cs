// ============================================================================================
// File: MxMPreProcessor.cs
// 
// Authors:  Kenneth Claassen
// Date:     2019-02-17: Created this file.
// 
//     Contains a part of the 'MxMEditor' namespace for 'Unity Engine'.
// 
// Copyright (c) 2019 Kenneth Claassen. All rights reserved.
// ============================================================================================

using System.Collections.Generic;
using UnityEngine;

namespace MxMEditor
{
    //============================================================================================
    /**
    *  @brief 
    *         
    *********************************************************************************************/
    public class GlobalSpacePose
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Forward;
        public List<Vector3> JointPositions = new List<Vector3>();

        public int ClipId;
        public float Time;

        public bool TrajectoryOnly;

    }//End of class: GlobalSpacePose
}//End of namespace: MxMEditor