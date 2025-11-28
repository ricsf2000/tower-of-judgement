using UnityEngine;
using System.Linq;

public class CheckpointRestorer : MonoBehaviour
{
    private void Start()
    {
        if (!CheckpointGameData.hasCheckpoint) return;

        if (CheckpointGameData.sceneName != UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
            return;

        RestorePlayer();
        RestoreBarriers();
        RestoreWaves();

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

    private void RestoreBarriers()
    {
        var allBarriers = FindObjectsOfType<BarrierController>();

        foreach (var state in CheckpointGameData.barrierStates)
        {
            var b = allBarriers.FirstOrDefault(x => x.GetInstanceID() == state.barrierID);
            if (b == null) continue;

            if (state.isActive)
                b.ActivateBarrier();     // from BarrierController.cs  :contentReference[oaicite:0]{index=0}
            else
                b.DeactivateBarrier();
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
}
