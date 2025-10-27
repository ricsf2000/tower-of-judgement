using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeController : MonoBehaviour
{
    public static FadeController Instance { get; private set; }
    public UnityEngine.UI.Image fadeImage;
    public float defaultDuration = 0.5f;

    // new flag
    public bool autoFadeOutOnSceneLoad = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (autoFadeOutOnSceneLoad)
            StartCoroutine(FadeOut(defaultDuration));
    }

    public IEnumerator FadeIn(float duration)
    {
        Color c = fadeImage.color;
        for (float t = 0f; t < 1f; t += Time.deltaTime / duration)
        {
            c.a = t;
            fadeImage.color = c;
            yield return null;
        }
        c.a = 1f;
        fadeImage.color = c;
    }

    public IEnumerator FadeOut(float duration)
    {
        Color c = fadeImage.color;
        for (float t = 1f; t > 0f; t -= Time.deltaTime / duration)
        {
            c.a = t;
            fadeImage.color = c;
            yield return null;
        }
        c.a = 0f;
        fadeImage.color = c;
    }
}
