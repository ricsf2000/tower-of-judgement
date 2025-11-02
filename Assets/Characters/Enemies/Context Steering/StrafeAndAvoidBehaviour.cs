using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;   // ⬅️ Needed for UnityEvent<Vector2>

public class StrafeAndAvoidBehaviour : SteeringBehaviour
{
    [Header("Strafe Settings")]
    [SerializeField] private float preferredDistance = 2.5f;
    [SerializeField] private float strafeStrength = 1.0f;
    [SerializeField] private int strafeDirection = 1; // +1 = right, -1 = left

    [Header("Distance Blend Settings")]
    [Tooltip("How close before strafing starts (0 = always, 1 = never)")]
    [Range(0f, 1f)] public float strafeStartRatio = 0.7f;
    [Tooltip("How quickly strafing ramps up once inside range")]
    [Range(0.1f, 5f)] public float strafeFalloffSharpness = 3f;

    [Header("Facing Event")]
    [Tooltip("Hook this to EnemyAI.OnPointerInput so the enemy always faces the player.")]
    public UnityEvent<Vector2> OnPointerInput;

    [Header("Debug")]
    [SerializeField] private bool showGizmos = false;
    private float[] interestTemp;

    private void Start()
    {
        // Each enemy orbits slightly differently
        float idOffset = (GetInstanceID() % 100) * 0.01f; // deterministic randomization
        preferredDistance += Random.Range(-0.5f, 0.5f) + idOffset;
        strafeDirection = Random.value < 0.5f ? 1 : -1;
    }
    
    public override (float[] danger, float[] interest) GetSteering(
        float[] danger, float[] interest, AIData aiData)
    {
        // Safety: no data, no target
        if (aiData == null || aiData.currentTarget == null)
            return (danger, interest);

        // Tell the enemy to face the player
        OnPointerInput?.Invoke(aiData.currentTarget.position);

        Vector2 toTarget = (aiData.currentTarget.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, aiData.currentTarget.position);
        float distRatio = Mathf.Clamp01(distance / preferredDistance);

        // No strafing when too far away
        if (distRatio > strafeStartRatio)
            return (danger, interest);

        // 📈 Strafing ramps up sharply when close
        float strafeWeight = Mathf.Pow(1f - (distRatio / strafeStartRatio), strafeFalloffSharpness);

        for (int i = 0; i < Directions.eightDirections.Count; i++)
        {
            Vector2 dir = Directions.eightDirections[i];

            // Use perpendicular vector to bias side movement
            Vector2 sideDir = Vector2.Perpendicular(toTarget * strafeDirection);
            float sideDot = Vector2.Dot(sideDir, dir);

            float value = Mathf.Max(0f, sideDot) * strafeStrength * strafeWeight;
            if (value > interest[i])
                interest[i] = value;
        }

        interestTemp = interest;
        return (danger, interest);
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos || !Application.isPlaying) return;

        if (interestTemp != null)
        {
            for (int i = 0; i < Directions.eightDirections.Count; i++)
            {
                Gizmos.color = Color.Lerp(Color.blue, Color.green, interestTemp[i]);
                Gizmos.DrawRay(transform.position, Directions.eightDirections[i] * interestTemp[i] * 2f);
            }
        }
    }
}
