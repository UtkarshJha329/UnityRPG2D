using UnityEngine;

[RequireComponent(typeof(CharacterStates))]
public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float currentHealth = 16.0f;
    [SerializeField] private float maxHealth = 16.0f;

    private CharacterStates characterStates;

    private void Awake()
    {
        characterStates = GetComponent<CharacterStates>();
    }

    public void ChangeHealth(float changeAmmount)
    {
        currentHealth += changeAmmount;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        if (currentHealth <= 0)
        {
            characterStates.isDead = true;
        }
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }
}
