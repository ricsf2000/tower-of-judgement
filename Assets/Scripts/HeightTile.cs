using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Tiles/Height Tile")]
public class HeightTile : Tile
{
    [Header("Height Settings")]
    [Tooltip("The elevation level of this tile (0 = ground, 1 = platform, etc.)")]
    public int heightLevel = 0;
}
