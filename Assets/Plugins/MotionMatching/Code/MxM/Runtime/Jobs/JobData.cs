using Unity.Mathematics;

namespace MxM
{
    public struct Trajectory6
    {
        public float3x4 A;
        public float3x4 B;
    }

    public struct Trajectory8 //128 Bytes
    {
        public float4x4 A;
        public float4x4 B;
    }

    public struct Trajectory9
    {
        public float3x4 A;
        public float3x4 B;
        public float3x4 C;
    }

    public struct Trajectory12 //192 Bytes
    {
        public float4x4 A;
        public float4x4 B;
        public float4x4 C;
    }

    public struct Pose2
    {
        public float2x3 JointPositions;
        public float3x3 JointVelocities;
    }

    public struct Pose3 //84 Bytes
    {
        public float3x3 JointPositions;
        public float4x3 JointVelocities; //Contains body velocity as well
    }

    public struct Pose4 //108 Bytes
    {
        public float4x3 JointPositions;
        public float4x3 JointVelocities;
        public float3 BodyVelocity;
    }

    public struct Pose5
    {
        public float3x3 JointPositionsA;
        public float3x3 JointVelocitiesA;

        public float2x3 JointPositionsB;
        public float3x3 JointVelocitiesB;
    }

    public struct Pose6 //168 Bytes
    {
        public float3x3 JointPositionsA;
        public float3x3 JointVelocitiesA;

        public float3x3 JointPositionsB;
        public float3x3 JointVelocitiesB;

        public float3 BodyVelocity;
    }

    public struct Pose7
    {
        public float4x3 JointPositionsA;
        public float4x3 JointVelocitiesA;

        public float3x3 JointPositionsB;
        public float4x3 JointVelocitiesB;
    }

    public struct Pose8 //204 Bytes
    {
        public float4x3 JointPositionsA;
        public float4x3 JointVelocitiesA;
        public float3 BodyVelocity;

        public float4x3 JointPositionsB;
        public float4x3 JointVelocitiesB;
    }

}//End of namespace: MxM