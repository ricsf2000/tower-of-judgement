using UnityEngine;
using UnityEngine.U2D.Animation;
using System.Collections.Generic;
using System.Collections;

public class SwitchController : MonoBehaviour
{
    [Header("Group Settings")]
    [Tooltip("Add all switches in this puzzle group (including this one).")]
    public List<SwitchController> allSwitches; // assign all 3 switches in Inspector
    public BarrierController barrier;          // barrier to disable when done

    [Header("Camera Focus")]
    public Transform focusTarget;              // usually barrier.transform

    [Header("Sprite Resolver")]
    public SpriteResolver spriteResolver;

    private bool isActivated = false;

    private void Awake()
    {
        if (spriteResolver == null)
            spriteResolver = GetComponent<SpriteResolver>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isActivated) return;
        if (other.CompareTag("PlayerWeapon"))
        {
            DeactivateSwitch();
        }
    }

    public void DeactivateSwitch()
    {
        isActivated = true;
        spriteResolver?.SetCategoryAndLabel("Off", "tileset_393");

        // Check if all switches in group are activated
        if (AreAllSwitchesActivated())
        {
            StartCoroutine(HandleBarrierSequence());
        }
    }

    private bool AreAllSwitchesActivated()
    {
        foreach (var s in allSwitches)
        {
            if (!s.isActivated)
                return false;
        }
        return true;
    }

    private IEnumerator HandleBarrierSequence()
    {
        // Focus camera on the barrier before it disappears
        if (focusTarget != null && CameraFocusController.Instance != null)
        {
            CameraFocusController.Instance.FocusOnTarget(focusTarget);
            yield return new WaitForSeconds(1f); // short delay for camera pan
        }

        barrier?.DeactivateBarrier();

        // Wait for fade-out
        yield return new WaitForSeconds(1.5f);

        // Return camera to player
        if (CameraFocusController.Instance != null)
            CameraFocusController.Instance.ReturnToPlayer();
    }
}
