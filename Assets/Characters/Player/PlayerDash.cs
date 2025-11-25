using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerDash : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public int maxDashCount = 2;
    public float dashCooldown = 0.2f;
    public TrailRenderer tr;
    public float dashDistance = 5f;
    private bool rechargeRunning = false;

    [Header("Dash Bridge Tilemap")]
    public Tilemap dashBridgeTilemap;

    private int currentDashCount;
    private bool isDashing = false;
    public bool IsDashing => isDashing;
    private bool canDash = true;

    private Rigidbody2D rb;
    private Animator animator;
    private PlayerController controller;

    private PlayerDamageable dmgChar;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        controller = GetComponent<PlayerController>();
        dmgChar = GetComponent<PlayerDamageable>();
        currentDashCount = maxDashCount;
    }

    public void TryDash(Vector2 moveInput, Vector2 lastMove)
    {
        if (!canDash || isDashing || currentDashCount <= 0) return;

        FallableCharacter fallable = GetComponent<FallableCharacter>();
        if (fallable != null && fallable.IsFalling) return;

        Vector2 dashDir = moveInput != Vector2.zero ? moveInput.normalized : lastMove;
        Debug.Log($"moveInput: {moveInput}, lastMove: {lastMove}");
        StartCoroutine(PerformDash(dashDir));
    }

    private bool TryGetDashBridge(Vector2 worldPos, Vector2 dashDir, out Vector2 targetWorldPos)
    {
        targetWorldPos = worldPos;

        if (dashBridgeTilemap == null)
            return false;

        Vector3Int startCell = dashBridgeTilemap.WorldToCell(worldPos);

        // Must be standing on a bridge tile
        if (!dashBridgeTilemap.HasTile(startCell))
            return false;

        Vector3Int dir = new Vector3Int(
            Mathf.RoundToInt(dashDir.x),
            Mathf.RoundToInt(dashDir.y),
            0
        );

        // Search forward for another bridge tile in that direction
        for (int i = 1; i <= 10; i++)
        {
            Vector3Int check = startCell + dir * i;
            if (dashBridgeTilemap.HasTile(check))
            {
                targetWorldPos = dashBridgeTilemap.GetCellCenterWorld(check);
                return true;
            }
        }

        return false;
    }

    private IEnumerator PerformDash(Vector2 dashDir)
    {
        if (controller != null)
            controller.canMove = true;
            
        isDashing = true;
        currentDashCount--;
        
        StartCoroutine(RechargeDash());

        // Only disable immediate dashing if no charges remain
        if (currentDashCount <= 0)
            canDash = false;
        
        controller.moveSpeedMultiplier = 1f;

        if (dmgChar != null)
            dmgChar.Invincible = true;

        animator.Play("player_dash_tree", 0, 0f);
        tr.emitting = true;

        if (controller != null)
            controller.canMove = false;

        rb.linearVelocity = Vector2.zero;

        // Dash bridge check
        float finalDashDistance = dashDistance;
        Vector2 targetPos;
        if (TryGetDashBridge(transform.position, dashDir, out targetPos))
        {
            float bridgeDist = Vector2.Distance(rb.position, targetPos);
            finalDashDistance = bridgeDist + 1f;  

            Debug.Log($"[DashBridge] Found bridge tile at {targetPos}, dist={bridgeDist:F2}, newDuration={dashDuration:F2}");
        }

        // Dash start
        dashSpeed = finalDashDistance / dashDuration;
        Debug.Log(dashSpeed);
        rb.linearVelocity = dashDir.normalized * dashSpeed;

        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int groundEdgeLayer = LayerMask.NameToLayer("GroundEdge");

        // Ignore enemies & edge colliders while dashing
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
        Physics2D.IgnoreLayerCollision(playerLayer, groundEdgeLayer, true);

        yield return new WaitForSeconds(dashDuration);

        // Dash end
        if (controller != null)
        {
            controller.canMove = true;
            controller.moveSpeedMultiplier = 1f;
            controller.canAttack = true;    // Make sure dashes can never lock attacks
        }

        rb.linearVelocity = Vector2.zero;
        tr.emitting = false;

        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
        Physics2D.IgnoreLayerCollision(playerLayer, groundEdgeLayer, false);

        if (dmgChar != null)
            dmgChar.Invincible = false;

        if (currentDashCount > 0)
            canDash = true;
        
        isDashing = false;
    }

    private IEnumerator RechargeDash()
    {
        // If cooldown already running, restart it
        if (rechargeCoroutine != null)
        {
            StopCoroutine(rechargeCoroutine);
        }

        rechargeCoroutine = StartCoroutine(RechargeCoroutine());

        // Must return something because signature is IEnumerator
        yield break;
    }

    private Coroutine rechargeCoroutine;

    private IEnumerator RechargeCoroutine()
    {
        yield return new WaitForSeconds(dashCooldown);

        // Refill all charges
        currentDashCount = maxDashCount;
        canDash = true;

        // Mark cooldown as finished
        rechargeCoroutine = null;
    }


}
