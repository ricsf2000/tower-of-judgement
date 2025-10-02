using Unity.VisualScripting;
using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    public Collider2D swordCollider;
    public float damage = 3;

    public Vector3 faceRight = new Vector3(.107f, 0.085f, 0);
    public Vector3 faceLeft = new Vector3(-.107f, 0.085f, 0);

    void Start()
    {
        if (swordCollider != null)
        {
            Debug.LogWarning("Sword collider not set");
        }
    }

    void IsFacingRight(bool isFacingRight)
    {
        if (isFacingRight)
        {
            gameObject.transform.localPosition = faceRight;
        }
        else
        {
            gameObject.transform.localPosition = faceLeft;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        other.SendMessage("OnHit", damage);
        Debug.Log("Hit for " + damage + " points");
    }
}
