using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    [SerializeField] private EnemyWaveManager waveManager;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            Debug.Log("[RoomTrigger] Player entered trigger — enabling WaveManager!");
            if (waveManager != null)
            {
                waveManager.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError("[RoomTrigger] WaveManager not assigned!");
            }

            gameObject.SetActive(false); // disable trigger after activation
        }
    }
}
