using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AIData))]
public class PathCoordinator : MonoBehaviour
{
    [Header("References")]
    public Transform player; // or any destination
    public AStarManager pathfinder;

    private AIData aiData;
    private List<Node> currentPath;
    private int currentIndex;

    [Header("Path Settings")]
    public float waypointReachThreshold = 0.2f;
    public float repathInterval = 1.0f; // seconds between recalculations
    private float repathTimer;

    private void Awake()
    {
        aiData = GetComponent<AIData>();
    }

    private void Update()
    {
        if (player == null || pathfinder == null)
            return;

        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f)
        {
            GenerateNewPath();
            repathTimer = repathInterval;
        }

        FollowPath();

        if (aiData.currentWaypoint != null)
            Debug.DrawLine(transform.position, aiData.currentWaypoint.Value, Color.yellow);
    }

    private void GenerateNewPath()
    {
        Node startNode = pathfinder.FindNearestNode(transform.position);
        Node endNode = pathfinder.FindNearestNode(player.position);

        currentPath = pathfinder.GeneratePath(startNode, endNode);
        currentIndex = 0;

        if (currentPath != null && currentPath.Count > 0)
        {
            Debug.Log($"[PathCoordinator] {name} generated path with {currentPath.Count} nodes.");
        }
        else
        {
            Debug.LogWarning($"[PathCoordinator] {name} failed to generate path!");
        }
    }

    private void FollowPath()
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            Debug.Log($"[PathCoordinator] No path");
            return;
        }

        if (currentIndex >= currentPath.Count)
        {
            Debug.Log($"[PathCoordinator] End of path");
            aiData.currentWaypoint = null;
            return;
        }

        Vector2 waypointPos = currentPath[currentIndex].transform.position;
        aiData.currentWaypoint = waypointPos;      // <-- this should always execute
        Debug.Log($"[PathCoordinator] Waypoint set to {aiData.currentWaypoint}");

        float dist = Vector2.Distance(transform.position, waypointPos);
        Debug.Log($"[PathCoordinator] Dist = {dist}");
        if (dist < waypointReachThreshold)
            currentIndex++;
    }

    private void OnDrawGizmos()
    {
        if (currentPath == null || currentPath.Count == 0)
            return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < currentPath.Count - 1; i++)
        {
            Vector3 a = currentPath[i].transform.position;
            Vector3 b = currentPath[i + 1].transform.position;
            Gizmos.DrawLine(a, b);
            Gizmos.DrawSphere(a, 0.1f);
        }
    }

}
