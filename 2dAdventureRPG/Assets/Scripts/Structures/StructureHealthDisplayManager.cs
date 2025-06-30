using UnityEngine;

[RequireComponent(typeof(StructureHealth))]
public class StructureHealthDisplayManager : MonoBehaviour
{
    [SerializeField] private GameObject structureHeartObject;
    [SerializeField] private Transform structureHeartParentTransform;

    private StructureHealth s_StructureHealth;

    public float paddingBetweenHearts = 1.5f;
    private Sprite structureHeartSprite;

    private float previousHealth = 0.0f;

    public bool damaged = false;

    private Vector3 originalHeartsTransformPosition = Vector3.zero;

    private void Awake()
    {
        s_StructureHealth = GetComponent<StructureHealth>();

        structureHeartSprite = structureHeartObject.GetComponent<SpriteRenderer>().sprite;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalHeartsTransformPosition = structureHeartParentTransform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (damaged && previousHealth != s_StructureHealth.structureCurrentHealth)
        {
            UpdateEnemyHealthUIHearts();
            previousHealth = s_StructureHealth.structureCurrentHealth;
        }
    }

    private void UpdateEnemyHealthUIHearts()
    {
        //Debug.Log("Created enemy hearts.");
        int structureCurrentHealth = (int)s_StructureHealth.structureCurrentHealth;

        float horizontalSpacingBetweenEachHeart = structureHeartSprite.bounds.size.x + paddingBetweenHearts;

        if (structureHeartParentTransform.childCount < structureCurrentHealth)
        {
            for (int i = structureHeartParentTransform.childCount; i < structureCurrentHealth; i++)
            {
                GameObject heart = Instantiate(structureHeartObject, transform.position, Quaternion.identity, structureHeartParentTransform);
                heart.name = "Structure Heart (" + i + ")";
            }
        }

        structureHeartParentTransform.position = originalHeartsTransformPosition + Vector3.left * (structureCurrentHealth / 2) * horizontalSpacingBetweenEachHeart;

        for (int i = 0; i < structureHeartParentTransform.childCount; i++)
        {
            if (i < structureCurrentHealth)
            {
                GameObject heart = structureHeartParentTransform.GetChild(i).gameObject;
                heart.transform.position = structureHeartParentTransform.position + Vector3.right * (i * horizontalSpacingBetweenEachHeart);
                heart.SetActive(true);
            }
            else
            {
                structureHeartParentTransform.GetChild(i).gameObject.SetActive(false);
            }
        }

        //enemyHeartParentTransform.localPosition -= Vector3.left * enemyCurrentHealth * horizontalSpacingBetweenEachHeart;

    }
}
