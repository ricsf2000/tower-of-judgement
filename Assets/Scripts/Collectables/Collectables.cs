using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Collectables : MonoBehaviour
{
    private ICollectableBehaviour collectableBehaviour;

    [SerializeField] private float pickupDelay = .25f;
    private bool canPickUp = false;

    private void Awake()
    {
        collectableBehaviour = GetComponent<ICollectableBehaviour>();
    }

    private void Start()
    {
        StartCoroutine(EnablePickupAfterDelay());
    }

    private IEnumerator EnablePickupAfterDelay()
    {
        yield return new WaitForSeconds(pickupDelay);
        canPickUp = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryPickup(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryPickup(other);
    }

    private void TryPickup(Collider2D other)
    {
        if (!canPickUp) return;

        var player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            collectableBehaviour.OnCollect(player.gameObject);
        }
    }
    
}
