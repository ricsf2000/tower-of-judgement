using System.Collections;
using UnityEngine;

public class BossDoorSequence : MonoBehaviour
{
    [SerializeField] private Transform doorFocusTarget;
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string openTriggerName = "OpenDoor";
    [SerializeField] private float focusDelay = 0.35f;
    [SerializeField] private float postOpenDelay = 0.75f;
    [SerializeField] private bool autoReturnToPlayer = true;
    [SerializeField] private float returnToPlayerDelay = 1f;
    [SerializeField] private GameObject bossHealthBarRoot;
    [SerializeField] private Transform player;
    [SerializeField] private Transform cameraFocusAnchor;
    [SerializeField] private float cameraMoveDuration = 1.5f;
    [SerializeField] private AnimationCurve cameraMoveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private bool enforceDoorOpenDuration = true;
    [SerializeField] private float desiredDoorOpenDuration = 3f;
    [SerializeField] private AnimationClip doorOpeningClip;
    [SerializeField] private AudioClip doorSequenceClip;
    [SerializeField, Range(0f, 1f)] private float doorSequenceVolume = 0.75f;
    [SerializeField] private bool stopBossMusic = true;
    [SerializeField] private Collider2D whiteSpaceCollider;
    [SerializeField] private bool enableWhiteSpaceOnSequenceComplete = true;
    private AudioSource doorSequenceSource;
    [SerializeField] private bool destroyBossMusicPlayer = true;
    [SerializeField] private float musicFadeOutDuration = 1.25f;
    [SerializeField] private AnimationCurve musicFadeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    [SerializeField] private AudioSource bossMusicSource;

    private bool sequenceStarted;
    private bool musicCleanupStarted;
    private Coroutine doorSpeedResetRoutine;

    private void Awake()
    {
        if (whiteSpaceCollider != null)
            whiteSpaceCollider.enabled = false;
    }

    public void FocusDoorAndOpen()
    {
        if (sequenceStarted || CameraFocusController.Instance == null)
            return;

        StartCoroutine(RunSequence());
    }

    public void StopBossMusicEarly()
    {
        StopBossMusic();
    }

    private IEnumerator RunSequence()
    {
        sequenceStarted = true;

        if (bossHealthBarRoot != null && bossHealthBarRoot.activeSelf)
            bossHealthBarRoot.SetActive(false);

        StopBossMusic();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        if (cameraFocusAnchor != null)
        {
            if (player != null)
                cameraFocusAnchor.position = player.position;

            CameraFocusController.Instance.FocusOnTarget(cameraFocusAnchor);

            if (doorFocusTarget != null)
                yield return MoveCameraAnchor(cameraFocusAnchor, doorFocusTarget.position);
        }
        else if (doorFocusTarget != null)
        {
            CameraFocusController.Instance.FocusOnTarget(doorFocusTarget);
        }

        if (focusDelay > 0f)
            yield return new WaitForSeconds(focusDelay);

        float doorOpenDuration = GetDoorOpenDuration();

        if (doorAnimator != null && !string.IsNullOrEmpty(openTriggerName))
        {
            if (enforceDoorOpenDuration)
                ApplyDoorOpenDuration();

            doorAnimator.SetTrigger(openTriggerName);
        }

        PlayDoorSequenceAudio();

        if (doorOpenDuration > 0f)
            yield return new WaitForSeconds(doorOpenDuration);

        if (postOpenDelay > 0f)
            yield return new WaitForSeconds(postOpenDelay);

        if (autoReturnToPlayer)
            yield return ReturnCameraToPlayer();

        sequenceStarted = false;

        if (enableWhiteSpaceOnSequenceComplete && whiteSpaceCollider != null)
            whiteSpaceCollider.enabled = true;
    }

    private void StopBossMusic()
    {
        if (!stopBossMusic || musicCleanupStarted)
            return;

        musicCleanupStarted = true;

        if (musicFadeOutDuration <= 0f)
            StopBossMusicImmediate();
        else
            StartCoroutine(FadeOutBossMusic());
    }

    private void ApplyDoorOpenDuration()
    {
        if (doorAnimator == null || doorOpeningClip == null)
            return;

        float clipLength = Mathf.Max(doorOpeningClip.length, 0.01f);
        float targetDuration = Mathf.Max(desiredDoorOpenDuration, 0.01f);
        float targetSpeed = clipLength / targetDuration;

        doorAnimator.speed = targetSpeed;

        if (doorSpeedResetRoutine != null)
            StopCoroutine(doorSpeedResetRoutine);

        doorSpeedResetRoutine = StartCoroutine(ResetDoorAnimatorSpeed(targetDuration));
    }

    private IEnumerator ResetDoorAnimatorSpeed(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (doorAnimator != null)
            doorAnimator.speed = 1f;

        doorSpeedResetRoutine = null;
    }

    private IEnumerator ReturnCameraToPlayer()
    {
        if (returnToPlayerDelay > 0f)
            yield return new WaitForSeconds(returnToPlayerDelay);

        CameraFocusController.Instance.ReturnToPlayer();
    }

    private float GetDoorOpenDuration()
    {
        if (enforceDoorOpenDuration)
            return Mathf.Max(desiredDoorOpenDuration, 0f);

        if (doorOpeningClip != null)
            return Mathf.Max(doorOpeningClip.length, 0f);

        return 0f;
    }

    private void PlayDoorSequenceAudio()
    {
        if (doorSequenceClip == null)
            return;

        if (doorSequenceSource == null)
        {
            doorSequenceSource = gameObject.AddComponent<AudioSource>();
            doorSequenceSource.playOnAwake = false;
            doorSequenceSource.loop = false;
            doorSequenceSource.spatialBlend = 0f;
        }

        doorSequenceSource.clip = doorSequenceClip;
        doorSequenceSource.volume = doorSequenceVolume;
        doorSequenceSource.Stop();
        doorSequenceSource.Play();
    }

    private void StopBossMusicImmediate()
    {
        if (bossMusicSource == null && MusicManager.Instance != null)
            bossMusicSource = MusicManager.Instance.GetComponent<AudioSource>();

        if (bossMusicSource == null)
            return;

        bossMusicSource.volume = 0f;
        bossMusicSource.Stop();
        bossMusicSource.clip = null;

        if (destroyBossMusicPlayer)
            Destroy(bossMusicSource.gameObject);
    }

    private IEnumerator FadeOutBossMusic()
    {
        if (bossMusicSource == null && MusicManager.Instance != null)
            bossMusicSource = MusicManager.Instance.GetComponent<AudioSource>();

        if (bossMusicSource == null)
            yield break;

        float initialVolume = bossMusicSource.volume;
        float elapsed = 0f;

        while (elapsed < musicFadeOutDuration)
        {
            float normalizedTime = musicFadeOutDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / musicFadeOutDuration);
            float curvedT = musicFadeCurve != null ? musicFadeCurve.Evaluate(normalizedTime) : normalizedTime;
            bossMusicSource.volume = Mathf.Lerp(initialVolume, 0f, curvedT);
            elapsed += Time.deltaTime;
            yield return null;
        }

        bossMusicSource.volume = 0f;
        bossMusicSource.Stop();
        bossMusicSource.clip = null;

        if (destroyBossMusicPlayer && bossMusicSource != null)
            Destroy(bossMusicSource.gameObject);
    }

    private IEnumerator MoveCameraAnchor(Transform anchor, Vector3 destination)
    {
        if (cameraMoveDuration <= 0f)
        {
            anchor.position = destination;
            yield break;
        }

        Vector3 start = anchor.position;
        float elapsed = 0f;

        while (elapsed < cameraMoveDuration)
        {
            float normalizedTime = Mathf.Clamp01(elapsed / cameraMoveDuration);
            float curvedT = cameraMoveCurve != null ? cameraMoveCurve.Evaluate(normalizedTime) : normalizedTime;
            anchor.position = Vector3.Lerp(start, destination, curvedT);
            elapsed += Time.deltaTime;
            yield return null;
        }

        anchor.position = destination;
    }
}

