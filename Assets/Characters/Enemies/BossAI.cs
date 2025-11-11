using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BossAI : MonoBehaviour
{
    private enum EnemyState
    {
        Idle,
        Chase,
        MeleeAttack,
        RangedAttack,
        Stunned
    }

    private enum BossPhase
    {
        Phase1,
        Phase2,
        Phase3
    }

    [Header("Phase Settings")]
    [SerializeField] private BossPhase currentPhase = BossPhase.Phase1;
    [SerializeField] private EnemyDamageable damageableCharacter;
    [SerializeField] private float phase2Threshold = 0.7f;
    [SerializeField] private float phase3Threshold = 0.3f;

    [Header("AI Settings")]
    [SerializeField] private List<Detector> detectors;
    [SerializeField] private AIData aiData;
    [SerializeField] private List<SteeringBehaviour> steeringBehaviours;
    [SerializeField] private ContextSolver movementDirectionSolver;

    [Header("Combat Settings")]
    [SerializeField] private float meleeRange = 1.5f;
    [SerializeField] private float attackDecisionDelay = 1.5f;
    [SerializeField] private float rangedDecisionCooldown = 5f;

    [Header("References")]
    [SerializeField] private Michael michael;

    [Header("Debug")]
    public bool logStates = true;

    private EnemyState currentState = EnemyState.Idle;
    private float aiTimer;
    private float rangedCooldownTimer;
    private float stunTimer;
    private Coroutine activeMeleeRoutine;
    private bool rangedRoutineActive = false;

    private Vector2 movementInput;

    private void Start()
    {
        aiTimer = attackDecisionDelay;
        rangedCooldownTimer = 0f;
    }

    private void Update()
    {
        if (damageableCharacter == null || !damageableCharacter.IsAlive)
            return;

        aiTimer -= Time.deltaTime;
        rangedCooldownTimer -= Time.deltaTime;

        // Run detection
        foreach (var detector in detectors)
            detector.Detect(aiData);

        UpdatePhase();
        UpdateAI();
    }

    // Phase Control
    private void UpdatePhase()
    {
        if (!damageableCharacter) return;

        float ratio = Mathf.Clamp01(damageableCharacter.Health / damageableCharacter.maxHealth);
        if (ratio <= phase3Threshold)
            currentPhase = BossPhase.Phase3;
        else if (ratio <= phase2Threshold)
            currentPhase = BossPhase.Phase2;
        else
            currentPhase = BossPhase.Phase1;
    }

    // Core AI
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

        Vector2 targetPos = aiData.currentTarget.position;
        float distance = Vector2.Distance(transform.position, targetPos);

        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdle();
                break;

            case EnemyState.Chase:
                HandleChase(targetPos);
                break;

            case EnemyState.MeleeAttack:
                HandleMeleeAttack(targetPos);
                break;

            case EnemyState.RangedAttack:
                HandleRangedAttack();
                break;

            // case EnemyState.Stunned:
            //     HandleStunned();
            //     break;
        }
    }

    // State Logic
    private void HandleIdle()
    {
        movementInput = Vector2.zero;
        if (aiData.currentTarget != null)
            SetState(EnemyState.Chase);
    }

    private void HandleChase(Vector2 targetPos)
    {
        michael.LookAt(targetPos);

        movementInput = movementDirectionSolver.GetDirectionToMove(steeringBehaviours, aiData);
        michael.Move(movementInput);

        // Every few seconds, decide what to do
        if (aiTimer <= 0f)
        {
            aiTimer = attackDecisionDelay;

            if (currentPhase == BossPhase.Phase1)
            {
                float choice = Random.value;

                // 70% chance melee, 30% ranged
                if (choice < 0.7f)
                {
                    SetState(EnemyState.MeleeAttack);
                    return;
                }
                else if (rangedCooldownTimer <= 0f)
                {
                    SetState(EnemyState.RangedAttack);
                    rangedCooldownTimer = rangedDecisionCooldown;
                    return;
                }
            }
        }
    }

    // Melee Attack
    private void HandleMeleeAttack(Vector2 targetPos)
    {
        // Prevent stacking multiple routines
        if (activeMeleeRoutine == null)
            activeMeleeRoutine = StartCoroutine(MeleeAttackRoutine());
    }

    private IEnumerator MeleeAttackRoutine()
    {
        Debug.Log("[BossAI] Starting melee attack sequence.");
        float attackRange = meleeRange;
        float maxApproachTime = 3f;
        float timer = 0f;

        while (aiData.currentTarget != null && Vector2.Distance(transform.position, aiData.currentTarget.position) > attackRange && timer < maxApproachTime)
        {
            michael.LookAt(aiData.currentTarget.position);
            Vector2 dir = (aiData.currentTarget.position - transform.position).normalized;
            michael.Move(dir);
            timer += Time.deltaTime;
            yield return null;
        }

        // Double-check before using again
        if (aiData == null || aiData.currentTarget == null || michael == null)
        {
            Debug.LogWarning("[BossAI] Target or Michael missing — aborting melee attack.");
            yield break;
        }

        // Stop movement
        michael.Move(Vector2.zero);

        // Perform attack
        michael.LookAt(aiData.currentTarget.position);
        michael.Attack();

        // Wait for attack + cooldown
        yield return new WaitForSeconds(michael.attackDuration + michael.attackCooldown);

        // Return to chase
        SetState(EnemyState.Chase);
        activeMeleeRoutine = null;
        Debug.Log("[BossAI] Finished melee attack.");
    }

    // Ranged Attack
    private void HandleRangedAttack()
    {
        if (rangedRoutineActive) return; // prevent multiple starts
        rangedRoutineActive = true;

        Debug.Log("[BossAI] Performing ranged volley.");
        StartCoroutine(RangedAttackRoutine());
    }

    private IEnumerator RangedAttackRoutine()
    {
        yield return StartCoroutine(michael.CornerRangedAttack());
        SetState(EnemyState.Chase);
        rangedRoutineActive = false;
    }

    // Stun
    // private void HandleStunned()
    // {
    //     stunTimer -= Time.deltaTime;
    //     if (stunTimer <= 0f)
    //     {
    //         SetState(EnemyState.Chase);
    //     }
    //     michael.Move(Vector2.zero);
    // }

    // public void ApplyStun(float duration)
    // {
    //     stunTimer = duration;
    //     michael.CancelAttack();
    //     SetState(EnemyState.Stunned);
    // }

    // Set the current state
    private void SetState(EnemyState newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;
        if (logStates)
            Debug.Log($"[BossAI] {name} entered {currentState}");
    }
}
