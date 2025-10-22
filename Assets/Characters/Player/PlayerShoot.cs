using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    private Shoot shoot;
    private bool canShoot = true;

    void Start()
    {
        shoot = GetComponent<Shoot>();
    }

    public void HandleShootInput(InputValue value)
    {
        if (!canShoot || shoot == null) return;

        if (value.isPressed)
            shoot.OnShootPressed();
        else
            shoot.OnShootReleased();
    }
}
