using UnityEngine;
using UnityEngine.AI;

public class NavMeshManager : MonoBehaviour
{
    public int pathfindingIterations = 500;

    void Awake()
    {
        NavMesh.pathfindingIterationsPerFrame = pathfindingIterations;
    }
}