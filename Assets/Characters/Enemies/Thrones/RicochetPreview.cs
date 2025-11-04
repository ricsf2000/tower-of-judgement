using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class RicochetPreview : MonoBehaviour
{
    [Header("Settings")]
    public int maxBounces = 5;
    public float maxDistance = 30f;
    public LayerMask bounceMask; // what layers to bounce on (e.g., Walls)

    private LineRenderer lr;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 0;
    }

    public void DrawPath(Vector2 origin, Vector2 direction)
    {
        lr.positionCount = 1;
        lr.SetPosition(0, origin);

        Vector2 currentDir = direction.normalized;
        Vector2 currentPos = origin;

        for (int i = 0; i < maxBounces; i++)
        {
            // Raycast to next wall
            RaycastHit2D hit = Physics2D.Raycast(currentPos, currentDir, maxDistance, bounceMask);

            if (hit.collider != null)
            {
                // Record hit position
                lr.positionCount++;
                lr.SetPosition(lr.positionCount - 1, hit.point);

                // Reflect direction
                currentDir = Vector2.Reflect(currentDir, hit.normal);
                currentPos = hit.point + currentDir * 0.01f; // small offset to avoid re-hitting the same wall
            }
            else
            {
                // No more walls — extend line outward
                lr.positionCount++;
                lr.SetPosition(lr.positionCount - 1, currentPos + currentDir * maxDistance);
                break;
            }
        }
    }

    public void ClearPath()
    {
        lr.positionCount = 0;
    }
}
