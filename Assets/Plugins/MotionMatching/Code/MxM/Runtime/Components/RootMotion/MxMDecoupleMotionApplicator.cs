// ================================================================================================
// File: MxMDecoupleMotionApplicator.cs
// 
// Authors:  Kenneth Claassen
// Date:     2019-08-10: Created this file.
// 
//     Contains a part of the 'MxM' namespace for 'Unity Engine'.
// ================================================================================================
using UnityEngine;
using MxMGameplay;

namespace MxM
{
    //===========================================================================================
    /**
    *  @brief A root motion applicator that can be used with a decouple type controller. Unlike a
    *  normal root motion applicator which applies motion through a controller, the decouple 
    *  controller root motion must be applied to the model which is decoupled from the controller.
    *  The decouple will take care of clamping the model to the controller.
    *         
    *********************************************************************************************/
    public class MxMDecoupleMotionApplicator : MonoBehaviour, IMxMRootMotion
    {
        [SerializeField]
        private GenericControllerWrapper m_charController;

        private MxMAnimationDecoupler m_decoupler;

        private Transform m_rootTransform;

        private Vector3 m_modelRewindPos;
        private Quaternion m_modelRewindRot;

        public bool ApplyMovementToController { get; set; }

        //===========================================================================================
        /**
        *  @brief Monobehaviour awake function. Ensures all references are setup before updating
        *         
        *********************************************************************************************/
        public virtual void Awake()
        {
            if (m_charController == null)
                m_charController = GetComponentInChildren<GenericControllerWrapper>();

            m_decoupler = m_charController.GetComponent<MxMAnimationDecoupler>();

            MxMAnimator mxmAnimator = GetComponent<MxMAnimator>();

            if (mxmAnimator.AnimationRoot != null)
            {
                m_rootTransform = mxmAnimator.AnimationRoot;

                if (m_charController == null)
                {
                    m_charController = m_rootTransform.GetComponent<GenericControllerWrapper>();
                }
            }
            else
            {
                m_rootTransform = transform;
            }

            if (m_charController == null)
            {
                Debug.LogWarning("Attached Generic Controller Wrapper is null. " +
                    "Root motion will be applied directly to transform");
            }
        }

        //===========================================================================================
        /**
        *  @brief Applies root motion + warping to the animated character transform
        *  
        *  @param [Vector3] a_rootPosition - the root position provided by the animator
        *  @param [Quaternion] a_rootRotation - the root rotation provided by the animator
        *  @param [Vector3] a_warp - the positional warping provided by the MxMAnimator (should be added to a_rootPosition)
        *  @param [Quaternion] a_warpRot - the rotation warping provided by the MxMAnimator (should be multiplied with a_rootRotation)
        *         
        *********************************************************************************************/
        public void HandleRootMotion(Vector3 a_rootDelta, Quaternion a_rootRotDelta,
            Vector3 a_warp, Quaternion a_warpRot, float a_deltaTime)
        {
            Vector3 moveDelta = a_rootDelta + a_warp;
            Quaternion rotDelta = a_rootRotDelta * a_warpRot;

            if (ApplyMovementToController && m_charController != null)
            {
                m_charController.MoveAndRotate(moveDelta, rotDelta);
                //If KCC we need to set the position of the model as well???
            }
            else
            {
                //Apply the root motion directly to the character model but not the controller
                m_rootTransform.SetPositionAndRotation(m_rootTransform.position + moveDelta,
                    m_rootTransform.rotation * rotDelta);
            }

            m_decoupler.UpdatePhase1(a_deltaTime);
        }

        //===========================================================================================
        /**
        *  @brief Applies angular error warping to the animated character transform.
        *  
        *  @param [Quaternion] a_warpRot - the rotation warping to apply to the character transform.
        *         
        *********************************************************************************************/
        public void HandleAngularErrorWarping(Quaternion a_warpRot)
        {
            if(ApplyMovementToController && m_charController != null)
            {
                m_charController.Rotate(transform.rotation * a_warpRot);
            }
            else
            {
                m_rootTransform.Rotate(a_warpRot.normalized.eulerAngles);
            }
        }

        //===========================================================================================
        /**
        *  @brief Sets the position of the character. 
        *  
        *  @param [Vector3] a_position - the new position for the character
        *         
        *********************************************************************************************/
        public void SetPosition(Vector3 a_position)
        {
            if (m_charController != null)
            {
                m_charController.SetPosition(a_position);
            }
            else
            {
                m_rootTransform.position = a_position;
            }
        } 

        //===========================================================================================
        /**
        *  @brief Sets the rotation of the character 
        *  
        *  @param [Quaternion] a_rotation - the new rotation for the character
        *         
        *********************************************************************************************/
        public void SetRotation(Quaternion a_rotation)
        {
            if (m_charController != null)
            {
                m_charController.SetRotation(a_rotation);
            }
            else
            {
                m_rootTransform.rotation = a_rotation;
            }
        }

        //===========================================================================================
        /**
        *  @brief Sets the position and rotation of the character
        *  
        *  @param [Vector3] a_position - the new position of the character
        *  @param [Quaternion] a_rotation - the new rotation of the character
        *         
        *********************************************************************************************/
        public void SetPositionAndRotation(Vector3 a_position, Quaternion a_rotation)
        {
            if(m_charController != null)
            {
                m_charController.SetPositionAndRotation(a_position, a_rotation);
            }
            else
            {
                m_rootTransform.SetPositionAndRotation(a_position, a_rotation);
            }
        }

        //===========================================================================================
        /**
        *  @brief Translates the character via a passed delta
        *  
        *  @param [Vector3] a_delta - the delta to translate the character by
        *         
        *********************************************************************************************/
        public void Translate(Vector3 a_delta)
        {
            if (m_charController != null)
            {
                m_charController.Move(a_delta);
            }
            else
            {
                m_rootTransform.Translate(a_delta);
            }
        }

        //===========================================================================================
        /**
        *  @brief Rotates the character via a given axis and angle
        *  
        *  @param [Vector3] a_axis - the axis around which the rotation takes place
        *  @param [float] a_angle - the angle of rotation around that axis to apply.
        *         
        *********************************************************************************************/
        public void Rotate(Vector3 a_axis, float a_angle)
        {
            if (m_charController != null)
            {
                m_charController.Rotate(Quaternion.AngleAxis(a_angle, a_axis));
            }
            else
            {
                m_rootTransform.Rotate(a_axis, a_angle);
            }
        }
        
        //===========================================================================================
        /**
        *  @brief
        *
        *********************************************************************************************/
        public void FinalizeRootMotion()
        {
            
        }

    }//End of class: MxMDecoupleMotionApplicator
}//End of namespace: MxM