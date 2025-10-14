using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeController : MonoBehaviour
{
    private Image fadeImage;

    void Awake()
    {
        fadeImage = GetComponent<Image>();
    }

    public IEnumerator FadeIn(float duration)
    {
        yield return Fade(0f, 1f, duration); // transparent → black
    }

    public IEnumerator FadeOut(float duration)
    {
        yield return Fade(1f, 0f, duration); // black → transparent
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        Color c = fadeImage.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            fadeImage.color = c;
            yield return null;
        }

        c.a = endAlpha;
        fadeImage.color = c;
    }
}
