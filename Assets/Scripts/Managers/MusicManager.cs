using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }
    private AudioSource source;

    void Awake()
    {
        // Make sure only one MusicManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        source = GetComponent<AudioSource>();
        source.loop = true;
        source.playOnAwake = false; // don't auto-play on awake
        source.spatialBlend = 0f;   // 2D sound
        source.volume = 1f;
    }

    void Start()
    {
        // Play the clip set in the AudioSource automatically when the game starts
        if (source != null && source.clip && !source.isPlaying)
            source.Play();
    }

    public void SetTrack(AudioClip clip, bool restart = true)
    {
        if (!clip) return;
        if (!restart && source.clip == clip && source.isPlaying) return;

        source.clip = clip;
        source.Play();
    }
}
