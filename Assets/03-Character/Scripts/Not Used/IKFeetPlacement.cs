/****************************************************
 * File: IKFeetPlacement.cs
   * Author: Eduardo Alvarado
   * Email: alvaradopinero.eduardo@gmail.com
   * Date: 12/01/2024
   * Project: Foot2Trail
   * Last update: 12/01/2024
*****************************************************/

// OLD VERSION -> Use IKFootAdaptation.cs instead

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class IKFeetPlacement : MonoBehaviour
{
    #region Instance Fields

    [Header("Terrain Deformation")]
    [SerializeField] private DeformTerrainMaster deformTerrainMaster;

    //public float minFootY = 1;

    [Header("Offset TEST")]
    [Range(-90f, 90f)] public float proxyAngle;
    public bool activateHeelSlopeAdaptation;
    public float maxOffset = 0.05f;
    public float maxAngle = 20f;
    public float maxOffsetKnee = 0.05f;
    public float maxAngleKnee = 20f;

    [Header("Offset Gizmo")]
    public Vector3 offsetGizmoHeel; 
    public Vector3 offsetGizmoToe;
    [Range(0f, 1f)] public float offsetRadius;

    [Header("Feet Grounder")]
    public bool enableFeetIK = true;
    [Range(0, 20f)][SerializeField] private float heightFromGroundRaycast = 0.2f;
    [Range(0, 20f)][SerializeField] private float raycastDownDistance = 1.0f;
    [SerializeField] private LayerMask environmentLayer;
    [SerializeField] private LayerMask vegetationLayer;
    [SerializeField] private float pelvisOffset = 0f;
    [SerializeField] private float offsetFootLineLeft = 0f;
    [SerializeField] private float offsetRisingLeftKnee;
    [SerializeField] private float offsetFootLineRight = 0f;
    [SerializeField] private float offsetRisingRightKnee;
    [Range(0, 1f)][SerializeField] private float pelvisUpAndDownSpeed = 0.3f;
    [Range(0, 1f)][SerializeField] private float feetToIKPositionSpeed = 0.2f;

    [Header("Individual Feet Grounder - Debug")]
    public bool isLeftFootGrounded = false;
    public bool isRightFootGrounded = false;

    [Header("Individual Feet Grounder - Toes and Heels - Debug")]
    public bool isLeftHeelGrounded = false;
    public bool isLeftToeGrounded = false;
    public bool isRightHeelGrounded = false;
    public bool isRightToeGrounded = false;

    [Header("Individual Feet Grounder - SET-UP")]
    public bool drawNormalHit = false;
    public bool drawSensorRayGrounder = false;
    public Transform groundCheckerLeftFootHeel;
    public Transform groundCheckerLeftFootToe;
    public Transform groundCheckerRightFootHeel;
    public Transform groundCheckerRightFootToe;
    //public float feetToGroundDistance = 0.1f;
    public float heelToGroundDistance = 0.055f;
    public float toeToGroundDistance = 0.04f;

    [Header("IK Weights - Debug")]
    [Range(0f, 1f)] public float leftWeightHeel;
    [Range(0f, 1f)] public float leftWeightToe;
    [Range(0f, 1f)] public float rightWeighHeel;
    [Range(0f, 1f)] public float rightWeightToe;

    [Header("IK Weights - (SET-UP)")]
    public float leftToGroundDistance;
    public float rightToGroundDistance;
    public float minHeelDistance = 0f;
    public float maxHeelDistance = 0.01f;
    public float minToeDistance = 0f;
    public float maxToeDistance = 0.01f;
    public bool activateToeAdaptation;
    public bool rotateWithRespectOriginalNormalTerrain;
    public float footYToeDistanceRight;
    public float footYToeDistanceRightOld = 0;
    public float footYToeDistanceLeft;
    public float footYToeDistanceLeftOld = 0;
    public LayerMask ground;

    [Header("Feet Positions - Debug")]
    public float _leftHeelHeight;
    public float _leftToeHeight;
    public float _rightHeelHeight;
    public float _rightToeHeight;
    public Vector3 leftFootPosition;
    public Vector3 leftFootIKPosition;
    public Vector3 rightFootPosition;
    public Vector3 rightFootIKPosition;

    [Header("Other Settings")]
    public string leftHeelWeightAnimator = "LeftHeelWeight";
    public string leftToeWeightAnimator = "LeftToeWeight";
    public string rightHeelWeightAnimator = "RightHeelWeight";
    public string rightToeWeightAnimator = "RightToeWeight";
    public bool useDefaultIKFeature = true;
    public bool useProDefaultIKFeature = true;
    public bool showSolverDebug = true;
    public bool showGrounders = true;

    #endregion

    #region Instance Properties

    public Vector3 RightFootPosition {
        get { return rightFootPosition; }
        set { rightFootPosition = value; }
    }

    public Vector3 LeftFootPosition
    {
        get { return leftFootPosition; }
        set { leftFootPosition = value; }
    }

    public Vector3 RightFootIKPosition
    {
        get { return rightFootIKPosition; }
        set { rightFootIKPosition = value; }
    }

    public Vector3 LeftFootIKPosition
    {
        get { return leftFootIKPosition; }
        set { leftFootIKPosition = value; }
    }

    public float LeftHeelHeight
    {
        get { return _leftHeelHeight; }
        set { _leftHeelHeight = value; }
    }
    public float LeftToeHeight
    {
        get { return _leftToeHeight; }
        set { _leftToeHeight = value; }
    }

    public float RightHeelHeight
    {
        get { return _rightHeelHeight; }
        set { _rightHeelHeight = value; }
    }
    public float RightToeHeight
    {
        get { return _rightToeHeight; }
        set { _rightToeHeight = value; }
    }

    #endregion

    #region Read-only & Static Fields

    private Animator _anim;
    private Quaternion _leftFootIKRotation, _rightFootIKRotation;
    private float _lastPelvisPositionY, _lastRightFootPositionY, _lastLeftFootPositionY;

    // Balance Experimental - TODO    
    private float _eps = 0.001f;
    private Vector3 _leftToeNormal;
    private Vector3 _rightToeNormal;
    private Vector3 _leftHeelNormal;
    private Vector3 _rightHeelNormal;
    private Vector3 _leftToePoint;
    private Vector3 _rightToePoint;
    private Vector3 _leftHeelPoint;
    private Vector3 _rightHeelPoint;
    
    private Transform _transformLeftToe;
    private Transform _transformLeftFoot;
    private Quaternion _currentRotationLeftToe;
    private Transform _transformRightToe;
    private Transform _transformRightFoot;
    private Quaternion _currentRotationRightToe;

    private Quaternion _currentRotationLeftHeel;
    private Quaternion _currentRotationRightHeel;


    #endregion

    #region Unity Methods

    void Start()
    {
        // Set Environment Layer
        environmentLayer = LayerMask.GetMask("Ground");
        _anim = GetComponent<Animator>();

        _transformLeftToe = _anim.GetBoneTransform(HumanBodyBones.LeftToes);
        _transformLeftFoot = _anim.GetBoneTransform(HumanBodyBones.LeftFoot);
        _transformRightToe = _anim.GetBoneTransform(HumanBodyBones.RightToes);
        _transformRightFoot = _anim.GetBoneTransform(HumanBodyBones.RightFoot);
    }

    private void Update()
    {
        // Use to estimate offsets when walking uphill
        EstimateOffsetUphill();

        // Define weights for Foot and Toe IK. In heels we apply, for toes is done later in LateUpdate().
        //DefineIKWeightsLeft();
        //DefineIKWeightsRight();

        // Estaimte distances between sensors and ground at each frame - In-progress
        //GetHeightsHeelToe();
    }

    private void EstimateOffsetUphill()
    {
        Vector3 globalToeEndRight = _transformRightFoot.position - groundCheckerRightFootToe.position;
        Vector3 localToeEndRight = groundCheckerRightFootToe.InverseTransformVector(globalToeEndRight);
        //Debug.DrawRay(groundCheckerRightFootToe.position, globalToeEndRight, Color.cyan);

        Vector3 globalProjectedVectorRight = Vector3.ProjectOnPlane(globalToeEndRight, Vector3.up);
        //Debug.DrawRay(groundCheckerRightFootToe.position, globalProjectedVectorRight, Color.red);

        float magnitudeRight = globalProjectedVectorRight.magnitude;
        //if (magnitudeRight > footYToeDistanceRightOld)
        //{
        //    footYToeDistanceRight = magnitudeRight;
        //    footYToeDistanceRightOld = footYToeDistanceRight;
        //}
        footYToeDistanceRight = magnitudeRight;

        //Vector3 maxProjectedVectorRight = globalProjectedVectorRight.normalized * footYToeDistanceRight;
        //Debug.DrawRay(groundCheckerRightFootToe.position, maxProjectedVectorRight, Color.red);

        // --

        // FROM TOE TO HEEL
        Vector3 globalToeEndLeft = _transformLeftFoot.position - groundCheckerLeftFootToe.position;
        //Vector3 localToeEndLeft = groundCheckerLeftFootToe.InverseTransformVector(globalToeEndLeft);

        //Debug.DrawRay(groundCheckerLeftFootToe.position, globalToeEndLeft, Color.cyan);

        // FROM TOE TO PROJECTED IN B (VIRTUAL GROUND)
        Vector3 globalProjectedVectorLeft = Vector3.ProjectOnPlane(globalToeEndLeft, Vector3.up);

        //Debug.DrawRay(groundCheckerLeftFootToe.position, globalProjectedVectorLeft, Color.red);

        float magnitudeLeft = globalProjectedVectorLeft.magnitude;

        //if (magnitudeLeft > footYToeDistanceLeftOld)
        //{
        //    footYToeDistanceLeft = magnitudeLeft;
        //    footYToeDistanceLeftOld = footYToeDistanceLeft;
        //}
        footYToeDistanceLeft = magnitudeLeft;
    }

    /// <summary>
    /// Estimate distances between sensors and ground.
    /// </summary>
    private void GetHeightsHeelToe()
    {
        // Left Foot
        RaycastHit hitLeftHeel;
        if (Physics.Raycast(groundCheckerLeftFootHeel.position, -deformTerrainMaster.slopeInitialNormal, out hitLeftHeel, Mathf.Infinity, ground)) // Changed: slopeNormal by slopeInitialNormal
        {
            _leftHeelPoint = hitLeftHeel.point;

            if (hitLeftHeel.collider.gameObject.CompareTag("Obstacle") || hitLeftHeel.collider.gameObject.CompareTag("Terrain"))
            {
                _leftHeelHeight = hitLeftHeel.distance - _eps;

                // Test: Given contact point, calculate original normal there was in that point
                if (rotateWithRespectOriginalNormalTerrain)
                {
                    _leftHeelNormal = deformTerrainMaster.initialNormalMatrix[(int)deformTerrainMaster.World2Grid(_leftHeelPoint).x, (int)deformTerrainMaster.World2Grid(_leftHeelPoint).z];

                    if (drawNormalHit)
                        Debug.DrawRay(_leftHeelPoint, _leftHeelNormal, Color.blue);
                }
                else
                {
                    _leftHeelNormal = hitLeftHeel.normal;

                    if (drawNormalHit)
                        Debug.DrawRay(_leftHeelPoint, _leftHeelNormal, Color.yellow);
                }
            }
        }

        RaycastHit hitLeftToe;
        if (Physics.Raycast(groundCheckerLeftFootToe.position, -deformTerrainMaster.slopeInitialNormal, out hitLeftToe, Mathf.Infinity, ground)) // Changed: slopeNormal by slopeInitialNormal
        {
            _leftToePoint = hitLeftToe.point;

            if (hitLeftToe.collider.gameObject.CompareTag("Obstacle") || hitLeftToe.collider.gameObject.CompareTag("Terrain"))
            {
                _leftToeHeight = hitLeftToe.distance;

                // Test: Given contact point, calculate original normal there was in that point
                if (rotateWithRespectOriginalNormalTerrain)
                {
                    _leftToeNormal = deformTerrainMaster.initialNormalMatrix[(int)deformTerrainMaster.World2Grid(_leftToePoint).x, (int)deformTerrainMaster.World2Grid(_leftToePoint).z];
                    
                    if (drawNormalHit)
                        Debug.DrawRay(_leftToePoint, _leftToeNormal, Color.blue);
                }
                else
                {
                    _leftToeNormal = hitLeftToe.normal;

                    if (drawNormalHit)
                        Debug.DrawRay(_leftToePoint, _leftToeNormal, Color.yellow);
                }       
            }
        }

        // Right Foot
        RaycastHit hitRightHeel;
        if (Physics.Raycast(groundCheckerRightFootHeel.position, -deformTerrainMaster.slopeInitialNormal, out hitRightHeel, Mathf.Infinity, ground)) // Changed: slopeNormal by slopeInitialNormal
        {
            _rightHeelPoint = hitRightHeel.point;

            if (hitRightHeel.collider.gameObject.CompareTag("Obstacle") || hitRightHeel.collider.gameObject.CompareTag("Terrain"))
            {
                _rightHeelHeight = hitRightHeel.distance - _eps;

                // Test: Given contact point, calculate original normal there was in that point
                if (rotateWithRespectOriginalNormalTerrain)
                {
                    _rightHeelNormal = deformTerrainMaster.initialNormalMatrix[(int)deformTerrainMaster.World2Grid(_rightHeelPoint).x, (int)deformTerrainMaster.World2Grid(_rightHeelPoint).z];

                    if (drawNormalHit)
                        Debug.DrawRay(_rightHeelPoint, _rightHeelNormal, Color.blue);
                }
                else
                {
                    _rightHeelNormal = hitRightHeel.normal;

                    if (drawNormalHit)
                        Debug.DrawRay(_rightHeelPoint, _rightHeelNormal, Color.yellow);
                }
            }
        }

        RaycastHit hitRightToe;
        if (Physics.Raycast(groundCheckerRightFootToe.position, -deformTerrainMaster.slopeInitialNormal, out hitRightToe, Mathf.Infinity, ground)) // Changed: slopeNormal by slopeInitialNormal
        {
            _rightToePoint = hitRightToe.point;

            if (hitRightToe.collider.gameObject.CompareTag("Obstacle") || hitRightToe.collider.gameObject.CompareTag("Terrain"))
            {
                _rightToeHeight = hitRightToe.distance;

                // Test: Given contact point, calculate original normal there was in that point
                if (rotateWithRespectOriginalNormalTerrain)
                {
                    _rightToeNormal = deformTerrainMaster.initialNormalMatrix[(int)deformTerrainMaster.World2Grid(_rightToePoint).x, (int)deformTerrainMaster.World2Grid(_rightToePoint).z];

                    if (drawNormalHit)
                        Debug.DrawRay(_rightToePoint, _rightToeNormal, Color.blue);
                }
                else
                {
                    _rightToeNormal = hitRightToe.normal;

                    if (drawNormalHit)
                        Debug.DrawRay(_rightToePoint, _rightToeNormal, Color.yellow);
                }
            }
        }

        // Debug
        if (_leftHeelHeight < _leftToeHeight)
        {
            //Debug.DrawLine(groundCheckerLeftFootHeel.position, _leftHeelPoint, Color.red);
            //Debug.DrawLine(groundCheckerLeftFootToe.position, _leftToePoint, Color.blue);
            //Debug.Log("Left Heel CLOSER: " + leftHeelHeight + " than Toe: " + leftToeHeight);
        }
        else
        {
            //Debug.DrawLine(groundCheckerLeftFootToe.position, _leftToePoint, Color.red);
            //Debug.DrawLine(groundCheckerLeftFootHeel.position, _leftHeelPoint, Color.blue);
            //Debug.Log("Left Toe CLOSER: " + leftToeHeight + " than Heel: " + leftHeelHeight);
        }

        if (_rightHeelHeight < _rightToeHeight)
        {
            //Debug.DrawLine(groundCheckerRightFootHeel.position, _rightHeelPoint, Color.red);
            //Debug.DrawLine(groundCheckerRightFootToe.position, _rightToePoint, Color.blue);
            //Debug.Log("Left Heel CLOSER: " + leftHeelHeight + " than Toe: " + leftToeHeight);
        }
        else
        {
            //Debug.DrawLine(groundCheckerRightFootToe.position, _rightToePoint, Color.red);
            //Debug.DrawLine(groundCheckerRightFootHeel.position, _rightHeelPoint, Color.blue);
            //Debug.Log("Left Toe CLOSER: " + leftToeHeight + " than Heel: " + leftHeelHeight);
        }

        // Not ready yet
        //var numerator = (leftHeelHeight - leftToeHeight) * alpha;
        //var denominator = terrainMaster.leftFootCollider.transform.localScale.z;

        //Debug.Log("BALANCE: " + Mathf.Clamp((Mathf.Atan2(numerator, denominator) / Mathf.PI), 0f, 1f));
    }

    /// <summary>
    /// Update the AdjustFeetTarget method and also find the position of each foot inside our Solver Position.
    /// </summary>
    private void FixedUpdate()
    {            
        // Estaimte distances between sensors and ground at each frame - In-progress
        GetHeightsHeelToe();

        // Define weights for Foot and Toe IK - SEE IF MOVE TO UPDATE() or LATEUPDATE() - ORIGINALLY IN BOTH
        DefineIKWeightsLeft();
        DefineIKWeightsRight();

        if (enableFeetIK == false) { return; }
        if (_anim == null) { return; }

        // Adjust Target for feet
        AdjustFeetTarget(ref rightFootPosition, HumanBodyBones.RightFoot);
        AdjustFeetTarget(ref leftFootPosition, HumanBodyBones.LeftFoot);

        // Find a raycast to the ground to find positions
        FeetPositionSolver(AvatarIKGoal.RightFoot, rightFootPosition, ref rightFootIKPosition, ref _rightFootIKRotation); // Handle the solver for right foot
        FeetPositionSolver(AvatarIKGoal.LeftFoot, leftFootPosition, ref leftFootIKPosition, ref _leftFootIKRotation); // Handle the solver for left foot

        // Check if each foot is grounded
        CheckFeetAreGrounded();

        /* The pivot is the most stable point between the left and right foot of the avatar. 
         * For a value of 0, the left foot is the most stable point. For a value of 1, the right foot is the most stable point.
         */

        //Debug.Log("[INFO] Pivot Weight: " + anim.pivotWeight);
        //Debug.Log("[INFO] Pivot Position: " + this.transform.InverseTransformPoint(anim.pivotPosition));

        // Apply offset!
        // -----------------------

        /// FOR DEFORMABLE TERRAIN ///
        // ------------------------ //
        if (deformTerrainMaster.slopeInitialAngle > 0)
        {
            offsetFootLineLeft = footYToeDistanceLeft * Mathf.Tan(deformTerrainMaster.slopeInitialAngle * Mathf.Deg2Rad);
            offsetFootLineRight = footYToeDistanceRight * Mathf.Tan(deformTerrainMaster.slopeInitialAngle * Mathf.Deg2Rad);

            //Debug.DrawRay(leftFootIKPosition, Vector3.up * offsetFootLineLeft, Color.black);
        }
        else
        {
            offsetFootLineLeft = 0f;
            offsetFootLineRight = 0f;
        }

        /// FOR OBSTACLE ///
        // -------------- //

        // USING TAN
        //offsetFootLineLeft = footYToeDistanceLeft * Mathf.Tan(proxyAngle * Mathf.Deg2Rad);
        //if (showProxyAngle)
        //    Debug.DrawRay(leftFootIKPosition, Vector3.up * offsetFootLineLeft, Color.black);

        //if (deformTerrainMaster.slopeAngle > 0)
        //{
        //    offsetFootLineLeft = footYToeDistanceLeft * Mathf.Tan(deformTerrainMaster.slopeAngle * Mathf.Deg2Rad);
        //    offsetFootLineRight = footYToeDistanceRight * Mathf.Tan(deformTerrainMaster.slopeAngle * Mathf.Deg2Rad);

        //    //if (showProxyAngle)
        //    //    Debug.DrawRay(leftFootIKPosition, Vector3.up * offsetFootLineLeft, Color.black);
        //}
        //else
        //{
        //    offsetFootLineLeft = 0f;
        //    offsetFootLineRight = 0f;
        //}

        // Test - EXPERIMENTAL
        //offsetFootLineLeft = Mathf.Lerp(0f, maxOffset, deformTerrainMaster.slopeAngle / maxAngle);
        //offsetFootLineRight = Mathf.Lerp(0f, maxOffset, deformTerrainMaster.slopeAngle / maxAngle);

        // Another Test - EXPERIMENTAL

        offsetRisingLeftKnee = Mathf.Lerp(0f, maxOffsetKnee, deformTerrainMaster.slopeAngle / maxAngleKnee);
        offsetRisingRightKnee = Mathf.Lerp(0f, maxOffsetKnee, deformTerrainMaster.slopeAngle / maxAngleKnee);

    }

    #endregion

    #region Weights Estimation

    /// <summary>
    /// Estimate distance between heel sensor (or joint) and ground.
    /// Weight is estimated by looking to the respective distance between sensor and ground wrt. min (0) and max height during gait. The closer to the ground, the higher the weight.
    /// Later, when toes are in contact, we define their weights (without applying) based on the time.
    /// This function, in theory, should replace the curve to estimate the weight.
    /// </summary>
    private void DefineIKWeightsLeft()
    {
        // Heels
        // -----

        // Estimate weight based on distance
        leftWeightHeel = Mathf.Clamp(1 - ((_leftHeelHeight - minHeelDistance) / ((maxHeelDistance) - minHeelDistance)), 0f, 1f);
        _anim.SetFloat(leftHeelWeightAnimator, leftWeightHeel);

        // Toes
        // ----
       
        leftWeightToe = Mathf.Clamp(1 - ((_leftToeHeight - minToeDistance) / ((maxToeDistance) - minToeDistance)), 0f, 1f);
        _anim.SetFloat(leftToeWeightAnimator, leftWeightToe);
    }

    /// <summary>
    /// Estimate distance between heel sensor (or joint) and ground.
    /// Weight is estimated by looking to the respective distance between sensor and ground wrt. min (0) and max height during gait. The closer to the ground, the higher the weight.
    /// Later, when toes are in contact, we define their weights (without applying) based on the time.
    /// This function, in theory, should replace the curve to estimate the weight.
    /// </summary>
    private void DefineIKWeightsRight()
    {
        // Heels
        // -----

        rightWeighHeel = Mathf.Clamp(1 - ((_rightHeelHeight - minHeelDistance) / ((maxHeelDistance) - minHeelDistance)), 0f, 1f);

        //Debug.Log("Defining RIGHT HEEL weight");
        //Debug.Log("_rightHeelHeight: " + _rightHeelHeight);
        //Debug.Log("maxHeelDistance: " + maxHeelDistance);
        //Debug.Log("minHeelDistance: " + minHeelDistance);
        //Debug.Log("(_rightHeelHeight - minHeelDistance) " + (_rightHeelHeight - minHeelDistance));
        //Debug.Log("(maxHeelDistance - minHeelDistance)) " + (maxHeelDistance - minHeelDistance));
        //Debug.Log("Weight Right Heel: " + rightWeighHeel);

        _anim.SetFloat(rightHeelWeightAnimator, rightWeighHeel);

        // Toes
        // ----

        rightWeightToe = Mathf.Clamp(1 - ((_rightToeHeight - minToeDistance) / ((maxToeDistance) - minToeDistance)), 0f, 1f);
        _anim.SetFloat(rightToeWeightAnimator, rightWeightToe);
    }

    #endregion

    #region FeetGrounding

    /// <summary>
    /// Check if heel/toes sensors (and overall foot) is grounded.
    /// </summary>
    private void CheckFeetAreGrounded()
    {
        // Left Foot
        if (Physics.CheckSphere(groundCheckerLeftFootToe.position, toeToGroundDistance, environmentLayer, QueryTriggerInteraction.Ignore) ||
            Physics.CheckSphere(groundCheckerLeftFootHeel.position, heelToGroundDistance, environmentLayer, QueryTriggerInteraction.Ignore))
        {
            isLeftFootGrounded = true;

        }
        else
        {
            isLeftFootGrounded = false;
        }

        // Left Toe & Heel
        if (Physics.CheckSphere(groundCheckerLeftFootToe.position, toeToGroundDistance, environmentLayer, QueryTriggerInteraction.Ignore))
            isLeftToeGrounded = true;
        else
            isLeftToeGrounded = false;

        if (Physics.CheckSphere(groundCheckerLeftFootHeel.position, heelToGroundDistance, environmentLayer, QueryTriggerInteraction.Ignore))
            isLeftHeelGrounded = true;
        else
            isLeftHeelGrounded = false;

        // Right Foot
        if (Physics.CheckSphere(groundCheckerRightFootToe.position, toeToGroundDistance, environmentLayer, QueryTriggerInteraction.Ignore) ||
            Physics.CheckSphere(groundCheckerRightFootHeel.position, heelToGroundDistance, environmentLayer, QueryTriggerInteraction.Ignore))
        {
            isRightFootGrounded = true;

        }
        else
        {
            isRightFootGrounded = false;
        }

        // Right Toe & Heel
        if (Physics.CheckSphere(groundCheckerRightFootToe.position, toeToGroundDistance, environmentLayer, QueryTriggerInteraction.Ignore))
            isRightToeGrounded = true;
        else
            isRightToeGrounded = false;

        if (Physics.CheckSphere(groundCheckerRightFootHeel.position, heelToGroundDistance, environmentLayer, QueryTriggerInteraction.Ignore))
            isRightHeelGrounded = true;
        else
            isRightHeelGrounded = false;

        if (drawSensorRayGrounder)
        {
            Debug.DrawRay(groundCheckerLeftFootToe.position, -Vector3.up * toeToGroundDistance, Color.white);
            Debug.DrawRay(groundCheckerLeftFootHeel.position, -Vector3.up * heelToGroundDistance, Color.white);
            Debug.DrawRay(groundCheckerRightFootToe.position, -Vector3.up * toeToGroundDistance, Color.white);
            Debug.DrawRay(groundCheckerRightFootHeel.position, -Vector3.up * heelToGroundDistance, Color.white);
        }
    }
    
    /// <summary>
    /// Called when IK Pass is activated.
    /// </summary>
    /// <param name="layerIndex"></param>
    private void OnAnimatorIK(int layerIndex)
    {
        if (_anim == null) { return; }
        
        MovePelvisHeight();

        // TEST
        //SetOffsetVegetation();

        // Right Foot
        // ----------

        // RightFoot IK Position  - Max. IK position
        if (useDefaultIKFeature)
        {
            _anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
        }
        else
        {
            _anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
        }

        // RightFoot IK Rotation  - for PRO feature
        if (useProDefaultIKFeature)
        {
            if (!isRightHeelGrounded)
            {
                _anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, _anim.GetFloat(rightHeelWeightAnimator));
                //Debug.Log("isRightHeelGrounded: " + isRightHeelGrounded);
                //Debug.Log("_anim.GetFloat(rightHeelWeightAnimator): " + _anim.GetFloat(rightHeelWeightAnimator));
            }
            else if (isRightHeelGrounded)
            {
                _anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);
                //Debug.Log("isRightHeelGrounded: " + isRightHeelGrounded);
                //Debug.Log("Value is 1");
            }
        }

        // Move RightFoot to the target IK position and rotation
        MoveFeetToIKPoint(AvatarIKGoal.RightFoot, rightFootIKPosition, _rightFootIKRotation, ref _lastRightFootPositionY);

        // Left Foot
        // ----------

        // LeftFoot IK Position  - Max. IK position
        if (useDefaultIKFeature)
        {
            _anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
        }
        else
        {
            _anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
        }

        // LeftFoot IK Rotation  - for PRO feature
        if (useProDefaultIKFeature)
        {
            if (!isLeftHeelGrounded)
            {
                _anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, _anim.GetFloat(leftHeelWeightAnimator));
            }
            else if (isLeftHeelGrounded)
            {
                _anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
            }
        }

        // Move RightFoot to the target IK position and rotation
        MoveFeetToIKPoint(AvatarIKGoal.LeftFoot, leftFootIKPosition, _leftFootIKRotation, ref _lastLeftFootPositionY);
        
        // Save current orientation at the end of the OnAnimatorIK
        _currentRotationLeftToe = _transformLeftToe.localRotation;
        _currentRotationRightToe = _transformRightToe.localRotation;

        // Test
        _currentRotationLeftHeel = _transformLeftFoot.localRotation;
        _currentRotationRightHeel = _transformRightFoot.localRotation;
    }

    private void SetOffsetVegetation()
    {
        throw new NotImplementedException();
    }

    private void LateUpdate()
    {
        // EXPERIMENTAL
        if (activateHeelSlopeAdaptation)
        {
            RotateLeftHeelSlope();
            RotateRightHeelSlope();
        }

        // Must be apply at the end
        if (activateToeAdaptation)
        {
            RotateLeftToes();
            RotateRigthToes();
        }
    }

    private void RotateLeftHeelSlope()
    {
        Quaternion currentRotation = _currentRotationLeftHeel;

        //Debug.Log("deformTerrainMaster.slopeAngle: " + deformTerrainMaster.slopeAngle);
        Quaternion rotation = Quaternion.Euler(deformTerrainMaster.slopeAngle, 0, 0);
        //Quaternion rotation = Quaternion.Euler(proxyAngle, 0, 0);

        Quaternion targetRotation = currentRotation * rotation;
        _transformLeftFoot.localRotation = Quaternion.Slerp(currentRotation, targetRotation, 1f);
    }


    private void RotateRightHeelSlope()
    {
        Quaternion currentRotation = _currentRotationRightHeel;

        //Debug.Log("deformTerrainMaster.slopeAngle: " + deformTerrainMaster.slopeAngle);
        Quaternion rotation = Quaternion.Euler(deformTerrainMaster.slopeAngle, 0, 0);
        //Quaternion rotation = Quaternion.Euler(proxyAngle, 0, 0);

        Quaternion targetRotation = currentRotation * rotation;
        _transformRightFoot.localRotation = Quaternion.Slerp(currentRotation, targetRotation, 1f);
    }

    private void RotateLeftToes()
    {
        Vector3 localNormal = Quaternion.Inverse(_transformLeftFoot.rotation) * _leftToeNormal;

        Quaternion currentRotation = _currentRotationLeftToe;
        Quaternion targetRotation = Quaternion.LookRotation(localNormal, Quaternion.Inverse(_transformLeftFoot.rotation) * Vector3.ProjectOnPlane(_transformLeftToe.up, _leftToeNormal));
        
        if(!isLeftToeGrounded)
        {
            _transformLeftToe.localRotation = Quaternion.Slerp(currentRotation, targetRotation, leftWeightToe);
        }
        else if (isLeftToeGrounded)
        {
            _transformLeftToe.localRotation = Quaternion.Slerp(currentRotation, targetRotation, 1f);
        }
    }

    private void RotateRigthToes()
    {
        Vector3 localNormal = Quaternion.Inverse(_transformRightFoot.rotation) * _rightToeNormal;

        Quaternion currentRotation = _currentRotationRightToe;
        Quaternion targetRotation = Quaternion.LookRotation(localNormal, Quaternion.Inverse(_transformRightFoot.rotation) * Vector3.ProjectOnPlane(_transformRightToe.up, _rightToeNormal));

        if (!isRightToeGrounded)
        {
            _transformRightToe.localRotation = Quaternion.Slerp(currentRotation, targetRotation, rightWeightToe);
        }
        else if (isRightToeGrounded)
        {
            _transformRightToe.localRotation = Quaternion.Slerp(currentRotation, targetRotation, 1f);
        }

    }

    #endregion

    #region FeetGroundingMethods

    /// <summary>
    /// Move feet to the IK target.
    /// </summary>
    /// <param name="foot"></param>
    /// <param name="positionIKHolder"></param>
    /// <param name="rotationIKHolder"></param>
    /// <param name="lastFootPositionY"></param>
    void MoveFeetToIKPoint(AvatarIKGoal foot, Vector3 positionIKHolder, Quaternion rotationIKHolder, ref float lastFootPositionY)
    {
        //  Get the current position of the foot, which we are going to move
        Vector3 targetIKPosition = _anim.GetIKPosition(foot);

        //Debug.DrawRay(targetIKPosition, Vector3.right, Color.cyan); // On the foot
        //Debug.DrawRay(positionIKHolder, Vector3.right, Color.green); // On the ground

        //Debug.Log("targetIKPosition: " + targetIKPosition);
        //Debug.Log("positionIKHolder: " + positionIKHolder);

        // If there is a IK target in a different position (not 0 locally) than the position where we have our foot currently
        if (positionIKHolder != Vector3.zero)
        {              
            // Convert the world coordinates for current/target foot positions to local coordinates with respect to the character
            targetIKPosition = transform.InverseTransformPoint(targetIKPosition);
            positionIKHolder = transform.InverseTransformPoint(positionIKHolder);

            //Debug.Log("targetIKPosition Before: " + targetIKPosition);
            //Debug.Log("positionIKHolder Before: " + positionIKHolder);

            // Calculate the translation in Y necessary to move the last foot position to the target position, by a particular speed
            float yVariable = 0f;
            if (foot == AvatarIKGoal.LeftFoot)
            {
                //offsetRisingLeftKnee...
                yVariable = Mathf.Lerp(lastFootPositionY, positionIKHolder.y, feetToIKPositionSpeed) + offsetFootLineLeft + offsetRisingLeftKnee;
            }
            else
            {
                yVariable = Mathf.Lerp(lastFootPositionY, positionIKHolder.y, feetToIKPositionSpeed) + offsetFootLineRight + offsetRisingRightKnee;
            }

            //Debug.Log("targetIKPosition After: " + targetIKPosition);
            //Debug.Log("positionIKHolder After: " + positionIKHolder);
            //Debug.Log("yVariable: " + yVariable);

            // Add this desired translation in Y to our current feet position
            targetIKPosition.y += (yVariable);

            // We update the last foot position in Y
            lastFootPositionY = (yVariable);

            //Debug.Log("targetIKPosition After yVariable: " + targetIKPosition);
            //Debug.Log("positionIKHolder After yVariable: " + positionIKHolder);

            // Convert the current foot position to world coordinates
            targetIKPosition = transform.TransformPoint(targetIKPosition);

            // Set the new goal rotation (world coordinates) for the foot
            _anim.SetIKRotation(foot, rotationIKHolder);
        }

        // Set the new goal position (world coordinates) for the foot
        _anim.SetIKPosition(foot, targetIKPosition);
    }

    /// <summary>
    /// Adapt height of pelvis.
    /// </summary>
    private void MovePelvisHeight()
    {
        if(rightFootIKPosition == Vector3.zero || leftFootIKPosition == Vector3.zero || _lastPelvisPositionY == 0)
        {
            _lastPelvisPositionY = _anim.bodyPosition.y;
            return;
        }

        float leftOffsetPosition = leftFootIKPosition.y - transform.position.y;
        float rightOffsetPosition = rightFootIKPosition.y - transform.position.y;

        float totalOffset = (leftOffsetPosition < rightOffsetPosition) ? leftOffsetPosition: rightOffsetPosition;

        // Hold new pelvis position where we want to move to
        // Move from last to new position with certain speed
        Vector3 newPelvisPosition = _anim.bodyPosition + Vector3.up * totalOffset;
        newPelvisPosition.y = Mathf.Lerp(_lastPelvisPositionY, newPelvisPosition.y, pelvisUpAndDownSpeed);

        // Update current body position
        _anim.bodyPosition = newPelvisPosition;

        // Now the last known pelvis position in Y is the current body position in Y
        _lastPelvisPositionY = _anim.bodyPosition.y;
    }

    /// <summary>
    /// Locate the feet position via a raycast and then solving.
    /// </summary>
    /// <param name="fromSkyPosition"></param>
    /// <param name="feetIKPositions"></param>
    /// <param name="feetIKRotations"></param>
    private void FeetPositionSolver(AvatarIKGoal foot, Vector3 fromSkyPosition, ref Vector3 feetIKPositions, ref Quaternion feetIKRotations)
    {
        // To store all the info regarding the hit of the ray
        RaycastHit feetoutHit;

        // To visualize the ray
        if (showSolverDebug)
        {
            Debug.DrawLine(fromSkyPosition, fromSkyPosition + Vector3.down * (raycastDownDistance + heightFromGroundRaycast), Color.yellow);
        }

        // If the ray, starting at the sky position, goes down certain distance and hits an environment layer
        if (Physics.Raycast(fromSkyPosition, Vector3.down, out feetoutHit, raycastDownDistance + heightFromGroundRaycast, environmentLayer))
        {
            // Position the new IK feet positions parallel to the sky position, and put them where the ray intersects with the environment layer
            feetIKPositions = fromSkyPosition;
            feetIKPositions.y = feetoutHit.point.y + pelvisOffset;

            // TEST - Not that better. Go with the original way.
            //if (foot == AvatarIKGoal.LeftFoot)
            //{
            //    if (isLeftFootGrounded)
            //    {
            //        minFootY = Mathf.Min(minFootY, feetoutHit.point.y);
            //        feetIKPositions.y = minFootY + pelvisOffset;
            //    }
            //    else
            //    {
            //        feetIKPositions.y = feetoutHit.point.y + pelvisOffset;
            //        minFootY = feetIKPositions.y;
            //    }
            //}
            //else if(foot == AvatarIKGoal.RightFoot)
            //{
            //    if (isRightFootGrounded)
            //    {
            //        minFootY = Mathf.Min(minFootY, feetoutHit.point.y);
            //        feetIKPositions.y = minFootY + pelvisOffset;
            //    }
            //    else
            //    {
            //        feetIKPositions.y = feetoutHit.point.y + pelvisOffset;
            //        minFootY = feetIKPositions.y;
            //    }
            //}
           
            // This lines updates too much!
            // Creates a rotation from the (0,1,0) to the normal of where the feet is placed it in the terrain
            if (rotateWithRespectOriginalNormalTerrain)
            {
                //Vector3 normal = deformTerrainMaster.slopeNormal; // Put current normal of the terrain
                Vector3 normal = deformTerrainMaster.slopeInitialNormal; // Put original normal of the terrain
                
                feetIKRotations = Quaternion.FromToRotation(Vector3.up, normal) * transform.rotation;

                if (drawNormalHit)
                    Debug.DrawRay(feetoutHit.point, normal, Color.red);
            }
            else
            {                
                feetIKRotations = Quaternion.FromToRotation(Vector3.up, feetoutHit.normal) * transform.rotation;

                if (drawNormalHit)
                    Debug.DrawRay(feetoutHit.point, feetoutHit.normal, Color.yellow);
            }

            return;
        }

        feetIKPositions = Vector3.zero; // If we reach this, it didn't work
    }

    /// <summary>
    /// Adjust the IK target for the feet.
    /// </summary>
    /// <param name="feetPositions"></param>
    /// <param name="foot"></param>
    private void AdjustFeetTarget(ref Vector3 feetPositions, HumanBodyBones foot)
    {
        // Takes the Vector3 transform of that human bone id
        feetPositions = _anim.GetBoneTransform(foot).position;
        feetPositions.y = transform.position.y + heightFromGroundRaycast;
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        if (showGrounders)
        {
            if (isLeftToeGrounded)
            {
                //Gizmos.color = Color.green;
                Gizmos.color = new Color(0, 1, 0, 0.5f);
                Gizmos.DrawSphere(groundCheckerLeftFootToe.position + offsetGizmoToe, toeToGroundDistance * offsetRadius);
            }
            else
            {
                //Gizmos.color = Color.red;
                Gizmos.color = new Color(1, 0, 0, 0.5f);
                Gizmos.DrawSphere(groundCheckerLeftFootToe.position + offsetGizmoToe, toeToGroundDistance * offsetRadius);
            }

            if (isLeftHeelGrounded)
            {
                //Gizmos.color = Color.green;
                Gizmos.color = new Color(0, 1, 0, 0.5f);
                Gizmos.DrawSphere(groundCheckerLeftFootHeel.position + offsetGizmoHeel, heelToGroundDistance * offsetRadius);
            }
            else
            {
                //Gizmos.color = Color.red;
                Gizmos.color = new Color(1, 0, 0, 0.5f);
                Gizmos.DrawSphere(groundCheckerLeftFootHeel.position + offsetGizmoHeel, heelToGroundDistance * offsetRadius);
            }

            if (isRightToeGrounded)
            {
                //Gizmos.color = Color.green;
                Gizmos.color = new Color(0, 1, 0, 0.5f);
                Gizmos.DrawSphere(groundCheckerRightFootToe.position + offsetGizmoToe, toeToGroundDistance * offsetRadius);
            }
            else
            {
                //Gizmos.color = Color.red;
                Gizmos.color = new Color(1, 0, 0, 0.5f);
                Gizmos.DrawSphere(groundCheckerRightFootToe.position + offsetGizmoToe, toeToGroundDistance * offsetRadius);
            }

            if (isRightHeelGrounded)
            {
                //Gizmos.color = Color.green;
                Gizmos.color = new Color(0, 1, 0, 0.5f);
                Gizmos.DrawSphere(groundCheckerRightFootHeel.position + offsetGizmoHeel, heelToGroundDistance * offsetRadius);
            }
            else
            {
                //Gizmos.color = Color.red;
                Gizmos.color = new Color(1, 0, 0, 0.5f);
                Gizmos.DrawSphere(groundCheckerRightFootHeel.position + offsetGizmoHeel, heelToGroundDistance * offsetRadius);
            } 
        }

    }

    #endregion
}
