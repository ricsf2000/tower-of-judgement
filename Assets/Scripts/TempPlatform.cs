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

    private Color tempColor;
    private Color[] originalColors; // Now the ONLY stored color array

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

        // Ignore dashing
        PlayerDash dash = other.GetComponent<PlayerDash>();
        if (dash != null && dash.IsDashing)
            return;

        // Must be fallable
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

            Color c = originalColors[i];
            c.a = 0f;
            renderers[i].color = c;
        }

        if (col != null)
            col.enabled = false;

        collapseTriggered = true;
    }

    private IEnumerator DisappearRoutine()
    {
        yield return new WaitForSeconds(disappearDelay);

        float fadeTime = 0.2f;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, elapsed / fadeTime);

            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer sr = renderers[i];
                tempColor = originalColors[i];  // Use original colors
                tempColor.a = a;
                sr.color = tempColor;
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
            SpriteRenderer sr = renderers[i];
            sr.enabled = true;
            sr.color = originalColors[i];
        }

        if (col != null)
            col.enabled = true;

        if (damageFlash != null)
            damageFlash.CallDamageFlash(Color.white);

        collapseTriggered = false;
    }
}
