using UnityEngine;
using UnityEngine.InputSystem;

public class Shoot : MonoBehaviour
{
    public Bullet bulletPrefab;
    public Transform bulletSpawnPos;
    // public LineRenderer lineRenderer;   // Assign in Inspector
    public LayerMask wallLayer;         // What counts as a bounce surface
    public float maxLineLength = 8f; // total distance per segment
    public float chargeTime = 0.7f; // Seconds to fully charge
    public int normalShotBounces = 0;
    public int chargedShotBounces = 3;

    private Camera cam;
    private bool isCharging;
    private float chargeTimer;
    private bool isCharged;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        Vector2 start = bulletSpawnPos.position;
        Vector2 dir = Vector2.right;

        // lineRenderer.enabled = true;
        // lineRenderer.positionCount = 2;
        // lineRenderer.SetPosition(0, start);
        // lineRenderer.SetPosition(1, start + dir * 5f);
        
        if (isCharging)
        {
            chargeTimer += Time.deltaTime;
            if (chargeTimer >= chargeTime)
                isCharged = true;
            // DrawPredictionLine();
        }
        // else
        // {
        //     if (lineRenderer.enabled)
        //         lineRenderer.enabled = false;
        // }
    }

    public void OnShootPressed()
    {
        isCharging = true;
        chargeTimer = 0f;
        isCharged = false;
    }

    public void OnShootReleased()
    {
        isCharging = false;
        // lineRenderer.enabled = false;   // hide line
        FireBullet(isCharged);
    }

    private void FireBullet(bool charged)
    {
        Vector2 mouseWorld = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 dir = (mouseWorld - (Vector2)bulletSpawnPos.position).normalized;

        Bullet bullet = Instantiate(bulletPrefab, bulletSpawnPos.position, Quaternion.identity);
        bullet.shoot(dir);

        if (charged)
        {
            bullet.life = 3;          // can bounce
            bullet.damage *= 2.0f;    // double damage
        }
        else
        {
            bullet.life = 0;          // no bounce
        }
    }

    // private void DrawPredictionLine()
    // {
    //     Vector2 start = bulletSpawnPos.position;
    //     Vector2 mouseWorld = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    //     Vector2 dir = (mouseWorld - start).normalized;

    //     lineRenderer.enabled = true;
    //     lineRenderer.positionCount = 1;
    //     lineRenderer.SetPosition(0, start);

    //     for (int i = 0; i < chargedShotBounces; i++)
    //     {
    //         // limit how far each ray can travel
    //         RaycastHit2D hit = Physics2D.Raycast(start, dir, maxLineLength, wallLayer);

    //         if (hit.collider == null)
    //         {
    //             // no hit — extend to max length instead of based on mouse distance
    //             lineRenderer.positionCount++;
    //             lineRenderer.SetPosition(lineRenderer.positionCount - 1, start + dir * maxLineLength);
    //             break;
    //         }

    //         // hit detected
    //         lineRenderer.positionCount++;
    //         lineRenderer.SetPosition(lineRenderer.positionCount - 1, hit.point);

    //         // reflect + offset start
    //         dir = Vector2.Reflect(dir, hit.normal).normalized;
    //         start = hit.point + dir * 0.05f;
    //     }
    // }


}
