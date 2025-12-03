using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;


public class SettingsMenu : MonoBehaviour
{
    public AudioMixer audioMixer;

    public TMP_Dropdown resolutionDropdown;

    Resolution[] resolutions;

    [Header("Canvas References")]
    public GameObject settingsMenu;      // This menu
    public GameObject pauseMenuObject;           // Pause menu to return to
    public PauseMenu pauseMenu;          // The script to call ResumeGame() on

    [Header("UI References")]
    public GameObject firstSelectedButton;
    private PlayerInput playerInput;

    public bool IsOpen => settingsMenu.activeSelf;

    [Header("Main Menu Buttons")]
    public GameObject startButton;
    public GameObject settingsButton;
    public GameObject exitButton;


    void Start()
    {
        resolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height + " @ " + resolutions[i].refreshRateRatio + "hz";;
            options.Add(option);

            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        playerInput = FindFirstObjectByType<PlayerInput>();
        if (playerInput == null)
            Debug.LogWarning("[PauseMenu] No PlayerInput found in scene.");

        settingsMenu.SetActive(false);
    }

    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("MasterVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat("SFXVolume", volume);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    // Called by the Settings button in the PauseMenu
    public void OpenSettingsMenu()
    {
        // Clear selected object
        EventSystem.current.SetSelectedGameObject(null);

        pauseMenuObject.SetActive(false);
        settingsMenu.SetActive(true);

        Time.timeScale = 0f;
        PauseMenu.isPaused = true;

        playerInput.SwitchCurrentActionMap("UI");

        //Set new selected object
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }

    public void CloseSettingsMenu()
    {
        settingsMenu.SetActive(false);
        pauseMenuObject.SetActive(true);

        Time.timeScale = 0f;
        PauseMenu.isPaused = true;

        playerInput.SwitchCurrentActionMap("UI");

        EventSystem.current.SetSelectedGameObject(pauseMenu.GetComponent<PauseMenu>().firstSelectedButton);
    }

    public void OpenFromMainMenu()
    {
        // Hide individual main menu buttons
        startButton.SetActive(false);
        settingsButton.SetActive(false);
        exitButton.SetActive(false);

        // Show settings menu
        settingsMenu.SetActive(true);

        // Reset selection
        EventSystem.current.SetSelectedGameObject(null);

        // Force-select first settings item
        var s = firstSelectedButton.GetComponent<UnityEngine.UI.Selectable>();
        s.Select();
        s.OnSelect(null);  // Ensures controller navigation works IMMEDIATELY
    }

    public void CloseFromMainMenu()
    {
        // Hide settings menu
        settingsMenu.SetActive(false);

        // Show individual main menu buttons again
        startButton.SetActive(true);
        settingsButton.SetActive(true);
        exitButton.SetActive(true);

        // Give controller focus back to Start Button (or your choice)
        EventSystem.current.SetSelectedGameObject(null);

        var s = startButton.GetComponent<UnityEngine.UI.Selectable>();
        s.Select();
        s.OnSelect(null);
    }


}
