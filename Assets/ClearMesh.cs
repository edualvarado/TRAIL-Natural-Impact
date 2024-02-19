using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ClearMesh : MonoBehaviour
{
    [MenuItem("Light Brigade/Debug/Force Cleanup NavMesh")]
    public static void ForceCleanupNavMesh()
    {
        if (Application.isPlaying)
            return;

        UnityEngine.AI.NavMesh.RemoveAllNavMeshData();
    }
}
