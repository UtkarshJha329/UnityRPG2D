using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class TurnGrassToSand : MonoBehaviour
{
    [SerializeField] private float turnGrassIntoSandEveryXSeconds = 5.0f;
    [SerializeField] private float turnSandIntoGrassEveryXSecondsForMineDestruction = 30.0f;
    [SerializeField] private float turnGrassIntoSandEveryXSecondsForStructureDestruction = 0.5f;

    private MapGenerator mapGenerator;

    private float nextTurnGrassIntoSandSeconds = 5.0f;
    private float nextTurnSandIntoGrassSeconds = 5.0f;
    private float nextTurnSandIntoGrassSecondsFinal = 0.0f;

    private bool initTilesList = true;
    private Queue<Vector3Int> tilesToTurnIntoSand = new Queue<Vector3Int>();

    [SerializeField] private int maxNumTilesToConvertEachFrame = 40;

    private Queue<Vector3Int> tilesToTurnIntoGrassFromMineDestruction = new Queue<Vector3Int>();
    [SerializeField] private int maxTilesToConvertSandIntoGrassFromMines = 30;

    private Queue<Vector3Int> tilesToTurnIntoGrassFromFinalStructureDestruction = new Queue<Vector3Int>();
    [SerializeField] private int maxTilesToConvertSandIntoGrassFromFinalStructureDestruction = 10;

    public bool performFinalConversion = false;
    public Vector3Int finalPerformanceStartTile = Vector3Int.zero;

    private bool gainedPlayerReference = false;
    private PlayerProperties s_PlayerProperties;

    public AudioClip grassToSandSFX;
    public AudioClip sandToGrassSFX;
    private AudioSource conversionSoundAudioSource;

    private float grassToSandAudioSourcePlayingVolumeOffset = 1.0f;

    public GameObject sandToGrassParticleSystemGameObjectPrefab;

    private Queue<SandGrassConversionParticleManager> conversionParticleSystems = new Queue<SandGrassConversionParticleManager>();
    private Queue<SandGrassConversionParticleManager> conversionParticleSystemsInUse = new Queue<SandGrassConversionParticleManager>();

    private void Awake()
    {
        conversionSoundAudioSource = GetComponent<AudioSource>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mapGenerator = GameObject.FindGameObjectWithTag("MapTileGrid").GetComponent<MapGenerator>();

        int maxParticleSystemsEverNeeded = Mathf.Max(maxTilesToConvertSandIntoGrassFromFinalStructureDestruction, maxTilesToConvertSandIntoGrassFromMines);
        maxParticleSystemsEverNeeded = Mathf.Max(maxParticleSystemsEverNeeded, maxNumTilesToConvertEachFrame);

        for (int i = 0; i < maxParticleSystemsEverNeeded; i++)
        {
            GameObject currentParticleSystemGameObject = Instantiate(sandToGrassParticleSystemGameObjectPrefab, transform);
            currentParticleSystemGameObject.SetActive(false);
            conversionParticleSystems.Enqueue(currentParticleSystemGameObject.GetComponent<SandGrassConversionParticleManager>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if (performFinalConversion)
        //{
        //    AddTileToTurnIntoGrassFinal(finalPerformanceStartTile);
        //    performFinalConversion = false;
        //}

        if (!gainedPlayerReference)
        {
            GameObject playerGameObject = GameObject.FindGameObjectWithTag("Player");
            if(playerGameObject != null)
            {
                s_PlayerProperties = playerGameObject.GetComponent<PlayerProperties>();
            }
        }

        if(s_PlayerProperties == null)
        {
            return;
        }

        while(conversionParticleSystemsInUse.Count > 0)
        {
            if (conversionParticleSystemsInUse.Peek().IsParticleSystemPlaying())
            {
                break;
            }
            else
            {
                SandGrassConversionParticleManager currentFinishedUseParticleSystem = conversionParticleSystemsInUse.Dequeue();
                currentFinishedUseParticleSystem.gameObject.SetActive(false);
                conversionParticleSystems.Enqueue(currentFinishedUseParticleSystem);
            }
        }

        if (initTilesList)
        {
            for (int i = 0; i < mapGenerator.mineTilePositions.Count; i++)
            {
                Vector3Int centre = new Vector3Int((int)mapGenerator.mineTilePositions[i].x, (int)mapGenerator.mineTilePositions[i].y, 0);
                //Vector3 top = centre + Vector3.up;
                //Vector3 right = centre + Vector3.right;
                //Vector3 down = centre + Vector3.down;
                //Vector3 left = centre + Vector3.left;

                tilesToTurnIntoSand.Enqueue(centre);
                //tilesToTurnIntoSand.Add(top);
                //tilesToTurnIntoSand.Add(right);
                //tilesToTurnIntoSand.Add(down);
                //tilesToTurnIntoSand.Add(left);
            }
            initTilesList = false;
        }

        if (nextTurnGrassIntoSandSeconds <= Time.time /* || notFinishedPreviousConversionsToSand*/ && !GameStats.finalStructuresHaveBeenDestroyed)
        {
            int numTilesConvertedThisRound = 0;
            bool playedForThisItterationOfConversion = false;
            while (numTilesConvertedThisRound < maxNumTilesToConvertEachFrame && tilesToTurnIntoSand.Count > 0)
            {
                numTilesConvertedThisRound++;

                Vector3Int currentTilePosition = tilesToTurnIntoSand.Dequeue();

                if (mapGenerator.IsTileGrass(currentTilePosition))
                {
                    mapGenerator.SetGroundTileToSand(currentTilePosition);
                    if (mapGenerator.IsTileInSameRoomAsPlayer(currentTilePosition))
                    {
                        PlayAvailableParticleEffectAtPosition(currentTilePosition, true);
                    }

                    if (mapGenerator.bushOnTile.ContainsKey(currentTilePosition))
                    {
                        mapGenerator.bushOnTile[currentTilePosition].structureCurrentHealth = 0;
                    }

                    if (mapGenerator.treeOnTile.ContainsKey(currentTilePosition))
                    {
                        mapGenerator.treeOnTile[currentTilePosition].structureCurrentHealth = 0;
                    }

                    Vector3Int top = currentTilePosition + Vector3Int.up;
                    Vector3Int right = currentTilePosition + Vector3Int.right;
                    Vector3Int down = currentTilePosition + Vector3Int.down;
                    Vector3Int left = currentTilePosition + Vector3Int.left;

                    if (mapGenerator.IsTileGrass(top))
                    {
                        tilesToTurnIntoSand.Enqueue(top);
                    }
                    if (mapGenerator.IsTileGrass(right))
                    {
                        tilesToTurnIntoSand.Enqueue(right);
                    }
                    if (mapGenerator.IsTileGrass(down))
                    {
                        tilesToTurnIntoSand.Enqueue(down);
                    }
                    if (mapGenerator.IsTileGrass(left))
                    {
                        tilesToTurnIntoSand.Enqueue(left);
                    }

                    if (!playedForThisItterationOfConversion && mapGenerator.IsTileInSameRoomAsPlayer(currentTilePosition))
                    {
                        //Debug.Log("Grass to sand := " + mapGenerator.RoomIndexOfTile(currentTilePosition) + ", " + mapGenerator.PlayerRoomIndex());
                        //StartCoroutine(PlayNConversionSFXDelayed(5, 0.0f, 0.15f, grassToSandSFX, 0.0f));
                        PlayConversionSFXImmediate(grassToSandSFX, 0.0f);
                        s_PlayerProperties.impulseSourceForScreenShake.GenerateImpulseWithVelocity(Random.insideUnitCircle * 0.15f);
                        playedForThisItterationOfConversion = true;
                    }

                    //Debug.Log("Converted tile to sand.");
                }
            }

            nextTurnGrassIntoSandSeconds = Time.time + turnGrassIntoSandEveryXSeconds;
        }

        if (nextTurnSandIntoGrassSeconds <= Time.time)
        {
            int numTilesConvertedThisRound = 0;
            bool playedForThisItterationOfConversion = false;
            while (numTilesConvertedThisRound < maxTilesToConvertSandIntoGrassFromMines && tilesToTurnIntoGrassFromMineDestruction.Count > 0)
            {
                numTilesConvertedThisRound++;

                Vector3Int currentTilePosition = tilesToTurnIntoGrassFromMineDestruction.Dequeue();

                if (mapGenerator.IsTileSand(currentTilePosition))
                {
                    mapGenerator.SetGroundTileToGrass(currentTilePosition);
                    if (mapGenerator.IsTileInSameRoomAsPlayer(currentTilePosition))
                    {
                        PlayAvailableParticleEffectAtPosition(currentTilePosition, false);
                    }

                    if (mapGenerator.bushOnTile.ContainsKey(currentTilePosition))
                    {
                        mapGenerator.bushOnTile[currentTilePosition].ResetSprite();
                    }

                    if (mapGenerator.treeOnTile.ContainsKey(currentTilePosition))
                    {
                        mapGenerator.treeOnTile[currentTilePosition].ResetSprite();
                    }

                    Vector3Int top = currentTilePosition + Vector3Int.up;
                    Vector3Int right = currentTilePosition + Vector3Int.right;
                    Vector3Int down = currentTilePosition + Vector3Int.down;
                    Vector3Int left = currentTilePosition + Vector3Int.left;

                    if (mapGenerator.IsTileSand(top))
                    {
                        tilesToTurnIntoGrassFromMineDestruction.Enqueue(top);
                    }
                    if (mapGenerator.IsTileSand(right))
                    {
                        tilesToTurnIntoGrassFromMineDestruction.Enqueue(right);
                    }
                    if (mapGenerator.IsTileSand(down))
                    {
                        tilesToTurnIntoGrassFromMineDestruction.Enqueue(down);
                    }
                    if (mapGenerator.IsTileSand(left))
                    {
                        tilesToTurnIntoGrassFromMineDestruction.Enqueue(left);
                    }

                    if (!playedForThisItterationOfConversion && mapGenerator.IsTileInSameRoomAsPlayer(currentTilePosition))
                    {
                        //Debug.Log("Sand to grass := " + mapGenerator.RoomIndexOfTile(currentTilePosition) + ", " + mapGenerator.PlayerRoomIndex());
                        //StartCoroutine(PlayNConversionSFXDelayed(5, 0.0f, 0.15f, sandToGrassSFX, 0.0f));
                        PlayConversionSFXImmediate(sandToGrassSFX, grassToSandAudioSourcePlayingVolumeOffset);
                        s_PlayerProperties.impulseSourceForScreenShake.GenerateImpulseWithVelocity(Random.insideUnitCircle * 0.15f);
                        playedForThisItterationOfConversion = true;
                    }

                    //Debug.Log("Converted tile to grass.");
                }
            }

            if (numTilesConvertedThisRound >= maxTilesToConvertSandIntoGrassFromMines)
            {
                tilesToTurnIntoGrassFromMineDestruction.Clear();
            }

            nextTurnSandIntoGrassSeconds = Time.time + turnSandIntoGrassEveryXSecondsForMineDestruction;
        }

        if (nextTurnSandIntoGrassSecondsFinal <= Time.time)
        {
            //Debug.Log("Checking to perform final conversion at := " + nextTurnSandIntoGrassSecondsFinal);
            //Debug.Log("Can convert but := " + tilesToTurnIntoGrassFromFinalStructureDestruction.Count);
            int numTilesConvertedThisRound = 0;
            bool playedForThisItterationOfConversion = false;
            while (tilesToTurnIntoGrassFromFinalStructureDestruction.Count > 0 && numTilesConvertedThisRound < maxTilesToConvertSandIntoGrassFromFinalStructureDestruction)
            {
                //Debug.Log("Converting to grass final.");
                numTilesConvertedThisRound++;

                Vector3Int currentTilePosition = tilesToTurnIntoGrassFromFinalStructureDestruction.Dequeue();

                if (mapGenerator.IsTileSand(currentTilePosition))
                {
                    mapGenerator.SetGroundTileToGrass(currentTilePosition);
                    if (mapGenerator.IsTileInSameRoomAsPlayer(currentTilePosition))
                    {
                        PlayAvailableParticleEffectAtPosition(currentTilePosition, false);
                    }

                    if (mapGenerator.bushOnTile.ContainsKey(currentTilePosition))
                    {
                        mapGenerator.bushOnTile[currentTilePosition].ResetSprite();
                    }

                    if (mapGenerator.treeOnTile.ContainsKey(currentTilePosition))
                    {
                        mapGenerator.treeOnTile[currentTilePosition].ResetSprite();
                    }

                    Vector3Int top = currentTilePosition + Vector3Int.up;
                    Vector3Int right = currentTilePosition + Vector3Int.right;
                    Vector3Int down = currentTilePosition + Vector3Int.down;
                    Vector3Int left = currentTilePosition + Vector3Int.left;

                    if (mapGenerator.IsTileSand(top))
                    {
                        tilesToTurnIntoGrassFromFinalStructureDestruction.Enqueue(top);
                    }
                    if (mapGenerator.IsTileSand(right))
                    {
                        tilesToTurnIntoGrassFromFinalStructureDestruction.Enqueue(right);
                    }
                    if (mapGenerator.IsTileSand(down))
                    {
                        tilesToTurnIntoGrassFromFinalStructureDestruction.Enqueue(down);
                    }
                    if (mapGenerator.IsTileSand(left))
                    {
                        tilesToTurnIntoGrassFromFinalStructureDestruction.Enqueue(left);
                    }

                    if (!playedForThisItterationOfConversion && mapGenerator.IsTileInSameRoomAsPlayer(currentTilePosition))
                    {
                        //Debug.Log("Sand to grass := " + mapGenerator.RoomIndexOfTile(currentTilePosition) + ", " + mapGenerator.PlayerRoomIndex());
                        PlayConversionSFXImmediate(sandToGrassSFX, grassToSandAudioSourcePlayingVolumeOffset);
                        playedForThisItterationOfConversion = true;
                    }

                    //Debug.Log("Converted tile to grass final conversion.");
                }
            }
            nextTurnSandIntoGrassSecondsFinal = Time.time + turnGrassIntoSandEveryXSecondsForStructureDestruction;

            if (GameStats.playerReachedCutSceneTile && !GameStats.finalRoomConvertedIntoGrassFully)
            {
                s_PlayerProperties.impulseSourceForScreenShake.GenerateImpulseWithVelocity(Random.insideUnitCircle * 0.15f);
                //StartCoroutine(PlayNConversionSFXDelayed(5, 0.0f, 0.15f, grassToSandSFX, 0.0f));

                if (IsCastleRoomFullOfGrass())
                {
                    GameStats.finalRoomConvertedIntoGrassFully = true;
                    //Debug.Log("Castle Room Was Fully Converted Into Grass.");
                    // WIN THE GAME!!!
                    GameStats.gameOverState = 1;
                }
            }
        }
    }

    public void AddMineTileToTurnIntoGrassFrom(Vector3Int tileToTurnIntoGrass, bool immediate)
    {
        if (immediate)
        {
            //Debug.Log("Called immediate sand to grass conversion.");

            nextTurnSandIntoGrassSeconds = 0.0f;
            tilesToTurnIntoGrassFromMineDestruction.Clear();
            tilesToTurnIntoGrassFromMineDestruction.Enqueue(tileToTurnIntoGrass);
        }
    }

    public void AddTileToTurnIntoGrassFinal(Vector3Int tileToTurnIntoGrass)
    {
        //Debug.Log("Called for final conversion.");
        nextTurnSandIntoGrassSecondsFinal = 0.0f;
        tilesToTurnIntoGrassFromFinalStructureDestruction.Enqueue(tileToTurnIntoGrass);
    }

    public bool IsCastleRoomFullOfGrass()
    {
        Vector2Int castleSpawnRoomTileOffset = mapGenerator.castleSpawnRoom * mapGenerator.NumTilesInRooms();
        for (int x = 0; x < mapGenerator.NumTilesInRooms().x; x++)
        {
            for (int y = 0; y < mapGenerator.NumTilesInRooms().y; y++)
            {
                if (mapGenerator.IsTileSand(new Vector3Int(castleSpawnRoomTileOffset.x + x, castleSpawnRoomTileOffset.y + y, 0)))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void PlayConversionSFXImmediate(AudioClip conversionSFX, float volumeOffset)
    {
        conversionSoundAudioSource.PlayOneShot(conversionSFX, Random.Range(0.75f, 1.0f) + volumeOffset);
    }

    private void PlayAvailableParticleEffectAtPosition(Vector3 position, bool playGrassParticles)
    {
        SandGrassConversionParticleManager newUseParticleSystem;
        if(conversionParticleSystems.TryDequeue(out newUseParticleSystem))
        {
            newUseParticleSystem.transform.position = position;
            if (playGrassParticles)
            {
                newUseParticleSystem.PlayGrassParticles(position);
            }
            else
            {
                newUseParticleSystem.PlaySandParticles(position);
            }
            newUseParticleSystem.gameObject.SetActive(true);

            conversionParticleSystemsInUse.Enqueue(newUseParticleSystem);
        }
    }
}