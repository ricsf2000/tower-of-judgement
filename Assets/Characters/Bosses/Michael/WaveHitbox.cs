using UnityEngine;

public class WaveHitbox : MonoBehaviour, IHitbox
{
    private WaveProjectile parentProjectile;
    private float damage;

    public float Damage => damage;
    public bool canBreakObjects = true;
    public bool CanBreakObjects => canBreakObjects;

    public void SetOwner(WaveProjectile owner)
    {
        parentProjectile = owner;
    }

    public void SetDamage(float dmg)
    {
        damage = dmg;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only the player is damaged
        var player = other.GetComponent<PlayerDamageable>();
        if (player != null)
        {
            // Check if player is dashing
            var dash = player.GetComponent<PlayerDash>();
            if (dash != null && dash.IsDashing)
            {
                // Player is invulnerable; ignore hit and don't destroy the wave
                Debug.Log("[WaveHitbox] Hit player during dash — ignoring.");
                return;
            }
            
            player.OnHit(damage);
            // Debug.Log("[WaveHitbox] Hit player, destroying projectile.");
            // parentProjectile.DestroySelf();
        }
    }
}
