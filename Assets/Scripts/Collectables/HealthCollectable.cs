using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class HealthCollectable : MonoBehaviour, ICollectableBehaviour
{
    [SerializeField]
    private float healthRestoreAmount;

    public void OnCollect(GameObject player)
    {
        player.GetComponent<PlayerDamageable>().RestoreHealth(healthRestoreAmount);
    }
}
