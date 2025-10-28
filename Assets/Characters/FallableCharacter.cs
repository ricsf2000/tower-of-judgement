using System.Collections;
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

        // Immediately freeze Rigidbody
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        sortingGroup.sortingLayerName = "Ground";

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
            Destroy(gameObject);
        }
        else if (respawnPoint != null)
        {
            Respawn();
        }

        isFalling = false;
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
    }
}
