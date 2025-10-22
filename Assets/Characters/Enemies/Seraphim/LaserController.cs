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

    [Header("Collision Settings")]
    public LayerMask obstacleMask;

    void Start()
    {
        boxCol = GetComponent<BoxCollider2D>();
        middleRenderer = middlePart.GetComponent<SpriteRenderer>();
        baseSpriteWidth = middleRenderer.sprite.bounds.size.x;
        boxCol.isTrigger = true;

        // Initialize beam at correct starting length (avoid first-frame overshoot)
        Vector2 origin = (Vector2)startPart.position + (Vector2)(transform.right * 0.05f);
        Vector2 direction = transform.right;
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, laserLength, obstacleMask);

        float initLength = hit.collider ? hit.distance - 0.02f : laserLength;
        currentLength = Mathf.Max(0.01f, initLength);

        ApplyBeamLength(currentLength);
    }


    void Update()
    {
        // Perform a raycast forward to detect walls
        Vector2 origin = startPart.position;
        Vector2 direction = transform.right.normalized;

        // Slight forward offset to prevent micro self-hits (if the start sprite is thick)
        origin += direction * 0.05f;

        // Cast toward walls only
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, laserLength, obstacleMask);

        float targetLength;

        if (hit.collider != null)
        {
            // Subtract a tiny margin so the beam doesn’t overlap the wall visually
            targetLength = hit.distance + 1.0f;
            Debug.DrawRay(origin, direction * targetLength, Color.red);
        }
        else
        {
            targetLength = laserLength;
            Debug.DrawRay(origin, direction * targetLength, Color.green);
        }

        // Instantly set (or smoothly Lerp) to new length
        currentLength = Mathf.Lerp(currentLength, targetLength, Time.deltaTime * stretchSmoothness);

        // Apply same length to visuals & collider
        ApplyBeamLength(currentLength);
    }

    private void ApplyBeamLength(float worldLength)
    {
        // --- middle scaling ---
        Vector3 middleScale = middlePart.localScale;
        middleScale.x = worldLength / baseSpriteWidth;
        middlePart.localScale = middleScale;

        // --- end placement ---
        endPart.localPosition = new Vector3(worldLength, 0f, 0f);

        // --- collider sync ---
        if (boxCol != null)
        {
            float adjusted = Mathf.Max(0f, worldLength - colliderLengthAdjustment);
            boxCol.size = new Vector2(adjusted, 0.1f);
            boxCol.offset = new Vector2(adjusted / 2f, 0f);
        }
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
