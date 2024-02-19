// ============================================================================================
// File: PoseProperty.cs
// 
// Authors:  Kenneth Claassen
// Date:     2018-05-19: Created this file.
// 
//     Contains a part of the 'MxMEditor' namespace for 'Unity Engine 2018.1'.
// 
// Copyright (c) 2017 Kenneth Claassen. All rights reserved.
// ============================================================================================
using UnityEngine;

namespace MxMEditor
{
    //=============================================================================================
    /**
    *  @brief Defines a pose property to match.
    *         
    *********************************************************************************************/
    [System.Serializable]
    public class PoseJoint
    {
        public string BoneName;
        public HumanBodyBones BoneId;

    }//End of class: PoseProperty
}//End of namespace: MxMEditor
