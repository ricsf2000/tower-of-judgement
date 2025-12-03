using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


[System.Serializable]
public class EnemyWave
{
    public GameObject[] enemies; // each wave’s enemy prefabs
}

public class EnemyWaveManager : MonoBehaviour
{      
    public string persistentID;  
    [Header("Waves")]
    public List<EnemyWave> waves = new List<EnemyWave>(); // list of waves
    public float delayBetweenWaves = 2f;

    [Header("Spawn Settings")]
    public Vector2 spawnAreaSize = new Vector2(10, 6);
    public Transform[] spawnPoints; // optional fixed spawn points
    // public bool loopSpawnPoints = false; // if false, use random area when spawn points run out (hard code for now to not change all instances)

    [Header("Optional Barrier")]
    public List<BarrierController> barriers = new List<BarrierController>();

    private List<EnemyDamageable> activeEnemies = new List<EnemyDamageable>();
    private int currentWave = 0;
    public int GetCurrentWaveIndex() => currentWave;
    private int spawnedThisWave = 0;
    private List<int> availableSpawnIndices = new List<int>();

    // For Michael boss
    public static event System.Action OnAllWavesCleared;
    public void SkipToWave(int wave)
{
    currentWave = Mathf.Clamp(wave, 0, waves.Count);
}

  private void OnEnable()
    {
        if (!Application.isPlaying)
            return;

        Debug.Log("[WaveManager] OnEnable()");

        // Find saved state for this wave manager
        var saved = CheckpointGameData.waveStates
            .FirstOrDefault(s => s.managerID == persistentID);

        // If checkpoint exists and state exists for this room
        if (CheckpointGameData.hasCheckpoint && saved != null)
        {
            currentWave = Mathf.Clamp(saved.currentWave, 0, waves.Count);

            // Room was fully cleared before checkpoint → do nothing
            if (currentWave >= waves.Count)
            {
                foreach (var b in barriers)
                    if (b != null)
                        b.DeactivateBarrier();

                Debug.Log("[WaveManager] Room cleared before checkpoint — no spawn.");
                return;
            }

            // Otherwise resume from saved wave
            StartCoroutine(BeginEncounter());
            return;
        }

        // Fresh room start
        StartCoroutine(BeginEncounter());
    }



    private IEnumerator BeginEncounter()
    {
        Debug.Log("[WaveManager] BeginEncounter() running");

        if (waves == null || waves.Count == 0)
        {
            Debug.LogError("[WaveManager] No waves assigned! Add at least one in the Inspector.");
            yield break;
        }

        if (barriers != null && barriers.Count > 0)
        {
            foreach (var b in barriers)
            {
                if (b != null)
                {
                    if (!b.gameObject.activeSelf)
                        b.gameObject.SetActive(true);   // ensure the barrier is active in the scene

                    b.ActivateBarrier();                 // now trigger fade-in/visual enable
                }
            }
        }

        while (currentWave < waves.Count)
        {
            Debug.Log($"[WaveManager] Spawning wave {currentWave}");
            SpawnWave(waves[currentWave]);

            yield return new WaitUntil(() => activeEnemies.Count == 0);
            Debug.Log($"[WaveManager] Wave {currentWave} cleared!");

            currentWave++;
            yield return new WaitForSeconds(delayBetweenWaves);
        }

        if (barriers != null && barriers.Count > 0)
        {
            foreach (var b in barriers)
                if (b != null)
                    b.DeactivateBarrier();
        }

        Debug.Log("[WaveManager] All waves cleared!");

        // For Michael boss
        OnAllWavesCleared?.Invoke();
    }

    private void SpawnWave(EnemyWave wave)
    {
        activeEnemies.RemoveAll(e => e == null);
        spawnedThisWave = 0; // Reset counter at start of wave
        
        // Reset available spawn points for this wave
        availableSpawnIndices.Clear();
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                availableSpawnIndices.Add(i);
            }
        }
        
        if (wave == null || wave.enemies == null || wave.enemies.Length == 0)
        {
            Debug.LogWarning("[WaveManager] Empty wave — skipping");
            return;
        }

        foreach (var enemyPrefab in wave.enemies)
        {
            if (enemyPrefab == null)
            {
                Debug.LogWarning("[WaveManager] Null enemy in wave — skipping");
                continue;
            }

            // Check if we've run out of spawn points
            if (spawnPoints != null && spawnPoints.Length > 0 && availableSpawnIndices.Count == 0)
            {
                Debug.LogWarning($"[WaveManager] Ran out of spawn points! Only spawned {spawnedThisWave} enemies.");
                break; // Stop spawning
            }

            Vector3 spawnPos = GetSpawnPosition();
            var e = Instantiate(enemyPrefab, spawnPos, Quaternion.identity, transform);
            spawnedThisWave++; // Increment after each spawn

            // Get EnemyDamageable
            EnemyDamageable enemyDmg = e.GetComponent<EnemyDamageable>();
            if (enemyDmg != null)
            {
                enemyDmg.SpawnedByWave = true;
                activeEnemies.Add(enemyDmg);

                // Relay handles spawn animation trigger
                var relay = e.GetComponentInChildren<AnimatorRelay>();
                if (relay != null)
                    relay.PlaySpawnAnimation();

                // Random delay before AI activates
                if (enemyDmg != null)
                    StartCoroutine(enemyDmg.SpawnDelay());
            }
            else
            {
                Debug.LogWarning($"[WaveManager] Spawned {enemyPrefab.name} has no EnemyDamageable component!");
            }

            Debug.Log($"[WaveManager] Spawned {enemyPrefab.name} at {spawnPos}");
        }
    }



    private Vector3 GetSpawnPosition()
    {
        if (spawnPoints != null && spawnPoints.Length > 0 && availableSpawnIndices.Count > 0)
        {
            // Pick a random index from available spawn points
            int randomIndex = Random.Range(0, availableSpawnIndices.Count);
            int spawnPointIndex = availableSpawnIndices[randomIndex];
            
            // Remove this spawn point from available list
            availableSpawnIndices.RemoveAt(randomIndex);
            
            return spawnPoints[spawnPointIndex].position;
        }

        // Random area spawning (fallback if no spawn points)
        Vector2 offset = new Vector2(
            Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f),
            Random.Range(-spawnAreaSize.y / 2f, spawnAreaSize.y / 2f)
        );

        return transform.position + (Vector3)offset;
    }

    public void RemoveEnemy(EnemyDamageable enemy)
    {
        if (!activeEnemies.Contains(enemy))
            return;

        activeEnemies.Remove(enemy);
        Debug.Log($"[WaveManager] Enemy removed. Remaining = {activeEnemies.Count}");

        if (activeEnemies.Count <= 0)
        {
            Debug.Log("[WaveManager] Wave cleared!");
        }
    }

    private void OnDrawGizmos()
    {
        // Set gizmo color for the spawn area
        Gizmos.color = new Color(1f, 1f, 0f, 0.35f); // semi-transparent yellow

        // Draw the random spawn area box
        Gizmos.DrawCube(transform.position, new Vector3(spawnAreaSize.x, spawnAreaSize.y, 0.1f));

        // Draw a wire outline for clarity
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnAreaSize.x, spawnAreaSize.y, 0.1f));

        // Draw lines to show spawn points if any exist
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Gizmos.color = Color.cyan;
            foreach (var point in spawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawSphere(point.position, 0.2f);
                    Gizmos.DrawLine(transform.position, point.position);
                }
            }
        }
    }
        public bool WasClearedBeforeCheckpoint()
    {
        var saved = CheckpointGameData.waveStates
            .FirstOrDefault(s => s.managerID == persistentID);

        if (saved == null) return false;

        return saved.currentWave >= waves.Count;
    }

}
