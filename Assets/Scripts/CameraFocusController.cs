using UnityEngine;
using Unity.Cinemachine;

public class CameraFocusController : MonoBehaviour
{
    public static CameraFocusController Instance;
    private CinemachineCamera vcam;
    private Transform player;

    void Awake()
    {
        Instance = this;
        vcam = GetComponent<CinemachineCamera>();
        
        // Find player more safely
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("[CameraFocusController] Player not found with tag 'Player'");
        }
        
        if (vcam == null)
        {
            Debug.LogError("[CameraFocusController] CinemachineCamera component not found!");
        }
    }

    public void FocusOnTarget(Transform target)
    {
        if (vcam == null)
        {
            Debug.LogError("[CameraFocusController] Cannot focus - vcam is null!");
            return;
        }
        
        if (target == null)
        {
            Debug.LogError("[CameraFocusController] Cannot focus - target is null!");
            return;
        }
        
        Debug.Log($"[CameraFocusController] Focusing camera on: {target.name}");
        vcam.Follow = target;
    }

    public void ReturnToPlayer()
    {
        if (vcam == null)
        {
            Debug.LogError("[CameraFocusController] Cannot return to player - vcam is null!");
            return;
        }
        
        if (player == null)
        {
            Debug.LogWarning("[CameraFocusController] Cannot return to player - player transform is null!");
            // Try to find player again
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                return;
            }
        }
        
        Debug.Log("[CameraFocusController] Returning camera to player.");
        vcam.Follow = player;
    }
}
