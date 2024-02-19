using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PositionBasedDynamics.Collisions
{
    public class DetectCollision : MonoBehaviour
    {
        public Collision Hit { get; set; }

        private GameObject root;

        private void Awake()
        {
            root = GameObject.Find("Root");
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Save collision
            Hit = collision;

            //Debug.Log("[DetectCollision] Sphere: " + int.Parse(gameObject.name) + ": Hit " + Hit.GetContact(0).point);

            // Forward to the parent and let know a collision happened

            //BasicPBDDemo parentScript = transform.parent.GetComponent<BasicPBDDemo>();
            BasicPBDDemo parentScript = root.GetComponent<BasicPBDDemo>(); // BasicPBDDemo before VegetationCreator

            if (transform.parent.gameObject.name == "Tet")
                parentScript.CollisionFromChildBody1(Hit, this.gameObject);

            if (transform.parent.gameObject.name == "Cloth")
                parentScript.CollisionFromChildBody2(Hit, this.gameObject);

            if (transform.parent.gameObject.name == "Cloth3")
                parentScript.CollisionFromChildBody3(Hit, this.gameObject);
        }

        private void OnCollisionExit(Collision collision)
        {
            // Save collision
            //Hit = collision;

            //Debug.Log("[DetectCollision] Sphere: " + int.Parse(gameObject.name) + ": Hit " + Hit.GetContact(0).point);

            // Forward to the parent and let know a collision happened
            BasicPBDDemo parentScript = transform.parent.GetComponent<BasicPBDDemo>();

            if (transform.parent.gameObject.name == "Tet")
                parentScript.ExitCollisionFromChildBody1(this.gameObject);

            if (transform.parent.gameObject.name == "Cloth")
                parentScript.ExitCollisionFromChildBody2(this.gameObject);

            if (transform.parent.gameObject.name == "Cloth3")
                parentScript.ExitCollisionFromChildBody3(this.gameObject);
        }
    } 
}
