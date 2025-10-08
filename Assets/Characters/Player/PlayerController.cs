using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 700.0f;
    public float maxSpeed = 2.2f;
    public float idleFriction = 0.9f;
    public float collisionOffset = 0.05f;
    public ContactFilter2D movementFilter;
    // public SwordAttack swordAttack;
    public GameObject swordHitbox;
    Collider2D swordCollider;


    Vector2 movementInput = Vector2.zero;
    SpriteRenderer spriteRenderer;
    Rigidbody2D rb;
    Animator animator;
    List<RaycastHit2D> castCollisions = new List<RaycastHit2D>();
    private Vector2 facingDirection = Vector2.right; // default right
    private bool isAttacking = false;
    private bool holdAttackFacing = false;
    private float holdAttackTimer = 0f;
    [SerializeField] private float holdAttackDuration = 0.50f;


    bool canMove = true;
    private bool isMoving = false;
    public bool IsMoving
    {
        set
        {
            isMoving = value;
            animator.SetBool("isMoving", value);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        swordCollider = swordHitbox.GetComponent<Collider2D>();
    }

    private void FixedUpdate()
    {
        if (canMove == true && movementInput != Vector2.zero)
        {
            // Move animation and add velocity

            // Accelerate the player while run direction is pressed
            // BUT don't allow the player to run faster than max speed in any direction
            // rb.linearVelocity = movementInput.normalized * maxSpeed;
            // rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity + (movementInput * moveSpeed * Time.deltaTime), maxSpeed);
            rb.AddForce(movementInput * moveSpeed * Time.deltaTime);

            if (rb.linearVelocity.magnitude > maxSpeed)
            {
                float limitedSpeed = Mathf.Lerp(rb.linearVelocity.magnitude, maxSpeed, idleFriction);
                rb.linearVelocity = rb.linearVelocity.normalized * limitedSpeed;
            }

            // Track the facing direction
            facingDirection = movementInput.normalized;

            // Control whether looking left or right
            if (!holdAttackFacing)
            {
                if (movementInput.x > 0)
                {
                    spriteRenderer.flipX = false;
                    gameObject.BroadcastMessage("IsFacingRight", true);
                    animator.SetFloat("moveX", 1);
                }
                else if (movementInput.x < 0)
                {
                    spriteRenderer.flipX = true;
                    gameObject.BroadcastMessage("IsFacingRight", false);
                    animator.SetFloat("moveX", -1);
                }
                else if (movementInput.y < 0)   // Down
                {
                    animator.SetFloat("moveY", -1);
                }
                else if (movementInput.y > 0)   // Up
                {
                    animator.SetFloat("moveY", 1);
                }
            }
            IsMoving = true;
        }
        else
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, idleFriction);
            // rb.linearVelocity = Vector2.zero;
            IsMoving = false;
        }
    }

    private void Update()
    {
        if (holdAttackFacing)
        {
            holdAttackTimer -= Time.deltaTime;
            if (holdAttackTimer <= 0f)
                holdAttackFacing = false;
        }
    }

    void OnMove(InputValue movementValue)
    {
        movementInput = movementValue.Get<Vector2>();
    }

    private void HoldAttackFacing()
    {
        holdAttackFacing = true;
        holdAttackTimer = holdAttackDuration;
    }

    void OnFire()
    {
        // Get mouse position in world space
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 dir = (mouseWorldPos - (Vector2)transform.position).normalized;

        // Flip toward mouse
        bool mouseRight = mouseWorldPos.x > transform.position.x;
        spriteRenderer.flipX = !mouseRight;
        gameObject.BroadcastMessage("IsFacingRight", mouseRight);

        // (existing animator / hitbox setup)
        animator.SetFloat("attackX", dir.x);
        animator.SetFloat("attackY", dir.y);
        animator.SetTrigger("swordAttack");
        swordHitbox.GetComponent<SwordAttack>().SetAttackDirection(dir);

        // start facing-hold timer
        HoldAttackFacing();
    }

    public void LockMovement()
    {
        canMove = false;
    }

    public void UnlockMovement()
    {
        canMove = true;
    }
}
