using UnityEngine;

public class ThroneHitbox : MonoBehaviour
{
    public Thrones parent;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!parent.IsHurling) return; // only during charge
        if (other.CompareTag("Player"))
        {
            var dmg = other.GetComponent<IDamageable>();
            if (dmg != null && dmg.Targetable && !dmg.Invincible)
            {
                Vector2 dir = (other.transform.position - parent.transform.position).normalized;
                dmg.OnHit(parent.damage, dir * parent.knockbackForce);
            }
        }
    }
}
