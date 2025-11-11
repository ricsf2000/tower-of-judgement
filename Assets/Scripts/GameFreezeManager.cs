using UnityEngine;
using UnityEngine.InputSystem;

public class GameFreezeManager : MonoBehaviour
{
    public static GameFreezeManager Instance { get; private set; }

    private bool isFrozen = false;
    public bool IsFrozen => isFrozen;

    private PlayerInput playerInput;

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

    public void FreezeGame()
    {
        if (isFrozen) return;
        isFrozen = true;

        // 1Disable player input
        if (playerInput == null)
            playerInput = FindAnyObjectByType<PlayerInput>();

        if (playerInput != null)
            playerInput.enabled = false;

        // 2Freeze all rigidbodies manually
        foreach (var rb in FindObjectsOfType<Rigidbody2D>())
        {
            rb.simulated = false;
        }
    }

    public void UnfreezeGame()
    {
        if (!isFrozen) return;
        isFrozen = false;

        // Re-enable player input
        if (playerInput == null)
            playerInput = FindAnyObjectByType<PlayerInput>();

        if (playerInput != null)
            playerInput.enabled = true;

        // Resume physics
        foreach (var rb in FindObjectsOfType<Rigidbody2D>())
        {
            rb.simulated = true;
        }
    }
}
