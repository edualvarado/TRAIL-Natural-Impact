using System.Collections.Generic;
using UnityEngine;

namespace MxMEditor
{
    [CreateAssetMenu(fileName = "MxMConfigModule", menuName = "MxM/Core/Modules/MxMConfigModule", order = 1)]
    public class MotionMatchConfigModule : ScriptableObject
    {
        [SerializeField] private GameObject m_targetPrefab = null;
        [SerializeField] private List<PoseJoint> m_poseJoints = new List<PoseJoint>();
        [SerializeField] private List<float> m_trajectoryTimes = new List<float>();
        [SerializeField] private bool m_getBonesByName = false;

        public GameObject Prefab
        {
            get => m_targetPrefab;
            set => m_targetPrefab = value;
        }

        public List<PoseJoint> PoseJoints { get { return m_poseJoints; } }
        public List<float> TrajectoryPoints { get { return m_trajectoryTimes; } }
        public bool GetBonesByName { get { return m_getBonesByName; } }

    }//End of class: PoseConfigModule
}//End of namespace: MxMEditor