using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;

public class NodeGridGenerator : MonoBehaviour
{
    public Tilemap walkableTilemap;
    public Tilemap wallTilemap;
    public GameObject nodePrefab;

    private Dictionary<Vector3Int, Node> nodes = new();

    [ContextMenu("Generate Nodes")]
    public void Generate()
    {
        nodes.Clear();

        foreach (Vector3Int pos in walkableTilemap.cellBounds.allPositionsWithin)
        {
            if (walkableTilemap.HasTile(pos) && !wallTilemap.HasTile(pos))
            {
                Vector3 worldPos = walkableTilemap.GetCellCenterWorld(pos);
                Node node = Instantiate(nodePrefab, worldPos, Quaternion.identity, transform).GetComponent<Node>();
                nodes[pos] = node;
            }
        }

        // Connect 8-way
        Vector3Int[] directions = {
            new Vector3Int(1,0,0), new Vector3Int(-1,0,0),
            new Vector3Int(0,1,0), new Vector3Int(0,-1,0),
            new Vector3Int(1,1,0), new Vector3Int(-1,1,0),
            new Vector3Int(1,-1,0), new Vector3Int(-1,-1,0)
        };

        foreach (var kvp in nodes)
        {
            Node node = kvp.Value;
            node.connections = new List<Node>();
            foreach (var dir in directions)
            {
                // Skip diagonals that cut across corners
                if (Mathf.Abs(dir.x) == 1 && Mathf.Abs(dir.y) == 1)
                {
                    Vector3Int horizontal = kvp.Key + new Vector3Int(dir.x, 0, 0);
                    Vector3Int vertical   = kvp.Key + new Vector3Int(0, dir.y, 0);

                    // Only allow diagonal if both adjacent sides exist
                    if (!nodes.ContainsKey(horizontal) || !nodes.ContainsKey(vertical))
                        continue;
                }

                if (nodes.TryGetValue(kvp.Key + dir, out Node neighbor))
                    node.connections.Add(neighbor);
            }
        }

        
    }
}

