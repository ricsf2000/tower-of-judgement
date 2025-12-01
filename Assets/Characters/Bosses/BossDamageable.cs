using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using System.Collections;

public class BossDamageable : EnemyDamageable
{
    [Header("Boss Cutscene Settings")]
    public PlayableDirector defeatTimeline;     // assign defeat timeline
    public MonoBehaviour bossAI;               // assign Michael's AI script
    public Michael boss;                       // reference to Michael.cs

    private bool cutsceneFinished = false;
    private bool deathAnimationStarted = false;
    private bool deathAnimationPlayed = false; // Track if death animation has already played

    [Header("Death Audio")]
    public AudioClip deathFX;

    [Header("Boss Health Bar")]
    [Tooltip("Boss health bar to hide after defeat")]
    public BossHealthBar bossHealthBar;

    protected override IEnumerator DeathSequence()
    {
        Debug.Log("[BossDamageable] Boss reached 0 HP — defeated sequence starting.");

        // Hide boss health bar immediately to prevent it from reappearing during death animation
        if (bossHealthBar != null)
        {
            if (bossHealthBar.barRoot != null)
            {
                bossHealthBar.barRoot.SetActive(false);
            }
            bossHealthBar.enabled = false;
            Debug.Log("[BossDamageable] Boss health bar hidden and component disabled.");
        }
        else
        {
            // Try to find it if not assigned
            bossHealthBar = FindFirstObjectByType<BossHealthBar>();
            if (bossHealthBar != null)
            {
                if (bossHealthBar.barRoot != null)
                {
                    bossHealthBar.barRoot.SetActive(false);
                }
                bossHealthBar.enabled = false;
                Debug.Log("[BossDamageable] Boss health bar found and hidden.");
            }
        }

        // Stop boss music before death animation
        if (MusicManager.Instance != null)
        {
            AudioSource musicSource = MusicManager.Instance.GetComponent<AudioSource>();
            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.Stop();
                Debug.Log("[BossDamageable] Boss music stopped.");
            }
        }

        // Enter defeated animation
        rb.simulated = false;
        Targetable = false;  // disables collisions/hits
        if (bossAI != null) 
        {
            bossAI.enabled = false;
            Debug.Log("[BossDamageable] Boss AI disabled.");
        }
        
        // Disable animator from being reset or triggered again
        if (animator != null)
        {
            Debug.Log("[BossDamageable] Animator ready for death animation.");
        }

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

        // Wait 1 second after Michael dies, then wait for death animation to finish
        if (deathAnimationStarted && animator != null)
        {
            Debug.Log("[BossDamageable] Waiting 1 second after death...");
            yield return new WaitForSecondsRealtime(1f);
            
            // Wait for death animation to finish
            Debug.Log("[BossDamageable] Waiting for death animation (mic_death) to finish...");
            
            float timeout = 10f;
            float elapsed = 0f;
            bool animationFinished = false;
            bool foundDeathState = false;
            
            while (elapsed < timeout && !animationFinished)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                
                if (stateInfo.IsName("mic_death"))
                {
                    foundDeathState = true;
                    if (stateInfo.normalizedTime >= 1f)
                    {
                        animationFinished = true;
                        Debug.Log("[BossDamageable] Death animation (mic_death) finished (normalizedTime >= 1).");
                        break;
                    }
                }
                else if (foundDeathState)
                {
                    animationFinished = true;
                    Debug.Log("[BossDamageable] Death animation finished (exited mic_death state).");
                    break;
                }
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            if (!animationFinished)
            {
                Debug.LogWarning($"[BossDamageable] Death animation wait timed out after {elapsed}s, continuing anyway.");
            }
            
            // Disable animator to prevent any further animation playback
            if (animator != null)
            {
                animator.enabled = false;
                Debug.Log("[BossDamageable] Animator disabled after death animation finished.");
            }
            
            // Hide all sprites immediately after death animation finishes
            SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var sr in spriteRenderers)
            {
                if (sr != null)
                {
                    sr.enabled = false;
                }
            }
            
            // Disable the main "Sprite" GameObject if it exists
            Transform spriteTransform = transform.Find("Sprite");
            if (spriteTransform != null)
            {
                spriteTransform.gameObject.SetActive(false);
            }
            
            Debug.Log("[BossDamageable] All sprites hidden after death animation.");
            
            // Wait a few frames to ensure animator state is fully settled
            yield return null;
            yield return null;
            yield return null;
            
            // Double-check animator is disabled and sprites are hidden
            if (animator != null && animator.enabled)
            {
                animator.enabled = false;
                Debug.LogWarning("[BossDamageable] Animator was re-enabled! Disabled again.");
            }
            
            // Re-hide sprites in case they were re-enabled
            foreach (var sr in spriteRenderers)
            {
                if (sr != null && sr.enabled)
                {
                    sr.enabled = false;
                }
            }
            
            if (spriteTransform != null && spriteTransform.gameObject.activeInHierarchy)
            {
                spriteTransform.gameObject.SetActive(false);
            }
        }
        else if (deathAnimationStarted)
        {
            Debug.Log("[BossDamageable] Waiting 1 second after death (no animator)...");
            yield return new WaitForSecondsRealtime(1f);
            yield return new WaitForSecondsRealtime(2f);
            
            if (animator != null)
            {
                animator.enabled = false;
            }
            
            yield return null;
            yield return null;
        }
        else
        {
            Debug.LogWarning("[BossDamageable] Death animation not started, skipping wait.");
            if (animator != null)
            {
                animator.enabled = false;
            }
            
            yield return null;
            yield return null;
        }

        // Ensure animator is completely disabled and sprites are hidden before starting cutscene
        if (animator != null)
        {
            animator.enabled = false;
        }
        
        // Final sprite hide before cutscene - do this multiple times to ensure it sticks
        SpriteRenderer[] finalSprites = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in finalSprites)
        {
            if (sr != null)
            {
                sr.enabled = false;
            }
        }
        
        Transform finalSpriteTransform = transform.Find("Sprite");
        if (finalSpriteTransform != null)
        {
            finalSpriteTransform.gameObject.SetActive(false);
        }
        
        // Wait a frame
        yield return null;
        
        // Hide again after a frame
        foreach (var sr in finalSprites)
        {
            if (sr != null && sr.enabled)
            {
                sr.enabled = false;
            }
        }
        
        if (finalSpriteTransform != null && finalSpriteTransform.gameObject.activeInHierarchy)
        {
            finalSpriteTransform.gameObject.SetActive(false);
        }
        
        // Wait one more frame to ensure everything is settled
        yield return null;
        
        // Final sprite hide and animator disable BEFORE starting cutscene
        if (animator != null)
        {
            animator.enabled = false;
        }
        
        // Hide all sprites one final time before cutscene (reuse existing variables)
        foreach (var sr in finalSprites)
        {
            if (sr != null)
            {
                sr.enabled = false;
            }
        }
        
        if (finalSpriteTransform != null)
        {
            finalSpriteTransform.gameObject.SetActive(false);
        }
        
        // Wait a frame to ensure sprites are hidden
        yield return null;
        
        // Find BossDefeatCutscene and start the sequence
        BossDefeatCutscene defeatCutscene = FindFirstObjectByType<BossDefeatCutscene>();
        if (defeatCutscene != null)
        {
            Debug.Log("[BossDamageable] Found BossDefeatCutscene, starting defeat sequence...");
            
            // Hide sprites again right before calling StartBossDefeatCutscene
            foreach (var sr in finalSprites)
            {
                if (sr != null)
                {
                    sr.enabled = false;
                }
            }
            
            if (finalSpriteTransform != null)
            {
                finalSpriteTransform.gameObject.SetActive(false);
            }
            
            defeatCutscene.StartBossDefeatCutscene(gameObject);
            
            // Wait a moment for the cutscene to start, then destroy this object
            yield return new WaitForSecondsRealtime(0.2f);
            
            // Destroy boss object immediately to prevent sprite reappearance
            Debug.Log("[BossDamageable] Destroying boss object.");
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("[BossDamageable] BossDefeatCutscene not found! Please add BossDefeatCutscene component to CutsceneManager.");
        }
    }

    // Called by Timeline signal at the end
    public void NotifyCutsceneFinished()
    {
        Debug.Log("[BossDamageable] NotifyCutsceneFinished() called by timeline signal!");
        cutsceneFinished = true;
    }
    
    // Static method that can be called from any GameObject's Signal Receiver
    public static void NotifyCutsceneFinishedStatic()
    {
        Debug.Log("[BossDamageable] NotifyCutsceneFinishedStatic() called - searching for BossDamageable...");
        BossDamageable bossDamageable = FindFirstObjectByType<BossDamageable>();
        if (bossDamageable != null)
        {
            Debug.Log("[BossDamageable] Found BossDamageable, calling NotifyCutsceneFinished()");
            bossDamageable.NotifyCutsceneFinished();
        }
        else
        {
            Debug.LogError("[BossDamageable] BossDamageable not found! Boss may have been destroyed too early.");
        }
    }

    // Called in the timeline
    public void playDeathAnimation()
    {
        // Prevent death animation from playing multiple times
        if (deathAnimationPlayed)
        {
            Debug.Log("[BossDamageable] Death animation already played, skipping.");
            return;
        }
        
        // Trigger death animation
        if (animator != null)
        {
            animator.SetTrigger("deathTrigger");
            deathAnimationStarted = true;
            deathAnimationPlayed = true;
            Debug.Log("[BossDamageable] Playing death animation.");
        }

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
        Debug.Log("[BossDamageable] DestroyObject() called - boss will be destroyed when scene loads.");
    }
}
