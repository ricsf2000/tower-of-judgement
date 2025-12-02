using UnityEngine;
using UnityEngine.SceneManagement;

public class WhiteSpaceExit : MonoBehaviour
{
    [SerializeField] private string endingSceneName = "EndingScene";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (!string.IsNullOrEmpty(endingSceneName))
            SceneManager.LoadScene(endingSceneName);
    }
}

