using UnityEngine;

public class EyeFollow : MonoBehaviour
{
    [Header("References")]
    public DetectionZone detectionZone;

    [Header("Movement Settings")]
    public float followSpeed = 8f;
    public float directionSmooth = 5f;

    [Header("Eye Offsets (Local Space)")]
    public Vector2 up = new Vector2(0f, 0.01f);
    public Vector2 down = new Vector2(0f, -0.01f);
    public Vector2 left = new Vector2(-0.02f, 0f);
    public Vector2 right = new Vector2(0.02f, 0f);

    public Vector2 upLeft = new Vector2(-0.01f, 0.01f);
    public Vector2 upRight = new Vector2(0.01f, 0.01f);
    public Vector2 downLeft = new Vector2(-0.02f, -0.01f);
    public Vector2 downRight = new Vector2(0.02f, -0.01f);

    private Vector3 initialLocalPos;
    private Transform eyeRoot;
    private Vector2 smoothDir;

    private bool isLocked = false;
    private Vector2 lockedDir = Vector2.zero;

    void Start()
    {
        eyeRoot = transform.parent;
        initialLocalPos = transform.localPosition;
    }

    void LateUpdate()
    {
        // If locked, just stay in locked direction
        if (isLocked)
        {
            Vector2 lockedOffset = GetDirectionalOffset(lockedDir);
            Vector3 lockedLocalPos = initialLocalPos + (Vector3)lockedOffset;
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                lockedLocalPos,
                Time.deltaTime * followSpeed
            );
            return;
        }

        // Return to neutral if no target
        if (detectionZone == null || detectionZone.detectedObjs.Count == 0)
        {
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                initialLocalPos,
                Time.deltaTime * followSpeed
            );
            return;
        }

        // Follow target from detection zone
        Transform target = detectionZone.detectedObjs[0].transform;

        // Smooth the direction vector
        Vector2 rawDir = (target.position - eyeRoot.position).normalized;
        smoothDir = Vector2.Lerp(smoothDir, rawDir, Time.deltaTime * directionSmooth);

        // Get which offset to move toward
        Vector2 targetOffset = GetDirectionalOffset(smoothDir);

        // Move smoothly toward that position
        Vector3 targetLocalPos = initialLocalPos + (Vector3)targetOffset;
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetLocalPos,
            Time.deltaTime * followSpeed
        );
    }

    Vector2 GetDirectionalOffset(Vector2 dir)
    {
        float x = dir.x;
        float y = dir.y;

        // Decide which octant (8-way direction) the player is in
        if (Mathf.Abs(x) > 0.5f && Mathf.Abs(y) > 0.5f)
        {
            // Diagonals
            if (x > 0 && y > 0) return upRight;
            if (x < 0 && y > 0) return upLeft;
            if (x > 0 && y < 0) return downRight;
            if (x < 0 && y < 0) return downLeft;
        }
        else if (Mathf.Abs(x) > Mathf.Abs(y))
        {
            // Horizontal
            return x > 0 ? right : left;
        }
        else
        {
            // Vertical
            return y > 0 ? up : down;
        }

        return Vector2.zero;
    }

    public void LockDirection(Vector2 dir)
    {
        isLocked = true;
        lockedDir = dir.normalized;
    }

    public void UnlockDirection()
    {
        isLocked = false;
    }


}
