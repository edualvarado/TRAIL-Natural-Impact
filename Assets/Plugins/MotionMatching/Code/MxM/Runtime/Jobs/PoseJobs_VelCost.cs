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
    public struct ComputePose01Costs_VelCost : IJobParallelForBatch
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

        [ReadOnly]
        public float ResultantVelocityWeight;

        [ReadOnly]
        public float PoseInterval;

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

                //Calculate resultsant pose
                float3 jointResultVel = jointDiff.c0 / PoseInterval;
                float3 jointResultVelDiff = InputPoses[i].c1 - jointResultVel;

                float jointResultVelDist = math.sqrt((jointResultVelDiff.x * jointResultVelDiff.x) +
                                      (jointResultVelDiff.y * jointResultVelDiff.y) +
                                      (jointResultVelDiff.z * jointResultVelDiff.z)) * (Weights.y * ResultantVelocityWeight);

                GoalCosts[i] = jointDist.x + jointDist.y + jointDist.z + jointResultVelDist;
            }
        }
    }//End of struct: ComputePose03Costs

    //============================================================================================
    /**
    *  @brief Job for computing pose cost for 2
    *         
    *********************************************************************************************/
    [BurstCompile(CompileSynchronously = true)]
    public struct ComputePose02Costs_VelCost : IJobParallelForBatch
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

        [ReadOnly]
        public float ResultantVelocityWeight;

        [ReadOnly]
        public float PoseInterval;

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

                float3x3 jointVelocities = InputPoses[i].JointVelocities;

                //Calculate Cost of Pose Velocity & Joints Velocity
                float3x3 jointVelDiff = jointVelocities - DesiredPose.JointVelocities;

                float3 jointVelDist = math.sqrt((jointVelDiff.c0 * jointVelDiff.c0) +
                                      (jointVelDiff.c1 * jointVelDiff.c1) +
                                      (jointVelDiff.c2 * jointVelDiff.c2)) * JointVelocityWeights;

                //Calculate resultsant pose
                float3x3 jointResultVel = new float3x3(new float3(jointPosDiff.c0, 0f),
                                                          new float3(jointPosDiff.c1, 0f),
                                                          new float3(jointPosDiff.c2, 0f)) / PoseInterval;

                float3x3 jointResultVelDiff = jointVelocities - jointResultVel;

                float3 jointResultVelDist = math.sqrt((jointResultVelDiff.c0 * jointResultVelDiff.c0) +
                                      (jointResultVelDiff.c1 * jointResultVelDiff.c1) +
                                      (jointResultVelDiff.c2 * jointResultVelDiff.c2)) * (JointVelocityWeights * ResultantVelocityWeight);

                //Calculate final total cost of the pose
                GoalCosts[i] = jointVelDist.x + jointVelDist.y + jointVelDist.z
                                + jointPosDist.x + jointPosDist.y
                                + jointResultVelDist.x + jointResultVelDist.y + jointResultVelDist.z;

            }
        }
    }//End of struct: ComputePose02Costs

    //============================================================================================
    /**
    *  @brief Job for computing pose cost for 3 or less joints
    *         
    *********************************************************************************************/
    [BurstCompile(CompileSynchronously = true)]
    public struct ComputePose03Costs_VelCost : IJobParallelForBatch
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

        [ReadOnly]
        public float ResultantVelocityWeight;

        [ReadOnly]
        public float PoseInterval;

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


                float4x3 jointVelocities = InputPoses[i].JointVelocities;

                //Calculate Cost of Pose Velocity & Joints Velocity
                float4x3 jointVelDiff = jointVelocities - DesiredPose.JointVelocities;

                float4 jointVelDist = math.sqrt((jointVelDiff.c0 * jointVelDiff.c0) +
                                      (jointVelDiff.c1 * jointVelDiff.c1) +
                                      (jointVelDiff.c2 * jointVelDiff.c2)) * JointVelocityWeights;


                //Calculate resultsant pose
                float4x3 jointResultVel = new float4x3(new float4(jointPosDiff.c0, 0f),
                                                          new float4(jointPosDiff.c1, 0f),
                                                          new float4(jointPosDiff.c2, 0f)) / PoseInterval;

                float4x3 jointResultVelDiff = jointVelocities - jointResultVel;

                float4 jointResultVelDist = math.sqrt((jointResultVelDiff.c0 * jointResultVelDiff.c0) +
                                      (jointResultVelDiff.c1 * jointResultVelDiff.c1) +
                                      (jointResultVelDiff.c2 * jointResultVelDiff.c2)) * (JointVelocityWeights * ResultantVelocityWeight);

                //Calculate final total cost of the pose
                GoalCosts[i] = jointVelDist.x + jointVelDist.y + jointVelDist.z + jointVelDist.w
                                + jointPosDist.x + jointPosDist.y + jointPosDist.z +
                                jointResultVelDist.x + jointResultVelDist.y + jointResultVelDist.z;
            }
        }
    }//End of struct: ComputePose03Costs

    //============================================================================================
    /**
    *  @brief Job for computing pose cost for 4 or less joints
    *         
    *********************************************************************************************/
    [BurstCompile(CompileSynchronously = true)]
    public struct ComputePose04Costs_VelCost : IJobParallelForBatch
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

        [ReadOnly]
        public float ResultantVelocityWeight;

        [ReadOnly]
        public float PoseInterval;

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

                float4x3 jointVelocities = InputPoses[i].JointVelocities;

                //Calculate Cost of Pose Velocity & Joints Velocity
                float4x3 jointVelDiff = jointVelocities - DesiredPose.JointVelocities;

                float4 jointVelDist = math.sqrt((jointVelDiff.c0 * jointVelDiff.c0) +
                                      (jointVelDiff.c1 * jointVelDiff.c1) +
                                      (jointVelDiff.c2 * jointVelDiff.c2)) * JointVelocityWeights;

                float3 bodyVel = (InputPoses[i].BodyVelocity - DesiredPose.BodyVelocity);
                float bodyVelDist = ((bodyVel.x * bodyVel.x) +
                                              (bodyVel.y * bodyVel.y) +
                                              (bodyVel.z * bodyVel.z)) * BodyVelocityWeight;

                //Calculate resultsant pose
                float4x3 jointResultVel = jointPosDiff / PoseInterval;
                float4x3 jointResultVelDiff = jointVelocities - jointResultVel;

                float4 jointResultVelDist = math.sqrt((jointResultVelDiff.c0 * jointResultVelDiff.c0) +
                                      (jointResultVelDiff.c1 * jointResultVelDiff.c1) +
                                      (jointResultVelDiff.c2 * jointResultVelDiff.c2)) * (JointVelocityWeights * ResultantVelocityWeight);


                GoalCosts[i] = jointVelDist.x + jointVelDist.y + jointVelDist.z + jointVelDist.w
                                + jointPosDist.x + jointPosDist.y + jointPosDist.z + bodyVelDist
                                + jointResultVelDist.x + jointResultVelDist.y + jointResultVelDist.z + jointResultVelDist.w;
            }
        }
    }//End of struct: ComputePose04Job

    //============================================================================================
    /**
    *  @brief Job for computing pose cost for 3 or less joints
    *         
    *********************************************************************************************/
    [BurstCompile(CompileSynchronously = true)]
    public struct ComputePose05Costs_VelCost : IJobParallelForBatch
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

        [ReadOnly]
        public float ResultantVelocityWeight;

        [ReadOnly]
        public float PoseInterval;

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

                float3x3 jointVelocitiesA = InputPoses[i].JointVelocitiesA;

                //Calculate Cost of Pose Velocity & Joints Velocity
                float3x3 jointVelDiff = jointVelocitiesA - DesiredPose.JointVelocitiesA;

                float3 jointVelDist = math.sqrt((jointVelDiff.c0 * jointVelDiff.c0) +
                                      (jointVelDiff.c1 * jointVelDiff.c1) +
                                      (jointVelDiff.c2 * jointVelDiff.c2)) * JointVelocityWeightsA;

                //Calculate resultsant pose A
                float3x3 jointResultVelA = jointPosDiff / PoseInterval;
                float3x3 jointResultVelDiffA = jointVelocitiesA - jointResultVelA;

                float3 jointResultVelDistA = math.sqrt((jointResultVelDiffA.c0 * jointResultVelDiffA.c0) +
                                      (jointResultVelDiffA.c1 * jointResultVelDiffA.c1) +
                                      (jointResultVelDiffA.c2 * jointResultVelDiffA.c2)) * (JointVelocityWeightsB * ResultantVelocityWeight);

                GoalCosts[i] = jointVelDist.x + jointVelDist.y + jointVelDist.z
                                + jointPosDist.x + jointPosDist.y + jointPosDist.z
                                + jointResultVelDistA.x + jointResultVelDistA.y + jointResultVelDistA.z;

                float2x3 jointPosDiffB = InputPoses[i].JointPositionsB - DesiredPose.JointPositionsB;

                float2 jointPosDistB = math.sqrt((jointPosDiffB.c0 * jointPosDiffB.c0) +
                                      (jointPosDiffB.c1 * jointPosDiffB.c1) +
                                      (jointPosDiffB.c2 * jointPosDiffB.c2)) * JointPositionWeightsB;

                float3x3 jointVelocitiesB = InputPoses[i].JointVelocitiesB;

                jointVelDiff = jointVelocitiesB - DesiredPose.JointVelocitiesB;

                jointVelDist = math.sqrt((jointVelDiff.c0 * jointVelDiff.c0) +
                                      (jointVelDiff.c1 * jointVelDiff.c1) +
                                      (jointVelDiff.c2 * jointVelDiff.c2)) * JointVelocityWeightsB;

                //Calculate resultant pose B
                float3x3 jointResultVelB = new float3x3(new float3(jointPosDiffB.c0, 0f),
                                                        new float3(jointPosDiffB.c1, 0f),
                                                        new float3(jointPosDiffB.c2, 0f));

                float3x3 jointResultVelDiffB = jointVelocitiesB - jointResultVelB;

                float3 jointResultVelDistB = math.sqrt((jointResultVelDiffB.c0 * jointResultVelDiffB.c0) +
                                      (jointResultVelDiffB.c1 * jointResultVelDiffB.c1) +
                                      (jointResultVelDiffB.c2 * jointResultVelDiffB.c2)) * (JointVelocityWeightsB * ResultantVelocityWeight);

                GoalCosts[i] += jointVelDist.x + jointVelDist.y + jointVelDist.z
                                + jointPosDistB.x + jointPosDistB.y
                                + jointResultVelDistB.x + jointResultVelDistB.y;
            }
        }
    }//End of struct: ComputePose06Costs

    //============================================================================================
    /**
    *  @brief Job for computing pose cost for 3 or less joints
    *         
    *********************************************************************************************/
    [BurstCompile(CompileSynchronously = true)]
    public struct ComputePose06Costs_VelCost : IJobParallelForBatch
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

        [ReadOnly]
        public float ResultantVelocityWeight;

        [ReadOnly]
        public float PoseInterval;

        public void Execute(int startIndex, int count)
        {
            for (int i = startIndex; i < startIndex + count; ++i)
            {

                //Calculate Cost of Pose Joint Positions
                float3x3 jointPosDiff = InputPoses[i].JointPositionsA - DesiredPose.JointPositionsA;

                float3 jointPosDist = math.sqrt((jointPosDiff.c0 * jointPosDiff.c0) +
                                      (jointPosDiff.c1 * jointPosDiff.c1) +
                                      (jointPosDiff.c2 * jointPosDiff.c2)) * JointPositionWeightsA;

                float3x3 jointVelocities = InputPoses[i].JointVelocitiesA;

                //Calculate Cost of Pose Velocity & Joints Velocity
                float3x3 jointVelDiff = jointVelocities - DesiredPose.JointVelocitiesA;

                float3 jointVelDist = math.sqrt((jointVelDiff.c0 * jointVelDiff.c0) +
                                      (jointVelDiff.c1 * jointVelDiff.c1) +
                                      (jointVelDiff.c2 * jointVelDiff.c2)) * JointVelocityWeightsA;

                float3 bodyVel = (InputPoses[i].BodyVelocity - DesiredPose.BodyVelocity);
                float bodyVelDist = ((bodyVel.x * bodyVel.x) +
                                              (bodyVel.y * bodyVel.y) +
                                              (bodyVel.z * bodyVel.z)) * BodyVelocityWeight;

                //Calculate resultant pose A
                float3x3 jointResultVel = jointPosDiff / PoseInterval;
                float3x3 jointResultVelDiff = jointVelocities - jointResultVel;

                float3 jointResultVelDist = math.sqrt((jointResultVelDiff.c0 * jointResultVelDiff.c0) +
                                      (jointResultVelDiff.c1 * jointResultVelDiff.c1) +
                                      (jointResultVelDiff.c2 * jointResultVelDiff.c2)) * (JointVelocityWeightsB * ResultantVelocityWeight);

                GoalCosts[i] = jointVelDist.x + jointVelDist.y + jointVelDist.z
                                + jointPosDist.x + jointPosDist.y + jointPosDist.z
                                + jointResultVelDist.x + jointResultVelDist.y + jointResultVelDist.z +
                                +bodyVelDist;

                //Calcualte positions B
                jointVelocities = InputPoses[i].JointPositionsB;

                jointPosDiff = jointVelocities - DesiredPose.JointPositionsB;

                jointPosDist = math.sqrt((jointPosDiff.c0 * jointPosDiff.c0) +
                                      (jointPosDiff.c1 * jointPosDiff.c1) +
                                      (jointPosDiff.c2 * jointPosDiff.c2)) * JointPositionWeightsB;

                //Calculate velocities B
                jointVelDiff = jointVelocities - DesiredPose.JointVelocitiesB;

                jointVelDist = math.sqrt((jointVelDiff.c0 * jointVelDiff.c0) +
                                      (jointVelDiff.c1 * jointVelDiff.c1) +
                                      (jointVelDiff.c2 * jointVelDiff.c2)) * JointVelocityWeightsB;

                //Calculate resultsant pose B
                jointResultVel = jointPosDiff / PoseInterval;
                jointResultVelDiff = jointVelocities - jointResultVel;

                jointResultVelDist = math.sqrt((jointResultVelDiff.c0 * jointResultVelDiff.c0) +
                                      (jointResultVelDiff.c1 * jointResultVelDiff.c1) +
                                      (jointResultVelDiff.c2 * jointResultVelDiff.c2)) * (JointVelocityWeightsB * ResultantVelocityWeight);

                GoalCosts[i] += jointVelDist.x + jointVelDist.y + jointVelDist.z
                                + jointPosDist.x + jointPosDist.y + jointPosDist.z
                                + jointResultVelDist.x + jointResultVelDist.y + jointResultVelDist.z;
            }
        }
    }//End of struct: ComputePose06Costs

    //============================================================================================
    /**
    *  @brief Job for computing pose cost for 3 or less joints
    *         
    *********************************************************************************************/
    [BurstCompile(CompileSynchronously = true)]
    public struct ComputePose07Costs_VelCost : IJobParallelForBatch
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

        [ReadOnly]
        public float ResultantVelocityWeight;

        [ReadOnly]
        public float PoseInterval;

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

                float4x3 jointVelocities = InputPoses[i].JointVelocitiesA;

                //Calculate Cost of Pose Velocity & Joints Velocity
                float4x3 jointVelDiff = jointVelocities - DesiredPose.JointVelocitiesA;

                float4 jointVelDist = math.sqrt((jointVelDiff.c0 * jointVelDiff.c0) +
                                      (jointVelDiff.c1 * jointVelDiff.c1) +
                                      (jointVelDiff.c2 * jointVelDiff.c2)) * JointVelocityWeightsA;

                //Calculate resultant pose
                float4x3 jointResultVelA = jointPosDiff / PoseInterval;
                float4x3 jointResultVelDiffA = jointVelocities - jointResultVelA;

                float4 jointResultVelDistA = math.sqrt((jointResultVelDiffA.c0 * jointResultVelDiffA.c0) +
                                      (jointResultVelDiffA.c1 * jointResultVelDiffA.c1) +
                                      (jointResultVelDiffA.c2 * jointResultVelDiffA.c2)) * (JointVelocityWeightsA * ResultantVelocityWeight);

                GoalCosts[i] = jointVelDist.x + jointVelDist.y + jointVelDist.z + jointVelDist.w
                                + jointPosDist.x + jointPosDist.y + jointPosDist.z + jointPosDist.w
                                + jointResultVelDistA.x + jointResultVelDistA.y + jointResultVelDistA.z + jointResultVelDistA.w;

                float3x3 jointPosDiffB = InputPoses[i].JointPositionsB - DesiredPose.JointPositionsB;

                float3 jointPosDistB = math.sqrt((jointPosDiffB.c0 * jointPosDiffB.c0) +
                                      (jointPosDiffB.c1 * jointPosDiffB.c1) +
                                      (jointPosDiffB.c2 * jointPosDiffB.c2)) * JointPositionWeightsB;

                jointVelocities = InputPoses[i].JointVelocitiesB;

                jointVelDiff = jointVelocities - DesiredPose.JointVelocitiesB;

                jointVelDist = math.sqrt((jointVelDiff.c0 * jointVelDiff.c0) +
                                      (jointVelDiff.c1 * jointVelDiff.c1) +
                                      (jointVelDiff.c2 * jointVelDiff.c2)) * JointVelocityWeightsB;

                //Calculate resultant poses
                float4x3 jointResultVel = new float4x3(new float4(jointPosDiffB.c0, 0f),
                                                          new float4(jointPosDiffB.c1, 0f),
                                                          new float4(jointPosDiffB.c2, 0f)) / PoseInterval;

                float4x3 jointResultVelDiff = jointVelocities - jointResultVel;

                float4 jointResultVelDist = math.sqrt((jointResultVelDiff.c0 * jointResultVelDiff.c0) +
                                      (jointResultVelDiff.c1 * jointResultVelDiff.c1) +
                                      (jointResultVelDiff.c2 * jointResultVelDiff.c2)) * (JointVelocityWeightsB * ResultantVelocityWeight);


                GoalCosts[i] += jointVelDist.x + jointVelDist.y + jointVelDist.z + jointVelDist.w
                                + jointPosDistB.x + jointPosDistB.y + jointPosDistB.z
                                + jointResultVelDist.x + jointResultVelDist.y + jointResultVelDist.z;

            }
        }
    }//End of struct: ComputePose07Costs

    //============================================================================================
    /**
    *  @brief Job for computing pose cost for 8 or less joints
    *         
    *********************************************************************************************/
    [BurstCompile(CompileSynchronously = true)]
    public struct ComputePose08Costs_VelCost : IJobParallelForBatch
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

        [ReadOnly]
        public float ResultantVelocityWeight;

        [ReadOnly]
        public float PoseInterval;

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

                float4x3 jointVelocities = InputPoses[i].JointVelocitiesA;

                //Calculate Cost of Pose Velocity & Joints Velocity
                float4x3 jointVelDiff = jointVelocities - DesiredPose.JointVelocitiesA;

                float4 jointVelDist = math.sqrt((jointVelDiff.c0 * jointVelDiff.c0) +
                                      (jointVelDiff.c1 * jointVelDiff.c1) +
                                      (jointVelDiff.c2 * jointVelDiff.c2)) * JointVelocityWeightsA;

                float3 bodyVel = (InputPoses[i].BodyVelocity - DesiredPose.BodyVelocity);
                float bodyVelDist = ((bodyVel.x * bodyVel.x) +
                                              (bodyVel.y * bodyVel.y) +
                                              (bodyVel.z * bodyVel.z)) * BodyVelocityWeight;

                //Calculate resultant poses
                float4x3 jointResultVel = jointPosDiff / PoseInterval;
                float4x3 jointResultVelDiff = jointVelocities - jointResultVel;

                float4 jointResultVelDist = math.sqrt((jointResultVelDiff.c0 * jointResultVelDiff.c0) +
                                      (jointResultVelDiff.c1 * jointResultVelDiff.c1) +
                                      (jointResultVelDiff.c2 * jointResultVelDiff.c2)) * (JointVelocityWeightsA * ResultantVelocityWeight);

                float4 localCost = jointVelDist + jointPosDist + jointResultVelDist;

                //Calculate Cost of Pose Joint Positions
                jointPosDiff = InputPoses[i].JointPositionsB - DesiredPose.JointPositionsB;

                jointPosDist = math.sqrt((jointPosDiff.c0 * jointPosDiff.c0) +
                                      (jointPosDiff.c1 * jointPosDiff.c1) +
                                      (jointPosDiff.c2 * jointPosDiff.c2)) * JointPositionWeightsB;

                jointVelocities = InputPoses[i].JointPositionsB;

                //Calculate Cost of Pose Velocity & Joints Velocity
                jointVelDiff = jointVelocities - DesiredPose.JointVelocitiesB;

                jointVelDist = math.sqrt((jointVelDiff.c0 * jointVelDiff.c0) +
                                      (jointVelDiff.c1 * jointVelDiff.c1) +
                                      (jointVelDiff.c2 * jointVelDiff.c2)) * JointVelocityWeightsB;

                //Calculate resultant poses
                jointResultVel = jointPosDiff / PoseInterval;
                jointResultVelDiff = jointVelocities - jointResultVel;

                jointResultVelDist = math.sqrt((jointResultVelDiff.c0 * jointResultVelDiff.c0) +
                                      (jointResultVelDiff.c1 * jointResultVelDiff.c1) +
                                      (jointResultVelDiff.c2 * jointResultVelDiff.c2)) * (JointVelocityWeightsA * ResultantVelocityWeight);


                localCost += jointVelDist + jointPosDist + jointResultVelDist;

                GoalCosts[i] = localCost.x + localCost.y + localCost.z + localCost.w + bodyVelDist;
            }
        }
    }//End of struct: ComputePose08Job_VelCost
}//End of namespace: MxM