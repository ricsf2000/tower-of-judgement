using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class CinemachineShake : MonoBehaviour
{
    public static CinemachineShake Instance { get; private set; }

    private CinemachineCamera cam;
    private CinemachineBasicMultiChannelPerlin perlin;

    private void Awake()
    {
        Instance = this;
        cam = GetComponent<CinemachineCamera>();

        perlin = cam.GetComponent<CinemachineBasicMultiChannelPerlin>();
        if (perlin == null)
        {
            perlin = cam.gameObject.AddComponent<CinemachineBasicMultiChannelPerlin>();
        }

        // Reset initial values
        perlin.AmplitudeGain = 0f;
        perlin.FrequencyGain = 0f;
    }

    public void Shake(float amplitude, float frequency, float duration)
    {
        Debug.Log($"[CinemachineShake] Shake triggered! amp={amplitude}, freq={frequency}, dur={duration}");
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine(amplitude, frequency, duration));
    }

    private IEnumerator ShakeRoutine(float amplitude, float frequency, float duration)
    {
        perlin.AmplitudeGain = amplitude;
        perlin.FrequencyGain = frequency;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            perlin.AmplitudeGain = Mathf.Lerp(amplitude, 0f, t);
            yield return null;
        }

        perlin.AmplitudeGain = 0f;
        perlin.FrequencyGain = 0f;
    }
}
