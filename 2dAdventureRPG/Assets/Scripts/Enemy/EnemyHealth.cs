using DG.Tweening;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterStates))]
public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float currentHealth = 16.0f;
    [SerializeField] private float maxHealth = 16.0f;

    private CharacterStates characterStates;

    private EnemyHealthHeartsDisplayManager enemyHealthHeartsDisplayManager;

    private EnemyAudioManager enemyAudioManager;

    private void Awake()
    {
        characterStates = GetComponent<CharacterStates>();
        enemyHealthHeartsDisplayManager = GetComponent<EnemyHealthHeartsDisplayManager>();
        enemyAudioManager = GetComponent<EnemyAudioManager>();
    }

    public void ChangeHealth(float changeAmmount)
    {
        enemyHealthHeartsDisplayManager.damaged = true;
        currentHealth += changeAmmount;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        if(changeAmmount < 0)
        {
            enemyAudioManager.PlayHurtAudio();
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
