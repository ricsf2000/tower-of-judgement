using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Power : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 60f;
    public float maxVelocity = 3.5f;
    public float stopThreshold = 0.2f;

    [Header("Attack Settings")]
    public float attackRange = 1.2f;
    public float attackDuration = 1.3f;
    public float attackAnimationSpeed = 1.5f;
    public float attackCooldown = 1.5f;
    private bool canAttack = true;
    private bool isAttacking = false;
    private Coroutine activeAttackRoutine;

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

            animator.SetFloat("moveX", lastMoveDir.x);
            animator.SetFloat("moveY", lastMoveDir.y);
        }
        else
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, 0.2f);
        }

        animator.SetBool("isMoving", isMoving);
    }


    // Called by EnemyAI
    public void LookAt(Vector2 targetPos)
    {
        if (isDead || isAttacking) return;

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
        canAttack = false;
        isAttacking = true;

        animator.SetFloat("attackX", lastMoveDir.x);
        animator.SetFloat("attackY", lastMoveDir.y);
        animator.SetTrigger("attack");

        animator.speed = attackAnimationSpeed;
        yield return new WaitForSeconds(attackDuration);
        animator.speed = 1.0f;
        isAttacking = false;

        // delay simulates attack cooldown
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
        activeAttackRoutine = null;
    }

    // Called by the attack animations events in Unity
    public void LungeForward()
    {
        rb.AddForce(lastMoveDir * 15f, ForceMode2D.Impulse);
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
        animator.ResetTrigger("attack");
        animator.SetBool("isMoving", false);
        // rb.linearVelocity = Vector2.zero;

        animator.Play("Power Idle Tree", 0, 0f);

        Debug.Log($"[Power] {name}'s attack was canceled!");
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
        animator.SetFloat("moveX", dir.x);
        animator.SetFloat("moveY", dir.y);

        if (dir.sqrMagnitude > 0.01f)
        {
            lastMoveDir = dir;
            animator.SetFloat("lastMoveX", dir.x);
            animator.SetFloat("lastMoveY", dir.y);
        }
    }

}
