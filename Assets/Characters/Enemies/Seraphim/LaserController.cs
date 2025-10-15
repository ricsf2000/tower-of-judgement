using UnityEngine;

public class LaserController : MonoBehaviour
{
    [Header("Laser Parts (Assign in Inspector)")]
    public Transform startPart;     // The glowing origin of the laser
    public Transform middlePart;    // The stretchable segment
    public Transform endPart;       // The tip of the laser beam

    [Header("Laser Settings")]
    public float laserLength = 6.0f;
    public float endOffset = 0.0f;
    public float stretchSmoothness = 10f;
    public float colliderLengthAdjustment = 0.1f; // tweak this value until it looks perfect

    [Header("Damage Settings")]
    public float damage = 1f;
    public float knockbackForce = 5f;

    private float currentLength;

    private BoxCollider2D boxCol;
    private SpriteRenderer middleRenderer;
    private float baseSpriteWidth; // the width of the unscaled sprite in world units

    public AudioSource audioSource;
    public AudioClip impactSound;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        boxCol = GetComponent<BoxCollider2D>();
        middleRenderer = middlePart.GetComponent<SpriteRenderer>();

        // Compute base sprite width once (unscaled)
        baseSpriteWidth = middleRenderer.sprite.bounds.size.x;

        boxCol.isTrigger = true;
        currentLength = laserLength;
    
        // Start at full length (or grow in smoothly if you prefer)
        currentLength = laserLength;
        UpdateLaserParts();
    }

    void Update()
    {
        // Smoothly interpolate toward target length if you want dynamic stretching
        currentLength = Mathf.Lerp(currentLength, laserLength, Time.deltaTime * stretchSmoothness);
        UpdateLaserParts();
        UpdateColliderLength();
    }

    private void UpdateLaserParts()
    {
        if (startPart == null || middlePart == null || endPart == null)
            return;

        // Set positions based on beam length
        startPart.localPosition = Vector3.zero;

        // Stretch the middle part horizontally
        Vector3 middleScale = middlePart.localScale;
        middleScale.x = Mathf.Max(0.01f, currentLength); // prevent negative/zero scaling
        middlePart.localScale = middleScale;

        // Make sure the middle stretches correctly from the start
        middlePart.localPosition = Vector3.zero;

        // Get the world-space width of the stretched middle section
        float middleWorldLength = middlePart.GetComponent<SpriteRenderer>().bounds.size.x;

        // Place the end part right after the middle ends
        endPart.localPosition = new Vector3(middleWorldLength, 0f, 0f);
    }

    private void UpdateColliderLength()
    {
        if (boxCol == null) return;

        // Length in local space, same math as the middlePart’s visual length
        float totalLength = baseSpriteWidth * middlePart.localScale.x;

        // Set collider size and offset directly
        float adjustedLength = Mathf.Max(0f, totalLength - colliderLengthAdjustment);
        boxCol.size = new Vector2(adjustedLength, 0.1f);
        boxCol.offset = new Vector2(adjustedLength / 2f, 0f);
    }

    // Called by Animation Event at the end of the clip
    public void OnLaserEnd()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            DamageableCharacter player = other.GetComponent<DamageableCharacter>();
            if (player != null)
            {
                Vector2 dir = (other.transform.position - transform.position).normalized;
                player.OnHit(damage, dir * knockbackForce);

                audioSource.pitch = Random.Range(0.95f, 1.05f);

                audioSource.volume = 0.15f;

                // Play it
                audioSource.PlayOneShot(impactSound);
            }
        }
    }


    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (boxCol != null)
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = boxCol.transform.localToWorldMatrix;

            if (boxCol is BoxCollider2D box)
            {
                Gizmos.DrawWireCube(box.offset, box.size);
            }
        }
    }
    #endif
}
