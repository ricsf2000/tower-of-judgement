using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public bool start = false;
    public float defaultDuration = 0.5f;
    public AnimationCurve defaultCurve;

    public static CameraShake Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void Shake(float duration = -1f, AnimationCurve curve = null)
    {
        if (duration < 0f) duration = defaultDuration;
        if (curve == null) curve = defaultCurve;
        StartCoroutine(Shaking(duration, curve));
    }

    private IEnumerator Shaking(float duration, AnimationCurve curve)
    {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0.0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float strength = curve.Evaluate(elapsedTime / duration);
            transform.position = startPosition + Random.insideUnitSphere * strength;
            yield return null;
        }

        transform.position = startPosition;
    }
}
