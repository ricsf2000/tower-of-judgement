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
            rb.linearVelocity = movementInput.normalized * maxSpeed;

            // Control whether looking left or right
            if (movementInput.x > 0)
            {
                spriteRenderer.flipX = false;
                gameObject.BroadcastMessage("IsFacingRight", true);
            }
            else if (movementInput.x < 0)
            {
                spriteRenderer.flipX = true;
                gameObject.BroadcastMessage("IsFacingRight", false);
            }

            IsMoving = true;
        }
        else
        {
            // rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, idleFriction);
            rb.linearVelocity = Vector2.zero;
            IsMoving = false;
        }

    }

    void OnMove(InputValue movementValue)
    {
        movementInput = movementValue.Get<Vector2>();
    }

    void OnFire()
    {
        animator.SetTrigger("swordAttack");
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
