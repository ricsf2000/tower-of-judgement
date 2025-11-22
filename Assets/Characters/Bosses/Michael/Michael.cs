using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Michael : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 60f;
    public float maxVelocity = 3.5f;
    public float stopThreshold = 0.2f;
    private bool canMove = true;

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

    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float waveAttackDuration = 1.0f;
    [SerializeField] private float projectileSpeed = 6f;
    [SerializeField] private float timeBetweenShots = 0.5f;
    private bool fireProjectileThisAttack = false;


    // Fly away settings
    [HideInInspector] public bool flownAway = false;
    [SerializeField] private EnemyWaveManager[] waveManagers;
    private int nextWaveIndex = 0;
    [SerializeField] public float landingDelayTimer = 0f;


    [Header("References")]
    public Animator animator;
    private Rigidbody2D rb;
    private EnemyDamageable damageableCharacter;
    private Collider2D[] allColliders;

    private bool isDead = false;
    private Vector2 lastMoveDir = Vector2.down;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        damageableCharacter = GetComponent<EnemyDamageable>();

        // Force-reset alive state on spawn
        if (damageableCharacter != null && animator != null)
        {
            bool alive = damageableCharacter.Health > 0;
            animator.SetBool("isAlive", alive);

            Debug.Log($"[Power] Start() synced isAlive={alive} for {name}");
        }

        allColliders = GetComponentsInChildren<Collider2D>();

        nextWaveIndex = 0;
        flownAway = false;

    }

    private void Update()
    {
        // Check for death condition
        if (!isDead && damageableCharacter != null && !damageableCharacter.IsAlive)
        {
            OnDeath();
        }

        if (landingDelayTimer > 0f)
            landingDelayTimer -= Time.deltaTime;
    }

    private void OnDisable()
    {
        EnemyWaveManager.OnAllWavesCleared -= FlyBackHandler;
    }

    private void OnDestroy()
    {
        EnemyWaveManager.OnAllWavesCleared -= FlyBackHandler;
    }


    // Called by EnemyAI
    public void Move(Vector2 moveInput)
    {
        if (isDead || isAttacking || !canMove) return;

        bool isMoving = moveInput.sqrMagnitude > 0.01f;

        if (isMoving)
        {
            rb.AddForce(moveInput.normalized * moveSpeed * Time.deltaTime, ForceMode2D.Force);
            rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxVelocity);

            animator.SetFloat("lookX", lastMoveDir.x);
            animator.SetFloat("lookY", lastMoveDir.y);
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
        fireProjectileThisAttack = true;

        yield return StartCoroutine(CornerAttackRoutine());

        isAttacking = false;
        fireProjectileThisAttack = false;
    }

    private IEnumerator CornerAttackRoutine()
    {
        // Choose random corner
        Transform targetCorner = cornerPositions.Where(c => Vector2.Distance(transform.position, c.position) > 0.5f).OrderBy(c => Random.value).First();

        Debug.Log($"[Michael] Moving to corner: {targetCorner.name}");

        // Fly up
        damageableCharacter.invincibleOverride = true;
        animator.SetBool("flownAway", true);
        rb.linearVelocity = Vector2.zero;

        // Wait for fly-up anim to finish
        yield return new WaitForSeconds(1.0f);

        // Teleport to corner
        transform.position = targetCorner.position;

        // Land
        animator.SetBool("flownAway", false);

        // Wait for landing animation to finish
        yield return new WaitForSeconds(1.0f);

        // Begin attack sequence
        var player = FindFirstObjectByType<PlayerController>();
        if (!player)
        {
            Debug.LogWarning("[Michael] No player found for ranged attack.");
            yield break;
        }

        int shots = Random.Range(5, 7);
        Debug.Log($"[Michael] Performing {shots} ranged swings.");

        for (int i = 0; i < shots; i++)
        {
            LookAt(player.transform.position);
            animator.SetTrigger("waveAttack");

            // Wait for the animation duration
            yield return new WaitForSeconds(waveAttackDuration);

            if (i < shots - 1)
                yield return new WaitForSeconds(timeBetweenShots);
        }

        yield return new WaitForSeconds(0.3f);
        damageableCharacter.invincibleOverride = false;
        Debug.Log("[Michael] Finished corner ranged volley.");
    }

    // Called during the attack animation in Unity
    public void AnimEventFireProjectile()
    {
        if (!fireProjectileThisAttack) return;

        FireProjectileAtPlayer();
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
            wave.Initialize(dir, projectileSpeed);
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


    // ============================================ PHASE 2 =========================================================================================================

    // Michael flies away and spawns a wave of enemies
    public void flyAway()
    {
        Debug.Log("[Michael] flyAway() called");
        StartCoroutine(FlyAwayRoutine());
    }

    private IEnumerator FlyAwayRoutine()
    {
        // Enable IFrames and stop movement
        damageableCharacter.invincibleOverride = true;

         // Disable all colliders
        foreach (var col in allColliders)
            col.enabled = false;

        flownAway = true;
        animator.SetBool("flownAway", true);

        // Wait until animation is finished
        yield return new WaitForSeconds(2.5f);  

        // Enable the wave manager
        // Make sure there's still waves to spawn
        if (nextWaveIndex < waveManagers.Length)
        {
            waveManagers[nextWaveIndex].gameObject.SetActive(true);
            EnemyWaveManager.OnAllWavesCleared += FlyBackHandler;
        }
        else
        {
            Debug.Log("[Michael] No more wave managers to activate.");
        }
    }

    private void FlyBackHandler()
    {
        // Unsubscribe to avoid duplicate calls
        EnemyWaveManager.OnAllWavesCleared -= FlyBackHandler;

        // Call the flyBack function
        flyBack();

        // Move to the next wave
        nextWaveIndex++;
    }

    // Called after all waves are cleared
    public void flyBack()
    {
        Debug.Log("[Michael] flyBack() called");
        StartCoroutine(FlyBackRoutine());
    }
    
    private IEnumerator FlyBackRoutine()
    {
         // Pick a landing spot near the player
        var player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            Vector2 playerPos = player.transform.position;

            float offsetX = Random.Range(0.05f,0.20f);
            float offsetY = Random.Range(0.05f,0.20f);
            Vector2 landingOffset = new Vector2(offsetX, offsetY);
            Vector2 landingPos = playerPos + landingOffset;

            transform.position = landingPos; // instantly put Michael there
        }

        canMove = false;
        flownAway = false;
        animator.SetBool("flownAway", flownAway);

        // Enable all colliders again
        foreach (var col in allColliders)
            col.enabled = true;
        
        landingDelayTimer = 1.0f;   // Delay before bossAI picks a new state
        
        yield return new WaitForSeconds(1.0f);

        canMove = true;
        damageableCharacter.invincibleOverride = false;
    }

    // ============================================ DEFEATED =========================================================================================================

    public void OnDefeat()
    {
        
    }

    public void OnDeath()
    {
        NoPushing KinematicObject = GetComponent<NoPushing>();
        KinematicObject.DisableShell();

        if (isDead) return;
        isDead = true;
        rb.linearVelocity = Vector2.zero;
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
