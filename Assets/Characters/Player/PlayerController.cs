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
    public Rigidbody2D Rb => rb;
    public Animator Animator => animator;
    private PauseMenu pauseMenu;


    private Vector2 movementInput = Vector2.zero;
    private float lastMoveX = 0f;
    private float lastMoveY = 1f; // default facing up
    [HideInInspector] public bool canMove = true;
    [HideInInspector] public bool canAttack = true;
    [HideInInspector] public bool canDash = true;
    [HideInInspector]public bool canShoot = true;
    private bool isMoving = false;
    
    public Vector2 LastMoveDir { get; private set; }

    [HideInInspector] public float moveSpeedMultiplier = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        pauseMenu = FindAnyObjectByType<PauseMenu>();
    }

    void FixedUpdate()
    {
        if (!canMove) return;

        if (PauseMenu.isPaused || CutsceneDialogueController.IsCutsceneActive)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isMoving", false);
            animator.SetFloat("moveX", 0);
            animator.SetFloat("moveY", 0);
            return;
        }

        if (movementInput != Vector2.zero)
        {
            rb.AddForce(movementInput * moveSpeed * moveSpeedMultiplier * Time.deltaTime, ForceMode2D.Force);
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
        if (PauseMenu.isPaused || CutsceneDialogueController.IsCutsceneActive || !canMove)
        {
            movementInput = Vector2.zero;
            return;
        }
    
        movementInput = movementValue.Get<Vector2>();
    }

    void OnAttack()
    {
        if (PauseMenu.isPaused || CutsceneDialogueController.IsCutsceneActive || !canAttack)
            return;

        playerAttack?.HandleAttack();
    }

    void OnShoot(InputValue value)
    {
        if (CutsceneDialogueController.IsCutsceneActive || PauseMenu.isPaused) return;
        if (playerAttack) playerAttack.HandleAttack();
    }

    void OnDash()
    {
        if (CutsceneDialogueController.IsCutsceneActive || PauseMenu.isPaused) return;
        if (playerDash) playerDash.TryDash(movementInput, new Vector2(lastMoveX, lastMoveY));
    }

    void OnPause()
    {
        if (PauseMenu.isPaused)
        {
            pauseMenu.ResumeGame();
        }
        else if (!PauseMenu.isPaused)
        {
            pauseMenu.PauseGame();
        }
    }

    private void SetIsMoving(bool value)
    {
        if (isMoving == value) return;
        isMoving = value;
        animator.SetBool("isMoving", value);
    }

}


