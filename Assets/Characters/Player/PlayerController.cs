using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 700.0f;
    // public float maxSpeed = 2.2f;
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
    // private bool isAttacking = false;
    private bool holdAttackFacing = false;
    private float holdAttackTimer = 0f;
    [SerializeField] private float holdAttackDuration = 0.50f;
    private float lastMoveX = 0f;
    private float lastMoveY = -1f; // default facing down
    private Vector2 attackDirection = Vector2.zero;

    private Shoot shoot;

    bool canMove = true;
    bool canShoot = true;
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
        shoot = GetComponent<Shoot>();
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
            rb.AddForce(movementInput * moveSpeed * Time.deltaTime, ForceMode2D.Force);

            // if (rb.linearVelocity.magnitude > maxSpeed)
            // {
            //     float limitedSpeed = Mathf.Lerp(rb.linearVelocity.magnitude, maxSpeed, idleFriction);
            //     rb.linearVelocity = rb.linearVelocity.normalized * limitedSpeed;
            // }

            // Track the facing direction
            facingDirection = movementInput.normalized;

            // Control whether looking left or right/up or down
            if (!holdAttackFacing)
            {
                Vector2 moveDir = movementInput.normalized;

                // Update animator parameters directly
                animator.SetFloat("moveX", moveDir.x);
                animator.SetFloat("moveY", moveDir.y);

                // Remember the last direction we moved
                lastMoveX = moveDir.x;
                lastMoveY = moveDir.y;
            }
            IsMoving = true;
        }
        else
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, idleFriction);
            // rb.linearVelocity = Vector2.zero;
            IsMoving = false;

            // When not moving, keep the last direction in the animator
            animator.SetFloat("lastMoveX", lastMoveX);
            animator.SetFloat("lastMoveY", lastMoveY);
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

        // Immediately update facing direction
        lastMoveX = Mathf.Abs(dir.x) > Mathf.Abs(dir.y) ? Mathf.Sign(dir.x) : 0;
        lastMoveY = Mathf.Abs(dir.y) > Mathf.Abs(dir.x) ? Mathf.Sign(dir.y) : 0;

        animator.SetFloat("lastMoveX", lastMoveX);
        animator.SetFloat("lastMoveY", lastMoveY);

        // (existing animator / hitbox setup)
        animator.SetFloat("attackX", dir.x);
        animator.SetFloat("attackY", dir.y);
        animator.SetTrigger("swordAttack");
        // swordHitbox.GetComponent<SwordAttack>().SetAttackDirection(dir);

        // Save direction for movement push
        attackDirection = dir.normalized;

        // start facing-hold timer
        HoldAttackFacing();
    }

    void OnShoot(InputValue value)
    {
        Debug.Log($"OnShoot triggered: {value.isPressed}");
        if (!canShoot) return;

        if (value.isPressed)
        {
            shoot.OnShootPressed();  // begin charging
        }
        else
        {
            shoot.OnShootReleased();  // release and fire
        }
    }

    public void LockMovement()
    {
        // Debug.Log("LockMovement fired");
        canMove = false;
        canShoot = false;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(attackDirection * 5.0f, ForceMode2D.Impulse);
    }

    public void UnlockMovement()
    {
        // Debug.Log("UnlockMovement fired");
        canMove = true;
        canShoot = true;
        rb.linearVelocity = Vector2.zero;
    }
    
}
