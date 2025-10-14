using UnityEngine;
using System.Collections;

public class RoomTransition : MonoBehaviour
{
    public Transform teleportTarget;
    public float fadeDuration = 0.5f;

    private FadeController fadeController;
    private bool transitioning = false;

    void Start()
    {
        fadeController = FindObjectOfType<FadeController>(); // grab from UI Canvas
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
        if (fadeController != null)
            yield return fadeController.FadeIn(fadeDuration);  // block until done

        // Teleport instantly while screen is black
        player.position = teleportTarget.position;

        // small pause so the black screen holds for a bit
        yield return new WaitForSeconds(1.0f);

        // Fade back to gameplay
        if (fadeController != null)
            yield return fadeController.FadeOut(fadeDuration);

        transitioning = false;
    }

}
