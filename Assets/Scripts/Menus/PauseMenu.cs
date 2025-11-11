using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public static bool isPaused;
    public GameObject firstSelectedButton;

    private PlayerInput playerInput;

    [SerializeField] private GameObject deathScreen;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pauseMenu.SetActive(false);
        playerInput = FindFirstObjectByType<PlayerInput>();
        if (playerInput == null)
            Debug.LogWarning("[PauseMenu] No PlayerInput found in scene.");

        if (!deathScreen)
        {
            deathScreen = GameObject.Find("DeathScreen");
            if (!deathScreen)
                Debug.LogWarning("[PauseMenu] No DeathScreen assigned or found.");
        }
    }

    public void PauseGame()
    {
        if (deathScreen != null && deathScreen.activeInHierarchy)
        {
            Debug.Log("[PauseMenu] Cannot pause — death screen is active.");
            return;
        }
        
        pauseMenu.SetActive(true);
        Time.timeScale = 0.0f;
        isPaused = true;
        playerInput.SwitchCurrentActionMap("UI");
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        playerInput.SwitchCurrentActionMap("Player");
    }
    
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene("Menu");
    }
}
