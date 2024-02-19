// ============================================================================================
// File: IMxMTrajectory.cs
// 
// Authors:  Kenneth Claassen
// Date:     2017-09-16: Created this file.
// 
//     Contains a part of the 'MxM' namespace for 'Unity Engine 5'.
// ============================================================================================
using System.Collections.Generic;
using UnityEngine;

namespace MxM
{

    //=============================================================================================
    /**
    *  @brief Interface for any gameplay object which is to have a motion matching animation entity
    *         
    *********************************************************************************************/
    public interface IMxMTrajectory
    {
        TrajectoryPoint[] GetCurrentGoal();
        Transform GetTransform();
        void SetGoalRequirements(float[] a_predictionTimes);
        void SetGoal(TrajectoryPoint[] a_goal);
        void CopyGoalFromPose(ref PoseData a_poseData);
        void Pause();
        void UnPause();
        void ResetMotion(float a_rotation=0f);
        bool HasMovementInput();
        bool IsEnabled();

    }//End of interface: IMxMTrajectory
}//End of namespace: MxM