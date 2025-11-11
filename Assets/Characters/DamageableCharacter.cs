using System.Collections;
using UnityEngine;

public abstract class DamageableCharacter : MonoBehaviour, IDamageable
{
    public static event System.Action<DamageableCharacter> OnAnyCharacterDeath;

    [Header("General Settings")]
    public GameObject healthText;
    public bool disableSimulation = false;
    public bool canTurnInvincible = false;
    public float invincibilityTime = 0.25f;
    public float _health = 10f;
    public float maxHealth = 10f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] impactSounds;

    protected Animator animator;
    protected Rigidbody2D rb;
    protected Collider2D physicsCol;
    protected DamageFlash _damageFlash;

    protected bool isAlive = true;
    public bool IsAlive => isAlive;
    protected bool _targetable = true;
    public bool _invincible = false;
    public bool invincibleOverride = false;
    protected float invincibleTimeElapsed = 0f;

    public bool showInvincibilityGizmo = true;
    private float invincibilityGizmoAlpha = 0f;

    public bool Targetable
    {
        get => _targetable;
        set
        {
            _targetable = value;
            if (disableSimulation) 
                rb.simulated = false;
            physicsCol.enabled = value;
        }
    }

    public bool Invincible
    {
        get => _invincible;
        set
        {
            _invincible = value;
            if (value) 
                invincibleTimeElapsed = 0f;
        }
    }

    public float Health
    {
        get => _health;
        set
        {
            if (value < _health)
            {
                OnDamageTaken(_health - value);
            }

            _health = value;

            if (CompareTag("Player") && GameEvents.Instance != null)
            {
                Debug.Log($"[DamageableCharacter] Player health updated: {_health}/{maxHealth}");
                GameEvents.Instance.PlayerHealthChanged(_health, maxHealth);
            }

            if (_health <= 0 && isAlive)
            {
                isAlive = false;
                Targetable = false;
                rb.simulated = false;
                animator.SetBool("isAlive", false);
                HandleDeath();
            }
        }
    }

    protected virtual void Start()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        physicsCol = GetComponent<Collider2D>();
        _damageFlash = GetComponent<DamageFlash>();

        if (animator != null)
            animator.SetBool("isAlive", isAlive);
    }

    protected virtual void FixedUpdate()
    {
        invincibilityGizmoAlpha = Invincible ? 1f : 0f;
        if (!invincibleOverride)
        {
            if (Invincible)
            {
                invincibleTimeElapsed += Time.deltaTime;
                if (invincibleTimeElapsed > invincibilityTime)
                    Invincible = false;
            }
        }
        else
            Invincible = true;  // When override is active, force invincible ON every frame
            
    }

    public virtual void OnHit(float damage, Vector2 knockback)
    {
        if (Invincible) return;

        Health -= damage;
        rb.AddForce(knockback, ForceMode2D.Impulse);
        _damageFlash?.CallDamageFlash();

        var hitReaction = GetComponent<HitReaction>();
        hitReaction?.TriggerHit();

        if (TryGetComponent(out EnemyAI enemyAI))
        {
            Debug.Log($"[DamageableCharacter] {name} was hit — attempting to stun.");

            // Optional: Check for shield logic later (enemyAI.HasActiveShield())
            // if (!enemyAI.HasActiveShield()) // Only stun if shield is gone
            enemyAI.ApplyStun(0.7f);
        }
        
        // Allow falling into holes when hit
        if (TryGetComponent(out FallableCharacter fallable))
        {
            fallable.OnHit(0.7f); // temporarily ignore GroundEdge collisions for 0.7 seconds
        }

        if (canTurnInvincible)
            Invincible = true;
    }

    public virtual void OnHit(float damage)
    {
        if (Invincible) return;

        Health -= damage;
        _damageFlash?.CallDamageFlash();

        var hitReaction = GetComponent<HitReaction>();
        hitReaction?.TriggerHit();

        if (TryGetComponent(out EnemyAI enemyAI))
        {
            // Optional: Check for shield logic later (enemyAI.HasActiveShield())
            enemyAI.ApplyStun(0.7f);
        }

        // Allow falling into holes when hit
        if (TryGetComponent(out FallableCharacter fallable))
        {
            fallable.OnHit(0.7f); // temporarily ignore GroundEdge collisions for 0.7 seconds
        }

        if (canTurnInvincible)
            Invincible = true;
    }

    protected void PlayImpactSound()
    {
        if (impactSounds.Length == 0 || audioSource == null) return;
        int randomIndex = Random.Range(0, impactSounds.Length);
        AudioClip clip = impactSounds[randomIndex];

        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.volume = 0.15f;
        audioSource.PlayOneShot(clip);
    }

    protected void OnDamageTaken(float damageDealt)
    {
        if (healthText == null) return;

        var textTransform = Instantiate(healthText).GetComponent<RectTransform>();
        textTransform.position = Camera.main.WorldToScreenPoint(transform.position);
        var canvas = GameObject.FindFirstObjectByType<Canvas>();
        textTransform.SetParent(canvas.transform);
        textTransform.GetComponent<HealthText>().SetDamageText(damageDealt);

        PlayImpactSound();
    }

    protected virtual void HandleDeath()
    {
        OnAnyCharacterDeath?.Invoke(this);
        StartCoroutine(DeathSequence());
    }

    protected virtual IEnumerator DeathSequence()
    {
        // default: wait, then remove object
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }

    public void RemoveEnemy()
    {
        Destroy(gameObject);
    }

    public void PlaySpawnAnimation(float duration)
    {
        if (animator != null)
            animator.SetTrigger("spawn");
        StartCoroutine(EndSpawnAfter(duration));
    }

    private IEnumerator EndSpawnAfter(float duration)
    {
        yield return new WaitForSeconds(duration);
        animator?.SetBool("spawnDone", true);
    }

    private void OnDrawGizmos()
    {
        if (!showInvincibilityGizmo || invincibilityGizmoAlpha <= 0f)
            return;

        // Choose color and radius
        Color gizmoColor = new Color(1f, 1f, 0f, invincibilityGizmoAlpha * 0.5f); // yellowish, semi-transparent
        Gizmos.color = gizmoColor;

        float radius = 1.0f; // size around player
        Gizmos.DrawSphere(transform.position, radius);
    }

}
