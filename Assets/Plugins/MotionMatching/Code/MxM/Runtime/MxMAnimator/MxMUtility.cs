// ================================================================================================
// File: MxMUtility.cs
// 
// Authors:  Kenneth Claassen
// Date:     2019-10-15: Created this file.
// 
//     Contains a part of the 'MxM' namespace for 'Unity Engine'.
// ================================================================================================
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;    
#endif
using UnityEngine.Playables;
using UnityEngine.Animations;
using System;
using Object = UnityEngine.Object;

namespace MxM
{
    //============================================================================================
    /**
    *  @brief Static class for MxMutility functions
    *         
    *********************************************************************************************/
    public static class MxMUtility
    {
        //============================================================================================
        /**
        *  @brief Copies the data from one pose to another
        *  
        *  @param [ref PoseData] a_fromPose - the pose to copy data from
        *  @param [ref PoseData] a_toPose - the pose to copy data to
        *         
        *********************************************************************************************/
        public static void CopyPose(ref PoseData a_fromPose, ref PoseData a_toPose)
        {
            a_toPose.PoseId = a_fromPose.PoseId;
            a_toPose.AnimId = a_fromPose.AnimId;
            a_toPose.Time = a_fromPose.Time;
            a_toPose.NextPoseId = a_fromPose.NextPoseId;
            a_toPose.LastPoseId = a_fromPose.LastPoseId;
            a_toPose.Favour = a_fromPose.Favour;
            a_toPose.LocalVelocity = a_fromPose.LocalVelocity;
            a_toPose.Tags = a_fromPose.Tags;
            a_toPose.GenericTags = a_fromPose.GenericTags;
            a_toPose.AnimType = a_fromPose.AnimType;

            if (a_toPose.JointsData == null)
                a_toPose.JointsData = new JointData[a_fromPose.JointsData.Length];

            if (a_toPose.Trajectory == null)
                a_toPose.Trajectory = new TrajectoryPoint[a_fromPose.Trajectory.Length];

            System.Array.Copy(a_fromPose.JointsData, a_toPose.JointsData, a_fromPose.JointsData.Length);
            System.Array.Copy(a_fromPose.Trajectory, a_toPose.Trajectory, a_fromPose.Trajectory.Length);
        }

        //============================================================================================
        /**
        *  @brief Wraps an animation time based on the animation length
        *  
        *  @param [float] a_time - the time to wrap
        *  @param [float] a_length - the length of the animation clip
        *         
        *********************************************************************************************/
        public static float WrapAnimationTime(float a_time, float a_length)
        {
            if (a_time < -Mathf.Epsilon)
            {
                int wholeNumbers = Mathf.FloorToInt(Mathf.Abs(a_time) / a_length);
                a_time = a_length + (a_time + wholeNumbers * a_length);
            }
            else if (a_time > a_length)
            {
                int wholeNumbers = Mathf.FloorToInt(a_time / a_length);
                a_time = a_time - (wholeNumbers * a_length);
            }

            return a_time;
        }

        //============================================================================================
        /**
        *  @brief Rotates a vector by a quaternion
        *  
        *  @param [Vector3] a_v - the vector to rotate
        *  @param [Quaternion] a_q - the amoutn to rotate the vector by
        *         
        *********************************************************************************************/
        public static Vector3 RotateVector(Vector3 a_v, Quaternion a_q)
        {
            Vector3 u = new Vector3(a_q.x, a_q.y, a_q.z);

            float s = a_q.w;

            return 2f * Vector3.Dot(u, a_v) * u
                + (s * s - Vector3.Dot(u, a_v)) * a_v
                + 2f * s * Vector3.Cross(u, a_v);
        }

        //============================================================================================
        /**
        *  @brief Counts the number of active tags in a tag
        *  
        *  @param [ETags] a_tags - the tags to check
        *         
        *********************************************************************************************/
        public static uint CountFlags(ETags a_tags)
        {
            uint v = (uint)a_tags;

            v = v - ((v >> 1) & 0x55555555); 
            v = (v & 0x33333333) + ((v >> 2) & 0x33333333); 
            uint c = ((v + (v >> 4) & 0xF0F0F0F) * 0x1010101) >> 24; // count

            return c;
        }

#if UNITY_EDITOR        
        //============================================================================================
        /**
        *  @brief Finds a mirrored version of an animation clip with the _MIRROR postfix
        *  
        *  @param [AnimationClip] a_sourceClip
        *         
        *********************************************************************************************/
        public static AnimationClip FindMirroredClip(AnimationClip a_sourceClip)
        {
            if (a_sourceClip == null)
                return null;

            string animPath = AssetDatabase.GetAssetPath(a_sourceClip);
            var assetRepresentationsAtPath = AssetDatabase.LoadAllAssetRepresentationsAtPath(animPath);

            string desiredMirrorClipName = a_sourceClip.name + "_MIRROR";

            AnimationClip newMirrorClip = null;
            foreach (var assetRepresentation in assetRepresentationsAtPath)
            {
                AnimationClip animClip = assetRepresentation as AnimationClip;

                if (animClip == null)
                    continue;

                if (animClip.name == desiredMirrorClipName)
                {
                    newMirrorClip = animClip;
                    break;
                }
            }

            if (newMirrorClip != null)
            {
                return newMirrorClip;
            }
            else
            {
                Debug.LogWarning("Find Mirror Clip: Could not find mirrored version of AnimationClip: " + a_sourceClip.name);
                return a_sourceClip;
            }
        }
#endif        
        

    }//End of static class: MxMUtility
}//End of namespace: MxM