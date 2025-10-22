using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering;

[RequireComponent(typeof(Rigidbody2D))]
[SelectionBase] public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 700.0f;
    public float idleFriction = 0.9f;

    [Header("References")]
    public PlayerAttack playerAttack;
    public PlayerDash playerDash;
    public PlayerShoot playerShoot;
    public PlayerSFX playerSFX;

    private Rigidbody2D rb;
    private Animator animator;

    private Vector2 movementInput = Vector2.zero;
    private float lastMoveX = 0f;
    private float lastMoveY = 1f; // default facing up
    [HideInInspector] public bool canMove = true;
    [HideInInspector] public bool canAttack = true;
    [HideInInspector] public bool canDash = true;
    [HideInInspector]public bool canShoot = true;
    private bool isMoving = false;
    
    public Vector2 LastMoveDir { get; private set; }

    [Header("Falling Settings")]
    public Tilemap holeTilemap;
    public Transform sprite; // reference to the visual child
    public SortingGroup sortingGroup;
    public float fallGravity = 12f;
    public float fallDepth = -10f;
    public Vector3 respawnPoint = Vector3.zero;

    private Vector3 spriteStartLocalPos;
    private Vector3 fallVelocity;
    [HideInInspector] public bool isFalling = false;
    private bool isRespawning = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        spriteStartLocalPos = sprite.localPosition;
        sortingGroup.sortingLayerName = "Player";
    }

    void FixedUpdate()
    {
        if (!canMove) return;

        if (movementInput != Vector2.zero)
        {
            rb.AddForce(movementInput * moveSpeed * Time.deltaTime, ForceMode2D.Force);
            Vector2 moveDir = movementInput.normalized;

            if (movementInput.sqrMagnitude > 0.01f)
            {
                LastMoveDir = movementInput.normalized;
            }

            animator.SetFloat("moveX", moveDir.x);
            animator.SetFloat("moveY", moveDir.y);
            lastMoveX = moveDir.x;
            lastMoveY = moveDir.y;
            SetIsMoving(true);
        }
        else
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, idleFriction);
            animator.SetFloat("lastMoveX", lastMoveX);
            animator.SetFloat("lastMoveY", lastMoveY);
            SetIsMoving(false);
        }
    }

    void Update()
    {

        if (playerAttack) playerAttack.UpdateAttackTimer();

        // Handle Falling
        Vector3Int pos = holeTilemap.WorldToCell(transform.position);
        if (holeTilemap.HasTile(pos) && !(playerDash && playerDash.IsDashing))
        {
            // Respawn logic
            if (sprite.position.y < fallDepth)
            {
                rb.constraints = RigidbodyConstraints2D.None; // unfreeze before resetting
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                transform.position = respawnPoint;
                sprite.localPosition = spriteStartLocalPos;
                sortingGroup.sortingLayerName = "Player";
                fallVelocity = Vector3.zero;
                return;
            }

            // Freeze the Rigidbody immediately
            rb.linearVelocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll; // this prevents any motion
            sortingGroup.sortingLayerName = "Ground";
            fallVelocity += Physics.gravity * fallGravity * Time.deltaTime;
            sprite.transform.position += fallVelocity * Time.deltaTime;
            return; // stop normal movement while falling
        }
        
    }
    
    
    void OnMove(InputValue movementValue)
    {
        movementInput = movementValue.Get<Vector2>();
    }

    void OnAttack()
    {
        if (playerAttack) playerAttack.HandleAttack();
    }

    void OnShoot(InputValue value)
    {
        if (playerShoot) playerShoot.HandleShootInput(value);
    }

    void OnDash()
    {
        if (playerDash) playerDash.TryDash(movementInput, new Vector2(lastMoveX, lastMoveY));
    }

    private void SetIsMoving(bool value)
    {
        if (isMoving == value) return;
        isMoving = value;
        animator.SetBool("isMoving", value);
    }

    private IEnumerator FallRoutine()
    {
        isFalling = true;
        canMove = false;
        rb.linearVelocity = Vector2.zero;
        sortingGroup.sortingLayerName = "Ground"; // fall below ground

        fallVelocity = Vector3.zero;

        while (sprite.position.y > fallDepth)
        {
            fallVelocity += Physics.gravity * fallGravity * Time.deltaTime;
            sprite.position += fallVelocity * Time.deltaTime;
            yield return null;
        }

        // respawn
        isRespawning = true;
        transform.position = respawnPoint;
        sprite.localPosition = spriteStartLocalPos;
        sortingGroup.sortingLayerName = "Player";
        isFalling = false;

        yield return new WaitForSeconds(0.5f); // small pause before regaining control
        isRespawning = false;
        canMove = true;
    }
}


