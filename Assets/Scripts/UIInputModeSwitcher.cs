using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UIInputModeSwitcher : MonoBehaviour
{
    private PlayerInput playerInput;
    private bool usingMouse = true;
    private Vector2 lastMousePos;

    [Header("UI Focus Settings")]
    [Tooltip("Button to select when switching back to controller input")]
    public GameObject firstSelectedButton;

    void Awake()
    {
        playerInput = FindFirstObjectByType<PlayerInput>();
        if (Mouse.current != null)
            lastMousePos = Mouse.current.position.ReadValue();
    }

    void Update()
    {
        if (playerInput == null || EventSystem.current == null)
            return;

        bool mouseMoved = false;
        bool gamepadUsed = false;

        // Detect mouse movement
        if (Mouse.current != null)
        {
            Vector2 currentPos = Mouse.current.position.ReadValue();
            if ((currentPos - lastMousePos).sqrMagnitude > 0.1f)
                mouseMoved = true;
            lastMousePos = currentPos;
        }

        // Detect any stick or D-pad movement
        if (Gamepad.current != null)
        {
            Vector2 leftStick = Gamepad.current.leftStick.ReadValue();
            Vector2 dpad = Gamepad.current.dpad.ReadValue();
            if (leftStick.sqrMagnitude > 0.2f || dpad.sqrMagnitude > 0.2f)
                gamepadUsed = true;
        }

        // Switch to mouse mode
        if (mouseMoved && !usingMouse)
        {
            usingMouse = true;
            EventSystem.current.SetSelectedGameObject(null);
        }

        // Switch to gamepad mode
        else if (gamepadUsed && usingMouse)
        {
            usingMouse = false;

            // Re-focus the first button so navigation works again
            if (firstSelectedButton != null)
                EventSystem.current.SetSelectedGameObject(firstSelectedButton);
            else if (EventSystem.current.firstSelectedGameObject != null)
                EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);
        }
    }
}
