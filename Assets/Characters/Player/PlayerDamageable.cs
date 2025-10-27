using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerDamageable : DamageableCharacter
{
    private Volume _volume;
    private Vignette _vignette;

    protected override void Start()
    {
        base.Start();

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
        CinemachineShake.Instance?.Shake(1f, 3.5f, 0.25f);

        // Player UI
        if (GameEvents.Instance != null)
            GameEvents.Instance.PlayerHealthChanged(_health, maxHealth);

        // Save back to persistent data
        if (PlayerData.Instance != null)
            PlayerData.Instance.currentHealth = _health;

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
            controller.transform.position = controller.respawnPoint;
        }

        // Restore targetability and rigidbody state
        Targetable = true;
        rb.simulated = true;

        Debug.Log("[PlayerDamageable] Player reset after retry.");
    }
}