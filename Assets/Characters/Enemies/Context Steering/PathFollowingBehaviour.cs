using UnityEngine;

[CreateAssetMenu(menuName = "Steering/Path Following")]
public class PathFollowingBehaviour : SteeringBehaviour
{
    public override (float[], float[]) GetSteering(float[] danger, float[] interest, AIData aiData)
    {
        if (aiData.currentWaypoint == null)
            return (danger, interest);

        Vector2 dirToWaypoint = ((Vector2)aiData.currentWaypoint - (Vector2)aiData.position).normalized;

        int bestDirIndex = Directions.GetClosestDirectionIndex(dirToWaypoint);

        // boost interest in that direction
        interest[bestDirIndex] += 1.5f;

        return (danger, interest);
    }
}
