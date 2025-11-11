using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class WaveProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 6f;
    public float lifetime = 5f;
    public float damage = 1f;

    private Vector2 direction;
    private Rigidbody2D rb;
    private WaveHitbox hitbox;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        hitbox = GetComponentInChildren<WaveHitbox>(); // get the child hitbox
    }

    // Called by the spawner (Michael / Player)
    public void Initialize(Vector2 fireDirection)
    {
        direction = fireDirection.normalized;

        // Rotate sprite to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 180f;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Pass data to the hitbox
        if (hitbox != null)
        {
            hitbox.SetOwner(this);
            hitbox.SetDamage(damage);
        }

        // Apply force
        if (rb != null)
        {
            rb.AddForce(direction * speed, ForceMode2D.Impulse);
        }

        // Destroy automatically after lifetime
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore triggers from the child hitbox
        if (other.GetComponent<WaveHitbox>() != null)
            return;
        
        if (other.gameObject.layer == LayerMask.NameToLayer("Walls"))
        {
            Debug.Log("[WaveProjectile] Trigger hit wall, destroying.");
            Destroy(gameObject);
        }
    }

    // Called by hitbox when it connects with player
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
