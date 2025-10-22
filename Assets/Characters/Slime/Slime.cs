using UnityEngine;

public class Slime : MonoBehaviour
{
    public float damage = 1.0f;
    public float knockbackForce = 800.0f;
    public float moveSpeed = 50.0f;

    public DetectionZone detectionZone;
    Rigidbody2D rb;

    DamageableCharacter damageableCharacter;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        damageableCharacter = GetComponent<DamageableCharacter>();
    }

    void FixedUpdate()
    {
        if (damageableCharacter.Targetable && detectionZone.detectedObjs.Count > 0)
        {
            // Calculate direction to target object
            Vector2 direction = (detectionZone.detectedObjs[0].transform.position - transform.position).normalized;

            // Move towards detected object
            rb.AddForce(direction * moveSpeed * Time.deltaTime);
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        Collider2D collider = col.collider;

        // Only deal damage if the collided object is the player
        if (!col.collider.CompareTag("Player"))
            return;
            
        IDamageable damageable = collider.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            // Offset for collision detection changes the direction where the force comes from
            Vector2 direction = (Vector2) (collider.gameObject.transform.position - transform.position).normalized;

            // Knockback is in direction of swordCollider towards collider
            Vector2 knockback = direction * knockbackForce;

            // After making sure the collider has a script that implements IDamageable, we can run OnHit implementation and pass our Vector2 force
            damageable.OnHit(damage, knockback);
        }

    }
}
