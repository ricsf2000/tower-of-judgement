using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Playables;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CutsceneDialogueController : MonoBehaviour
{
    [Header("References")]
    public TypewriterEffect typewriter;
    public TMP_Text dialogueText;
    public TMP_Text skipPromptText;
    public PlayableDirector director;

    [Header("Dialogue Lines")]
    [TextArea(2, 4)] public string[] lines;
    [Header("Timing")]
    public float delayBetweenLines = 2f;

    private bool skipping = false;
    private bool skipPromptVisible = false;

    [Header("Scene Transition")]
    [SerializeField] private string nextSceneName;
    private Coroutine dialogueRoutine;

    [Header("Input")]
    [SerializeField] private PlayerInput playerInput; // assign your persistent InputManager here
    [SerializeField] private TMP_SpriteAsset xboxIcons;
    [SerializeField] private TMP_SpriteAsset keyboardIcons;

    private string lastScheme;

    public static bool IsCutsceneActive { get; private set; } = false;

    public static void SetCutsceneActive(bool value)
    {
        IsCutsceneActive = value;
    }

    private void Awake()
    {
        if (!playerInput)
            playerInput = FindAnyObjectByType<PlayerInput>();

        if (playerInput)
        {
            lastScheme = playerInput.currentControlScheme;
            UpdateSkipPrompt(lastScheme);
            playerInput.onControlsChanged += OnControlsChanged;
        }
        else
        {
            Debug.LogWarning("[CutsceneDialogueController] No PlayerInput found in scene!");
        }

        if (skipPromptText != null)
            skipPromptText.gameObject.SetActive(false);
    }

    public void PlayDialogue()
    {
        IsCutsceneActive = true;
        Debug.Log("[CutsceneDialogueController] PlayDialogue triggered by Timeline");
        if (dialogueRoutine != null)
            StopCoroutine(dialogueRoutine);
        dialogueRoutine = StartCoroutine(PlayDialogueRoutine());
    }

    private IEnumerator PlayDialogueRoutine()
    {
        for (int i = 0; i < lines.Length; i++)
        {
            if (skipping) break;

            // Prevent the one-frame flash
            dialogueText.alpha = 0;
            dialogueText.maxVisibleCharacters = 0;

            // Assign text (this will trigger TypewriterEffect)
            dialogueText.text = lines[i];

            // Wait a single frame so TMP & TypewriterEffect prepare
            yield return null;

            // Reveal text now that TypewriterEffect has reset
            dialogueText.alpha = 1;

            // Wait for typing to finish
            bool done = false;
            System.Action handler = () => done = true;
            TypewriterEffect.CompleteTextRevealed += handler;
            yield return new WaitUntil(() => done);
            TypewriterEffect.CompleteTextRevealed -= handler;

            // Short delay before next line
            float t = 0f;
            while (t < delayBetweenLines && !skipping)
            {
                t += Time.deltaTime;
                yield return null;
            }
        }

        EndCutscene();
    }

    public void PlayDialogueLine(int lineIndex)
    {
        if (lineIndex < 0 || lineIndex >= lines.Length)
            return;

        StopAllCoroutines();
        StartCoroutine(PlaySingleLineRoutine(lineIndex));
    }

    private IEnumerator PlaySingleLineRoutine(int i)
    {
        // Hide text first to prevent flash
        dialogueText.alpha = 0;
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.text = lines[i];

        yield return null; // let TMP rebuild
        dialogueText.alpha = 1;

        bool done = false;
        System.Action handler = () => done = true;
        TypewriterEffect.CompleteTextRevealed += handler;

        yield return new WaitUntil(() => done);
        TypewriterEffect.CompleteTextRevealed -= handler;
    }

    private void Update()
    {
        if (!playerInput) return;

        if (!IsCutsceneActive)
            return;

        string currentScheme = playerInput.currentControlScheme;
        if (currentScheme != lastScheme)
        {
            lastScheme = currentScheme;
            UpdateSkipPrompt(currentScheme);
        }
    
        bool skipPressed =
            (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)) ||
            (Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame); // Y on Xbox / Triangle on PlayStation

        if (skipPressed)
        {
            if (!skipPromptVisible)
            {
                skipPromptVisible = true;
                skipPromptText.gameObject.SetActive(true);
                UpdateSkipPrompt(playerInput ? playerInput.currentControlScheme : "Keyboard&Mouse");
            }
            else
            {
                SkipCutscene();
            }
        }

    }

    private void SkipCutscene()
    {
        if (skipping) return;
        skipping = true;

        dialogueText.maxVisibleCharacters = dialogueText.text.Length;
        if (skipPromptText != null)
            skipPromptText.gameObject.SetActive(false);

        if (director != null)
        {
            director.time = director.duration;
            director.Evaluate();
            director.Stop();
        }

        StopAllCoroutines();
        EndCutscene();
    }

    private void UpdateSkipPrompt(string controlScheme)
    {
        if (skipPromptText == null)
            return;

        string iconTag = "";

        switch (controlScheme)
        {
            case "Gamepad":
                skipPromptText.spriteAsset = xboxIcons;
                iconTag = "<sprite name=\"xbox_Y\">";
                skipPromptText.text = $"Press {iconTag} to skip";
                break;

            case "Keyboard&Mouse":
            default:
                skipPromptText.spriteAsset = keyboardIcons;
                iconTag = "<sprite name=\"KBM_Space\">";
                skipPromptText.text = $"Press {iconTag} to skip";
                break;
        }

        skipPromptText.ForceMeshUpdate();
    }

    private void OnControlsChanged(PlayerInput input)
    {
        if (input.currentControlScheme != lastScheme)
        {
            lastScheme = input.currentControlScheme;
            UpdateSkipPrompt(lastScheme);
        }
    }

    private void OnDestroy()
    {
        if (playerInput)
            playerInput.onControlsChanged -= OnControlsChanged;
    }

    public void EndCutscene()
    {
        IsCutsceneActive = false;

        if (skipPromptText != null)
        skipPromptText.gameObject.SetActive(false);

        // Automatically load next scene when cutscene finishes or is skipped
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }
}
