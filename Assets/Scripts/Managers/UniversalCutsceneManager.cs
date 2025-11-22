using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class UniversalCutsceneManager : MonoBehaviour
{
    public static UniversalCutsceneManager Instance;

    private PlayerController player;
    private PlayerInput playerInput;

    [Header("Scripts to Disable During Cutscenes")]
    public List<MonoBehaviour> AIScripts = new List<MonoBehaviour>();


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        player = FindFirstObjectByType<PlayerController>();
        playerInput = FindFirstObjectByType<PlayerInput>();
    }

    // Functions that the timeline will call in signals

    public void FreezeCutscene()
    {


        Debug.Log("[UCM] FreezeCutscene called.");

        // Freeze player
        if (player != null)
        {
            player.Rb.linearVelocity = Vector2.zero;
            player.Animator.SetBool("isMoving", false);
        }

        playerInput.SwitchCurrentActionMap("UI");

        // Disable all AI scripts
        foreach (var ai in AIScripts)
        {
            if (ai != null)
                ai.enabled = false;
        }
    }

    public void UnfreezeCutscene()
    {

        Debug.Log("[UCM] UnfreezeCutscene called.");

        playerInput.SwitchCurrentActionMap("Player");

        // Enable all AI scripts
        foreach (var ai in AIScripts)
        {
            if (ai != null)
                ai.enabled = true;
        }
    }
}
