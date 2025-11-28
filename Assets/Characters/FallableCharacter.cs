using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering;

[RequireComponent(typeof(Rigidbody2D))]
public class FallableCharacter : MonoBehaviour
{
    [Header("References")]
    public Tilemap holeTilemap;
    public Transform sprite;                       // The child sprite transform (for visual sinking)
    public SortingGroup sortingGroup;               // To change sorting layer while falling
    public Vector3 respawnPosition;                 // Where to respawn
    private Rigidbody2D rb;

    [Header("Falling Settings")]
    public float fallGravity = 9.81f;               // How fast they sink visually
    public float fallDepth = -10f;                  // Y position threshold before respawn
    public bool destroyOnFall = false;              // If true, object is destroyed instead of respawned
    public float moveLockDuration = 0.5f;           // Time before player can move again after respawn

    private Vector3 spriteStartLocalPos;
    private Vector3 fallVelocity;
    private bool isFalling = false;

    public bool IsFalling => isFalling;

    public bool ignoreDuringDash = false;
    public PlayerDash playerDash;

    [Header("Platform Check")]
    public LayerMask platformLayer;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (sprite) spriteStartLocalPos = sprite.localPosition;

        if (holeTilemap == null)
        {
            GameObject holeObj = GameObject.Find("Hole");
            if (holeObj != null)
                holeTilemap = holeObj.GetComponent<Tilemap>();
        }
        respawnPosition = transform.position; // fallback default
    }

    private void Update()
    {
        if (isFalling || holeTilemap == null) return;
        if (ignoreDuringDash && playerDash != null && playerDash.IsDashing) return;

        Vector3Int pos = holeTilemap.WorldToCell(transform.position);
        bool holeTile = holeTilemap.HasTile(pos);

        // Check if platform GameObject is underneath
        bool platformBelow = HasPlatformBelow();

        if (holeTile && !platformBelow)
        {
            StartCoroutine(HandleFall());
        }
    }

    private IEnumerator HandleFall()
    {
        isFalling = true;

        // Cache the DamageableCharacter
        DamageableCharacter dmgChar = null;
        TryGetComponent(out dmgChar);

        // Freeze Rigidbody
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        sortingGroup.sortingLayerName = "Ground";

        // Make the character untargetable / invincible
        if (dmgChar != null)
        {
            dmgChar.Invincible = true;
            dmgChar.Targetable = false;
        }

        fallVelocity = Vector3.zero;

        while (sprite.position.y > fallDepth)
        {
            fallVelocity += Physics.gravity * fallGravity * Time.deltaTime;
            sprite.position += fallVelocity * Time.deltaTime;
            yield return null;
        }

        // After falling out of view
        if (destroyOnFall)
        {
            if (dmgChar != null)
            {
                Debug.Log($"[FallableCharacter] {name} fell into a hole — triggering death sequence.");
                dmgChar.Health = 0f;
            }
            else
            {
                Debug.Log($"[FallableCharacter] {name} destroyed after fall (no DamageableCharacter).");
                Destroy(gameObject);
            }
        }
        else
        {
            Respawn();
        }

        isFalling = false;
    }

    private bool HasPlatformBelow()
    {
        Collider2D myCol = GetComponent<Collider2D>();
        if (myCol == null) return false;

        Vector2 position = transform.position;
        Vector2 size = myCol.bounds.size * 0.8f;

        Collider2D hit = Physics2D.OverlapBox(position, size, 0f, platformLayer);

        return hit != null;
    }



    public void OnHit(float disableDuration = 0.5f)
    {
        StartCoroutine(TemporarilyIgnoreGroundEdges(disableDuration));
    }

    private IEnumerator TemporarilyIgnoreGroundEdges(float duration)
    {
        // Find all GroundEdge colliders in the scene
        Collider2D[] edgeColliders = FindObjectsOfType<Collider2D>();
        List<Collider2D> ignored = new List<Collider2D>();

        foreach (Collider2D col in edgeColliders)
        {
            if (col.gameObject.layer == LayerMask.NameToLayer("GroundEdge"))
            {
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), col, true);
                ignored.Add(col);
            }
        }

        yield return new WaitForSeconds(duration);

        // Re-enable collisions only for this object
        if (!isFalling)
        {
            foreach (Collider2D col in ignored)
            {
                if (col != null)
                    Physics2D.IgnoreCollision(GetComponent<Collider2D>(), col, false);
            }
        }
    }

    private void Respawn()
    {
        // Unfreeze and reset state
        rb.constraints = RigidbodyConstraints2D.None;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        transform.position = respawnPosition;
        sprite.localPosition = spriteStartLocalPos;
        sortingGroup.sortingLayerName = "Player";
        fallVelocity = Vector3.zero;

        // Apply fall penalty if this is the player
        if (CompareTag("Player"))
        {
            if (TryGetComponent(out DamageableCharacter dmgChar))
            {
                float fallDamage = 1f;
                // Disable temporary invincibility BEFORE applying damage
                dmgChar.Invincible = false;

                // Apply fall damage
                dmgChar.OnHit(fallDamage);
                Debug.Log($"[FallableCharacter] Player took {fallDamage} fall damage on respawn. New HP: {dmgChar.Health}");

                // Re-enable invincibility AFTER respawn (to prevent instant re-hit)
                StartCoroutine(ReenableAfterDelay(dmgChar, 0.5f));

                // Prevent player from moving for a short time
                if (TryGetComponent(out PlayerController controller))
                {
                    StartCoroutine(LockMovementTemporarily(controller));
                }
            }
        }
    }

    private IEnumerator ReenableAfterDelay(DamageableCharacter dmgChar, float delay)
    {
        yield return new WaitForSeconds(delay);
        dmgChar.Targetable = true;
        dmgChar.Invincible = false;
    }

    // Locks player movement after respawn
    private IEnumerator LockMovementTemporarily(PlayerController controller)
    {
        yield return null;
        controller.canMove = false;
        rb.linearVelocity = Vector2.zero;

        float timer = moveLockDuration;
        while (timer > 0)
        {
            rb.linearVelocity = Vector2.zero; // enforce stillness
            timer -= Time.deltaTime;
            yield return null;
        }

        controller.canMove = true;
    }
}
