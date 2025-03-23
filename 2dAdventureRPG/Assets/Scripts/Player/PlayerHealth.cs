using UnityEngine;

[RequireComponent(typeof(CharacterStates))]
[RequireComponent(typeof(PlayerHealthUIManager))]
public class PlayerHealth : MonoBehaviour
{

    [SerializeField]private float currentHealth = 16.0f;
    [SerializeField]private float maxHealth = 16.0f;

    private PlayerHealthUIManager s_PlayerHealthUIManager;
    private CharacterStates characterStates;

    private void Awake()
    {
        characterStates = GetComponent<CharacterStates>();
        s_PlayerHealthUIManager = GetComponent<PlayerHealthUIManager>();
    }

    public void ChangeHealth(float changeAmmount)
    {
        currentHealth += changeAmmount;

        if(currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        s_PlayerHealthUIManager.updatedHealth = true;

        if (currentHealth <= 0)
        {
            characterStates.isDead = true;
        }
    }

    public float GetCurrentPlayerHealth()
    {
        return currentHealth;
    }
    public float GetMaxPlayerHealth()
    {
        return maxHealth;
    }
}
