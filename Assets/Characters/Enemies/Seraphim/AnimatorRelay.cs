using UnityEngine;

public class AnimatorRelay : MonoBehaviour
{
    private Animator bodyAnimator;
    private DamageableCharacter parentDamageable;

    void Awake()
    {
        bodyAnimator = GetComponent<Animator>();
        parentDamageable = GetComponentInParent<DamageableCharacter>();

        Debug.Log($"[AnimatorRelay] Awake() on {name} — parent HP={parentDamageable?._health}, Targetable={parentDamageable?.Targetable}");

        if (bodyAnimator != null && parentDamageable != null)
        {
            bool alive = parentDamageable.Health > 0;
            bodyAnimator.SetBool("isAlive", alive);
            Debug.Log($"[AnimatorRelay] Awake() sets isAlive={alive}");
        }
    }

    void Start()
    {
        Debug.Log($"[AnimatorRelay] Start() isAlive param = {bodyAnimator.GetBool("isAlive")}");
    }

    void Update()
    {
        if (bodyAnimator != null && parentDamageable != null)
        {
            bool alive = parentDamageable.Health > 0;
            bodyAnimator.SetBool("isAlive", alive);
        }
    }

    public void RelayOnDeath()
    {
        var dmg = GetComponentInParent<DamageableCharacter>();
        var seraphim = GetComponentInParent<Seraphim>();
        Debug.LogWarning($"[{name}] RelayOnDeath() fired! HP={dmg?._health}, Targetable={dmg?.Targetable}, Animator.isAlive={bodyAnimator.GetBool("isAlive")}");
        if (dmg != null && dmg.Health <= 0 && seraphim != null)
            seraphim.onDeath();
    }

    public void PlaySpawnAnimation()
    {
        var dmg = GetComponentInParent<DamageableCharacter>();

        // Only wave-spawned enemies should play this
        if (dmg != null && !dmg.SpawnedByWave)
            return;

        if (bodyAnimator != null)
        {
            bodyAnimator.ResetTrigger("spawn");  // clear if already set
            bodyAnimator.SetTrigger("spawn");
            Debug.Log($"[{name}] Playing spawn animation (wave-spawned)");
        }
    }

}
