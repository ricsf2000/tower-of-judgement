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
    private bool lineFullyRevealed = false;

    [Header("Scene Transition")]
    [SerializeField] private string nextSceneName;
    private Coroutine dialogueRoutine;

    [Header("Input")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private TMP_SpriteAsset xboxIcons;
    [SerializeField] private TMP_SpriteAsset keyboardIcons;

    private string lastScheme;

    [Header("Dialogue Options")]
    public bool allowSkipCutscene = true;   // Enabled for opening cutscenes
    public bool requireAdvanceInput = false; // Dialogue boxes: true. Opening cutscene: false.


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

    private void OnEnable()
    {
        TypewriterEffect.CompleteTextRevealed += OnLineFinished;
    }

    private void OnDisable()
    {
        TypewriterEffect.CompleteTextRevealed -= OnLineFinished;
    }

    private void OnLineFinished()
    {
        lineFullyRevealed = true;
    }

    public void PlayDialogue()
    {
        IsCutsceneActive = true;
        if (dialogueRoutine != null)
            StopCoroutine(dialogueRoutine);
        dialogueRoutine = StartCoroutine(PlayDialogueRoutine());
    }

    private IEnumerator PlayDialogueRoutine()
    {
        for (int i = 0; i < lines.Length; i++)
        {
            if (skipping) break;

            dialogueText.alpha = 0;
            dialogueText.maxVisibleCharacters = 0;

            dialogueText.text = lines[i];
            yield return null;
            dialogueText.alpha = 1;

            lineFullyRevealed = false;

            bool done = false;
            System.Action handler = () => done = true;
            TypewriterEffect.CompleteTextRevealed += handler;
            yield return new WaitUntil(() => done);
            TypewriterEffect.CompleteTextRevealed -= handler;

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
        dialogueText.alpha = 0;
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.text = lines[i];

        yield return null;
        dialogueText.alpha = 1;

        lineFullyRevealed = false;

        bool done = false;
        System.Action handler = () => done = true;
        TypewriterEffect.CompleteTextRevealed += handler;

        yield return new WaitUntil(() => done);
        TypewriterEffect.CompleteTextRevealed -= handler;
    }

    private void Update()
    {
        if (!playerInput) return;
        if (!IsCutsceneActive) return;

        string currentScheme = playerInput.currentControlScheme;
        if (currentScheme != lastScheme)
        {
            lastScheme = currentScheme;
            UpdateSkipPrompt(currentScheme);
        }

        bool advancePressed =
            requireAdvanceInput &&
            (
                (Keyboard.current?.spaceKey.wasPressedThisFrame ?? false) ||
                (Gamepad.current?.buttonSouth.wasPressedThisFrame ?? false)
            );

        bool skipPressed =
            allowSkipCutscene &&
            (
                (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
                (Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame)
            );

        // If typing, reveal entire line
        if (!lineFullyRevealed && advancePressed && requireAdvanceInput)
        {
            typewriter.ForceComplete();
            return;
        }


        // Press again to advance timeline
        if (lineFullyRevealed && advancePressed && requireAdvanceInput)
        {
            lineFullyRevealed = false;
            AdvanceTimelineToNextSignal();
            return;
        }

        // Hide skip prompt entirely when skipping is turned off
        if (!allowSkipCutscene && skipPromptVisible)
        {
            skipPromptVisible = false;
            if (skipPromptText != null)
                skipPromptText.gameObject.SetActive(false);
        }

        // Skip cutscene
        if (skipPressed && allowSkipCutscene)
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

    // Advance the timeline when skipping
    private void AdvanceTimelineToNextSignal()
    {
        if (director == null) return;

        var markerTimes = new System.Collections.Generic.List<double>();

        foreach (var output in director.playableAsset.outputs)
        {
            if (output.sourceObject is UnityEngine.Timeline.SignalTrack track)
            {
                foreach (var m in track.GetMarkers())
                    markerTimes.Add(m.time);
            }
        }

        markerTimes.Sort();

        double current = director.time;

        foreach (double t in markerTimes)
        {
            if (t > current + 0.0001f)
            {
                // Jump slightly before the signal to guarantee it fires
                director.time = t - 0.01f;
                if (director.time < 0) director.time = 0;

                director.Evaluate();
                return;
            }
        }

        // No more signals, finish timeline
        director.time = director.duration;
        director.Evaluate();
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

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }
}
