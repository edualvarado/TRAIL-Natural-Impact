/****************************************************
 * File: ExportHeightMap.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 01/10/2020
   * Project: Foot2Trail
   * Last update: 24/02/2023
*****************************************************/

using AsyncIO;
using NetMQ;
using NetMQ.Sockets;
using UnityEngine;
using PositionBasedDynamics;

/// <summary>
///     Run() to send vegetation data.
/// </summary>
public class ExportHeightMap : RunAbleThread
{
    // Property with 1D array  
    public byte[] HeightmapBytes { get; set; }

    /// <summary>
    ///     Request message to server and receive message back.
    ///     Stop requesting when Running=false.
    /// </summary>
    protected override void Run()
    {
        ForceDotNet.Force(); // To prevent Unity freeze
        using (RequestSocket client = new RequestSocket())
        {
            // Connect the socket to the address
            client.Connect("tcp://localhost:5558");

            // Set a maximum (for testing only)
            for (int i = 0; i < 1000000 && Running; i++)
            {
                /* Examples for sending data
                // Example 1: Sending string
                Debug.Log("Sending Hello");
                client.SendFrame("Hello");

                // Example 2: Sending byte array
                byte[] myByteArray = new byte[10];
                client.SendFrame(myByteArray);
                */

                // Sending converted 2D array as byte array
                client.SendFrame(TerrainDeformationMaster.HeightMapBytes);

                /*
                 * Receiving data
                 * ReceiveFrameString() blocks the thread until you receive the string, but TryReceiveFrameString() do not block the thread. 
                 * You can try commenting one and see what the other does, try to reason why Unity freezes when you use ReceiveFrameString() and play and stop the scene without running the server.
                 * string message = client.ReceiveFrameString();
                 * Debug.Log("Received: " + message);
                 */

                //string message = null;
                //bool gotMessage = false;
                //while (Running)
                //{
                //    gotMessage = client.TryReceiveFrameString(out message); // True if it's successful
                //    if (gotMessage) break;
                //}

                //if (gotMessage) Debug.Log("Received " + message);

                byte[] message = null;
                bool gotMessage = false;

                while (Running)
                {
                    gotMessage = client.TryReceiveFrameBytes(out message); // True if it's successful
                    if (gotMessage) break;
                }

                if (gotMessage)
                {
                    HeightmapBytes = message;
                    Debug.Log("Received Heightmap: " + message);
                }
            }
        }

        NetMQConfig.Cleanup(); // To prevent Unity freeze
    }
}

