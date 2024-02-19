using System.Collections.Generic;
using UnityEngine;

namespace MxM
{
    //============================================================================================
    /**
    *  @brief 
    *         
    *********************************************************************************************/
    public class PoseMask : ScriptableObject
    {
        [SerializeField] public MxMAnimData TargetAnimData;
        [SerializeField] public int[] PoseUtilisation;
        [SerializeField] public int UsedPoseCount;

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void Initialize(MxMAnimData a_targetAnim)
        {
            if(a_targetAnim != null)
            {
                TargetAnimData = a_targetAnim;
                PoseUtilisation = new int[TargetAnimData.Poses.Length];
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void SetMask(Dictionary<int, int> a_poseUtilisation)
        {
            if (TargetAnimData != null)
            {
                foreach (KeyValuePair<int, int> kvp in a_poseUtilisation)
                {
                    PoseUtilisation[kvp.Key] = kvp.Value;

                    if (kvp.Value > 0)
                        ++UsedPoseCount;
                }
            }
        }
    }//End of class: PoseMask
}//End of namespace: MxM
