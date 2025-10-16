using System.Collections;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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

    public float _health = 10.0f;
    public float maxHealth = 10.0f;
    bool _targetable = true;
    public bool _invincible = false;

    public bool IsSpawning { get; private set; }
    [HideInInspector] public bool SpawnedByWave = false;


    [Header("Audio")]
    public AudioSource audioSource;       // plays the sound
    public AudioClip[] impactSounds;      // assign 3+ impact sounds in Inspector

    [Header("Player Only Stuff")]
    private float intensity = 0.0f;
    Volume _volume;
    Vignette _vignette;

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
                Debug.Log($"[Player Health] Taking damage! Old: {_health}, New: {value}");
            }

            _health = value;

            // For health bar
            if (CompareTag("Player") && GameEvents.Instance != null)
            {
                GameEvents.Instance.PlayerHealthChanged(_health, maxHealth);
            }


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

        _volume = FindFirstObjectByType<Volume>();
        if (_volume == null)
        {
            Debug.LogWarning("[DamageableCharacter] No Volume found!");
            return;
        }

        if (!_volume.profile.TryGet(out _vignette))
        {
            Debug.LogWarning("[DamageableCharacter] No Vignette override found in Volume profile!");
            return;
        }

        _vignette.active = false;

    }

    public void PlaySpawnAnimation(float duration)
    {
        if (animator != null)
            animator.SetTrigger("spawn");

        StartCoroutine(EndSpawnAfter(duration));
    }

    private IEnumerator EndSpawnAfter(float duration)
    {
        IsSpawning = true;
        yield return new WaitForSeconds(duration);
        IsSpawning = false;

        if (animator != null)
            animator.SetBool("spawnDone", true);
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

            if (impactSounds.Length > 0 && audioSource != null)
            {
                // Pick a random impact sound
                int randomIndex = Random.Range(0, impactSounds.Length);
                AudioClip clip = impactSounds[randomIndex];

                // Randomize the pitch
                audioSource.pitch = Random.Range(0.95f, 1.05f);

                audioSource.volume = 0.15f;

                // Play it
                audioSource.PlayOneShot(clip);
            }

            if (CompareTag("Player"))
            {
                CinemachineShake.Instance.Shake(1f, 3.5f, 0.2f);
                StartCoroutine(TakeDamageEffect());
            }

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
            {
                CinemachineShake.Instance.Shake(1f, 3.5f, 0.4f);
                StartCoroutine(TakeDamageEffect());
            }

            if (canTurnInvincible)
            {
                // Activate invincibility and timer
                Invincible = true;
            }
        }
    }

    private IEnumerator TakeDamageEffect()
    {
        if (_vignette == null)
            yield break;

        float maxIntensity = 0.45f;
        float duration = 0.2f;

        _vignette.active = true;
        _vignette.color.value = Color.red;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float value = Mathf.Sin(t * Mathf.PI); // fade in/out
            _vignette.intensity.value = value * maxIntensity;
            yield return null;
        }

        _vignette.intensity.value = 0;
        _vignette.active = false;
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
