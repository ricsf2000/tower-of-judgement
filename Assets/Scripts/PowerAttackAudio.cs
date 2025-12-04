using UnityEngine;

public class PowerAttackAudio : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip swordAttackClip;
    [SerializeField] private float volume = 0.5f;

    public void PlaySwordAttack()
    {
        if (audioSource != null && swordAttackClip != null)
        {
            audioSource.PlayOneShot(swordAttackClip, volume);
        }
    }
}
