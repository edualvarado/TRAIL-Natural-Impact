// ================================================================================================
// File: RootMotionApplicatorTemplate.cs
// 
// Authors:  Kenneth Claassen
// Date:     2019-10-10: Created this file.
// 
//     A template for 'Motion Matching for Unity'
// ================================================================================================
using UnityEngine;
using MxM;

//============================================================================================
/**
*  @brief A class to use as a template for creating custom 'root motion applicator' components \
*  with IMxMRootMotion
*         
*********************************************************************************************/
public class RootMotionApplicatorTemplate : MonoBehaviour, IMxMRootMotion
{
    //It is possible to override the animation root transform on the MxMAnimator so that root motion 
    //is applied to a different transform than what the MxMAnimator is placed on. It's best to apply
    //to this transform for the IMxMRootMotion component as well.
    private Transform m_rootTransform;

    //===========================================================================================
    /**
    *  Here you will need to get handles to the root transform. You can also do any other awake 
    *  logic you might need.
    *         
    *********************************************************************************************/
    public void Awake()
    {
        //Get the MxMAnimator component. You can do this in anyway. We just need it to get the 
        //root transform override that the MxMAnimator is using
        MxMAnimator mxmAnimator = GetComponent<MxMAnimator>();

        if (mxmAnimator.AnimationRoot != null)
        {
            //Set the root transform to that of the MxMAnimator AnimationRoot property
            m_rootTransform = mxmAnimator.AnimationRoot;
        }
        else
        {
            //If the MxMAnimator doesn't have a root transform then we default to the transform
            //that this component is placed on.
            m_rootTransform = transform;
        }

        //Your custom awake logic goes here
    }

    //===========================================================================================
    /**
    *  This is where the root motion is handled. By default, root motion is applied to the Animator
    *  transform. However, this is not ideal when you are using a character controller which has
    *  it's own specific movement logic. Apply your movement logic through his function when using
    *  the MxMAnimator.
    *  
    *  In addition to regular root motion. The MxMAnimator adds additional modifiers to root motion
    *  if there is any warping occuring. This usually only occurs during events where the animation
    *  may be warped to meet a specific contact point (both positionally and rotationally). Simply 
    *  combine the passed rootPosition / rootRotation with the warpPosition / warp rotation
    *  
    *  It is only called when the RootMotion setting on the MxMAnimator is set to 'RootMotionApplicator
    *  
    *  MANDATORY
    *         
    *********************************************************************************************/
    public void HandleRootMotion(Vector3 a_rootDelta, Quaternion a_rotationDelta,
            Vector3 a_warp, Quaternion a_warpRot, float a_deltaTime)
    {
        //Here is a how you would apply the above parameters to the root transform.
        //If you are using a character controller, make sure to instead apply the movement 
        //logic to that. At least when it is active

        //----- Commented out code below is pseudo code -----
        //if(myCharacterController.active)
        //{
        //    myCharacterController.Move(a_rootDelta + a_warp;
        //    myCharacterController.Rotate(a_rotationDelta * a_warpRot)
        //}
        //else
        //{
            m_rootTransform.position += a_rootDelta + a_warp;
            m_rootTransform.rotation *= a_rotationDelta * a_warpRot;
        //}
    }

    //===========================================================================================
    /**
    *  Motion Matching for Unity using angular warping to ensure that the character actually runs 
    *  in the direction you want it to go. This is because it's imposible to capture every possible
    *  angle of animation (Note this feature has to be turned on to work). This procedural rotation
    *  can be turned on independently of root motion. However, it needs to be applied through a 
    *  root motion applicator so that it can be applied to your character controller instead of
    *  just the transform.
    *  
    *  Event if you are not using root motion, you may still want to use AngularErrorWarping. Hence
    *  this override exists. It is only called when the RootMotion setting on the MxMAnimator is 
    *  set to 'RootMotionApplicator_AngularErrorWarpingOnly.
    *         
    *********************************************************************************************/
    public void HandleAngularErrorWarping(Quaternion a_warpRot)
    {
        //----- Commented out code below is pseudo code -----
        //if(myCharacterController.active)
        //{
        //    myCharacterController.Rotation(a_warpRot)
        //}
        //else
        //{
            m_rootTransform.rotation *= a_warpRot;
        //}
    }

    //===========================================================================================
    /**
    *  Depending on your settings, the animator may be updating in the physics update or not. This
    *  update function is called dependent on that setting with a passed delta time so you only 
    *  have to write this logic once regardless of your settings.
    *  
    *  While it is not mandatory, this is a good place to apply gravity to your controller
    *  or transform.
    *         
    *********************************************************************************************/
   // public void AnimatorDependentUpdate(float a_deltaTime)
    //{
        //----- Commented out code below is pseudo code for applying gravity -----
        //if (m_charController.enabled && EnableGravity && !m_charController.IsGrounded)
        //{
        //    m_moveDelta.y = (m_charController.Velocity.y +
        //        Physics.gravity.y * a_deltaTime) * a_deltaTime;
        //}

        //if (m_charController.enabled)
        //{
        //    m_charController.Move(m_moveDelta);
        //}
        //else
        //{
        //    transform.Translate(m_moveDelta, Space.World);
        //}
    //}

    //===========================================================================================
    /**
    *  Some character controllers take complete control of the character transform. Therefore, 
    *  MxM cannot directly manipulate the transform, it must be done through the controller. This
    *  function should contain any logic to set the position on your controller, otherwise default
    *  to setting the position on the rootTransform
    *         
    *********************************************************************************************/
    public void SetPosition(Vector3 a_position)
    {
        //----- Commented out code below is pseudo code for applying gravity -----
        //if (m_charController.enabled)
        //{
        //    m_charController.Teleport(a_position);
        //}
        //else
        //{
            m_rootTransform.position = a_position;
        //}
    }

    //===========================================================================================
    /**
    *  As with 'SetPosition' this function is the same but for the rotation
    *         
    *********************************************************************************************/
    public void SetRotation(Quaternion a_rotation)
    {
        //----- Commented out code below is pseudo code for applying gravity -----
        //if (m_charController.enabled)
        //{
        //    m_charController.SetRotation(a_rotation)
        //}
        //else
        //{
        m_rootTransform.rotation = a_rotation;
        //}
    }

    //===========================================================================================
    /**
    *  @brief This is basically Set Position and rotation combined
    *         
    *********************************************************************************************/
    public void SetPositionAndRotation(Vector3 a_position, Quaternion a_rotation)
    {
        //----- Commented out code below is pseudo code for applying gravity -----
        //if (m_charController.enabled)
        //{
        //    m_charController.Teleport(a_position, a_rotation);
        //}
        //else
        //{
            transform.SetPositionAndRotation(a_position, a_rotation);
        //}
    }

    //===========================================================================================
    /**
    *  @brief This function should instantly translate the character via the passed delta. In this
    * way, it should behave similarly to the SetPosition functions but use a delta instead to
    * instantly move the character.
    *         
    *********************************************************************************************/
    public void Translate(Vector3 a_delta)
    {
        // if (m_charController != null && m_charController.enabled)
        // {
        //     m_charController.Move(a_delta);
        // }
        // else
        // {
            m_rootTransform.Translate(a_delta);
       //}
    }

    //===========================================================================================
    /**
    *  @brief Same as the translate function expect for rotations instead of position
    *         
    *********************************************************************************************/
    public void Rotate(Vector3 a_axis, float a_angle)
    {
        // if (m_charController != null && m_charController.enabled)
        // {
        //     m_charController.Rotate(Quaternion.AngleAxis(a_angle, a_axis));
        // }
        // else
        // {
            m_rootTransform.Rotate(a_axis, a_angle);
       // }
    }
    
    //===========================================================================================
    /**
    *  @brief Called at the very end of OnAnimatorMove() in the MxMAnimator. Use this function to
    *  update any rigid bodies before the next physics update.
    *         
    *********************************************************************************************/
    public void FinalizeRootMotion()
    {
        
    }
    
    
}//End of class: RootMotionApplicatorTemplate