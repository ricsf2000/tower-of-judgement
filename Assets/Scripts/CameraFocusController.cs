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
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public void FocusOnTarget(Transform target)
    {
        vcam.Follow = target;
    }

    public void ReturnToPlayer()
    {
        vcam.Follow = player;
    }
}
