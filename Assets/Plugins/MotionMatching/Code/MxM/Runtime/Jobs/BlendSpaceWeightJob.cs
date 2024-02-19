using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;

namespace MxM
{
    [BurstCompile(CompileSynchronously = true)]
    public struct CalculateBlendSpaceWeightingsJob : IJob
    {
        [ReadOnly]
        public float2 Position;

        [ReadOnly]
        public NativeArray<float2> ClipPositions;

        public NativeArray<float> ClipWeights;

        public void Execute()
        {
            float totalWeight = 0f;

            for(int i = 0; i < ClipPositions.Length; ++i)
            {
                float2 positionI = ClipPositions[i];
                float2 iToSample = Position - positionI;

                float weight = 1f;

                for(int k = 0; k < ClipPositions.Length; ++k)
                {
                    if (k == i)
                        continue;

                    float2 positionK = ClipPositions[k];
                    float2 iToK = positionK - positionI;

                    float lensq_ik = math.dot(iToK, iToK);
                    float newWeight = math.dot(iToSample, iToK) / lensq_ik;
                    newWeight = 1f - newWeight;
                    newWeight = math.clamp(newWeight, 0f, 1f);
                    weight = math.min(weight, newWeight);
                }

                ClipWeights[i] = weight;
                totalWeight += weight;
            }

            for(int i = 0; i < ClipWeights.Length; ++i)
            {
                ClipWeights[i] = ClipWeights[i] / totalWeight;
            }
        }

    }//End of class: BlendSpaceJob
}//End of namespace: MxM