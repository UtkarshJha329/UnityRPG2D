using UnityEngine;

[RequireComponent(typeof(CharacterStates))]
[RequireComponent(typeof(PlayerHealthUIManager))]
public class PlayerHealth : MonoBehaviour
{

    [SerializeField]private float currentHealth = 16.0f;
    [SerializeField]private float maxHealth = 16.0f;

    private CharacterStates characterStates;
    private PlayerHealthUIManager s_PlayerHealthUIManager;
    private PlayerAudioManager s_PlayerAudioManager;

    private void Awake()
    {
        characterStates = GetComponent<CharacterStates>();
        s_PlayerHealthUIManager = GetComponent<PlayerHealthUIManager>();

        s_PlayerAudioManager = GetComponent<PlayerAudioManager>();
    }

    private void Start()
    {
        //currentHealth = 1000000;
        //maxHealth = 10000000;
    }

    private void Update()
    {
    }

    public bool IncreasePlayerHealthOnlyUntilNextFullHeart()
    {
        float fullHeartTotalHealth = 4;
        float newHealth = currentHealth + fullHeartTotalHealth;
        float remainder = newHealth % fullHeartTotalHealth;
        float changeHealthBy = fullHeartTotalHealth - remainder;

        return ChangeHealth(changeHealthBy);
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

            if(changeAmmount < 0.0f)
            {
                s_PlayerAudioManager.PlayPlayerHurtSfx();
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    public void IncreaseMaxHealth(int increaseByAmount)
    {
        maxHealth += increaseByAmount;
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
