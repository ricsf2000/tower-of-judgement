using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyDamageable : DamageableCharacter
{
    public bool SpawnedByWave = false;

    [Header("Loot")]
    public List<LootItems> lootTable = new List<LootItems>();

    [Header("Spawn Delay Settings")]
    public float minSpawnDelay = 0.5f;
    public float maxSpawnDelay = 2.0f;
    [HideInInspector] public bool hasSpawned = false;

    protected override void Start()
    {
        base.Start();

        // If enemy is not wave-spawned, mark as spawned immediately
        if (!SpawnedByWave)
            hasSpawned = true;
        else
            hasSpawned = false;  // WaveManager will trigger SpawnDelay
    }
    
     protected override void HandleDeath()
    {
        base.HandleDeath();
        EnemyWaveManager manager = GetComponentInParent<EnemyWaveManager>() 
                                   ?? FindFirstObjectByType<EnemyWaveManager>();
        if (manager != null)
            manager.RemoveEnemy(this);
    }

    protected override IEnumerator DeathSequence()
    {
        foreach(LootItems lootItem in lootTable)
        {
            if(Random.Range(0f, 100f) <= lootItem.dropChance)
            {
                InstantiateLoot(lootItem.itemPrefab);
            }
            break;
        }

        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }

    void InstantiateLoot(GameObject loot)
    {
        if(loot)
        {
            GameObject droppedLoot = Instantiate(loot, transform.position, Quaternion.identity);
        }
    }

    public IEnumerator SpawnDelay()
    {
        float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
        yield return new WaitForSeconds(delay);
        hasSpawned = true;

        Debug.Log($"[{name}] Finished spawn delay ({delay:F2}s) — AI active.");
    }
}
