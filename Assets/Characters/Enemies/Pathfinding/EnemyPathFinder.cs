using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AIData))]
public class EnemyPathfinder : MonoBehaviour
{
    private AIData aiData;
    private List<Node> nodePath = new(); // path of Nodes
    private List<Vector2> path = new();  // path of positions
    private int currentWaypoint = 0;
    private float waypointThreshold = 0.3f;

    private void Awake()
    {
        aiData = GetComponent<AIData>();
    }

    private void Update()
    {
        if (aiData.currentTarget == null)
            return;

        // Recalculate if no path or target moved too far
        if (path.Count == 0 || 
            Vector2.Distance(path[^1], aiData.currentTarget.position) > 1f)
        {
            Node start = AStarManager.instance.FindNearestNode(transform.position);
            Node end = AStarManager.instance.FindNearestNode(aiData.currentTarget.position);
            nodePath = AStarManager.instance.GeneratePath(start, end);

            path.Clear();
            if (nodePath != null)
            {
                foreach (Node n in nodePath)
                    path.Add(n.transform.position);
            }

            currentWaypoint = 0;
        }

        // Move along path
        if (path.Count > 0 && currentWaypoint < path.Count)
        {
            Vector2 waypoint = path[currentWaypoint];
            aiData.currentWaypoint = waypoint;

            if (Vector2.Distance(transform.position, waypoint) < waypointThreshold)
                currentWaypoint++;
        }
    }
}
