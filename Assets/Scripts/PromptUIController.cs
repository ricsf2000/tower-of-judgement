using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class PromptUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text actionText;
    [SerializeField] private TMP_SpriteAsset xboxIcons;
    [SerializeField] private TMP_SpriteAsset playstationIcons;
    [SerializeField] private TMP_SpriteAsset keyboardIcons;
    [SerializeField] private TMP_SpriteAsset WASD;
    [SerializeField] private TMP_SpriteAsset mouseIcons;

    [Header("Prompt Settings")]
    [SerializeField] private bool showFullPrompt = true;
    [SerializeField] private string actionName = "to Dash";
    [SerializeField] private string currentActionKey = "Dash";
    private bool useTextOnly = false;
    private string customText = "";

    private PlayerInput playerInput;
    private string lastScheme;

    private void Awake()
    {
        playerInput = FindAnyObjectByType<PlayerInput>();
        if (playerInput)
        {
            Debug.Log($"[PromptUIController] Found PlayerInput: {playerInput != null}");
            UpdatePrompt(playerInput.currentControlScheme);
            playerInput.onControlsChanged += OnControlsChanged;
        }
    }

    private void FixedUpdate()
    {
        if (!playerInput) return;

        string current = playerInput.currentControlScheme;
        if (current != lastScheme)
        {
            lastScheme = current;
            Debug.Log($"[PromptUIController] Scheme switched → {current}");
            UpdatePrompt(current);
        }
    }

    private void OnDestroy()
    {
        if (playerInput != null)
            playerInput.onControlsChanged -= OnControlsChanged;
    }

    public void SetAction(string newActionKey, string newActionName, bool fullPrompt, bool textOnly, string custom)
    {
        currentActionKey = newActionKey;
        actionName = newActionName;
        showFullPrompt = fullPrompt;    // allows triggers to choose mode
        useTextOnly = textOnly;
        customText = custom;
        UpdatePrompt(playerInput != null ? playerInput.currentControlScheme : "Keyboard&Mouse");
    }

    public void OnControlsChanged(PlayerInput input)
    {
        Debug.Log($"[PromptUIController] Control scheme changed → {input.currentControlScheme}");
        UpdatePrompt(input.currentControlScheme);
    }

    private void UpdatePrompt(string controlScheme)
    {
        string iconTag = "";

        switch (controlScheme)
        {
            case "Gamepad":
                {
                    var type = ControllerTypeDetector.Detect();

                    switch (type)
                    {
                        case ControllerTypeDetector.ControllerType.Xbox:
                            actionText.spriteAsset = xboxIcons;
                            if (currentActionKey == "Dash") iconTag = "<sprite name=\"xbox_A\">";
                            else if (currentActionKey == "Attack") iconTag = "<sprite name=\"xbox_X\">";
                            else if (currentActionKey == "Move") iconTag = "<sprite name=\"xbox_LS\">";
                            else iconTag = "<sprite name=\"xbox_A\">";
                            break;

                        case ControllerTypeDetector.ControllerType.PlayStation:
                            actionText.spriteAsset = playstationIcons;
                            if (currentActionKey == "Dash") iconTag = "<sprite name=\"ps_Cross\">";
                            else if (currentActionKey == "Attack") iconTag = "<sprite name=\"ps_Square\">";
                            else if (currentActionKey == "Move") iconTag = "<sprite name=\"ps_LS\">";
                            else iconTag = "<sprite name=\"ps_Cross\">";
                            break;

                        default:
                            // fallback
                            actionText.spriteAsset = xboxIcons;
                            iconTag = "<sprite name=\"xbox_A\">";
                            break;
                    }

                    break;
                }

            case "Keyboard&Mouse":
                switch (currentActionKey)
                {
                    case "Move":
                        actionText.spriteAsset = WASD;
                        iconTag = "<sprite name=\"KBM_WASD\">";
                        break;
                    case "Dash":
                        actionText.spriteAsset = keyboardIcons;
                        iconTag = "<sprite name=\"KBM_Space\">";
                        break;
                    case "Attack":
                        actionText.spriteAsset = mouseIcons;
                        iconTag = "<sprite name=\"MOUSE_Left\">";
                        break;
                    default:
                        actionText.spriteAsset = keyboardIcons;
                        iconTag = "<sprite name=\"KBM_Space\">";
                        break;
                }
                break;

            default:
                actionText.spriteAsset = keyboardIcons;
                iconTag = "<sprite name=\"KBM_Space\">";
                break;
        }

        // Display logic toggle

        // Text only overrides everything else
        if (useTextOnly)
        {
            actionText.text = customText;
            return;
        }

        if (showFullPrompt)
            actionText.text = $"Use {iconTag} {actionName}";
        else
            actionText.text = iconTag; // only show the button

        actionText.ForceMeshUpdate();
    }
}
