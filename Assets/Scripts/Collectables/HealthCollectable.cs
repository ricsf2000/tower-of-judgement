using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class HealthCollectable : MonoBehaviour, ICollectableBehaviour
{
    [SerializeField]
    private float healthRestoreAmount;

    // Percent of max HP to heal
    [SerializeField, Range(0f, 1f)]
    private float healPercent = 1f / 6f;   // Heals EXACTLY one UI square


    public void OnCollect(GameObject player)
    {
        var dmg = player.GetComponent<PlayerDamageable>();
        if (dmg == null) return;

        // Prevent pickup if already at full health
        if (dmg.IsFullHealth)
            return;

        dmg.RestoreHealthPercent(healPercent);

        // When health item is picked up, destroy it
        Destroy(gameObject);
    }
}
