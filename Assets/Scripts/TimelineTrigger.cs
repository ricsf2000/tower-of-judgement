using UnityEngine;
using UnityEngine.Playables; // Needed for Timeline
using System.Collections;

public class TimelineTrigger : MonoBehaviour
{
    [Header("References")]
    public PlayableDirector timeline;
    public MonoBehaviour bossAI;

    private bool hasPlayed = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasPlayed) return;

        if (other.CompareTag("Player"))
        {
            hasPlayed = true;
            StartCoroutine(PlayCutsceneSequence());
        }
    }

    private IEnumerator PlayCutsceneSequence()
    {
        // Freeze player and boss movement
        CutsceneDialogueController.SetCutsceneActive(true);

        // Freeze boss AI
        if (bossAI != null)
            bossAI.enabled = false;

        // Play the Timeline
        if (timeline != null)
            timeline.Play();

        // Wait until the Timeline finishes
        yield return new WaitUntil(() =>
            timeline.time >= timeline.duration ||
            timeline.state != PlayState.Playing
        );

        // Unfreeze after the Timeline finishes
        CutsceneDialogueController.SetCutsceneActive(false);

        if (bossAI != null)
            bossAI.enabled = true;

        Debug.Log("[TimelineTrigger] Cutscene finished. Player and boss re-enabled.");
    }
}
