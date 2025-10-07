using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float damage = 1.0f;
    public float knockbackForce = 800.0f;
    
    void OnCollisionEnter2D(Collision2D col)
    {
        Collider2D collider = col.collider;
        IDamageable damageable = collider.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            //Calculate distance between character and enemy
            Vector2 direction = (Vector2) (collider.gameObject.transform.position - transform.position).normalized;
            Vector2 knockback = direction * knockbackForce;
            damageable.OnHit(damage, knockback);
        }

    }
}
