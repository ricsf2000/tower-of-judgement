using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Audio;
using System.Collections;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public static bool isPaused;
    public GameObject firstSelectedButton;

    private PlayerInput playerInput;

    [SerializeField] private GameObject deathScreen;

    public AudioMixer masterMixer;



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

        // Pause audio
        masterMixer.SetFloat("MasterSFXPitch", 0f);

        // Fade the music
        StartCoroutine(FadeMusic(0.05f, 0.25f));

        playerInput.SwitchCurrentActionMap("UI");
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        
        // Resume audio
        masterMixer.SetFloat("MasterSFXPitch", 1f);

        // Fade music back in
        StartCoroutine(FadeMusic(MusicManager.Instance.defaultVolume,0.25f));

        playerInput.SwitchCurrentActionMap("Player");
    }
    
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        masterMixer.SetFloat("MasterSFXPitch", 1f);
        StartCoroutine(FadeMusic(MusicManager.Instance.defaultVolume,0.25f));
        SceneManager.LoadScene("Menu");
    }

    private IEnumerator FadeMusic(float targetVolume, float duration)
    {
        if (MusicManager.Instance == null) yield break;

        AudioSource music = MusicManager.Instance.GetComponent<AudioSource>();
        float start = music.volume;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // works while paused
            music.volume = Mathf.Lerp(start, targetVolume, t / duration);
            yield return null;
        }

        music.volume = targetVolume;
    }

}
