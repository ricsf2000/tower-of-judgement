using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerSFX : MonoBehaviour
{
    private AudioSource audioSource;

    [Header("Audio Clips")]
    public AudioClip[] swordSwing;
    public AudioClip dashFX;
    public AudioClip deathFX;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySwordSwing()
    {
        if (swordSwing == null || swordSwing.Length == 0) return;

        int randomIndex = Random.Range(0, swordSwing.Length);
        AudioClip clip = swordSwing[randomIndex];

        if (audioSource.isPlaying)
            audioSource.Stop();

        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.volume = 0.15f;
        audioSource.PlayOneShot(clip);
    }

    public void PlayDashFX()
    {
        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.volume = 0.25f;
        audioSource.PlayOneShot(dashFX);
    }

    public void PlayDeathFX()
    {
        audioSource.volume = 0.50f;
        audioSource.PlayOneShot(deathFX);
    }
}
