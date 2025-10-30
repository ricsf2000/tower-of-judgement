using UnityEngine;

[ExecuteAlways]
public class ContextSteeringDebugger : MonoBehaviour
{
    [Header("References")]
    public ContextSolver contextSolver;     // Assign in Inspector
    public AIData aiData;                   // Assign in Inspector

    [Header("Visualization Settings")]
    public float rayLength = 2f;            // How long to draw each direction
    public float interestScale = 1.5f;      // Visual length multiplier
    public float dangerScale = 1.5f;
    public bool drawInterest = true;
    public bool drawDanger = true;

    private void OnDrawGizmos()
    {
        if (contextSolver == null || aiData == null)
            return;

        // 🔹 Safety check — make sure we have steering data
        var behaviours = FindObjectsByType<SteeringBehaviour>(FindObjectsSortMode.None);
        float[] danger = new float[8];
        float[] interest = new float[8];

        // Run through each steering behaviour manually (read-only)
        foreach (var b in behaviours)
        {
            (danger, interest) = b.GetSteering(danger, interest, aiData);
        }

        // 🔹 Draw all 8 directions
        for (int i = 0; i < Directions.eightDirections.Count; i++)
        {
            Vector2 dir = Directions.eightDirections[i];

            // Draw danger rays (red)
            if (drawDanger && danger[i] > 0.01f)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.7f);
                Gizmos.DrawRay(transform.position, dir * danger[i] * rayLength * dangerScale);
            }

            // Draw interest rays (green)
            if (drawInterest && interest[i] > 0.01f)
            {
                Gizmos.color = new Color(0f, 1f, 0f, 0.7f);
                Gizmos.DrawRay(transform.position, dir * interest[i] * rayLength * interestScale);
            }
        }

        // Draw path waypoint target if available
        if (aiData.currentWaypoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere((Vector2)aiData.currentWaypoint, 0.1f);
            Gizmos.DrawLine(transform.position, (Vector2)aiData.currentWaypoint);
        }
    }
}
