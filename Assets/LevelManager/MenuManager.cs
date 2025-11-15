using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
public class MenuManager : MonoBehaviour
{
    [Header("UI References")]
    public string difficultyPanelName = "DifficultySelectionPanel";
    private GameObject difficultySelectionPanel;
    
    private string pendingLevelName;
    
    private void FindDifficultyPanel()
    {
        if (difficultySelectionPanel == null)
        {
            // Find the difficulty panel by name (including inactive objects)
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name == difficultyPanelName && obj.scene.IsValid())
                {
                    difficultySelectionPanel = obj;
                    break;
                }
            }
            
            Debug.Log($"[MenuManager] Panel search result: {difficultySelectionPanel != null}");
            if (difficultySelectionPanel != null)
                Debug.Log($"[MenuManager] Panel found: {difficultySelectionPanel.name}");
            else
                Debug.LogError($"[MenuManager] Could not find panel with name: {difficultyPanelName}");
        }
    }
    
    public void StartGame(string levelName)
    {
        // Store the level name and show difficulty selection
        pendingLevelName = levelName;
        ShowDifficultySelection();
    }
    
    public void ShowDifficultySelection()
    {
        // Try to find the panel if we don't have it
        FindDifficultyPanel();
        
        if (difficultySelectionPanel != null)
        {
            difficultySelectionPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("[MenuManager] Difficulty selection panel not found!");
        }
    }
    
    public void ConfirmDifficultyAndStart()
    {
        Debug.Log($"[MenuManager] ConfirmDifficultyAndStart called. Pending level: '{pendingLevelName}'");
        
        // Reset health based on selected difficulty
        if (PlayerData.Instance != null)
        {
            PlayerData.Instance.RestoreFullHealth();
        }

        if (!string.IsNullOrEmpty(pendingLevelName))
        {
            Debug.Log($"[MenuManager] Loading scene: {pendingLevelName}");
            SceneManager.LoadScene(pendingLevelName);
        }
        else
        {
            Debug.LogError("[MenuManager] No pending level name! Loading default scene.");
            SceneManager.LoadScene("CutScene"); // fallback to default
        }
    }
    

    public void ChangeScene(string name)
    {
        SceneManager.LoadScene(name);
    }

    public void OnExitClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
