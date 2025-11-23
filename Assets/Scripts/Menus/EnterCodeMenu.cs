using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;

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
        { "boss", "Level1-7-Boss-Room" }
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

        if (codeToScene.ContainsKey(entered))
        {
            string sceneName = codeToScene[entered];

            // Use PauseMenu logic to fully unpause audio + time
            pauseMenu.ResumeGame();

            // Reset cutscene stuff
            CutsceneDialogueController.SetCutsceneActive(false);
            CutsceneDialogueController.SetCutsceneLock(false);

            SceneManager.LoadScene(sceneName);
            return;
        }

        errorText.text = "Incorrect code. Try again.";
    }

    public void OnInputFieldClicked()
    {
        codeInputField.Select();
        codeInputField.ActivateInputField();
    }

}
