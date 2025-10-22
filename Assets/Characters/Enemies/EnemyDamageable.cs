using UnityEngine;
using System.Collections;

public class EnemyDamageable : DamageableCharacter
{
    public bool SpawnedByWave = false;
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
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }
}
