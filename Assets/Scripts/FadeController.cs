using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.Collections;

public class FadeController : MonoBehaviour
{
    public static FadeController Instance { get; private set; }
    public UnityEngine.UI.Image fadeImage;
    public float defaultDuration = 0.5f;

    // new flag
    public bool autoFadeOutOnSceneLoad = true;
    
    // Static flag to show health bar when next scene loads
    public static bool shouldShowHealthBarOnNextScene = false;

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
        // Reset camera zoom if it was modified in previous scene
        ResetCameraZoom();
        
        // Show health bar if flag is set (from boss defeat cutscene)
        if (shouldShowHealthBarOnNextScene)
        {
            ShowHealthBar();
            shouldShowHealthBarOnNextScene = false;
        }
        
        if (autoFadeOutOnSceneLoad)
            StartCoroutine(FadeOut(defaultDuration));
    }
    
    private void ShowHealthBar()
    {
        UIHandler uiHandler = FindFirstObjectByType<UIHandler>();
        if (uiHandler != null && uiHandler.uiDocument != null)
        {
            var root = uiHandler.uiDocument.rootVisualElement;
            var healthBarElement = root.Q<VisualElement>("HealthBar");
            if (healthBarElement != null)
            {
                healthBarElement.style.display = DisplayStyle.Flex;
                Debug.Log("[FadeController] Player health bar shown in new scene.");
            }
        }
    }
    
    private void ResetCameraZoom()
    {
        // Reset Cinemachine camera zoom to default when new scene loads
        // This ensures the zoom from the boss cutscene doesn't persist
        var cinemachineCamera = FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();
        if (cinemachineCamera != null)
        {
            // The new scene should have its own camera setup, but if Cinemachine persists,
            // we need to reset it. Try to get the default from the main camera in the new scene
            Camera mainCam = Camera.main;
            if (mainCam != null && mainCam.orthographic)
            {
                var lens = cinemachineCamera.Lens;
                lens.OrthographicSize = mainCam.orthographicSize;
                cinemachineCamera.Lens = lens;
                Debug.Log($"[FadeController] Reset Cinemachine camera zoom to: {mainCam.orthographicSize}");
            }
            else
            {
                // Fallback: reset to a reasonable default (adjust if needed)
                var lens = cinemachineCamera.Lens;
                if (lens.OrthographicSize < 1.0f) // If it's zoomed in (less than 1.0), reset it
                {
                    lens.OrthographicSize = 1.5f; // Default orthographic size
                    cinemachineCamera.Lens = lens;
                    Debug.Log("[FadeController] Reset Cinemachine camera zoom to default: 1.5");
                }
            }
        }
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
