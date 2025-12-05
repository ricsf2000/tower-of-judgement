using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public static class CheckpointGameData
{
    public static bool hasCheckpoint = false;
    public static string sceneName;
    public static Vector3 playerPosition;
    public static float playerHealth;
    public static HashSet<string> usedCheckpoints = new HashSet<string>();

    public static List<BarrierState> barrierStates = new();
    public static List<WaveState> waveStates = new();
    public static List<SwitchState> switchStates = new();
    public static List<PersistentEnemyState> persistentEnemyStates = new();

    public static void ClearAll()
    {
        hasCheckpoint = false;
        sceneName = string.Empty;
        playerPosition = Vector3.zero;
        playerHealth = 0f;
        usedCheckpoints.Clear();
        barrierStates.Clear();
        waveStates.Clear();
        switchStates.Clear();
        persistentEnemyStates.Clear();
    }

    public static void SetSwitchState(string switchID, bool activated)
    {
        if (!hasCheckpoint)
            return;

        if (sceneName != SceneManager.GetActiveScene().name)
            return;

        if (string.IsNullOrEmpty(switchID))
            return;

        var state = switchStates.Find(s => s.switchID == switchID);

        if (state == null)
        {
            switchStates.Add(new SwitchState
            {
                switchID = switchID,
                isActivated = activated
            });
        }
        else
        {
            state.isActivated = activated;
        }
    }

    public static void SetBarrierState(string barrierID, bool isActive)
    {
        if (string.IsNullOrEmpty(barrierID))
            return;

        var state = barrierStates.Find(s => s.barrierID == barrierID);

        if (state == null)
        {
            barrierStates.Add(new BarrierState
            {
                barrierID = barrierID,
                isActive = isActive
            });
        }
        else
        {
            state.isActive = isActive;
        }
    }
    public static void SetPersistentEnemyState(string scene, string enemyID, bool isDead)
    {
        if (string.IsNullOrEmpty(scene) || string.IsNullOrEmpty(enemyID))
            return;

        var state = persistentEnemyStates.Find(s => s.sceneName == scene && s.enemyID == enemyID);

        if (state == null)
        {
            persistentEnemyStates.Add(new PersistentEnemyState
            {
                sceneName = scene,
                enemyID = enemyID,
                isDead = isDead
            });
        }
        else
        {
            state.isDead = isDead;
        }
    }

    public static bool TryGetPersistentEnemyState(string scene, string enemyID, out bool isDead)
    {
        isDead = false;

        if (string.IsNullOrEmpty(scene) || string.IsNullOrEmpty(enemyID))
            return false;

        var state = persistentEnemyStates.Find(s => s.sceneName == scene && s.enemyID == enemyID);

        if (state == null)
            return false;

        isDead = state.isDead;
        return true;
    }
}

[System.Serializable]
public class BarrierState
{
    public string barrierID;
    public bool isActive;
}

[System.Serializable]
public class WaveState
{
    public string managerID;
    public int currentWave;
}

[System.Serializable]
public class SwitchState
{
    public string switchID;
    public bool isActivated;
}
[System.Serializable]
public class PersistentEnemyState
{
    public string sceneName;
    public string enemyID;
    public bool isDead;
}
