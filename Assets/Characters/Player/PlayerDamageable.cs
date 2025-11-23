using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerDamageable : DamageableCharacter
{
    private Volume _volume;
    private Vignette _vignette;
    private PlayerSFX sfx;

    public bool IsFullHealth => _health >= maxHealth;
    


    protected override void Start()
    {
        base.Start();

        sfx = GetComponent<PlayerSFX>();

        // Sync maxHealth from PlayerData (single source of truth)
        if (PlayerData.Instance != null)
        {
            maxHealth = PlayerData.Instance.maxHealth;
            _health = PlayerData.Instance.currentHealth;
        }
        else
        {
            _health = maxHealth;
        }

        _volume = FindFirstObjectByType<Volume>();
        if (_volume && _volume.profile.TryGet(out _vignette))
            _vignette.active = false;

        // Explicitly notify UI after health is loaded
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.PlayerHealthChanged(_health, maxHealth);
            Debug.Log($"[PlayerDamageable] Triggered UI update: {_health}/{maxHealth}");
        }
    }

    public override void OnHit(float damage, Vector2 knockback)
    {
        if (Invincible) return;

        base.OnHit(damage, knockback);

        // Screen shake
        CinemachineShake.Instance?.Shake(1f, 3.5f, 0.50f);

        // Player UI
        if (GameEvents.Instance != null)
            GameEvents.Instance.PlayerHealthChanged(_health, maxHealth);

        // Save back to persistent data
        if (PlayerData.Instance != null)
            PlayerData.Instance.currentHealth = _health;

        // Trigger vignette overlay
        StartCoroutine(DamageVignetteEffect());
    }

    public override void OnHit(float damage)
    {
        if (Invincible) return;

        base.OnHit(damage);

        // Screen shake (optional for fall)
        CinemachineShake.Instance?.Shake(1f, 3.5f, 0.50f);

        // UI and persistent updates
        if (GameEvents.Instance != null)
            GameEvents.Instance.PlayerHealthChanged(_health, maxHealth);

        if (PlayerData.Instance != null)
            PlayerData.Instance.currentHealth = _health;

        // Trigger vignette overlay
        StartCoroutine(DamageVignetteEffect());
    }


    protected override IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(2.0f);
        LevelManager.manager.GameOver();
    }

    private IEnumerator DamageVignetteEffect()
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

    public void ResetPlayer()
    {
        // Restore full health from PlayerData
        if (PlayerData.Instance != null)
        {
            PlayerData.Instance.RestoreFullHealth();
            maxHealth = PlayerData.Instance.maxHealth;
            _health = PlayerData.Instance.currentHealth;
        }
        else
        {
            _health = maxHealth;
        }

        isAlive = true;

        // Restore player movement and physics
        var controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.canMove = true;
            controller.Rb.simulated = true;
            controller.Animator.SetBool("isAlive", true);
            var fallable = GetComponent<FallableCharacter>();
            if (fallable != null && fallable.respawnPosition != null)
            {
                controller.transform.position = fallable.respawnPosition;
            }
        }

        // Restore targetability and rigidbody state
        Targetable = true;
        rb.simulated = true;

        Debug.Log("[PlayerDamageable] Player reset after retry.");
    }
    
    public void RestoreHealth(float amount)
    {
        if (amount <= 0) return;
        if (!isAlive) return;

        // Apply heal & clamp
        _health = Mathf.Clamp(_health + amount, 0, maxHealth);

        // UI update
        GameEvents.Instance?.PlayerHealthChanged(_health, maxHealth);

        // Persist
        if (PlayerData.Instance != null)
            PlayerData.Instance.currentHealth = _health;

        // Play heal SFX
        sfx.PlayHealFX();

        // Stop damage vignette
        StopCoroutine(nameof(DamageVignetteEffect));
        if (_vignette != null)
        {
            _vignette.intensity.value = 0f;
            _vignette.active = false;
        }

        // Play green heal vignette
        StartCoroutine(HealVignetteEffect());

        Debug.Log($"[PlayerDamageable] Restored health. Now {_health}/{maxHealth}");
    }

    private IEnumerator HealVignetteEffect()
    {
        if (_vignette == null)
            yield break;

        float maxIntensity = 0.35f;    // Slightly softer than red
        float duration = 0.25f;

        _vignette.active = true;
        _vignette.color.value = Color.green;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float value = Mathf.Sin(t * Mathf.PI);
            _vignette.intensity.value = value * maxIntensity;
            yield return null;
        }

        _vignette.intensity.value = 0;
        _vignette.active = false;
    }


    public void RestoreHealthPercent(float percent)
    {
        if (!isAlive) return;

        float healRaw = maxHealth * percent;
        int healAmount = Mathf.RoundToInt(healRaw);

        if (healAmount < 1)
            healAmount = 1; // Ensure always heals at least 1

        RestoreHealth(healAmount);
    }


}