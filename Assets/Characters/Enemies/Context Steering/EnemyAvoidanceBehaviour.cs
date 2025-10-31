using System.Collections.Generic;
using UnityEngine;

public class EnemyAvoidanceBehaviour : SteeringBehaviour
{
    [SerializeField] private float detectionRadius = 1.5f;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private bool showGizmos = false;

    private float[] dangerTemp;

    public override (float[] danger, float[] interest) GetSteering(
        float[] danger, float[] interest, AIData aiData)
    {
        // detect nearby enemies
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyMask);

        foreach (Collider2D hit in hits)
        {
            if (hit.transform == transform) continue; // ignore self

            Vector2 dirToEnemy = (hit.transform.position - transform.position).normalized;
            float dist = Vector2.Distance(transform.position, hit.transform.position);
            float weight = Mathf.Clamp01(1f - (dist / detectionRadius));

            for (int i = 0; i < Directions.eightDirections.Count; i++)
            {
                float dot = Vector2.Dot(dirToEnemy, Directions.eightDirections[i]);
                float value = dot * weight;
                if (value > danger[i])
                    danger[i] = value;
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
    }
}
