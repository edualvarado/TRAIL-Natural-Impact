using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DistanceTracker : MonoBehaviour
{   
    public static byte[] DistanceBytes { get; set; }

    private Vector3 lastPosition;
    private float totalDistance;

    // Start is called before the first frame update
    void Start()
    {
        DistanceBytes = new byte[sizeof(float)];
        lastPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        totalDistance += Vector3.Distance(transform.position, lastPosition);
        lastPosition = transform.position;

        DistanceBytes = BitConverter.GetBytes(totalDistance);

        //Debug.Log("Total distance travelled: " + totalDistance);
    }
}
