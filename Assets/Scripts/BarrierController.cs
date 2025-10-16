using UnityEngine;

public class BarrierController : MonoBehaviour
{
    private Collider2D col;
    private SpriteRenderer sr;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        sr  = GetComponent<SpriteRenderer>();
    }

    public void ActivateBarrier()
    {
        if (col) col.enabled = true;
        if (sr)  sr.enabled = true;
    }

    public void DeactivateBarrier()
    {
        if (col) col.enabled = false;

        // Optional fade / destroy effect
        if (sr)
        {
            StartCoroutine(FadeOutAndDestroy());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private System.Collections.IEnumerator FadeOutAndDestroy()
    {
        Color c = sr.color;
        for (float t = 0; t < 1f; t += Time.deltaTime)
        {
            c.a = Mathf.Lerp(1f, 0f, t);
            sr.color = c;
            yield return null;
        }
        Destroy(gameObject);
    }
}
