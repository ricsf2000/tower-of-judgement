using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    private PlayerAttack playerAttack;
    private PlayerSFX playerSFX;

    void Awake()
    {
        playerAttack = GetComponentInParent<PlayerAttack>();
        playerSFX = GetComponentInParent<PlayerSFX>();
    }

    // Relay functions called by Animation Events
    public void PlaySwordSwing() => playerSFX?.PlaySwordSwing();
    public void LockMovement() => playerAttack?.LockMovement();
    public void UnlockMovement() => playerAttack?.UnlockMovement();
    public void EnableNextAttack() => playerAttack?.EnableNextAttack();
    public void PlayDashFX() => playerSFX?.PlayDashFX();
    public void PlayDeathFX() => playerSFX?.PlayDeathFX();
}
