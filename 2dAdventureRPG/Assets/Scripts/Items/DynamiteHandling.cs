using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

public class DynamiteHandling : MonoBehaviour
{
    public float dynamiteExplosionVolume = 0.0f;

    public Vector3 moveDirection = Vector3.zero;
    public float moveSpeedDirect = 2.0f;

    public float timeInWhichDynamiteDetonates = 2.0f;
    public float timeToReachTarget = 0.0f;

    public float explosionAtTime = 0.0f;

    public int damage = 10;
    public float knockbackForce = 10.0f;

    public PlayerHealth playerHealthManager;
    public PlayerMovement s_PlayerMovement;

    public CircleCollider2D explosionRangeCollider;

    public GameObject explosionObjectPrefabToSummonBeforeDestroying;

    public Transform dynamiteShadowsParentTransform;
    public GameObject shadowSpriteObjectToSpawn;
    public Transform shadowSpriteTransform;

    public GameObject explosionRadiusIndicator;
    public float explosionRadiusIndicatorFlickerRate = 4;
    private float nextTimeToFlicker = 0.0f;

    public Transform dynamiteSpriteTransform;

    public float fakeHeightToReach = 1.5f;
    public float verticalSpeed = 0.0f;
    public float verticalAcceleration = 0.0f;

    public SpriteRenderer dynamiteSprite;

    private float travellingTimer = 0.0f;

    public bool showDynamiteSprite = true;
    public Transform tntGoblinEnemyParent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(explosionRangeCollider == null)
        {
            Debug.LogError("explosionRangeCollider reference is missing in Dynamite Handling script.");
        }
        if(explosionObjectPrefabToSummonBeforeDestroying  == null)
        {
            Debug.LogError("explosionObjectPrefabToSummonBeforeDestroying reference is missing in Dynamite Handling script.");
        }
        if (explosionRadiusIndicator == null)
        {
            Debug.LogError("explosionRadiusIndicator reference is missing in Dynamite Handling script.");
        }
        shadowSpriteTransform = Instantiate(shadowSpriteObjectToSpawn, transform.position, Quaternion.identity, dynamiteShadowsParentTransform).transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (!showDynamiteSprite)
        {
            dynamiteSpriteTransform.gameObject.SetActive(false);

            if (nextTimeToFlicker <= Time.time)
            {
                explosionRadiusIndicator.SetActive(!explosionRadiusIndicator.activeInHierarchy);
                nextTimeToFlicker = Time.time + 1.0f / explosionRadiusIndicatorFlickerRate;
            }
        }
        if (explosionAtTime <= Time.time)
        {
            Explode();
        }
        else if(showDynamiteSprite)
        {
            travellingTimer += Time.deltaTime;
            transform.position += moveDirection * moveSpeedDirect * Time.deltaTime;

            bool arcBomb = (timeToReachTarget > timeInWhichDynamiteDetonates * 0.65f);
            if (arcBomb && (travellingTimer < timeToReachTarget * 0.5f && moveSpeedDirect != 0.0f))
            {
                //Debug.Log("Travelling. Vertical speed is < 0 ? " + (verticalSpeed < 0.0f));
                dynamiteSpriteTransform.localPosition += Vector3.up * verticalSpeed * Time.deltaTime;
                //verticalSpeed -= Time.deltaTime;
            }
            else if (travellingTimer > timeToReachTarget)
            {
                if(nextTimeToFlicker <= Time.time)
                {
                    explosionRadiusIndicator.SetActive(!explosionRadiusIndicator.activeInHierarchy);
                    nextTimeToFlicker = Time.time + 1.0f / explosionRadiusIndicatorFlickerRate;
                }
                moveSpeedDirect = 0.0f;
            }
            else if(arcBomb && travellingTimer < timeToReachTarget)
            {
                //  || moveSpeedDirect == 0.0f
                dynamiteSpriteTransform.localPosition += Vector3.down * verticalSpeed * Time.deltaTime;
            }

            if (shadowSpriteTransform != null)
            {
                shadowSpriteTransform.position += moveDirection * moveSpeedDirect * Time.deltaTime;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log(collision.gameObject.name);
        moveSpeedDirect = 0.0f;
    }

    private void Explode()
    {
        if (explosionRangeCollider.OverlapPoint(s_PlayerMovement.transform.position))
        {
            if (playerHealthManager.ChangeHealth(damage))
            {
                Vector3 knockbackDirection = s_PlayerMovement.transform.position - transform.position;
                s_PlayerMovement.KnockbackPlayer(knockbackForce, knockbackDirection);
            }
        }
        else
        {
            s_PlayerMovement.GetComponent<PlayerProperties>().impulseSourceForScreenShake.GenerateImpulseWithVelocity(Random.insideUnitCircle * 1.0f);
        }

        if (shadowSpriteTransform != null)
        {
            //shadowSpriteTransform = Instantiate(shadowSpriteObjectToSpawn, transform.position, Quaternion.identity, dynamiteShadowsParentTransform).transform;
            //Debug.LogError("shadowSpiritTransform is missing!");

            GameObject explosionGameObject = Instantiate(explosionObjectPrefabToSummonBeforeDestroying, shadowSpriteTransform.position, Quaternion.identity);

            ExplosionSoundEffectHandler explosionSoundEffectHandler = explosionGameObject.GetComponent<ExplosionSoundEffectHandler>();
            EnemyType dynamiteLaunchedFromEnemy = showDynamiteSprite ? EnemyType.BombGoblin : EnemyType.TNTBarrelGoblin;
            explosionSoundEffectHandler.explosionSfxClip = AllAudioContainer.blastBasedOnEnemyType[dynamiteLaunchedFromEnemy];
            explosionSoundEffectHandler.explosionVolume = dynamiteExplosionVolume;

            Destroy(gameObject);

        }
    }
}
