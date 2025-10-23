using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerElevationJump : MonoBehaviour
{
    [Header("References")]
    public Tilemap heightTilemap;        // HeightManager tilemap (with HeightTiles)
    public Tilemap jumpPointTilemap;     // Jump trigger tilemap
    public Transform sprite;             // Visual child for jump arc
    private Rigidbody2D rb;

    [Header("Jump Settings")]
    public float minSpeedForJump = 2.0f;     // Minimum movement speed to jump
    public float minJumpDistance = 2.0f;     // Always jump at least this far
    public float maxJumpDistance = 5.0f;     // Max jump distance at full speed
    public float maxMoveSpeed = 7.0f;        // Max player movement speed
    public float jumpDuration = 0.4f;        // Time for the jump arc
    public float jumpArcHeight = 0.5f;       // How high the arc appears

    [Header("Debug Info")]
    public int currentElevation = 0;
    public bool isJumping = false;
    private bool canMove = true;

    private Vector2 lastMoveDir = Vector2.down;
    private Vector3 spriteStartLocalPos;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteStartLocalPos = sprite.localPosition;
        UpdateElevation();
    }

    void Update()
    {
        if (isJumping) return;

        // Track elevation from HeightManager
        UpdateElevation();

        // Track direction from movement
        if (rb.linearVelocity.sqrMagnitude > 0.01f)
            lastMoveDir = rb.linearVelocity.normalized;

        // Jump trigger detection
        Vector3Int jumpCell = jumpPointTilemap.WorldToCell(transform.position);
        if (jumpPointTilemap.HasTile(jumpCell))
        {
            HandleJumpPoint();
        }
    }

    // ---------------- HEIGHT MANAGER ----------------
    void UpdateElevation()
    {
        Vector3Int cell = heightTilemap.WorldToCell(transform.position);
        HeightTile tile = heightTilemap.GetTile<HeightTile>(cell);

        if (tile != null)
        {
            if (tile.heightLevel != currentElevation)
            {
                currentElevation = tile.heightLevel;
                // Debug.Log($"[HeightManager] Elevation: {currentElevation}");
            }
        }
        else
        {
            // If no tile, assume ground level
            currentElevation = 0;
        }
    }

    // ---------------- JUMP LOGIC ----------------
    void HandleJumpPoint()
    {
        float speed = rb.linearVelocity.magnitude;

        if (speed < minSpeedForJump)
        {
            // Too slow — block player
            rb.linearVelocity = Vector2.zero;
            canMove = false;
            Debug.Log("[JumpPoint] Too slow — blocked!");
            return;
        }

        canMove = true;
        StartCoroutine(JumpRoutine(speed));
    }

    IEnumerator JumpRoutine(float speed)
    {
        isJumping = true;
        canMove = false;
        rb.linearVelocity = Vector2.zero;

        float speedRatio = Mathf.Clamp01(speed / maxMoveSpeed);
        float jumpDistance = Mathf.Lerp(minJumpDistance, maxJumpDistance, speedRatio);

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + (Vector3)(lastMoveDir.normalized * jumpDistance);

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / jumpDuration;
            float arc = Mathf.Sin(t * Mathf.PI) * jumpArcHeight;

            transform.position = Vector3.Lerp(startPos, endPos, t);
            sprite.localPosition = spriteStartLocalPos + Vector3.up * arc;

            yield return null;
        }

        sprite.localPosition = spriteStartLocalPos;

        // Update elevation when landing
        UpdateElevation();

        isJumping = false;
        canMove = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 endPos = transform.position + (Vector3)(lastMoveDir.normalized * 2f);
        Gizmos.DrawLine(transform.position, endPos);
    }
}
