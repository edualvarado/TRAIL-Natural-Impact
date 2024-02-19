// ============================================================================================
// File: Goal.cs
// 
// Authors:  Kenneth Claassen
// Date:     2017-09-16: Created this file.
// 
//     Contains a part of the 'MxM' namespace for 'Unity Engine 5'.
// 
// Copyright (c) 2017 Kenneth Claassen. All rights reserved.
// ============================================================================================
using System.Collections.Generic;
using UnityEngine;

namespace MxM
{
    //=============================================================================================
    /**
    *  @brief 
    *         
    *********************************************************************************************/
    public static class Goal
    {
        //=============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public static void SetRelativeTo(TrajectoryPoint[] a_trajectory, Transform a_transform, int a_startIndex = 0)
        {
            float rot = a_transform.rotation.eulerAngles.y;

            for (int i = a_startIndex; i < a_trajectory.Length; ++i)
            {
                Vector3 newPos = a_transform.InverseTransformVector(a_trajectory[i].Position);
                float newRot = a_trajectory[i].FacingAngle - rot;

                a_trajectory[i] = new TrajectoryPoint(newPos, newRot);
            }
        }

        //=============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public static Vector3 RotateVector(Vector3 _vec, float _rot) //rot is in degrees
        {
            Vector3 ret = new Vector3();

            _rot *= Mathf.Deg2Rad;

            ret.x = _vec.x * Mathf.Cos(_rot) - _vec.z * Mathf.Sin(_rot);
            ret.z = _vec.x * Mathf.Sin(_rot) + _vec.z * Mathf.Cos(_rot);

            return ret;
        }

        //=============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public static void LerpGoal(TrajectoryPoint[] _target, TrajectoryPoint[] _startGoal, 
            TrajectoryPoint[] _endGoal, float _lerpVal)
        {
            for(int i=0; i < _target.Length; ++i)
            {
                TrajectoryPoint fromPoint = _startGoal[i];
                TrajectoryPoint toPoint = _endGoal[i];

                Vector3 pos = Vector3.Lerp(fromPoint.Position, toPoint.Position, _lerpVal);
                float fAngle = Mathf.LerpAngle(fromPoint.FacingAngle, toPoint.FacingAngle, _lerpVal);

                _target[i] = new TrajectoryPoint(pos, fAngle);
            }
        }


    }//End of class: Goal
}//End of namespace: MxM

