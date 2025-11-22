using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.InputSystem;
using TMPro;

public class CutsceneSkip : MonoBehaviour
{
    public PlayableDirector director;
    public CutsceneDialogueController dialogueController;
    public TMP_Text skipPrompt;        // assign your "Press Space to skip" TMP object

    private bool promptShown = false;
    public Key skipKey = Key.Space; // or Key.Escape

    void Start()
    {
        if (skipPrompt != null)
            skipPrompt.gameObject.SetActive(false);   // hide at start
    }

    void Update()
    {
        if (Keyboard.current[skipKey].wasPressedThisFrame)
        {
            if (!promptShown)
            {
                ShowPrompt();
            }
            else
            {
                SkipCutscene();
            }
        }
    }

    void ShowPrompt()
    {
        promptShown = true;
        if (skipPrompt != null)
            skipPrompt.gameObject.SetActive(true);
    }

    void SkipCutscene()
    {
        // Jump timeline forward
        if (director != null)
        {
            director.time = director.duration;
            director.Evaluate();
            director.Stop();
        }

        // Transition scenes / end cutscene
        if (dialogueController != null)
            dialogueController.EndCutscene();
    }
}
