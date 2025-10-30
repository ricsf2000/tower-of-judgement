using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIData : MonoBehaviour
{
    public List<Transform> targets = null;
    public Collider2D[] obstacles = null;

    public Transform currentTarget;
    public Vector2? currentWaypoint = null;

    public Vector2 position;

    public Vector2 lastKnownTargetPosition;
    public float lastSeenTimer;
    public float memoryDuration = 2.5f; // seconds to keep chasing after losing sight

    public int GetTargetsCount() => targets == null ? 0 : targets.Count;
}