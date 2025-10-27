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

        // After subscribing, immediately update with current health
        yield return null; // wait 1 frame to ensure PlayerData/PlayerDamageable initialized

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

        // Wait one frame for layout to resolve, then capture initial width
        StartCoroutine(CaptureInitialWidth());

        Debug.Log("[UIHandler] Health bar elements found successfully");
    }

    private IEnumerator CaptureInitialWidth()
    {
        yield return null; // Wait for layout pass

        initialHealthBarWidth = healthBar.resolvedStyle.width;
        Debug.Log($"[UIHandler] Captured initial health bar width: {initialHealthBarWidth}px");
    }

    private void UpdateHealthUI(float currentHealth, float maxHealth)
    {
        if (healthBar == null || maxHealth <= 0 || initialHealthBarWidth <= 0)
            return;

        // Calculate health percentage (0 to 1)
        float healthPercentage = Mathf.Clamp01(currentHealth / maxHealth);

        // Scale the health bar from its initial width
        float newWidth = initialHealthBarWidth * healthPercentage;
        healthBar.style.width = newWidth;

        Debug.Log($"[UIHandler] Health bar updated: {currentHealth}/{maxHealth} ({healthPercentage * 100f}%) - Width: {newWidth}px");
    }
}