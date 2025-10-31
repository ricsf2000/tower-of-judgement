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
    public Transform respawnPoint;                  // Optional: where to respawn (for player)
    private Rigidbody2D rb;

    [Header("Falling Settings")]
    public float fallGravity = 9.81f;               // How fast they sink visually
    public float fallDepth = -10f;                  // Y position threshold before respawn
    public bool destroyOnFall = false;              // If true, object is destroyed instead of respawned

    private Vector3 spriteStartLocalPos;
    private Vector3 fallVelocity;
    private bool isFalling = false;

    public bool IsFalling => isFalling;

    public bool ignoreDuringDash = false;
    public PlayerDash playerDash;

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

    }

    private void Update()
    {
        if (isFalling || holeTilemap == null) return;
        if (ignoreDuringDash && playerDash != null && playerDash.IsDashing) return;

        Vector3Int pos = holeTilemap.WorldToCell(transform.position);
        if (holeTilemap.HasTile(pos))
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
        else if (respawnPoint != null)
        {
            Respawn();
        }

        isFalling = false;
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
        transform.position = respawnPoint.position;
        sprite.localPosition = spriteStartLocalPos;
        sortingGroup.sortingLayerName = "Player";
        fallVelocity = Vector3.zero;

        // Apply fall penalty if this is the player
        if (CompareTag("Player"))
        {
            if (TryGetComponent(out DamageableCharacter dmgChar))
            {
                float fallDamage = 1f;
                dmgChar.OnHit(fallDamage);
                Debug.Log($"[FallableCharacter] Player took {fallDamage} fall damage on respawn. New HP: {dmgChar.Health}");

                StartCoroutine(ReenableAfterDelay(dmgChar, 0.5f));
            }
        }
    }

    private IEnumerator ReenableAfterDelay(DamageableCharacter dmgChar, float delay)
    {
        yield return new WaitForSeconds(delay);
        dmgChar.Targetable = true;
        dmgChar.Invincible = false;
    }
}
