using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyHealthHeartsDisplayManager : MonoBehaviour
{
    [SerializeField] private GameObject enemyHeartObject;
    [SerializeField] private Transform enemyHeartParentTransform;

    private EnemyHealth s_EnemyHealth;

    public float paddingBetweenEnemyHearts = 1.5f;
    private Sprite enemyHeartSprite;

    private float previousHealth = 0.0f;

    public bool damaged = false;

    private Vector3 originalHeartParentTransformLocalPosition = Vector3.zero;

    private void Awake()
    {
        s_EnemyHealth = GetComponent<EnemyHealth>();

        enemyHeartSprite = enemyHeartObject.GetComponent<SpriteRenderer>().sprite;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalHeartParentTransformLocalPosition = enemyHeartParentTransform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (damaged && previousHealth != s_EnemyHealth.GetCurrentHealth())
        {
            UpdateEnemyHealthUIHearts();
            previousHealth = s_EnemyHealth.GetCurrentHealth();
        }
    }

    private void UpdateEnemyHealthUIHearts()
    {
        //Debug.Log("Created enemy hearts.");
        int enemyCurrentHealth = (int)s_EnemyHealth.GetCurrentHealth();

        float horizontalSpacingBetweenEachHeart = enemyHeartSprite.bounds.size.x + paddingBetweenEnemyHearts;

        if(enemyHeartParentTransform.childCount < enemyCurrentHealth)
        {
            for (int i = enemyHeartParentTransform.childCount; i < enemyCurrentHealth; i++)
            {
                GameObject heart = Instantiate(enemyHeartObject, transform.position, Quaternion.identity, enemyHeartParentTransform);
                heart.name = "Enemy Heart (" + i + ")";
            }
        }

        enemyHeartParentTransform.localPosition = originalHeartParentTransformLocalPosition + (Vector3.left * enemyCurrentHealth / 2) * horizontalSpacingBetweenEachHeart;

        for (int i = 0; i < enemyHeartParentTransform.childCount; i++)
        {
            if(i < enemyCurrentHealth)
            {
                GameObject heart = enemyHeartParentTransform.GetChild(i).gameObject;
                heart.transform.position = enemyHeartParentTransform.position + Vector3.right * (i * horizontalSpacingBetweenEachHeart);
                heart.SetActive(true);
            }
            else
            {
                enemyHeartParentTransform.GetChild(i).gameObject.SetActive(false);
            }
        }

        //enemyHeartParentTransform.localPosition -= Vector3.left * enemyCurrentHealth * horizontalSpacingBetweenEachHeart;

    }
}
