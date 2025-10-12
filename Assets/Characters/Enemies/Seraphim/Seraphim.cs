using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seraphim : MonoBehaviour
{
    public float damage = 1.0f;
    public float knockbackForce = 800.0f;
    public float moveSpeed = 50.0f;
    public float preferredDistance = 5f;   // where it likes to stay from player
    public float stopThreshold = 0.5f;     // buffer zone (to prevent constant back-forth movement)

    [Header("Enrage Settings")]
    public float enragedSpeedMultiplier = 2.0f;   // temporary speed boost
    public float enrageDuration = 1.5f;           // how long boost lasts
    private bool isEnraged = false;
    private float baseMoveSpeed;
    private float lastHealth;


    public DetectionZone detectionZone;
    Rigidbody2D rb;

    DamageableCharacter damageableCharacter;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        damageableCharacter = GetComponent<DamageableCharacter>();
        baseMoveSpeed = moveSpeed;

    if (damageableCharacter != null)
        lastHealth = damageableCharacter.Health;
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
}
