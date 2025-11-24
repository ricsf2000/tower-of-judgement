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

    [Tooltip("Enable to have a text only prompt")]
    [SerializeField] private bool useTextOnly = false;

    [Tooltip("Custom text to display when TextOnly is enabled.")]
    [SerializeField] private string customPromptText = "";

    [Header("Follow Target Settings")]
    [SerializeField] private bool followPlayer = true;

    public bool showOnce = true;
    public bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // If this prompt is show-once AND has been used, skip
            if (showOnce && triggered)
                return;

            if (promptUI == null)
            {
                Debug.LogWarning("[PromptTrigger] No PromptUI assigned!");
                return;
            }

            var controller = promptUI.GetComponent<PromptUIController>();
            if (controller != null)
            {
                controller.SetAction(
                    actionKey,
                    actionName,
                    showFullPrompt,
                    useTextOnly,
                    customPromptText
                );

                if (followPlayer)
                    promptUI.GetComponent<FollowTargetUI>()?.SetTarget(other.transform);

                promptUI.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (showOnce)
                triggered = true;

            if (promptUI != null)
                promptUI.SetActive(false);
        }
    }
}
