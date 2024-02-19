/****************************************************
 * File: FixPositionToFoot.cs
   * Author: Eduardo Alvarado
   * Email: alvaradopinero.eduardo@gmail.com
   * Date: 12/01/2024
   * Project: Foot2Trail
   * Last update: 12/01/2024
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixPositionToFoot : MonoBehaviour
{
    public Transform parent;
    public Transform foot;
    public Vector3 offsetPosition;
    public Vector3 offsetRotation;

    // Start is called before the first frame update
    void Start()
    {
        // Set parent to the character
        this.transform.parent = parent;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = foot.position + foot.TransformVector(offsetPosition);
        this.transform.rotation = foot.rotation * Quaternion.Euler(offsetRotation);
    }
}
