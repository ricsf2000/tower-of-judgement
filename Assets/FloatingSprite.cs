using UnityEngine;

public class FloatingSprite : MonoBehaviour
{
    [Header("Floating Animation")]
    [SerializeField] private float floatAmplitude = 0.3f;
    [SerializeField] private float floatSpeed = 2f;
    
    [Header("Shadow")]
    [SerializeField] private Transform shadowTransform;
    [SerializeField] private float shadowScaleMin = 0.7f;
    [SerializeField] private float shadowScaleMax = 1f;
    
    private Vector3 startLocalPosition;
    private Vector3 shadowStartScale;
    
    void Start()
    {
        startLocalPosition = transform.localPosition;
        
        if (shadowTransform != null)
        {
            shadowStartScale = shadowTransform.localScale;
        }
    }
    
    void Update()
    {
        // Calculate the sine wave value
        float sineWave = Mathf.Sin(Time.time * floatSpeed);
        
        // Floating motion
        float newY = startLocalPosition.y + sineWave * floatAmplitude;
        transform.localPosition = new Vector3(startLocalPosition.x, newY, startLocalPosition.z);
        
        // Scale shadow based on feather height
        if (shadowTransform != null)
        {
            float normalizedHeight = (sineWave + 1f) / 2f;
            float scaleMultiplier = Mathf.Lerp(shadowScaleMax, shadowScaleMin, normalizedHeight);
            shadowTransform.localScale = shadowStartScale * scaleMultiplier;
        }
    }
}