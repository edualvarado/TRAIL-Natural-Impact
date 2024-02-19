using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace MxM
{
    [System.Serializable]
    public class CalibrationData
    {
        public string CalibrationName;

        public float PoseTrajectoryRatio = 0.6f;
        public float PoseVelocityWeight = 3f;
        public float PoseAspectMultiplier = 1f;
        public float PoseResultantVelocityMultiplier = 0.2f;
        public float TrajPosMultiplier = 5f;
        public float TrajFAngleMultiplier = 0.04f;
        public float[] JointPositionWeights;
        public float[] JointVelocityWeights;

        public CalibrationData()
        {

        }

        public CalibrationData(CalibrationData a_copy)
        {
            CalibrationName = a_copy.CalibrationName;
            PoseTrajectoryRatio = a_copy.PoseTrajectoryRatio;
            PoseVelocityWeight = a_copy.PoseVelocityWeight;
            PoseAspectMultiplier = a_copy.PoseAspectMultiplier;
            PoseResultantVelocityMultiplier = a_copy.PoseResultantVelocityMultiplier;
            TrajPosMultiplier = a_copy.TrajPosMultiplier;
            TrajFAngleMultiplier = a_copy.TrajFAngleMultiplier;

            JointPositionWeights = new float[a_copy.JointPositionWeights.Length];
            JointVelocityWeights = new float[a_copy.JointVelocityWeights.Length];

            for(int i=0; i < a_copy.JointPositionWeights.Length; ++i)
            {
                JointPositionWeights[i] = a_copy.JointPositionWeights[i];
                JointVelocityWeights[i] = a_copy.JointVelocityWeights[i];
            }
        }

        public void Initialize(string a_name, MxMAnimData a_animData)
        {
            CalibrationName = a_name;

            if (a_animData != null)
            {
                JointPositionWeights = new float[a_animData.MatchBones.Length];
                JointVelocityWeights = new float[a_animData.MatchBones.Length];

                for (int i = 0; i < JointPositionWeights.Length; ++i)
                {
                    JointPositionWeights[i] = 3f;
                }

                for (int i = 0; i < JointVelocityWeights.Length; ++i)
                {
                    JointVelocityWeights[i] = 1f;
                }
            }
            else
            {
                Debug.LogError("Error: Trying to construct calibration data with null MxMAnimData");
            }
        }

        public void Validate(MxMAnimData a_parentAnimData)
        {
            if (a_parentAnimData == null)
            {
                Debug.LogError("Error: Trying to construct calibration data with null MxMAnimData");
                return;
            }
            
                if (a_parentAnimData.MatchBones.Length != JointPositionWeights.Length)
                {
                    float[] newJointPosWeights = new float[a_parentAnimData.MatchBones.Length];
                    float[] newJointVelWeights = new float[a_parentAnimData.MatchBones.Length];

                    for (int i = 0; i < newJointPosWeights.Length; ++i)
                    {
                        if (i < JointPositionWeights.Length)
                        {
                            newJointPosWeights[i] = JointPositionWeights[i];
                            newJointVelWeights[i] = JointVelocityWeights[i];
                        }
                        else
                        {
                            newJointPosWeights[i] = 3;
                            newJointVelWeights[i] = 1;
                        }
                    }

                    JointPositionWeights = newJointPosWeights;
                    JointVelocityWeights = newJointVelWeights;
                }
        }

        public bool IsValid(MxMAnimData a_parentAnimData)
        {
            if (a_parentAnimData == null)
                return false;

            if (JointPositionWeights.Length != a_parentAnimData.MatchBones.Length)
                return false;

            if(JointVelocityWeights.Length != a_parentAnimData.MatchBones.Length)
                return false;

            return true;
        }

        public void GenerateStandardStandardDeviationWeights(MxMAnimData a_animData, ETags a_tagSet)
        {
            if (a_animData == null)
            {
                Debug.Log("Cannot generate standard deviation weights with null animData. Aborting operation.");
                return;
            }

            int poseCount = 0;
            
            //For momentum
            Vector3 totalMomentum = Vector3.zero;
            foreach (PoseData pose in a_animData.Poses)
            {
                if (pose.Tags != a_tagSet)
                    continue;

                ++poseCount;

                totalMomentum += pose.LocalVelocity;
            }

            poseCount = Mathf.Max(poseCount, 1);

            Vector3 meanMomentum = totalMomentum / poseCount;
            
            //Sum the total of all atom distances squared to the mean
            float totalDistanceToMeanSquared_Momentum = 0f;

            foreach (PoseData pose in a_animData.Poses)
            {
                if (pose.Tags != a_tagSet)
                    continue;

                float distanceToMean_Momentum = Vector3.Distance(pose.LocalVelocity, meanMomentum);

                totalDistanceToMeanSquared_Momentum += distanceToMean_Momentum * distanceToMean_Momentum;
            }

            PoseVelocityWeight = 1.0f / Mathf.Sqrt(totalDistanceToMeanSquared_Momentum / poseCount);
            
            //For Each Bone
            for (int i = 0; i < a_animData.MatchBones.Length; ++i)
            {
                Vector3 totalPosition = Vector3.zero;
                Vector3 totalVelocity = Vector3.zero;
                
                foreach(PoseData pose in a_animData.Poses)
                {
                    if (pose.Tags != a_tagSet)
                        continue;

                    ref JointData poseJointData = ref pose.JointsData[i];

                    totalPosition += poseJointData.Position;
                    totalVelocity = poseJointData.Velocity;
                }
                
                //Find Mean
                Vector3 meanPosition = totalPosition / poseCount;
                Vector3 meanVelocity = totalVelocity / poseCount;
                
                //Sum the total of all atom distances squared to the mean
                float totalDistanceToMeanSquared_Position = 0f;
                float totalDistanceToMeanSquared_Velocity = 0f;

                foreach (PoseData pose in a_animData.Poses)
                {
                    if (pose.Tags != a_tagSet)
                        continue;

                    ref JointData poseJointData = ref pose.JointsData[i];

                    float distanceToMean_Position = Vector3.Distance(poseJointData.Position, meanPosition);
                    float distanceToMean_Velocity = Vector3.Distance(poseJointData.Velocity, meanVelocity);

                    totalDistanceToMeanSquared_Position = distanceToMean_Position * distanceToMean_Position;
                    totalDistanceToMeanSquared_Velocity = distanceToMean_Velocity * distanceToMean_Velocity;
                }
                
                //Set the standard deviation of these features
                JointPositionWeights[i] = 1.0f / Mathf.Sqrt(totalDistanceToMeanSquared_Position / poseCount);
                JointVelocityWeights[i] = 1.0f / Mathf.Sqrt(totalDistanceToMeanSquared_Velocity / poseCount);
            }
            
            //For each trajectory point
            // for (int i = 0; i < a_animData.PosePredictionTimes.Length; ++i)
            // {
            //     Vector3 totalPosition = Vector3.zero;
            //     float totalFacingAngle = 0f;
            //
            //     foreach (PoseData pose in a_animData.Poses)
            //     {
            //         if (pose.Tags != a_tagSet)
            //             continue;
            //
            //         ref TrajectoryPoint trajPoint = ref pose.Trajectory[i];
            //
            //         totalPosition += trajPoint.Position;
            //         totalFacingAngle += trajPoint.FacingAngle;
            //     }
            //     
            //     //Find Mean
            //     Vector3 meanPosition = totalPosition / poseCount;
            //     float meanFacing = totalFacingAngle / poseCount;
            //     
            //     //Sum the total of all atom distances squared to the mean
            //     float totalDistanceToMeanSquared_Position = 0f;
            //     float totalDistanceToMeanSquared_Facing = 0f;
            //
            //     foreach (PoseData pose in a_animData.Poses)
            //     {
            //         if (pose.Tags != a_tagSet)
            //             continue;
            //
            //         ref TrajectoryPoint trajPoint = ref pose.Trajectory[i];
            //
            //         float distanceToMean_Position = Vector3.Distance(trajPoint.Position, meanPosition);
            //         float distanceToMean_Facing = Mathf.Abs(trajPoint.FacingAngle - meanFacing);
            //     }
            //     
            //     
            // }
        }
    }//End of class: CalibrationData
}//End of namespace: MxM
