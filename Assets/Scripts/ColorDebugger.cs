using UnityEngine;
public class ColorDebugger : MonoBehaviour
{
    private SpriteRenderer sr;
    void Start() => sr = GetComponent<SpriteRenderer>();
    void Update() => Debug.Log($"Iris color: {sr.color}");
}