using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace MxM
{
    //============================================================================================
    /**
    *  @brief Job for computing trajectory cost
    *         
    *********************************************************************************************/
    [BurstCompile(CompileSynchronously = true)]
    public struct ComputeTrajectory01Costs : IJobParallelForBatch
    {
        //Animation Pose Database
        [ReadOnly]
        public NativeArray<float4> InputTrajectories;

        //Desired Goal Data
        [ReadOnly]
        public float4 DesiredTrajectory;

        //Cost Multipliers
        [ReadOnly]
        public float TrajPosMultiplier;

        [ReadOnly]
        public float TrajFAngleMultiplier;

        //Output
        public NativeArray<float> GoalCosts;

        public void Execute(int startIndex, int count)
        {
            float oneEighty = 180f;
            float threeSixty = 360f;
            float ooThreeSixty = 1f / 360f;

            for (int i = startIndex; i < startIndex + count; ++i)
            {
                //Calculate cost of Trajectory positions
                float4 trajectoryDiff = InputTrajectories[i] - DesiredTrajectory;

                float trajectoryPointDist = math.sqrt((trajectoryDiff.x * trajectoryDiff.x) +
                                             (trajectoryDiff.y * trajectoryDiff.y) +
                                             (trajectoryDiff.z * trajectoryDiff.z)) * TrajPosMultiplier;

                //Calculate cost of trajectory facing angle
                float angleDiff = math.clamp(trajectoryDiff.w - math.floor(trajectoryDiff.w * ooThreeSixty) * threeSixty, 0f, threeSixty);
                float angleSub = math.select(0f, threeSixty, (angleDiff > oneEighty));

                angleDiff = angleDiff - angleSub;

                GoalCosts[i] = trajectoryPointDist + math.abs(angleDiff) * TrajFAngleMultiplier;
            }
        }
    }//End of class: ComputeTrajectory01Cost

    //============================================================================================
    /**
    *  @brief Job for computing trajectory cost
    *         
    *********************************************************************************************/
    [BurstCompile(CompileSynchronously = true)]
    public struct ComputeTrajectory02Costs : IJobParallelForBatch
    {
        //Animation Pose Database
        [ReadOnly]
        public NativeArray<float2x4> InputTrajectories;

        //Desired Goal Data
        [ReadOnly]
        public float2x4 DesiredTrajectory;

        //Cost Multipliers
        [ReadOnly]
        public float TrajPosMultiplier;

        [ReadOnly]
        public float TrajFAngleMultiplier;

        //Output
        public NativeArray<float> GoalCosts;

        public void Execute(int startIndex, int count)
        {
            float2 oneEighty = new float2(180f);
            float2 threeSixty = new float2(360f);
            float2 ooThreeSixty = new float2(1f / 360f);

            for (int i = startIndex; i < startIndex + count; ++i)
            {

                //Calculate cost of Trajectory positions
                float2x4 trajectoryDiff = InputTrajectories[i] - DesiredTrajectory;

                float2 trajectoryPointDist = math.sqrt((trajectoryDiff.c0 * trajectoryDiff.c0) +
                                             (trajectoryDiff.c1 * trajectoryDiff.c1) +
                                             (trajectoryDiff.c2 * trajectoryDiff.c2)) * TrajPosMultiplier;

                float2 localCost = trajectoryPointDist;

                //Calculate cost of trajectory facing angle
                float2 angleDiff = math.clamp(trajectoryDiff.c3 - math.floor(trajectoryDiff.c3 * ooThreeSixty) * threeSixty, float2.zero, threeSixty);
                float2 angleSub = math.select(float2.zero, threeSixty, (angleDiff > oneEighty));

                angleDiff = angleDiff - angleSub;
                localCost += math.abs(angleDiff) * TrajFAngleMultiplier;

                GoalCosts[i] = localCost.x + localCost.y;
            }
        }
    }//End of class: ComputeTrajectory02Cost

    //============================================================================================
    /**
    *  @brief Job for computing trajectory cost
    *         
    *********************************************************************************************/
    [BurstCompile(CompileSynchronously = true)]
    public struct ComputeTrajectory03Costs : IJobParallelForBatch
    {
        //Animation Pose Database
        [ReadOnly]
        public NativeArray<float3x4> InputTrajectories;       

        //Desired Goal Data
        [ReadOnly]
        public float3x4 DesiredTrajectory;

        //Cost Multipliers
        [ReadOnly]
        public float TrajPosMultiplier;

        [ReadOnly]
        public float TrajFAngleMultiplier;

        //Output
        public NativeArray<float> GoalCosts;


        public void Execute(int startIndex, int count)
        {
            float3 oneEighty = new float3(180f);
            float3 threeSixty = new float3(360f);
            float3 ooThreeSixty = new float3(1f / 360f);

            for (int i = startIndex; i < startIndex + count; ++i)
            {

                //Calculate cost of Trajectory positions
                float3x4 trajectoryDiff = InputTrajectories[i] - DesiredTrajectory;

                float3 trajectoryPointDist = math.sqrt((trajectoryDiff.c0 * trajectoryDiff.c0) +
                                             (trajectoryDiff.c1 * trajectoryDiff.c1) +
                                             (trajectoryDiff.c2 * trajectoryDiff.c2)) * TrajPosMultiplier;

                float3 localCost = trajectoryPointDist;

                //Calculate cost of trajectory facing angle
                float3 angleDiff = math.clamp(trajectoryDiff.c3 - math.floor(trajectoryDiff.c3 * ooThreeSixty) * threeSixty, float3.zero, threeSixty);
                float3 angleSub = math.select(float3.zero, threeSixty, (angleDiff > oneEighty));

                angleDiff = angleDiff - angleSub;
                localCost += math.abs(angleDiff) * TrajFAngleMultiplier;

                GoalCosts[i] = localCost.x + localCost.y + localCost.z;
            }
        }
    }//End of class: ComputeTrajectory03Cost


    //============================================================================================
    /**
    *  @brief Job for computing trajectory cost
    *         
    *********************************************************************************************/
    [BurstCompile(CompileSynchronously = true)]
    public struct ComputeTrajectory04Costs : IJobParallelForBatch
    {
        //Animation Pose Database
        [ReadOnly]
        public NativeArray<float4x4> InputTrajectories;

        //Desired Goal Data
        [ReadOnly]
        public float4x4 DesiredTrajectory;

        //Cost Multipliers
        [ReadOnly]
        public float TrajPosMultiplier;

        [ReadOnly]
        public float TrajFAngleMultiplier;

        //Output
        public NativeArray<float> GoalCosts;

        public void Execute(int startIndex, int count)
        {
            float4 oneEighty = new float4(180f);
            float4 threeSixty = new float4(360f);
            float4 ooThreeSixty = new float4(1f / 360f);

            for (int i = startIndex; i < startIndex + count; ++i)
            {
                //Calculate cost of Trajectory positions
                float4x4 trajectoryDiff = InputTrajectories[i] - DesiredTrajectory;

                float4 trajectoryPointDist = math.sqrt((trajectoryDiff.c0 * trajectoryDiff.c0) +
                                             (trajectoryDiff.c1 * trajectoryDiff.c1) +
                                             (trajectoryDiff.c2 * trajectoryDiff.c2));

                float4 localCost = trajectoryPointDist * TrajPosMultiplier;

                //Calculate cost of trajectory facing angle
                float4 angleDiff = math.clamp(trajectoryDiff.c3 - math.floor(trajectoryDiff.c3 * ooThreeSixty) * threeSixty, float4.zero, threeSixty);
                float4 angleSub = math.select(float4.zero, threeSixty, (angleDiff > oneEighty));

                angleDiff = angleDiff - angleSub;
                localCost += math.abs(angleDiff) * TrajFAngleMultiplier;

                GoalCosts[i] = localCost.x + localCost.y + localCost.z + localCost.w;
            }
        }
    }//End of class: ComputeTrajectory04Cost

    //============================================================================================
    /**
    *  @brief Job for computing trajectory cost
    *         
    *********************************************************************************************/
    [BurstCompile(CompileSynchronously = true)]
    public struct ComputeTrajectory05Costs : IJobParallelForBatch
    {
        //Animation Pose Database
        [ReadOnly]
        public NativeArray<Trajectory6> InputTrajectories;

        //Desired Goal Data
        [ReadOnly]
        public Trajectory6 DesiredTrajectory;

        //Cost Multipliers
        [ReadOnly]
        public float TrajPosMultiplier;

        [ReadOnly]
        public float TrajFAngleMultiplier;

        //Output
        public NativeArray<float> GoalCosts;

        public void Execute(int startIndex, int count)
        {
            float3 oneEighty = new float3(180f);
            float3 threeSixty = new float3(360f);
            float3 ooThreeSixty = new float3(1f / 360f);

            for (int i = startIndex; i < startIndex + count; ++i)
            {

                //First Half
                float3x4 trajectoryDiff = InputTrajectories[i].A - DesiredTrajectory.A;

                float3 trajectoryPointDist = math.sqrt((trajectoryDiff.c0 * trajectoryDiff.c0) +
                                             (trajectoryDiff.c1 * trajectoryDiff.c1) +
                                             (trajectoryDiff.c2 * trajectoryDiff.c2));

                float3 localCost = (trajectoryPointDist * TrajPosMultiplier);

                float3 angleDiff = math.clamp(trajectoryDiff.c3 - math.floor(trajectoryDiff.c3 * ooThreeSixty) * threeSixty, float3.zero, threeSixty);
                float3 angleSub = math.select(float3.zero, threeSixty, (angleDiff > oneEighty));

                angleDiff = angleDiff - angleSub;
                localCost += (math.abs(angleDiff) * TrajFAngleMultiplier);

                //Second Half
                trajectoryDiff = InputTrajectories[i].B - DesiredTrajectory.B;

                trajectoryPointDist = math.sqrt((trajectoryDiff.c0 * trajectoryDiff.c0) +
                                             (trajectoryDiff.c1 * trajectoryDiff.c1) +
                                             (trajectoryDiff.c2 * trajectoryDiff.c2));

                localCost += (trajectoryPointDist * new float3(TrajPosMultiplier, TrajPosMultiplier, 0f));

                angleDiff = math.clamp(trajectoryDiff.c3 - math.floor(trajectoryDiff.c3 * ooThreeSixty) * threeSixty, float3.zero, threeSixty);
                angleSub = math.select(float3.zero, threeSixty, (angleDiff > oneEighty));

                angleDiff = angleDiff - angleSub;
                localCost += (math.abs(angleDiff) * new float3(TrajFAngleMultiplier, TrajFAngleMultiplier, 0f));

                GoalCosts[i] = localCost.x + localCost.y + localCost.z;
            }
        }
    }//End of class: ComputeTrajectory05Cost

    //============================================================================================
    /**
    *  @brief Job for computing trajectory cost
    *         
    *********************************************************************************************/
    [BurstCompile(CompileSynchronously = true)]
    public struct ComputeTrajectory06Costs : IJobParallelForBatch
    {
        //Animation Pose Database
        [ReadOnly]
        public NativeArray<Trajectory6> InputTrajectories;

        //Desired Goal Data
        [ReadOnly]
        public Trajectory6 DesiredTrajectory;

        //Cost Multipliers
        [ReadOnly]
        public float TrajPosMultiplier;

        [ReadOnly]
        public float TrajFAngleMultiplier;

        //Output
        public NativeArray<float> GoalCosts;

        public void Execute(int startIndex, int count)
        {
            float3 oneEighty = new float3(180f);
            float3 threeSixty = new float3(360f);
            float3 ooThreeSixty = new float3(1f / 360f);

            for (int i = startIndex; i < startIndex + count; ++i)
            {

                //First Half
                float3x4 trajectoryDiff = InputTrajectories[i].A - DesiredTrajectory.A;

                float3 trajectoryPointDist = math.sqrt((trajectoryDiff.c0 * trajectoryDiff.c0) +
                                             (trajectoryDiff.c1 * trajectoryDiff.c1) +
                                             (trajectoryDiff.c2 * trajectoryDiff.c2));

                float3 localCost = (trajectoryPointDist * TrajPosMultiplier);

                float3 angleDiff = math.clamp(trajectoryDiff.c3 - math.floor(trajectoryDiff.c3 * ooThreeSixty) * threeSixty, float3.zero, threeSixty);
                float3 angleSub = math.select(float3.zero, threeSixty, (angleDiff > oneEighty));

                angleDiff = angleDiff - angleSub;
                localCost += (math.abs(angleDiff) * TrajFAngleMultiplier);

                //Second Half
                trajectoryDiff = InputTrajectories[i].B - DesiredTrajectory.B;

                trajectoryPointDist = math.sqrt((trajectoryDiff.c0 * trajectoryDiff.c0) +
                                             (trajectoryDiff.c1 * trajectoryDiff.c1) +
                                             (trajectoryDiff.c2 * trajectoryDiff.c2));

                localCost += (trajectoryPointDist * TrajPosMultiplier);

                angleDiff = math.clamp(trajectoryDiff.c3 - math.floor(trajectoryDiff.c3 * ooThreeSixty) * threeSixty, float3.zero, threeSixty);
                angleSub = math.select(float3.zero, threeSixty, (angleDiff > oneEighty));

                angleDiff = angleDiff - angleSub;
                localCost += (math.abs(angleDiff) * TrajFAngleMultiplier);

                GoalCosts[i] = localCost.x + localCost.y + localCost.z;
            }
        }
    }//End of class: ComputeTrajectory06Cost

    //============================================================================================
    /**
    *  @brief Job for computing trajectory cost
    *         
    *********************************************************************************************/
    [BurstCompile(CompileSynchronously = true)]
    public struct ComputeTrajectory07Costs : IJobParallelForBatch
    {
        //Animation Pose Database
        [ReadOnly]
        public NativeArray<Trajectory8> InputTrajectories;

        //Desired Goal Data
        [ReadOnly]
        public Trajectory8 DesiredTrajectory;

        //Cost Multipliers
        [ReadOnly]
        public float TrajPosMultiplier;

        [ReadOnly]
        public float TrajFAngleMultiplier;

        //Output
        public NativeArray<float> GoalCosts;

        public void Execute(int startIndex, int count)
        {
            float4 oneEighty = new float4(180f);
            float4 threeSixty = new float4(360f);
            float4 ooThreeSixty = new float4(1f / 360f);

            for (int i = startIndex; i < startIndex + count; ++i)
            {

                //First Half
                float4x4 trajectoryDiff = InputTrajectories[i].A - DesiredTrajectory.A;

                float4 trajectoryPointDist = math.sqrt((trajectoryDiff.c0 * trajectoryDiff.c0) +
                                             (trajectoryDiff.c1 * trajectoryDiff.c1) +
                                             (trajectoryDiff.c2 * trajectoryDiff.c2));

                float4 localCost = trajectoryPointDist * TrajPosMultiplier;

                float4 angleDiff = math.clamp(trajectoryDiff.c3 - math.floor(trajectoryDiff.c3 * ooThreeSixty) * threeSixty, float4.zero, threeSixty);
                float4 angleSub = math.select(float4.zero, threeSixty, (angleDiff > oneEighty));

                angleDiff = angleDiff - angleSub;
                localCost += math.abs(angleDiff) * TrajFAngleMultiplier;

                //Second Half
                trajectoryDiff = InputTrajectories[i].B - DesiredTrajectory.B;

                trajectoryPointDist = math.sqrt((trajectoryDiff.c0 * trajectoryDiff.c0) +
                                             (trajectoryDiff.c1 * trajectoryDiff.c1) +
                                             (trajectoryDiff.c2 * trajectoryDiff.c2));

                localCost += trajectoryPointDist * new float4(TrajPosMultiplier, TrajPosMultiplier, TrajPosMultiplier, 0f);

                angleDiff = math.clamp(trajectoryDiff.c3 - math.floor(trajectoryDiff.c3 * ooThreeSixty) * threeSixty, float4.zero, threeSixty);
                angleSub = math.select(float4.zero, threeSixty, (angleDiff > oneEighty));

                localCost += math.abs(angleDiff) * new float4(TrajFAngleMultiplier, TrajFAngleMultiplier, TrajFAngleMultiplier, 0f);

                GoalCosts[i] = localCost.x + localCost.y + localCost.z + localCost.w;
            }
        }
    }//End of class: ComputeTrajectory07Cost

    //============================================================================================
    /**
    *  @brief Job for computing trajectory cost
    *         
    *********************************************************************************************/
    [BurstCompile(CompileSynchronously = true)]
    public struct ComputeTrajectory08Costs : IJobParallelForBatch
    {
        //Animation Pose Database
        [ReadOnly]
        public NativeArray<Trajectory8> InputTrajectories;

        //Desired Goal Data
        [ReadOnly]
        public Trajectory8 DesiredTrajectory;

        //Cost Multipliers
        [ReadOnly]
        public float TrajPosMultiplier;

        [ReadOnly]
        public float TrajFAngleMultiplier;

        //Output
        public NativeArray<float> GoalCosts;

        public void Execute(int startIndex, int count)
        {
            float4 oneEighty = new float4(180f);
            float4 threeSixty = new float4(360f);
            float4 ooThreeSixty = new float4(1f / 360f);

            for (int i = startIndex; i < startIndex + count; ++i)
            {

                //First Half
                float4x4 trajectoryDiff = InputTrajectories[i].A - DesiredTrajectory.A;

                float4 trajectoryPointDist = math.sqrt((trajectoryDiff.c0 * trajectoryDiff.c0) +
                                             (trajectoryDiff.c1 * trajectoryDiff.c1) +
                                             (trajectoryDiff.c2 * trajectoryDiff.c2));

                float4 localCost = trajectoryPointDist * TrajPosMultiplier;

                float4 angleDiff = math.clamp(trajectoryDiff.c3 - math.floor(trajectoryDiff.c3 * ooThreeSixty) * threeSixty, float4.zero, threeSixty);
                float4 angleSub = math.select(float4.zero, threeSixty, (angleDiff > oneEighty));

                angleDiff = angleDiff - angleSub;
                localCost += math.abs(angleDiff) * TrajFAngleMultiplier;

                //Second Half
                trajectoryDiff = InputTrajectories[i].B - DesiredTrajectory.B;

                trajectoryPointDist = math.sqrt((trajectoryDiff.c0 * trajectoryDiff.c0) +
                                             (trajectoryDiff.c1 * trajectoryDiff.c1) +
                                             (trajectoryDiff.c2 * trajectoryDiff.c2));

                localCost += trajectoryPointDist * TrajPosMultiplier;

                angleDiff = math.clamp(trajectoryDiff.c3 - math.floor(trajectoryDiff.c3 * ooThreeSixty) * threeSixty, float4.zero, threeSixty);
                angleSub = math.select(float4.zero, threeSixty, (angleDiff > oneEighty));

                localCost += math.abs(angleDiff) * TrajFAngleMultiplier;

                GoalCosts[i] = localCost.x + localCost.y + localCost.z + localCost.w;
            }
        }
    }//End of class: ComputeTrajectory08Cost

    //============================================================================================
    /**
    *  @brief Job for computing trajectory cost
    *         
    *********************************************************************************************/
    [BurstCompile(CompileSynchronously = true)]
    public struct ComputeTrajectory09Costs : IJobParallelForBatch
    {
        //Animation Pose Database
        [ReadOnly]
        public NativeArray<Trajectory9> InputTrajectories;

        //Desired Goal Data
        [ReadOnly]
        public Trajectory9 DesiredTrajectory;

        //Cost Multipliers
        [ReadOnly]
        public float TrajPosMultiplier;

        [ReadOnly]
        public float TrajFAngleMultiplier;

        //Output
        public NativeArray<float> GoalCosts;

        public void Execute(int startIndex, int count)
        {
            float3 oneEighty = new float3(180f);
            float3 threeSixty = new float3(360f);
            float3 ooThreeSixty = new float3(1f / 360f);

            for (int i = startIndex; i < startIndex + count; ++i)
            {

                //First Part
                float3x4 trajectoryDiff = InputTrajectories[i].A - DesiredTrajectory.A;

                float3 trajectoryPointDist = math.sqrt((trajectoryDiff.c0 * trajectoryDiff.c0) +
                                             (trajectoryDiff.c1 * trajectoryDiff.c1) +
                                             (trajectoryDiff.c2 * trajectoryDiff.c2));

                float3 localCost = trajectoryPointDist * TrajPosMultiplier;

                float3 angleDiff = math.clamp(trajectoryDiff.c3 - math.floor(trajectoryDiff.c3 * ooThreeSixty) * threeSixty, float3.zero, threeSixty);
                float3 angleSub = math.select(float3.zero, threeSixty, (angleDiff > oneEighty));

                angleDiff = angleDiff - angleSub;
                localCost += math.abs(angleDiff) * TrajFAngleMultiplier;

                //Second Part
                trajectoryDiff = InputTrajectories[i].B - DesiredTrajectory.B;

                trajectoryPointDist = math.sqrt((trajectoryDiff.c0 * trajectoryDiff.c0) +
                                             (trajectoryDiff.c1 * trajectoryDiff.c1) +
                                             (trajectoryDiff.c2 * trajectoryDiff.c2));

                localCost += trajectoryPointDist * TrajPosMultiplier;

                angleDiff = math.clamp(trajectoryDiff.c3 - math.floor(trajectoryDiff.c3 * ooThreeSixty) * threeSixty, float3.zero, threeSixty);
                angleSub = math.select(float3.zero, threeSixty, (angleDiff > oneEighty));

                angleDiff = angleDiff - angleSub;
                localCost += math.abs(angleDiff) * TrajFAngleMultiplier;

                //Third Part
                trajectoryDiff = InputTrajectories[i].C - DesiredTrajectory.C;

                trajectoryPointDist = math.sqrt((trajectoryDiff.c0 * trajectoryDiff.c0) +
                                             (trajectoryDiff.c1 * trajectoryDiff.c1) +
                                             (trajectoryDiff.c2 * trajectoryDiff.c2));

                localCost += trajectoryPointDist * TrajPosMultiplier;

                angleDiff = math.clamp(trajectoryDiff.c3 - math.floor(trajectoryDiff.c3 * ooThreeSixty) * threeSixty, float3.zero, threeSixty);
                angleSub = math.select(float3.zero, threeSixty, (angleDiff > oneEighty));

                angleDiff = angleDiff - angleSub;
                localCost += math.abs(angleDiff) * TrajFAngleMultiplier;

                GoalCosts[i] = localCost.x + localCost.y + localCost.z;
            }
        }
    }//End of class: ComputeTrajectory09Cost

    //============================================================================================
    /**
    *  @brief Job for computing trajectory cost
    *         
    *********************************************************************************************/
    [BurstCompile(CompileSynchronously = true)]
    public struct ComputeTrajectory10Costs : IJobParallelForBatch
    {
        //Animation Pose Database
        [ReadOnly]
        public NativeArray<Trajectory12> InputTrajectories;

        //Desired Goal Data
        [ReadOnly]
        public Trajectory12 DesiredTrajectory;

        //Cost Multipliers
        [ReadOnly]
        public float TrajPosMultiplier;

        [ReadOnly]
        public float TrajFAngleMultiplier;

        //Output
        public NativeArray<float> GoalCosts;

        public void Execute(int startIndex, int count)
        {
            float4 oneEighty = new float4(180f);
            float4 threeSixty = new float4(360f);
            float4 ooThreeSixty = new float4(1f / 360f);

            for (int i = startIndex; i < startIndex + count; ++i)
            {
                //First Part
                float4x4 trajectoryDiff = InputTrajectories[i].A - DesiredTrajectory.A;

                float4 trajectoryPointDist = math.sqrt((trajectoryDiff.c0 * trajectoryDiff.c0) +
                                             (trajectoryDiff.c1 * trajectoryDiff.c1) +
                                             (trajectoryDiff.c2 * trajectoryDiff.c2));

                float4 localCost = trajectoryPointDist * TrajPosMultiplier;

                float4 angleDiff = math.clamp(trajectoryDiff.c3 - math.floor(trajectoryDiff.c3 * ooThreeSixty) * threeSixty, float4.zero, threeSixty);
                float4 angleSub = math.select(float4.zero, threeSixty, (angleDiff > oneEighty));

                angleDiff = angleDiff - angleSub;
                localCost += math.abs(angleDiff) * TrajFAngleMultiplier;

                //Second Part
                trajectoryDiff = InputTrajectories[i].B - DesiredTrajectory.B;

                trajectoryPointDist = math.sqrt((trajectoryDiff.c0 * trajectoryDiff.c0) +
                                             (trajectoryDiff.c1 * trajectoryDiff.c1) +
                                             (trajectoryDiff.c2 * trajectoryDiff.c2));

                float4 trajPosMultB = new float4(TrajPosMultiplier, TrajPosMultiplier, TrajPosMultiplier, 0f);
                float4 trajFAngleMultB = new float4(TrajFAngleMultiplier, TrajFAngleMultiplier, TrajFAngleMultiplier, 0f);

                localCost += trajectoryPointDist * trajPosMultB;

                angleDiff = math.clamp(trajectoryDiff.c3 - math.floor(trajectoryDiff.c3 * ooThreeSixty) * threeSixty, float4.zero, threeSixty);
                angleSub = math.select(float4.zero, threeSixty, (angleDiff > oneEighty));

                localCost += math.abs(angleDiff) * trajFAngleMultB;

                //Third Part
                trajectoryDiff = InputTrajectories[i].C - DesiredTrajectory.C;

                trajectoryPointDist = math.sqrt((trajectoryDiff.c0 * trajectoryDiff.c0) +
                                             (trajectoryDiff.c1 * trajectoryDiff.c1) +
                                             (trajectoryDiff.c2 * trajectoryDiff.c2));

                localCost += trajectoryPointDist * trajPosMultB;

                angleDiff = math.clamp(trajectoryDiff.c3 - math.floor(trajectoryDiff.c3 * ooThreeSixty) * threeSixty, float4.zero, threeSixty);
                angleSub = math.select(float4.zero, threeSixty, (angleDiff > oneEighty));

                localCost += math.abs(angleDiff) * trajFAngleMultB;

                GoalCosts[i] = localCost.x + localCost.y + localCost.z + localCost.w;
            }
        }
    }

    //============================================================================================
    /**
    *  @brief Job for computing trajectory cost
    *         
    *********************************************************************************************/
    [BurstCompile(CompileSynchronously = true)]
    public struct ComputeTrajectory11Costs : IJobParallelForBatch
    {
        //Animation Pose Database
        [ReadOnly]
        public NativeArray<Trajectory12> InputTrajectories;

        //Desired Goal Data
        [ReadOnly]
        public Trajectory12 DesiredTrajectory;

        //Cost Multipliers
        [ReadOnly]
        public float TrajPosMultiplier;

        [ReadOnly]
        public float TrajFAngleMultiplier;

        //Output
        public NativeArray<float> GoalCosts;

        public void Execute(int startIndex, int count)
        {
            float4 oneEighty = new float4(180f);
            float4 threeSixty = new float4(360f);
            float4 ooThreeSixty = new float4(1f / 360f);

            for (int i = startIndex; i < startIndex + count; ++i)
            {

                //First Part
                float4x4 trajectoryDiff = InputTrajectories[i].A - DesiredTrajectory.A;

                float4 trajectoryPointDist = math.sqrt((trajectoryDiff.c0 * trajectoryDiff.c0) +
                                             (trajectoryDiff.c1 * trajectoryDiff.c1) +
                                             (trajectoryDiff.c2 * trajectoryDiff.c2));

                float4 localCost = trajectoryPointDist * TrajPosMultiplier;

                float4 angleDiff = math.clamp(trajectoryDiff.c3 - math.floor(trajectoryDiff.c3 * ooThreeSixty) * threeSixty, float4.zero, threeSixty);
                float4 angleSub = math.select(float4.zero, threeSixty, (angleDiff > oneEighty));

                angleDiff = angleDiff - angleSub;
                localCost += math.abs(angleDiff) * TrajFAngleMultiplier;

                //Second Part
                trajectoryDiff = InputTrajectories[i].B - DesiredTrajectory.B;

                trajectoryPointDist = math.sqrt((trajectoryDiff.c0 * trajectoryDiff.c0) +
                                             (trajectoryDiff.c1 * trajectoryDiff.c1) +
                                             (trajectoryDiff.c2 * trajectoryDiff.c2));



                localCost += trajectoryPointDist * TrajPosMultiplier;

                angleDiff = math.clamp(trajectoryDiff.c3 - math.floor(trajectoryDiff.c3 * ooThreeSixty) * threeSixty, float4.zero, threeSixty);
                angleSub = math.select(float4.zero, threeSixty, (angleDiff > oneEighty));

                localCost += math.abs(angleDiff) * TrajFAngleMultiplier;

                //Third Part
                trajectoryDiff = InputTrajectories[i].C - DesiredTrajectory.C;

                trajectoryPointDist = math.sqrt((trajectoryDiff.c0 * trajectoryDiff.c0) +
                                             (trajectoryDiff.c1 * trajectoryDiff.c1) +
                                             (trajectoryDiff.c2 * trajectoryDiff.c2));

                localCost += trajectoryPointDist * new float4(TrajPosMultiplier, TrajPosMultiplier, TrajPosMultiplier, 0f);

                angleDiff = math.clamp(trajectoryDiff.c3 - math.floor(trajectoryDiff.c3 * ooThreeSixty) * threeSixty, float4.zero, threeSixty);
                angleSub = math.select(float4.zero, threeSixty, (angleDiff > oneEighty));

                localCost += math.abs(angleDiff) * new float4(TrajFAngleMultiplier, TrajFAngleMultiplier, TrajFAngleMultiplier, 0f);

                GoalCosts[i] = localCost.x + localCost.y + localCost.z + localCost.w;
            }
        }
    }
    //============================================================================================
    /**
    *  @brief Job for computing trajectory cost
    *         
    *********************************************************************************************/
    [BurstCompile(CompileSynchronously = true)]
    public struct ComputeTrajectory12Costs : IJobParallelForBatch
    {
        //Animation Pose Database
        [ReadOnly]
        public NativeArray<Trajectory12> InputTrajectories;

        //Desired Goal Data
        [ReadOnly]
        public Trajectory12 DesiredTrajectory;

        //Cost Multipliers
        [ReadOnly]
        public float TrajPosMultiplier;

        [ReadOnly]
        public float TrajFAngleMultiplier;

        //Output
        public NativeArray<float> GoalCosts;

        public void Execute(int startIndex, int count)
        {
            float4 oneEighty = new float4(180f);
            float4 threeSixty = new float4(360f);
            float4 ooThreeSixty = new float4(1f / 360f);

            for (int i = startIndex; i < startIndex + count; ++i)
            {

                //First Part
                float4x4 trajectoryDiff = InputTrajectories[i].A - DesiredTrajectory.A;

                float4 trajectoryPointDist = math.sqrt((trajectoryDiff.c0 * trajectoryDiff.c0) +
                                             (trajectoryDiff.c1 * trajectoryDiff.c1) +
                                             (trajectoryDiff.c2 * trajectoryDiff.c2));

                float4 localCost = trajectoryPointDist * TrajPosMultiplier;

                float4 angleDiff = math.clamp(trajectoryDiff.c3 - math.floor(trajectoryDiff.c3 * ooThreeSixty) * threeSixty, float4.zero, threeSixty);
                float4 angleSub = math.select(float4.zero, threeSixty, (angleDiff > oneEighty));

                angleDiff = angleDiff - angleSub;
                localCost += math.abs(angleDiff) * TrajFAngleMultiplier;

                //Second Part
                trajectoryDiff = InputTrajectories[i].B - DesiredTrajectory.B;

                trajectoryPointDist = math.sqrt((trajectoryDiff.c0 * trajectoryDiff.c0) +
                                             (trajectoryDiff.c1 * trajectoryDiff.c1) +
                                             (trajectoryDiff.c2 * trajectoryDiff.c2));

                localCost += trajectoryPointDist * TrajPosMultiplier;

                angleDiff = math.clamp(trajectoryDiff.c3 - math.floor(trajectoryDiff.c3 * ooThreeSixty) * threeSixty, float4.zero, threeSixty);
                angleSub = math.select(float4.zero, threeSixty, (angleDiff > oneEighty));

                localCost += math.abs(angleDiff) * TrajFAngleMultiplier;

                //Third Part
                trajectoryDiff = InputTrajectories[i].C - DesiredTrajectory.C;

                trajectoryPointDist = math.sqrt((trajectoryDiff.c0 * trajectoryDiff.c0) +
                                             (trajectoryDiff.c1 * trajectoryDiff.c1) +
                                             (trajectoryDiff.c2 * trajectoryDiff.c2));

                localCost += trajectoryPointDist * TrajPosMultiplier;

                angleDiff = math.clamp(trajectoryDiff.c3 - math.floor(trajectoryDiff.c3 * ooThreeSixty) * threeSixty, float4.zero, threeSixty);
                angleSub = math.select(float4.zero, threeSixty, (angleDiff > oneEighty));

                localCost += math.abs(angleDiff) * TrajFAngleMultiplier;

                GoalCosts[i] = localCost.x + localCost.y + localCost.z + localCost.w;
            }
        }
    }//End of class: ComputeTrajectory12Cost
}//End of namespace: MxM