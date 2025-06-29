using System.Collections;
using TMPro;
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

    //private bool displayingHealth = false;
    //private TextMeshProUGUI healthDisplayTextComponent;
    //private bool deletedTextObject = false;

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
            int enemyHealthInt = (int)s_EnemyHealth.GetCurrentHealth();
            damageNumbersUIHandler.ShowDamageText(transform, (int)(lastEnemyHealth - enemyHealthInt), s_EnemyProperties.personalDamageTextTime);
            displayedCurrentDamageText = true;

            //if (!displayingHealth)
            //{
            //    healthDisplayTextComponent = damageNumbersUIHandler.ShowEnemyHealth(transform, enemyHealthInt);
            //    displayingHealth = true;
            //}

            //healthDisplayTextComponent.text = enemyHealthInt.ToString();

            StartCoroutine(ResetDisplayedCurrentDamageText());
        }
        lastEnemyHealth = s_EnemyHealth.GetCurrentHealth();

        //if (s_EnemyHealth.GetCurrentHealth() <= 0 && !deletedTextObject)
        //{
        //    deletedTextObject = true;
        //    damageNumbersUIHandler.RemoveEnemyFromUIHealthList(s_EnemyProperties.transform);
        //}
    }

    IEnumerator ResetDisplayedCurrentDamageText()
    {
        yield return new WaitForSeconds(s_EnemyProperties.personalKnockbackTime);

        displayedCurrentDamageText = false;
    }
}
