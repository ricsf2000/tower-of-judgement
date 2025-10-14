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

    private bool canMove = true;

    public DetectionZone detectionZone;

    public Animator irisAnimator;

    Rigidbody2D rb;

    DamageableCharacter damageableCharacter;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        damageableCharacter = GetComponent<DamageableCharacter>();
        // irisAnimator = GetComponent<Animator>();
        baseMoveSpeed = moveSpeed;

        if (damageableCharacter != null)
            lastHealth = damageableCharacter.Health;

        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (!damageableCharacter.Targetable)
            return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (!isCharging && canAttack && distanceToPlayer <= attackRange)
        {
            StartCoroutine(ChargeAndFire());
        }
    }

    void FixedUpdate()
    {
        if (!damageableCharacter.Targetable || detectionZone.detectedObjs.Count == 0)
            return;

        Transform target = detectionZone.detectedObjs[0].transform;
        Vector2 toTarget = (target.position - transform.position);
        float distance = toTarget.magnitude;
        Vector2 direction = toTarget.normalized;

        // Proportional force control
        float distanceError = distance - preferredDistance;

        // Only move if outside threshold
        if (Mathf.Abs(distanceError) > stopThreshold)
        {
            // Force scales with how far off you are — smooth push/pull
            if (canMove)
                rb.AddForce(direction * moveSpeed * distanceError);
        }

        // Cap top speed
        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, 4f);

        if (damageableCharacter != null)
        {
            if (damageableCharacter.Health < lastHealth && !isEnraged)
            {
                StartCoroutine(EnrageRoutine());
            }

            lastHealth = damageableCharacter.Health;
        }
    }

    private IEnumerator EnrageRoutine()
    {
        isEnraged = true;
        moveSpeed = baseMoveSpeed * enragedSpeedMultiplier;

        yield return new WaitForSeconds(enrageDuration);

        moveSpeed = baseMoveSpeed;
        isEnraged = false;
    }

    // void OnCollisionEnter2D(Collision2D col)
    // {
    //     Collider2D collider = col.collider;

    //     // Only deal damage if the collided object is the player
    //     if (!col.collider.CompareTag("Player"))
    //         return;

    //     IDamageable damageable = collider.gameObject.GetComponent<IDamageable>();
    //     if (damageable != null)
    //     {
    //         // Offset for collision detection changes the direction where the force comes from
    //         Vector2 direction = (Vector2) (collider.gameObject.transform.position - transform.position).normalized;

    //         // Knockback is in direction of swordCollider towards collider
    //         Vector2 knockback = direction * knockbackForce;

    //         // After making sure the collider has a script that implements IDamageable, we can run OnHit implementation and pass our Vector2 force
    //         damageable.OnHit(damage, knockback);
    //     }

    // }


    private IEnumerator ChargeAndFire()
    {
        isCharging = true;
        canAttack = false;

        // Trigger the charge animation ONCE
        irisAnimator.ResetTrigger("isCharging");
        irisAnimator.SetTrigger("isCharging");

        // Make sure the animator actually switches states this frame
        yield return null;

        // Get animation info
        AnimatorStateInfo stateInfo = irisAnimator.GetCurrentAnimatorStateInfo(0);
        float animLength = stateInfo.length;

        // Optionally sync chargeTime to animation
        // chargeTime = animLength;
        float chargeTimer = 0f;

        // Charge phase
        while (chargeTimer < chargeTime)
        {
            chargeTimer += Time.deltaTime;

            Vector2 dirToPlayer = (player.position - transform.position).normalized;
            lockedDirection = dirToPlayer;

            // When the charging animation finishes, pause it on the last frame
            stateInfo = irisAnimator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("isCharging") && stateInfo.normalizedTime >= 1f)
            {
                irisAnimator.speed = 0f;  // freeze on the last frame
            }

            yield return null;
        }

        // Resume animator speed so we can play the next animation
        irisAnimator.speed = 1f;

        // Fire the laser
        FireLaser(lockedDirection);

        // Return to Idle AFTER firing
        irisAnimator.ResetTrigger("isCharging");
        irisAnimator.SetTrigger("isIdle");

        // Wait for the laser cooldown
        yield return new WaitForSeconds(attackCooldown);

        // Ready for next cycle
        isCharging = false;
        canAttack = true;
    }



    private void FireLaser(Vector2 fireDir)
    {
        canMove = false;

        if (laserPrefab == null || firePoint == null)
            return;

        // Convert direction into rotation
        float angle = Mathf.Atan2(fireDir.y, fireDir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

        // Instantiate and align the laser
        LaserController laser = Instantiate(laserPrefab, firePoint.position, rotation);
        laser.transform.right = fireDir;

        // (optional) trigger attack animation
        Debug.Log("Seraphim fired laser!");

        // Pass Seraphim’s damage values to the beam
        laser.damage = damage;
        laser.knockbackForce = knockbackForce;

        StartCoroutine(moveCooldown());
    }
    
    private IEnumerator moveCooldown()
    {
        yield return new WaitForSeconds(1f);
        canMove = true;
    }

}
