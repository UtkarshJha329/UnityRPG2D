using UnityEngine;

[RequireComponent(typeof(CharacterStates))]
[RequireComponent(typeof(PlayerHealthUIManager))]
public class PlayerHealth : MonoBehaviour
{

    [SerializeField]private float currentHealth = 16.0f;
    [SerializeField]private float maxHealth = 16.0f;

    private CharacterStates characterStates;
    private PlayerHealthUIManager s_PlayerHealthUIManager;

    private void Awake()
    {
        characterStates = GetComponent<CharacterStates>();
        s_PlayerHealthUIManager = GetComponent<PlayerHealthUIManager>();
    }

    private void Start()
    {
        currentHealth = 1000000;
        maxHealth = 10000000;
    }

    public bool ChangeHealth(float changeAmmount)
    {
        if(!characterStates.isKnockbacked)
        {
            currentHealth += changeAmmount;

            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }

            s_PlayerHealthUIManager.updatedHealth = true;

            if (currentHealth <= 0)
            {
                characterStates.isDead = true;
            }

            return true;
        }
        else
        {
            return false;
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
