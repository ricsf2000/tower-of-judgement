using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;
using System;

public class EnterCodeMenu : MonoBehaviour
{
    [Header("Canvas References")]
    public GameObject enterCodeMenu;      // This menu
    public GameObject pauseMenuObject;           // Pause menu to return to
    public PauseMenu pauseMenu;          // The script to call ResumeGame() on

    [Header("UI References")]
    public TMP_InputField codeInputField;
    public GameObject firstSelectedButton;
    public TMP_Text errorText;

    private PlayerInput playerInput;

    public bool IsOpen => enterCodeMenu.activeSelf;

    private Dictionary<string, string> codeToScene = new Dictionary<string, string>()
    {
        { "boss", "Level1-7-Boss-Room" },
        {"dev1", "Level1-1" }, {"dev2", "Level1-2" },
         {"dev3", "Level1-3" }, {"dev4", "Level1-4" }, 
         {"dev5", "Level1-5" }, {"dev6", "Level1-6" },
    };

    private readonly Dictionary<string, DifficultyLevel> codeToDifficulty = new()
    {
        { "deveasy", DifficultyLevel.Easy },
        { "devnor", DifficultyLevel.Normal },
        { "devhard", DifficultyLevel.Hard }
    };


    private void Start()
    {
        playerInput = FindFirstObjectByType<PlayerInput>();
        enterCodeMenu.SetActive(false);
        if (errorText != null) errorText.text = "";
    }

    // Called by the Enter Code button in the PauseMenu
    public void OpenCodeMenu()
    {
        pauseMenuObject.SetActive(false);
        enterCodeMenu.SetActive(true);

        Time.timeScale = 0f;
        PauseMenu.isPaused = true;

        playerInput.SwitchCurrentActionMap("UI");

        codeInputField.text = "";
        codeInputField.Select();

        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }

    public void CloseCodeMenu()
    {
        enterCodeMenu.SetActive(false);
        pauseMenuObject.SetActive(true);

        Time.timeScale = 0f;
        PauseMenu.isPaused = true;

        playerInput.SwitchCurrentActionMap("UI");

        EventSystem.current.SetSelectedGameObject(pauseMenu.GetComponent<PauseMenu>().firstSelectedButton);
    }

    public void SubmitCode()
    {
        string entered = codeInputField.text.Trim();

        if (TryLoadSceneFromCode(entered))
            return;

        if (codeToDifficulty.TryGetValue(entered, out var difficulty))
        {
            DifficultySettings.CurrentDifficulty = difficulty;
            if (errorText != null)
                errorText.text = $"Difficulty set to {DifficultySettings.GetDisplayName(difficulty)}.";

            codeInputField.text = "";
            codeInputField.DeactivateInputField();

            if (EventSystem.current != null && firstSelectedButton != null)
                EventSystem.current.SetSelectedGameObject(firstSelectedButton);

            return;
        }

        errorText.text = "Incorrect code. Try again.";
    }

    public void OnInputFieldClicked()
    {
        codeInputField.Select();
        codeInputField.ActivateInputField();
    }

    private bool TryLoadSceneFromCode(string enteredCode)
    {
        if (string.IsNullOrWhiteSpace(enteredCode))
            return false;

        if (codeToScene.TryGetValue(enteredCode, out var sceneName))
        {
            LoadScene(sceneName);
            return true;
        }

        if (enteredCode.StartsWith("dev", StringComparison.OrdinalIgnoreCase))
        {
            var suffix = enteredCode.Substring(3);
            if (int.TryParse(suffix, out var levelIndex))
            {
                string derivedScene = levelIndex switch
                {
                    >= 1 and <= 6 => $"Level1-{levelIndex}",
                    7 => "Level1-7-Boss-Room",
                    _ => null
                };

                if (!string.IsNullOrEmpty(derivedScene))
                {
                    LoadScene(derivedScene);
                    return true;
                }
            }
        }

        return false;
    }

    private void LoadScene(string sceneName)
    {
        // Use PauseMenu logic to fully unpause audio + time
        pauseMenu.ResumeGame();

        // Reset cutscene stuff
        CutsceneDialogueController.SetCutsceneActive(false);
        CutsceneDialogueController.SetCutsceneLock(false);

        SceneManager.LoadScene(sceneName);
    }
}