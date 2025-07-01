using System.Collections;
using UnityEngine;

[RequireComponent (typeof(PlayerProperties))]
[RequireComponent (typeof(PlayerHealth))]
public class PlayerPickupsHandler : MonoBehaviour
{
    public GameObject increasedSpeedIndicator;
    public GameObject increasedAttackDamageIndicator;

    private PlayerProperties s_PlayerProperties;
    private PlayerHealth s_PlayerHealth;

    private static int increasedAttackCount = 0;
    private static int increasedSpeedCount = 0;

    private void Awake()
    {
        s_PlayerProperties = GetComponent<PlayerProperties>();
        s_PlayerHealth = GetComponent<PlayerHealth>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        increasedAttackDamageIndicator.SetActive(increasedAttackCount > 0);
        increasedSpeedIndicator.SetActive(increasedSpeedCount > 0);
    }

    public void IncreasePlayerHealthByAmount(int amount)
    {
        s_PlayerHealth.ChangeHealth(amount);
    }

    public void IncreaseAttackDamageForSeconds(int increaseAmount, float duration)
    {
        s_PlayerProperties.attackDamageValue += increaseAmount;
        increasedAttackCount++;

        StartCoroutine(ReduceDamageByAmount(increaseAmount, duration));
    }

    IEnumerator ReduceDamageByAmount(int reduceByAmount, float duration)
    {
        yield return new WaitForSeconds(duration);

        Debug.Log("Reduced player attack damage amount.");

        increasedAttackCount--;
        s_PlayerProperties.attackDamageValue -= reduceByAmount;
    }

    public void IncreaseMovementSpeedForSeconds(int increaseAmount, float duration)
    {
        s_PlayerProperties.speed += increaseAmount;
        increasedSpeedCount++;

        StartCoroutine(ReduceMovementSpeedByAmount(increaseAmount, duration));
    }

    IEnumerator ReduceMovementSpeedByAmount(int reduceByAmount, float duration)
    {
        yield return new WaitForSeconds(duration);

        Debug.Log("Reduced player movement speed amount.");

        increasedSpeedCount--;
        s_PlayerProperties.speed -= reduceByAmount;
    }
}
