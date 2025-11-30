using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CheckpointRestorer : MonoBehaviour
{
    private void Start()
    {
        if (!CheckpointGameData.hasCheckpoint) return;

        if (CheckpointGameData.sceneName != UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
            return;

        RestorePlayer();
        RestoreSwitches();
        RestoreBarriers();
        RestoreWaves();
        RestorePersistentEnemies();

        Debug.Log("[Checkpoint] Scene restored.");
    }

    private void RestorePlayer()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        var dmg = player.GetComponent<PlayerDamageable>();
        if (dmg != null)
        {
            //Override what PlayerDamageable.Start() set
            dmg._health = CheckpointGameData.playerHealth;

            //Sync back to PlayerData
            if (PlayerData.Instance != null)
                PlayerData.Instance.currentHealth = dmg._health;

            //Force UI to refresh
            if (GameEvents.Instance != null)
                GameEvents.Instance.PlayerHealthChanged(dmg._health, dmg.maxHealth);
        }

        // Restore saved position
        player.transform.position = CheckpointGameData.playerPosition;
    }

    private void RestoreSwitches()
    {
        var switches = FindObjectsOfType<SwitchController>(includeInactive: true);

        var lookup = new Dictionary<string, SwitchController>();
        foreach (var switchController in switches)
        {
            var key = switchController.GetPersistenceKey();
            if (string.IsNullOrEmpty(key))
                continue;

            lookup[key] = switchController;
        }

        foreach (var state in CheckpointGameData.switchStates)
        {
            if (!lookup.TryGetValue(state.switchID, out var target))
                continue;

            target.RestoreState(state.isActivated, shouldPersist: false, instant: true);
        }
    }

    private void RestoreBarriers()
    {
        var allBarriers = FindObjectsOfType<BarrierController>();
        var lookup = allBarriers.ToDictionary(b => b.GetPersistenceKey(), b => b);

        foreach (var state in CheckpointGameData.barrierStates)
        {
            if (!lookup.TryGetValue(state.barrierID, out var barrier) || barrier == null)
                continue;

            if (state.isActive)
                barrier.ActivateBarrier();
            else
                barrier.DeactivateBarrier();
        }
    }
     private void RestoreWaves()
    {
        var managers = FindObjectsOfType<EnemyWaveManager>();

        foreach (var state in CheckpointGameData.waveStates)
        {
            var wm = managers.FirstOrDefault(x => x.persistentID == state.managerID);
            if (wm == null) continue;

            wm.SkipToWave(state.currentWave); 
        }
    }

    private void RestorePersistentEnemies()
    {
        if (CheckpointGameData.persistentEnemyStates == null || CheckpointGameData.persistentEnemyStates.Count == 0)
            return;

        string currentScene = SceneManager.GetActiveScene().name;

        var deadLookup = CheckpointGameData.persistentEnemyStates
            .Where(state => state.sceneName == currentScene && state.isDead)
            .GroupBy(state => state.enemyID)
            .ToDictionary(group => group.Key, group => group.Last().isDead);

        if (deadLookup.Count == 0)
            return;

        var persistentEnemies = FindObjectsOfType<PersistentEnemy>();

        foreach (var enemy in persistentEnemies)
        {
            var key = enemy.PersistenceID;
            if (string.IsNullOrEmpty(key))
                continue;

            if (deadLookup.TryGetValue(key, out var shouldBeDead) && shouldBeDead)
            {
                enemy.ApplySavedState(true);
            }
        }
    }
}