using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    public GameObject swordHitbox;
    public AudioClip[] swordSwing;

    [SerializeField] private float holdAttackDirectionDuration = 0.50f;
    private float holdAttackTimer = 0f;
    private bool holdAttackFacing = false;

    private Rigidbody2D rb;
    private Animator animator;
    private PlayerController controller;
    private PlayerSFX sfx;

    private PlayerControls controls;
    private Vector2 lookInput;
    private enum InputMode { KeyboardMouse, Controller }
    private InputMode currentInputMode = InputMode.KeyboardMouse;

    private float lastMouseInputTime;
    private const float mousePriorityDuration = 0.5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        controller = GetComponent<PlayerController>();
        sfx = GetComponent<PlayerSFX>();

        controls = new PlayerControls();
        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => lookInput = Vector2.zero;
        controls.Player.Attack.performed += ctx => HandleAttack();
    }

    private void OnEnable() => controls.Player.Enable();
    private void OnDisable() => controls.Player.Disable();

    private void Update()
    {
        DetectInputMode();
        UpdateAttackTimer();
    }

    private void DetectInputMode()
    {
        if (Mouse.current != null)
        {
            if (Mouse.current.delta.ReadValue().sqrMagnitude > 0.01f ||
                Mouse.current.leftButton.isPressed)
            {
                lastMouseInputTime = Time.time;
                currentInputMode = InputMode.KeyboardMouse;
            }
        }

        if (Gamepad.current != null)
        {
            Vector2 moveInput = controls.Player.Move.ReadValue<Vector2>();
            if (moveInput.sqrMagnitude > 0.1f && Time.time > lastMouseInputTime + mousePriorityDuration)
                currentInputMode = InputMode.Controller;
        }
    }

    public void HandleAttack()
    {
        if (!controller.canAttack) return;

        animator.ResetTrigger("swordAttack");

        Vector2 dir = Vector2.zero;

        if (currentInputMode == InputMode.KeyboardMouse)
        {
            if (Mouse.current != null)
            {
                Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                dir = (mouseWorldPos - (Vector2)transform.position).normalized;
            }
        }
        else if (currentInputMode == InputMode.Controller)
        {
             Vector2 moveDir = controls.Player.Move.ReadValue<Vector2>();
            if (moveDir.sqrMagnitude > 0.1f)
            {
                dir = GetAssistedDirection(moveDir.normalized);
            }
            else
            {
                dir = controller.LastMoveDir;
            }
        }

        // Fallback safeguard
        if (dir == Vector2.zero)
            dir = Vector2.down;

        // Update facing / animator parameters
        float lastMoveX = Mathf.Abs(dir.x) > Mathf.Abs(dir.y) ? Mathf.Sign(dir.x) : 0;
        float lastMoveY = Mathf.Abs(dir.y) > Mathf.Abs(dir.x) ? Mathf.Sign(dir.y) : 0;

        animator.SetFloat("lastMoveX", lastMoveX);
        animator.SetFloat("lastMoveY", lastMoveY);
        animator.SetFloat("attackX", dir.x);
        animator.SetFloat("attackY", dir.y);
        animator.SetTrigger("swordAttack");

        holdAttackFacing = true;
        holdAttackTimer = holdAttackDirectionDuration;
    }

    public void UpdateAttackTimer()
    {
        if (holdAttackFacing)
        {
            holdAttackTimer -= Time.deltaTime;
            if (holdAttackTimer <= 0f)
                holdAttackFacing = false;
        }
    }

    public void EnableNextAttack()
    {
        controller.canAttack = true; // allow chaining next combo attack
    }

    public void LockMovement()
    {
        if (controller == null) return;

        controller.canMove = false;
        rb.linearVelocity = Vector2.zero;

        Vector2 dir = Vector2.zero;

        // Use current input mode to decide lunge direction
        if (currentInputMode == InputMode.Controller)
        {
            dir = controller.LastMoveDir;
        }
        else if (currentInputMode == InputMode.KeyboardMouse && Mouse.current != null)
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            dir = (mouseWorldPos - (Vector2)transform.position).normalized;
        }

        if (dir != Vector2.zero)
            rb.AddForce(dir * 3f, ForceMode2D.Impulse);
    }


    public void UnlockMovement()
    {
        controller.canMove = true;
        controller.canAttack = true;
        rb.linearVelocity = Vector2.zero;
    }

    private Vector2 GetAssistedDirection(Vector2 inputDir)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 4f, LayerMask.GetMask("Enemy"));
        if (hits.Length == 0) return inputDir;

        Transform bestTarget = null;
        float bestDot = 0.8f; // only adjust if enemy is within ~36° cone
        foreach (var h in hits)
        {
            Vector2 toEnemy = ((Vector2)h.transform.position - (Vector2)transform.position).normalized;
            float dot = Vector2.Dot(inputDir, toEnemy);
            if (dot > bestDot)
            {
                bestDot = dot;
                bestTarget = h.transform;
            }
        }

        if (bestTarget != null)
            return ((Vector2)bestTarget.position - (Vector2)transform.position).normalized;
        return inputDir;
    }


}


