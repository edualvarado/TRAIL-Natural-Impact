/****************************************************
 * File: DetectTerrain.cs
   * Author: Eduardo Alvarado
   * Email: alvaradopinero.eduardo@gmail.com
   * Date: 12/01/2024
   * Project: Foot2Trail
   * Last update: 12/01/2024
*****************************************************/

using UnityEngine;
    
public class DetectTerrain : MonoBehaviour
{
    #region Instance Fields

    [Header("Terrain Deformation")]
    [SerializeField] private TerrainDeformationMaster deformTerrainMaster;

    [Header("Ground")]
    public Transform _groundChecker;
    public LayerMask ground;
    public bool _isGrounded = true;
    public float GroundDistance = 0.2f;

    [Header("Motion")]
    public bool isAIControlled = false;

    #endregion

    #region Instance Properties

    public Terrain CurrentTerrain { get; set; }

    #endregion

    #region Read-only & Static Fields

    private Animator _anim;
    private Vector3 _inputs;
    private float _inputMagnitude;

    #endregion

    #region Unity Methods

    // Start is called before the first frame update
    void Awake()
    {
        _anim = GetComponent<Animator>();
        CurrentTerrain = deformTerrainMaster.GetComponent<Terrain>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Get the current terrain where the character is
        if (collision.gameObject.CompareTag("Terrain"))
        {
            CurrentTerrain = collision.gameObject.GetComponent<Terrain>();
            //Debug.Log("[INFO] Collision: " + collision.gameObject.name);
        }
        else if (collision.gameObject.CompareTag("Obstacle"))
        {
            //Debug.Log("[INFO] Obstacle: " + collision.gameObject.name);
        }
        else if (collision.gameObject.CompareTag("Vegetation"))
        {
            //Debug.Log("[INFO] Vegetation: " + collision.gameObject.name);
        }

    }

    private void FixedUpdate()
    {
        // Is it grounded with the LayerMask ground?
        _isGrounded = Physics.CheckSphere(_groundChecker.position, GroundDistance, ground, QueryTriggerInteraction.Ignore);       
    }

    private void Update()
    {
        // User input
        _inputs = Vector3.zero;
        _inputs.x = Input.GetAxis("Horizontal");
        _inputs.z = Input.GetAxis("Vertical");

        // Only used as auxiliar method to detect motion
        _inputMagnitude = _inputs.sqrMagnitude;

        if ((_inputs != Vector3.zero) || isAIControlled)
            _anim.SetBool("isMoving", true);
        else
            _anim.SetBool("isMoving", false);
    }

    #endregion
}
