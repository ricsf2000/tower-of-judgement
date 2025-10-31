using System.Collections.Generic;
using UnityEngine;

public class NPC_Controller : MonoBehaviour
{
    public Node currentNode;
    public List<Node> path;

    public PlayerController player;

    public enum StateMachine { Patrol, Engage, Evade }
    public StateMachine currentState;

    public Vector2 CurrentMoveDirection { get; private set; }
    public bool HasPath => path != null && path.Count > 0;

    private void Start()
    {
        if (currentNode == null)
        {
            currentNode = AStarManager.instance.FindNearestNode(transform.position);
            if (currentNode == null)
                Debug.LogWarning($"[{name}] No node found near start position!");
        }

        currentState = StateMachine.Patrol;
        if (path == null)
            path = new List<Node>();
    }

    private void Update()
    {
        bool playerSeen = Vector2.Distance(transform.position, player.transform.position) < 5.0f;

        // only change state when conditions actually change
        if (!playerSeen && currentState != StateMachine.Patrol)
        {
            currentState = StateMachine.Patrol;
            path.Clear();
        }
        else if (playerSeen && currentState != StateMachine.Engage)
        {
            currentState = StateMachine.Engage;
            path.Clear();
        }

        switch (currentState)
        {
            case StateMachine.Patrol:  Patrol();  break;
            case StateMachine.Engage:  Engage();  break;
            case StateMachine.Evade:   Evade();   break;
        }

        CreatePath();
    }

    void Patrol()
    {
        if (path.Count == 0)
            path = AStarManager.instance.GeneratePath(
                currentNode,
                AStarManager.instance.NodesInScene()[Random.Range(0, AStarManager.instance.NodesInScene().Length)]);
    }

    void Engage()
    {
        if (path.Count == 0)
            path = AStarManager.instance.GeneratePath(
                currentNode,
                AStarManager.instance.FindNearestNode(player.transform.position));
    }

    void Evade()
    {
        if (path.Count == 0)
            path = AStarManager.instance.GeneratePath(
                currentNode,
                AStarManager.instance.FindFurthestNode(player.transform.position));
    }

    public void CreatePath()
    {
        if (path.Count > 0)
        {
            Vector3 targetPos = path[0].transform.position;
            CurrentMoveDirection = ((Vector2)(targetPos - transform.position)).normalized;

            if (Vector2.Distance(transform.position, targetPos) < 0.1f)
            {
                currentNode = path[0];
                path.RemoveAt(0);
            }
        }
        else
        {
            CurrentMoveDirection = Vector2.zero;
        }
    }
}
