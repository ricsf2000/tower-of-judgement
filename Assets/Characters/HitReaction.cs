using UnityEngine;
using System.Collections;

public class HitReaction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer[] renderers;

    [Header("Hit Settings")]
    [SerializeField] private float freezeDuration = 0.15f;
    [SerializeField] private bool freezeAnimation = true;
    private bool isFrozen = false;

    private void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            Debug.Log($"[HitReaction] Animator auto-assigned: {(animator != null ? animator.name : "None")}");
        }

        if (renderers == null || renderers.Length == 0)
        {
            renderers = GetComponentsInChildren<SpriteRenderer>();
            Debug.Log($"[HitReaction] Auto-assigned {renderers.Length} SpriteRenderers.");
        }
    }

    public void TriggerHit()
    {
        Debug.Log($"[HitReaction] TriggerHit() called on {gameObject.name}");
        if (!isFrozen)
            StartCoroutine(HitRoutine());
        else
            Debug.Log("[HitReaction] Ignored — already flashing.");
    }

    private IEnumerator HitRoutine()
    {
        isFrozen = true;
    

        if (freezeAnimation && animator != null)
            animator.speed = 0f;

        yield return new WaitForSeconds(freezeDuration);

        if (freezeAnimation && animator != null)
            animator.speed = 1f;

        isFrozen = false;
    }
}
