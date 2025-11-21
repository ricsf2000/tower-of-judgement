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
        // Whenever a new scene loads, fade the music back in
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
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
