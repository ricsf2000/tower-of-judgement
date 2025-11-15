using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DifficultySelector : MonoBehaviour
{
    [Header("UI References")]
    public Button easyButton;
    public Button normalButton;
    public Button hardButton;
    
    private MenuManager menuManager;
    
    void Start()
    {
        menuManager = FindFirstObjectByType<MenuManager>();
        if (menuManager == null)
            Debug.LogError("[DifficultySelector] MenuManager not found!");
        
        // Setup difficulty buttons to select and immediately start
        if (easyButton != null)
            easyButton.onClick.AddListener(() => SelectDifficultyAndStart(DifficultyLevel.Easy));
        else
            Debug.LogWarning("[DifficultySelector] Easy button not assigned!");
        
        if (normalButton != null)
            normalButton.onClick.AddListener(() => SelectDifficultyAndStart(DifficultyLevel.Normal));
        else
            Debug.LogWarning("[DifficultySelector] Normal button not assigned!");
        
        if (hardButton != null)
            hardButton.onClick.AddListener(() => SelectDifficultyAndStart(DifficultyLevel.Hard));
        else
            Debug.LogWarning("[DifficultySelector] Hard button not assigned!");
    }
    
    private void SelectDifficultyAndStart(DifficultyLevel difficulty)
    {
        DifficultySettings.CurrentDifficulty = difficulty;
        
        if (menuManager != null)
            menuManager.ConfirmDifficultyAndStart();
    }
}