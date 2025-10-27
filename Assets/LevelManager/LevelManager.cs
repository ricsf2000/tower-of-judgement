using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager manager { get; private set; }
    public GameObject deathPanel;

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
    }
    public void Retry()
    {
        StartCoroutine(ReloadAndReset());
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
    }
}
