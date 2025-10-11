using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Rigidbody2D rb;
    public float speed = 5000.0f;
    public float damage = 1.0f;
    public float knockbackForce = 5.0f;
    public int life = 3;    // How many bounces before destroyed

    private Vector2 direction;

    public void shoot(Vector2 direction)
    {
        this.direction = direction;
        rb.linearVelocity = this.direction * speed;

        // 💣 Auto-destroy after 5 seconds
        Destroy(gameObject, 5.0f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        life--;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            var damageable = collision.gameObject.GetComponent<IDamageable>();
            if (damageable != null && damageable.Targetable && !damageable.Invincible)
            {
                // Calculate knockback direction from bullet to enemy
                Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;
                damageable.OnHit(damage, knockbackDir * knockbackForce);
            }


            Destroy(gameObject); // destroy immediately when hitting enemy
            return;
        }

        if (life <= 0)
            {
                Destroy(gameObject);
                return;
            }

        var firstContact = collision.contacts[0];
        Vector2 newVelocity = Vector2.Reflect(direction.normalized, firstContact.normal);
        shoot(newVelocity.normalized);
    }
}
