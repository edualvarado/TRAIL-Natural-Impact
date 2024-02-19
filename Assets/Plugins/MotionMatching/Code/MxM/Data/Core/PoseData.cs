// ============================================================================================
// File: PoseData.cs
// 
// Authors:  Kenneth Claassen
// Date:     2017-09-16: Created this file.
// 
//     Contains a part of the 'MxM' namespace for 'Unity Engine 5'.
// 
// Copyright (c) 2017 Kenneth Claassen. All rights reserved.
// ============================================================================================
using UnityEngine;
using System.Collections.Generic;

namespace MxM
{
    //============================================================================================
    /**
    *  @brief A pose either saved to file or interpolated between two poses which is used
    *  for pose matching an motion matching
    *         
    *********************************************************************************************/
    [System.Serializable]
    public struct PoseData
    {
        public int PoseId;
        public int AnimId; //The ID of the animation type
        public int TracksId; //The ID of the tracks to use for this pose
        public int PrimaryClipId;
        public float Time; //The time that the pose is in the clip

        public int NextPoseId;
        public int LastPoseId;

        public float Favour;

        public Vector3 LocalVelocity;

        public JointData[] JointsData; 
        public TrajectoryPoint[] Trajectory;

        public ETags Tags; //Flags for all the tags that this posedata has
        public ETags FavourTags; //Flags for all favour tags that this posedata has
		public EGenericTags GenericTags; //Flags for all the generic tags that this pose data has public EMxMAnimtype AnimType; //Composite, IdleSet or BlendSpace
        public EUserTags UserTags;
		public EMxMAnimtype AnimType; //Composite, IdleSet or BlendSpace

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public PoseData(int a_poseId, int a_clipId, int a_tracksId, float a_time, Vector3 a_localVelocity, 
            int a_jointCount, int a_trajCount, float a_favour, ETags a_tags = ETags.None, 
            ETags a_favourTags = ETags.None, EGenericTags a_genericTags = EGenericTags.None,
            EUserTags a_userTags = EUserTags.None, EMxMAnimtype a_animType = EMxMAnimtype.Composite)

        {
            PoseId = a_poseId;
            AnimId = a_clipId;
            TracksId = a_tracksId;
            PrimaryClipId = a_clipId;
            Time = a_time;
            LocalVelocity = a_localVelocity;

            Favour = a_favour;

            JointsData = new JointData[a_jointCount];
            Trajectory = new TrajectoryPoint[a_trajCount];

            NextPoseId = -1;
            LastPoseId = -1;

            Tags = a_tags;
            FavourTags = a_favourTags;
			GenericTags = a_genericTags;
            UserTags = a_userTags;
            AnimType = a_animType;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public PoseData(PoseData a_pose)
        {
            PoseId = a_pose.PoseId;
            AnimId = a_pose.AnimId;
            TracksId = a_pose.TracksId;
            PrimaryClipId = a_pose.PrimaryClipId;
            Time = a_pose.Time;
            LocalVelocity = a_pose.LocalVelocity;
            NextPoseId = a_pose.NextPoseId;
            LastPoseId = a_pose.LastPoseId;

            Favour = a_pose.Favour;

            JointsData = new JointData[a_pose.JointsData.Length];
            for (int i = 0; i < a_pose.JointsData.Length; ++i)
                JointsData[i] = a_pose.JointsData[i];

            Trajectory = new TrajectoryPoint[a_pose.Trajectory.Length];
            for(int i=0; i < Trajectory.Length; ++i)
                Trajectory[i] = a_pose.Trajectory[i];

            Tags = a_pose.Tags;
            FavourTags = a_pose.FavourTags;
            GenericTags = a_pose.GenericTags;
            UserTags = a_pose.UserTags;
            AnimType = a_pose.AnimType;         
        }
    }//End of class: PoseData
}//End of namespace: MxM
