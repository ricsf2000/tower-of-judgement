using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Playables;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.InputSystem.Controls;
using System.Collections.Generic;

public class CutsceneDialogueController : MonoBehaviour
{
    [Header("References")]
    public TypewriterEffect typewriter;
    public TMP_Text dialogueText;
    public TMP_Text skipPromptText;
    public PlayableDirector director;

    [System.Serializable]
    public class DialogueSequence
    {
        public string sequenceName;
        [TextArea(2, 4)] public string[] lines;
        [Tooltip("Index of character from characters array (-1 = hide all)")]
        public int[] activeCharacterIndex;
    }
    
    [Header("Characters")]
    [Tooltip("Array of character GameObjects. Index 0, 1, 2, etc.")]
    public GameObject[] characters;
    
    [Header("Dialogue Sequences")]
    public DialogueSequence[] dialogueSequences;
    
    [Header("Current Dialogue (Runtime)")]
    [TextArea(2, 4)] public string[] lines;
    public int[] activeCharacterIndex;
    [Header("Timing")]
    public float delayBetweenLines = 2f;

    private bool skipping = false;
    private bool skipPromptVisible = false;
    private bool lineFullyRevealed = false;
    private int currentLineIndex = 0;
    private bool dialogueStarted = false;
    private bool isPostFight = false;

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
    private float advanceCooldown = 0f;
    

    private PlayerController player;

    [Header("Scripts to Disable During Cutscenes")]
    public List<MonoBehaviour> AIScripts = new List<MonoBehaviour>();

    public static bool IsCutsceneActive { get; private set; } = false;

    public static void SetCutsceneActive(bool value)
    {
        IsCutsceneActive = value;
    }

    public static bool CutsceneLocksActionMap { get; private set; } = false;

    public static void SetCutsceneLock(bool locked)
    {
        CutsceneLocksActionMap = locked;
    }

    private void Awake()
    {
        if (!playerInput)
            playerInput = FindAnyObjectByType<PlayerInput>();

        if (!player)
            player = FindFirstObjectByType<PlayerController>();

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

        ShowSpeaker(lineIndex);
        StopAllCoroutines();
        StartCoroutine(PlaySingleLineRoutine(lineIndex));
    }

    public void PlayNextDialogue()
    {
        if (currentLineIndex < lines.Length)
        {
            PlayDialogueLine(currentLineIndex);
            currentLineIndex++;
        }
        else
        {
            // Player pressed button after the last line - now end dialogue
            dialogueStarted = false; // Disable input handling
            
            // Hide all characters when dialogue ends
            HideAllCharacters();
            
            // Resume timeline when dialogue ends
            if (director != null)
            {
                Debug.Log($"[{gameObject.name}] Dialogue finished, resuming timeline");
                director.Resume();
            }
        }
    }

    public void StartDialogueSequence(int sequenceIndex)
    {
        if (sequenceIndex < 0 || sequenceIndex >= dialogueSequences.Length)
        {
            Debug.LogError($"Invalid dialogue sequence index: {sequenceIndex}");
            return;
        }

        var sequence = dialogueSequences[sequenceIndex];
        lines = sequence.lines;
        activeCharacterIndex = sequence.activeCharacterIndex;

        Debug.Log($"[{gameObject.name}] Starting dialogue sequence: {sequence.sequenceName}");

        // Reset typewriter state before starting new dialogue sequence
        if (typewriter != null)
            typewriter.ResetState();

        ResetDialogueIndex();
    }
    
    public void StartDialogueSequence(string sequenceName)
    {
        for (int i = 0; i < dialogueSequences.Length; i++)
        {
            if (dialogueSequences[i].sequenceName == sequenceName)
            {
                StartDialogueSequence(i);
                return;
            }
        }
        Debug.LogError($"Dialogue sequence not found: {sequenceName}");
    }

    public void ResetDialogueIndex()
    {
        currentLineIndex = 0;
        dialogueStarted = true; // Enable input handling
        
        // Pause the timeline during dialogue
        if (director != null)
        {
            director.Pause();
            Debug.Log($"[{gameObject.name}] Timeline paused for dialogue");
        }
        
        Debug.Log($"[{gameObject.name}] Dialogue started, dialogueStarted = {dialogueStarted}");
        if (lines.Length > 0)
        {
            PlayDialogueLine(0);
            currentLineIndex = 1; // Ready for next line
        }
    }

    private void HideAllCharacters()
    {
        if (characters == null || characters.Length == 0) return;
        
        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] != null)
                characters[i].SetActive(false);
        }
    }
    
    private void ShowSpeaker(int lineIndex)
    {
        if (characters == null || characters.Length == 0) return;
        
        // Hide all characters first
        HideAllCharacters();
        
        // Show the active character for this line
        if (lineIndex < activeCharacterIndex.Length)
        {
            int charIndex = activeCharacterIndex[lineIndex];
            if (charIndex >= 0 && charIndex < characters.Length && characters[charIndex] != null)
            {
                characters[charIndex].SetActive(true);
            }
            // If charIndex is -1, all characters stay hidden
        }
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
        if (!IsCutsceneActive) return;

        bool skipPressed =
            allowSkipCutscene &&
            (
                (Keyboard.current?.anyKey.wasPressedThisFrame ?? false) ||
                (Gamepad.current?.allControls.Any(c =>
                    c is ButtonControl b &&
                    b.wasPressedThisFrame &&
                    b != Gamepad.current.leftStickButton &&
                    b != Gamepad.current.rightStickButton &&
                    b != Gamepad.current.leftStick.up &&
                    b != Gamepad.current.leftStick.down &&
                    b != Gamepad.current.leftStick.left &&
                    b != Gamepad.current.leftStick.right &&
                    b != Gamepad.current.rightStick.up &&
                    b != Gamepad.current.rightStick.down &&
                    b != Gamepad.current.rightStick.left &&
                    b != Gamepad.current.rightStick.right
                ) ?? false)
            );


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
                if (skipPromptText != null)
                {
                    skipPromptText.gameObject.SetActive(true);
                    skipPromptText.text = "Press any button to skip";
                }
            }
            else
            {
                SkipCutscene();
            }
        }

        // Cooldown for skipping between signals in the timeline
        if (advanceCooldown > 0f)
        {
            advanceCooldown -= Time.deltaTime;
        }

    }

    public void OnAdvancePressed()
    {
        Debug.Log($"[{gameObject.name}] OnAdvancePressed called, dialogueStarted = {dialogueStarted}");
        
        // Only respond if dialogue has started
        if (!dialogueStarted) return;
        
        // If lines are typing, reveal entire line
        if (!lineFullyRevealed)
        {
            typewriter.ForceComplete();
            advanceCooldown = 0.2f;
            return;
        }

        // Press again to advance to next dialogue line
        if (requireAdvanceInput)
        {
            lineFullyRevealed = false;
            PlayNextDialogue();
            advanceCooldown = 0.2f;
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

        for (int i = 0; i < markerTimes.Count; i++)
        {
            double t = markerTimes[i];

            if (t > current + 0.0001f)
            {
                // If this next signal is the last one, dont skip
                if (i == markerTimes.Count - 1)
                {
                    return;
                }

                // Jump slightly before the signal to guarantee it fires
                director.time = t - 0.07f;
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

        skipPromptText.text = "Press any button to skip";
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
        ResetCutsceneState();

        IsCutsceneActive = false;

        if (skipPromptText != null)
            skipPromptText.gameObject.SetActive(false);

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }

    private void ResetCutsceneState()
    {
        // Reset reveal and skip state
        lineFullyRevealed = false;
        skipping = false;
        skipPromptVisible = false;

        if (skipPromptText != null)
            skipPromptText.gameObject.SetActive(false);

        // Fully reset dialogue text
        if (dialogueText != null)
        {
            dialogueText.text = "";
            dialogueText.alpha = 0;
            dialogueText.maxVisibleCharacters = 0;
            dialogueText.ForceMeshUpdate(true);
        }

        if (typewriter != null)
        {
            typewriter.ResetState();
        }
    }

    // Functions that the timeline will call in signals at the beginning and end of timeline

    public void FreezeCutscene()
    {
        Debug.Log("[UCM] FreezeCutscene called.");

        SetCutsceneActive(true);
        SetCutsceneLock(true);

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

        SetCutsceneActive(false);
        SetCutsceneLock(false);
    }


}
