using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance { get; private set; }

    [Header("Player Stats")]
    public float maxHealth = 6f;  // Single source of truth for max health
    public float currentHealth = 6f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // stays across all scenes
        
        // Apply difficulty settings to max health
        maxHealth = DifficultySettings.GetMaxHealth();
        
        // Initialize current health to max if not set
        if (currentHealth <= 0)
            currentHealth = maxHealth;
    }

    // Auto-create if missing when game starts
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureExists()
    {
        if (Instance == null)
        {
            GameObject obj = new GameObject("PlayerDataManager");
            obj.AddComponent<PlayerData>();
            Debug.Log("[PlayerData] Auto-created at game start.");
        }
    }

    public void SetHealth(float value)
    {
        currentHealth = Mathf.Clamp(value, 0, maxHealth);
    }

    public void RestoreFullHealth()
    {
        currentHealth = maxHealth;
    }
}