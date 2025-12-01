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
    private SpriteRenderer[] spriteRenderers; // Cache sprite renderers

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
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);

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
        
        // Ensure sprite renderers are enabled at the start of combat animation
        // This prevents Michael from disappearing if he flies away right after starting combat
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }
        
        foreach (var sr in spriteRenderers)
        {
            if (sr != null)
            {
                sr.enabled = true;
            }
        }
        Debug.Log("[Michael] Starting combat animation - sprite renderers ensured enabled.");
        
        // Freeze Michael's movement during starting animation
        canMove = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        // Freeze player movement during starting animation
        var player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.canMove = false;
            if (player.Rb != null)
            {
                player.Rb.linearVelocity = Vector2.zero;
            }
            
            // Disable player input
            var playerInput = FindFirstObjectByType<UnityEngine.InputSystem.PlayerInput>();
            if (playerInput != null)
            {
                playerInput.enabled = false;
            }
        }
        
        Debug.Log("[Michael] Starting combat animation - Michael and player frozen.");
    }

    // Ends the starting animation
    // Called at the end of startingCombat anim
    public void startingDone()
    {
        animator.SetTrigger("startingDone");
        
        // Ensure sprite renderers are enabled when starting animation ends
        // This fixes the issue where Michael disappears if he flies away right after starting combat
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }
        
        foreach (var sr in spriteRenderers)
        {
            if (sr != null)
            {
                sr.enabled = true;
            }
        }
        Debug.Log("[Michael] Starting combat animation finished - sprite renderers ensured enabled.");
        
        // Unfreeze Michael's movement
        canMove = true;
        
        // Unfreeze player movement
        var player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.canMove = true;
            
            // Re-enable player input
            var playerInput = FindFirstObjectByType<UnityEngine.InputSystem.PlayerInput>();
            if (playerInput != null)
            {
                playerInput.enabled = true;
            }
        }
        
        Debug.Log("[Michael] Starting combat animation finished - Michael and player unfrozen.");
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

    // Called by the attack animations events in Unity
    public void LungeForward()
    {
        rb.AddForce(lockedDirection * 15f, ForceMode2D.Impulse);
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
        
        // If corner attack happens right after starting combat animation,
        // wait for starting animation to fully complete before triggering fly away
        // This fixes the issue where the fly away animation doesn't play on the first corner attack
        if (animator != null)
        {
            float timeout = 2f;
            float waitElapsed = 0f;
            while (waitElapsed < timeout)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (!stateInfo.IsName("mic_startingCombat") && !stateInfo.IsName("mic_startingStand"))
                {
                    Debug.Log("[Michael] Starting animation has completed, proceeding with corner attack fly away.");
                    break;
                }
                waitElapsed += Time.deltaTime;
                yield return null;
            }
        }
        
        // Wait one more frame to ensure animator state has fully transitioned
        yield return null;
        
        // Ensure sprite renderers are enabled before setting flownAway
        // This fixes the issue where Michael disappears on first fly away (corner attack)
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }
        
        // Ensure "Sprite" GameObject is active
        Transform cornerSpriteTransform = transform.Find("Sprite");
        if (cornerSpriteTransform != null && !cornerSpriteTransform.gameObject.activeInHierarchy)
        {
            cornerSpriteTransform.gameObject.SetActive(true);
            Debug.LogWarning("[Michael] 'Sprite' GameObject was inactive before corner attack fly away! Re-activated.");
        }
        
        foreach (var sr in spriteRenderers)
        {
            if (sr != null)
            {
                if (!sr.gameObject.activeInHierarchy)
                {
                    sr.gameObject.SetActive(true);
                }
                sr.enabled = true;
            }
        }
        Debug.Log("[Michael] Sprite renderers ensured enabled before corner attack fly away.");
        
        // Ensure animator is enabled
        if (animator != null && !animator.enabled)
        {
            animator.enabled = true;
            Debug.LogWarning("[Michael] Animator was disabled! Re-enabled before corner attack fly away.");
        }
        
        animator.SetBool("flownAway", true);
        rb.linearVelocity = Vector2.zero;
        
        // Wait a frame for animator to process
        yield return null;
        
        // Double-check sprites after animator state change
        cornerSpriteTransform = transform.Find("Sprite");
        if (cornerSpriteTransform != null && !cornerSpriteTransform.gameObject.activeInHierarchy)
        {
            cornerSpriteTransform.gameObject.SetActive(true);
            Debug.LogWarning("[Michael] 'Sprite' GameObject was deactivated by animator in corner attack! Re-activated.");
        }
        
        foreach (var sr in spriteRenderers)
        {
            if (sr != null && (!sr.enabled || !sr.gameObject.activeInHierarchy))
            {
                if (!sr.gameObject.activeInHierarchy)
                {
                    sr.gameObject.SetActive(true);
                }
                sr.enabled = true;
                Debug.LogWarning($"[Michael] Sprite renderer was disabled in corner attack! Re-enabled: {sr.gameObject.name}");
            }
        }

        // Wait for the fly-away animation to actually complete before teleporting
        // This ensures the animation plays fully before moving to the corner
        if (animator != null)
        {
            // Wait until in the mic_flyAway state
            float timeout = 0.5f;
            float waitElapsed = 0f;
            while (waitElapsed < timeout)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.IsName("mic_flyAway"))
                {
                    Debug.Log("[Michael] Fly-away animation state reached.");
                    break;
                }
                waitElapsed += Time.deltaTime;
                yield return null;
            }
            
            // Now wait for the animation to complete (normalizedTime >= 1.0)
            timeout = 2.0f; // Max wait time for the animation
            waitElapsed = 0f;
            while (waitElapsed < timeout)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.IsName("mic_flyAway") && stateInfo.normalizedTime >= 1.0f)
                {
                    Debug.Log("[Michael] Fly-away animation completed, teleporting to corner.");
                    break;
                }
                waitElapsed += Time.deltaTime;
                
                // Continuously monitor and ensure sprite renderers stay enabled during fly-up animation
                if (spriteRenderers == null || spriteRenderers.Length == 0)
                {
                    spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
                }
                
                // Check "Sprite" GameObject
                cornerSpriteTransform = transform.Find("Sprite");
                if (cornerSpriteTransform != null && !cornerSpriteTransform.gameObject.activeInHierarchy)
                {
                    cornerSpriteTransform.gameObject.SetActive(true);
                    Debug.LogWarning($"[Michael] 'Sprite' GameObject became inactive during corner fly-up! Re-activated at {waitElapsed}s");
                }
                
                foreach (var sr in spriteRenderers)
                {
                    if (sr != null)
                    {
                        if (!sr.gameObject.activeInHierarchy)
                        {
                            sr.gameObject.SetActive(true);
                            Debug.LogWarning($"[Michael] Sprite GameObject became inactive during corner fly-up! Re-activated at {waitElapsed}s: {sr.gameObject.name}");
                        }
                        if (!sr.enabled)
                        {
                            sr.enabled = true;
                            Debug.LogWarning($"[Michael] Sprite renderer was disabled during corner fly-up! Re-enabled at {waitElapsed}s: {sr.gameObject.name}");
                        }
                    }
                }
                
                yield return null;
            }
        }
        else
        {
            // Fallback: wait a fixed duration if animator is not available
            yield return new WaitForSeconds(1.0f);
        }

        // Teleport to corner
        transform.position = targetCorner.position;

        // Land
        animator.SetBool("flownAway", false);
        
        // Wait a frame for animator to process landing state
        yield return null;
        
        // Continuously monitor and ensure sprite renderers stay enabled during landing animation
        // This prevents Michael from disappearing/reappearing during the landing transition
        float landingDuration = 1.0f;
        float landingElapsed = 0f;
        float landingCheckInterval = 0.05f;
        
        while (landingElapsed < landingDuration)
        {
            // Re-check and enable sprite renderers periodically during landing
            if (spriteRenderers == null || spriteRenderers.Length == 0)
            {
                spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            }
            
            // Check "Sprite" GameObject
            Transform landingSpriteTransform = transform.Find("Sprite");
            if (landingSpriteTransform != null && !landingSpriteTransform.gameObject.activeInHierarchy)
            {
                landingSpriteTransform.gameObject.SetActive(true);
                Debug.LogWarning($"[Michael] 'Sprite' GameObject became inactive during landing! Re-activated at {landingElapsed}s");
            }
            
            foreach (var sr in spriteRenderers)
            {
                if (sr != null)
                {
                    if (!sr.gameObject.activeInHierarchy)
                    {
                        sr.gameObject.SetActive(true);
                        Debug.LogWarning($"[Michael] Sprite GameObject became inactive during landing! Re-activated at {landingElapsed}s: {sr.gameObject.name}");
                    }
                    if (!sr.enabled)
                    {
                        sr.enabled = true;
                        Debug.LogWarning($"[Michael] Sprite renderer was disabled during landing! Re-enabled at {landingElapsed}s: {sr.gameObject.name}");
                    }
                }
            }
            
            landingElapsed += landingCheckInterval;
            yield return new WaitForSeconds(landingCheckInterval);
        }

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
        
        // Ensure sprite renderers are enabled BEFORE starting fly away
        // This is especially important if fly away happens right after starting combat animation
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }
        
        // Force enable all sprite renderers and their GameObjects
        foreach (var sr in spriteRenderers)
        {
            if (sr != null)
            {
                // Ensure GameObject is active
                if (!sr.gameObject.activeInHierarchy)
                {
                    sr.gameObject.SetActive(true);
                }
                sr.enabled = true;
            }
        }
        Debug.Log("[Michael] Sprite renderers ensured enabled before fly away.");
        
        // If animator exists, ensure it's enabled and in a valid state
        if (animator != null && !animator.enabled)
        {
            animator.enabled = true;
            Debug.LogWarning("[Michael] Animator was disabled! Re-enabled before fly away.");
        }
        
        StartCoroutine(FlyAwayRoutine());
    }

    private IEnumerator FlyAwayRoutine()
    {
        Debug.Log("[Michael] FlyAwayRoutine() started.");
        
        // Enable IFrames and stop movement
        damageableCharacter.invincibleOverride = true;

        // Ensure GameObject and all parents are active
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("[Michael] GameObject is not active in hierarchy! Activating...");
            gameObject.SetActive(true);
        }
        
        // If fly away happens right after starting combat animation,
        // wait for starting animation to fully complete
        if (animator != null)
        {
            // Wait until no longer in the starting combat state
            float timeout = 2f;
            float waitElapsed = 0f;
            while (waitElapsed < timeout)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (!stateInfo.IsName("mic_startingCombat") && !stateInfo.IsName("mic_startingStand"))
                {
                    Debug.Log("[Michael] Starting animation has completed, proceeding with fly away.");
                    break;
                }
                waitElapsed += Time.deltaTime;
                yield return null;
            }
        }
        
        // Wait one more frame to ensure animator state has fully transitioned
        yield return null;
        
        // Ensure sprite renderers are enabled BEFORE starting fly away animation
        // This fixes the issue where Michael disappears on first fly away
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            Debug.Log($"[Michael] Found {spriteRenderers.Length} sprite renderers during fly away initialization.");
        }
        
        // Force enable all sprite renderers and their GameObjects
        int enabledCount = 0;
        foreach (var sr in spriteRenderers)
        {
            if (sr != null)
            {
                // Ensure the GameObject containing the sprite renderer is active
                if (!sr.gameObject.activeInHierarchy)
                {
                    sr.gameObject.SetActive(true);
                    Debug.LogWarning($"[Michael] Sprite renderer GameObject was inactive! Activated: {sr.gameObject.name}");
                }
                
                sr.enabled = true;
                enabledCount++;
            }
        }
        Debug.Log($"[Michael] Enabled {enabledCount} sprite renderers before fly away animation.");
        
        // Wait one more frame after enabling to ensure they stay enabled
        yield return null;
        
        // Double-check they're still enabled after animator update
        foreach (var sr in spriteRenderers)
        {
            if (sr != null)
            {
                if (!sr.gameObject.activeInHierarchy)
                {
                    sr.gameObject.SetActive(true);
                    Debug.LogWarning($"[Michael] Sprite renderer GameObject became inactive after first frame! Re-activated: {sr.gameObject.name}");
                }
                if (!sr.enabled)
                {
                    sr.enabled = true;
                    Debug.LogWarning($"[Michael] Sprite renderer was disabled after first frame! Re-enabled: {sr.gameObject.name}");
                }
            }
        }

         // Disable all colliders
        foreach (var col in allColliders)
            col.enabled = false;

        flownAway = true;
        
        // Enable sprite renderers immediately
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }
        Transform mainSpriteTransform = transform.Find("Sprite");
        if (mainSpriteTransform != null)
        {
            if (!mainSpriteTransform.gameObject.activeInHierarchy)
            {
                mainSpriteTransform.gameObject.SetActive(true);
                Debug.LogWarning("[Michael] 'Sprite' GameObject was inactive before setting flownAway! Re-activated.");
            }
        }
        
        foreach (var sr in spriteRenderers)
        {
            if (sr != null)
            {
                if (!sr.gameObject.activeInHierarchy)
                {
                    sr.gameObject.SetActive(true);
                }
                sr.enabled = true;
            }
        }
        Debug.Log("[Michael] Sprite renderers forced enabled RIGHT BEFORE setting flownAway animator state.");
        
        AnimatorOverrideController overrideController = null;
        RuntimeAnimatorController originalController = null;
        
        if (animator != null)
        {
            originalController = animator.runtimeAnimatorController;
        }
        
        animator.SetBool("flownAway", true);
        
        // Wait a frame for animator to process the state change
        yield return null;
        
        // Immediately re-enable all sprites after animator processes the state change
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }
        
        foreach (var sr in spriteRenderers)
        {
            if (sr != null)
            {
                if (!sr.gameObject.activeInHierarchy)
                {
                    sr.gameObject.SetActive(true);
                    Debug.LogWarning($"[Michael] Sprite GameObject was deactivated by animator! Re-activated: {sr.gameObject.name}");
                }
                if (!sr.enabled)
                {
                    sr.enabled = true;
                    Debug.LogWarning($"[Michael] Sprite renderer was disabled by animator! Re-enabled: {sr.gameObject.name}");
                }
            }
        }
        
        // Triple-check sprite renderers after animator state change
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }
        
       Transform mainSpriteTransform2 = transform.Find("Sprite");
        if (mainSpriteTransform2 != null && !mainSpriteTransform2.gameObject.activeInHierarchy)
        {
            mainSpriteTransform2.gameObject.SetActive(true);
            Debug.LogWarning("[Michael] 'Sprite' GameObject was inactive! Re-activated.");
        }
        
        foreach (var sr in spriteRenderers)
        {
            if (sr != null)
            {
                if (!sr.gameObject.activeInHierarchy)
                {
                    sr.gameObject.SetActive(true);
                    Debug.LogWarning($"[Michael] Sprite GameObject inactive after animator state change! Activated: {sr.gameObject.name}");
                }
                if (!sr.enabled)
                {
                    sr.enabled = true;
                    Debug.LogWarning($"[Michael] Sprite renderer disabled after animator state change! Re-enabled: {sr.gameObject.name}");
                }
            }
        }
        
        // Continuously ensure sprite renderers stay enabled during fly away animation
        // This fixes the issue where Michael disappears on first fly away
        float flyAwayDuration = 2.5f;
        float elapsed = 0f;
        float checkInterval = 0.05f; // Check every 0.05 seconds (more frequent)
        
        while (elapsed < flyAwayDuration)
        {
            // Re-check and enable sprite renderers periodically
            if (spriteRenderers == null || spriteRenderers.Length == 0)
            {
                spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            }
            
            // Check the "Sprite" GameObject itself
            Transform mainSpriteTransform3 = transform.Find("Sprite");
            if (mainSpriteTransform3 != null && !mainSpriteTransform3.gameObject.activeInHierarchy)
            {
                mainSpriteTransform3.gameObject.SetActive(true);
                Debug.LogWarning($"[Michael] 'Sprite' GameObject became inactive during fly away! Re-activated at {elapsed}s");
            }
            
            foreach (var sr in spriteRenderers)
            {
                if (sr != null)
                {
                    // Check GameObject active state
                    if (!sr.gameObject.activeInHierarchy)
                    {
                        sr.gameObject.SetActive(true);
                        Debug.LogWarning($"[Michael] Sprite GameObject became inactive during fly away! Re-activated at {elapsed}s: {sr.gameObject.name}");
                    }
                    // Check sprite renderer enabled state
                    if (!sr.enabled)
                    {
                        sr.enabled = true;
                        Debug.LogWarning($"[Michael] Sprite renderer was disabled during fly away! Re-enabled at {elapsed}s: {sr.gameObject.name}");
                    }
                }
            }
            
            elapsed += checkInterval;
            yield return new WaitForSeconds(checkInterval);
        }  

        // Hide Michael after fly away animation completes
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }
        
        foreach (var sr in spriteRenderers)
        {
            if (sr != null)
            {
                sr.enabled = false;
            }
        }
        Debug.Log("[Michael] Michael hidden after fly away animation.");

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

        // Ensure sprite renderers are enabled when flying back
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }
        
        foreach (var sr in spriteRenderers)
        {
            if (sr != null)
            {
                sr.enabled = true;
            }
        }

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
