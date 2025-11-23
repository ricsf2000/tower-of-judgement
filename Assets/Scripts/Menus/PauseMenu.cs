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
    public EnterCodeMenu enterCodeMenu;

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
        // CUTSCENE MODE — do NOT modify action maps
        if (CutsceneDialogueController.IsCutsceneActive)
        {
            pauseMenu.SetActive(true);
            Time.timeScale = 0f;
            isPaused = true;

            masterMixer.SetFloat("MasterSFXPitch", 0f);

            StartCoroutine(FadeMusic(0.05f, 0.25f));
            return;
        }

        // NORMAL PAUSE
        pauseMenu.SetActive(true);
        Time.timeScale = 0.0f;
        isPaused = true;

        masterMixer.SetFloat("MasterSFXPitch", 0f);

        StartCoroutine(FadeMusic(0.05f, 0.25f));

        if (!CutsceneDialogueController.CutsceneLocksActionMap)
            playerInput.SwitchCurrentActionMap("UI");

        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }


    public void ResumeGame()
    {
        // CUTSCENE MODE — do NOT modify action maps
        if (CutsceneDialogueController.IsCutsceneActive)
        {
            pauseMenu.SetActive(false);
            Time.timeScale = 1f;
            isPaused = false;

            masterMixer.SetFloat("MasterSFXPitch", 1f);

            StartCoroutine(FadeMusic(MusicManager.Instance.defaultVolume, 0.25f));
            return;
        }

        // NORMAL RESUME
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        masterMixer.SetFloat("MasterSFXPitch", 1f);

        StartCoroutine(FadeMusic(MusicManager.Instance.defaultVolume, 0.25f));

        if (!CutsceneDialogueController.CutsceneLocksActionMap)
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

    public void OpenEnterCodeMenu()
    {
        enterCodeMenu.OpenCodeMenu();
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
