using System.Collections.Generic;
using UnityEngine.SceneManagement;

public static class PersistentEnemyRuntime
{
    private static readonly Dictionary<string, KilledEnemyRecord> pendingKills = new();

    static PersistentEnemyRuntime()
    {
        SceneManager.sceneLoaded += (_, __) => pendingKills.Clear();
    }

    public static void MarkKilled(string sceneName, string enemyID)
    {
        if (string.IsNullOrEmpty(sceneName) || string.IsNullOrEmpty(enemyID))
            return;

        var key = BuildKey(sceneName, enemyID);

        if (!pendingKills.ContainsKey(key))
        {
            pendingKills[key] = new KilledEnemyRecord
            {
                sceneName = sceneName,
                enemyID = enemyID
            };
        }
    }

    public static IReadOnlyCollection<KilledEnemyRecord> PendingKills => pendingKills.Values;

    public static void ClearPendingKills()
    {
        pendingKills.Clear();
    }

    private static string BuildKey(string scene, string enemyID) => $"{scene}::{enemyID}";
}

public class KilledEnemyRecord
{
    public string sceneName;
    public string enemyID;
}