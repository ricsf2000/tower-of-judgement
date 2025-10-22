using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public static GameEvents Instance { get; private set; }

    public event System.Action<float, float> OnPlayerHealthChanged;
    public event System.Action<DamageableCharacter> OnAnyCharacterDeath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // --- Auto-bootstrap before any scene objects load ---
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureGameEventsExists()
    {
        if (Instance == null)
        {
            Debug.Log("[GameEvents] Auto-spawning persistent GameEvents singleton");

            var obj = new GameObject("GameEvents");
            obj.AddComponent<GameEvents>();
            DontDestroyOnLoad(obj);
        }
    }

    // --- Event invocation helpers ---
    public void PlayerHealthChanged(float current, float max)
    {
        OnPlayerHealthChanged?.Invoke(current, max);
    }

    public void CharacterDied(DamageableCharacter c)
    {
        OnAnyCharacterDeath?.Invoke(c);
    }
}
