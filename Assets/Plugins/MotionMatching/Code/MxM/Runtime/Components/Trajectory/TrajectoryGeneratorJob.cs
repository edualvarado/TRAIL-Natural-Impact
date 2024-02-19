// ================================================================================================
// File: TrajectoryGeneratorJob.cs
// 
// Authors:  Kenneth Claassen
// Date:     2019-11-05: Created this file.
// 
//     Contains a part of the 'MxM' namespace for 'Unity Engine'.
// ================================================================================================
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

namespace MxM
{
    [BurstCompile(CompileSynchronously = true)]
    public struct TrajectoryGeneratorJob : IJob
    {
        public NativeArray<float3> TrajectoryPositions;
        public NativeArray<float> TrajectoryRotations;

        public NativeArray<float3> NewTrajectoryPositions;

        [ReadOnly] public float CurrentRotation;
        [ReadOnly] public float3 DesiredLinearDisplacement;
        [ReadOnly] public float DesiredOrientation;
        [ReadOnly] public float MoveRate;
        [ReadOnly] public float TurnRate;

        public void Execute()
        {
            NewTrajectoryPositions[0] = float3.zero;
            TrajectoryRotations[0] = 0f;

            int iterations = TrajectoryPositions.Length;

            for (int i = 1; i < iterations; ++i)
            {
                float percentage = (float)i / (float)(iterations - 1);
                float3 trajectoryDisplacement = TrajectoryPositions[i] - TrajectoryPositions[i - 1];

                float3 adjustedTrajectoryDisplacement = math.lerp(trajectoryDisplacement, DesiredLinearDisplacement,
                    1f - math.exp(-MoveRate * percentage));

                NewTrajectoryPositions[i] = NewTrajectoryPositions[i - 1] + adjustedTrajectoryDisplacement;
                
                TrajectoryRotations[i] = math.degrees(LerpAngle(math.radians(TrajectoryRotations[i]), math.radians(DesiredOrientation),
                    1f - math.exp(-TurnRate * percentage)));
            }

            for (int i = 0; i < iterations; ++i)
            {
                TrajectoryPositions[i] = NewTrajectoryPositions[i];
            }

        }

        float LerpAngle(float a_angleA, float a_angleB, float a_t)
        {
            float max = math.PI * 2f;
            float da = (a_angleB - a_angleA) % max;

            return a_angleA + (2f * da % max - da) * a_t;
        }
    }//End of struct: TrajectoryGeneratorJob
}//End of namespace: MxM