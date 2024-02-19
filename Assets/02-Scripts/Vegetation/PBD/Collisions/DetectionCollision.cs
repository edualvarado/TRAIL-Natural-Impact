/****************************************************
 * File: DetectionCollision.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 01/10/2020
   * Project: Foot2Trail
   * Last update: 21/02/2023
*****************************************************/

using UnityEngine;

namespace PositionBasedDynamics.Collisions
{
    public class DetectionCollision : MonoBehaviour
    {
        #region Instance Properties

        public VegetationCreator ParentScript { get; set; }

        public Collision Hit { get; set; }
        public float PenetrationDistance { get; set; }

        #endregion

        #region Read-only & Static Fields

        private GameObject _root;
        private VegetationCreator _parentScript;
        private bool _plantCollision;

        #endregion  

        private void Awake()
        {
            _root = GameObject.Find("Vegetation");
        }

        private void Start()
        {
            // Bring to parent script
            _parentScript = _root.GetComponent<VegetationCreator>();
        }

        private void OnCollisionEnter(Collision other)
        {
            // Only detect collisions with tagged objects
            if (other.gameObject.CompareTag("ObstacleVegetation"))
            {
                _plantCollision = true;

                //Debug.Log("Reacting against: " + other.gameObject.name);

                // Save collision
                Hit = other;
                Vector3 center = GetComponent<Collider>().bounds.center;

                // To estimate penetration distance
                ContactPoint[] contactPoints = Hit.contacts;
                ContactPoint contactPoint = contactPoints[0];
                Vector3 normal = contactPoint.normal;

                RaycastHit hitInfo;
                if (Physics.Raycast(center, -normal, out hitInfo))
                {
                    PenetrationDistance = Mathf.Abs((float)ParentScript.diameter / 2 - hitInfo.distance);
                    //Debug.Log("PenetrationDistance: " + PenetrationDistance);
                    //Debug.Log("normal: " + normal);

                    Debug.DrawRay(contactPoint.point, normal * PenetrationDistance, Color.blue);
                }
                // Send information
                ParentScript.CollisionFromChildBody(Hit, PenetrationDistance, this.gameObject, this.transform.parent.gameObject, _plantCollision); 
            }
        }
        
        private void OnCollisionExit(Collision other)
        {
            if (other.gameObject.CompareTag("ObstacleVegetation"))
            {
                _plantCollision = false;

                // Send information
                ParentScript.ExitCollisionFromChildBody(this.gameObject, this.transform.parent.gameObject, _plantCollision);
            }
        }
    }
}
