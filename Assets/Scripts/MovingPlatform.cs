using System.Collections;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    public Transform[] waypoints;
    public float speed = 2f;
    public float slowDownDistance = 0.5f;

    [Tooltip("If true, platform moves continuously between waypoints.")]
    public bool pingPong = false;

    [Header("Trigger Mode Settings")]
    public float activationDelay = 0.25f; // small delay before moving

    [Header("Activation Flash")]
    public DamageFlash damageFlash;
    public Color activatedFlashColor = Color.white;
    public float flashStrength = 0.8f;

    [Header("Ping Pong Settings")]
    public float pingPongWaitTime = 0.25f;

    private int currentWaypoint = 0;
    private int direction = 1;
    private bool moving = false;

    private void Start()
    {
        if (damageFlash == null)
            damageFlash = GetComponent<DamageFlash>();
    }

    private void FixedUpdate()
    {
        if (pingPong)
        {
            HandlePingPongMovement();
        }
        else if (moving)
        {
            HandleTriggerMovement();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        other.transform.SetParent(transform);

        if (!pingPong)
            TriggerPlatform();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            StartCoroutine(DelayedDetach(other.transform));
    }

    private IEnumerator DelayedDetach(Transform player)
    {
        yield return new WaitForSeconds(0.05f);

        // If another platform took over, they have a new parent
        if (player.parent != transform)
            yield break;

        player.SetParent(null);
    }

    // Trigger mode (When the player steps on the platform, activate movement)
    private void TriggerPlatform()
    {
        StopAllCoroutines();
        StartCoroutine(StartTriggerMovementDelayed());
    }

    private IEnumerator StartTriggerMovementDelayed()
    {
        // Flash immediately when stepped on
        if (damageFlash != null)
        {
            damageFlash.CallPersistentFlash(
                activatedFlashColor, 
                0.15f,   // fade-in duration
                flashStrength     // final intensity
            );
        }

        yield return new WaitForSeconds(activationDelay);

        // Toggle waypoint 0 to 1
        currentWaypoint = 1 - currentWaypoint;

        moving = true;
    }

    private void HandleTriggerMovement()
    {
        if (MoveTowardWaypoint(currentWaypoint))
        {
            moving = false;

            // Turn off flash when reaching destination
            if (damageFlash != null)
                damageFlash.ClearPersistentFlash();
        }
    }

    // Ping pong mode
    private void HandlePingPongMovement()
    {
        if (waypoints.Length < 2)
            return;

        if (MoveTowardWaypoint(currentWaypoint))
        {
            currentWaypoint += direction;

            if (currentWaypoint >= waypoints.Length || currentWaypoint < 0)
            {
                direction *= -1;
                currentWaypoint += direction * 2;
            }
        }
    }

    // Movement logic
    private bool MoveTowardWaypoint(int index)
    {
        Transform target = waypoints[index];
        float distance = Vector2.Distance(transform.position, target.position);

        float actualSpeed = speed;
        if (distance < slowDownDistance)
            actualSpeed *= Mathf.Clamp01(distance / slowDownDistance);

        transform.position = Vector2.MoveTowards(
            transform.position,
            target.position,
            actualSpeed * Time.deltaTime
        );

        return distance < 0.001f;
    }
}
