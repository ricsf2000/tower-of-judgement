using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Michael : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 60f;
    public float maxVelocity = 3.5f;
    public float stopThreshold = 0.2f;

    [Header("Attack Settings")]
    public float attackDuration = 1.3f;
    public float attackAnimationSpeed = 1.5f;
    public float attackCooldown = 1.5f;
    private bool canAttack = true;
    private bool isAttacking = false;
    private Coroutine activeAttackRoutine;
    private Vector2 lockedDirection;

    [Header("Corner Positions (Assign in Inspector)")]
    [SerializeField] private List<Transform> cornerPositions;

    [Header("Projectile Prefab")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 6f;
    [SerializeField] private float timeBetweenShots = 0.5f;


    [Header("References")]
    public Animator animator;
    private Rigidbody2D rb;
    private EnemyDamageable damageableCharacter;

    private bool isDead = false;
    private Vector2 lastMoveDir = Vector2.down;

    private AudioSource audioSource;

    public AudioClip deathFX;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        damageableCharacter = GetComponent<EnemyDamageable>();
        audioSource = GetComponent<AudioSource>();

        // Force-reset alive state on spawn
        if (damageableCharacter != null && animator != null)
        {
            bool alive = damageableCharacter.Health > 0;
            animator.SetBool("isAlive", alive);

            Debug.Log($"[Power] Start() synced isAlive={alive} for {name}");
        }
    }

    private void Update()
    {
        // Check for death condition
        if (!isDead && damageableCharacter != null && !damageableCharacter.IsAlive)
        {
            OnDeath();
        }
    }

    // Called by EnemyAI
    public void Move(Vector2 moveInput)
    {
        if (isDead || isAttacking) return;

        bool isMoving = moveInput.sqrMagnitude > 0.01f;

        if (isMoving)
        {
            rb.AddForce(moveInput.normalized * moveSpeed * Time.deltaTime, ForceMode2D.Force);
            rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxVelocity);

            animator.SetFloat("lookX", lastMoveDir.x);
            animator.SetFloat("lookX", lastMoveDir.y);
        }
        else
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, 0.2f);
        }

        // animator.SetBool("isMoving", isMoving);
    }


    // Called by EnemyAI
    public void LookAt(Vector2 targetPos)
    {
        if (isDead) return;

        Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
        if (dir.sqrMagnitude > 0.01f)
        {
            lastMoveDir = dir;
            animator.SetFloat("lookX", lastMoveDir.x);
            animator.SetFloat("lookY", lastMoveDir.y);
        }
    }

    // ============================================ START =========================================================================================================
    
    // Begin the starting animation
    // Called in the timeline
    public void startCombat()
    {
        animator.SetTrigger("startCombat");
    }

    // Ends the starting animation
    // Called at the end of startingCombat anim
    public void startingDone()
    {
        animator.SetTrigger("startingDone");
    }

    // ============================================ PHASE 1 =========================================================================================================

    // Called by EnemyAI
    public void Attack()
    {
        if (!canAttack || isDead) return;

        if (activeAttackRoutine != null)
            StopCoroutine(activeAttackRoutine);

        lockedDirection = lastMoveDir;
        activeAttackRoutine = StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        canAttack = false;
        isAttacking = true;

        animator.SetFloat("attackX", lastMoveDir.x);
        animator.SetFloat("attackY", lastMoveDir.y);
        animator.SetTrigger("swordAttack");

        animator.speed = attackAnimationSpeed;
        yield return new WaitForSeconds(attackDuration);
        animator.speed = 1.0f;
        isAttacking = false;

        // delay simulates attack cooldown
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
        activeAttackRoutine = null;
    }

    public void CancelAttack()
    {
        if (activeAttackRoutine != null)
        {
            StopCoroutine(activeAttackRoutine);
            activeAttackRoutine = null;
        }

        isAttacking = false;
        canAttack = true;
        animator.ResetTrigger("swordAttack");
        animator.SetBool("isMoving", false);
        // rb.linearVelocity = Vector2.zero;

        animator.Play("Michael Idle Tree", 0, 0f);

        Debug.Log($"[Michael] {name}'s attack was canceled!");
    }


    // Corner range attack
    public IEnumerator CornerRangedAttack()
    {
        // If already attacking, skip starting another volley
        if (isDead || isAttacking)
        {
            Debug.LogWarning("[Michael] Tried to start ranged attack while already attacking.");
            yield break;
        }

        isAttacking = true;
        yield return StartCoroutine(CornerAttackRoutine());
        isAttacking = false;
    }

    private IEnumerator CornerAttackRoutine()
    {
        // Choose random corner
        Transform targetCorner = cornerPositions[Random.Range(0, cornerPositions.Count)];
        Debug.Log($"[Michael] Moving to corner: {targetCorner.name}");

        // Move quickly to that corner
        float moveSpeed = 8f;
        float stopDistance = 0.1f;

        while (Vector2.Distance(transform.position, targetCorner.position) > stopDistance)
        {
            Vector2 dir = (targetCorner.position - transform.position).normalized;
            rb.linearVelocity = dir * moveSpeed;
            LookAt(targetCorner.position);
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.25f);

        // Begin attack sequence
        var player = FindFirstObjectByType<PlayerController>();
        if (!player)
        {
            Debug.LogWarning("[Michael] No player found for ranged attack.");
            yield break;
        }

        int shots = Random.Range(3, 6);
        Debug.Log($"[Michael] Performing {shots} ranged swings.");

        for (int i = 0; i < shots; i++)
        {
            LookAt(player.transform.position);
            animator.SetTrigger("swordAttack");

            yield return new WaitForSeconds(0.67f); // Impact timing
            FireProjectileAtPlayer();

            // Only wait between swings if more remain
            if (i < shots - 1)
                yield return new WaitForSeconds(attackDuration + timeBetweenShots);
        }

        yield return new WaitForSeconds(0.3f);
        Debug.Log("[Michael] Finished corner ranged volley.");
    }




    private void FireProjectileAtPlayer()
    {
        var player = FindFirstObjectByType<PlayerController>();
        if (!player)
        {
            Debug.LogWarning("[Michael] No player found for projectile target.");
            return;
        }

        if (!projectilePrefab)
        {
            Debug.LogError("[Michael] projectilePrefab is missing in the Inspector!");
            return;
        }

        Vector2 spawnPos = transform.position;
        Vector2 dir = ((Vector2)player.transform.position - spawnPos).normalized;

        // Instantiate projectile
        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

         // Initialize projectile script
        WaveProjectile wave = proj.GetComponent<WaveProjectile>();
        if (wave != null)
        {
            wave.Initialize(dir);
        }
        else
        {
            // Fallback if using Rigidbody only
            var rb = proj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = dir * projectileSpeed;
            }
        }

        Debug.Log("[Michael] Fired projectile toward player.");
    }


    public void OnDeath()
    {
        NoPushing KinematicObject = GetComponent<NoPushing>();
        KinematicObject.DisableShell();

        if (isDead) return;
        isDead = true;
        rb.linearVelocity = Vector2.zero;

        if (audioSource != null && deathFX != null && audioSource.enabled)
        {
            audioSource.volume = 0.50f;
            audioSource.PlayOneShot(deathFX);
        }
        else
        {
            Debug.LogWarning($"[{name}] Missing or disabled AudioSource or deathFX");
        }
    }

    private void UpdateAnimator(Vector2 dir)
    {
        // animator.SetFloat("moveX", dir.x);
        // animator.SetFloat("moveY", dir.y);

        if (dir.sqrMagnitude > 0.01f)
        {
            lastMoveDir = dir;
            animator.SetFloat("lookX", dir.x);
            animator.SetFloat("lookY", dir.y);
        }
    }
}
