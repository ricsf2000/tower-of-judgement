using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LevelChange : MonoBehaviour
{
    public string sceneToLoad;
    public float fadeDuration = 0.5f;

    private FadeController fadeController;
    private bool transitioning = false;

    void Start()
    {
        fadeController = FindFirstObjectByType<FadeController>(); // grab from UI Canvas
        if (fadeController == null)
            Debug.LogWarning("[LevelChange] No FadeController found in scene!");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (transitioning) return;
        if (other.CompareTag("Player"))
            StartCoroutine(TeleportPlayer(other.transform));
    }

    private IEnumerator TeleportPlayer(Transform player)
    {
        transitioning = true;

        // Fade fully to black
        if (FadeController.Instance != null)
        {
            Debug.Log("[LevelChange] Calling FadeController.Instance.FadeIn");
            yield return FadeController.Instance.FadeIn(fadeDuration);
        }

        // Change scene
        SceneManager.LoadScene(sceneToLoad);

        // Fade back out
        if (FadeController.Instance != null)
        {
            StartCoroutine(FadeController.Instance.FadeOut(fadeDuration));
        }
       
        transitioning = false;
    }
}
