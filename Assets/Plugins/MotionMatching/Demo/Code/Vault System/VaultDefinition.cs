using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MxM;

namespace MxMGameplay
{
    [CreateAssetMenu(fileName = "VaultDefenition", menuName = "MxM/Gameplay/Vault Defenition", order = 1)]
    public class VaultDefinition : ScriptableObject
    {
        public MxMEventDefinition EventDefinition;

        public float MinRise;
        public float MaxRise;
        public float MinDepth;
        public float MaxDepth;
        public float MinDrop;
        public float MaxDrop;

        public bool DisableCollision = true;
        public bool LineUpWithObstacle = false;

        public EVaultType VaultType;

        public EVaultContactOffsetMethod OffsetMethod_Contact1;
        public Vector3 Offset_Contact1;

        public EVaultContactOffsetMethod OffsetMethod_Contact2;
        public Vector3 Offset_Contact2;
    }
}