using UnityEngine;

[RequireComponent(typeof(CharacterStates))]
[RequireComponent(typeof(PlayerHealthUIManager))]
public class PlayerHealth : MonoBehaviour
{

    [SerializeField]private float currentHealth = 16.0f;
    [SerializeField]private float maxHealth = 16.0f;

    private CharacterStates characterStates;
    private PlayerHealthUIManager s_PlayerHealthUIManager;

    private MapGenerator mapGenerator;

    [SerializeField] private float timeBetweenSandDamage = 5.0f;
    private float nextSandDamageTime = 0.0f;

    private void Awake()
    {
        characterStates = GetComponent<CharacterStates>();
        s_PlayerHealthUIManager = GetComponent<PlayerHealthUIManager>();
    }

    private void Start()
    {
        //currentHealth = 1000000;
        //maxHealth = 10000000;

        mapGenerator = GameObject.FindGameObjectWithTag("MapTileGrid").GetComponent<MapGenerator>();
    }

    private void Update()
    {
        if(nextSandDamageTime <= Time.time)
        {
            Vector3Int playerTilePos = new Vector3Int((int)transform.position.x, (int)transform.position.y, 0);
            if (mapGenerator.IsTileSand(playerTilePos))
            {
                ChangeHealth(-1);

                nextSandDamageTime = Time.time + timeBetweenSandDamage;
            }
        }
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
