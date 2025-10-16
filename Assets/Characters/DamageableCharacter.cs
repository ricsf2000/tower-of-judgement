using System.Collections;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

// A base class for damageable characters (both players and enemies)
public class DamageableCharacter : MonoBehaviour, IDamageable
{
    public static event System.Action<DamageableCharacter> OnAnyCharacterDeath;
    public GameObject healthText;
    public bool disableSimulation = false;
    public bool canTurnInvincible = false;
    public float invincibilityTime = 0.25f;
    Animator animator;
    Rigidbody2D rb;
    Collider2D physicsCol;
    bool isAlive = true;
    private float invincibleTimeElapsed = 0.0f;

    private DamageFlash _damageFlash;

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
                rb.simulated = false;

                // Try to find the closest WaveManager in the hierarchy
                EnemyWaveManager manager = GetComponentInParent<EnemyWaveManager>();

                if (manager == null)
                {
                    // Fallback for scene-level managers
                    manager = FindFirstObjectByType<EnemyWaveManager>();
                }

                if (manager != null)
                {
                    manager.RemoveEnemy(this);
                }

                if (CompareTag("Player"))
                    PlayerDied();
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

    public float _health = 10.0f;
    bool _targetable = true;

    public bool _invincible = false;

    public void Start()
    {
        animator = GetComponentInChildren<Animator>();
        if (animator != null)
            Debug.Log($"[{name}] Found animator: {animator.name}");
        else
            Debug.LogError($"[{name}] No Animator found!");
        animator.SetBool("isAlive", isAlive);
        rb = GetComponent<Rigidbody2D>();
        physicsCol = GetComponent<Collider2D>();
        _damageFlash = GetComponent<DamageFlash>();
        Debug.Log($"[{name}] spawned with Health = {_health}, Targetable = {Targetable}");
    }

    public void OnHit(float damage, Vector2 knockback)
    {
        if (!Invincible)
        {
            Health -= damage;

            // Apply knockback force
            rb.AddForce(knockback, ForceMode2D.Impulse);
            Debug.Log("Knockback applied: " + knockback);

            // Damage flash effect
            _damageFlash.CallDamageFlash();

            HitReaction hitReaction = GetComponent<HitReaction>();
            if (hitReaction != null)
                hitReaction.TriggerHit();

            if (CompareTag("Player"))
                CinemachineShake.Instance.Shake(1f, 3.5f, 0.2f);

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
            Debug.Log("Enemy hit for " + damage);
            Health -= damage;

            // Damage flash effect
            _damageFlash.CallDamageFlash();

            HitReaction hitReaction = GetComponent<HitReaction>();
            if (hitReaction != null)
                hitReaction.TriggerHit();

            if (CompareTag("Player"))
                CinemachineShake.Instance.Shake(1f, 3.5f, 0.4f);

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
    // DamageableCharacter.cs (replace PlayerDied)
    private void PlayerDied()
    {
        StartCoroutine(DeathSequence());
    }

    private System.Collections.IEnumerator DeathSequence()
    {
        // stop collisions/physics, but keep object active so the Animator can play
        Targetable = false;
        rb.simulated = false;

        // ensure the death animation is playing
        animator.SetBool("isAlive", false);

        // Wait for the current state's length 
        // float wait = animator.GetCurrentAnimatorStateInfo(0).length;
        // if (wait <= 0f) wait = 0.5f;  // fallback

        // yield return new WaitForSeconds(wait);
        yield return new WaitForSeconds(2.0f);

        // Open the death panel
        LevelManager.manager.GameOver();

    }

}
