using UnityEngine;
using System.Collections;

public class BossMusicTrigger : MonoBehaviour
{   
    [Header("Boss Music Sequence")]
    public AudioClip introClip;
    public AudioClip mainLoopClip;
    
    private Coroutine introCoroutine;
    private AudioSource introSource;
    
    public void PlayBossIntro()
    {
        if (introCoroutine != null)
            StopCoroutine(introCoroutine);
            
        introCoroutine = StartCoroutine(PlayIntroThenLoop());
    }
    
    private IEnumerator PlayIntroThenLoop()
    {
        if (introClip == null || mainLoopClip == null)
        {
            Debug.LogWarning("[BossMusicTrigger] Intro or Main Loop clip is not assigned!");
            yield break;
        }
        
        if (MusicManager.Instance == null)
        {
            Debug.LogWarning("[BossMusicTrigger] MusicManager instance not found!");
            yield break;
        }
            
        AudioSource musicSource = MusicManager.Instance.GetComponent<AudioSource>();
        if (musicSource == null)
        {
            Debug.LogWarning("[BossMusicTrigger] AudioSource not found on MusicManager!");
            yield break;
        }
        
        if (introSource == null)
        {
            introSource = gameObject.AddComponent<AudioSource>();
            introSource.playOnAwake = false;
            introSource.loop = false;
            introSource.spatialBlend = 0f;
        }
        
        musicSource.Stop();
        
        introSource.clip = introClip;
        introSource.volume = MusicManager.Instance.defaultVolume;
        double introStartTime = AudioSettings.dspTime;
        introSource.PlayScheduled(introStartTime);
        
        float introLength = introClip.length;
        musicSource.loop = true;
        musicSource.clip = mainLoopClip;
        musicSource.volume = MusicManager.Instance.defaultVolume;
        musicSource.PlayScheduled(introStartTime + introLength);
        
        yield return new WaitForSeconds(introLength + 0.1f);
        
        if (introSource != null)
        {
            introSource.Stop();
        }
        
        introCoroutine = null;
    }
}
