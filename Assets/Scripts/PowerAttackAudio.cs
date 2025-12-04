using UnityEngine;

public class PowerAttackAudio : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip swordAttackClip;

    public void PlaySwordAttack()
    {
        if (audioSource != null && swordAttackClip != null)
        {
            audioSource.PlayOneShot(swordAttackClip);
        }
    }
}
