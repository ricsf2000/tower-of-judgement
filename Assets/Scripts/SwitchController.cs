using UnityEngine;
using UnityEngine.U2D.Animation;
using System.Collections.Generic;
using System.Collections;

public class SwitchController : MonoBehaviour
{
    [Header("Group Settings")]
    [Tooltip("Add all switches in this puzzle group (including this one).")]
    public List<SwitchController> allSwitches; 
    public BarrierController barrier;

    [Header("Camera Focus")]
    public Transform focusTarget;

    [Header("Sprite Resolver")]
    public SpriteResolver spriteResolver;

    [Header("Audio")]
    public AudioClip[] hitToActivate;
    public AudioClip[] hitToDeactivate;
    private AudioSource audioSource;

    [Header("Toggle Settings")]
    [Tooltip("If true, this switch can be turned ON and OFF repeatedly.")]
    public bool isToggleSwitch = false;
    private bool isActivated = false;

    // Platform control
    [Header("Platform Toggle")]
    [Tooltip("Platforms ACTIVE when switch is ON")]
    public List<TempPlatform> platformsForOnState;

    [Tooltip("Platforms ACTIVE when switch is OFF")]
    public List<TempPlatform> platformsForOffState;

    private void Awake()
    {
        if (spriteResolver == null)
            spriteResolver = GetComponent<SpriteResolver>();

        audioSource = GetComponent<AudioSource>();

        if (allSwitches.Count == 0)
            allSwitches.Add(this);

        // If sprite resolver exists, detect initial activation state from its category
        if (spriteResolver != null)
        {
            string cat = spriteResolver.GetCategory();

            if (cat == "On")
                isActivated = true;
            else if (cat == "Off")
                isActivated = false;
        }

        UpdateSprite();

        // Apply initial platform layout
        if (isActivated)
            ApplyOnState();
        else
            ApplyOffState();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("PlayerWeapon"))
            return;

        Debug.Log("Switch hit! isActivated = " + isActivated + ", isToggle = " + isToggleSwitch);
        if (isToggleSwitch)
        {
            ToggleSwitch();
        }
        else
        {
            DeactivateSwitch(); // one way activation
        }
    }

    // Toggle switch
    private void ToggleSwitch()
    {
        isActivated = !isActivated;

        // Determine which sound to play
        PlaySound(isActivated ? "Activate" : "Deactivate");

        UpdateSprite();

        if (isActivated)
            ApplyOnState();
        else
            ApplyOffState();
    }

    // One way switch
    public void DeactivateSwitch()
    {
        if (!isActivated)
            return; // already off, prevent double trigger

        isActivated = false;

        PlaySound("Deactivate");
        UpdateSprite();

        ApplyOffState();

        if (AreAllSwitchesDeactivated())
            StartCoroutine(HandleBarrierSequence());
    }

    private bool AreAllSwitchesDeactivated()
    {
        foreach (var s in allSwitches)
        {
            if (s.isActivated)
                return false;
        }
        return true;
    }

    // Control platforms
    private void ApplyOnState()
    {
        // ON platforms respawn
        foreach (var p in platformsForOnState)
            p?.TriggerRespawn();

        // OFF platforms collapse
        foreach (var p in platformsForOffState)
            p?.TriggerCollapse();
    }

    private void ApplyOffState()
    {
        // OFF platforms respawn
        foreach (var p in platformsForOffState)
            p?.TriggerRespawn();

        // ON platforms collapse
        foreach (var p in platformsForOnState)
            p?.TriggerCollapse();
    }

    // Sprite and audio
    private void UpdateSprite()
    {
        if (spriteResolver == null) return;

        // Your existing categories/labels
        if (isActivated)
            spriteResolver.SetCategoryAndLabel("On", "tileset_325");
        else
            spriteResolver.SetCategoryAndLabel("Off", "tileset_393");
    }

    private void PlaySound(string activation)
    {
        if (audioSource == null) 
            return;

        if (activation == "Activate")
        {
            if (hitToActivate != null && hitToActivate.Length > 0)
            {
                int randomIndex = Random.Range(0, hitToActivate.Length);
                audioSource.PlayOneShot(hitToActivate[randomIndex]);
            }
        }
        else if (activation == "Deactivate")
        {
            if (hitToDeactivate != null && hitToDeactivate.Length > 0)
            {
                int randomIndex = Random.Range(0, hitToDeactivate.Length);
                audioSource.PlayOneShot(hitToDeactivate[randomIndex]);
            }
        }
    }

    // Barrier
    private IEnumerator HandleBarrierSequence()
    {
        GameFreezeManager.Instance?.FreezeGame();

        if (focusTarget != null && CameraFocusController.Instance != null)
        {
            CameraFocusController.Instance.FocusOnTarget(focusTarget);
            yield return new WaitForSecondsRealtime(1f);
        }

        barrier?.DeactivateBarrier();

        yield return new WaitForSecondsRealtime(1.5f);

        if (CameraFocusController.Instance != null)
            CameraFocusController.Instance.ReturnToPlayer();

        yield return new WaitForSecondsRealtime(0.5f);

        GameFreezeManager.Instance?.UnfreezeGame();
    }
}
