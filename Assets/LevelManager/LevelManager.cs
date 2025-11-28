using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class LevelManager : MonoBehaviour
{
    public static LevelManager manager { get; private set; }
    public GameObject deathPanel;
    public GameObject firstSelectedButton;

    private void Awake()
    {
        // Replace destroyed static reference safely
        if (manager != null && manager != this)
        {
            Destroy(gameObject);
            return;
        }

        manager = this;
    }

    private void OnEnable()
    {
        // Confirm reinitialization on scene load
        Debug.Log($"[LevelManager] Active in scene: {SceneManager.GetActiveScene().name}");
    }


    public void GameOver()
    {
        Debug.Log("[LevelManager] GameOver() called");
        deathPanel.SetActive(true);
         // Disable gameplay input
        var playerInput = FindFirstObjectByType<PlayerInput>();
        if (playerInput != null)
        {
            Debug.Log("[LevelManager] Switching to UI action map (input locked)");
            playerInput.SwitchCurrentActionMap("UI");
        }

        // Select Retry button automatically
        if (EventSystem.current != null && firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
            Debug.Log($"[LevelManager] Selected button: {firstSelectedButton.name}");
        }
    }
    public void Retry()
    {
        if (CheckpointGameData.hasCheckpoint)
        {
            StartCoroutine(ReloadCheckpoint());
        }
        else
        {
            StartCoroutine(ReloadAndReset());
        }
    }

    private IEnumerator ReloadCheckpoint()
    {
        SceneManager.LoadScene(CheckpointGameData.sceneName);
        yield return null;

        var player = FindFirstObjectByType<PlayerDamageable>();
        if (player != null)
        {
            player.transform.position = CheckpointGameData.playerPosition;
            player._health = CheckpointGameData.playerHealth;

            if (PlayerData.Instance != null)
                PlayerData.Instance.currentHealth = player._health;

            GameEvents.Instance?.PlayerHealthChanged(player._health, player.maxHealth);
        }

        var playerInput = FindFirstObjectByType<PlayerInput>();
        if (playerInput != null)
            playerInput.SwitchCurrentActionMap("Player");
    }

    private IEnumerator ReloadAndReset()
    {
        // Reset PlayerData before reloading
        if (PlayerData.Instance != null)
            PlayerData.Instance.RestoreFullHealth();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        yield return null; // wait for scene to fully load

        // Reconnect player to the new scene
        var player = FindFirstObjectByType<PlayerDamageable>();
        if (player != null)
            player.ResetPlayer(); // match the fresh PlayerData values
        
        var playerInput = FindFirstObjectByType<PlayerInput>();
        if (playerInput != null)
        {
            Debug.Log("[LevelManager] Restoring Player action map");
            playerInput.SwitchCurrentActionMap("Player");
        }
    }
}
