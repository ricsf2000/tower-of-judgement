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

    public void PlayDialogue()
    {
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
        if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (!skipPromptVisible)
            {
                skipPromptVisible = true;
                if (skipPromptText != null)
                    skipPromptText.gameObject.SetActive(true);
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

    public void EndCutscene()
    {
        if (skipPromptText != null)
        skipPromptText.gameObject.SetActive(false);

        // Automatically load next scene when cutscene finishes or is skipped
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }
}
