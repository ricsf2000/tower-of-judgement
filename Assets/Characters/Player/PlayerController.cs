using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class PlayerController : MonoBehaviour
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
    private bool isMoving = false;
    
    public Vector2 LastMoveDir { get; private set; }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
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
}
