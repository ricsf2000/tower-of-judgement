using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Range(0f, 1f)]
    public float defaultVolume = 0.3f; // lower by default

    private AudioSource source;
    private AudioSource introSource;
    private AudioClip currentLoopClip;

    void Awake()
    {
        // Ensure only one MusicManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        source = GetComponent<AudioSource>();
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0f; // 2D
        source.volume = defaultVolume; // apply volume immediately
    }

    void Start()
    {
        if (source.clip && !source.isPlaying)
            source.Play();
    }

    public void SetTrack(AudioClip clip, bool restart = true)
    {
        if (!clip) return;
        if (!restart && source.clip == clip && source.isPlaying) return;

        source.clip = clip;
        source.volume = defaultVolume;
        source.Play();
        currentLoopClip = clip;
        
        Debug.Log($"[MusicManager] SetTrack: {clip.name}, Length: {clip.length} seconds ({clip.length / 60f:F2} minutes), LoadState: {clip.loadState}");
    }
    
    public bool IsPlayingLoopClip(AudioClip loopClip)
    {
        return source != null && source.clip == loopClip && source.isPlaying;
    }
    
    public void ContinuePlaying()
    {
        if (source != null && source.clip != null && !source.isPlaying)
        {
            source.Play();
        }
    }
    
    public void PlayIntroThenLoop(AudioClip introClip, AudioClip loopClip)
    {
        if (introClip == null || loopClip == null) return;
        
        StartCoroutine(PlayIntroThenLoopCoroutine(introClip, loopClip));
    }
    
    private IEnumerator PlayIntroThenLoopCoroutine(AudioClip introClip, AudioClip loopClip)
    {
        if (introSource == null)
        {
            introSource = gameObject.AddComponent<AudioSource>();
            introSource.playOnAwake = false;
            introSource.loop = false;
            introSource.spatialBlend = 0f;
        }
        
        source.Stop();
        
        introSource.clip = introClip;
        introSource.volume = defaultVolume;
        double introStartTime = AudioSettings.dspTime;
        introSource.PlayScheduled(introStartTime);
        
        float introLength = introClip.length;
        source.loop = true;
        source.clip = loopClip;
        source.volume = defaultVolume;
        source.PlayScheduled(introStartTime + introLength);
        currentLoopClip = loopClip;
        
        yield return new WaitForSeconds(introLength + 0.1f);
        
        if (introSource != null)
        {
            introSource.Stop();
        }
    }

    public void StopMusic(bool clearClip = true)
    {
        if (source != null)
        {
            source.Stop();
            source.volume = defaultVolume;
            if (clearClip)
                source.clip = null;
            currentLoopClip = null;
        }

        if (introSource != null)
        {
            introSource.Stop();
            if (clearClip)
                introSource.clip = null;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded; // Listen to scene loads
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // When the player enters a code and enters a new scene, fade the music back in
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Only reset if there's no clip (let SceneMusicTag handle track changes)
        if (source.clip == null)
        {
            StartCoroutine(FadeIn());
        }
    }

    private IEnumerator FadeIn()
    {   
        yield return null;

        if (source.clip != null)
        source.Play();

        float duration = 0.25f;
        float t = 0f;

        float start = source.volume;
        float end = defaultVolume;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(start, end, t / duration);
            yield return null;
        }

        source.volume = end;
    }

}
