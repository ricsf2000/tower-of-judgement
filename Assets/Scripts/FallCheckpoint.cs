using UnityEngine;

public class FallCheckpoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.respawnPoint = this.transform.position;
                Debug.Log($"[Checkpoint] Respawn point set to {transform.position}");
            }
        }
    }
}
