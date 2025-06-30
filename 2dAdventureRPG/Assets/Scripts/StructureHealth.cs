using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class StructureHealth : MonoBehaviour
{
    public Sprite destroyedStructureSprite;

    public int structureCurrentHealth = 40;
    private SpriteRenderer structureSpriteRenderer;

    public static Transform playerTransform;

    private StructureHealthDisplayManager structureHealthDisplayManager;

    private void Awake()
    {
        if(destroyedStructureSprite == null)
        {
            Debug.LogError("closedMineSprite variable has not been assigned in the inspector on the object, " + gameObject.name);
        }

        if(playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }

        structureSpriteRenderer = GetComponent<SpriteRenderer>();

        structureHealthDisplayManager = GetComponent<StructureHealthDisplayManager>();
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < playerTransform.position.y)
        {
            structureSpriteRenderer.sortingOrder = 500;
        }
        else
        {
            structureSpriteRenderer.sortingOrder = 0;
        }

        if (structureCurrentHealth <= 0)
        {
            structureSpriteRenderer.sprite = destroyedStructureSprite;
            // Add smoke and sound effects during the change.
        }
    }

    public void DamageStructure(int damageAmount)
    {
        structureCurrentHealth += damageAmount;
        structureHealthDisplayManager.damaged = true;
    }
}
