using UnityEngine;
using UnityEngine.Playables;

public class TimelineTrigger : MonoBehaviour
{
    public PlayableDirector timeline;
    private bool hasPlayed = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (hasPlayed) return;

        hasPlayed = true;

        if (timeline != null)
        {
            Debug.Log("[TimelineTrigger] Playing Timeline.");
            timeline.Play();
        }
    }
}
