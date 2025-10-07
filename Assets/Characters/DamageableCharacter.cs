using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

// A base class for damageable characters (both players and enemies)
public class DamageableChar : MonoBehaviour, IDamageable
{
    public bool disableSimulation = false;
    public float _health = 10.0f;
    Animator animator;
    Rigidbody2D rb;
    Collider2D physicsCol;
    bool isAlive = true;

    public float Health
    {
        set
        {
            if (value < _health)
            {
                animator.SetTrigger("hit");
            }

            _health = value;

            if (_health <= 0)
            {
                animator.SetBool("isAlive", false);
                Targetable = false;
            }
        }
        get
        {
            return _health;
        }
    }

    public void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("isAlive", isAlive);
        rb = GetComponent<Rigidbody2D>();
        physicsCol = GetComponent<Collider2D>();
    }

    public void OnHit(float damage, Vector2 knockback)
    {
        Health -= damage;

        // Apply knockback force
        rb.AddForce(knockback, ForceMode2D.Impulse);
        Debug.Log("Knockback applied: " + knockback);
    }

    public void OnHit(float damage)
    {
        Debug.Log("Slime hit for " + damage);
        Health -= damage;
    }

    public bool Targetable
    {
        get
        {
            return _targetable;
        }
        set
        {
            _targetable = value;
            if (disableSimulation)
            {
                rb.simulated = false;
            }

            physicsCol.enabled = value;
        }
    }
    public bool _targetable = true;

    public void RemoveEnemy()
    {
        Destroy(gameObject);
    }

}
