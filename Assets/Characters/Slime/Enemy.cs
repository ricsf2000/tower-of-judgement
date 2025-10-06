using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float health = 10.0f;
    Animator animator;
    bool isAlive = true;

    public void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("isAlive", isAlive);
    }
    
    public void OnHit(float damage)
    {
        Debug.Log("Slime hit for " + damage);

        health -= damage;

        if (health > 0)
        {
            // Play "hit" reaction if still alive
            animator.SetTrigger("hit");
        }
        else if (health <= 0)
        {
            // Trigger death animation
            Defeated();
        }
    }
    public void Defeated()
    {
        Debug.Log($"{gameObject.name} calling Defeated() with isAlive = {isAlive}, health = {health}");
        Debug.Log($"{gameObject.name} defeated!");
        animator.SetBool("isAlive", false);
    }

    public void RemoveEnemy()
    {
        Destroy(gameObject);
    }

}
