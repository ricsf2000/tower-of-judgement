using UnityEngine;

public enum DifficultyLevel
{
    Easy,
    Normal,
    Hard
}

public static class DifficultySettings
{
    private static DifficultyLevel _currentDifficulty = DifficultyLevel.Normal;
    
    public static DifficultyLevel CurrentDifficulty 
    { 
        get => _currentDifficulty;
        set 
        { 
            _currentDifficulty = value;
            ApplyDifficultySettings();
            SaveDifficulty();
        }
    }
    
    private static readonly DifficultyConfig[] Configs = 
    {
        new DifficultyConfig { Level = DifficultyLevel.Easy, MaxHealth = 12f, DisplayName = "Easy" },
        new DifficultyConfig { Level = DifficultyLevel.Normal, MaxHealth = 6f, DisplayName = "Normal" },
        new DifficultyConfig { Level = DifficultyLevel.Hard, MaxHealth = 3f, DisplayName = "Hard" }
    };
    
    [System.Serializable]
    private struct DifficultyConfig
    {
        public DifficultyLevel Level;
        public float MaxHealth;
        public string DisplayName;
    }
    
    static DifficultySettings()
    {
        LoadDifficulty();
    }
    
    public static float GetMaxHealth()
    {
        return GetConfig(_currentDifficulty).MaxHealth;
    }
    
    public static string GetDisplayName(DifficultyLevel difficulty)
    {
        return GetConfig(difficulty).DisplayName;
    }
    
    public static string GetCurrentDisplayName()
    {
        return GetConfig(_currentDifficulty).DisplayName;
    }
    
    private static DifficultyConfig GetConfig(DifficultyLevel difficulty)
    {
        foreach (var config in Configs)
        {
            if (config.Level == difficulty)
                return config;
        }
        return Configs[1]; // Default to Normal
    }
    
    private static void ApplyDifficultySettings()
    {
        if (PlayerData.Instance != null)
        {
            float newMaxHealth = GetMaxHealth();
            PlayerData.Instance.maxHealth = newMaxHealth;
            PlayerData.Instance.RestoreFullHealth();
            Debug.Log($"[DifficultySettings] Applied {GetCurrentDisplayName()} difficulty: {newMaxHealth} max health");
        }
    }
    
    private static void SaveDifficulty()
    {
        PlayerPrefs.SetInt("Difficulty", (int)_currentDifficulty);
        PlayerPrefs.Save();
    }
    
    private static void LoadDifficulty()
    {
        int savedDifficulty = PlayerPrefs.GetInt("Difficulty", (int)DifficultyLevel.Normal);
        _currentDifficulty = (DifficultyLevel)savedDifficulty;
    }
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InitializeDifficulty()
    {
        LoadDifficulty();
        ApplyDifficultySettings();
        Debug.Log($"[DifficultySettings] Initialized with {GetCurrentDisplayName()} difficulty");
    }
}