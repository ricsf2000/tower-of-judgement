using UnityEngine;
using UnityEngine.Playables; // Needed for Timeline
using System.Collections;
using System.Collections.Generic;

public class TimelineTrigger : MonoBehaviour
{
    public PlayableDirector timeline;

    private bool hasPlayed = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasPlayed)
        {
            timeline.Play();
            hasPlayed = true;
        }
    }
}
