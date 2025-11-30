using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyDamageable))]
public class PersistentEnemy : MonoBehaviour
{
    [SerializeField] private string persistenceID;
    [SerializeField] private bool deactivateInsteadOfDestroy = false;

    private EnemyDamageable damageable;
    private bool hasRegisteredDeath;

    public string PersistenceID => persistenceID;
    public bool HasRegisteredDeath => hasRegisteredDeath;

    private void Awake()
    {
        damageable = GetComponent<EnemyDamageable>();

        if (string.IsNullOrEmpty(persistenceID))
        {
            Debug.LogWarning($"[PersistentEnemy] {name} is missing a persistence ID. Assign a unique value in the Inspector.");
        }
    }

    private void OnEnable()
    {
        DamageableCharacter.OnAnyCharacterDeath += HandleDeath;
    }

    private void OnDisable()
    {
        DamageableCharacter.OnAnyCharacterDeath -= HandleDeath;
    }

    private void HandleDeath(DamageableCharacter character)
    {
        if (character != damageable || hasRegisteredDeath || string.IsNullOrEmpty(persistenceID))
            return;

        hasRegisteredDeath = true;
        PersistentEnemyRuntime.MarkKilled(SceneManager.GetActiveScene().name, persistenceID);
    }

    public void ApplySavedState(bool shouldBeDead)
    {
        if (!shouldBeDead)
            return;

        hasRegisteredDeath = true;

        if (deactivateInsteadOfDestroy)
        {
            gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}