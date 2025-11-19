using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyDamageable : DamageableCharacter
{
    public bool SpawnedByWave = false;

    [Header("Loot")]
    public List<LootItems> lootTable = new List<LootItems>();

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
}
