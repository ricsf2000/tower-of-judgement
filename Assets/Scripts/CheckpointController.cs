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

        // Save barriers
        var barriers = FindObjectsOfType<BarrierController>();
        CheckpointGameData.barrierStates.Clear();

        foreach (var b in barriers)
        {
            CheckpointGameData.barrierStates.Add(new BarrierState()
            {
                barrierID = b.GetInstanceID(),
                isActive = b.gameObject.activeSelf
            });
        }

        Debug.Log("[Checkpoint] Saved game progress.");
        return true;
    }
}
