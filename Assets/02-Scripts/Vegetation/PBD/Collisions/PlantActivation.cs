/****************************************************
 * File: PlantActivation.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 01/10/2020
   * Project: Foot2Trail
   * Last update: 21/02/2023
*****************************************************/

using UnityEngine;

namespace PositionBasedDynamics.Collisions
{
    public class PlantActivation : MonoBehaviour
    {
        private GameObject _root;
        private VegetationCreator _parentScript;
        private bool _plantActivation;

        private void Awake()
        {
            _root = GameObject.Find("Vegetation");
        }

        private void Start()
        {
            // Bring to parent script
            _parentScript = _root.GetComponent<VegetationCreator>();
        }

        private void OnTriggerEnter(Collider other)
        {
            //_plantActivation = true;
            
            //if (other.CompareTag("ActivationVolume"))
            //{
            //    Debug.Log("[INFO] Activating plant " + this.transform.gameObject.name);
            //    _parentScript.TriggerPlantActivation(this.transform.gameObject, _plantActivation);
            //}
        }

        private void OnTriggerExit(Collider other)
        {
            //_plantActivation = false;

            //if (other.CompareTag("ActivationVolume"))
            //{
            //    Debug.Log("[INFO] Desactivating plant " + this.transform.gameObject.name);
            //    _parentScript.TriggerPlantActivation(this.transform.gameObject, _plantActivation);
            //}
        }
    } 
}
