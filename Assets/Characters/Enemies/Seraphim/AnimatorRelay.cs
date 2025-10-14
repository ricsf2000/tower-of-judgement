using UnityEngine;

public class AnimatorRelay : MonoBehaviour
{
    private Animator bodyAnimator;
    private DamageableCharacter parentDamageable;

    void Start()
    {
        bodyAnimator = GetComponent<Animator>();
        parentDamageable = GetComponentInParent<DamageableCharacter>();
    }

    void Update()
    {
        if (bodyAnimator != null && parentDamageable != null)
        {
            bool alive = parentDamageable.Health > 0;
            bodyAnimator.SetBool("isAlive", alive);
        }
    }

    public void DeathEvent()
    {
        DamageableCharacter dmg = GetComponentInParent<DamageableCharacter>();
        if (dmg != null)
            dmg.RemoveEnemy();
    }

    public void RelayOnDeath()
    {
        // Get the Seraphim (or any other component with OnDeath) from parent
        var seraphim = GetComponentInParent<Seraphim>();
        if (seraphim != null)
        {
            seraphim.onDeath();
        }
    }
}
