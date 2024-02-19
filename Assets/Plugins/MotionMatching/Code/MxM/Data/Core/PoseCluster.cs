using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace MxM
{
    [System.Serializable]
    public class PoseClusterCollection
    {
        public ETags Tags;
        public PoseCluster[] Clusters;

        public void Initialize(MxMAnimData a_animData)
        {
            if (a_animData == null)
                return;

            foreach(PoseCluster cluster in Clusters)
            {
                cluster.Initialize(a_animData);
            }
        }

        public void DisposeNativeData()
        {
            foreach (PoseCluster cluster in Clusters)
            {
                cluster.DisposeNativeData();
            }
        }
    }//End of class: PoseClusterCollection


    [System.Serializable]
    public class PoseCluster
    {
        public int[] ClusterPoseIds;

        [System.NonSerialized] public NativeArray<float4> TrajectoriesPacked_01;
        [System.NonSerialized] public NativeArray<float2x4> TrajectoriesPacked_02;
        [System.NonSerialized] public NativeArray<float3x4> TrajectoriesPacked_03;
        [System.NonSerialized] public NativeArray<float4x4> TrajectoriesPacked_04;
        [System.NonSerialized] public NativeArray<Trajectory6> TrajectoriesPacked_06;
        [System.NonSerialized] public NativeArray<Trajectory8> TrajectoriesPacked_08;
        [System.NonSerialized] public NativeArray<Trajectory9> TrajectoriesPacked_09;
        [System.NonSerialized] public NativeArray<Trajectory12> TrajectoriesPacked_12;
        [System.NonSerialized] public NativeArray<float3x3> PosesPacked_01;
        [System.NonSerialized] public NativeArray<Pose2> PosesPacked_02;
        [System.NonSerialized] public NativeArray<Pose3> PosesPacked_03;
        [System.NonSerialized] public NativeArray<Pose4> PosesPacked_04;
        [System.NonSerialized] public NativeArray<Pose5> PosesPacked_05;
        [System.NonSerialized] public NativeArray<Pose6> PosesPacked_06;
        [System.NonSerialized] public NativeArray<Pose7> PosesPacked_07;
        [System.NonSerialized] public NativeArray<Pose8> PosesPacked_08;

        [System.NonSerialized] public NativeArray<ETags> FavourTagsPacked;
        [System.NonSerialized] public NativeArray<int> ClipIdsPacked;
        [System.NonSerialized] public NativeArray<float> FavourPacked;

        [System.NonSerialized] public int[] UsedPoseIds;
        [System.NonSerialized] public Dictionary<int, int> UsedPoseIdMap;

        public int MinimaBatchCount { get; private set; }

        private const int k_minimaBatchSize = 64;

        public void Initialize(MxMAnimData a_animData)
        {
            int usedPoseCount = ClusterPoseIds.Length;

            MinimaBatchCount = (usedPoseCount + k_minimaBatchSize - 1) / k_minimaBatchSize;

            UsedPoseIds = new int[usedPoseCount];
            UsedPoseIdMap = new Dictionary<int, int>();

            FavourTagsPacked = new NativeArray<ETags>(usedPoseCount, Allocator.Persistent);
            ClipIdsPacked = new NativeArray<int>(usedPoseCount, Allocator.Persistent);
            FavourPacked = new NativeArray<float>(usedPoseCount, Allocator.Persistent);

            switch (a_animData.PosePredictionTimes.Length)
            {
                case 1: { GenerateTrajectory1(a_animData); } break;
                case 2: { GenerateTrajectory2( a_animData); } break;
                case 3: { GenerateTrajectory3(a_animData); } break;
                case 4: { GenerateTrajectory4(a_animData); } break;
                case 5: { GenerateTrajectory5(a_animData); } break;
                case 6: { GenerateTrajectory6(a_animData); } break;
                case 7: { GenerateTrajectory7(a_animData); } break;
                case 8: { GenerateTrajectory8(a_animData); } break;
                case 9: { GenerateTrajectory9(a_animData); } break;
                case 10: { GenerateTrajectory10(a_animData); } break;
                case 11: { GenerateTrajectory11(a_animData); } break;
                case 12: { GenerateTrajectory12(a_animData); } break;
                default:
                    {
                        Debug.LogError("MxM: Unsoppoorted trajectory length. MxM supports 1 - 12 trajectory points.");
                    }
                    break;
            }

            switch (a_animData.MatchBones.Length)
            {
                case 1: { GeneratePose1(a_animData); } break;
                case 2: { GeneratePose2(a_animData); } break;
                case 3: { GeneratePose3(a_animData); } break;
                case 4: { GeneratePose4(a_animData); } break;
                case 5: { GeneratePose5(a_animData); } break;
                case 6: { GeneratePose6(a_animData); } break;
                case 7: { GeneratePose7(a_animData); } break;
                case 8: { GeneratePose8(a_animData); } break;
                default:
                    {
                        Debug.LogError("MxM: Unsoppoorted pose configuration. MxM supports 1 - 8 pose joint matches");
                    }
                    break;
            }
        }

        public void DisposeNativeData()
        {
            if (TrajectoriesPacked_01.IsCreated)
                TrajectoriesPacked_01.Dispose();

            if (TrajectoriesPacked_02.IsCreated)
                TrajectoriesPacked_02.Dispose();

            if (TrajectoriesPacked_03.IsCreated)
                TrajectoriesPacked_03.Dispose();

            if (TrajectoriesPacked_04.IsCreated)
                TrajectoriesPacked_04.Dispose();

            if (TrajectoriesPacked_06.IsCreated)
                TrajectoriesPacked_06.Dispose();

            if (TrajectoriesPacked_08.IsCreated)
                TrajectoriesPacked_08.Dispose();

            if (TrajectoriesPacked_09.IsCreated)
                TrajectoriesPacked_09.Dispose();

            if (TrajectoriesPacked_12.IsCreated)
                TrajectoriesPacked_12.Dispose();

            if (PosesPacked_01.IsCreated)
                PosesPacked_01.Dispose();

            if (PosesPacked_02.IsCreated)
                PosesPacked_02.Dispose();

            if (PosesPacked_03.IsCreated)
                PosesPacked_03.Dispose();

            if (PosesPacked_04.IsCreated)
                PosesPacked_04.Dispose();

            if (PosesPacked_05.IsCreated)
                PosesPacked_05.Dispose();

            if (PosesPacked_06.IsCreated)
                PosesPacked_06.Dispose();

            if (PosesPacked_07.IsCreated)
                PosesPacked_07.Dispose();

            if (PosesPacked_08.IsCreated)
                PosesPacked_08.Dispose();

            if (FavourTagsPacked.IsCreated)
                FavourTagsPacked.Dispose();

            if (ClipIdsPacked.IsCreated)
                ClipIdsPacked.Dispose();

            if (FavourPacked.IsCreated)
                FavourPacked.Dispose();
        }

        private void GenerateTrajectory1(MxMAnimData a_animData)
        {
            int usedPoseCount = ClusterPoseIds.Length;

            TrajectoriesPacked_01 = new NativeArray<float4>(usedPoseCount, Allocator.Persistent);

            for(int i = 0; i < ClusterPoseIds.Length; ++i)
            {
                ref readonly PoseData pose = ref a_animData.Poses[ClusterPoseIds[i]];

                //Pose Mask?

                UsedPoseIds[i] = pose.PoseId;
                UsedPoseIdMap[pose.PoseId] = i;

                ref TrajectoryPoint trajPoint = ref pose.Trajectory[0];

                TrajectoriesPacked_01[i] = new float4(trajPoint.Position, trajPoint.FacingAngle);

            }
        }

        private void GenerateTrajectory2(MxMAnimData a_animData)
        {
            int usedPoseCount = ClusterPoseIds.Length;

            TrajectoriesPacked_02 = new NativeArray<float2x4>(usedPoseCount, Allocator.Persistent);

            for (int i = 0; i < ClusterPoseIds.Length; ++i)
            {
                ref readonly PoseData pose = ref a_animData.Poses[i];

                UsedPoseIds[i] = pose.PoseId;
                UsedPoseIdMap[pose.PoseId] = i;

                float2x4 packedTrajectory = new float2x4();
                TrajectoryPoint[] trajectoryPoints = pose.Trajectory;

                packedTrajectory.c0 = new float2(trajectoryPoints[0].Position.x, trajectoryPoints[1].Position.x);
                packedTrajectory.c1 = new float2(trajectoryPoints[0].Position.y, trajectoryPoints[1].Position.y);
                packedTrajectory.c2 = new float2(trajectoryPoints[0].Position.z, trajectoryPoints[1].Position.z);
                packedTrajectory.c3 = new float2(trajectoryPoints[0].FacingAngle, trajectoryPoints[1].FacingAngle);

                TrajectoriesPacked_02[i] = packedTrajectory;

            }
        }

        private void GenerateTrajectory3(MxMAnimData a_animData)
        {
            int usedPoseCount = ClusterPoseIds.Length;

            TrajectoriesPacked_03 = new NativeArray<float3x4>(usedPoseCount, Allocator.Persistent);

            for (int i = 0; i < ClusterPoseIds.Length; ++i)
            {
                ref readonly PoseData pose = ref a_animData.Poses[i];

                UsedPoseIds[i] = pose.PoseId;
                UsedPoseIdMap[pose.PoseId] = i;

                float3x4 packedTrajectory = new float3x4();

                TrajectoryPoint[] trajectoryPoints = pose.Trajectory;

                packedTrajectory.c0 = new float3(trajectoryPoints[0].Position.x, trajectoryPoints[1].Position.x,
                                                 trajectoryPoints[2].Position.x);

                packedTrajectory.c1 = new float3(trajectoryPoints[0].Position.y, trajectoryPoints[1].Position.y,
                                                 trajectoryPoints[2].Position.y);

                packedTrajectory.c2 = new float3(trajectoryPoints[0].Position.z, trajectoryPoints[1].Position.z,
                                                 trajectoryPoints[2].Position.z);

                packedTrajectory.c3 = new float3(trajectoryPoints[0].FacingAngle, trajectoryPoints[1].FacingAngle,
                                                 trajectoryPoints[2].FacingAngle);


                TrajectoriesPacked_03[i] = packedTrajectory;
            }
        }

        private void GenerateTrajectory4(MxMAnimData a_animData)
        {
            int usedPoseCount = ClusterPoseIds.Length;

            TrajectoriesPacked_04 = new NativeArray<float4x4>(usedPoseCount, Allocator.Persistent);

            for (int i = 0; i < ClusterPoseIds.Length; ++i)
            {
                ref readonly PoseData pose = ref a_animData.Poses[i];

                UsedPoseIds[i] = pose.PoseId;
                UsedPoseIdMap[pose.PoseId] = i;

                float4x4 packedTrajectory = new float4x4();
                TrajectoryPoint[] trajectoryPoints = pose.Trajectory;

                packedTrajectory.c0 = new float4(trajectoryPoints[0].Position.x, trajectoryPoints[1].Position.x,
                                                 trajectoryPoints[2].Position.x, trajectoryPoints[3].Position.x);

                packedTrajectory.c1 = new float4(trajectoryPoints[0].Position.y, trajectoryPoints[1].Position.y,
                                                 trajectoryPoints[2].Position.y, trajectoryPoints[3].Position.y);

                packedTrajectory.c2 = new float4(trajectoryPoints[0].Position.z, trajectoryPoints[1].Position.z,
                                                 trajectoryPoints[2].Position.z, trajectoryPoints[3].Position.z);

                packedTrajectory.c3 = new float4(trajectoryPoints[0].FacingAngle, trajectoryPoints[1].FacingAngle,
                                                 trajectoryPoints[2].FacingAngle, trajectoryPoints[3].FacingAngle);


                TrajectoriesPacked_04[i] = packedTrajectory;
            }
        }

        private void GenerateTrajectory5(MxMAnimData a_animData)
        {
            int usedPoseCount = ClusterPoseIds.Length;

            TrajectoriesPacked_06 = new NativeArray<Trajectory6>(usedPoseCount, Allocator.Persistent);

            for (int i = 0; i < ClusterPoseIds.Length; ++i)
            {
                ref readonly PoseData pose = ref a_animData.Poses[i];

                UsedPoseIds[i] = pose.PoseId;
                UsedPoseIdMap[pose.PoseId] = i;

                Trajectory6 packedTrajectory = new Trajectory6();
                packedTrajectory.A = new float3x4();
                packedTrajectory.B = new float3x4();

                TrajectoryPoint[] trajectoryPoints = pose.Trajectory;

                packedTrajectory.A.c0 = new float3(trajectoryPoints[0].Position.x, trajectoryPoints[1].Position.x,
                                                 trajectoryPoints[2].Position.x);

                packedTrajectory.A.c1 = new float3(trajectoryPoints[0].Position.y, trajectoryPoints[1].Position.y,
                                                 trajectoryPoints[2].Position.y);

                packedTrajectory.A.c2 = new float3(trajectoryPoints[0].Position.z, trajectoryPoints[1].Position.z,
                                                 trajectoryPoints[2].Position.z);

                packedTrajectory.A.c3 = new float3(trajectoryPoints[0].FacingAngle, trajectoryPoints[1].FacingAngle,
                                                 trajectoryPoints[2].FacingAngle);

                packedTrajectory.B.c0 = new float3(trajectoryPoints[3].Position.x, trajectoryPoints[4].Position.x, 0f);

                packedTrajectory.B.c1 = new float3(trajectoryPoints[3].Position.y, trajectoryPoints[4].Position.y, 0f);

                packedTrajectory.B.c2 = new float3(trajectoryPoints[3].Position.z, trajectoryPoints[4].Position.z, 0f);

                packedTrajectory.B.c3 = new float3(trajectoryPoints[3].FacingAngle, trajectoryPoints[4].FacingAngle, 0f);

                TrajectoriesPacked_06[i] = packedTrajectory;
            }
        }

        private void GenerateTrajectory6(MxMAnimData a_animData)
        {
            int usedPoseCount = ClusterPoseIds.Length;

            TrajectoriesPacked_06 = new NativeArray<Trajectory6>(usedPoseCount, Allocator.Persistent);

            for (int i = 0; i < ClusterPoseIds.Length; ++i)
            {
                ref readonly PoseData pose = ref a_animData.Poses[i];

                UsedPoseIds[i] = pose.PoseId;
                UsedPoseIdMap[pose.PoseId] = i;

                Trajectory6 packedTrajectory = new Trajectory6();
                packedTrajectory.A = new float3x4();
                packedTrajectory.B = new float3x4();

                TrajectoryPoint[] trajectoryPoints = pose.Trajectory;

                packedTrajectory.A.c0 = new float3(trajectoryPoints[0].Position.x, trajectoryPoints[1].Position.x,
                                                 trajectoryPoints[2].Position.x);

                packedTrajectory.A.c1 = new float3(trajectoryPoints[0].Position.y, trajectoryPoints[1].Position.y,
                                                 trajectoryPoints[2].Position.y);

                packedTrajectory.A.c2 = new float3(trajectoryPoints[0].Position.z, trajectoryPoints[1].Position.z,
                                                 trajectoryPoints[2].Position.z);

                packedTrajectory.A.c3 = new float3(trajectoryPoints[0].FacingAngle, trajectoryPoints[1].FacingAngle,
                                                 trajectoryPoints[2].FacingAngle);

                packedTrajectory.B.c0 = new float3(trajectoryPoints[3].Position.x, trajectoryPoints[4].Position.x,
                                                 trajectoryPoints[5].Position.x);

                packedTrajectory.B.c1 = new float3(trajectoryPoints[3].Position.y, trajectoryPoints[4].Position.y,
                                                 trajectoryPoints[5].Position.y);

                packedTrajectory.B.c2 = new float3(trajectoryPoints[3].Position.z, trajectoryPoints[4].Position.z,
                                                 trajectoryPoints[5].Position.z);

                packedTrajectory.B.c3 = new float3(trajectoryPoints[3].FacingAngle, trajectoryPoints[4].FacingAngle,
                                                 trajectoryPoints[5].FacingAngle);

                TrajectoriesPacked_06[i] = packedTrajectory;
            }
        }

        private void GenerateTrajectory7(MxMAnimData a_animData)
        {
            int usedPoseCount = ClusterPoseIds.Length;

            TrajectoriesPacked_08 = new NativeArray<Trajectory8>(usedPoseCount, Allocator.Persistent);

            for (int i = 0; i < ClusterPoseIds.Length; ++i)
            {
                ref readonly PoseData pose = ref a_animData.Poses[i];

                UsedPoseIds[i] = pose.PoseId;
                UsedPoseIdMap[pose.PoseId] = i;

                Trajectory8 packedTrajectory = new Trajectory8();
                packedTrajectory.A = new float4x4();
                packedTrajectory.B = new float4x4();

                TrajectoryPoint[] trajectoryPoints = pose.Trajectory;

                packedTrajectory.A.c0 = new float4(trajectoryPoints[0].Position.x, trajectoryPoints[1].Position.x,
                                                 trajectoryPoints[2].Position.x, trajectoryPoints[3].Position.x);

                packedTrajectory.A.c1 = new float4(trajectoryPoints[0].Position.y, trajectoryPoints[1].Position.y,
                                                 trajectoryPoints[2].Position.y, trajectoryPoints[3].Position.y);

                packedTrajectory.A.c2 = new float4(trajectoryPoints[0].Position.z, trajectoryPoints[1].Position.z,
                                                 trajectoryPoints[2].Position.z, trajectoryPoints[3].Position.z);

                packedTrajectory.A.c3 = new float4(trajectoryPoints[0].FacingAngle, trajectoryPoints[1].FacingAngle,
                                                 trajectoryPoints[2].FacingAngle, trajectoryPoints[3].FacingAngle);

                packedTrajectory.B.c0 = new float4(trajectoryPoints[4].Position.x, trajectoryPoints[5].Position.x,
                                                 trajectoryPoints[6].Position.x, 0f);

                packedTrajectory.B.c1 = new float4(trajectoryPoints[4].Position.y, trajectoryPoints[5].Position.y,
                                                 trajectoryPoints[6].Position.y, 0f);

                packedTrajectory.B.c2 = new float4(trajectoryPoints[4].Position.z, trajectoryPoints[5].Position.z,
                                                 trajectoryPoints[6].Position.z, 0f);

                packedTrajectory.B.c3 = new float4(trajectoryPoints[4].FacingAngle, trajectoryPoints[5].FacingAngle,
                                                 trajectoryPoints[6].FacingAngle, 0f);


                TrajectoriesPacked_08[i] = packedTrajectory;
            }
        }

        private void GenerateTrajectory8(MxMAnimData a_animData)
        {
            int usedPoseCount = ClusterPoseIds.Length;

            TrajectoriesPacked_08 = new NativeArray<Trajectory8>(usedPoseCount, Allocator.Persistent);

            for (int i = 0; i < ClusterPoseIds.Length; ++i)
            {
                ref readonly PoseData pose = ref a_animData.Poses[i];

                UsedPoseIds[i] = pose.PoseId;
                UsedPoseIdMap[pose.PoseId] = i;

                Trajectory8 packedTrajectory = new Trajectory8();
                packedTrajectory.A = new float4x4();
                packedTrajectory.B = new float4x4();

                TrajectoryPoint[] trajectoryPoints = pose.Trajectory;

                packedTrajectory.A.c0 = new float4(trajectoryPoints[0].Position.x, trajectoryPoints[1].Position.x,
                                                 trajectoryPoints[2].Position.x, trajectoryPoints[3].Position.x);

                packedTrajectory.A.c1 = new float4(trajectoryPoints[0].Position.y, trajectoryPoints[1].Position.y,
                                                 trajectoryPoints[2].Position.y, trajectoryPoints[3].Position.y);

                packedTrajectory.A.c2 = new float4(trajectoryPoints[0].Position.z, trajectoryPoints[1].Position.z,
                                                 trajectoryPoints[2].Position.z, trajectoryPoints[3].Position.z);

                packedTrajectory.A.c3 = new float4(trajectoryPoints[0].FacingAngle, trajectoryPoints[1].FacingAngle,
                                                 trajectoryPoints[2].FacingAngle, trajectoryPoints[3].FacingAngle);

                packedTrajectory.B.c0 = new float4(trajectoryPoints[4].Position.x, trajectoryPoints[5].Position.x,
                                                 trajectoryPoints[6].Position.x, trajectoryPoints[7].Position.x);

                packedTrajectory.B.c1 = new float4(trajectoryPoints[4].Position.y, trajectoryPoints[5].Position.y,
                                                 trajectoryPoints[6].Position.y, trajectoryPoints[7].Position.y);

                packedTrajectory.B.c2 = new float4(trajectoryPoints[4].Position.z, trajectoryPoints[5].Position.z,
                                                 trajectoryPoints[6].Position.z, trajectoryPoints[7].Position.z);

                packedTrajectory.B.c3 = new float4(trajectoryPoints[4].FacingAngle, trajectoryPoints[5].FacingAngle,
                                                 trajectoryPoints[6].FacingAngle, trajectoryPoints[7].FacingAngle);


                TrajectoriesPacked_08[i] = packedTrajectory;
            }
        }

        private void GenerateTrajectory9(MxMAnimData a_animData)
        {
            int usedPoseCount = ClusterPoseIds.Length;

            TrajectoriesPacked_09 = new NativeArray<Trajectory9>(usedPoseCount, Allocator.Persistent);

            for (int i = 0; i < ClusterPoseIds.Length; ++i)
            {
                ref readonly PoseData pose = ref a_animData.Poses[i];

                UsedPoseIds[i] = pose.PoseId;
                UsedPoseIdMap[pose.PoseId] = i;

                Trajectory9 packedTrajectory = new Trajectory9();
                packedTrajectory.A = new float3x4();
                packedTrajectory.B = new float3x4();
                packedTrajectory.C = new float3x4();

                TrajectoryPoint[] trajectoryPoints = pose.Trajectory;

                packedTrajectory.A.c0 = new float3(trajectoryPoints[0].Position.x, trajectoryPoints[1].Position.x,
                                                 trajectoryPoints[2].Position.x);

                packedTrajectory.A.c1 = new float3(trajectoryPoints[0].Position.y, trajectoryPoints[1].Position.y,
                                                 trajectoryPoints[2].Position.y);

                packedTrajectory.A.c2 = new float3(trajectoryPoints[0].Position.z, trajectoryPoints[1].Position.z,
                                                 trajectoryPoints[2].Position.z);

                packedTrajectory.A.c3 = new float3(trajectoryPoints[0].FacingAngle, trajectoryPoints[1].FacingAngle,
                                                 trajectoryPoints[2].FacingAngle);

                packedTrajectory.B.c0 = new float3(trajectoryPoints[3].Position.x, trajectoryPoints[4].Position.x,
                                                 trajectoryPoints[5].Position.x);

                packedTrajectory.B.c1 = new float3(trajectoryPoints[3].Position.y, trajectoryPoints[4].Position.y,
                                                 trajectoryPoints[5].Position.y);

                packedTrajectory.B.c2 = new float3(trajectoryPoints[3].Position.z, trajectoryPoints[4].Position.z,
                                                 trajectoryPoints[5].Position.z);

                packedTrajectory.B.c3 = new float3(trajectoryPoints[3].FacingAngle, trajectoryPoints[4].FacingAngle,
                                                 trajectoryPoints[5].FacingAngle);

                packedTrajectory.C.c0 = new float3(trajectoryPoints[6].Position.x, trajectoryPoints[7].Position.x,
                                                 trajectoryPoints[8].Position.x);

                packedTrajectory.C.c1 = new float3(trajectoryPoints[6].Position.y, trajectoryPoints[7].Position.y,
                                                 trajectoryPoints[8].Position.y);

                packedTrajectory.C.c2 = new float3(trajectoryPoints[6].Position.z, trajectoryPoints[7].Position.z,
                                                 trajectoryPoints[8].Position.z);

                packedTrajectory.C.c3 = new float3(trajectoryPoints[6].FacingAngle, trajectoryPoints[7].FacingAngle,
                                                 trajectoryPoints[8].FacingAngle);

                TrajectoriesPacked_09[i] = packedTrajectory;
            }
        }

        private void GenerateTrajectory10(MxMAnimData a_animData)
        {
            int usedPoseCount = ClusterPoseIds.Length;

            TrajectoriesPacked_12 = new NativeArray<Trajectory12>(usedPoseCount, Allocator.Persistent);

            for (int i = 0; i < a_animData.Poses.Length; ++i)
            {
                ref readonly PoseData pose = ref a_animData.Poses[i];

                UsedPoseIds[i] = pose.PoseId;
                UsedPoseIdMap[pose.PoseId] = i;

                Trajectory12 packedTrajectory = new Trajectory12();
                packedTrajectory.A = new float4x4();
                packedTrajectory.B = new float4x4();
                packedTrajectory.C = new float4x4();

                TrajectoryPoint[] trajectoryPoints = pose.Trajectory;

                packedTrajectory.A.c0 = new float4(trajectoryPoints[0].Position.x, trajectoryPoints[1].Position.x,
                                                 trajectoryPoints[2].Position.x, trajectoryPoints[3].Position.x);

                packedTrajectory.A.c1 = new float4(trajectoryPoints[0].Position.y, trajectoryPoints[1].Position.y,
                                                 trajectoryPoints[2].Position.y, trajectoryPoints[3].Position.y);

                packedTrajectory.A.c2 = new float4(trajectoryPoints[0].Position.z, trajectoryPoints[1].Position.z,
                                                 trajectoryPoints[2].Position.z, trajectoryPoints[3].Position.z);

                packedTrajectory.A.c3 = new float4(trajectoryPoints[0].FacingAngle, trajectoryPoints[1].FacingAngle,
                                                 trajectoryPoints[2].FacingAngle, trajectoryPoints[3].FacingAngle);

                packedTrajectory.B.c0 = new float4(trajectoryPoints[4].Position.x, trajectoryPoints[5].Position.x,
                                                 trajectoryPoints[6].Position.x, trajectoryPoints[7].Position.x);

                packedTrajectory.B.c1 = new float4(trajectoryPoints[4].Position.y, trajectoryPoints[5].Position.y,
                                                 trajectoryPoints[6].Position.y, trajectoryPoints[7].Position.y);

                packedTrajectory.B.c2 = new float4(trajectoryPoints[4].Position.z, trajectoryPoints[5].Position.z,
                                                 trajectoryPoints[6].Position.z, trajectoryPoints[7].Position.z);

                packedTrajectory.B.c3 = new float4(trajectoryPoints[4].FacingAngle, trajectoryPoints[5].FacingAngle,
                                                 trajectoryPoints[6].FacingAngle, trajectoryPoints[7].FacingAngle);

                packedTrajectory.C.c0 = new float4(trajectoryPoints[8].Position.x, trajectoryPoints[9].Position.x, 0f, 0f);
                packedTrajectory.C.c1 = new float4(trajectoryPoints[8].Position.y, trajectoryPoints[9].Position.y, 0f, 0f);
                packedTrajectory.C.c2 = new float4(trajectoryPoints[8].Position.z, trajectoryPoints[9].Position.z, 0f, 0f);
                packedTrajectory.C.c3 = new float4(trajectoryPoints[8].FacingAngle, trajectoryPoints[9].FacingAngle, 0f, 0f);

                TrajectoriesPacked_12[i] = packedTrajectory;
            }
        }

        private void GenerateTrajectory11(MxMAnimData a_animData)
        {
            int usedPoseCount = ClusterPoseIds.Length;

            TrajectoriesPacked_12 = new NativeArray<Trajectory12>(usedPoseCount, Allocator.Persistent);

            for (int i = 0; i < ClusterPoseIds.Length; ++i)
            {
                ref readonly PoseData pose = ref a_animData.Poses[i];

                UsedPoseIds[i] = pose.PoseId;
                UsedPoseIdMap[pose.PoseId] = i;

                Trajectory12 packedTrajectory = new Trajectory12();
                packedTrajectory.A = new float4x4();
                packedTrajectory.B = new float4x4();
                packedTrajectory.C = new float4x4();

                TrajectoryPoint[] trajectoryPoints = pose.Trajectory;

                packedTrajectory.A.c0 = new float4(trajectoryPoints[0].Position.x, trajectoryPoints[1].Position.x,
                                                 trajectoryPoints[2].Position.x, trajectoryPoints[3].Position.x);

                packedTrajectory.A.c1 = new float4(trajectoryPoints[0].Position.y, trajectoryPoints[1].Position.y,
                                                 trajectoryPoints[2].Position.y, trajectoryPoints[3].Position.y);

                packedTrajectory.A.c2 = new float4(trajectoryPoints[0].Position.z, trajectoryPoints[1].Position.z,
                                                 trajectoryPoints[2].Position.z, trajectoryPoints[3].Position.z);

                packedTrajectory.A.c3 = new float4(trajectoryPoints[0].FacingAngle, trajectoryPoints[1].FacingAngle,
                                                 trajectoryPoints[2].FacingAngle, trajectoryPoints[3].FacingAngle);

                packedTrajectory.B.c0 = new float4(trajectoryPoints[4].Position.x, trajectoryPoints[5].Position.x,
                                                 trajectoryPoints[6].Position.x, trajectoryPoints[7].Position.x);

                packedTrajectory.B.c1 = new float4(trajectoryPoints[4].Position.y, trajectoryPoints[5].Position.y,
                                                 trajectoryPoints[6].Position.y, trajectoryPoints[7].Position.y);

                packedTrajectory.B.c2 = new float4(trajectoryPoints[4].Position.z, trajectoryPoints[5].Position.z,
                                                 trajectoryPoints[6].Position.z, trajectoryPoints[7].Position.z);

                packedTrajectory.B.c3 = new float4(trajectoryPoints[4].FacingAngle, trajectoryPoints[5].FacingAngle,
                                                 trajectoryPoints[6].FacingAngle, trajectoryPoints[7].FacingAngle);

                packedTrajectory.C.c0 = new float4(trajectoryPoints[8].Position.x, trajectoryPoints[9].Position.x,
                                                 trajectoryPoints[10].Position.x, 0f);

                packedTrajectory.C.c1 = new float4(trajectoryPoints[8].Position.y, trajectoryPoints[9].Position.y,
                                                 trajectoryPoints[10].Position.y, 0f);

                packedTrajectory.C.c2 = new float4(trajectoryPoints[8].Position.z, trajectoryPoints[9].Position.z,
                                                 trajectoryPoints[10].Position.z, 0f);

                packedTrajectory.C.c3 = new float4(trajectoryPoints[8].FacingAngle, trajectoryPoints[9].FacingAngle,
                                                 trajectoryPoints[10].FacingAngle, 0f);

                TrajectoriesPacked_12[i] = packedTrajectory;
            }
        }

        private void GenerateTrajectory12(MxMAnimData a_animData)
        {
            int usedPoseCount = ClusterPoseIds.Length;

            TrajectoriesPacked_12 = new NativeArray<Trajectory12>(usedPoseCount, Allocator.Persistent);

            for (int i = 0; i < ClusterPoseIds.Length; ++i)
            {
                ref readonly PoseData pose = ref a_animData.Poses[i];

                UsedPoseIds[i] = pose.PoseId;
                UsedPoseIdMap[pose.PoseId] = i;

                Trajectory12 packedTrajectory = new Trajectory12();
                packedTrajectory.A = new float4x4();
                packedTrajectory.B = new float4x4();
                packedTrajectory.C = new float4x4();

                TrajectoryPoint[] trajectoryPoints = pose.Trajectory;

                packedTrajectory.A.c0 = new float4(trajectoryPoints[0].Position.x, trajectoryPoints[1].Position.x,
                                                 trajectoryPoints[2].Position.x, trajectoryPoints[3].Position.x);

                packedTrajectory.A.c1 = new float4(trajectoryPoints[0].Position.y, trajectoryPoints[1].Position.y,
                                                 trajectoryPoints[2].Position.y, trajectoryPoints[3].Position.y);

                packedTrajectory.A.c2 = new float4(trajectoryPoints[0].Position.z, trajectoryPoints[1].Position.z,
                                                 trajectoryPoints[2].Position.z, trajectoryPoints[3].Position.z);

                packedTrajectory.A.c3 = new float4(trajectoryPoints[0].FacingAngle, trajectoryPoints[1].FacingAngle,
                                                 trajectoryPoints[2].FacingAngle, trajectoryPoints[3].FacingAngle);

                packedTrajectory.B.c0 = new float4(trajectoryPoints[4].Position.x, trajectoryPoints[5].Position.x,
                                                 trajectoryPoints[6].Position.x, trajectoryPoints[7].Position.x);

                packedTrajectory.B.c1 = new float4(trajectoryPoints[4].Position.y, trajectoryPoints[5].Position.y,
                                                 trajectoryPoints[6].Position.y, trajectoryPoints[7].Position.y);

                packedTrajectory.B.c2 = new float4(trajectoryPoints[4].Position.z, trajectoryPoints[5].Position.z,
                                                 trajectoryPoints[6].Position.z, trajectoryPoints[7].Position.z);

                packedTrajectory.B.c3 = new float4(trajectoryPoints[4].FacingAngle, trajectoryPoints[5].FacingAngle,
                                                 trajectoryPoints[6].FacingAngle, trajectoryPoints[7].FacingAngle);

                packedTrajectory.C.c0 = new float4(trajectoryPoints[8].Position.x, trajectoryPoints[9].Position.x,
                                                 trajectoryPoints[10].Position.x, trajectoryPoints[11].Position.x);

                packedTrajectory.C.c1 = new float4(trajectoryPoints[8].Position.y, trajectoryPoints[9].Position.y,
                                                 trajectoryPoints[10].Position.y, trajectoryPoints[11].Position.y);

                packedTrajectory.C.c2 = new float4(trajectoryPoints[8].Position.z, trajectoryPoints[9].Position.z,
                                                 trajectoryPoints[10].Position.z, trajectoryPoints[11].Position.z);

                packedTrajectory.C.c3 = new float4(trajectoryPoints[8].FacingAngle, trajectoryPoints[9].FacingAngle,
                                                 trajectoryPoints[10].FacingAngle, trajectoryPoints[11].FacingAngle);


                TrajectoriesPacked_12[i] = packedTrajectory;
            }
        }

        private void GeneratePose1(MxMAnimData a_animData)
        {
            int usedPoseCount = ClusterPoseIds.Length;

            PosesPacked_01 = new NativeArray<float3x3>(usedPoseCount, Allocator.Persistent);

            for (int i = 0; i < ClusterPoseIds.Length; ++i)
            {
                ref PoseData pose = ref a_animData.Poses[i];

                UsedPoseIds[i] = pose.PoseId;
                UsedPoseIdMap[pose.PoseId] = i;

                float3x3 newPackedPose = new float3x3();

                newPackedPose.c0 = new float3(pose.JointsData[0].Position.x, pose.JointsData[1].Velocity.x, pose.LocalVelocity.x);
                newPackedPose.c1 = new float3(pose.JointsData[0].Position.y, pose.JointsData[1].Velocity.y, pose.LocalVelocity.x);
                newPackedPose.c2 = new float3(pose.JointsData[0].Position.z, pose.JointsData[1].Velocity.z, pose.LocalVelocity.x);

                PosesPacked_01[i] = newPackedPose;

                ClipIdsPacked[i] = pose.PrimaryClipId;
                FavourPacked[i] = pose.Favour;
                FavourTagsPacked[i] = pose.FavourTags;
            }
        }

        private void GeneratePose2(MxMAnimData a_animData)
        {
            int usedPoseCount = ClusterPoseIds.Length;

            PosesPacked_02 = new NativeArray<Pose2>(usedPoseCount, Allocator.Persistent);

            for (int i = 0; i < ClusterPoseIds.Length; ++i)
            {
                ref PoseData pose = ref a_animData.Poses[i];

                UsedPoseIds[i] = pose.PoseId;
                UsedPoseIdMap[pose.PoseId] = i;

                Pose2 newPackedPose = new Pose2();
                newPackedPose.JointPositions = new float2x3();
                newPackedPose.JointVelocities = new float3x3();

                newPackedPose.JointPositions.c0 = new float2(pose.JointsData[0].Position.x, pose.JointsData[1].Position.x);
                newPackedPose.JointPositions.c1 = new float2(pose.JointsData[0].Position.y, pose.JointsData[1].Position.y);
                newPackedPose.JointPositions.c2 = new float2(pose.JointsData[0].Position.z, pose.JointsData[1].Position.z);

                newPackedPose.JointVelocities.c0 = new float3(pose.LocalVelocity.x, pose.JointsData[0].Velocity.x,
                                                      pose.JointsData[1].Velocity.x);

                newPackedPose.JointVelocities.c1 = new float3(pose.LocalVelocity.y, pose.JointsData[0].Velocity.y,
                                                      pose.JointsData[1].Velocity.y);

                newPackedPose.JointVelocities.c2 = new float3(pose.LocalVelocity.z, pose.JointsData[0].Velocity.z,
                                                      pose.JointsData[1].Velocity.z);

                PosesPacked_02[i] = newPackedPose;


                ClipIdsPacked[i] = pose.PrimaryClipId;
                FavourPacked[i] = pose.Favour;
                FavourTagsPacked[i] = pose.FavourTags;
            }
        }

        private void GeneratePose3(MxMAnimData a_animData)
        {
            int usedPoseCount = ClusterPoseIds.Length;

            PosesPacked_03 = new NativeArray<Pose3>(usedPoseCount, Allocator.Persistent);

            for (int i = 0; i < ClusterPoseIds.Length; ++i)
            {
                ref PoseData pose = ref a_animData.Poses[i];

                UsedPoseIds[i] = pose.PoseId;
                UsedPoseIdMap[pose.PoseId] = i;

                Pose3 newPackedPose = new Pose3();
                newPackedPose.JointPositions = new float3x3();
                newPackedPose.JointVelocities = new float4x3();

                newPackedPose.JointPositions.c0 = new float3(pose.JointsData[0].Position.x, pose.JointsData[1].Position.x, pose.JointsData[2].Position.x);
                newPackedPose.JointPositions.c1 = new float3(pose.JointsData[0].Position.y, pose.JointsData[1].Position.y, pose.JointsData[2].Position.y);
                newPackedPose.JointPositions.c2 = new float3(pose.JointsData[0].Position.z, pose.JointsData[1].Position.z, pose.JointsData[2].Position.z);

                newPackedPose.JointVelocities.c0 = new float4(pose.LocalVelocity.x, pose.JointsData[0].Velocity.x,
                                                      pose.JointsData[1].Velocity.x, pose.JointsData[2].Velocity.x);

                newPackedPose.JointVelocities.c1 = new float4(pose.LocalVelocity.y, pose.JointsData[0].Velocity.y,
                                                      pose.JointsData[1].Velocity.y, pose.JointsData[2].Velocity.y);

                newPackedPose.JointVelocities.c2 = new float4(pose.LocalVelocity.z, pose.JointsData[0].Velocity.z,
                                                      pose.JointsData[1].Velocity.z, pose.JointsData[2].Velocity.z);

                PosesPacked_03[i] = newPackedPose;

                ClipIdsPacked[i] = pose.PrimaryClipId;
                FavourPacked[i] = pose.Favour;
                FavourTagsPacked[i] = pose.FavourTags;
            }
        }

        private void GeneratePose4(MxMAnimData a_animData)
        {
            int usedPoseCount = ClusterPoseIds.Length;

            PosesPacked_04 = new NativeArray<Pose4>(usedPoseCount, Allocator.Persistent);

            for (int i = 0; i < ClusterPoseIds.Length; ++i)
            {
                ref PoseData pose = ref a_animData.Poses[i];

                UsedPoseIds[i] = pose.PoseId;
                UsedPoseIdMap[pose.PoseId] = i;

                Pose4 newPackedPose = new Pose4();
                newPackedPose.JointPositions = new float4x3();
                newPackedPose.JointVelocities = new float4x3();
                newPackedPose.BodyVelocity = pose.LocalVelocity;

                newPackedPose.JointPositions.c0 = new float4(pose.JointsData[0].Position.x, pose.JointsData[1].Position.x,
                    pose.JointsData[2].Position.x, pose.JointsData[3].Position.x);

                newPackedPose.JointPositions.c1 = new float4(pose.JointsData[0].Position.y, pose.JointsData[1].Position.y,
                    pose.JointsData[2].Position.y, pose.JointsData[3].Position.y);

                newPackedPose.JointPositions.c2 = new float4(pose.JointsData[0].Position.z, pose.JointsData[1].Position.z,
                    pose.JointsData[2].Position.z, pose.JointsData[3].Position.z);

                newPackedPose.JointVelocities.c0 = new float4(pose.JointsData[0].Velocity.x, pose.JointsData[1].Velocity.x,
                    pose.JointsData[2].Velocity.x, pose.JointsData[3].Velocity.x);

                newPackedPose.JointVelocities.c1 = new float4(pose.JointsData[0].Velocity.y, pose.JointsData[1].Velocity.y,
                    pose.JointsData[2].Velocity.y, pose.JointsData[3].Velocity.y);

                newPackedPose.JointVelocities.c2 = new float4(pose.JointsData[0].Velocity.z, pose.JointsData[1].Velocity.z,
                    pose.JointsData[2].Velocity.z, pose.JointsData[3].Velocity.z);

                PosesPacked_04[i] = newPackedPose;
                ClipIdsPacked[i] = pose.PrimaryClipId;
                FavourPacked[i] = pose.Favour;
                FavourTagsPacked[i] = pose.FavourTags;
            }
        }

        private void GeneratePose5(MxMAnimData a_animData)
        {
            int usedPoseCount = ClusterPoseIds.Length;

            PosesPacked_05 = new NativeArray<Pose5>(usedPoseCount, Allocator.Persistent);

            for (int i = 0; i < ClusterPoseIds.Length; ++i)
            {
                ref PoseData pose = ref a_animData.Poses[i];

                UsedPoseIds[i] = pose.PoseId;
                UsedPoseIdMap[pose.PoseId] = i;

                Pose5 newPackedPose = new Pose5();
                newPackedPose.JointPositionsA = new float3x3();
                newPackedPose.JointVelocitiesA = new float3x3();
                newPackedPose.JointPositionsB = new float2x3();
                newPackedPose.JointVelocitiesB = new float3x3();

                newPackedPose.JointPositionsA.c0 = new float3(pose.JointsData[0].Position.x, pose.JointsData[1].Position.x, pose.JointsData[2].Position.x);
                newPackedPose.JointPositionsA.c1 = new float3(pose.JointsData[0].Position.y, pose.JointsData[1].Position.y, pose.JointsData[2].Position.y);
                newPackedPose.JointPositionsA.c2 = new float3(pose.JointsData[0].Position.z, pose.JointsData[1].Position.z, pose.JointsData[2].Position.z);

                newPackedPose.JointVelocitiesA.c0 = new float3(pose.LocalVelocity.x, pose.JointsData[0].Velocity.x, pose.JointsData[1].Velocity.x);
                newPackedPose.JointVelocitiesA.c1 = new float3(pose.LocalVelocity.y, pose.JointsData[0].Velocity.y, pose.JointsData[1].Velocity.y);
                newPackedPose.JointVelocitiesA.c2 = new float3(pose.LocalVelocity.z, pose.JointsData[0].Velocity.z, pose.JointsData[1].Velocity.z);

                newPackedPose.JointPositionsB.c0 = new float2(pose.JointsData[3].Position.x, pose.JointsData[4].Position.x);
                newPackedPose.JointPositionsB.c1 = new float2(pose.JointsData[3].Position.y, pose.JointsData[4].Position.y);
                newPackedPose.JointPositionsB.c2 = new float2(pose.JointsData[3].Position.z, pose.JointsData[4].Position.z);
                newPackedPose.JointVelocitiesB.c0 = new float3(pose.JointsData[2].Velocity.x, pose.JointsData[3].Velocity.x, pose.JointsData[4].Velocity.x);
                newPackedPose.JointVelocitiesB.c1 = new float3(pose.JointsData[2].Velocity.y, pose.JointsData[3].Velocity.y, pose.JointsData[4].Velocity.y);
                newPackedPose.JointVelocitiesB.c2 = new float3(pose.JointsData[2].Velocity.z, pose.JointsData[3].Velocity.z, pose.JointsData[4].Velocity.z);

                PosesPacked_05[i] = newPackedPose;

                ClipIdsPacked[i] = pose.PrimaryClipId;
                FavourPacked[i] = pose.Favour;
                FavourTagsPacked[i] = pose.FavourTags;
            }
        }

        private void GeneratePose6(MxMAnimData a_animData)
        {
            int usedPoseCount = ClusterPoseIds.Length;

            PosesPacked_06 = new NativeArray<Pose6>(usedPoseCount, Allocator.Persistent);

            for (int i = 0; i < ClusterPoseIds.Length; ++i)
            {
                ref PoseData pose = ref a_animData.Poses[i];

                UsedPoseIds[i] = pose.PoseId;
                UsedPoseIdMap[pose.PoseId] = i;

                Pose6 newPackedPose = new Pose6();
                newPackedPose.JointPositionsA = new float3x3();
                newPackedPose.JointVelocitiesA = new float3x3();
                newPackedPose.JointPositionsB = new float3x3();
                newPackedPose.JointVelocitiesB = new float3x3();

                newPackedPose.JointPositionsA.c0 = new float3(pose.JointsData[0].Position.x, pose.JointsData[1].Position.x, pose.JointsData[2].Position.x);
                newPackedPose.JointPositionsA.c1 = new float3(pose.JointsData[0].Position.y, pose.JointsData[1].Position.y, pose.JointsData[2].Position.y);
                newPackedPose.JointPositionsA.c2 = new float3(pose.JointsData[0].Position.z, pose.JointsData[1].Position.z, pose.JointsData[2].Position.z);

                newPackedPose.JointVelocitiesA.c0 = new float3(pose.JointsData[0].Velocity.x,
                                                      pose.JointsData[1].Velocity.x, pose.JointsData[2].Velocity.x);

                newPackedPose.JointVelocitiesA.c1 = new float3(pose.JointsData[0].Velocity.y,
                                                      pose.JointsData[1].Velocity.y, pose.JointsData[2].Velocity.y);

                newPackedPose.JointVelocitiesA.c2 = new float3(pose.JointsData[0].Velocity.z,
                                                      pose.JointsData[1].Velocity.z, pose.JointsData[2].Velocity.z);

                newPackedPose.JointPositionsB.c0 = new float3(pose.JointsData[3].Position.x, pose.JointsData[4].Position.x, pose.JointsData[5].Position.x);
                newPackedPose.JointPositionsB.c1 = new float3(pose.JointsData[3].Position.y, pose.JointsData[4].Position.y, pose.JointsData[5].Position.y);
                newPackedPose.JointPositionsB.c2 = new float3(pose.JointsData[3].Position.z, pose.JointsData[4].Position.z, pose.JointsData[5].Position.z);
                newPackedPose.JointVelocitiesB.c0 = new float3(pose.JointsData[3].Velocity.x, pose.JointsData[4].Velocity.x, pose.JointsData[5].Velocity.x);
                newPackedPose.JointVelocitiesB.c1 = new float3(pose.JointsData[3].Velocity.y, pose.JointsData[4].Velocity.y, pose.JointsData[5].Velocity.y);
                newPackedPose.JointVelocitiesB.c2 = new float3(pose.JointsData[3].Velocity.z, pose.JointsData[4].Velocity.z, pose.JointsData[5].Velocity.z);

                newPackedPose.BodyVelocity = pose.LocalVelocity;

                PosesPacked_06[i] = newPackedPose;

                ClipIdsPacked[i] = pose.PrimaryClipId;
                FavourPacked[i] = pose.Favour;
                FavourTagsPacked[i] = pose.FavourTags;
            }
        }

        private void GeneratePose7(MxMAnimData a_animData)
        {
            int usedPoseCount = ClusterPoseIds.Length;

            PosesPacked_07 = new NativeArray<Pose7>(usedPoseCount, Allocator.Persistent);

            for (int i = 0; i < ClusterPoseIds.Length; ++i)
            {
                ref PoseData pose = ref a_animData.Poses[i];

                UsedPoseIds[i] = pose.PoseId;
                UsedPoseIdMap[pose.PoseId] = i;

                Pose7 newPackedPose = new Pose7();
                newPackedPose.JointPositionsA = new float4x3();
                newPackedPose.JointVelocitiesA = new float4x3();
                newPackedPose.JointPositionsB = new float3x3();
                newPackedPose.JointVelocitiesB = new float4x3();

                newPackedPose.JointPositionsA.c0 = new float4(pose.JointsData[0].Position.x, pose.JointsData[1].Position.x, pose.JointsData[2].Position.x, pose.JointsData[3].Position.x);
                newPackedPose.JointPositionsA.c1 = new float4(pose.JointsData[0].Position.y, pose.JointsData[1].Position.y, pose.JointsData[2].Position.y, pose.JointsData[3].Position.y);
                newPackedPose.JointPositionsA.c2 = new float4(pose.JointsData[0].Position.z, pose.JointsData[1].Position.z, pose.JointsData[2].Position.z, pose.JointsData[3].Position.z);

                newPackedPose.JointVelocitiesA.c0 = new float4(pose.LocalVelocity.x, pose.JointsData[0].Velocity.x, pose.JointsData[1].Velocity.x, pose.JointsData[2].Velocity.x);
                newPackedPose.JointVelocitiesA.c1 = new float4(pose.LocalVelocity.y, pose.JointsData[0].Velocity.y, pose.JointsData[1].Velocity.y, pose.JointsData[2].Velocity.y);
                newPackedPose.JointVelocitiesA.c2 = new float4(pose.LocalVelocity.z, pose.JointsData[0].Velocity.z, pose.JointsData[1].Velocity.z, pose.JointsData[2].Velocity.z);

                newPackedPose.JointPositionsB.c0 = new float3(pose.JointsData[4].Position.x, pose.JointsData[5].Position.x, pose.JointsData[6].Position.x);
                newPackedPose.JointPositionsB.c1 = new float3(pose.JointsData[4].Position.y, pose.JointsData[5].Position.y, pose.JointsData[6].Position.y);
                newPackedPose.JointPositionsB.c2 = new float3(pose.JointsData[4].Position.z, pose.JointsData[5].Position.z, pose.JointsData[6].Position.z);

                newPackedPose.JointVelocitiesB.c0 = new float4(pose.JointsData[3].Velocity.x, pose.JointsData[4].Velocity.x, pose.JointsData[5].Velocity.x, pose.JointsData[6].Velocity.x);
                newPackedPose.JointVelocitiesB.c1 = new float4(pose.JointsData[3].Velocity.y, pose.JointsData[4].Velocity.y, pose.JointsData[5].Velocity.y, pose.JointsData[6].Velocity.x);
                newPackedPose.JointVelocitiesB.c2 = new float4(pose.JointsData[3].Velocity.z, pose.JointsData[4].Velocity.z, pose.JointsData[5].Velocity.z, pose.JointsData[6].Velocity.x);

                PosesPacked_07[i] = newPackedPose;

                ClipIdsPacked[i] = pose.PrimaryClipId;
                FavourPacked[i] = pose.Favour;
                FavourTagsPacked[i] = pose.FavourTags;
            }
        }

        private void GeneratePose8(MxMAnimData a_animData)
        {
            int usedPoseCount = ClusterPoseIds.Length;

            PosesPacked_08 = new NativeArray<Pose8>(usedPoseCount, Allocator.Persistent);

            int index7 = Mathf.Min(7, a_animData.MatchBones.Length - 1);

            for (int i = 0; i < ClusterPoseIds.Length; ++i)
            {
                ref PoseData pose = ref a_animData.Poses[i];

                UsedPoseIds[i] = pose.PoseId;
                UsedPoseIdMap[pose.PoseId] = i;

                Pose8 newPackedPose = new Pose8();
                newPackedPose.JointPositionsA = new float4x3();
                newPackedPose.JointPositionsB = new float4x3();
                newPackedPose.JointVelocitiesA = new float4x3();
                newPackedPose.JointVelocitiesB = new float4x3();
                newPackedPose.BodyVelocity = pose.LocalVelocity;

                newPackedPose.JointPositionsA.c0 = new float4(pose.JointsData[0].Position.x, pose.JointsData[1].Position.x,
                    pose.JointsData[2].Position.x, pose.JointsData[3].Position.x);

                newPackedPose.JointPositionsA.c1 = new float4(pose.JointsData[0].Position.y, pose.JointsData[1].Position.y,
                    pose.JointsData[2].Position.y, pose.JointsData[3].Position.y);

                newPackedPose.JointPositionsA.c2 = new float4(pose.JointsData[0].Position.z, pose.JointsData[1].Position.z,
                    pose.JointsData[2].Position.z, pose.JointsData[3].Position.z);

                newPackedPose.JointVelocitiesA.c0 = new float4(pose.JointsData[0].Velocity.x, pose.JointsData[1].Velocity.x,
                    pose.JointsData[2].Velocity.x, pose.JointsData[3].Velocity.x);

                newPackedPose.JointVelocitiesA.c1 = new float4(pose.JointsData[0].Velocity.y, pose.JointsData[1].Velocity.y,
                    pose.JointsData[2].Velocity.y, pose.JointsData[3].Velocity.y);

                newPackedPose.JointVelocitiesA.c2 = new float4(pose.JointsData[0].Velocity.z, pose.JointsData[1].Velocity.z,
                    pose.JointsData[2].Velocity.z, pose.JointsData[3].Velocity.z);

                newPackedPose.JointPositionsB.c0 = new float4(pose.JointsData[4].Position.x, pose.JointsData[5].Position.x,
                    pose.JointsData[6].Position.x, pose.JointsData[index7].Position.x);

                newPackedPose.JointPositionsB.c1 = new float4(pose.JointsData[4].Position.y, pose.JointsData[5].Position.y,
                    pose.JointsData[6].Position.y, pose.JointsData[index7].Position.y);

                newPackedPose.JointPositionsB.c2 = new float4(pose.JointsData[4].Position.z, pose.JointsData[5].Position.z,
                    pose.JointsData[6].Position.z, pose.JointsData[index7].Position.z);

                newPackedPose.JointVelocitiesB.c0 = new float4(pose.JointsData[4].Velocity.x, pose.JointsData[5].Velocity.x,
                    pose.JointsData[6].Velocity.x, pose.JointsData[index7].Velocity.x);

                newPackedPose.JointVelocitiesB.c1 = new float4(pose.JointsData[4].Velocity.y, pose.JointsData[5].Velocity.y,
                    pose.JointsData[6].Velocity.y, pose.JointsData[index7].Velocity.y);

                newPackedPose.JointVelocitiesB.c2 = new float4(pose.JointsData[4].Velocity.z, pose.JointsData[5].Velocity.z,
                    pose.JointsData[6].Velocity.z, pose.JointsData[index7].Velocity.z);

                PosesPacked_08[i] = newPackedPose;
                ClipIdsPacked[i] = pose.PrimaryClipId;
                FavourPacked[i] = pose.Favour;
                FavourTagsPacked[i] = pose.FavourTags;
            }
        }
    }
}