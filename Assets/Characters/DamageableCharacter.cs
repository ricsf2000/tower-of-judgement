using TMPro;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

// A base class for damageable characters (both players and enemies)
public class DamageableCharacter : MonoBehaviour, IDamageable
{
    public GameObject healthText;
    public bool disableSimulation = false;
    public bool canTurnInvincible = false;
    public float invincibilityTime = 0.25f;
    Animator animator;
    Rigidbody2D rb;
    Collider2D physicsCol;
    bool isAlive = true;
    private float invincibleTimeElapsed = 0.0f;

    public float Health
    {
        set
        {
            if (value < _health)
            {
                animator.SetTrigger("hit");
                RectTransform textTransform = Instantiate(healthText).GetComponent<RectTransform>();
                textTransform.transform.position = Camera.main.WorldToScreenPoint(gameObject.transform.position);

                Canvas canvas = GameObject.FindFirstObjectByType<Canvas>();
                textTransform.SetParent(canvas.transform);

                float damageDealt = _health - value;
                textTransform.GetComponent<HealthText>().SetDamageText(damageDealt);
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

    public bool Invincible
    {
        get
        {
            return _invincible;
        }
        set
        {
            _invincible = value;

            if (_invincible == true)
            {
                invincibleTimeElapsed = 0.0f;
            }
        }
    }

    [SerializeField] private float _health = 10.0f;
    bool _targetable = true;

    public bool _invincible = false;

    public void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("isAlive", isAlive);
        rb = GetComponent<Rigidbody2D>();
        physicsCol = GetComponent<Collider2D>();
    }

    public void OnHit(float damage, Vector2 knockback)
    {
        if (!Invincible)
        {
            Health -= damage;

            // Apply knockback force
            rb.AddForce(knockback, ForceMode2D.Impulse);
            Debug.Log("Knockback applied: " + knockback);

            if (canTurnInvincible)
            {
                // Activate invincibility and timer
                Invincible = true;
            }
        }
    }

    public void OnHit(float damage)
    {
        if (!Invincible)
        {
            Debug.Log("Slime hit for " + damage);
            Health -= damage;

            if (canTurnInvincible)
            {
                // Activate invincibility and timer
                Invincible = true;
            }
        }
    }
    public void RemoveEnemy()
    {
        Destroy(gameObject);
    }

    public void FixedUpdate()
    {
        if (Invincible)
        {
            invincibleTimeElapsed += Time.deltaTime;

            if (invincibleTimeElapsed > invincibilityTime)
            {
                Invincible = false;
            }
        }
    }

}
