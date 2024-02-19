using UnityEngine;
using MxM;


public class MxMTrajectoryGeneratorTemplate : MxMTrajectoryGeneratorBase
{
    protected override void Awake()
    {
        base.Awake();

        //Do any custom awake stuff here
    }

    protected void Start()
    {
        //Do any custom start stuff here
    }

    //===========================================================================================
    /**
    *  This is called directly from the MxM animator. The animator basically tells the 
    *  IMxMTrajectory what the prediction times are so that data can be setup for the 
    *  prediction model. This is mostly done but the MxMController base. However, if you need 
    *  anything additionaly to be setup for your custom controller based on the prediction times
    *  then override this function.
    *  
    *  NOT MANDATORY
    *         
    *********************************************************************************************/
    protected override void Setup(float[] a_predictionTimes)
    {
        //a_predictionTimes contains the trajectory prediction times you set when you created your
        //MxMAnimData
    }

    //===========================================================================================
    /**
    *  This function is called from FixedUpdate in the MxMController base. It should be used to 
    *  update your motion prediction. This is where the bulk of your logic for your controller
    *  has to go. 
    *  
    *  Important things to do in this function
    *  - Fill p_trajFacingAngles with 'y' rotation facing angles for each trajectory point 
    *  - Fill p_trajPositons with positions for each trajectory point
    *  
    *  Note: These trajectory points are not the ones you specified in the MxMPreProcessor. This
    *  is a trajectory point at regular iterations. The trajectory points specified in the 
    *  MxMPreProcessor will be extracted from this evenly spaced list of points. 
    *  
    *  p_trajectoryIterations - The number of iterations for your trajectory. 
    *  p_timeStep - the time between each iteration
    *  p_sampleRate - the number of iterations per second
    *  p_trajPositions - an array of trajectory positions that needs to be filled
    *  p_trajFacingAngles - an array of trajectory facing angles that need to be filled
    *  
    *  MANDATORY
    *         
    *********************************************************************************************/
    protected override void UpdatePrediction(float a_deltaTime)
    {
        //Poll your stick input here

        //Use your own logic to turn that stick input into a trajectory, filling p_trajPositions
        //and p_trajFacingAngles for each iteration of the trajectory

        p_trajPositions[0] = Vector3.zero;
        p_trajFacingAngles[0] = 0f;

        for(int i = 1; i < p_trajectoryIterations; ++i)
        {
            //p_trajFacingAngles[i] = ??
            //p_trajPositions[i] = ??
        }
    }

    ////===========================================================================================
    ///**
    //*  If you set the MxMAnimator Root Motion to "Handled by Motion Match Entity then you need to 
    //*  use this function
    //*  
    //*  NOT MANDATORY
    //*         
    //*********************************************************************************************/
    //public override void HandleRootMotion(Vector3 a_rootPosition, Quaternion a_rootRotation, 
    //    Vector3 a_warp, Quaternion a_warRot)
    //{
    //    //The MxMTrajectoryGeneratorBase implementation of this should be sufficient for basic needs
    //    //See the override in MxMTrajectoryGenerator to see a variation where the root motion is
    //    //Cached and then applied during Update.
    //}

    //===========================================================================================
    /**
    *  To handle early exits on movement from events, MxMAnimator sometimes wants to know is the 
    *  IMxMTrajectory has any movement input (stick input). It's up to you to deside what 
    *  constitutes your controller having movement input. Most of the time its fine to just check
    *  if you have any stick input.
    *  
    *  If this is not overriden here it will always return true
    *  
    *  NOT MANDATORY | OVERRIDE RECOMMENDED
    *         
    *********************************************************************************************/
    public override bool HasMovementInput()
    {
        return true;
    }

    //===========================================================================================
    /**
    *  This is called automatically by the MxMAnimator when it is paused. You probably don't 
    *  need to overwrite it unless you have some custom pause logic. Make sure you call base.Pause()
    *  though if you do overwrite.
    *  
    *  NOT MANDATORY | NOT RECOMMENDED TO OVERRIDE
    *         
    *********************************************************************************************/
    public override void Pause()
    {
        base.Pause();

        //your pause logic here if any
    }

    //===========================================================================================
    /**
    *  This is called automatically by the MxMAnimator when it is unpaused. You probably don't 
    *  need to overwrite it unless you have some custom unpause logic. Make sure you call base.Pause()
    *  though if you do overwrite.
    *  
    *  NOT MANDATORY | NOT RECOMMENDED TO OVERRIDE
    *         
    *********************************************************************************************/
    public override void UnPause()
    {
        base.Pause();

        //your unpause logic here if any
    }

    //===========================================================================================
    /**
    *  At the end of an event, the trajectory from the event pose can be copied to your 
    *  controller to provide consistency. The MxMTrajectoryGeneratorBase does this for you but if you have 
    *  any logic that has to go with this do it here. Just be sure to call the base function.
    *  
    *  This will only be called if PoseEventTrajectoryMode in the MxMAniamtor is set to 
    *  EPostEventTrajectoryMode.InheritEvent
    *  
    *  It is unlikely that you will need to override this.
    *  
    *  NOT MANDATORY | NOT RECOMMENDED TO OVERRIDE
    *         
    *********************************************************************************************/
    public override void CopyGoalFromPose(ref PoseData a_poseData)
    {
        base.CopyGoalFromPose(ref a_poseData);
    }

    //===========================================================================================
    /**
    *  At the end of an event, the trajectory from the event pose can be reset on your 
    *  controller to provide consistency. The MxMTrajectoryGeneratorBase does this for you but if you have 
    *  any logic that has to go with this do it here. Just be sure to call the base function.
    *  
    *  This will only be called if PoseEventTrajectoryMode in the MxMAniamtor is set to 
    *  EPostEventTrajectoryMode.Reset
    *  
    *  It is unlikely that you will need to override thisS
    *  
    *  NOT MANDATORY | NOT RECOMMENDED TO OVERRIDE
    *         
    *********************************************************************************************/
    public override void ResetMotion(float a_rotation=0f)
    {
        base.ResetMotion(a_rotation);
    }

    //===========================================================================================
    /**
    *  @brief If you have tagged any left footsteps in you animation set on the utility timeline
    *  then they will be triggered at runtime which will call this function. This passes footstep
    *  data which has some information about the footsteps. 
    *  
    *  The footstep meta data is currently not customizable in version 1.5b but it may be later.
    *  
    *         
    *********************************************************************************************/
    public void NotifyLeftFootStep(ref FootStepData a_stepData)
    {

    }

    //===========================================================================================
    /**
    *  @brief Same as above except for right foot.
    *         
    *********************************************************************************************/
    public void NotifyRightFootStep(ref FootStepData a_stepData)
    {

    }
}
