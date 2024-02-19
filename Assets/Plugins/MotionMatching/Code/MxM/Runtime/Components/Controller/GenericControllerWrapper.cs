// ================================================================================================
// File: GenericControllerWrapper.cs
// 
// Authors:  Kenneth Claassen
// Date:     2019-07-2019: Created this file.
// 
//     Contains a part of the 'MxMGameplay' namespace for 'Unity Engine'.
// ================================================================================================
using UnityEngine;

namespace MxMGameplay
{
    //============================================================================================
    /**
    *  @brief This is a component that is intended to be a base class for a generic character 
    *  controller wrapper. The MxM trajectory controllers will use this wrapper to communicate
    *  with any kind of movement controller in the same way, making integrations simple.
    *         
    *********************************************************************************************/
    public abstract class GenericControllerWrapper : MonoBehaviour
    {
        public abstract bool IsGrounded { get; }
        public abstract Vector3 Velocity { get; }
        public abstract bool CollisionEnabled { get; set; }
        public abstract void Move(Vector3 a_move);
        public abstract void MoveAndRotate(Vector3 a_move, Quaternion a_rotDelta);
        public abstract void Rotate(Quaternion a_rotDelta);
        public abstract float MaxStepHeight { get; set; }
        public abstract float Height { get; set; }
        public abstract Vector3 Center { get; set; }
        public abstract float Radius { get; set; }
        public abstract bool ApplyGravityWhenGrounded { get; }
        public abstract void Initialize();
        public abstract void SetPosition(Vector3 a_position);
        public abstract void SetRotation(Quaternion a_rotation);
        public abstract void SetPositionAndRotation(Vector3 a_position, Quaternion a_rotation);
        public abstract Vector3 GetCachedMoveDelta();
        public abstract Quaternion GetCachedRotDelta();

    }//End of class: GenericControllerWrapper
}//End of namespace: MxMGameplay