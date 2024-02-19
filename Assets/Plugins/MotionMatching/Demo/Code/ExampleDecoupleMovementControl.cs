using UnityEngine;
using UnityEngine.Events;
using MxM;
using MxMGameplay;

namespace MxMExamples
{
    
    public class ExampleDecoupleMovementControl : MonoBehaviour
    {
        private MxMTrajectoryGenerator m_trajectoryGenerator;
        private MxMAnimationDecoupler m_animDecoupler;
        private GenericControllerWrapper m_charController;

        private void Awake()
        {
            m_trajectoryGenerator = GetComponentInChildren<MxMTrajectoryGenerator>();
            m_animDecoupler = GetComponent<MxMAnimationDecoupler>();
            m_charController = GetComponent<GenericControllerWrapper>();

            if(m_trajectoryGenerator == null)
            {
                Debug.LogError("ExampleDecoupleMovementControl cannot find a trajectory generator component. Disabling component");
                enabled = false;
                return;
            }

            if(m_animDecoupler == null)
            {
                Debug.LogError("ExampleDecoupleMovementControl cannot find a MxMAnimationDecoupler component. Disabling component");
                enabled = false;
                return;
            }

            if(m_charController == null)
            {
                Debug.LogError("ExampleDecoupleMovementControl canno find a GenericControllerWrapper component. Disabling component");
                enabled = false;
                return;
            }
        }

        public void UpdateMovementLogic(float a_deltaTime)
        {
            //For this example controller we just extract the motion at the start of the trajectory. 
            //Here I take the first 0.3s of the trajectory
            var motion = m_trajectoryGenerator.ExtractMotion(0.3f);

            Quaternion rotDelta = Quaternion.Inverse(transform.rotation) * Quaternion.AngleAxis(motion.angleDelta, Vector3.up);

            motion.angleDelta = rotDelta.eulerAngles.y;

            //To get the average motion of that 0.3s trajectory per Time.deltaTime we multiply by (Time.deltaTime / 0.3f)
            motion.moveDelta *= (a_deltaTime / 0.3f);
            motion.angleDelta *= (a_deltaTime / 0.3f);

            //The movement extracted from the trajectory is then blended in based on Root motion blending settings on the MxMAnimationDecoupler
            //You only need to do this if you want root motion blending
             motion = m_animDecoupler.CalculateRootMotionBlending(motion.moveDelta, motion.angleDelta, m_trajectoryGenerator.HasMovementInput());

            //Now we apply gravity on top of the root motion blended movement delta. This can be done manually or use built in functionality
            if (!m_charController.IsGrounded)
                motion.moveDelta.y = m_animDecoupler.CalculateGravityMoveDelta(a_deltaTime);

            //Now that we have the final move delta we can apply it to our generic controller wrapper
            m_charController.Move(motion.moveDelta);

            //For this particular movement control I've decided that the rotation of the capsule will always be the same as the model rotation
            //rotation is not particularly important for the controller itself so its relatively trivial. Best to keep it in line with what
            //the player is seeing.
            //m_charController.Rotate(Quaternion.AngleAxis(motion.angleDelta, Vector3.up));

            //transform.rotation = m_trajectoryGenerator.transform.rotation;
        }

    }//End of class: ExampleDecoupleMovementControl
}//End of namespace: MxMExamples