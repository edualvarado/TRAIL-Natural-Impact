using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MxMGameplay
{
    [CreateAssetMenu(fileName = "VaultDetectionConfig", menuName = "MxM/Gameplay/VaultDetectionConfig", order = 0)]
    public class VaultDetectionConfig : ScriptableObject
    {
        public string ConfigName;
        public float DetectProbeRadius;
        public float DetectProbeAdvanceTime;
        public float ShapeAnalysisSpacing;

        

    }//End of class: VaultDetectionConfig
}//End of namespace: MxMGameplay