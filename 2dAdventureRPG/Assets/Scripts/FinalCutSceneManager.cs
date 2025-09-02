using UnityEngine;

public class FinalCutSceneManager : MonoBehaviour
{
    private GameObject playerGameObject;
    private PlayerProperties s_PlayerProperties;

    private bool once = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameStats.finalStructuresHaveBeenDestroyed && once)
        {
            MapGenerator mapGenerator = GameObject.FindGameObjectWithTag("MapTileGrid").GetComponent<MapGenerator>();
            playerGameObject = GameObject.FindGameObjectWithTag("Player");
            s_PlayerProperties = playerGameObject.GetComponent<PlayerProperties>();
            s_PlayerProperties.currentCutsceneMovementTarget = new Vector2(mapGenerator.castleSpawnRoom.x * mapGenerator.NumTilesInRooms().x, mapGenerator.castleSpawnRoom.y * mapGenerator.NumTilesInRooms().y)
                                                                + new Vector2(mapGenerator.NumTilesInRooms().x, mapGenerator.NumTilesInRooms().y) * 0.5f;
            s_PlayerProperties.isPlayingCutscene = true;
            once = false;
        }

        if (GameStats.finalStructuresHaveBeenDestroyed)
        {
            if(Vector3.Distance(playerGameObject.transform.position, new Vector3(s_PlayerProperties.currentCutsceneMovementTarget.x, s_PlayerProperties.currentCutsceneMovementTarget.y, 0.0f)) < 0.1f)
            {
                GameStats.playerFinishedFinalCutscene = true;
            }
        }

    }
}
