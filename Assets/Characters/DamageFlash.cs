using System.Collections;
using UnityEngine;

public class DamageFlash : MonoBehaviour
{
    [ColorUsage(true, true)]
    [SerializeField] private Color _flashColor = Color.white;
    [SerializeField] private float _flashTime = 0.25f;
    [SerializeField] private AnimationCurve _flashSpeedCurve;

    private SpriteRenderer[] _spriteRenderers;
    private Material[] _materials;
    private Coroutine _damageFlashCoroutine;

    private void Awake()
    {
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        Init();
    }

    private void Init()
    {
        _materials = new Material[_spriteRenderers.Length];

        // Assign sprite renderer materials to _materials
        for (int i = 0; i < _spriteRenderers.Length; i++)
        {
            _materials[i] = _spriteRenderers[i].material;
        }
    }

    public void CallDamageFlash()
    {
        _damageFlashCoroutine = StartCoroutine(DamageFlasher());
    }

    public void CallDamageFlash(Color overrideColor)
    {
        _flashColor = overrideColor;
        _damageFlashCoroutine = StartCoroutine(DamageFlasher());
    }

    public void CallPersistentFlash(Color color, float fadeInTime, float maxIntensity)
    {
        if (_damageFlashCoroutine != null)
            StopCoroutine(_damageFlashCoroutine);

        _damageFlashCoroutine = StartCoroutine(PersistentFlashFadeIn(color, fadeInTime, maxIntensity));
    }

    public void ClearPersistentFlash(float fadeOutTime = 0.25f)
    {
        if (_damageFlashCoroutine != null)
            StopCoroutine(_damageFlashCoroutine);

        _damageFlashCoroutine = StartCoroutine(PersistentFlashFadeOut(fadeOutTime));
    }

    private IEnumerator PersistentFlashFadeIn(Color color, float fadeInTime, float maxIntensity)
    {
        // Apply color immediately
        for (int i = 0; i < _materials.Length; i++)
            _materials[i].SetColor("_FlashColor", color);

        float elapsed = 0f;

        while (elapsed < fadeInTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeInTime);
            float amount = Mathf.Lerp(0f, maxIntensity, t);
            SetFlashAmount(amount);
            yield return null;
        }

        SetFlashAmount(maxIntensity); // Lock at full brightness
    }

    private IEnumerator PersistentFlashFadeOut(float fadeOutTime)
    {
        float startAmount = _materials[0].GetFloat("_FlashAmount");
        float elapsed = 0f;

        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutTime);
            float amount = Mathf.Lerp(startAmount, 0f, t);

            SetFlashAmount(amount);

            yield return null;
        }

        SetFlashAmount(0);
    }

    private IEnumerator DamageFlasher()
    {
        // Set color
        SetFlashColor();

        // Lerp flash amount
        float currentFlashAmount = 0.0f;
        float elapsedTime = 0.0f;
        while (elapsedTime < _flashTime)
        {
            // Iterate elapsedTime
            elapsedTime += Time.deltaTime;

            // Lerp flash amount
            currentFlashAmount = Mathf.Lerp(1.0f, _flashSpeedCurve.Evaluate(elapsedTime), (elapsedTime / _flashTime));
            SetFlashAmount(currentFlashAmount);

            yield return null;
        }
    }

    private void SetFlashColor()
    {
        // Set color
        for (int i = 0; i < _materials.Length; i++)
        {
            _materials[i].SetColor("_FlashColor", _flashColor);
        }
    }

    private void SetFlashAmount(float amount)
    {
        // Set flash amount
        for (int i = 0; i < _materials.Length; i++)
        {
            _materials[i].SetFloat("_FlashAmount", amount);
        }
    }
}
