/****************************************************
 * File: IKFootAdaptation.cs
   * Author: Eduardo Alvarado
   * Email: alvaradopinero.eduardo@gmail.com
   * Date: 12/01/2024
   * Project: Foot2Trail
   * Last update: 12/01/2024
*****************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class IKFootAdaptation : MonoBehaviour
{
    #region Instance Fields

    [Header("Experimental")]
    public float minDistanceFeet;
    public float maxDistanceFeet;

    [Header("Environment")]
    [SerializeField] private TerrainDeformationMaster deformTerrainMaster;
    //public LayerMask ground;
    [SerializeField] private LayerMask environmentLayer;

    [Header("Foot Solver - SET-UP")]
    public bool enableFeetIK = true;
    public bool useDefaultIKFeature = true;
    public bool useProDefaultIKFeature = true;
    [Range(0, 20f)] public float heightFromGroundRaycast = 0.2f;
    [Range(0, 20f)] public float raycastDownDistance = 1.0f;
    public float pelvisOffset = 0f;
    public float offsetFootLineLeft = 0f;
    public float offsetRisingLeftKnee;
    public float offsetFootLineRight = 0f;
    public float offsetRisingRightKnee;
    [Range(0, 1f)] public float pelvisUpAndDownSpeed = 0.3f;
    [Range(0, 1f)] public float feetToIKPositionSpeed = 0.2f;
    public bool rotateWithRespectOriginalNormalTerrain;

    [Header("Foot Solver - Debug")]
    public bool isLeftFootGrounded;
    public bool isRightFootGrounded;
    public bool showSolverDebug = true;
    public bool drawNormalHitSolver = false;

    [Header("Heel/Toe Sensors Heights/Normal - SET-UP")]
    public float heelToGroundDistance = 0.055f;
    public float toeToGroundDistance = 0.04f;
    public Transform groundCheckerLeftFootHeel;
    public Transform groundCheckerLeftFootToe;
    public Transform groundCheckerRightFootHeel;
    public Transform groundCheckerRightFootToe;
    public bool drawNormalHeelToeHit = false;
    public bool drawDistances = false;
    public float leftHeelHeight;
    public float leftToeHeight;
    public float rightHeelHeight;
    public float rightToeHeight;
    public bool isLeftHeelGrounded = false;
    public bool isLeftToeGrounded = false;
    public bool isRightHeelGrounded = false;
    public bool isRightToeGrounded = false;

    [Header("Heel/Toe Sensors Gizmos - SET-UP")]
    public bool showGrounders = true;
    public Vector3 offsetGizmoHeel;
    public Vector3 offsetGizmoToe;
    [Range(0f, 1f)] public float multiplicatorGizmoRadius;

    [Header("IK Weights - SET-UP")]
    public string leftHeelWeightAnimator = "LeftHeelWeight";
    public string leftToeWeightAnimator = "LeftToeWeight";
    public string rightHeelWeightAnimator = "RightHeelWeight";
    public string rightToeWeightAnimator = "RightToeWeight";
    [Range(0, 2f)] public float minHeelDistance = 0f;
    [Range(0, 2f)] public float maxHeelDistance = 0.01f;
    [Range(0, 2f)] public float minToeDistance = 0f;
    [Range(0, 1f)] public float maxToeDistance = 0.01f;

    [Header("IK Weights - Debug")]
    [Range(0f, 1f)] public float leftWeightHeel;
    [Range(0f, 1f)] public float leftWeightToe;
    [Range(0f, 1f)] public float rightWeightHeel;
    [Range(0f, 1f)] public float rightWeightToe;

    [Header("Foot Position - Debug")]
    public Vector3 leftFootPosition;
    public Vector3 leftFootIKPosition;
    public Vector3 rightFootPosition;
    public Vector3 rightFootIKPosition;
    public Quaternion leftFootIKRotation;
    public Quaternion rightFootIKRotation;
    public float lastPelvisPositionY;
    public float lastLeftFootPositionY;
    public float lastRightFootPositionY;


    [Header("Toe Solver - SET-UP")]
    public bool activateToeAdaptation;
    public bool activateHeelSlopeAdaptation;

    [Header("Toe Position - Debug")]
    public Quaternion currentRotationLeftToe;
    public Quaternion currentRotationRightToe;
    private Quaternion currentRotationLeftHeel;
    private Quaternion currentRotationRightHeel;

    [Header("Toe Position Uphill - Debug")]
    public float footYToeDistanceLeft;
    public float footYToeDistanceRight;


    #endregion

    #region Instance Properties

    public Vector3 LeftFootIKPosition
    {
        get { return leftFootIKPosition; }
        set { leftFootIKPosition = value; }
    }

    public Vector3 RightFootIKPosition
    {
        get { return rightFootIKPosition; }
        set { rightFootIKPosition = value; }
    }

    public float LeftHeelHeight
    {
        get { return leftHeelHeight; }
        set { leftHeelHeight = value; }
    }
    public float LeftToeHeight
    {
        get { return leftToeHeight; }
        set { leftToeHeight = value; }
    }

    public float RightHeelHeight
    {
        get { return rightHeelHeight; }
        set { rightHeelHeight = value; }
    }
    public float RightToeHeight
    {
        get { return rightToeHeight; }
        set { rightToeHeight = value; }
    }

    #endregion

    #region Read-only & Static Fields

    private Animator _anim;
    private LayerMask _environmentLayer;

    private Transform _transformLeftToe;
    private Transform _transformLeftFoot;
    private Transform _transformRightToe;
    private Transform _transformRightFoot;

    private Vector3 _leftHeelPoint;
    private Vector3 _leftToePoint;
    private Vector3 _rightHeelPoint;
    private Vector3 _rightToePoint;

    private Vector3 _leftToeNormal;
    private Vector3 _leftHeelNormal;
    private Vector3 _rightToeNormal;
    private Vector3 _rightHeelNormal;

    private float _eps = 0.001f;

    #endregion

    #region Unity Methods

    // Start is called before the first frame update
    void Start()
    {
        // Set Environment Layer
        _environmentLayer = LayerMask.GetMask("Ground");
        _anim = GetComponent<Animator>();

        _transformLeftToe = _anim.GetBoneTransform(HumanBodyBones.LeftToes);
        _transformLeftFoot = _anim.GetBoneTransform(HumanBodyBones.LeftFoot);
        _transformRightToe = _anim.GetBoneTransform(HumanBodyBones.RightToes);
        _transformRightFoot = _anim.GetBoneTransform(HumanBodyBones.RightFoot);
    }

    private void Update()
    {
        if (_anim == null) { return; }
        if (enableFeetIK == false) { return; }

        // 0. Use to estimate offsets when walking uphill
        EstimateOffsetUphill();

        // 1. Estimate distances between sensors and ground at each frame
        GetHeightsHeelToe();

        // 2. Define weights for Foot and Toe IK
        DefineIKWeightsLeft();
        DefineIKWeightsRight();

        // 3. Adjust Target for feet
        AdjustFeetTarget(ref rightFootPosition, HumanBodyBones.RightFoot);
        AdjustFeetTarget(ref leftFootPosition, HumanBodyBones.LeftFoot);

        // 4. Find a raycast to the ground to find positions
        FeetPositionSolver(AvatarIKGoal.RightFoot, rightFootPosition, ref rightFootIKPosition, ref rightFootIKRotation);
        FeetPositionSolver(AvatarIKGoal.LeftFoot, leftFootPosition, ref leftFootIKPosition, ref leftFootIKRotation);

        // 5. Check if each foot is grounded
        CheckFeetAreGrounded();

        // 6. Apply offset
        if (deformTerrainMaster.slopeInitialAngle > 0)
        {
            offsetFootLineLeft = footYToeDistanceLeft * Mathf.Tan(deformTerrainMaster.slopeInitialAngle * Mathf.Deg2Rad);
            offsetFootLineRight = footYToeDistanceRight * Mathf.Tan(deformTerrainMaster.slopeInitialAngle * Mathf.Deg2Rad);

        }
        else
        {
            offsetFootLineLeft = 0f;
            offsetFootLineRight = 0f;
        }
    }

    private void FixedUpdate()
    {

    }

    private void LateUpdate()
    {
        if (activateHeelSlopeAdaptation)
        {
            RotateLeftHeelSlope();
            RotateRightHeelSlope();
        }

        if (activateToeAdaptation)
        {
            RotateLeftToes();
            RotateRigthToes();
        }
    }


    #endregion

    #region Extract Information

    /// <summary>
    /// Estimate distances between sensors and ground.
    /// </summary>
    private void GetHeightsHeelToe()
    {
        #region Left Foot Height/Normal

        // Changed: slopeNormal by slopeInitialNormal and Mathf.Infinity by heelToGroundDistance or toeToGroundDistance
        RaycastHit hitLeftHeel;
        if (Physics.Raycast(groundCheckerLeftFootHeel.position, -deformTerrainMaster.slopeInitialNormal, out hitLeftHeel, Mathf.Infinity, environmentLayer))
        {
            _leftHeelPoint = hitLeftHeel.point;

            if (hitLeftHeel.collider.gameObject.CompareTag("Obstacle") || hitLeftHeel.collider.gameObject.CompareTag("Terrain"))
            {
                leftHeelHeight = hitLeftHeel.distance - _eps;

                // Given contact point, calculate original normal that there was in that point
                if (rotateWithRespectOriginalNormalTerrain)
                {
                    _leftHeelNormal = deformTerrainMaster.initialNormalMatrix[(int)deformTerrainMaster.World2Grid(_leftHeelPoint).x, (int)deformTerrainMaster.World2Grid(_leftHeelPoint).z];

                    if (drawNormalHeelToeHit)
                        Debug.DrawRay(_leftHeelPoint, _leftHeelNormal, Color.grey);
                }
                else
                {
                    _leftHeelNormal = hitLeftHeel.normal;

                    if (drawNormalHeelToeHit)
                        Debug.DrawRay(_leftHeelPoint, _leftHeelNormal, Color.grey);
                }
            }
        }

        // Changed: slopeNormal by slopeInitialNormal and Mathf.Infinity by heelToGroundDistance or toeToGroundDistance
        RaycastHit hitLeftToe;
        if (Physics.Raycast(groundCheckerLeftFootToe.position, -deformTerrainMaster.slopeInitialNormal, out hitLeftToe, Mathf.Infinity, environmentLayer))
        {
            _leftToePoint = hitLeftToe.point;

            if (hitLeftToe.collider.gameObject.CompareTag("Obstacle") || hitLeftToe.collider.gameObject.CompareTag("Terrain"))
            {
                leftToeHeight = hitLeftToe.distance;

                // Given contact point, calculate original normal that there was in that point
                if (rotateWithRespectOriginalNormalTerrain)
                {
                    _leftToeNormal = deformTerrainMaster.initialNormalMatrix[(int)deformTerrainMaster.World2Grid(_leftToePoint).x, (int)deformTerrainMaster.World2Grid(_leftToePoint).z];

                    if (drawNormalHeelToeHit)
                        Debug.DrawRay(_leftToePoint, _leftToeNormal, Color.grey);
                }
                else
                {
                    _leftToeNormal = hitLeftToe.normal;

                    if (drawNormalHeelToeHit)
                        Debug.DrawRay(_leftToePoint, _leftToeNormal, Color.grey);
                }
            }
        }

        #endregion

        #region Right Foot Height/Normal

        // Changed: slopeNormal by slopeInitialNormal and Mathf.Infinity by heelToGroundDistance or toeToGroundDistance
        RaycastHit hitRightHeel;
        if (Physics.Raycast(groundCheckerRightFootHeel.position, -deformTerrainMaster.slopeInitialNormal, out hitRightHeel, Mathf.Infinity, environmentLayer))
        {
            _rightHeelPoint = hitRightHeel.point;

            if (hitRightHeel.collider.gameObject.CompareTag("Obstacle") || hitRightHeel.collider.gameObject.CompareTag("Terrain"))
            {
                rightHeelHeight = hitRightHeel.distance - _eps;

                // Given contact point, calculate original normal that there was in that point
                if (rotateWithRespectOriginalNormalTerrain)
                {
                    _rightHeelNormal = deformTerrainMaster.initialNormalMatrix[(int)deformTerrainMaster.World2Grid(_rightHeelPoint).x, (int)deformTerrainMaster.World2Grid(_rightHeelPoint).z];

                    if (drawNormalHeelToeHit)
                        Debug.DrawRay(_rightHeelPoint, _rightHeelNormal, Color.grey);
                }
                else
                {
                    _rightHeelNormal = hitRightHeel.normal;

                    if (drawNormalHeelToeHit)
                        Debug.DrawRay(_rightHeelPoint, _rightHeelNormal, Color.grey);
                }
            }
        }

        // Changed: slopeNormal by slopeInitialNormal and Mathf.Infinity by heelToGroundDistance or toeToGroundDistance
        RaycastHit hitRightToe;
        if (Physics.Raycast(groundCheckerRightFootToe.position, -deformTerrainMaster.slopeInitialNormal, out hitRightToe, Mathf.Infinity, environmentLayer))
        {
            _rightToePoint = hitRightToe.point;

            if (hitRightToe.collider.gameObject.CompareTag("Obstacle") || hitRightToe.collider.gameObject.CompareTag("Terrain"))
            {
                rightToeHeight = hitRightToe.distance;

                // Given contact point, calculate original normal that there was in that point
                if (rotateWithRespectOriginalNormalTerrain)
                {
                    _rightToeNormal = deformTerrainMaster.initialNormalMatrix[(int)deformTerrainMaster.World2Grid(_rightToePoint).x, (int)deformTerrainMaster.World2Grid(_rightToePoint).z];

                    if (drawNormalHeelToeHit)
                        Debug.DrawRay(_rightToePoint, _rightToeNormal, Color.grey);
                }
                else
                {
                    _rightToeNormal = hitRightToe.normal;

                    if (drawNormalHeelToeHit)
                        Debug.DrawRay(_rightToePoint, _rightToeNormal, Color.grey);
                }
            }
        }

        #endregion

        if(drawDistances)
        {
            if (leftHeelHeight < leftToeHeight)
            {
                Debug.DrawLine(groundCheckerLeftFootHeel.position, _leftHeelPoint, Color.red);
                Debug.DrawLine(groundCheckerLeftFootToe.position, _leftToePoint, Color.blue);
            }
            else
            {
                Debug.DrawLine(groundCheckerLeftFootToe.position, _leftToePoint, Color.red);
                Debug.DrawLine(groundCheckerLeftFootHeel.position, _leftHeelPoint, Color.blue);
            }

            if (rightHeelHeight < rightToeHeight)
            {
                Debug.DrawLine(groundCheckerRightFootHeel.position, _rightHeelPoint, Color.red);
                Debug.DrawLine(groundCheckerRightFootToe.position, _rightToePoint, Color.blue);
            }
            else
            {
                Debug.DrawLine(groundCheckerRightFootToe.position, _rightToePoint, Color.red);
                Debug.DrawLine(groundCheckerRightFootHeel.position, _rightHeelPoint, Color.blue);
            }
        }
    }

    #endregion

    #region Weights Estimation

    /// <summary>
    /// Weight is estimated by looking to the respective distance between sensor and ground wrt. min and max height during gait. The closer to the ground, the larger the weight.
    /// Later, when toes are in contact, we define their weights (without applying) based on the time.
    /// This function, in theory, should replace the curve to estimate the weight.
    /// </summary>
    private void DefineIKWeightsLeft()
    {
        #region Heel Weights

        // Estimate weight based on distance
        leftWeightHeel = Mathf.Clamp(1 - ((leftHeelHeight - minHeelDistance) / ((maxHeelDistance) - minHeelDistance)), 0f, 1f);
        _anim.SetFloat(leftHeelWeightAnimator, leftWeightHeel);

        #endregion

        #region Toes Weights

        leftWeightToe = Mathf.Clamp(1 - ((leftToeHeight - minToeDistance) / ((maxToeDistance) - minToeDistance)), 0f, 1f);
        _anim.SetFloat(leftToeWeightAnimator, leftWeightToe);

        #endregion
    }

    private void DefineIKWeightsRight()
    {
        #region Heel Weights

        // Estimate weight based on distance
        rightWeightHeel = Mathf.Clamp(1 - ((rightHeelHeight - minHeelDistance) / ((maxHeelDistance) - minHeelDistance)), 0f, 1f);
        _anim.SetFloat(rightHeelWeightAnimator, rightWeightHeel);

        #endregion

        #region Toes Weights

        rightWeightToe = Mathf.Clamp(1 - ((rightToeHeight - minToeDistance) / ((maxToeDistance) - minToeDistance)), 0f, 1f);
        _anim.SetFloat(rightToeWeightAnimator, rightWeightToe);

        #endregion
    }

    #endregion

    #region Foot Solvers

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
        }

        // This lines updates too much!
        // Creates a rotation from the (0,1,0) to the normal of where the feet is placed it in the terrain
        if (rotateWithRespectOriginalNormalTerrain)
        {
            Vector3 normal = deformTerrainMaster.slopeInitialNormal; // Put original normal of the terrain
            feetIKRotations = Quaternion.FromToRotation(Vector3.up, normal) * transform.rotation;

            if (drawNormalHitSolver)
                Debug.DrawRay(feetoutHit.point, normal, Color.yellow);
        }
        else
        {
            feetIKRotations = Quaternion.FromToRotation(Vector3.up, feetoutHit.normal) * transform.rotation;

            if (drawNormalHitSolver)
                Debug.DrawRay(feetoutHit.point, feetoutHit.normal, Color.yellow);
        }

        return;

    }

    #endregion

    #region Foot Grounding

    /// <summary>
    /// Check if heel/toes sensors (and overall foot) is grounded.
    /// </summary>
    private void CheckFeetAreGrounded()
    {
        #region Left Foot

        // Coarse Foot
        if (Physics.CheckSphere(groundCheckerLeftFootToe.position, toeToGroundDistance, environmentLayer, QueryTriggerInteraction.Ignore) || Physics.CheckSphere(groundCheckerLeftFootHeel.position, heelToGroundDistance, environmentLayer, QueryTriggerInteraction.Ignore))
            isLeftFootGrounded = true;
        else
            isLeftFootGrounded = false;

        // Heel/Toe
        if (Physics.CheckSphere(groundCheckerLeftFootToe.position, toeToGroundDistance, environmentLayer, QueryTriggerInteraction.Ignore))
            isLeftToeGrounded = true;
        else
            isLeftToeGrounded = false;

        if (Physics.CheckSphere(groundCheckerLeftFootHeel.position, heelToGroundDistance, environmentLayer, QueryTriggerInteraction.Ignore))
            isLeftHeelGrounded = true;
        else
            isLeftHeelGrounded = false;

        #endregion

        #region Right Foot

        // Coarse Foot
        if (Physics.CheckSphere(groundCheckerRightFootToe.position, toeToGroundDistance, environmentLayer, QueryTriggerInteraction.Ignore) || Physics.CheckSphere(groundCheckerRightFootHeel.position, heelToGroundDistance, environmentLayer, QueryTriggerInteraction.Ignore))
            isRightFootGrounded = true;
        else
            isRightFootGrounded = false;

        // Heel/Toe
        if (Physics.CheckSphere(groundCheckerRightFootToe.position, toeToGroundDistance, environmentLayer, QueryTriggerInteraction.Ignore))
            isRightToeGrounded = true;
        else
            isRightToeGrounded = false;

        if (Physics.CheckSphere(groundCheckerRightFootHeel.position, heelToGroundDistance, environmentLayer, QueryTriggerInteraction.Ignore))
            isRightHeelGrounded = true;
        else
            isRightHeelGrounded = false;

        #endregion
    }

    #endregion

    #region Animator IK

    /// <summary>
    /// Called when IK Pass is activated.
    /// </summary>
    /// <param name="layerIndex"></param>
    private void OnAnimatorIK(int layerIndex)
    {
        if (_anim == null) { return; }

        MovePelvisHeight();

        #region Left Foot

        // LeftFoot IK Position  - Max. IK position
        if (useDefaultIKFeature)
            _anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
        else
            _anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);

        // LeftFoot IK Rotation  - for PRO feature
        if (useProDefaultIKFeature)
        {
            if (!isLeftHeelGrounded)
                _anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, _anim.GetFloat(leftHeelWeightAnimator));
            else if (isLeftHeelGrounded)
                _anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
        }

        // Move LeftFoot to the target IK position and rotation
        MoveFeetToIKPoint(AvatarIKGoal.LeftFoot, leftFootIKPosition, leftFootIKRotation, ref lastLeftFootPositionY);

        // Save current orientation at the end of the OnAnimatorIK
        currentRotationLeftToe = _transformLeftToe.localRotation;

        // Save current orientation at the end of the OnAnimatorIK
        currentRotationLeftHeel = _transformLeftFoot.localRotation;

        #endregion

        #region Right Foot

        // RightFoot IK Position  - Max. IK position
        if (useDefaultIKFeature)
            _anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
        else
            _anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);

        // RightFoot IK Rotation  - for PRO feature
        if (useProDefaultIKFeature)
        {
            if (!isRightHeelGrounded)
                _anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, _anim.GetFloat(rightHeelWeightAnimator));
            else if (isRightHeelGrounded)
                _anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);
        }

        // Move LeftFoot to the target IK position and rotation
        MoveFeetToIKPoint(AvatarIKGoal.RightFoot, rightFootIKPosition, rightFootIKRotation, ref lastRightFootPositionY);

        // Save current orientation at the end of the OnAnimatorIK
        currentRotationRightToe = _transformRightToe.localRotation;

        // Save current orientation at the end of the OnAnimatorIK
        currentRotationRightHeel = _transformRightFoot.localRotation;

        #endregion
    }

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

        // If there is a IK target in a different position (not 0 locally) than the position where we have our foot currently
        if (positionIKHolder != Vector3.zero)
        {
            // Convert the world coordinates for current/target foot positions to local coordinates with respect to the character
            targetIKPosition = transform.InverseTransformPoint(targetIKPosition);
            positionIKHolder = transform.InverseTransformPoint(positionIKHolder);

            // Calculate the translation in Y necessary to move the last foot position to the target position, by a particular speed
            float yVariable = 0f;
            if (foot == AvatarIKGoal.LeftFoot)
            {
                // TEST: Change offset between a and b given the distance between both feet. use 
                //float distance = Vector3.Distance(_transformLeftFoot.position, _transformRightFoot.position);
                ////Debug.Log("DISTANCE: " + distance);
                //float distanceNorm = Mathf.Clamp(1 - ((distance - minDistanceFeet) / (maxDistanceFeet - minDistanceFeet)), 0f, 1f);
                //offsetRisingLeftKnee = Mathf.Lerp(0, 0.15f, distanceNorm);

                yVariable = Mathf.Lerp(lastFootPositionY, positionIKHolder.y, feetToIKPositionSpeed) + offsetFootLineLeft + offsetRisingLeftKnee;
            }
            else if (foot == AvatarIKGoal.RightFoot)
            {

                // TEST: Change offset between a and b given the distance between both feet. use 
                //float distance = Vector3.Distance(_transformRightFoot.position, _transformLeftFoot.position);
                ////Debug.Log("DISTANCE: " + distance);
                //float distanceNorm = Mathf.Clamp(1 - ((distance - minDistanceFeet) / (maxDistanceFeet - minDistanceFeet)), 0f, 1f);
                //offsetRisingRightKnee = Mathf.Lerp(0, 0.15f, distanceNorm);

                yVariable = Mathf.Lerp(lastFootPositionY, positionIKHolder.y, feetToIKPositionSpeed) + offsetFootLineRight + offsetRisingRightKnee;
            }

            // Add this desired translation in Y to our current feet position
            targetIKPosition.y += (yVariable);

            // We update the last foot position in Y
            lastFootPositionY = (yVariable);

            // Convert the current foot position to world coordinates
            targetIKPosition = transform.TransformPoint(targetIKPosition);

            // Set the new goal rotation (world coordinates) for the foot
            _anim.SetIKRotation(foot, rotationIKHolder);
        }

        // Set the new goal position (world coordinates) for the foot
        _anim.SetIKPosition(foot, targetIKPosition);
    }

    #endregion

    #region Pelvis

    private void MovePelvisHeight()
    {
        if (rightFootIKPosition == Vector3.zero || leftFootIKPosition == Vector3.zero || lastPelvisPositionY == 0)
        {
            lastPelvisPositionY = _anim.bodyPosition.y;
            return;
        }

        float leftOffsetPosition = leftFootIKPosition.y - transform.position.y;
        float rightOffsetPosition = rightFootIKPosition.y - transform.position.y;

        float totalOffset = (leftOffsetPosition < rightOffsetPosition) ? leftOffsetPosition : rightOffsetPosition;

        // Hold new pelvis position where we want to move to
        // Move from last to new position with certain speed
        Vector3 newPelvisPosition = _anim.bodyPosition + Vector3.up * totalOffset;
        newPelvisPosition.y = Mathf.Lerp(lastPelvisPositionY, newPelvisPosition.y, pelvisUpAndDownSpeed);

        // Update current body position
        _anim.bodyPosition = newPelvisPosition;

        // Now the last known pelvis position in Y is the current body position in Y
        lastPelvisPositionY = _anim.bodyPosition.y;
    }

    #endregion

    #region Toes Solvers

    private void EstimateOffsetUphill()
    {

        #region Left Foot

        Vector3 globalToeEndLeft = _transformLeftFoot.position - groundCheckerLeftFootToe.position;
        Vector3 localToeEndLeft = groundCheckerLeftFootToe.InverseTransformVector(globalToeEndLeft);

        Vector3 globalProjectedVectorLeft = Vector3.ProjectOnPlane(globalToeEndLeft, Vector3.up);

        float magnitudeLeft = globalProjectedVectorLeft.magnitude;

        footYToeDistanceLeft = magnitudeLeft;

        #endregion

        #region Right Foot

        Vector3 globalToeEndRight = _transformRightFoot.position - groundCheckerRightFootToe.position;
        Vector3 localToeEndRight = groundCheckerRightFootToe.InverseTransformVector(globalToeEndRight);

        Vector3 globalProjectedVectorRight = Vector3.ProjectOnPlane(globalToeEndRight, Vector3.up);

        float magnitudeRight = globalProjectedVectorRight.magnitude;

        footYToeDistanceRight = magnitudeRight;

        #endregion
    }


    private void RotateLeftToes()
    {
        Vector3 localNormal = Quaternion.Inverse(_transformLeftFoot.rotation) * _leftToeNormal;

        Quaternion currentRotation = currentRotationLeftToe;

        // Dependent of local reference system of the toe!

        // For SMLP
        Quaternion targetRotation = Quaternion.LookRotation(Quaternion.Inverse(_transformLeftFoot.rotation) * Vector3.ProjectOnPlane(_transformLeftToe.forward, _leftToeNormal), localNormal);

        // For normal robot
        //Quaternion targetRotation = Quaternion.LookRotation(localNormal, Quaternion.Inverse(_transformLeftFoot.rotation) * Vector3.ProjectOnPlane(_transformLeftToe.up, _leftToeNormal));

        if (!isLeftToeGrounded)
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

        Quaternion currentRotation = currentRotationRightToe;

        // Dependent of local reference system of the toe!
        //Quaternion targetRotation = Quaternion.LookRotation(Quaternion.Inverse(_transformLeftFoot.rotation) * Vector3.ProjectOnPlane(_transformLeftToe.right, _leftToeNormal), -localNormal);

        // For SMLP
        Quaternion targetRotation = Quaternion.LookRotation(Quaternion.Inverse(_transformRightFoot.rotation) * Vector3.ProjectOnPlane(_transformRightToe.forward, _rightToeNormal), localNormal);

        // For normal robot
        //Quaternion targetRotation = Quaternion.LookRotation(localNormal, Quaternion.Inverse(_transformRightFoot.rotation) * Vector3.ProjectOnPlane(_transformRightToe.up, _rightToeNormal));

        if (!isRightToeGrounded)
        {
            _transformRightToe.localRotation = Quaternion.Slerp(currentRotation, targetRotation, rightWeightToe);
        }
        else if (isRightToeGrounded)
        {
            _transformRightToe.localRotation = Quaternion.Slerp(currentRotation, targetRotation, 1f);
        }
    }

    private void RotateLeftHeelSlope()
    {
        Quaternion currentRotation = currentRotationLeftHeel;

        //Debug.Log("deformTerrainMaster.slopeAngle: " + deformTerrainMaster.slopeAngle);
        Quaternion rotation = Quaternion.Euler(deformTerrainMaster.slopeAngle, 0, 0);
        //Quaternion rotation = Quaternion.Euler(proxyAngle, 0, 0);

        Quaternion targetRotation = currentRotation * rotation;
        _transformLeftFoot.localRotation = Quaternion.Slerp(currentRotation, targetRotation, 1f);
    }


    private void RotateRightHeelSlope()
    {
        Quaternion currentRotation = currentRotationRightHeel;

        //Debug.Log("deformTerrainMaster.slopeAngle: " + deformTerrainMaster.slopeAngle);
        Quaternion rotation = Quaternion.Euler(deformTerrainMaster.slopeAngle, 0, 0);
        //Quaternion rotation = Quaternion.Euler(proxyAngle, 0, 0);

        Quaternion targetRotation = currentRotation * rotation;
        _transformRightFoot.localRotation = Quaternion.Slerp(currentRotation, targetRotation, 1f);
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
                Gizmos.DrawSphere(groundCheckerLeftFootToe.position + offsetGizmoToe, toeToGroundDistance * multiplicatorGizmoRadius);
            }
            else
            {
                //Gizmos.color = Color.red;
                Gizmos.color = new Color(1, 0, 0, 0.5f);
                Gizmos.DrawSphere(groundCheckerLeftFootToe.position + offsetGizmoToe, toeToGroundDistance * multiplicatorGizmoRadius);
            }

            if (isLeftHeelGrounded)
            {
                //Gizmos.color = Color.green;
                Gizmos.color = new Color(0, 1, 0, 0.5f);
                Gizmos.DrawSphere(groundCheckerLeftFootHeel.position + offsetGizmoHeel, heelToGroundDistance * multiplicatorGizmoRadius);
            }
            else
            {
                //Gizmos.color = Color.red;
                Gizmos.color = new Color(1, 0, 0, 0.5f);
                Gizmos.DrawSphere(groundCheckerLeftFootHeel.position + offsetGizmoHeel, heelToGroundDistance * multiplicatorGizmoRadius);
            }

            if (isRightToeGrounded)
            {
                //Gizmos.color = Color.green;
                Gizmos.color = new Color(0, 1, 0, 0.5f);
                Gizmos.DrawSphere(groundCheckerRightFootToe.position + offsetGizmoToe, toeToGroundDistance * multiplicatorGizmoRadius);
            }
            else
            {
                //Gizmos.color = Color.red;
                Gizmos.color = new Color(1, 0, 0, 0.5f);
                Gizmos.DrawSphere(groundCheckerRightFootToe.position + offsetGizmoToe, toeToGroundDistance * multiplicatorGizmoRadius);
            }

            if (isRightHeelGrounded)
            {
                //Gizmos.color = Color.green;
                Gizmos.color = new Color(0, 1, 0, 0.5f);
                Gizmos.DrawSphere(groundCheckerRightFootHeel.position + offsetGizmoHeel, heelToGroundDistance * multiplicatorGizmoRadius);
            }
            else
            {
                //Gizmos.color = Color.red;
                Gizmos.color = new Color(1, 0, 0, 0.5f);
                Gizmos.DrawSphere(groundCheckerRightFootHeel.position + offsetGizmoHeel, heelToGroundDistance * multiplicatorGizmoRadius);
            }
        }
    }

    #endregion

}
