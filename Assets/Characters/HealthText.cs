using UnityEngine;
using TMPro;

public class HealthText : MonoBehaviour
{
    public float timeToLive = 0.5f;
    public float floatSpeed = 300;
    public Vector3 floatDirection = new Vector3(0, 1, 0);
    public TextMeshProUGUI textMesh;

    RectTransform rTransform;
    Color startingColor;

    float timeElapsed = 0.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startingColor = textMesh.color;
        rTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        timeElapsed += Time.deltaTime;

        rTransform.position += floatDirection * Time.deltaTime;

        textMesh.color = new Color(startingColor.r, startingColor.g, startingColor.b, 1 - (timeElapsed / timeToLive));

        if (timeElapsed > timeToLive)
        {
            Destroy(gameObject);
        }
    }
    
    public void SetDamageText(float damage)
    {
        textMesh.text = Mathf.RoundToInt(damage).ToString();
    }
}
