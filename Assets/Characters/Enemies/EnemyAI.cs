using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class EnemyAI : MonoBehaviour
{
    [SerializeField]
    private List<SteeringBehaviour> steeringBehaviours;

    [SerializeField]
    private List<Detector> detectors;

    [SerializeField]
    private AIData aiData;

    [SerializeField]
    private float detectionDelay = 0.05f, aiUpdateDelay = 0.06f, attackDelay = 1f;

    [SerializeField]
    private float attackDistance = 0.5f;

    //Inputs sent from the Enemy AI to the Enemy controller
    public UnityEvent OnAttackPressed;
    public UnityEvent<Vector2> OnMovementInput, OnPointerInput;

    [SerializeField]
    private Vector2 movementInput;

    [SerializeField]
    private ContextSolver movementDirectionSolver;

    [Header("Retreat Settings")]
    public bool enableRetreat = false;         // toggle for enemies that should back away
    [Range(0.1f, 0.9f)]
    public float retreatRatio = 0.6f;          // how close before retreat triggers

    [Header("Wall Detection Settings")]
    [SerializeField] private float wallCheckDistance = 1.0f; // How far back to check for walls
    [SerializeField] private LayerMask wallLayers = default; // layers that count as obstacles

    bool following = false;

    // Pathfinding Settings
    private NodeGridGenerator nodeGrid;
    private List<Node> currentPath;
    private int currentIndex;
    private bool usingPathfinding = false;

    private void Start()
    {
        //Detecting Player and Obstacles around
        InvokeRepeating("PerformDetection", 0, detectionDelay);

        nodeGrid = FindFirstObjectByType<NodeGridGenerator>();
    }

    private void PerformDetection()
    {
        foreach (Detector detector in detectors)
        {
            detector.Detect(aiData);
        }
    }

    private void Update()
    {
        //Enemy AI movement based on Target availability
        if (aiData.currentTarget != null)
        {
            //Looking at the Target
            OnPointerInput?.Invoke(aiData.currentTarget.position);
            if (following == false)
            {
                following = true;
                StartCoroutine(ChaseAndAttack());
            }
        }
        else if (aiData.GetTargetsCount() > 0)
        {
            //Target acquisition logic
            aiData.currentTarget = aiData.targets[0];
        }
        //Moving the Agent
        OnMovementInput?.Invoke(movementInput);
    }

    private IEnumerator ChaseAndAttack()
    {
        if (aiData.currentTarget == null)
        {
            //Stopping Logic
            Debug.Log("Stopping");
            movementInput = Vector2.zero;
            following = false;
            yield break;
        }
        else
        {
            float distance = Vector2.Distance(aiData.currentTarget.position, transform.position);
            float retreatDistance = attackDistance * retreatRatio;
            Vector2 dirToTarget = (aiData.currentTarget.position - transform.position).normalized;

            if (enableRetreat && distance < retreatDistance)
            {
                Vector2 retreatDir = -dirToTarget;

                // Raycast behind the enemy to see if there’s a wall
                RaycastHit2D hit = Physics2D.Raycast(transform.position, retreatDir, wallCheckDistance, wallLayers);

                if (hit.collider != null)
                {
                    // Wall directly behind — can't retreat
                    Debug.DrawRay(transform.position, retreatDir * wallCheckDistance, Color.red);
                    Debug.Log($"[{name}] Wall behind, forced attack mode.");

                    // Force immediate attack if stuck against wall
                    movementInput = Vector2.zero;
                    OnMovementInput?.Invoke(movementInput);

                    // Add this safety trigger
                    OnAttackPressed?.Invoke(); // Force attack even if cooldown active in Seraphim
                    yield return new WaitForSeconds(attackDelay);
                    StartCoroutine(ChaseAndAttack());
                }
                else
                {
                    // Safe to retreat normally
                    movementInput = retreatDir;
                    OnMovementInput?.Invoke(movementInput);
                    yield return new WaitForSeconds(aiUpdateDelay);
                    StartCoroutine(ChaseAndAttack());
                }
            }
            else if (distance < attackDistance)
            {
                //Attack logic
                movementInput = Vector2.zero;
                OnAttackPressed?.Invoke();
                yield return new WaitForSeconds(attackDelay);
                StartCoroutine(ChaseAndAttack());
            }
            else
            {
                //Chase logic
                // movementInput = movementDirectionSolver.GetDirectionToMove(steeringBehaviours, aiData);
                // yield return new WaitForSeconds(aiUpdateDelay);
                // StartCoroutine(ChaseAndAttack());

                // If player is visible, chase directly
                bool hasLineOfSight = aiData.targets != null && aiData.targets.Contains(aiData.currentTarget);

                if (hasLineOfSight)
                {
                    // Direct pursuit (Context Steering)
                    usingPathfinding = false;
                    movementInput = movementDirectionSolver.GetDirectionToMove(steeringBehaviours, aiData);
                    OnPointerInput?.Invoke(aiData.currentTarget.position);
                }
                else
                {
                    // Player not visible → A* pathfinding mode
                    usingPathfinding = true;

                    Node startNode = FindNearestNode(transform.position);
                    Node endNode = FindNearestNode(aiData.currentTarget.position);

                    if (startNode == null || endNode == null)
                    {
                        movementInput = Vector2.zero;
                    }
                    else
                    {
                        // Only recalc path if it's empty or target node changed
                        if (currentPath == null || currentPath.Count == 0 ||
                            currentPath[^1] != endNode)
                        {
                            currentPath = AStarManager.instance.GeneratePath(startNode, endNode);
                            currentIndex = 0;
                        }

                        if (currentPath != null && currentIndex < currentPath.Count)
                        {
                            Vector2 targetPos = currentPath[currentIndex].transform.position;
                            Vector2 dir = (targetPos - (Vector2)transform.position);
                            float dist = dir.magnitude;

                            if (dist > 0.05f)
                                movementInput = dir.normalized;
                            else
                                currentIndex++; // advance to next node
                        }
                        else
                        {
                            movementInput = Vector2.zero;
                        }

                        // Look toward movement direction
                        if (movementInput.sqrMagnitude > 0.01f)
                            OnPointerInput?.Invoke((Vector2)transform.position + movementInput);
                    }
                }

                // normalize and apply
                if (movementInput.sqrMagnitude > 1f)
                    movementInput.Normalize();

                OnMovementInput?.Invoke(movementInput);
                yield return new WaitForSeconds(aiUpdateDelay);
                StartCoroutine(ChaseAndAttack());
            }

        }

    }

    private Node FindNearestNode(Vector3 worldPos)
    {
        if (nodeGrid == null)
            return null;

        Node nearest = null;
        float bestDist = float.MaxValue;

        // Loop through all child nodes in the NodeGridGenerator’s transform
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
}