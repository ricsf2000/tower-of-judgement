using UnityEngine;

public class ColorDebugger : MonoBehaviour
{
    private SpriteRenderer sr;
    private Color lastColor;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        lastColor = sr.color;
        Debug.Log($"[ColorDebugger] Starting color: {lastColor}");
    }

    void LateUpdate()
    {
        if (sr.color != lastColor)
        {
            Debug.Log($"[ColorDebugger] Color changed from {lastColor} → {sr.color} at frame {Time.frameCount}");
            Debug.Log($"[ColorDebugger] Color set by: {StackTraceUtility.ExtractStackTrace()}");
            lastColor = sr.color;
        }
    }
}
