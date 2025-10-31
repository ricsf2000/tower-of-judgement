using UnityEngine;

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
}
