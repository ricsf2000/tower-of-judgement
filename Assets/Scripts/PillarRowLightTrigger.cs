using UnityEngine;


public class PillarRowLightTrigger : MonoBehaviour
{
    public UnityEngine.Rendering.Universal.Light2D[] lightsInRow;
    public float fadeSpeed = 2f;
    private bool triggered = false;

    void Start()
    {
        // start turned off
        foreach (var light in lightsInRow)
            light.intensity = 0f;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;
            StartCoroutine(FadeLightsIn());
        }
    }

    private System.Collections.IEnumerator FadeLightsIn()
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * fadeSpeed;
            foreach (var light in lightsInRow)
            {
                light.intensity = Mathf.Lerp(0f, 1f, t);
            }

            yield return null;
        }
    }
}
