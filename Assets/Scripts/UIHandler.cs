using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIHandler : MonoBehaviour
{
    public UIDocument uiDocument;

    private VisualElement healthContainer;
    private List<VisualElement> healthSquares = new List<VisualElement>();

    private void OnEnable()
    {
        if (GameEvents.Instance != null)
        {
            Debug.Log("[UIHandler] Subscribing to health change");
            GameEvents.Instance.OnPlayerHealthChanged += UpdateHealthUI;
        }
        else
        {
            Debug.LogWarning("[UIHandler] GameEvents instance missing on enable!");
        }
    }

    private void OnDisable()
    {
        if (GameEvents.Instance != null)
        {
            Debug.Log("[UIHandler] Unsubscribing from health change");
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
        healthContainer = root.Q<VisualElement>("HealthBarContainer");

        if (healthContainer == null)
        {
            Debug.LogError("[UIHandler] Could not find HealthBarContainer!");
            return;
        }

        // Collect the health squares dynamically
        foreach (var child in healthContainer.Children())
        {
            healthSquares.Add(child);
            Debug.Log($"[UIHandler] Found health square: {child.name}");
        }
    }

    private void UpdateHealthUI(float currentHealth, float maxHealth)
    {
        // Show squares based on remaining health
        for (int i = 0; i < healthSquares.Count; i++)
        {
            healthSquares[i].style.display =
                (i < currentHealth) ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
