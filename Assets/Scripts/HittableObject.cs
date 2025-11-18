using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class HittableObject : MonoBehaviour
{
    [Header("Effects")]
    public GameObject breakEffectPrefab;
    public float shakeIntensity = 0.05f;
    public float shakeDuration = 0.15f;

    [Header("Settings")]
    public float health = 1f;
    private bool isBroken = false;

    [Header("Audio")]
    private AudioSource audioSource;
    public AudioClip[] hitImpact;
    public AudioClip[] breakSFX;

    private Animator animator;
    private DamageFlash damageFlash;
    private Vector3 originalPosition;

    private void Start()
    {
        animator = GetComponent<Animator>();
        damageFlash = GetComponent<DamageFlash>();
        originalPosition = transform.localPosition;
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isBroken)
            return;

        IHitbox hitbox = collision.GetComponent<IHitbox>();
        if (hitbox != null && hitbox.CanBreakObjects)
        {
            health -= hitbox.Damage;

            if (health > 0)
            {
                // Flash + Shake only if still alive
                if (damageFlash != null)
                    damageFlash.CallDamageFlash();

                StopAllCoroutines(); // prevent stacking shakes
                StartCoroutine(ShakeObject());

                if (hitImpact != null)
                {
                    int randomIndex = Random.Range(0, hitImpact.Length);
                    AudioClip clip = hitImpact[randomIndex];
                    audioSource.PlayOneShot(clip, 0.35f);
                }


            }
            else
            {
                Break();
            }
        }
    }

    private IEnumerator ShakeObject()
    {
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            Vector2 offset = Random.insideUnitCircle * shakeIntensity;
            transform.localPosition = originalPosition + (Vector3)offset;
            yield return null;
        }

        transform.localPosition = originalPosition;
    }

    private void Break()
    {
        isBroken = true;

        if (breakSFX != null)
        {
            int randomIndex = Random.Range(0, breakSFX.Length);
            AudioClip clip = breakSFX[randomIndex];
            PlayBreakSound(clip);
        }


        if (animator != null)
            animator.SetBool("isBroken", isBroken);
        else
            RemoveObject();     // Remove if object doesn't have animations for breaking

        if (breakEffectPrefab != null)
            Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);
    }

    private void RemoveObject()
    {
        Destroy(gameObject);
    }

    private void PlayBreakSound(AudioClip clip)
    {
        GameObject temp = new GameObject("BreakSound");
        temp.transform.position = transform.position;

        AudioSource tempSource = temp.AddComponent<AudioSource>();
        tempSource.clip = clip;
        tempSource.volume = 0.50f;
        tempSource.spatialBlend = 0f;
        tempSource.rolloffMode = AudioRolloffMode.Linear; 
        tempSource.minDistance = 0.01f; 
        tempSource.maxDistance = 500f;
        tempSource.outputAudioMixerGroup = audioSource.outputAudioMixerGroup;
        tempSource.Play();

        Destroy(temp, clip.length);
    }
}
