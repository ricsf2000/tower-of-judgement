using UnityEngine;

public class FollowTargetUI : MonoBehaviour
{
    [SerializeField] private Transform target; // the player or any object to follow
    [SerializeField] private Vector3 worldOffset = new Vector3(0, 0f, 0); // height above head
    private Camera mainCam;
    private RectTransform rectTransform;

    private void Awake()
    {
        mainCam = Camera.main;
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        if (target != null && mainCam != null)
        {
            Vector3 startScreenPos = mainCam.WorldToScreenPoint(target.position + worldOffset);
            rectTransform.position = startScreenPos;
        }
    }

    private void OnGUI()
    {
        if (target == null || mainCam == null)
            return;

        // Convert world position (player + offset) to screen position
        Vector3 screenPos = mainCam.WorldToScreenPoint(target.position + worldOffset);

        // Apply to UI element
        rectTransform.position = screenPos;

        // Optional: hide when off-screen or behind camera
        if (screenPos.z < 0)
            rectTransform.gameObject.SetActive(false);
        else
            rectTransform.gameObject.SetActive(true);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
