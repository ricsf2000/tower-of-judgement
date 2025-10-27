using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class UIHandler : MonoBehaviour
{
    public UIDocument uiDocument;

    private VisualElement healthBarBackground;
    private VisualElement healthBar;
    private float initialHealthBarWidth;
    private bool isInitialized = false;

    private void OnEnable()
    {
        StartCoroutine(WaitForGameEvents());
    }

    private IEnumerator WaitForGameEvents()
    {
        while (GameEvents.Instance == null)
            yield return null;

        Debug.Log("[UIHandler] Subscribing to GameEvents");
        GameEvents.Instance.OnPlayerHealthChanged += UpdateHealthUI;

        // Wait for initialization to complete
        while (!isInitialized)
            yield return null;

        // Now sync with current health
        if (PlayerData.Instance != null)
        {
            UpdateHealthUI(PlayerData.Instance.currentHealth, PlayerData.Instance.maxHealth);
            Debug.Log($"[UIHandler] Initial UI synced to {PlayerData.Instance.currentHealth}/{PlayerData.Instance.maxHealth}");
        }
    }

    private void OnDisable()
    {
        if (GameEvents.Instance != null)
        {
            Debug.Log("[UIHandler] Unsubscribing from GameEvents");
            GameEvents.Instance.OnPlayerHealthChanged -= UpdateHealthUI;
        }
    }

    void Start()
    {
        if (uiDocument == null)
        {
            Debug.LogError("[UIHandler] No UIDocument assigned!");
            return;
        }

        var root = uiDocument.rootVisualElement;
        healthBarBackground = root.Q<VisualElement>("HealthBarBackground");
        healthBar = root.Q<VisualElement>("HealthBar");

        if (healthBarBackground == null)
        {
            Debug.LogError("[UIHandler] Could not find HealthBarBackground!");
            return;
        }

        if (healthBar == null)
        {
            Debug.LogError("[UIHandler] Could not find HealthBar!");
            return;
        }

        // Capture initial width before allowing updates
        StartCoroutine(CaptureInitialWidth());

        Debug.Log("[UIHandler] Health bar elements found successfully");
    }

    private IEnumerator CaptureInitialWidth()
    {
        yield return null; // Wait for layout pass

        initialHealthBarWidth = healthBar.resolvedStyle.width;
        isInitialized = true; // Mark as ready
        Debug.Log($"[UIHandler] Captured initial health bar width: {initialHealthBarWidth}px");
    }

    private void UpdateHealthUI(float currentHealth, float maxHealth)
    {
        if (healthBar == null || maxHealth <= 0 || initialHealthBarWidth <= 0)
        {
            Debug.LogWarning($"[UIHandler] UpdateHealthUI blocked - healthBar: {healthBar != null}, maxHealth: {maxHealth}, initialWidth: {initialHealthBarWidth}");
            return;
        }

        // Calculate health percentage (0 to 1)
        float healthPercentage = Mathf.Clamp01(currentHealth / maxHealth);

        // Scale the health bar width from its initial width (height is preserved automatically)
        float newWidth = initialHealthBarWidth * healthPercentage;
        healthBar.style.width = newWidth;

        Debug.Log($"[UIHandler] Health bar updated: {currentHealth}/{maxHealth} ({healthPercentage * 100f}%) - Width: {newWidth}px");
    }
}