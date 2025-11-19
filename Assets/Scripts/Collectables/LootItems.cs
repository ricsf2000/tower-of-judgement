using UnityEngine;

[System.Serializable]
public class LootItems
{
    public GameObject itemPrefab;
    [Range(0,100)] public float dropChance;
}
