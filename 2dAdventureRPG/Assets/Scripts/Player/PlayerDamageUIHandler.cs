using System.Collections;
using UnityEngine;

[RequireComponent (typeof(PlayerHealth))]
[RequireComponent (typeof(CharacterStates))]
[RequireComponent (typeof(PlayerProperties))]
public class PlayerDamageUIHandler : MonoBehaviour
{
    private CharacterStates characterStates;
    private PlayerHealth s_playerHealth;
    private PlayerProperties s_PlayerProperties;

    private DamageNumbersUIHandler damageNumbersUIHandler;

    private float lastPlayerHealth = 0.0f;

    private bool displayedCurrentDamageText = false;

    private void Awake()
    {
        characterStates = GetComponent<CharacterStates>();
        s_playerHealth = GetComponent<PlayerHealth>();
        s_PlayerProperties = GetComponent<PlayerProperties>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        damageNumbersUIHandler = GameObject.FindGameObjectWithTag("DamageTextUIHandler").GetComponent<DamageNumbersUIHandler>();
        lastPlayerHealth = s_playerHealth.GetCurrentPlayerHealth();
    }

    // Update is called once per frame
    void Update()
    {
        if (characterStates.isKnockbacked && !displayedCurrentDamageText)
        {
            damageNumbersUIHandler.ShowDamageText(transform, (int)(lastPlayerHealth - s_playerHealth.GetCurrentPlayerHealth()), s_PlayerProperties.damageTextTime);
            displayedCurrentDamageText = true;
            StartCoroutine(ResetDisplayedCurrentDamageText());
        }
        lastPlayerHealth = s_playerHealth.GetCurrentPlayerHealth();
    }

    IEnumerator ResetDisplayedCurrentDamageText()
    {
        yield return new WaitForSeconds(s_PlayerProperties.knockbackTime);

        displayedCurrentDamageText = false;
    }
}
