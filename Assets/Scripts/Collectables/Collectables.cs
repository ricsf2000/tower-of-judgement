using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Collectables : MonoBehaviour
{
    private ICollectableBehaviour collectableBehaviour;
    private void Awake()
    {
        collectableBehaviour = GetComponent<ICollectableBehaviour>();
    }
       private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            collectableBehaviour.OnCollect(player.gameObject);
            Destroy(gameObject);
        }
    }
}
