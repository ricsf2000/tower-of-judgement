using UnityEngine;
using System.Collections;

public class TempPlatform : MonoBehaviour
{
    [Header("Platform Settings")]
    public float disappearDelay = 2f;
    public float respawnDelay = 3f;
    public bool autoRespawn = true;

    [Tooltip("If true, stepping on this platform causes it to collapse.")]
    public bool collapsesOnStep = true;


    private SpriteRenderer[] renderers;
    private Collider2D col;
    private Color[] originalColors;

    private bool collapseTriggered = false;

    private DamageFlash damageFlash;

    private void Awake()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        damageFlash = GetComponent<DamageFlash>();

        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
            originalColors[i] = renderers[i].color;

        gameObject.layer = LayerMask.NameToLayer("PlatformLayer");
    }

    // Trigger collapse when any FallableCharacter enters
    private void OnTriggerEnter2D(Collider2D other)
    {
        TryCollapse(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryCollapse(other);
    }

    private void TryCollapse(Collider2D other)
    {
        if (collapseTriggered || !collapsesOnStep)
            return;

        // Ignore if player is dashing
        PlayerDash dash = other.GetComponent<PlayerDash>();
        if (dash != null && dash.IsDashing)
            return;

        // Must be a fallable character
        if (other.GetComponent<FallableCharacter>() == null)
            return;

        // Trigger collapse
        collapseTriggered = true;

        if (damageFlash != null)
            damageFlash.CallDamageFlash(Color.red);

        StartCoroutine(DisappearRoutine());
    }

    public void TriggerCollapse()
    {
        if (!collapseTriggered)
        {
            collapseTriggered = true;
            StartCoroutine(DisappearRoutine());
        }
    }

    public void TriggerRespawn()
    {
        StartCoroutine(RespawnRoutine());
    }

    public void ForceShowImmediate()
    {
        StopAllCoroutines();

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;

            renderers[i].enabled = true;
            renderers[i].color = originalColors[i];
        }

        if (col != null)
            col.enabled = true;

        collapseTriggered = false;
    }

    public void ForceHideImmediate()
    {
        StopAllCoroutines();

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;

            renderers[i].enabled = false;
            var baseColor = originalColors[i];
            renderers[i].color = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);
        }

        if (col != null)
            col.enabled = false;

        collapseTriggered = true;
    }

    private IEnumerator DisappearRoutine()
    {
        // Wait before collapse
        yield return new WaitForSeconds(disappearDelay);

        // Fade out
        float fadeTime = 0.2f;
        float elapsed = 0f;

        // Cache starting colors
        Color[] startColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            startColors[i] = renderers[i].color;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, elapsed / fadeTime);

            // Apply alpha to ALL sprites at once
            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer sr = renderers[i];
                sr.color = new Color(startColors[i].r, startColors[i].g, startColors[i].b, a);
            }

            yield return null;
        }


        if (col != null)
            col.enabled = false;

        if (autoRespawn)
            StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;

            renderers[i].enabled = true;
            renderers[i].color = originalColors[i];
        }

        if (col != null)
            col.enabled = true;

        if (damageFlash != null)
        damageFlash.CallDamageFlash(Color.white);

        collapseTriggered = false; // allow triggering again
    }
}
