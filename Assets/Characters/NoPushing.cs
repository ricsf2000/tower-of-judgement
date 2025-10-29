using UnityEngine;

public class NoPushing : MonoBehaviour
{
    [HideInInspector] public GameObject collisionShell;
    private void Start()
    {
        Collider2D parentCol = GetComponent<Collider2D>();
        Collider2D childCol = null;

        // Find the child collider attached to a Kinematic Rigidbody2D
        foreach (Rigidbody2D rb in GetComponentsInChildren<Rigidbody2D>())
        {
            if (rb != GetComponent<Rigidbody2D>() && rb.bodyType == RigidbodyType2D.Kinematic)
            {
                childCol = rb.GetComponent<Collider2D>();
                collisionShell = rb.gameObject;
                break;
            }
        }

        Physics2D.IgnoreCollision(parentCol, childCol, true);
    }

    public void DisableShell()
    {
        if (collisionShell != null)
        {
            collisionShell.SetActive(false);
        }
    }
}
