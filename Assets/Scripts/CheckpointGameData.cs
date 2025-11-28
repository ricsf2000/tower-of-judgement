using UnityEngine;
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
}

[System.Serializable]
public class BarrierState
{
    public int barrierID;
    public bool isActive;
}

[System.Serializable]
public class WaveState
{
    public string managerID;
    public int currentWave;
}
