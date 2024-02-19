using UnityEngine;
using MxM;

namespace MxMGameplay
{
    public class ExampleDemoInput : MonoBehaviour
    {
        public void TestOnPoseChange(MxMAnimator.PoseChangeData a_poseChangeData)
        {
            Debug.Log("Pose Id: "+ a_poseChangeData.PoseId.ToString() + " Speed Mod: " 
                      + a_poseChangeData.SpeedMod.ToString() + " Time Offset: " + a_poseChangeData.TimeOffset.ToString());
        }
        
        private MxMAnimator m_mxmAnimator;
        //private MxMBlendSpaceLayers m_blendSpaceLayers;
        private MxMTrajectoryGenerator m_trajectoryGenerator;
        private MxMRootMotionApplicator m_rootMotionAplicator;
        private LocomotionSpeedRamp m_locomotionSpeedRamp;
        private VaultDetector m_vaultDetector;

        private GenericControllerWrapper m_controller;

        [SerializeField]
        private MxMEventDefinition m_slideDefinition = null;

        [SerializeField]
        private MxMEventDefinition m_jumpDefinition = null;

        [SerializeField] 
        private MxMEventDefinition m_danceDefinition = null;

        [Header("Input Profiles")]
        [SerializeField]
        private MxMInputProfile m_generalLocomotion = null;

        [SerializeField]
        private MxMInputProfile m_strafeLocomotion = null;

        [SerializeField]
        private MxMInputProfile m_sprintLocomotion = null;

        [Header("Warp Modules")] 
        [SerializeField]
        private WarpModule m_normalWarpModule;

        [SerializeField] 
        private WarpModule m_strafeWarpModule;

        private EState m_curState = EState.General;

        private Vector3 m_lastPosition;
        private Vector3 m_curVelocity;

        private bool m_controllerOn;
        private float m_defaultControllerHeight;
        private float m_defaultControllerCenter;

        private bool bIsDancing = false;

        private enum EState
        {
            General,
            Sliding,
            Jumping
        }

        // Start is called before the first frame update
        void Start()
        {
            m_mxmAnimator = GetComponentInChildren<MxMAnimator>();
            m_controller = GetComponent<GenericControllerWrapper>();
            m_trajectoryGenerator = GetComponentInChildren<MxMTrajectoryGenerator>();
            m_rootMotionAplicator = GetComponent<MxMRootMotionApplicator>();
            m_vaultDetector = GetComponent<VaultDetector>();
            m_locomotionSpeedRamp = GetComponent<LocomotionSpeedRamp>();
          
            // m_blendSpaceLayers = GetComponent<MxMBlendSpaceLayers>();
            //m_mxmAnimator.BlendInLayer(m_blendSpaceLayers.LayerId, 4, 1f);

            m_defaultControllerHeight = m_controller.Height;
            m_defaultControllerCenter = m_controller.Center.y;

            m_controller.Initialize();

            m_trajectoryGenerator.InputProfile = m_generalLocomotion;
        }

        // Update is called once per frame
        void Update()
        {
            if (m_locomotionSpeedRamp != null)
                m_locomotionSpeedRamp.UpdateSpeedRamp();

            Vector3 position = transform.position;
            m_curVelocity = (position - m_lastPosition) / Time.deltaTime;
            m_curVelocity.y = 0f;

            switch (m_curState)
            {
                case EState.General:
                    {
                        UpdateGeneral();
                    }
                    break;
                case EState.Sliding:
                    {
                        UpdateSliding();
                    }
                    break;
                case EState.Jumping:
                    {
                        UpdateJump();
                    }
                    break;
            }

            m_lastPosition = position;
        }
        
        private void UpdateGeneral()
        {
            if(Input.GetKeyDown(KeyCode.K))
            {
                if (bIsDancing)
                {
                    m_mxmAnimator.EndLoopEvent();
                    bIsDancing = false;
                    
                }
                else
                {
                    m_mxmAnimator.BeginEvent(m_danceDefinition);
                    bIsDancing = true;
                }
            }

            if (bIsDancing)
                return;


            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                m_locomotionSpeedRamp.BeginSprint();
                m_trajectoryGenerator.MaxSpeed = 6.7f;
                m_trajectoryGenerator.PositionBias = 6f;
                m_trajectoryGenerator.DirectionBias = 6f;
                m_mxmAnimator.SetCalibrationData("Sprint");
                m_trajectoryGenerator.InputProfile = m_sprintLocomotion;
            }
            else if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                m_locomotionSpeedRamp.ResetFromSprint();
                m_trajectoryGenerator.MaxSpeed = 4.3f;
                m_trajectoryGenerator.PositionBias = 10f;
                m_trajectoryGenerator.DirectionBias = 10f;
                m_mxmAnimator.SetCalibrationData("General");
                m_trajectoryGenerator.InputProfile = m_generalLocomotion;
            }

            if (Input.GetMouseButtonDown(1))
            {
                m_mxmAnimator.AddRequiredTag("Strafe");
                m_mxmAnimator.SetCalibrationData("Strafe");
                m_mxmAnimator.SetFavourCurrentPose(true, 0.95f);
                m_locomotionSpeedRamp.ResetFromSprint();
                m_mxmAnimator.SetWarpOverride(m_strafeWarpModule);
                m_mxmAnimator.AngularErrorWarpRate = 360f;
                m_mxmAnimator.AngularErrorWarpThreshold = 270f;
                m_mxmAnimator.AngularErrorWarpMethod = EAngularErrorWarpMethod.TrajectoryFacing;
                m_trajectoryGenerator.TrajectoryMode = ETrajectoryMoveMode.Strafe;
                m_trajectoryGenerator.InputProfile = m_strafeLocomotion;
                m_mxmAnimator.PastTrajectoryMode = EPastTrajectoryMode.CopyFromCurrentPose;
            }
            else if (Input.GetMouseButtonUp(1))
            {
                m_mxmAnimator.RemoveRequiredTag("Strafe");
                m_mxmAnimator.SetFavourCurrentPose(false, 1.0f);
                m_mxmAnimator.SetCalibrationData(0);
                m_mxmAnimator.SetWarpOverride(m_normalWarpModule);
                m_mxmAnimator.AngularErrorWarpRate = 60.0f;
                m_mxmAnimator.AngularErrorWarpThreshold = 90f;
                m_mxmAnimator.AngularErrorWarpMethod = EAngularErrorWarpMethod.CurrentHeading;
                m_trajectoryGenerator.TrajectoryMode = ETrajectoryMoveMode.Normal;
                m_trajectoryGenerator.InputProfile = m_generalLocomotion;
                m_mxmAnimator.PastTrajectoryMode = EPastTrajectoryMode.ActualHistory;
            }

            //Loop Blend Test
            if (Input.GetKeyDown(KeyCode.L))
            {
                m_mxmAnimator.BeginLoopBlend(1, 0f, 0f);
            }
            else if(Input.GetKeyDown(KeyCode.J))
            {
                m_mxmAnimator.SetBlendSpacePositionX(Mathf.Clamp(m_mxmAnimator.DesiredBlendSpacePositionX + 0.1f, -1f, 1f));
            }
            else if (Input.GetKeyDown(KeyCode.H))
            {
                m_mxmAnimator.SetBlendSpacePositionX(Mathf.Clamp(m_mxmAnimator.DesiredBlendSpacePositionX - 0.1f, -1f, 1f));
            }

            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                m_mxmAnimator.BeginEvent(m_slideDefinition);
                BeginSliding();

            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                if (m_jumpDefinition != null)
                {
                    m_jumpDefinition.ClearContacts();
                    m_jumpDefinition.AddDummyContacts(1);
                    m_mxmAnimator.BeginEvent(m_jumpDefinition);

                    ref readonly EventContact eventContact = ref m_mxmAnimator.NextEventContactRoot_Actual_World;

                    Ray ray = new Ray(eventContact.Position + (Vector3.up * 3.5f), Vector3.down);
                    RaycastHit rayHit = new RaycastHit();

                    if (Physics.Raycast(ray, out rayHit, 10f) 
                        && rayHit.distance > 1.5f 
                        && rayHit.distance < 5f)
                    {
                        m_mxmAnimator.ModifyDesiredEventContactPosition(rayHit.point);
                    }
                    else
                    {
                        m_mxmAnimator.ModifyDesiredEventContactPosition(eventContact.Position);
                    }

                    //m_controller.enabled = false;
                    m_rootMotionAplicator.EnableGravity = false;

                    m_curState = EState.Jumping;
                }
            }
        }

        private void BeginSliding()
        {
            m_curState = EState.Sliding;
            m_controller.Height = m_defaultControllerHeight / 2f;
            m_controller.Center = new Vector3(0f, m_defaultControllerCenter / 2f + 0.05f, 0f);
            m_vaultDetector.enabled = false;
        }

        private void UpdateSliding()
        {
            if (m_mxmAnimator.IsEventComplete)
            {
                m_curState = EState.General;
                m_controller.Center = new Vector3(0f, m_defaultControllerCenter, 0f);
                m_controller.Height = m_defaultControllerHeight;
                m_vaultDetector.enabled = true;
            }
        }

        private void UpdateJump()
        {
            if (m_mxmAnimator.IsEventComplete)
            {
                m_curState = EState.General;
                m_rootMotionAplicator.EnableGravity = true;
                m_lastPosition = transform.position;
                m_curVelocity = Vector3.zero;
            }
        }
    }
}
