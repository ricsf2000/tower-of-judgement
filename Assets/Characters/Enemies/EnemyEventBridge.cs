using UnityEngine;

public class EnemyEventBridge : MonoBehaviour
{
    public Power power;

    public void OnMove(Vector2 dir)
    {
        if (power) power.Move(dir);
    }

    public void OnLook(Vector2 pointer)
    {
        if (power) power.LookAt(pointer);
    }

    public void OnAttack()
    {
        if (power) power.Attack();
    }
}
