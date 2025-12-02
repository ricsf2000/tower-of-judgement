using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyAI : MonoBehaviour
{
    private enum EnemyState
    {
        Idle,
        Chase,
        Attack,
        Retreat,
        Stunned
    }

    private EnemyState currentState = EnemyState.Idle;

    [Header("Detection Settings")]
    [SerializeField] private List<Detector> detectors;
    [SerializeField] private AIData aiData;
    [SerializeField] private float detectionDelay = 0.05f;

    [Header("AI Update Settings")]
    [SerializeField] private float aiUpdateDelay = 0.06f;

    [Header("Combat Settings")]
    [SerializeField] private float attackDistance = 0.5f;
    [SerializeField] private float attackDelay = 1f;
    private float lastAttackTime = -999f;

    [SerializeField, Range(0f, 1f)] 
    private float attackChance = 0.5f;

    [SerializeField] 
    private float attackDecisionCooldown = 0.3f;

    private float nextAttackDecisionTime = 0f;

    [Header("Retreat Settings")]
    [SerializeField] private bool enableRetreat = false;
    [Range(0.1f, 0.9f)] [SerializeField] private float retreatRatio = 0.6f;

    [Header("Wall Detection")]
    [SerializeField] private float wallCheckDistance = 1f;
    [SerializeField] private LayerMask wallLayers = default;

    [Header("Steering")]
    [SerializeField] private List<SteeringBehaviour> steeringBehaviours;
    [SerializeField] private ContextSolver movementDirectionSolver;

    [Header("Events")]
    public UnityEvent<Vector2> OnMovementInput;
    public UnityEvent<Vector2> OnPointerInput;
    public UnityEvent OnAttackPressed;
    public UnityEvent OnCancelAttack;

    private Vector2 movementInput;
    private float aiTimer;
    private float detectionTimer;

    private float stunTimer = 0f;

    private void Start()
    {
        aiTimer = aiUpdateDelay;
        detectionTimer = detectionDelay;
    }

    private void Update()
    {
        // Run detection loop
        detectionTimer -= Time.deltaTime;
        if (detectionTimer <= 0f)
        {
            PerformDetection();
            detectionTimer = detectionDelay;
        }

        // Handle AI updates at fixed intervals
        aiTimer -= Time.deltaTime;
        if (aiTimer <= 0f)
        {
            aiTimer = aiUpdateDelay;
            UpdateAI();
        }

        // Send movement input every frame
        // OnMovementInput?.Invoke(movementInput);

        // if (Time.frameCount % 30 == 0) // once per half second
        //     Debug.Log($"[EnemyAI] {name} current state: {currentState}");
    }

    private void FixedUpdate()
    {
        // Send movement input every frame
        OnMovementInput?.Invoke(movementInput);
    }

    private void PerformDetection()
    {
        foreach (var detector in detectors)
            detector.Detect(aiData);
    }

    private void UpdateAI()
    {
        if (aiData.currentTarget == null)
        {
            if (aiData.GetTargetsCount() > 0)
                aiData.currentTarget = aiData.targets[0];
            else
            {
                SetState(EnemyState.Idle);
                return;
            }
        }

        // Always look at target
        OnPointerInput?.Invoke(aiData.currentTarget.position);

        float distance = Vector2.Distance(transform.position, aiData.currentTarget.position);
        float retreatDistance = attackDistance * retreatRatio;

        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdle(distance);
                break;

            case EnemyState.Chase:
                HandleChase(distance, retreatDistance);
                break;

            case EnemyState.Attack:
                HandleAttack(distance);
                break;

            case EnemyState.Retreat:
                HandleRetreat(distance, retreatDistance);
                break;

            case EnemyState.Stunned:
                HandleStunned();
                break;
        }
    }

    private void HandleIdle(float distance)
    {
        movementInput = Vector2.zero;

        if (aiData.currentTarget != null)
            SetState(EnemyState.Chase);
    }

    private void HandleChase(float distance, float retreatDistance)
    {
        if (enableRetreat && distance < retreatDistance)
        {
            SetState(EnemyState.Retreat);
            return;
        }

        // Chance-based attack trigger
        if (distance < attackDistance && Time.time >= nextAttackDecisionTime)
        {
            nextAttackDecisionTime = Time.time + attackDecisionCooldown;

            if (Random.value < attackChance)
            {
                SetState(EnemyState.Attack);
                return;
            }
        }

        movementInput = movementDirectionSolver.GetDirectionToMove(steeringBehaviours, aiData);
    }

    private void HandleAttack(float distance)
    {
        // movementInput = movementDirectionSolver.GetDirectionToMove(steeringBehaviours, aiData);
        movementInput = Vector2.zero;

        if (Time.time - lastAttackTime >= attackDelay)
        {
            lastAttackTime = Time.time;
            OnAttackPressed?.Invoke();
        }

        // Transition rules
        if (distance > attackDistance * 1.1f)
            SetState(EnemyState.Chase);
        else if (enableRetreat && distance < attackDistance * retreatRatio)
            SetState(EnemyState.Retreat);
    }

    private void HandleRetreat(float distance, float retreatDistance)
    {
        Vector2 dirToTarget = (aiData.currentTarget.position - transform.position).normalized;
        Vector2 retreatDir = -dirToTarget;
        Vector2 strafeDir = Vector2.Perpendicular(dirToTarget);

        // Raycast behind the enemy to avoid walls
        RaycastHit2D hit = Physics2D.Raycast(transform.position, retreatDir, wallCheckDistance, wallLayers);
        if (hit.collider != null)
        {
            // Wall behind → can't retreat
            Debug.DrawRay(transform.position, retreatDir * wallCheckDistance, Color.red);
            movementInput = Vector2.zero;

            // Attack if stuck
            if (Time.time - lastAttackTime >= attackDelay)
            {
                lastAttackTime = Time.time;
                OnAttackPressed?.Invoke();
            }
        }
        else
        {
            // Retreat normally
            Debug.DrawRay(transform.position, retreatDir * wallCheckDistance, Color.green);
            // movementInput = retreatDir;
            movementInput = (retreatDir * 0.7f + strafeDir * 0.3f).normalized;
        }

        // Transition rules
        if (distance > retreatDistance * 1.25f)
            SetState(EnemyState.Chase);
    }

    private void HandleStunned()
    {
        if (stunTimer == 0)
            Debug.Log($"[EnemyAI] {name} entered STUNNED state.");

        // Completely stop movement
        movementInput = Vector2.zero;

        // Decrease timer
        stunTimer -= aiUpdateDelay;

        if (stunTimer <= 0f)
        {
            Debug.Log($"[EnemyAI] {name} stun ended — returning to chase/idle.");
            // After stun ends, return to chase or idle
            if (aiData.currentTarget != null)
                SetState(EnemyState.Chase);
            else
                SetState(EnemyState.Idle);
        }
    }

    public void ApplyStun(float duration)
    {
        Debug.Log($"[EnemyAI] ApplyStun({duration}) called on {name}.");

        // if (HasActiveShield()) return;

        OnCancelAttack?.Invoke();

        stunTimer = duration;
        SetState(EnemyState.Stunned);

        Debug.Log($"[EnemyAI] {name} entering STUNNED state for {stunTimer}s.");
    }

    private void SetState(EnemyState newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;
        // Optional debug line for clarity
        // Debug.Log($"[{name}] → State: {currentState}");
    }
}
