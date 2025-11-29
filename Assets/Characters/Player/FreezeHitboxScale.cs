using UnityEngine;

public class FreezeHitboxScale : MonoBehaviour
{
    void LateUpdate()
    {
        transform.localScale = Vector3.one;
    }
}
