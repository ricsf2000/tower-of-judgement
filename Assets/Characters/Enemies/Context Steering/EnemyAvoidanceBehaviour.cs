using System.Collections.Generic;
using UnityEngine;

public class EnemyAvoidanceBehaviour : SteeringBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 2.5f;
    [SerializeField] private LayerMask enemyMask;

    [Header("Separation Settings")]
    [SerializeField, Tooltip("How strongly enemies try to avoid each other")]
    private float separationStrength = 1.5f;
    [SerializeField, Tooltip("Exponent for distance falloff (higher = sharper repulsion)")]
    private float distanceFalloff = 3f;

    [Header("Angular Offset")]
    [SerializeField, Tooltip("Preferred angular offset from direct opposite (-1 = opposite, -0.65 = angled)")]
    private float separationAngleDot = -0.65f;
    [SerializeField, Tooltip("Adds a left/right bias for more fluid avoidance")]
    private int separationSideBias = 1;

    [Header("Debug")]
    [SerializeField] private bool showGizmos = false;
    private float[] dangerTemp;

    public override (float[] danger, float[] interest) GetSteering(float[] danger, float[] interest, AIData aiData)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyMask);

        foreach (var hit in hits)
        {
            if (hit.transform == transform) continue;

            Vector2 toOther = (hit.transform.position - transform.position);
            float dist = toOther.magnitude;
            if (dist < 0.01f) continue;

            Vector2 dirToEnemy = toOther.normalized;
            float proximity = Mathf.Pow(1f - Mathf.Clamp01(dist / detectionRadius), distanceFalloff);

            for (int i = 0; i < Directions.eightDirections.Count; i++)
            {
                Vector2 dir = Directions.eightDirections[i];
                float dot = Vector2.Dot(dirToEnemy, dir);

                // Shape preference: avoid directly opposite, favor diagonal side-steps
                float shape = 1.0f - Mathf.Abs(dot - separationAngleDot);
                float sideOffset = separationSideBias * 0.1f * dot;
                float avoidance = Mathf.Clamp01((shape + sideOffset) * proximity * separationStrength);

                danger[i] = Mathf.Max(danger[i], avoidance);
            }
        }

        dangerTemp = danger;
        return (danger, interest);
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos || !Application.isPlaying) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (dangerTemp != null)
        {
            for (int i = 0; i < Directions.eightDirections.Count; i++)
            {
                Gizmos.color = Color.Lerp(Color.green, Color.red, dangerTemp[i]);
                Gizmos.DrawRay(transform.position, Directions.eightDirections[i] * dangerTemp[i] * detectionRadius);
            }
        }
    }
}
