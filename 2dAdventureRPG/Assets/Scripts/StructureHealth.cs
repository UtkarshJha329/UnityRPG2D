using System.Xml.Schema;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(SpriteRenderer))]
public class StructureHealth : MonoBehaviour
{
    public bool isGoblinHut = false;
    public AudioClip goblinHutHitSFX;
    public AudioClip goblinHutCollapseSFX;
    
    public bool isMine = false;
    public Sprite destroyedStructureSprite;

    public int structureCurrentHealth = 40;
    private SpriteRenderer structureSpriteRenderer;

    public static Transform playerTransform;

    private StructureHealthDisplayManager structureHealthDisplayManager;
    private JiggleStructure jiggleManager;

    private TurnGrassToSand grassSandConversionManager;

    public Vector3Int structureTilePos = Vector3Int.zero;

    //public FinalSandToGrassConversionManager finalSandToGrassConversionManager;

    private Sprite originalStructureSprite;
    private AudioSource structureBrokeAudioSourcePlayer;

    public bool foliage = false;
    private bool alreadyUsedForConversionToGrass = false;

    private bool playedGoblinHutDestroyedSfx = false;

    private GameStats s_GameStats;

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

        structureBrokeAudioSourcePlayer = GetComponent<AudioSource>();
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        s_GameStats = GameObject.FindGameObjectWithTag("GameStatsManager").GetComponent<GameStats>();
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

        if (!foliage)
        {
            if (transform.position.y < playerTransform.position.y)
            {
                structureSpriteRenderer.sortingOrder = 500;
            }
            else
            {
                structureSpriteRenderer.sortingOrder = ((int)transform.position.y * -1);
            }
        }

        if (structureCurrentHealth <= 0)
        {
            structureSpriteRenderer.sprite = destroyedStructureSprite;

            if (isMine && !alreadyUsedForConversionToGrass)
            {
                structureBrokeAudioSourcePlayer.PlayOneShot(AllAudioContainer.structureCollapseAudioClips[Random.Range(0, AllAudioContainer.structureCollapseAudioClips.Count)], 5.0f);

                grassSandConversionManager.AddMineTileToTurnIntoGrassFrom(structureTilePos, true);
                alreadyUsedForConversionToGrass = true;

                s_GameStats.DestroyedMine();
            }
            // Add smoke and sound effects during the change.
            if(isGoblinHut && !playedGoblinHutDestroyedSfx)
            {
                structureBrokeAudioSourcePlayer.pitch = Random.Range(0.95f, 1.0f);
                structureBrokeAudioSourcePlayer.clip = goblinHutCollapseSFX;
                structureBrokeAudioSourcePlayer.volume = Random.Range(0.85f, 1.5f);
                structureBrokeAudioSourcePlayer.Play();

                playedGoblinHutDestroyedSfx = true;
            }
        }
    }

    public void DamageStructure(int damageAmount)
    {
        structureCurrentHealth += damageAmount;
        structureHealthDisplayManager.damaged = true;
        jiggleManager.jiggle = true;

        if (isGoblinHut)
        {
            structureBrokeAudioSourcePlayer.pitch = Random.Range(0.65f, 1.0f);
            structureBrokeAudioSourcePlayer.clip = goblinHutHitSFX;
            structureBrokeAudioSourcePlayer.volume = Random.Range(0.85f, 1.5f);
            structureBrokeAudioSourcePlayer.Play();
        }
    }

    public void ResetSprite()
    {
        structureSpriteRenderer.sprite = originalStructureSprite;
        structureCurrentHealth = 1;
    }
}
