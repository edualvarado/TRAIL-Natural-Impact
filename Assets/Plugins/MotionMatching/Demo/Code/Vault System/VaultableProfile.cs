using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MxMGameplay
{
    //This struct describes 
    public struct VaultableProfile
    {
        public float Depth;
        public float Rise;
        public float Drop;

        public EVaultType VaultType;

        public Vector3 Contact1;
        public Vector3 Contact2;
    }
}
