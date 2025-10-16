using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyWave
{
    public GameObject[] enemies; // each wave’s enemy prefabs
}

public class EnemyWaveManager : MonoBehaviour
{
    [Header("Waves")]
    public List<EnemyWave> waves = new List<EnemyWave>(); // list of waves
    public float delayBetweenWaves = 2f;

    [Header("Spawn Settings")]
    public Vector2 spawnAreaSize = new Vector2(10, 6);
    public Transform[] spawnPoints; // optional fixed spawn points

    [Header("Optional Barrier")]
    public List<BarrierController> barriers = new List<BarrierController>();

    private List<DamageableCharacter> activeEnemies = new List<DamageableCharacter>();
    private int currentWave = 0;

    private void OnEnable()
    {
        Debug.Log($"[WaveManager] OnEnable() — starting encounter");
        if (Application.isPlaying)
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
    }

    private void SpawnWave(EnemyWave wave)
    {
        activeEnemies.RemoveAll(e => e == null);
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

            Vector3 spawnPos = GetSpawnPosition();
            var e = Instantiate(enemyPrefab, spawnPos, Quaternion.identity, transform);
            DamageableCharacter dmg = e.GetComponent<DamageableCharacter>();

            if (dmg != null && !e.CompareTag("Player"))
                activeEnemies.Add(dmg);

            Debug.Log($"[WaveManager] Spawned {enemyPrefab.name} at {spawnPos}");
        }
    }

    private Vector3 GetSpawnPosition()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int i = Random.Range(0, spawnPoints.Length);
            return spawnPoints[i].position;
        }

        Vector2 offset = new Vector2(
            Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f),
            Random.Range(-spawnAreaSize.y / 2f, spawnAreaSize.y / 2f)
        );

        return transform.position + (Vector3)offset;
    }

    public void RemoveEnemy(DamageableCharacter enemy)
    {
        if (!activeEnemies.Contains(enemy))
            return; // only handle enemies spawned by this wave manager

        activeEnemies.Remove(enemy);
        Debug.Log($"[WaveManager] Enemy removed. Remaining = {activeEnemies.Count}");

        if (activeEnemies.Count <= 0)
        {
            Debug.Log("[WaveManager] Wave cleared!");
            // StartCoroutine(NextWaveDelay());
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

}
