// ============================================================================================
// File: WarpModule.cs
// 
// Authors:  Kenneth Claassen
// Date:     2020-11-11: Created this file.
// 
//     Contains a part of the 'MxM' namespace for 'Unity Engine'.
// 
// Copyright (c) 2019 - 2020 Kenneth Claassen. All rights reserved.
// ============================================================================================
using UnityEngine;

namespace MxM
{
    [CreateAssetMenu(fileName = "MxMWarpDataModule", menuName = "MxM/Utility/MxMWarpDataModule", order = 0)]
    public class WarpModule : ScriptableObject
    {
        public EAngularErrorWarp AngularErrorWarpType = EAngularErrorWarp.On;
        public EAngularErrorWarpMethod AngularErrorWarpMethod = EAngularErrorWarpMethod.CurrentHeading;
        public float WarpRate = 60f;
        public float DistanceThreshold = 1f;
        public Vector2 AngleRange = new Vector2(0.5f, 90f);
        public ELongitudinalErrorWarp LongErrorWarpType = ELongitudinalErrorWarp.None;
        public Vector2 LongWarpSpeedRange = new Vector2(0.9f, 1.2f);
    }
}