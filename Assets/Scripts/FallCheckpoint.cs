using UnityEngine;

public class FallCheckpoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            FallableCharacter fallable = other.GetComponent<FallableCharacter>();
            if (fallable != null)
            {
                fallable.respawnPoint = this.transform; // or transform.position if using Vector3
                Debug.Log($"[Checkpoint] Respawn point set to {transform.position}");
            }
        }
    }
}
