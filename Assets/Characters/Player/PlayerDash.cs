using System.Collections;
using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public int maxDashCount = 2;
    public float dashCooldown = 0.2f;
    public TrailRenderer tr;

    private int currentDashCount;
    private bool isDashing = false;
    private bool canDash = true;

    private Rigidbody2D rb;
    private Animator animator;
    private PlayerController controller;

    private PlayerDamageable dmgChar;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        controller = GetComponent<PlayerController>();
        dmgChar = GetComponent<PlayerDamageable>();
        currentDashCount = maxDashCount;
    }

    public void TryDash(Vector2 moveInput, Vector2 lastMove)
    {
        if (!canDash || isDashing || currentDashCount <= 0) return;

        Vector2 dashDir = moveInput != Vector2.zero ? moveInput.normalized : lastMove;
        StartCoroutine(PerformDash(dashDir));
    }

    private IEnumerator PerformDash(Vector2 dashDir)
    {
        if (controller != null)
            controller.canMove = true;
        
        isDashing = true;
        canDash = false;
        currentDashCount--;

        if (dmgChar != null)
            dmgChar.Invincible = true;

        animator.Play("player_dash_tree", 0, 0f);
        tr.emitting = true;
        rb.linearVelocity = dashDir * dashSpeed;

        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);

        yield return new WaitForSeconds(dashDuration);

        rb.linearVelocity = Vector2.zero;
        tr.emitting = false;

        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);

        if (dmgChar != null)
            dmgChar.Invincible = false;

        if (currentDashCount <= 0)
        {
            yield return new WaitForSeconds(dashCooldown);
            currentDashCount = maxDashCount;
        }

        canDash = true;
        isDashing = false;
    }
}
