using UnityEngine;
using System.Collections;

public class Thrones : MonoBehaviour, IHitbox
{
    [Header("Movement Settings")]
    public float moveSpeed = 60f;
    public float maxVelocity = 3.5f;
    public float stopThreshold = 0.2f;

    [Header("Attack Settings")]
    // public float attackRange = 1.2f;
    public float attackCooldown = 2.5f;
    private bool canAttack = true;
    private bool isAttacking = false;
    public float chargeTime = 2.0f;
    public float chargeSpeed = 5f;
    private Coroutine activeAttackRoutine;
    private Vector2 lockedDirection;
    public float attackAnimationSpeed = 3f;
    [SerializeField] private int maxBounces = 5;
    private int currentBounces;
    private float lastBounceTime;
    [SerializeField] private float hurlTimeout = 2f; // seconds without bouncing before stop
    public float damage = 1.0f;
    public float knockbackForce = 5.0f;
    private Vector2 direction;
    [HideInInspector] public bool IsHurling;
    [Header("Hitbox")]
    public ThroneHitbox hitbox;
    private float originalLinearDrag;
    private float originalAngularDrag;

    public bool canBreakObjects = true;

    public float Damage => damage;
    public bool CanBreakObjects => canBreakObjects;



    [Header("References")]
    public Animator animator;
    private Rigidbody2D rb;
    private EnemyDamageable damageableCharacter;
    public EyeFollow eyeFollow;
    private Transform player;
    Collider2D myCollider;
    public TrailRenderer tr;
    public RicochetPreview ricochetPreview;
    private int enemyLayer;



    private bool isDead = false;
    private Vector2 lastMoveDir = Vector2.down;

    private AudioSource audioSource;

    public AudioClip deathFX;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        damageableCharacter = GetComponent<EnemyDamageable>();
        audioSource = GetComponent<AudioSource>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Force-reset alive state on spawn
        if (damageableCharacter != null && animator != null)
        {
            bool alive = damageableCharacter.Health > 0;
            animator.SetBool("isAlive", alive);

            Debug.Log($"[Power] Start() synced isAlive={alive} for {name}");
        }

        myCollider = GetComponent<Collider2D>();
        Collider2D[] holeColliders = FindObjectsOfType<Collider2D>();

        foreach (var holeCol in holeColliders)
        {
            if (holeCol.gameObject.layer == LayerMask.NameToLayer("GroundEdge"))
                Physics2D.IgnoreCollision(myCollider, holeCol);
        }

        enemyLayer = LayerMask.NameToLayer("Enemy");
    }

    private void Update()
    {
        // Check for death condition
        if (!isDead && damageableCharacter != null && !damageableCharacter.IsAlive)
        {
            Debug.Log($"[Thrones] Calling OnDeath() for {name}");
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

            // animator.SetFloat("moveX", lastMoveDir.x);
            // animator.SetFloat("moveY", lastMoveDir.y);
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
            animator.SetFloat("lastMoveX", dir.x);
            animator.SetFloat("lastMoveY", dir.y);
        }
    }

    // Called by EnemyAI
    public void Attack()
    {
        if (!canAttack || isDead) return;

        if (activeAttackRoutine != null)
            StopCoroutine(activeAttackRoutine);

        activeAttackRoutine = StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        if (isDead) yield break;

        canAttack = false;
        isAttacking = true;

        animator.speed = attackAnimationSpeed;

        float chargeTimer = 0f;

        // Charge-up phase: show predictive ricochet path (continuously update direction)
        while (chargeTimer < chargeTime)
        {
            chargeTimer += Time.deltaTime;

            // Update direction to track player during charge
            lockedDirection = (player.position - transform.position).normalized;

            if (ricochetPreview != null)
            {
                ricochetPreview.DrawPath(transform.position, lockedDirection);
            }

            yield return null;
        }

        // Lock the direction right before launch
        lockedDirection = (player.position - transform.position).normalized;

        // Clear ricochet preview before launching
        if (ricochetPreview != null)
            ricochetPreview.ClearPath();

        // Lock eye direction
        if (eyeFollow != null)
            eyeFollow.LockDirection(lockedDirection);

        // Disable collision with player
        Collider2D[] PlayerCollider = FindObjectsOfType<Collider2D>();
        foreach (var playerCol in PlayerCollider)
        {
            if (playerCol.gameObject.layer == LayerMask.NameToLayer("Player"))
                Physics2D.IgnoreCollision(myCollider, playerCol, true);
        }

        // Launch
        IsHurling = true;
        hitbox.enabled = true;
        tr.emitting = true;
        currentBounces = maxBounces;
        lastBounceTime = Time.time;
        StartCoroutine(HurlMonitor());

        // Store and remove drag
        originalLinearDrag = rb.linearDamping;
        originalAngularDrag = rb.angularDamping;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;

        direction = lockedDirection;
        rb.linearVelocity = direction * chargeSpeed;

        Physics2D.IgnoreLayerCollision(gameObject.layer, enemyLayer, true);

        if (eyeFollow != null)
            eyeFollow.UnlockDirection();

        animator.speed = 1.0f;

        yield return new WaitForSeconds(1.0f);

        // Enable collision with player
        foreach (var playerCol in PlayerCollider)
        {
            if (playerCol.gameObject.layer == LayerMask.NameToLayer("Player"))
                Physics2D.IgnoreCollision(myCollider, playerCol, false);
        }
    }


    private void StopHurl()
    {
        // Stop movement and restore state
        rb.linearVelocity = Vector2.zero;
        IsHurling = false;
        hitbox.enabled = false;
        tr.emitting = false;

        // Restore damping
        rb.linearDamping = originalLinearDrag;
        rb.angularDamping = originalAngularDrag;

        Physics2D.IgnoreLayerCollision(gameObject.layer, enemyLayer, false);

        // Start cooldown after bouncing ends
        StartCoroutine(AttackCooldownRoutine());
    }

    private IEnumerator AttackCooldownRoutine()
    {
        // Prevent attacking again during cooldown
        canAttack = false;

        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
        isAttacking = false;
    }

    private IEnumerator HurlMonitor()
    {
        while (IsHurling)
        {
            // If it’s been too long since the last bounce, stop
            if (Time.time - lastBounceTime > hurlTimeout)
            {
                StopHurl();
                yield break;
            }

            yield return null;
        }
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
        // animator.ResetTrigger("attack");
        // animator.SetBool("isMoving", false);
        // rb.linearVelocity = Vector2.zero;

        animator.Play("Power Idle Tree", 0, 0f);

        Debug.Log($"[Power] {name}'s attack was canceled!");
    }

    public void OnDeath()
    {
        var kinematicObject = GetComponent<NoPushing>();
        if (kinematicObject != null)
            kinematicObject.DisableShell();

        if (isDead) return;
        isDead = true;
        
        // Stop animation immediately
        animator.speed = 0f;
        
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
            // animator.SetFloat("lastMoveX", dir.x);
            // animator.SetFloat("lastMoveY", dir.y);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsHurling) return;

        lastBounceTime = Time.time;
        currentBounces--;

        if (currentBounces <= 0)
        {
            StopHurl();
            return;
        }

        // Calculate reflection
        var contact = collision.contacts[0];
        Vector2 reflectDir = Vector2.Reflect(direction.normalized, contact.normal);

        // Find direction toward player
        if (player != null)
        {
            Vector2 toPlayer = ((Vector2)player.position - (Vector2)transform.position).normalized;

            // Blend reflection and player direction
            float homingStrength = 0.7f; // 0 = pure reflection, 1 = always go straight toward player
            Vector2 blended = Vector2.Lerp(reflectDir, toPlayer, homingStrength).normalized;

            bounce(blended);
        }
        else
        {
            bounce(reflectDir.normalized);
        }
    }

    
    private void bounce(Vector2 dir)
    {
        direction = dir;
        rb.linearVelocity = direction * chargeSpeed;
    }
    
}
