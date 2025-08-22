using UnityEngine;

[RequireComponent(typeof(CharacterStates))]
[RequireComponent(typeof(EnemyProperties))]
[RequireComponent(typeof(EnemyDropManager))]
public class EnemyDeath : MonoBehaviour
{
    private CharacterStates characterStates;

    private EnemyProperties s_EnemyProperties;
    private EnemyDropManager dropManager;

    public GameObject deathObject;

    private bool droppedItem = false;

    private Transform playerTransform;

    private GameStats s_GameStats;

    private void Awake()
    {
        characterStates = GetComponent<CharacterStates>();
        s_EnemyProperties = GetComponent<EnemyProperties>();
        dropManager = GetComponent<EnemyDropManager>();

        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        s_GameStats = GameObject.FindGameObjectWithTag("GameStatsManager").GetComponent<GameStats>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void LateUpdate()
    {
        if (characterStates.isDead)
        {
            if (!droppedItem)
            {
                Vector3 directionAwayFromPlayer = transform.position - playerTransform.position;
                dropManager.DropAppropriateItemAtLocation(transform.position, directionAwayFromPlayer, s_EnemyProperties.dropType, s_EnemyProperties.dynamiteShadowsParentTransform);
                droppedItem = true;

                GameObject instantiatedDeathObject = Instantiate(deathObject, transform.position, Quaternion.identity);
                DeathAnimationHandler deathAnimationHandler = instantiatedDeathObject.GetComponent<DeathAnimationHandler>();
                deathAnimationHandler.BuryInTime(s_EnemyProperties.audioSourcePitch, s_EnemyProperties.enemyType, 2.5f);

                if(s_EnemyProperties.enemyType == EnemyType.TorchGoblin)
                {
                    s_GameStats.KilledTorchGoblin();
                }
                else if (s_EnemyProperties.enemyType == EnemyType.TNTBarrelGoblin)
                {
                    s_GameStats.KilledBarrelGoblin();
                }
                else if (s_EnemyProperties.enemyType == EnemyType.BombGoblin)
                {
                    s_GameStats.KilledTNTGoblin();
                }
            }
            gameObject.SetActive(false);
        }
    }
}
