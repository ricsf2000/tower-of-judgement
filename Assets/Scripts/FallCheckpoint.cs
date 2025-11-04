using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Collider2D))]
public class FallCheckpoint : MonoBehaviour
{
    [Header("Checkpoint Options")]
    [Tooltip("Assign if this checkpoint is a tilemap region.")]
    [SerializeField] private Tilemap checkpointTilemap;

    [Tooltip("If true, logs checkpoint updates in the console.")]
    [SerializeField] private bool debugLogs = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        FallableCharacter fallable = other.GetComponent<FallableCharacter>();
        if (fallable == null)
            return;

        Vector3 newRespawnPos = Vector3.zero;

        // Tilemap checkpoint
        if (checkpointTilemap != null)
        {
            Vector3 world = other.bounds.center;
            Vector3Int cellPos = checkpointTilemap.WorldToCell(world);

            Vector3 tileCenter = checkpointTilemap.GetCellCenterWorld(cellPos);
            fallable.respawnPosition = tileCenter;

            if (checkpointTilemap.HasTile(cellPos))
            {
                // Snap to the center of that tile
                newRespawnPos = checkpointTilemap.GetCellCenterWorld(cellPos);
                fallable.respawnPosition = newRespawnPos;

                if (debugLogs)
                    Debug.Log($"[Checkpoint] Player stepped on tile {cellPos}, respawn set to {newRespawnPos}");
            }
            else if (debugLogs)
            {
                Debug.LogWarning($"[Checkpoint] Player entered tilemap trigger, but no tile found at {cellPos}");
            }
        }
        // Transform checkpoint
        else
        {
            newRespawnPos = transform.position;
            fallable.respawnPosition = newRespawnPos;

            if (debugLogs)
                Debug.Log($"[Checkpoint] Player reached single checkpoint at {newRespawnPos}");
        }
    }
}
