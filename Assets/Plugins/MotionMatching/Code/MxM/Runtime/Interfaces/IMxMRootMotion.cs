// ================================================================================================
// File: IMxMRootMotion.cs
// 
// Authors:  Kenneth Claassen
// Date:     2019-10-10: Created this file.
// 
//     Contains a part of the 'MxM' namespace for 'Unity Engine'.
// ================================================================================================
using UnityEngine;

namespace MxM
{
    //============================================================================================
    /**
    *  @brief Interface for any component that is to handle root motion for the MxMAnimator.
    *         
    *********************************************************************************************/
    public interface IMxMRootMotion
    {
        void HandleRootMotion(Vector3 a_rootPosition, Quaternion a_rootRotation,
                Vector3 a_warp, Quaternion a_warpRot, float a_deltaTime);

        void HandleAngularErrorWarping(Quaternion a_warpRot);
        void SetPosition(Vector3 a_position);
        void SetRotation(Quaternion a_rotation);
        void SetPositionAndRotation(Vector3 a_position, Quaternion a_rotation);

        void Translate(Vector3 a_delta);
        void Rotate(Vector3 a_axis, float a_angle);

        /*Called at the end of OnAnimatorMove*/
        void FinalizeRootMotion();

    }//End of interface: IMxMRootMotion
}//End of namespace MxM