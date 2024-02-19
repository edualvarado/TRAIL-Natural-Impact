using UnityEngine;
using System;

namespace MxM
{
    [System.Serializable]
    public struct Trajectory
    {
        TrajectoryPoint PointA;
        TrajectoryPoint PointB;
        TrajectoryPoint PointC;
        TrajectoryPoint PointD;

    }//End of struct: Trajectory

    public struct TrajectoryFlat
    {
        float PointAPosX;
        float PointAPosY;
        float PointAPosZ;
        float PointAFacing;

        float PointBPosX;
        float PointBPosY;
        float PointBPosZ;
        float PointBFacing;

        float PointCPosX;
        float PointCPosY;
        float PointCPosZ;
        float PointCFacing;

        float PointDPosX;
        float PointDPosY;
        float PointDPosZ;
        float PointDFacing;

        float PointEPosX;
        float PointEPosY;
        float PointEPosZ;
        float PointEFacing;

    }//End of struct: TrajectoryFlat

    public struct TrajectorySemi
    {
        Vector3 PointAPos;
        float PointAFacing;

        Vector3 PointBPos;
        float PointBFacing;

        Vector3 PointCPos;
        float PointCFacing;

        Vector3 PointDPos;
        float PointDFacing;

        Vector3 PointEPos;
        float PointEFacing;
    }//End of struct: TrajectorySemi


}//End of namespace: MxM

