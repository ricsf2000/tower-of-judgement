using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance { get; private set; }

    [Header("Player Stats")]
    public float currentHealth = 3f;
    public float maxHealth = 3f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // stays across all scenes
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
