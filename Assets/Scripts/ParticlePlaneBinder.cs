using UnityEngine;

public class ParticlePlaneBinder : MonoBehaviour
{
    void Start()
    {
        var ps = GetComponent<ParticleSystem>();
        if (ps == null) return;

        var collision = ps.collision;

        // find the plane in the scene
        var plane = GameObject.Find("Ground");
        if (plane != null)
        {
            collision.SetPlane(0, plane.transform);
        }
    }
}
