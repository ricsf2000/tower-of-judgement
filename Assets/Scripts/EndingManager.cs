using UnityEngine;
using UnityEngine.Playables;

public class EndingManager : MonoBehaviour
{
    [Header("References to Ending Timelines")]
    public PlayableDirector returnEndingTimeline;
    public PlayableDirector stayEndingTimeline;

    void Start()
    {
        // Fetch what choice the player made
        var choice = CutsceneDialogueController.PlayerChoice;

        Debug.Log($"[EndingManager] PlayerChoice = {choice}");

        // Play the appropriate ending timeline
        switch (choice)
        {
            case CutsceneDialogueController.LuciferChoice.ReturnToHeaven:
                PlayTimeline(returnEndingTimeline, stayEndingTimeline);
                break;

            case CutsceneDialogueController.LuciferChoice.StayWithHumanity:
                PlayTimeline(stayEndingTimeline, returnEndingTimeline);
                break;

            default:
                Debug.LogWarning("[EndingManager] No choice detected. Defaulting to 'Return' ending.");
                PlayTimeline(returnEndingTimeline, stayEndingTimeline);
                break;
        }
    }

    private void PlayTimeline(PlayableDirector toPlay, PlayableDirector toDisable)
    {
        if (toDisable != null)
            toDisable.gameObject.SetActive(false);

        if (toPlay != null)
        {
            toPlay.gameObject.SetActive(true);
            toPlay.time = 0;
            toPlay.Play();
        }
    }
}
