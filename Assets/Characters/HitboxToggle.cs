using UnityEngine;

public class HitboxToggle : MonoBehaviour
{
    [SerializeField] private Collider2D hitboxCollider;

    private void Awake()
    {
        if (hitboxCollider == null)
            hitboxCollider = GetComponent<Collider2D>();
        hitboxCollider.enabled = false; // off by default
    }

    public void EnableHitbox() => hitboxCollider.enabled = true;
    public void DisableHitbox() => hitboxCollider.enabled = false;
}
