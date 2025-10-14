using Unity.VisualScripting;
using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    public Collider2D swordCollider;

    public float damage = 3;
    public float knockbackForce = 5000f;
    // public Vector3 faceRight = new Vector3(.107f, 0.085f, 0);
    // public Vector3 faceLeft = new Vector3(-.107f, 0.085f, 0);
    // public Vector3 faceUp = new Vector3(0f, 0.179f, 0);
    // public Vector3 faceDown = new Vector3(0f, -0.041f, 0);

    [Header("Audio")]
    public AudioSource audioSource;       // plays the sound
    public AudioClip[] impactSounds;      // assign 3+ impact sounds in Inspector

    void Start()
    {
        if (swordCollider == null)
        {
            Debug.LogWarning("Sword collider not set");
        }
        audioSource = GetComponent<AudioSource>();

    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        IDamageable damageableObject = collider.GetComponent<IDamageable>();
        if (damageableObject != null)
        {
            //Calculate distance between character and enemy

            //Vector3 parentPosition = gameObject.GetComponentInParent<Transform>().position;
            Vector3 parentPosition = transform.parent.position;
            Vector2 direction = (Vector2)(collider.gameObject.transform.position - parentPosition).normalized;
            Vector2 knockback = direction * knockbackForce;

            damageableObject.OnHit(damage, knockback);
            // Debug.Log("Hit for " + damage + " points");
            Debug.Log("Hit for " + damage + " points");

            if (impactSounds.Length > 0 && audioSource != null)
            {
                // Pick a random impact sound
                int randomIndex = Random.Range(0, impactSounds.Length);
                AudioClip clip = impactSounds[randomIndex];

                // Randomize the pitch
                audioSource.pitch = Random.Range(0.95f, 1.05f);

                audioSource.volume = 0.15f;

                // Play it
                audioSource.PlayOneShot(clip);
            }
        }
    }
    
    // Debug to show the hitbox in play mode
    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (swordCollider != null)
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = swordCollider.transform.localToWorldMatrix;

            if (swordCollider is BoxCollider2D box)
            {
                Gizmos.DrawWireCube(box.offset, box.size);
            }
            else if (swordCollider is CircleCollider2D circle)
            {
                Gizmos.DrawWireSphere(circle.offset, circle.radius);
            }
        }
    }
    #endif

}
