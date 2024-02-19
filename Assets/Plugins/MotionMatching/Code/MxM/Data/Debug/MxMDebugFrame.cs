using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MxM
{
    //============================================================================================
    /**
    *  @brief This struct contains all the relevant information from a single frame of the
    *  MxMAnimator needed to re-create the animation from that frame. It is tied to the MxMDebugData
    *  so that when DebugStates are played in succession, it allows for rewinding and fast 
    *  fowarding of the animation for debugging purposes.
    *         
    *********************************************************************************************/
    public struct MxMDebugFrame
    {    
        public bool Used;
        public float Time;
        public float DeltaTime;

        public Vector3 Position;
        public Quaternion Rotation;

        public EMxMStates AnimatorState;

        //MxM Settings
        public float UpdateInterval;
        public float PlaybackSpeed;
        public float PlaybackSmoothRate;
        public float MatchBlendTime;
        public float EventBlendTime;
        public float IdleBlendTime;
        public bool ApplyTrajectoryBlending;
        public float TrajectoryBlendingWeight;
        public bool FavourCurrentPose;
        public float CurrentPoseFavour;
        public EAngularErrorWarp AngularWarpType;
        public EAngularErrorWarpMethod AngularWarpMethod;
        public float AngularWarpRate;
        public float AngularWarpThreshold;
        public float AngularWarpAngleThreshold;
        public ELongitudinalErrorWarp LongErrorWarpType;
        public Vector2 SpeedWarpLimits;

        //Current Data
        public int AnimDataId;
        public int CalibrationDataId;

        //Animation playable State
        public int PrimaryBlendChannel;
        public int DominantBlendChannel;

        public struct PlayableStateDebugData
        {
            public int StartPoseId;
            public int AnimId;
            public EMxMAnimtype AnimType;
            public float Age;
            public float DecayAge;
            public float Weight;
            public EBlendStatus BlendStatus;
            public float Speed;
        }

        public PlayableStateDebugData[] playableStates;

        //Trajectory
        public TrajectoryPoint[] DesiredGoal;
        public TrajectoryPoint[] DesiredGoalBase;

        //Tracking
        public float TimeSinceMotionUpdate;
        public float TimeSinceMotionChosen;
        public float DesiredPlaybackSpeed;
        public bool EnforceClipChange;
        public float PoseInterpolationValue;
        public PoseData CurrentInterpolatedPose;

        //Error Warping
        public float LateralWarpAngle;
        public float LongitudinalWarpScale;

        //Cost Calculations
        public bool UpdateThisFrame;
        public float LastChosenCost;
        public float LastPoseCost;
        public float LastTrajectoryCost;

        //Tags
        public ETags RequiredTags;
        public ETags FavourTags;
        public float FavourTagMultiplier;

        //BlendSpaces
        public int CurrentBlendSpaceChannel;
        public Vector2 BlendSpacePosition;
        public Vector2 DesiredBlendSpacePosition;

        //Events
        public EventData CurrentEvent;
        public EMxMEventType EventType;
        public EEventState CurrentEventState;
        public float EventLength;
        public float TimeSinceEventTriggered;
        public int CurrentEventPriority;
        public bool ExitWithMotion;
        public int ContactCountToWarp;
        public EEventWarpType WarpType;
        public EEventWarpType RotWarpType;
        public EEventWarpType TimeWarpType;
        public float EventStartTimeOffset;
        public int CurrentEventContactIndex;
        public float CurrentEventContactTime;
        public Vector3 LinearWarpRate;
        public float LinearWarpRotRate;
        public EventContact CurrentEventRootWorld;
        public EventContact DesiredEventRootWorld;
        public EventContact[] CurrentEventContacts;
        public EPostEventTrajectoryMode PostEventTrajectoryMode;
        public float CumulativeActionDuration;

        //Idle
        public IdleSetData CurrentIdleSet;
        public EIdleState CurrentIdleState;
        public int CurrentIdleSetId;
        public float IdleDetectTimer;
        public int LastSecondaryIdleClipId;
        public int LastIdleGlobalClipId;
        public float TimeSinceLastIdleStarted;
        public int ChosenIdleLoopCount;

        //Layers
        public struct LayerData
        {
            public int LayerId;
            public int MaxClips;

            public float Weight;
            public bool Additive;
            public float Time;
            public AvatarMask Mask;
            public AnimationClip Clip;

            public int PrimaryInputId;
            public float TransitionRate;

            public float[] SubLayerWeights;
        }

        public LayerData[] Layers;
        public bool MecanimLayer;
        public float MecanimLayerWeight;
        public AvatarMask MecanimMask;

        public void SetChannelCount(int a_channelCount)
        {
            playableStates = new PlayableStateDebugData[a_channelCount];
        }

        public void SetTrajectoryCount(int a_trajCount)
        {
            DesiredGoal = new TrajectoryPoint[a_trajCount];
            DesiredGoalBase = new TrajectoryPoint[a_trajCount];
        }

    }//End of class: MxMDebugFrame
}//End of namespace: MxM