using System.Xml.Schema;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class StructureHealth : MonoBehaviour
{
    public bool isMine = false;
    public Sprite destroyedStructureSprite;

    public int structureCurrentHealth = 40;
    private SpriteRenderer structureSpriteRenderer;

    public static Transform playerTransform;

    private StructureHealthDisplayManager structureHealthDisplayManager;
    private JiggleStructure jiggleManager;

    private TurnGrassToSand grassSandConversionManager;

    public Vector3Int structureTilePos = Vector3Int.zero;

    public FinalSandToGrassConversionManager finalSandToGrassConversionManager;

    private Sprite originalStructureSprite;

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

        grassSandConversionManager = GameObject.FindGameObjectWithTag("GrassSandConversionManager").GetComponent<TurnGrassToSand>();

        structureSpriteRenderer = GetComponent<SpriteRenderer>();

        structureHealthDisplayManager = GetComponent<StructureHealthDisplayManager>();
        jiggleManager = GetComponent<JiggleStructure>();

        originalStructureSprite = structureSpriteRenderer.sprite;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //if (transform.position.y < playerTransform.position.y)
        //{
        //    structureSpriteRenderer.sortingOrder = 500;
        //}
        //else
        //{
        //    structureSpriteRenderer.sortingOrder = ((int) transform.position.y * -1);
        //}

        if (structureCurrentHealth <= 0)
        {
            structureSpriteRenderer.sprite = destroyedStructureSprite;

            if (isMine)
            {
                grassSandConversionManager.AddMineTileToTurnIntoGrassFrom(structureTilePos, true);
            }
            // Add smoke and sound effects during the change.
        }
    }

    public void DamageStructure(int damageAmount)
    {
        structureCurrentHealth += damageAmount;
        structureHealthDisplayManager.damaged = true;
        jiggleManager.jiggle = true;
    }

    public void ResetSprite()
    {
        structureSpriteRenderer.sprite = originalStructureSprite;
        structureCurrentHealth = 1;
    }
}
