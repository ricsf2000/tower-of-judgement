using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPathFinder : MonoBehaviour
{
    public Transform player;                 // assign at runtime or in inspector
    public float moveSpeed = 3f;
    public float nodeReachDistance = 0.1f;

    private Rigidbody2D rb;
    private List<Node> currentPath = new List<Node>();
    private int currentIndex = 0;
    private NodeGridGenerator nodeGrid;      // reference to your node grid generator

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Find references in scene
        nodeGrid = FindFirstObjectByType<NodeGridGenerator>();
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        InvokeRepeating(nameof(UpdatePath), 0f, 1.0f); // recalc path every 1s
    }

    private void UpdatePath()
    {
        if (nodeGrid == null || player == null) return;

        Node startNode = FindNearestNode(transform.position);
        Node endNode = FindNearestNode(player.position);
        if (startNode == null || endNode == null) return;

        currentPath = AStarManager.instance.GeneratePath(startNode, endNode);
        currentIndex = 0;

        if (currentPath != null && currentPath.Count > 0)
        {
            float distToGoal = Vector2.Distance(transform.position, currentPath[^1].transform.position);
            float newDistToGoal = Vector2.Distance(transform.position, endNode.transform.position);

            // Repath only if new path is significantly better
            if (newDistToGoal < distToGoal * 0.8f)
                currentPath = AStarManager.instance.GeneratePath(startNode, endNode);
        }

    }

    private Node FindNearestNode(Vector3 worldPos)
    {
        Node nearest = null;
        float bestDist = float.MaxValue;

        foreach (Transform child in nodeGrid.transform)
        {
            Node node = child.GetComponent<Node>();
            float dist = Vector2.Distance(worldPos, node.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                nearest = node;
            }
        }
        return nearest;
    }

    private void FixedUpdate()
    {
        if (currentPath == null || currentPath.Count == 0) return;
        if (currentIndex >= currentPath.Count) return;

        Vector2 targetPos = currentPath[currentIndex].transform.position;
        Vector2 dir = targetPos - (Vector2)transform.position;
        float dist = dir.magnitude;

        if (dist > 0.001f)
        {
            dir /= dist;
            float step = Mathf.Min(moveSpeed * Time.fixedDeltaTime, dist);
            rb.MovePosition(rb.position + dir * step);
        }

        // Reached node?
        if (dist < nodeReachDistance)
        {
            rb.linearVelocity = Vector2.zero;
            currentIndex++;
        }
    }

}
