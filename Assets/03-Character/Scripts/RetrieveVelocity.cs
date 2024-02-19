/****************************************************
 * File: RetrieveVelocity.cs
   * Author: Eduardo Alvarado
   * Email: alvaradopinero.eduardo@gmail.com
   * Date: 12/01/2024
   * Project: Foot2Trail
   * Last update: 12/01/2024
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MxM
{
    public class RetrieveVelocity : MonoBehaviour
    {
        #region Instance Fields

        [Header("Set-up")]
        public Transform root;

        #endregion

        #region Read-only & Static Fields

        private static Vector3 velocityLeftFoot;
        private static Vector3 velocityRightFoot;
        private static Vector3 velocityLeftToe;
        private static Vector3 velocityRightToe;
        private static bool isIdle;
        
        #endregion

        #region Instance Properties

        public static Vector3 VelocityLeftFoot { get { return velocityLeftFoot; } }
        public static Vector3 VelocityRightFoot { get { return velocityRightFoot; } }
        public static Vector3 VelocityLeftToe { get { return velocityLeftToe; } }
        public static Vector3 VelocityRightToe { get { return velocityRightToe; } }
        public static bool IsIdle { get { return isIdle; } }

        #endregion

        #region Read-only & Static Properties

        private MxMAnimator currentAnimator;

        #endregion

        #region Unity Methods

        // Start is called before the first frame update
        void Start()
        {
            currentAnimator = GetComponent<MxMAnimator>();
        }
        
        // Update is called once per frame
        void Update()
        {
            //for (int i = 0; i < currentAnimator.CurrentInterpolatedPose.JointsData.Length; i++)
            //{
            //    Debug.Log("Joint " + i + " : " + currentAnimator.CurrentInterpolatedPose.JointsData[i].Velocity);
            //}

            velocityLeftFoot = root.TransformVector(currentAnimator.CurrentInterpolatedPose.JointsData[0].Velocity);
            velocityRightFoot = root.TransformVector(currentAnimator.CurrentInterpolatedPose.JointsData[1].Velocity);
            velocityLeftToe = root.TransformVector(currentAnimator.CurrentInterpolatedPose.JointsData[3].Velocity);
            velocityRightToe = root.TransformVector(currentAnimator.CurrentInterpolatedPose.JointsData[4].Velocity);
            isIdle = currentAnimator.IsIdle;
        }

        #endregion
    }
}

