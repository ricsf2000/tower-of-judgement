using UnityEngine;

public class CheckpointController : MonoBehaviour
{   
    public string checkpointID;
    private bool triggered = false;
    [SerializeField] private bool repeatable = false;
    

    private void OnTriggerEnter2D(Collider2D col)
    {
        
        if (!col.CompareTag("Player")) return;
        if (!repeatable && triggered) return;
        bool saved = SaveCheckpoint();
        if (!repeatable && saved)
            triggered = true;
    }

    private bool SaveCheckpoint()
    {
        if (!repeatable)
        {
            if (CheckpointGameData.usedCheckpoints.Contains(checkpointID))
                return false;

            CheckpointGameData.usedCheckpoints.Add(checkpointID);
        }
        var player = GameObject.FindGameObjectWithTag("Player").transform;

        CheckpointGameData.hasCheckpoint = true;
        CheckpointGameData.sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        var box = GetComponent<BoxCollider2D>();
        Vector3 center = transform.position + (Vector3)box.offset;

        CheckpointGameData.playerPosition = center;

        // Save health
        var dmg = player.GetComponent<PlayerDamageable>();
        CheckpointGameData.playerHealth = dmg != null ? dmg.maxHealth : 100f;

        // Save waves
        var managers = FindObjectsOfType<EnemyWaveManager>();
        CheckpointGameData.waveStates.Clear();

       foreach (var wm in managers)
    {
        int waveIndexToSave = wm.GetCurrentWaveIndex();

        // If manager is cleared, wave index must be waves.Count
        if (waveIndexToSave >= wm.waves.Count)
            waveIndexToSave = wm.waves.Count;

        CheckpointGameData.waveStates.Add(new WaveState()
        {
            managerID = wm.persistentID,
            currentWave = waveIndexToSave
        });
    }

        // Save barriers (non-destructive; barriers persist even if destroyed)
        var barriers = FindObjectsOfType<BarrierController>();

        foreach (var barrier in barriers)
        {
            var barrierID = barrier.GetPersistenceKey();
            if (string.IsNullOrEmpty(barrierID))
                continue;

            CheckpointGameData.SetBarrierState(barrierID, barrier.IsActive);
        }

        // Save switch states (non-destructive; keeps previous progress if switches not yet spawned)
        var switches = FindObjectsOfType<SwitchController>();

        foreach (var switchController in switches)
        {
            var switchID = switchController.GetPersistenceKey();
            if (string.IsNullOrEmpty(switchID))
                continue;

            CheckpointGameData.SetSwitchState(switchID, switchController.IsActivated);
        }

        Debug.Log("[Checkpoint] Saved game progress.");
        return true;
    }
}
