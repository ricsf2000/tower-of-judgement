using UnityEngine;

public class BossHealthBar : MonoBehaviour
{
    public Transform fillBar;               // yellow bar sprite
    public DamageableCharacter bossDamage; // reference to boss's health component
    public MonoBehaviour bossAI;
    public GameObject barRoot;       // Parent object of background/fill/border

    private Vector3 originalScale;

    void Start()
    {
        originalScale = fillBar.localScale;
    }

    void Update()
    {
        // If boss object is destroyed then hide bar
        if (bossAI == null)
        {
            barRoot.SetActive(false);
            return; 
        }

        // If boss object exists, show/hide based on AI enabled state
        barRoot.SetActive(bossAI.enabled);
        
        UpdateBar();
    }

    private void UpdateBar()
    {
        float t = Mathf.Clamp01(bossDamage.Health / bossDamage.maxHealth);

        fillBar.localScale = new Vector3(
            originalScale.x * t,
            originalScale.y,
            originalScale.z
        );
    }
}
