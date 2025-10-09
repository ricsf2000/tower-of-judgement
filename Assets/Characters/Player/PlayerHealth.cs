using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour//, IDamageable
{
    public float Health { get => throw new System.NotImplementedException();  set => throw new System.NotImplementedException(); }
    public bool Targetable { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    
    public float maxHealth = 10.0f;
    public float _health = 3.0f;
    public bool _targetable = true;
    public float currentHealth;
    Animator animator;
    Rigidbody2D rb;
    Collider2D physicsCol;
    bool isAlive = true;

    public void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        animator.SetBool("isAlive", isAlive);
        rb = GetComponent<Rigidbody2D>();
        physicsCol = GetComponent<Collider2D>();
    }

    public void OnHit(float damage)
    {
        Debug.Log("Player hit for " + damage);

        currentHealth -= damage;

        if (currentHealth > 0)
        {
            // Play "hit" reaction if still alive
            animator.SetTrigger("hit");
        }
        else if (currentHealth <= 0)
        {
            // Trigger death animation
            Defeated();
        }
    }

    public void OnHit(float damage, Vector2 knockback)
    {
        Debug.Log("Player hit for " + damage);

        currentHealth -= damage;
        // Apply knockback force
        rb.AddForce(knockback);
        Debug.Log("Knockback applied: " + knockback);
        if (currentHealth > 0)
        {
            // Play "hit" reaction if still alive
            animator.SetTrigger("hit");
        }
        else if (currentHealth <= 0)
        {
            // Trigger death animation
            Defeated();
        }
    }

    void Defeated()
    {
        isAlive = false;
        animator.SetBool("isAlive", isAlive);
        physicsCol.enabled = false;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        this.enabled = false;
        Debug.Log("Player defeated");
    }
}