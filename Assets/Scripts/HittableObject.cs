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

    private Animator animator;
    private DamageFlash damageFlash;
    private Vector3 originalPosition;

    private void Start()
    {
        animator = GetComponent<Animator>();
        damageFlash = GetComponent<DamageFlash>();
        originalPosition = transform.localPosition;
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
}
