using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace MxM
{
    //============================================================================================
    /**
    *  @brief Job for computing pose cost for 3 or less joints
    *         
    *********************************************************************************************/
    [BurstCompile(CompileSynchronously = true)]
    public struct ComputePose01Costs : IJobParallelForBatch
    {
        //Animation Pose Database
        [ReadOnly]
        public NativeArray<float3x3> InputPoses;

        //Desired Goal Data
        [ReadOnly]
        public float3x3 DesiredPose;

        //Cost Multipliers
        [ReadOnly]
        public float3 Weights;

        //Output
        [WriteOnly]
        public NativeArray<float> GoalCosts;


        public void Execute(int startIndex, int count)
        {
            for (int i = startIndex; i < startIndex + count; ++i)
            {

                //Calculate Cost of Pose Joint Positions
                float3x3 jointDiff = InputPoses[i] - DesiredPose;

                float3 jointDist = math.sqrt((jointDiff.c0 * jointDiff.c0) +
                                     (jointDiff.c1 * jointDiff.c1) +
                                     (jointDiff.c2 * jointDiff.c2)) * Weights;

                GoalCosts[i] = jointDist.x + jointDist.y + jointDist.z;
            }
        }
    }//End of struct: ComputePose03Costs

    //============================================================================================
    /**
    *  @brief Job for computing pose cost for 2
    *         
    *********************************************************************************************/
    [BurstCompile(CompileSynchronously = true)]
    public struct ComputePose02Costs : IJobParallelForBatch
    {
        //Animation Pose Database
        [ReadOnly]
        public NativeArray<Pose2> InputPoses;
        //Desired Goal Data
        [ReadOnly]
        public Pose2 DesiredPose;

        //Cost Multipliers
        [ReadOnly]
        public float3 JointVelocityWeights;

        [ReadOnly]
        public float2 JointPositionWeights;

        //Output
        [WriteOnly]
        public NativeArray<float> GoalCosts;

        public void Execute(int startIndex, int count)
        {
            for (int i = startIndex; i < startIndex + count; ++i)
            {

                //Calculate Cost of Pose Joint Positions
                float2x3 jointPosDiff = InputPoses[i].JointPositions - DesiredPose.JointPositions;

                float2 jointPosDist = math.sqrt((jointPosDiff.c0 * jointPosDiff.c0) +
                                      (jointPosDiff.c1 * jointPosDiff.c1) +
                                      (jointPosDiff.c2 * jointPosDiff.c2)) * JointPositionWeights;

                //Calculate Cost of Pose Velocity & Joints Velocity
                float3x3 jointVelDiff = InputPoses[i].JointVelocities - DesiredPose.JointVelocities;

                float3 jointVelDist = math.sqrt((jointVelDiff.c0 * jointVelDiff.c0) +
                                      (jointVelDiff.c1 * jointVelDiff.c1) +
                                      (jointVelDiff.c2 * jointVelDiff.c2)) * JointVelocityWeights;

                GoalCosts[i] = jointVelDist.x + jointVelDist.y + jointVelDist.z
                                + jointPosDist.x + jointPosDist.y;
            }
        }
    }//End of struct: ComputePose02Costs

    //============================================================================================
    /**
    *  @brief Job for computing pose cost for 3 or less joints
    *         
    *********************************************************************************************/
    [BurstCompile(CompileSynchronously = true)]
    public struct ComputePose03Costs : IJobParallelForBatch
    {
        //Animation Pose Database
        [ReadOnly]
        public NativeArray<Pose3> InputPoses;
        //Desired Goal Data
        [ReadOnly]
        public Pose3 DesiredPose;

        //Cost Multipliers
        [ReadOnly]
        public float4 JointVelocityWeights;

        [ReadOnly]
        public float3 JointPositionWeights;

        //Output
        [WriteOnly]
        public NativeArray<float> GoalCosts;

        public void Execute(int startIndex, int count)
        {
            for (int i = startIndex; i < startIndex + count; ++i)
            {

                //Calculate Cost of Pose Joint Positions
                float3x3 jointPosDiff = InputPoses[i].JointPositions - DesiredPose.JointPositions;

                float3 jointPosDist = math.sqrt((jointPosDiff.c0 * jointPosDiff.c0) +
                                      (jointPosDiff.c1 * jointPosDiff.c1) +
                                      (jointPosDiff.c2 * jointPosDiff.c2)) * JointPositionWeights;

                //Calculate Cost of Pose Velocity & Joints Velocity
                float4x3 jointVelDiff = InputPoses[i].JointVelocities - DesiredPose.JointVelocities;

                float4 jointVelDist = math.sqrt((jointVelDiff.c0 * jointVelDiff.c0) +
                                      (jointVelDiff.c1 * jointVelDiff.c1) +
                                      (jointVelDiff.c2 * jointVelDiff.c2)) * JointVelocityWeights;

                GoalCosts[i] = jointVelDist.x + jointVelDist.y + jointVelDist.z + jointVelDist.w
                                + jointPosDist.x + jointPosDist.y + jointPosDist.z;
            }
        }
    }//End of struct: ComputePose03Costs

    //============================================================================================
    /**
    *  @brief Job for computing pose cost for 4 or less joints
    *         
    *********************************************************************************************/
    [BurstCompile(CompileSynchronously = true)]
    public struct ComputePose04Costs : IJobParallelForBatch
    {
        //Animation Pose Database
        [ReadOnly]
        public NativeArray<Pose4> InputPoses;

        //Desired Goal Data
        [ReadOnly]
        public Pose4 DesiredPose;

        //Cost Multipliers
        [ReadOnly]
        public float4 JointVelocityWeights;

        [ReadOnly]
        public float4 JointPositionWeights;

        [ReadOnly]
        public float BodyVelocityWeight;

        //Output
        [WriteOnly]
        public NativeArray<float> GoalCosts;


        public void Execute(int startIndex, int count)
        {
            for (int i = startIndex; i < startIndex + count; ++i)
            {

                //Calculate Cost of Pose Joint Positions
                float4x3 jointPosDiff = InputPoses[i].JointPositions - DesiredPose.JointPositions;

                float4 jointPosDist = math.sqrt((jointPosDiff.c0 * jointPosDiff.c0) +
                                      (jointPosDiff.c1 * jointPosDiff.c1) +
                                      (jointPosDiff.c2 * jointPosDiff.c2)) * JointPositionWeights;

                //Calculate Cost of Pose Velocity & Joints Velocity
                float4x3 jointVelDiff = InputPoses[i].JointVelocities - DesiredPose.JointVelocities;

                float4 jointVelDist = math.sqrt((jointVelDiff.c0 * jointVelDiff.c0) +
                                      (jointVelDiff.c1 * jointVelDiff.c1) +
                                      (jointVelDiff.c2 * jointVelDiff.c2)) * JointVelocityWeights;

                float3 bodyVel = (InputPoses[i].BodyVelocity - DesiredPose.BodyVelocity);
                float bodyVelDist = ((bodyVel.x * bodyVel.x) +
                                              (bodyVel.y * bodyVel.y) +
                                              (bodyVel.z * bodyVel.z)) * BodyVelocityWeight;



                GoalCosts[i] = jointVelDist.x + jointVelDist.y + jointVelDist.z + jointVelDist.w
                                + jointPosDist.x + jointPosDist.y + jointPosDist.z + bodyVelDist;

            }
        }
    }//End of struct: ComputePose04Job

    //============================================================================================
    /**
    *  @brief Job for computing pose cost for 3 or less joints
    *         
    *********************************************************************************************/
    [BurstCompile(CompileSynchronously = true)]
    public struct ComputePose05Costs : IJobParallelForBatch
    {
        //Animation Pose Database
        [ReadOnly]
        public NativeArray<Pose5> InputPoses;

        //Desired Goal Data
        [ReadOnly]
        public Pose5 DesiredPose;

        //Cost Multipliers
        [ReadOnly]
        public float3 JointVelocityWeightsA;

        [ReadOnly]
        public float3 JointVelocityWeightsB;

        [ReadOnly]
        public float3 JointPositionWeightsA;

        [ReadOnly]
        public float2 JointPositionWeightsB;

        //Output
        public NativeArray<float> GoalCosts;

        public void Execute(int startIndex, int count)
        {
            for (int i = startIndex; i < startIndex + count; ++i)
            {
                //Calculate Cost of Pose Joint Positions
                float3x3 jointPosDiff = InputPoses[i].JointPositionsA - DesiredPose.JointPositionsA;

                float3 jointPosDist = math.sqrt((jointPosDiff.c0 * jointPosDiff.c0) +
                                      (jointPosDiff.c1 * jointPosDiff.c1) +
                                      (jointPosDiff.c2 * jointPosDiff.c2)) * JointPositionWeightsA;

                //Calculate Cost of Pose Velocity & Joints Velocity
                float3x3 jointVelDiff = InputPoses[i].JointVelocitiesA - DesiredPose.JointVelocitiesA;

                float3 jointVelDist = math.sqrt((jointVelDiff.c0 * jointVelDiff.c0) +
                                      (jointVelDiff.c1 * jointVelDiff.c1) +
                                      (jointVelDiff.c2 * jointVelDiff.c2)) * JointVelocityWeightsA;

                GoalCosts[i] = jointVelDist.x + jointVelDist.y + jointVelDist.z
                                + jointPosDist.x + jointPosDist.y + jointPosDist.z;

                float2x3 jointPosDiffB = InputPoses[i].JointPositionsB - DesiredPose.JointPositionsB;

                float2 jointPosDistB = math.sqrt((jointPosDiffB.c0 * jointPosDiffB.c0) +
                                      (jointPosDiffB.c1 * jointPosDiffB.c1) +
                                      (jointPosDiffB.c2 * jointPosDiffB.c2)) * JointPositionWeightsB;

                jointVelDiff = InputPoses[i].JointVelocitiesB - DesiredPose.JointVelocitiesB;

                jointVelDist = math.sqrt((jointVelDiff.c0 * jointVelDiff.c0) +
                                      (jointVelDiff.c1 * jointVelDiff.c1) +
                                      (jointVelDiff.c2 * jointVelDiff.c2)) * JointVelocityWeightsB;

                GoalCosts[i] += jointVelDist.x + jointVelDist.y + jointVelDist.z
                                + jointPosDistB.x + jointPosDistB.y;
            }
        }
    }//End of struct: ComputePose06Costs

    //============================================================================================
    /**
    *  @brief Job for computing pose cost for 3 or less joints
    *         
    *********************************************************************************************/
    [BurstCompile(CompileSynchronously = true)]
    public struct ComputePose06Costs : IJobParallelForBatch
    {
        //Animation Pose Database
        [ReadOnly]
        public NativeArray<Pose6> InputPoses;

        //Desired Goal Data
        [ReadOnly]
        public Pose6 DesiredPose;

        //Cost Multipliers
        [ReadOnly]
        public float3 JointVelocityWeightsA;

        [ReadOnly]
        public float3 JointVelocityWeightsB;

        [ReadOnly]
        public float BodyVelocityWeight;

        [ReadOnly]
        public float3 JointPositionWeightsA;

        [ReadOnly]
        public float3 JointPositionWeightsB;

        //Output
        public NativeArray<float> GoalCosts;

        public void Execute(int startIndex, int count)
        {
            for (int i = startIndex; i < startIndex + count; ++i)
            {

                //Calculate Cost of Pose Joint Positions
                float3x3 jointPosDiff = InputPoses[i].JointPositionsA - DesiredPose.JointPositionsA;

                float3 jointPosDist = math.sqrt((jointPosDiff.c0 * jointPosDiff.c0) +
                                      (jointPosDiff.c1 * jointPosDiff.c1) +
                                      (jointPosDiff.c2 * jointPosDiff.c2)) * JointPositionWeightsA;

                //Calculate Cost of Pose Velocity & Joints Velocity
                float3x3 jointVelDiff = InputPoses[i].JointVelocitiesA - DesiredPose.JointVelocitiesA;

                float3 jointVelDist = math.sqrt((jointVelDiff.c0 * jointVelDiff.c0) +
                                      (jointVelDiff.c1 * jointVelDiff.c1) +
                                      (jointVelDiff.c2 * jointVelDiff.c2)) * JointVelocityWeightsA;

                float3 bodyVel = (InputPoses[i].BodyVelocity - DesiredPose.BodyVelocity);
                float bodyVelDist = ((bodyVel.x * bodyVel.x) +
                                              (bodyVel.y * bodyVel.y) +
                                              (bodyVel.z * bodyVel.z)) * BodyVelocityWeight;

                GoalCosts[i] = jointVelDist.x + jointVelDist.y + jointVelDist.z
                                + jointPosDist.x + jointPosDist.y + jointPosDist.z
                                + bodyVelDist;

                jointPosDiff = InputPoses[i].JointPositionsB - DesiredPose.JointPositionsB;

                jointPosDist = math.sqrt((jointPosDiff.c0 * jointPosDiff.c0) +
                                      (jointPosDiff.c1 * jointPosDiff.c1) +
                                      (jointPosDiff.c2 * jointPosDiff.c2)) * JointPositionWeightsB;

                float3x3 jointVelDiffB = InputPoses[i].JointVelocitiesB - DesiredPose.JointVelocitiesB;

                float3 jointVelDistB = math.sqrt((jointVelDiffB.c0 * jointVelDiffB.c0) +
                                      (jointVelDiffB.c1 * jointVelDiffB.c1) +
                                      (jointVelDiffB.c2 * jointVelDiffB.c2)) * JointVelocityWeightsB;

                GoalCosts[i] += jointVelDistB.x + jointVelDistB.y + jointVelDistB.z
                                + jointPosDist.x + jointPosDist.y + jointPosDist.z;
            }
        }
    }//End of struct: ComputePose06Costs

    //============================================================================================
    /**
    *  @brief Job for computing pose cost for 3 or less joints
    *         
    *********************************************************************************************/
    [BurstCompile(CompileSynchronously = true)]
    public struct ComputePose07Costs : IJobParallelForBatch
    {
        //Animation Pose Database
        [ReadOnly]
        public NativeArray<Pose7> InputPoses;

        //Desired Goal Data
        [ReadOnly]
        public Pose7 DesiredPose;

        //Cost Multipliers
        [ReadOnly]
        public float4 JointVelocityWeightsA;

        [ReadOnly]
        public float4 JointVelocityWeightsB;

        [ReadOnly]
        public float4 JointPositionWeightsA;

        [ReadOnly]
        public float3 JointPositionWeightsB;

        //Output
        public NativeArray<float> GoalCosts;

        public void Execute(int startIndex, int count)
        {
            for (int i = startIndex; i < startIndex + count; ++i)
            {

                //Calculate Cost of Pose Joint Positions
                float4x3 jointPosDiff = InputPoses[i].JointPositionsA - DesiredPose.JointPositionsA;

                float4 jointPosDist = math.sqrt((jointPosDiff.c0 * jointPosDiff.c0) +
                                      (jointPosDiff.c1 * jointPosDiff.c1) +
                                      (jointPosDiff.c2 * jointPosDiff.c2)) * JointPositionWeightsA;

                //Calculate Cost of Pose Velocity & Joints Velocity
                float4x3 jointVelDiff = InputPoses[i].JointVelocitiesA - DesiredPose.JointVelocitiesA;

                float4 jointVelDist = math.sqrt((jointVelDiff.c0 * jointVelDiff.c0) +
                                      (jointVelDiff.c1 * jointVelDiff.c1) +
                                      (jointVelDiff.c2 * jointVelDiff.c2)) * JointVelocityWeightsA;

                GoalCosts[i] = jointVelDist.x + jointVelDist.y + jointVelDist.z + jointVelDist.w
                                + jointPosDist.x + jointPosDist.y + jointPosDist.z + jointPosDist.w;

                float3x3 jointPosDiffB = InputPoses[i].JointPositionsB - DesiredPose.JointPositionsB;

                float3 jointPosDistB = math.sqrt((jointPosDiffB.c0 * jointPosDiffB.c0) +
                                      (jointPosDiffB.c1 * jointPosDiffB.c1) +
                                      (jointPosDiffB.c2 * jointPosDiffB.c2)) * JointPositionWeightsB;

                jointVelDiff = InputPoses[i].JointVelocitiesB - DesiredPose.JointVelocitiesB;

                jointVelDist = math.sqrt((jointVelDiff.c0 * jointVelDiff.c0) +
                                      (jointVelDiff.c1 * jointVelDiff.c1) +
                                      (jointVelDiff.c2 * jointVelDiff.c2)) * JointVelocityWeightsB;

                GoalCosts[i] += jointVelDist.x + jointVelDist.y + jointVelDist.z + jointVelDist.w
                                + jointPosDistB.x + jointPosDistB.y + jointPosDistB.z;
            }
        }
    }//End of struct: ComputePose07Costs

    //============================================================================================
    /**
    *  @brief Job for computing pose cost for 8 or less joints
    *         
    *********************************************************************************************/
    [BurstCompile(CompileSynchronously = true)]
    public struct ComputePose08Costs : IJobParallelForBatch
    {
        //Animation Pose Database
        [ReadOnly]
        public NativeArray<Pose8> InputPoses;

        //Desired Goal Data
        [ReadOnly]
        public Pose8 DesiredPose;

        //Cost Multipliers
        [ReadOnly]
        public float4 JointVelocityWeightsA;

        [ReadOnly]
        public float4 JointVelocityWeightsB;

        [ReadOnly]
        public float4 JointPositionWeightsA;

        [ReadOnly]
        public float4 JointPositionWeightsB;

        [ReadOnly]
        public float BodyVelocityWeight;

        //Output
        [WriteOnly]
        public NativeArray<float> GoalCosts;

        public void Execute(int startIndex, int count)
        {
            for (int i = startIndex; i < startIndex + count; ++i)
            {

                //Calculate Cost of Pose Joint Positions
                float4x3 jointPosDiff = InputPoses[i].JointPositionsA - DesiredPose.JointPositionsA;

                float4 jointPosDist = math.sqrt((jointPosDiff.c0 * jointPosDiff.c0) +
                                      (jointPosDiff.c1 * jointPosDiff.c1) +
                                      (jointPosDiff.c2 * jointPosDiff.c2)) * JointPositionWeightsA;

                //Calculate Cost of Pose Velocity & Joints Velocity
                float4x3 jointVelDiff = InputPoses[i].JointVelocitiesA - DesiredPose.JointVelocitiesA;

                float4 jointVelDist = math.sqrt((jointVelDiff.c0 * jointVelDiff.c0) +
                                      (jointVelDiff.c1 * jointVelDiff.c1) +
                                      (jointVelDiff.c2 * jointVelDiff.c2)) * JointVelocityWeightsA;

                float3 bodyVel = (InputPoses[i].BodyVelocity - DesiredPose.BodyVelocity);
                float bodyVelDist = ((bodyVel.x * bodyVel.x) +
                                              (bodyVel.y * bodyVel.y) +
                                              (bodyVel.z * bodyVel.z)) * BodyVelocityWeight;

                float4 localCost = jointVelDist + jointPosDist;

                //Calculate Cost of Pose Joint Positions
                jointPosDiff = InputPoses[i].JointPositionsB - DesiredPose.JointPositionsB;

                jointPosDist = math.sqrt((jointPosDiff.c0 * jointPosDiff.c0) +
                                      (jointPosDiff.c1 * jointPosDiff.c1) +
                                      (jointPosDiff.c2 * jointPosDiff.c2)) * JointPositionWeightsB;

                //Calculate Cost of Pose Velocity & Joints Velocity
                jointVelDiff = InputPoses[i].JointVelocitiesB - DesiredPose.JointVelocitiesB;

                jointVelDist = math.sqrt((jointVelDiff.c0 * jointVelDiff.c0) +
                                      (jointVelDiff.c1 * jointVelDiff.c1) +
                                      (jointVelDiff.c2 * jointVelDiff.c2)) * JointVelocityWeightsB;

                localCost += jointVelDist + jointPosDist;

                GoalCosts[i] = localCost.x + localCost.y + localCost.z + localCost.w + bodyVelDist;
            }
        }
    }//End of struct: ComputePose08Job
}//End of namespace: MxM