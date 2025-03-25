using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(CharacterStates))]
[RequireComponent(typeof(EnemyProperties))]
public class EnemyDamageUIHandler : MonoBehaviour
{
    private CharacterStates characterStates;
    private EnemyHealth s_EnemyHealth;
    private EnemyProperties s_EnemyProperties;

    private DamageNumbersUIHandler damageNumbersUIHandler;

    private float lastEnemyHealth = 0.0f;

    private bool displayedCurrentDamageText = false;

    private void Awake()
    {
        characterStates = GetComponent<CharacterStates>();
        s_EnemyHealth = GetComponent<EnemyHealth>();
        s_EnemyProperties = GetComponent<EnemyProperties>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        damageNumbersUIHandler = GameObject.FindGameObjectWithTag("DamageTextUIHandler").GetComponent<DamageNumbersUIHandler>();
        lastEnemyHealth = s_EnemyHealth.GetCurrentHealth();
    }

    // Update is called once per frame
    void Update()
    {
        if (characterStates.isKnockbacked && !displayedCurrentDamageText)
        {
            damageNumbersUIHandler.ShowDamageText(transform, (int)(lastEnemyHealth - s_EnemyHealth.GetCurrentHealth()), s_EnemyProperties.personalDamageTextTime);
            displayedCurrentDamageText = true;
            StartCoroutine(ResetDisplayedCurrentDamageText());
        }
        lastEnemyHealth = s_EnemyHealth.GetCurrentHealth();
    }

    IEnumerator ResetDisplayedCurrentDamageText()
    {
        yield return new WaitForSeconds(s_EnemyProperties.personalKnockbackTime);

        displayedCurrentDamageText = false;
    }
}
