using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    [Header("Hitbox Settings")]
    public Collider2D swordCollider;
    public float damage = 3f;
    public float knockbackForce = 5000f;

    [Header("Faction Settings")]
    [Tooltip("What tag this attack should damage (e.g. 'Enemy' for player sword, 'Player' for enemy sword)")]
    public string targetTag = "Enemy";

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] impactSounds;
    [Range(0f, 1f)] public float impactVolume = 0.15f;

    private Transform parentTransform;

    void Start()
    {
        parentTransform = transform.parent;

        if (swordCollider == null)
            Debug.LogWarning($"{name}: Sword collider not set!");

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        // Ignore hits on self or same faction
        if (collider.CompareTag(transform.root.tag))
            return;

        // Only damage intended target type
        if (!collider.CompareTag(targetTag))
            return;

        IDamageable damageableObject = collider.GetComponent<IDamageable>();
        if (damageableObject == null)
            return;

        // Calculate knockback direction
        Vector2 direction = ((Vector2)collider.transform.position - (Vector2)parentTransform.position).normalized;
        Vector2 knockback = direction * knockbackForce;

        // Apply damage
        damageableObject.OnHit(damage, knockback);
        Debug.Log($"{name} hit {collider.name} for {damage} points");

        // Play impact SFX
        if (impactSounds.Length > 0 && audioSource != null)
        {
            int randomIndex = Random.Range(0, impactSounds.Length);
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.volume = impactVolume;
            audioSource.PlayOneShot(impactSounds[randomIndex]);
        }

        // Camera shake only for player attacks
        if (transform.root.CompareTag("Player") && CinemachineShake.Instance != null)
            CinemachineShake.Instance.Shake(0.4f, .5f, 0.4f);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (swordCollider != null)
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = swordCollider.transform.localToWorldMatrix;

            if (swordCollider is BoxCollider2D box)
                Gizmos.DrawWireCube(box.offset, box.size);
            else if (swordCollider is CircleCollider2D circle)
                Gizmos.DrawWireSphere(circle.offset, circle.radius);
        }
    }
#endif
}
