using UnityEngine;
using UnityEngine.Playables;
using System.Collections;

public class BossDamageable : EnemyDamageable
{
    [Header("Boss Cutscene Settings")]
    public PlayableDirector defeatTimeline;     // assign your defeat timeline
    public MonoBehaviour bossAI;               // assign Michael's AI script
    public Michael boss;                       // reference to Michael.cs

    private bool cutsceneFinished = false;

    [Header("Death Audio")]
    public AudioClip deathFX;

    protected override IEnumerator DeathSequence()
    {
        Debug.Log("[BossDamageable] Boss reached 0 HP — defeated sequence starting.");

        // Enter defeated animation
        rb.simulated = false;
        Targetable = false;  // disables collisions/hits
        if (bossAI != null) bossAI.enabled = false;

        CutsceneDialogueController dialogue = FindFirstObjectByType<CutsceneDialogueController>();
        if (dialogue != null)
        {
            dialogue.director = defeatTimeline;
        }

        // Set cutscene as active
        CutsceneDialogueController.SetCutsceneActive(true);

        yield return new WaitForSeconds(2.5f); // short settle time


        // Play Timeline cutscene
        if (defeatTimeline != null)
        {
            Debug.Log("[BossDamageable] Playing defeat timeline.");
            defeatTimeline.Play();

        }
        else
        {
            Debug.LogWarning("[BossDamageable] No defeatTimeline assigned!");
            yield return new WaitForSeconds(2f);
        }


        // Wait for timeline signal 
        while (!cutsceneFinished)
            yield return null;

        Debug.Log("[BossDamageable] Cutscene fully finished.");
    }

    // Called by Timeline signal at the end
    public void NotifyCutsceneFinished()
    {
        cutsceneFinished = true;
    }

    // Called in the timeline
    public void playDeathAnimation()
    {
        // Trigger death animation
        animator.SetTrigger("deathTrigger");
        Debug.Log("[BossDamageable] Playing death animation.");

        if (audioSource != null && deathFX != null && audioSource.enabled)
        {
            audioSource.volume = 0.50f;
            audioSource.PlayOneShot(deathFX);
        }
        else
        {
            Debug.LogWarning($"[{name}] Missing or disabled AudioSource or deathFX");
        }
    }

    // Called at the end of the death animation
    public void DestroyObject()
    {
        // Destroy boss
        Debug.Log("[BossDamageable] Boss object destroyed.");
        Destroy(gameObject);
    }


}
