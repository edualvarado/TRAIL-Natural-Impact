// ================================================================================================
// File: MxMPlayableState.cs
// 
// Authors:  Kenneth Claassen
// Date:     2019-10-10: Created this file.
// 
//     Contains a part of the 'MxM' namespace for 'Unity Engine'.
// ================================================================================================

using UnityEngine.Playables;
using UnityEngine.Animations;

namespace MxM
{ 
    //============================================================================================
    /**
    *  @brief This struct is used to hold data on an MxMPlayable. This is useful for tracking the
    *  state of the playable without having to extract data every frame. Additionally, not all 
    *  required data can be extracted from playables for use with MotionMatching.
    *  
    *  An MxMPlayable is any animation (including blend spaces) That has been added to the 
    *  motion matching mixer. In order to run the motion matching blending logic, the state of
    *  blending needs to be continually tracked and maintained.This state struct is used to 
    *  perform this tracking.
    *         
    *********************************************************************************************/
    public struct MxMPlayableState
    {
        public Playable TargetPlayable;
        public AnimationMixerPlayable ParentMixer;
        public int InputIndex { get; private set; }
        public float Weight;
        public float HighestWeight;
        public EMxMAnimtype AnimType;
        public int AnimId;
        public int StartPoseId;
        public float StartTime;
        public float Age;
        public float DecayAge;
        public EBlendStatus BlendStatus;
        public int AnimDataId;

        //float Length { get; }
        public float Time
        {
            get { return StartTime + Age; }
            set { TargetPlayable.SetTime(value); }
        }

        public float TimeX2
        {
            set
            {
                if (TargetPlayable.IsValid())
                {
                    TargetPlayable.SetTime(value);
                    TargetPlayable.SetTime(value);
                }
            }
        }

        public float Speed
        {
            get
            {
                if (!TargetPlayable.IsValid())
                    return 1f;

                return (float)TargetPlayable.GetSpeed();
            }
            set
            {
                if(TargetPlayable.IsValid())
                    TargetPlayable.SetSpeed(value);
            }
        }

        //============================================================================================
        /**
        *  @brief Constructorfor the Playable state
        *  
        *  @param [int] a_inputIndex - the input index that this playable is connected to 
        *  @param [ref AnimationMixerPlayable] a_mixerPlayer - the animation mixer that this playable state is connected to
        *         
        *********************************************************************************************/
        public MxMPlayableState(int a_inputIndex, ref AnimationMixerPlayable a_mixerPlayable)
        {
            TargetPlayable = new Playable();
            ParentMixer = a_mixerPlayable;
            InputIndex = a_inputIndex;
            Weight = 0f;
            HighestWeight = 0f;
            AnimType = EMxMAnimtype.Clip;
            AnimId = -1;
            AnimDataId = 0;

            StartPoseId = -1;
            StartTime = 0f;
            Age = 0f;
            DecayAge = 0f;
            BlendStatus = EBlendStatus.None;
        }

        //============================================================================================
        /**
        *  @brief Sets the time of the playable recursively so all child playables ahve their time 
        *  set as well.
        *  
        *  @param [ref Playable] a_playable - the playable to set the time recursively on.
        *  @param [float] a_time - the time to set the playable to
        *         
        *********************************************************************************************/
        public static void SetTimeRecursive(ref Playable a_playable, float a_time)
        {
            if (a_playable.IsValid())
            {
                a_playable.SetTime(a_time);
                a_playable.SetTime(a_time);

                int inputCount = a_playable.GetInputCount();
                for (int i = 0; i < inputCount; ++i)
                {
                    Playable nextedPlayable = a_playable.GetInput(i);
                    SetTimeRecursive(ref nextedPlayable, a_time);
                }
            }
        }

        //============================================================================================
        /**
        *  @brief Sets the status of the Playable State to chosen with the passed pose data
        *  
        *  @param [ref PoseData] a_pose - the pose to set as chosen
        *  @param [float] a_startAge - Start the pose with an age (i.e. a time offset)
        *         
        *********************************************************************************************/
        public void SetAsChosenWithPose(ref PoseData a_pose, float a_startAge = 0f)
        {
            HighestWeight = Weight = 0f; //This may be wrong, start blend weight of 0?
            AnimType = a_pose.AnimType;
            AnimId = a_pose.AnimId;
            StartPoseId = a_pose.PoseId;
            StartTime = a_pose.Time;
            Age = a_startAge;
            DecayAge = 0f;
            BlendStatus = EBlendStatus.Chosen;
        }
        
        //============================================================================================
        /**
        *  @brief Sets the status of the Playable State to chosen with the passed pose data. However,
        * this version of the function assumes no blending 
        *
        *  @param [ref PoseData] a_pose - the pose to set as chosen
        *  @param [float] a_startAge - Start the pose with an age (i.e. a time offset)
        *
        *********************************************************************************************/
        public void SetAsChosenWithPoseNoBlend(ref PoseData a_pose, float a_startAge = 0f)
        {
            HighestWeight = Weight = 1.0f;
            AnimType = a_pose.AnimType;
            AnimId = a_pose.AnimId;
            StartPoseId = a_pose.PoseId;
            StartTime = a_pose.Time;
            Age = a_startAge;
            DecayAge = 0f;
            BlendStatus = EBlendStatus.Dominant;
        }

    }//End of struct: MxMPlayableState
}//End of namespace: MxM