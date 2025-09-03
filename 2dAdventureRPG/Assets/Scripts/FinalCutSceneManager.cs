using UnityEngine;
using UnityEngine.Tilemaps;

public class FinalCutSceneManager : MonoBehaviour
{
    public Tilemap holyLightTileMap;
    public AnimatedTile holyLightDescendedLoopTile;
    public AnimatedTile holyLightFinishedAfterDescendedLoopTile;

    private GameObject playerGameObject;
    private PlayerProperties s_PlayerProperties;
    private MapGenerator mapGenerator;

    public CameraTargetManager finalRoomCameraTargetManager;

    public AudioClip finalCutSceneBeamPowerUpClip;
    public AudioSource finalCutSceneBeamPowerUpAudioSource;

    public bool once = true;
    private bool playedCutScene = false;

    private void Awake()
    {
        finalCutSceneBeamPowerUpAudioSource = GetComponent<AudioSource>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //if (temp)
        //{
        //    MapGenerator mapGenerator = GameObject.FindGameObjectWithTag("MapTileGrid").GetComponent<MapGenerator>();

        //    Vector2Int playerTilePosition = mapGenerator.castleSpawnRoom * mapGenerator.NumTilesInRooms() + mapGenerator.NumTilesInRooms() / 2;
        //    Vector3Int holyLightTilePosition = new Vector3Int(playerTilePosition.x, playerTilePosition.y, 0);

        //    for (int yOffset = 0; yOffset < mapGenerator.NumTilesInRooms().y / 2; yOffset++)
        //    {
        //        holyLightTileMap.SetTile(holyLightTilePosition, holyLightDescendedLoopTile);
        //        holyLightTilePosition.y++;
        //    }

        //    temp = false;
        //}

        if (GameStats.finalStructuresHaveBeenDestroyed && once)
        {
            mapGenerator = GameObject.FindGameObjectWithTag("MapTileGrid").GetComponent<MapGenerator>();
            playerGameObject = GameObject.FindGameObjectWithTag("Player");
            s_PlayerProperties = playerGameObject.GetComponent<PlayerProperties>();
            s_PlayerProperties.currentCutsceneMovementTarget = new Vector2(mapGenerator.castleSpawnRoom.x * mapGenerator.NumTilesInRooms().x, mapGenerator.castleSpawnRoom.y * mapGenerator.NumTilesInRooms().y)
                                                                + new Vector2(mapGenerator.NumTilesInRooms().x, mapGenerator.NumTilesInRooms().y) * 0.5f;

            s_PlayerProperties.isPlayingCutscene = true;
            once = false;
        }

        if (GameStats.playerReachedCutSceneTile && !GameStats.playerFinishedFinalCutscene && !playedCutScene)
        {
            PlayCutscene();
            playedCutScene = true;
        }

        //if (GameStats.finalStructuresHaveBeenDestroyed)
        //{
        //    if(Vector3.Distance(playerGameObject.transform.position, new Vector3(s_PlayerProperties.currentCutsceneMovementTarget.x, s_PlayerProperties.currentCutsceneMovementTarget.y, 0.0f)) < 0.1f)
        //    {
        //        GameStats.playerFinishedFinalCutscene = true;
        //    }
        //}
    }

    private void PlayCutscene()
    {
        finalCutSceneBeamPowerUpAudioSource.PlayOneShot(finalCutSceneBeamPowerUpClip, 1.5f);
        DrawHolyLightTiles();
        Invoke("FinishPlayingHolyLight", 4.0f);
        Invoke("CompleteCutscene", 5.0f);
    }

    private void CompleteCutscene()
    {
        HideHolyLightTiles();
        GameStats.playerFinishedFinalCutscene = true;
        playerGameObject.GetComponent<PlayerHaloManager>().haloIsVisible = false;
    }

    private void DrawHolyLightTiles()
    {
        MapGenerator mapGenerator = GameObject.FindGameObjectWithTag("MapTileGrid").GetComponent<MapGenerator>();

        Vector2Int playerTilePosition = mapGenerator.castleSpawnRoom * mapGenerator.NumTilesInRooms() + mapGenerator.NumTilesInRooms() / 2;
        Vector3Int holyLightTilePosition = new Vector3Int(playerTilePosition.x, playerTilePosition.y, 0);

        for (int yOffset = 0; yOffset < mapGenerator.NumTilesInRooms().y / 2; yOffset++)
        {
            holyLightTileMap.SetTile(holyLightTilePosition, holyLightDescendedLoopTile);
            holyLightTilePosition.y++;
        }

        finalRoomCameraTargetManager.DestroyAllShadows();
    }

    private void FinishPlayingHolyLight()
    {
        MapGenerator mapGenerator = GameObject.FindGameObjectWithTag("MapTileGrid").GetComponent<MapGenerator>();

        Vector2Int playerTilePosition = mapGenerator.castleSpawnRoom * mapGenerator.NumTilesInRooms() + mapGenerator.NumTilesInRooms() / 2;
        Vector3Int holyLightTilePosition = new Vector3Int(playerTilePosition.x, playerTilePosition.y, 0);

        for (int yOffset = 0; yOffset < mapGenerator.NumTilesInRooms().y / 2; yOffset++)
        {
            holyLightTileMap.SetTile(holyLightTilePosition, holyLightFinishedAfterDescendedLoopTile);
            holyLightTilePosition.y++;
        }
    }

    private void HideHolyLightTiles()
    {
        holyLightTileMap.gameObject.SetActive(false);
    }
}
