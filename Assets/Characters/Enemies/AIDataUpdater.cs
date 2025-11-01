using UnityEngine;

public class AIDataUpdater : MonoBehaviour
{
    private AIData aiData;
    private void Awake() => aiData = GetComponent<AIData>();
    private void Update() => aiData.position = transform.position;
}

