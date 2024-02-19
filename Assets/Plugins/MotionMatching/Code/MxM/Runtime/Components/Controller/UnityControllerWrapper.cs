// ================================================================================================
// File: UnityControllerWrapper.cs
// 
// Authors:  Kenneth Claassen
// Date:     2019-07-2019: Created this file.
// 
//     Contains a part of the 'MxMGameplay' namespace for 'Unity Engine'.
// ================================================================================================
using UnityEngine;
using UnityEngine.Assertions;

namespace MxMGameplay
{
    //============================================================================================
    /**
    *  @brief Wrapper for unity's capsule controller. Allows generic interface with MxM
    *         
    *********************************************************************************************/
    [RequireComponent(typeof(CharacterController))]
    public class UnityControllerWrapper : GenericControllerWrapper
    {
        [SerializeField]
        private bool m_applyGravityWhenGrounded = false;

        private CharacterController m_characterController;

        private bool m_enableCollision = true;

        public override bool IsGrounded { get { return m_characterController.isGrounded; } }
        public override bool ApplyGravityWhenGrounded { get { return m_applyGravityWhenGrounded; } }
        public override Vector3 Velocity { get { return m_characterController.velocity; } }
        public override float MaxStepHeight 
        { 
            get { return m_characterController.stepOffset; } 
            set { m_characterController.stepOffset = value; }
        }
        public override float Height 
        { 
            get { return m_characterController.height; } 
            set { m_characterController.height = value; }
        }

        public override Vector3 Center
        {
            get { return m_characterController.center; }
            set { m_characterController.center = value; }
        }
        public override float Radius 
        { 
            get { return m_characterController.radius; } 
            set { m_characterController.radius = value; }
        }

        public override void Initialize()
        {
            m_characterController.enableOverlapRecovery = true;
        }

        public override Vector3 GetCachedMoveDelta() { return Vector3.zero; }
        public override Quaternion GetCachedRotDelta() { return Quaternion.identity; }

        //Gets or sets whether collision is enabled (GenericControllerWrapper override)
        public override bool CollisionEnabled
        {
            get { return m_enableCollision; }
            set
            {
                if(m_enableCollision != value)
                {
                    if(m_enableCollision)
                        m_characterController.enabled = false;
                    else
                        m_characterController.enabled = true;
                }

                m_enableCollision = value;
            }
        }

        //============================================================================================
        /**
        *  @brief Sets up the wrapper, getting a reference to an attached character controller
        *         
        *********************************************************************************************/
        private void Awake()
        {
            m_characterController = GetComponent<CharacterController>();

            Assert.IsNotNull(m_characterController, "Error (UnityControllerWrapper): Could not find" +
                "CharacterController component");

            m_characterController.enableOverlapRecovery = true;
        }

        //============================================================================================
        /**
        *  @brief Moves the controller by an absolute Vector3 tranlation. Delta time must be applied
        *  before passing a_move to this function.
        *  
        *  @param [Vector3] a_move - the movement vector
        *         
        *********************************************************************************************/
        public override void Move(Vector3 a_move)
        {
            if (m_enableCollision)
            {
                m_characterController.Move(a_move);
            }
            else
            {
                m_characterController.transform.Translate(a_move, Space.World);
            }
        }

        //============================================================================================
        /**
        *  @brief Moves the controller by an absolute Vector3 tranlation and sets the new rotation
        *  of the controller. Delta time must be applied before passing a_move to this function.
        *  
        *  @param [Vector3] a_move - the movement vector
        *  @param [Quaternion] a_newRotation - the new rotation for the character
        *         
        *********************************************************************************************/
        public override void MoveAndRotate(Vector3 a_move, Quaternion a_rotDelta)
        {
            m_characterController.transform.rotation *= a_rotDelta;

            if (m_enableCollision)
            {
                m_characterController.Move(a_move);
                m_characterController.SimpleMove(a_move);
            }
            else
            {
                m_characterController.transform.Translate(a_move, Space.World);
            }
        }

        //============================================================================================
        /**
        *  @brief Sets the rotation of the character contorller with interpolation if supported
        *  
        *  @param [Quaternion] a_newRotation - the new rotation for the character
        *         
        *********************************************************************************************/
        public override void Rotate(Quaternion a_rotDelta)
        {
            m_characterController.transform.rotation *= a_rotDelta;
        }

        //============================================================================================
        /**
        *  @brief Monobehaviour OnEnable function which simply passes the enabling status onto the
        *  controller compopnnet
        *         
        *********************************************************************************************/
        private void OnEnable()
        {
            m_characterController.enabled = true;
        }

        //============================================================================================
        /**
        *  @brief Monobehaviour OnDisable function which simply passes the disabling status onto the
        *  controller compopnnet
        *         
        *********************************************************************************************/
        private void OnDisable()
        {
            m_characterController.enabled = true;
        }

        //============================================================================================
        /**
        *  @brief Sets the position of the character controller (teleport)
        *  
        *  @param [Vector3] a_position - the new position
        *         
        *********************************************************************************************/
        public override void SetPosition(Vector3 a_position)
        {
            transform.position = a_position;
        }

        //============================================================================================
        /**
        *  @brief Sets the rotation of the character controller (teleport)
        *         
        *  @param [Quaternion] a_rotation - the new rotation        
        *         
        *********************************************************************************************/
        public override void SetRotation(Quaternion a_rotation)
        {
            transform.rotation = a_rotation;
        }

        //============================================================================================
        /**
        *  @brief Sets the position and rotation of the character controller (teleport)
        *  
        *  @param [Vector3] a_position - the new position
        *  @param [Quaternion] a_rotation - the new rotation    
        *         
        *********************************************************************************************/
        public override void SetPositionAndRotation(Vector3 a_position, Quaternion a_rotation)
        {
            transform.SetPositionAndRotation(a_position, a_rotation);
        }

    }//End of class: UnityControllerWrapper
}//End of namespace: MxMGameplay