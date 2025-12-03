using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seraphim : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damage = 1.0f;
    public float knockbackForce = 800.0f;

    [Header("Movement Settings")]
    public float moveSpeed = 50.0f;
    public float preferredDistance = 5f;   // where it likes to stay from player
    public float stopThreshold = 0.5f;     // buffer zone (to prevent constant back-forth movement)
    public float maxVelocity = 3.5f;

    [Header("Enrage Settings")]
    public float enragedSpeedMultiplier = 2.0f;   // temporary speed boost
    public float enrageDuration = 1.5f;           // how long boost lasts
    private bool isEnraged = false;
    private float baseMoveSpeed;
    private float lastHealth;

    [Header("Attack Settings")]
    public float attackRange = 8f;
    public float chargeTime = 1.2f;
    public float attackCooldown = 3f;


    [Header("Laser Setup")]
    public LaserController laserPrefab;
    public Transform firePoint;   // usually the iris transform
    private Transform player;
    private bool isCharging = false;
    private bool canAttack = true;
    private Vector2 lockedDirection;
    public EyeFollow eyeFollow;

    
    private bool canMove = true;
    private bool isDead = false;

    public DetectionZone detectionZone;

    public Animator irisAnimator;

    Rigidbody2D rb;

    private EnemyDamageable damageableCharacter;

    private AudioSource audioSource;

    public AudioClip deathFX;

    [Header("Aim Line Settings")]
    public LineRenderer aimLine;
    public Color warningColor = new Color(1f, 0.2f, 0.2f, 0.4f);
    public float lineWidth = 0.05f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        damageableCharacter = GetComponent<EnemyDamageable>();
        // irisAnimator = GetComponent<Animator>();
        baseMoveSpeed = moveSpeed;

        if (damageableCharacter != null)
            lastHealth = damageableCharacter.Health;

        player = GameObject.FindGameObjectWithTag("Player").transform;
        audioSource = GetComponent<AudioSource>();

        if (aimLine != null)
        {
            aimLine.enabled = false;
            aimLine.startWidth = lineWidth;
            aimLine.endWidth = lineWidth;
            aimLine.startColor = warningColor;
            aimLine.endColor = warningColor;
        }
        
        Collider2D myCollider = GetComponent<Collider2D>();
        Collider2D[] holeColliders = FindObjectsOfType<Collider2D>();

        foreach (var holeCol in holeColliders)
        {
            if (holeCol.gameObject.layer == LayerMask.NameToLayer("GroundEdge"))
                Physics2D.IgnoreCollision(myCollider, holeCol);
        }
    }

    void Update()
    {
        if (isDead) return;

        if (!damageableCharacter.Targetable || !damageableCharacter.hasSpawned)
            return;

        // Enrage check
        if (damageableCharacter != null && damageableCharacter.Health < lastHealth && !isEnraged)
        {
            StartCoroutine(EnrageRoutine());
        }

    }

    public void Move(Vector2 moveInput)
    {
        if (isDead || isCharging) return;

        if (moveInput.sqrMagnitude > 0.01f)
        {
            rb.AddForce(moveInput.normalized * moveSpeed * Time.deltaTime, ForceMode2D.Force);
            rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxVelocity);
        }
        else
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, 0.2f);
        }
        
    }

    public void LookAt(Vector2 targetPos)
    {
        if (isDead || isCharging) return;

        Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
        if (dir.sqrMagnitude > 0.01f)
            lockedDirection = dir;
    }

    public void Attack()
    {
        if (!canAttack || isCharging || isDead)
            return;

        StartCoroutine(ChargeAndFire());
    }

    private IEnumerator EnrageRoutine()
    {
        isEnraged = true;
        moveSpeed = baseMoveSpeed * enragedSpeedMultiplier;

        yield return new WaitForSeconds(enrageDuration);

        moveSpeed = baseMoveSpeed;
        isEnraged = false;
    }

    private IEnumerator ChargeAndFire()
    {
        if (isDead) yield break;

        isCharging = true;
        canAttack = false;
        canMove = false; // stop movement during attack

        // Instantly lock direction toward player's current position
        lockedDirection = (player.position - transform.position).normalized;
        Debug.Log("[Seraphim] Locked direction at time of firing prep.");

        // Tell eye to freeze in that direction
        if (eyeFollow != null)
            eyeFollow.LockDirection(lockedDirection);

        // Show warning line in locked direction
        if (aimLine != null)
            aimLine.enabled = true;


        // Trigger charging animation
        irisAnimator.ResetTrigger("isCharging");
        irisAnimator.SetTrigger("isCharging");

        // Wait a single frame so the animation updates
        yield return null;

        // Charge-up delay (visual + sound telegraph)
        float chargeTimer = 0f;
        while (chargeTimer < chargeTime)
        {
            chargeTimer += Time.deltaTime;

            // Reposition line so it stays anchored to firePoint even if Seraphim moves
            if (aimLine != null)
                UpdateAimLine(lockedDirection);

            // Pulse line brightness
            float pulse = Mathf.PingPong(Time.time * 5f, 0.3f) + 0.7f;
            Color c = warningColor;
            c.a = pulse * 0.5f;
            aimLine.startColor = c;
            aimLine.endColor = c;

            yield return null;
        }

        // Hide warning line right before firing
        if (aimLine != null)
            aimLine.enabled = false;

        // Fire laser in frozen direction
        FireLaser(lockedDirection);
        Debug.Log("[Seraphim] Laser fired in locked direction!");

        if (eyeFollow != null)
            eyeFollow.UnlockDirection();

        // Play idle animation again
        irisAnimator.ResetTrigger("isCharging");
        irisAnimator.SetTrigger("isIdle");

        // Cooldown before next attack
        yield return new WaitForSeconds(attackCooldown);

        canMove = true;
        isCharging = false;
        canAttack = true;
    }



    private void FireLaser(Vector2 fireDir)
    {
        if (isDead) return;

        canMove = false;

        if (laserPrefab == null || firePoint == null)
            return;

        // Convert direction into rotation
        float angle = Mathf.Atan2(fireDir.y, fireDir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

        // Instantiate and align the laser
        LaserController laser = Instantiate(laserPrefab, firePoint.position, rotation, transform);
        laser.transform.right = fireDir;

        // (optional) trigger attack animation
        Debug.Log("Seraphim fired laser!");

        // Pass Seraphim's damage values to the beam
        laser.damage = damage;
        laser.knockbackForce = knockbackForce;

        StartCoroutine(moveCooldown());
    }

    private IEnumerator moveCooldown()
    {
        yield return new WaitForSeconds(1f);
        canMove = true;
    }

    public void onDeath()
    {
        NoPushing KinematicObject = GetComponent<NoPushing>();
        KinematicObject.DisableShell();

        if (isDead) return;
        isDead = true;
        StopAllCoroutines();

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
    
    private void UpdateAimLine(Vector2 direction)
    {
        if (aimLine == null || firePoint == null)
            return;

        Vector3 start = firePoint.position;
        Vector3 end = start + (Vector3)direction.normalized * attackRange;

        aimLine.positionCount = 2;
        aimLine.SetPosition(0, start);
        aimLine.SetPosition(1, end);
    }

}
