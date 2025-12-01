using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.Collections;

public class BossDefeatCutscene : MonoBehaviour
{
    [Header("Boss Reference")]
    [Tooltip("Reference to the boss GameObject (will be destroyed after death animation)")]
    public GameObject bossObject;

    [Header("Door Settings")]
    [Tooltip("The door transform to focus camera on")]
    public Transform doorTransform;
    [Tooltip("The door's animator component (must have OpenDoor trigger)")]
    public Animator doorAnimator;
    [Tooltip("Audio clip to play during door opening animation")]
    public AudioClip doorOpeningAudio;

    [Header("Player Settings")]
    [Tooltip("Position to respawn player in front of door")]
    public Transform playerSpawnPosition;
    [Tooltip("Target position for player to walk to (white space after door)")]
    public Transform walkTargetPosition;
    [Tooltip("Speed at which player walks during cutscene")]
    public float walkCutsceneSpeed = 0.5f;
    [Tooltip("Distance multiplier for walk (1.0 = full distance, 0.5 = half distance, etc.)")]
    [Range(0.1f, 1.0f)]
    public float walkDistanceMultiplier = 1.0f;

    [Header("Scene Transition")]
    [Tooltip("Name of the scene to load after the walk sequence")]
    public string nextSceneName;

    [Header("Boss Health Bar")]
    [Tooltip("Boss health bar to hide after defeat")]
    public BossHealthBar bossHealthBar;

    private bool cutsceneActive = false;

    // Called by BossDamageable after death animation completes
    public void StartBossDefeatCutscene(GameObject boss)
    {
        if (cutsceneActive)
        {
            Debug.LogWarning("[BossDefeatCutscene] Cutscene already active!");
            return;
        }

        bossObject = boss;
        cutsceneActive = true;
        
        // IMMEDIATELY hide sprites and disable animator synchronously (before coroutine starts)
        if (bossObject != null)
        {
            Animator bossAnimator = bossObject.GetComponent<Animator>();
            if (bossAnimator != null)
            {
                bossAnimator.enabled = false;
            }
            
            // Hide all sprite renderers immediately
            SpriteRenderer[] spriteRenderers = bossObject.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var sr in spriteRenderers)
            {
                if (sr != null)
                {
                    sr.enabled = false;
                }
            }
            
            // Disable the main "Sprite" GameObject if it exists
            Transform spriteTransform = bossObject.transform.Find("Sprite");
            if (spriteTransform != null)
            {
                spriteTransform.gameObject.SetActive(false);
            }
            
            Debug.Log("[BossDefeatCutscene] Boss sprites hidden and animator disabled synchronously before coroutine starts.");
        }
        
        StartCoroutine(BossDefeatSequence());
    }

    private IEnumerator BossDefeatSequence()
    {
        Debug.Log("[BossDefeatCutscene] Starting boss defeat cutscene sequence.");
        
        VisualElement healthBarElement = null;
        bool healthBarWasVisible = false;

        // IMMEDIATELY hide boss sprites and disable animator before anything else
        if (bossObject != null)
        {
            Animator bossAnimator = bossObject.GetComponent<Animator>();
            if (bossAnimator != null)
            {
                bossAnimator.enabled = false;
            }
            
            // Hide all sprite renderers immediately
            SpriteRenderer[] spriteRenderers = bossObject.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var sr in spriteRenderers)
            {
                if (sr != null)
                {
                    sr.enabled = false;
                }
            }
            
            // Disable the main "Sprite" GameObject if it exists
            Transform spriteTransform = bossObject.transform.Find("Sprite");
            if (spriteTransform != null)
            {
                spriteTransform.gameObject.SetActive(false);
            }
            
            Debug.Log("[BossDefeatCutscene] Boss sprites hidden and animator disabled at start of sequence.");
        }

        // Hide boss health bar immediately
        if (bossHealthBar != null)
        {
            if (bossHealthBar.barRoot != null)
            {
                bossHealthBar.barRoot.SetActive(false);
            }
            bossHealthBar.enabled = false;
        }

        // Get player reference
        PlayerController player = FindFirstObjectByType<PlayerController>();
        
        // Disable player input and movement
        if (player != null)
        {
            player.canMove = false;
            player.canAttack = false;
            player.canDash = false;
            player.canShoot = false;
            player.Rb.linearVelocity = Vector2.zero;
            player.Animator.SetBool("isMoving", false);
            
            var playerInput = FindFirstObjectByType<UnityEngine.InputSystem.PlayerInput>();
            if (playerInput != null)
                playerInput.enabled = false;
            
            var playerColliders = player.GetComponentsInChildren<Collider2D>();
            foreach (var col in playerColliders)
            {
                col.enabled = false;
            }
        }

        // Freeze other rigidbodies but not the player's
        if (GameFreezeManager.Instance != null)
        {
            foreach (var rb in UnityEngine.Object.FindObjectsOfType<Rigidbody2D>())
            {
                if (rb != player?.Rb)
                {
                    rb.simulated = false;
                }
            }
        }

        // Ensure animator is disabled and sprites are hidden before destroying boss
        if (bossObject != null)
        {
            Animator bossAnimator = bossObject.GetComponent<Animator>();
            if (bossAnimator != null)
            {
                // Stop animator completely - don't reset trigger to avoid re-triggering
                bossAnimator.enabled = false;
                Debug.Log("[BossDefeatCutscene] Disabled boss animator before destruction.");
            }
            
            // Hide all sprite renderers before destroying
            SpriteRenderer[] spriteRenderers = bossObject.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var sr in spriteRenderers)
            {
                if (sr != null)
                {
                    sr.enabled = false;
                }
            }
            
            // Disable the main "Sprite" GameObject if it exists
            Transform spriteTransform = bossObject.transform.Find("Sprite");
            if (spriteTransform != null)
            {
                spriteTransform.gameObject.SetActive(false);
            }
            
            // Wait a frame to ensure sprites are hidden
            yield return null;
            
            // Double-check sprites are still hidden
            foreach (var sr in spriteRenderers)
            {
                if (sr != null && sr.enabled)
                {
                    sr.enabled = false;
                }
            }
            
            if (spriteTransform != null && spriteTransform.gameObject.activeInHierarchy)
            {
                spriteTransform.gameObject.SetActive(false);
            }
            
            Debug.Log("[BossDefeatCutscene] Destroying boss object.");
            Destroy(bossObject);
            bossObject = null;
        }

        // Wait a moment for boss to be fully destroyed
        yield return new WaitForSecondsRealtime(0.3f);

        // Move camera to closed door
        if (doorTransform != null)
        {
            Debug.Log("[BossDefeatCutscene] Moving camera to door...");
            
            if (CameraFocusController.Instance != null)
            {
                CameraFocusController.Instance.FocusOnTarget(doorTransform);
                yield return new WaitForSecondsRealtime(1.5f);
            }
            else
            {
                var cinemachineCamera = FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();
                if (cinemachineCamera != null)
                {
                    cinemachineCamera.Follow = doorTransform;
                    yield return new WaitForSecondsRealtime(1.5f);
                }
                else
                {
                    Debug.LogError("[BossDefeatCutscene] Cannot find Cinemachine camera!");
                    yield return new WaitForSecondsRealtime(1f);
                }
            }
        }
        else
        {
            Debug.LogError("[BossDefeatCutscene] Door transform is NULL!");
            yield return new WaitForSecondsRealtime(1f);
        }

        // Wait 1 second after camera finishes moving before starting door opening
        Debug.Log("[BossDefeatCutscene] Camera movement finished, waiting 1 second before door opening...");
        yield return new WaitForSecondsRealtime(1f);

        // Hide player health bar during door opening and walking sequences
        UIHandler uiHandler = FindFirstObjectByType<UIHandler>();
        if (uiHandler != null)
        {
            var root = uiHandler.uiDocument.rootVisualElement;
            healthBarElement = root.Q<VisualElement>("HealthBar");
            if (healthBarElement != null)
            {
                healthBarWasVisible = healthBarElement.visible;
                healthBarElement.style.display = DisplayStyle.None;
                Debug.Log("[BossDefeatCutscene] Player health bar hidden.");
            }
        }

        // Play door opening animation
        if (doorAnimator != null)
        {
            doorAnimator.enabled = true;
            doorAnimator.ResetTrigger("OpenDoor");
            
            float targetDuration = 4f;
            AnimatorStateInfo stateInfo = doorAnimator.GetCurrentAnimatorStateInfo(0);
            
            doorAnimator.SetTrigger("OpenDoor");
            yield return null;
            
            // Wait for transition to DoorOpening state
            float timeout = 0.5f;
            float elapsed = 0f;
            while (elapsed < timeout)
            {
                stateInfo = doorAnimator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.IsName("DoorOpening"))
                {
                    break;
                }
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Get AudioSource for door opening audio
            AudioSource doorAudioSource = null;
            if (doorOpeningAudio != null)
            {
                doorAudioSource = doorAnimator.GetComponent<AudioSource>();
                if (doorAudioSource == null && doorTransform != null)
                {
                    doorAudioSource = doorTransform.GetComponent<AudioSource>();
                }
                
                if (doorAudioSource == null && doorTransform != null)
                {
                    doorAudioSource = doorTransform.gameObject.AddComponent<AudioSource>();
                    doorAudioSource.spatialBlend = 0f;
                    doorAudioSource.playOnAwake = false;
                }
                
                // Wait 0.5 seconds before playing audio
                yield return new WaitForSecondsRealtime(0.5f);
                
                // Start playing audio when entering DoorOpening state
                if (doorAudioSource != null)
                {
                    doorAudioSource.clip = doorOpeningAudio;
                    doorAudioSource.volume = 0.5f;
                    doorAudioSource.loop = false;
                    doorAudioSource.Play();
                }
            }
            
            // Get animation clip length
            AnimationClip[] clips = doorAnimator.runtimeAnimatorController.animationClips;
            float originalLength = 1f;
            foreach (var clip in clips)
            {
                if (clip.name == "DoorOpening")
                {
                    originalLength = clip.length;
                    break;
                }
            }
            
            // Calculate speed to make animation take 4 seconds
            float animatorSpeed = originalLength / targetDuration;
            doorAnimator.speed = animatorSpeed;
            
            // Monitor animator state and stop audio when transitioning to DoorOpen
            float animationElapsed = 0f;
            while (animationElapsed < targetDuration)
            {
                stateInfo = doorAnimator.GetCurrentAnimatorStateInfo(0);
                
                if (stateInfo.IsName("DoorOpen"))
                {
                    if (doorAudioSource != null && doorAudioSource.isPlaying)
                    {
                        doorAudioSource.Stop();
                    }
                    break;
                }
                
                animationElapsed += Time.deltaTime;
                yield return null;
            }
            
            // Ensure audio is stopped
            if (doorAudioSource != null && doorAudioSource.isPlaying)
            {
                doorAudioSource.Stop();
            }
            
            doorAnimator.speed = 1f;
        }
        else
        {
            Debug.LogWarning("[BossDefeatCutscene] Door animator not assigned!");
            yield return new WaitForSecondsRealtime(1f);
        }

        // Fade to black screen
        if (FadeController.Instance != null)
        {
            Debug.Log("[BossDefeatCutscene] Fading to black...");
            yield return FadeController.Instance.FadeIn(0.5f);
            yield return new WaitForSecondsRealtime(2f);
        }
        else
        {
            yield return new WaitForSecondsRealtime(2f);
        }

        // Respawn player in front of door
        if (player != null && playerSpawnPosition != null)
        {
            Debug.Log("[BossDefeatCutscene] Respawning player in front of door...");
            
            var playerColliders = player.GetComponentsInChildren<Collider2D>();
            foreach (var col in playerColliders)
            {
                col.enabled = true;
            }
            
            player.Rb.position = playerSpawnPosition.position;
            player.Rb.linearVelocity = Vector2.zero;
            
            // Calculate walking direction
            if (walkTargetPosition != null)
            {
                Vector2 walkDirection = (walkTargetPosition.position - playerSpawnPosition.position).normalized;
                
                player.Animator.SetFloat("moveX", walkDirection.x);
                player.Animator.SetFloat("moveY", walkDirection.y);
                player.Animator.SetFloat("lastMoveX", walkDirection.x);
                player.Animator.SetFloat("lastMoveY", walkDirection.y);
            }
            
            player.Animator.SetBool("isMoving", false);
            
            // Return camera to player
            if (CameraFocusController.Instance != null)
            {
                CameraFocusController.Instance.ReturnToPlayer();
            }
            else
            {
                var cinemachineCamera = FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();
                if (cinemachineCamera != null)
                {
                    cinemachineCamera.Follow = player.transform;
                }
            }
        }

        // Fade out from black
        if (FadeController.Instance != null)
        {
            yield return FadeController.Instance.FadeOut(0.5f);
        }

        // Start player walk cutscene
        if (player != null && walkTargetPosition != null)
        {
            Vector3 targetPos = walkTargetPosition.position;
            yield return StartCoroutine(PlayerWalkCutscene(player, targetPos));
        }

        // Set flag to show health bar when next scene loads
        if (healthBarWasVisible)
        {
            FadeController.shouldShowHealthBarOnNextScene = true;
            Debug.Log("[BossDefeatCutscene] Flag set to show health bar in next scene.");
        }

        // Transition to next scene
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log($"[BossDefeatCutscene] Loading next scene: {nextSceneName}");
            
            // Verify scene exists in build settings
            bool sceneExists = false;
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                if (sceneName == nextSceneName)
                {
                    sceneExists = true;
                    break;
                }
            }
            
            if (sceneExists)
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.LogError($"[BossDefeatCutscene] Scene '{nextSceneName}' not found in build settings!");
            }
        }
    }

    private IEnumerator PlayerWalkCutscene(PlayerController player, Vector3 targetPosition)
    {
        Debug.Log("[BossDefeatCutscene] Starting player walk cutscene.");

        player.canMove = false;
        Rigidbody2D playerRb = player.Rb;
        Animator playerAnim = player.Animator;
        
        if (playerRb != null)
        {
            playerRb.simulated = true;
        }

        var playerColliders = player.GetComponentsInChildren<Collider2D>();
        foreach (var col in playerColliders)
        {
            col.enabled = false;
        }

        Vector2 startPos = playerRb.position;
        Vector2 originalTargetPos = targetPosition;
        
        Vector2 direction = (originalTargetPos - startPos).normalized;
        float fullDistance = Vector2.Distance(startPos, originalTargetPos);
        float adjustedDistance = fullDistance * walkDistanceMultiplier;
        Vector2 targetPos = startPos + (direction * adjustedDistance);

        // Set animator direction immediately
        playerAnim.SetFloat("moveX", direction.x);
        playerAnim.SetFloat("moveY", direction.y);
        playerAnim.SetFloat("lastMoveX", direction.x);
        playerAnim.SetFloat("lastMoveY", direction.y);
        playerAnim.SetBool("isMoving", true);
        
        yield return null;

        float distance = Vector2.Distance(startPos, targetPos);
        float elapsedTime = 0f;
        float duration = distance / walkCutsceneSpeed;

        // Get camera for zoom effect
        Unity.Cinemachine.CinemachineCamera cinemachineCamera = null;
        Camera mainCamera = Camera.main;
        float originalOrthoSize = 0f;
        bool useCinemachine = false;
        
        if (CameraFocusController.Instance != null)
        {
            cinemachineCamera = CameraFocusController.Instance.GetComponent<Unity.Cinemachine.CinemachineCamera>();
        }
        
        if (cinemachineCamera == null)
        {
            cinemachineCamera = FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();
        }
        
        if (cinemachineCamera != null)
        {
            originalOrthoSize = cinemachineCamera.Lens.OrthographicSize;
            useCinemachine = true;
        }
        else if (mainCamera != null)
        {
            originalOrthoSize = mainCamera.orthographicSize;
        }

        // Walk towards target with camera zoom
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            
            playerRb.position = Vector2.Lerp(startPos, targetPos, t);
            
            // Zoom camera in
            if (useCinemachine && cinemachineCamera != null)
            {
                float zoomTarget = originalOrthoSize * 0.5f;
                float currentSize = cinemachineCamera.Lens.OrthographicSize;
                cinemachineCamera.Lens.OrthographicSize = Mathf.Lerp(currentSize, zoomTarget, Time.deltaTime * 2f);
            }
            else if (mainCamera != null)
            {
                float zoomTarget = originalOrthoSize * 0.5f;
                mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, zoomTarget, Time.deltaTime * 2f);
            }
            
            yield return null;
        }

        // Ensure player reaches exact target
        playerRb.position = targetPos;
        playerAnim.SetBool("isMoving", false);

        // Set player to face the direction they walked
        playerAnim.SetFloat("moveX", direction.x);
        playerAnim.SetFloat("moveY", direction.y);
        playerAnim.SetFloat("lastMoveX", direction.x);
        playerAnim.SetFloat("lastMoveY", direction.y);
        
        player.canMove = false;

        Debug.Log($"[BossDefeatCutscene] Player walk cutscene finished.");
    }
}

