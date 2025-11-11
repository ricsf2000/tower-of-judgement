using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PromptTrigger : MonoBehaviour
{
    [Header("Prompt Settings")]
    [Tooltip("Reference to the shared Prompt UI in your Canvas.")]
    [SerializeField] private GameObject promptUI;

    [Tooltip("The type of action this prompt should show (e.g. Dash, Attack, Interact).")]
    [SerializeField] private string actionKey = "Dash";

    [Tooltip("Text displayed after the button icon, e.g. 'to Dash'. Leave blank for icon-only mode.")]
    [SerializeField] private string actionName = "to Dash";

    [Tooltip("If disabled, only the button icon will show (no text).")]
    [SerializeField] private bool showFullPrompt = true;

    [Header("Follow Target Settings")]
    [SerializeField] private bool followPlayer = true;

    public bool showOnce = true;
    public bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggered && showOnce)
        {
            if (other.CompareTag("Player"))
            {
                if (promptUI == null)
                {
                    Debug.LogWarning("[PromptTrigger] No PromptUI assigned!");
                    return;
                }

                var controller = promptUI.GetComponent<PromptUIController>();
                if (controller != null)
                {
                    // Set the prompt’s action and display mode
                    controller.SetAction(actionKey, actionName, showFullPrompt);

                    // Assign the player as the follow target if applicable
                    if (followPlayer)
                        promptUI.GetComponent<FollowTargetUI>()?.SetTarget(other.transform);

                    // Enable the prompt
                    promptUI.SetActive(true);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (showOnce)
            triggered = true;

        if (other.CompareTag("Player"))
        {
            if (promptUI != null)
                promptUI.SetActive(false);
        }
    }
}
