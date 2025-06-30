using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Drawing.Inspector.PropertyDrawers;
using UnityEngine;
using UnityEngine.Tilemaps;
using static System.Collections.Specialized.BitVector32;

public enum TileBorderType
{
    TOP_MIDDLE,
    BOTTOM_MIDDLE,
    RIGHT_MIDDLE,
    LEFT_MIDDLE
}

public enum SectionBorderingWallType
{
    Top,
    Left,
    Bottom,
    Right
}

[System.Serializable]
public class Section
{
    public Vector2 bottomLeft;
    public Vector2 size;

    public bool mineSection = false;

    public List<int> connectedSectionsInThisRoom = new List<int>();
    public List<SectionBorderingWallType> borderingWallTypes = new List<SectionBorderingWallType>();

    public List<Vector2> patrolPoints = new List<Vector2>();
    public List<bool> patrolPointsOccupied = new List<bool>();

    public List<EnemyProperties> enemiesPropertyComponentList = new List<EnemyProperties>();
    public List<Enemy_Movement> enemiesMovementComponentList = new List<Enemy_Movement>();
    public List<EnemyHealth> enemiesHealthComponentList = new List<EnemyHealth>();
}

[System.Serializable]
public class Room
{
    public List<Section> sections = new List<Section>();

    public List<Vector2Int> connectedRooms = new List<Vector2Int>();

    public List<int> entryExitSectionIndex = new List<int>();
}

[System.Serializable]
public class DebugRoom
{
    public Vector2Int roomIndex;
    public Room roomData;
}

public class Connection
{
    public Vector2Int a;
    public Vector2Int b;
}

public class ConnectionTiles
{
    public Vector3Int worldTileA;
    public Vector3Int worldTileB;
}

public class EnemyTypeAndPercentageSpawnList
{
    public EnemyTypeAndPercentageSpawnList(EnemyType enemyType, float spawnProbability)
    {
        enemySpawnTypeAndProbabilityRanges.Add(spawnProbability, enemyType);
    }
    public EnemyTypeAndPercentageSpawnList(List<EnemyType> enemyTypes, List<float> spawnProbabilities)
    {
        for (int i = 0; i < enemyTypes.Count; i++)
        {
            enemySpawnTypeAndProbabilityRanges.Add(spawnProbabilities[i], enemyTypes[i]);
        }
    }

    //public Dictionary<EnemyType, float> enemySpawnTypeAndProbability = new Dictionary<EnemyType, float>();
    public Dictionary<float, EnemyType> enemySpawnTypeAndProbabilityRanges = new Dictionary<float, EnemyType>();
}

public class MapGenerator : MonoBehaviour
{
    public bool drawConnnectionsInSand = false;

    public EnemyType spawnEnemyType = EnemyType.TorchGoblin;

    [Header("Enemy Instantiating Data")]
    [SerializeField] private GameObject torchGoblinEnemy;
    [SerializeField] private GameObject bombGoblinEnemy;
    [SerializeField] private GameObject tntGoblinEnemy;
    [SerializeField] private GameObject randomPrefabEnemy;
    [SerializeField] private bool debugEnemy = false;

    private GameObject goblinToSpawn;

    [Header("Map Generator Data")]
    [SerializeField] private Vector2Int mapSizeInRooms;
    [SerializeField] private Vector2Int numTilesInRooms;
    [SerializeField] private Vector2Int minimumSectionSize = new Vector2Int(5, 5);
    [SerializeField] private Vector2Int maximumSectionSize = Vector2Int.zero;
    [SerializeField] private Vector2Int playerSpawnRoom;
    [SerializeField] private Vector2Int playerSpawnTile;
    [SerializeField] public Vector2Int castleSpawnRoom;

    [Header("Resource Section Properties")]
    [SerializeField] private Vector2Int dimsForRecourceRooms = new Vector2Int(5, 5);
    [SerializeField] private int maxNumTilesForResourceRooms = 35;
    [SerializeField] private int minYForResourceRooms = 3;
    [SerializeField] private int minYForTeleportationRooms = 7;
    [SerializeField] private float probabilityOfSpecialSection = 0.15f;
    [SerializeField] private int minNumberOfTilesForTreesAndBushes = 36;

    [Header("Map Generator Tiles Data")]
    [SerializeField] private Tilemap groundTileMap;
    [SerializeField] private Tilemap wallsTileMap;

    [SerializeField] private Tile groundTileGrassTL;
    [SerializeField] private Tile groundTileGrassTM;
    [SerializeField] private Tile groundTileGrassTR;
    [SerializeField] private Tile groundTileGrassML;
    [SerializeField] private Tile groundTileGrassMM;
    [SerializeField] private Tile groundTileGrassMR;
    [SerializeField] private Tile groundTileGrassBL;
    [SerializeField] private Tile groundTileGrassBM;
    [SerializeField] private Tile groundTileGrassBR;

    [SerializeField] private Tile stoneWallTile;

    [SerializeField] private Tile groundTileDrySand;

    [Header("Room Instantiating Data")]
    [SerializeField] private Transform mapParent;
    [SerializeField] private Transform roomsParent;
    [SerializeField] private GameObject enemiesParentPrefab;
    [SerializeField] private GameObject goldMinesParentPrefab;
    [SerializeField] private GameObject emptyRoomPrefab;
    [SerializeField] private GameObject playerPrefab;

    [SerializeField] private GameObject minesObjectPrefab;
    [SerializeField] private GameObject bushesObjectPrefab;
    [SerializeField] private GameObject treesObjectPrefab;
    [SerializeField] private GameObject goblinSpawnTowerPrefab;
    [SerializeField] private List<Vector2Int> goblinTowerSpawnerPositions;
    private List<Transform> goblinTowers = new List<Transform>();

    public Dictionary<Vector2Int, GameObject> roomObjectDictionary = new Dictionary<Vector2Int, GameObject>();

    [Header("Room Camera Generator Data")]
    [SerializeField] private CinemachineCamera cineCamera;
    [SerializeField] private GameObject currentRoomBoundaryObject;

    private Vector2Int mapSizeInTiles;

    private List<Vector3> enemyPositions = new List<Vector3>();

    public Dictionary<Vector2Int, Room> roomIndexAndRoom = new Dictionary<Vector2Int, Room>();

    [SerializeField] string debugStringVisitedRooms = "";
    [SerializeField] string debugStringUntouchedRooms = "";

    [SerializeField] List<DebugRoom> roomsThatHaveBeenCreated;

    private int numEnemiesSpawned = 0;

    private int left = 0;
    private int right = 1;
    private int up = 2;

    private int borderInset = 1;


    private Dictionary<int, EnemyTypeAndPercentageSpawnList> spawnTypesBasedOnRoomY = new Dictionary<int, EnemyTypeAndPercentageSpawnList>();

    private GameObject player;

    public List<Vector3> mineTilePositions = new List<Vector3>();

    private void Awake()
    {
        if (groundTileMap == null)
        {
            Debug.LogWarning("WARNING : Ground tilemap reference not set in Map Generator.");
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        maximumSectionSize = new Vector2Int((2 * numTilesInRooms.x) / 3, (2 * numTilesInRooms.y) / 3);
        minYForResourceRooms = mapSizeInRooms.y / 3;
        minYForTeleportationRooms = 2 * mapSizeInRooms.y / 3;

        mapSizeInTiles = mapSizeInRooms * numTilesInRooms;

        playerSpawnRoom = new Vector2Int(mapSizeInRooms.x / 2, 0);
        //playerSpawnTile = new Vector2Int(numTilesInRooms.x / 2, 2);
        castleSpawnRoom = new Vector2Int(mapSizeInRooms.x / 2, mapSizeInRooms.y - 1);


        for (int y = 0; y < mapSizeInRooms.y; y++)
        {
            for (int x = 0; x < mapSizeInRooms.x; x++)
            {
                Room curRoom = new Room();
                roomIndexAndRoom.Add(new Vector2Int(x, y), curRoom);
            }
        }

        player = Instantiate(playerPrefab);

        spawnTypesBasedOnRoomY[0] = new EnemyTypeAndPercentageSpawnList(EnemyType.TorchGoblin, 1.0f);
        spawnTypesBasedOnRoomY[1] = new EnemyTypeAndPercentageSpawnList(EnemyType.TorchGoblin, 1.0f);
        spawnTypesBasedOnRoomY[2] = new EnemyTypeAndPercentageSpawnList(EnemyType.TorchGoblin, 1.0f);

        List<EnemyType> enemyTypesToSpawnOn3 = new List<EnemyType> { EnemyType.TorchGoblin, EnemyType.TNTBarrelGoblin };
        List<float> enemyTypesToSpawnOn3ProbabilitiesRanges = new List<float> { 0.8f, 1.0f };
        spawnTypesBasedOnRoomY[3] = new EnemyTypeAndPercentageSpawnList(enemyTypesToSpawnOn3, enemyTypesToSpawnOn3ProbabilitiesRanges);

        List<EnemyType> enemyTypesToSpawnOn4 = new List<EnemyType> { EnemyType.TorchGoblin, EnemyType.TNTBarrelGoblin };
        List<float> enemyTypesToSpawnOn4ProbabilitiesRanges = new List<float> { 0.65f, 1.0f };
        spawnTypesBasedOnRoomY[4] = new EnemyTypeAndPercentageSpawnList(enemyTypesToSpawnOn4, enemyTypesToSpawnOn4ProbabilitiesRanges);

        List<EnemyType> enemyTypesToSpawnOn5 = new List<EnemyType> { EnemyType.TorchGoblin, EnemyType.TNTBarrelGoblin };
        List<float> enemyTypesToSpawnOn5ProbabilitiesRanges = new List<float> { 0.45f, 1.0f };
        spawnTypesBasedOnRoomY[5] = new EnemyTypeAndPercentageSpawnList(enemyTypesToSpawnOn5, enemyTypesToSpawnOn5ProbabilitiesRanges);

        spawnTypesBasedOnRoomY[6] = new EnemyTypeAndPercentageSpawnList(EnemyType.TNTBarrelGoblin, 1.0f);

        List<EnemyType> enemyTypesToSpawnOn7 = new List<EnemyType> { EnemyType.TorchGoblin, EnemyType.TNTBarrelGoblin, EnemyType.BombGoblin };
        List<float> enemyTypesToSpawnOn7ProbabilitiesRanges = new List<float> { 0.45f, 0.8f, 1.0f };
        spawnTypesBasedOnRoomY[7] = new EnemyTypeAndPercentageSpawnList(enemyTypesToSpawnOn7, enemyTypesToSpawnOn7ProbabilitiesRanges);

        List<EnemyType> enemyTypesToSpawnOn8 = new List<EnemyType> { EnemyType.TorchGoblin, EnemyType.TNTBarrelGoblin, EnemyType.BombGoblin };
        List<float> enemyTypesToSpawnOn8ProbabilitiesRanges = new List<float> { 0.35f, 0.8f, 1.0f };
        spawnTypesBasedOnRoomY[8] = new EnemyTypeAndPercentageSpawnList(enemyTypesToSpawnOn8, enemyTypesToSpawnOn8ProbabilitiesRanges);

        List<EnemyType> enemyTypesToSpawnOn9 = new List<EnemyType> { EnemyType.TorchGoblin, EnemyType.TNTBarrelGoblin, EnemyType.BombGoblin };
        List<float> enemyTypesToSpawnOn9ProbabilitiesRanges = new List<float> { 0.1f, 0.55f, 1.0f };
        spawnTypesBasedOnRoomY[9] = new EnemyTypeAndPercentageSpawnList(enemyTypesToSpawnOn9, enemyTypesToSpawnOn9ProbabilitiesRanges);


        MakeMap();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            MakeMap();
        }
    }

    private void MakeMap()
    {
        //curSeed = (int)Time.time;
        //Random.InitState(curSeed);
        roomObjectDictionary.Clear();
        groundTileMap.ClearAllTiles();
        wallsTileMap.ClearAllTiles();
        for (int i = 0; i < roomsParent.childCount; i++)
        {
            GameObject.Destroy(roomsParent.GetChild(i).gameObject);
        }

        List<Vector2Int> roomsSeen = new List<Vector2Int>();
        TraverseUniqueRoomsThroughConnections(playerSpawnRoom, Vector2Int.down + Vector2Int.left, ref roomsSeen, CreateConnectionsBetweenRooms);

        DecideCastleSpawnRoom();

        roomsSeen.Clear();
        TraverseUniqueRoomsThroughConnections(playerSpawnRoom, Vector2Int.down + Vector2Int.left, ref roomsSeen, CreateSectionsInCurrentRoom);              // V -> One section, full room.

        roomsSeen.Clear();
        TraverseUniqueRoomsThroughConnections(playerSpawnRoom, Vector2Int.down + Vector2Int.left, ref roomsSeen, DrawSectionsToTileMaps);                   // V -> Draw one section.

        roomsSeen.Clear();
        TraverseUniqueRoomsThroughConnections(playerSpawnRoom, Vector2Int.down + Vector2Int.left, ref roomsSeen, FormConnectionsBetweenSectionsInRoom);     // X -> No other sections to form connection with.

        roomsSeen.Clear();
        TraverseUniqueRoomsThroughConnections(playerSpawnRoom, Vector2Int.down + Vector2Int.left, ref roomsSeen, MakeConnectionsBetweenRoomsVisibleOnMap);  // V -> Need to draw connections to this room with everything else.


        roomsSeen.Clear();
        TraverseUniqueRoomsThroughConnections(playerSpawnRoom, Vector2Int.down + Vector2Int.left, ref roomsSeen, GenerateRoomGameObject);                   // V -> Generate information about room.

        roomsSeen.Clear();
        TraverseUniqueRoomsThroughConnections(playerSpawnRoom, Vector2Int.down + Vector2Int.left, ref roomsSeen, CreateResourcesInRooms);              // X -> No resources in this room? Or create towers for enemies here?

        int playerSpawnSectionIndex = 0;
        Vector2Int roomOffset = new Vector2Int((int)(playerSpawnRoom.x * numTilesInRooms.x), (int)(playerSpawnRoom.y * numTilesInRooms.y));
        Room playerSpawnRoomData = roomIndexAndRoom[playerSpawnRoom];
        for (int i = 0; i < playerSpawnRoomData.sections.Count; i++)
        {
            if (SectionContainsPointPadded(playerSpawnRoom, i, roomOffset + playerSpawnTile)){
                playerSpawnSectionIndex = i;
                break;
            }
        }

        Vector3 playerSpawnSectionCentre = playerSpawnRoomData.sections[playerSpawnSectionIndex].bottomLeft + playerSpawnRoomData.sections[playerSpawnSectionIndex].size * 0.5f;
        player.transform.position = new Vector3(playerSpawnRoom.x * numTilesInRooms.x + playerSpawnSectionCentre.x, playerSpawnRoom.y * numTilesInRooms.y + playerSpawnSectionCentre.y, 0.0f);
        playerSpawnTile = new Vector2Int((int)playerSpawnSectionCentre.x, (int)playerSpawnSectionCentre.y);

        roomsSeen.Clear();
        TraverseUniqueRoomsThroughConnections(playerSpawnRoom, Vector2Int.down + Vector2Int.left, ref roomsSeen, GenerateEnemiesForRoom);                   // V/X -> Pregenerate all the enemies that this room will need?

        roomsThatHaveBeenCreated.Clear();
        foreach (KeyValuePair<Vector2Int, Room> roomData in roomIndexAndRoom)
        {
            DebugRoom dRoom = new DebugRoom();
            dRoom.roomIndex = roomData.Key;
            dRoom.roomData = roomData.Value;
            roomsThatHaveBeenCreated.Add(dRoom);
        }
    }

    private void OnDrawGizmos()
    {
        if (roomIndexAndRoom.Count > 0)
        {
            List<Vector2Int> roomsSeen = new List<Vector2Int>();
            TraverseUniqueRoomsThroughConnections(playerSpawnRoom, Vector2Int.down + Vector2Int.left, ref roomsSeen, DrawRoomConnectionsGizmos);
            //TraverseRoomsThroughConnections(playerSpawnRoom, Vector2Int.down + Vector2Int.left, DrawSectionConnectionToRoomCentreGizmos);
            roomsSeen.Clear();
            TraverseUniqueRoomsThroughConnections(playerSpawnRoom, Vector2Int.down + Vector2Int.left, ref roomsSeen, DrawSectionsGizmos);

            roomsSeen.Clear();
            TraverseUniqueRoomsThroughConnections(playerSpawnRoom, Vector2Int.down + Vector2Int.left, ref roomsSeen, DrawAllPossibleConnectionsBetweenSectionsInRoom);

            //roomsSeen.Clear();
            //TraverseUniqueRoomsThroughConnections(playerSpawnRoom, Vector2Int.down + Vector2Int.left, ref roomsSeen, PaintRoomEntryExitSections);
        }
    }

    private void TraverseUniqueRoomsThroughConnections(Vector2Int roomWhoseChildrenNeedToBeTraversed, Vector2Int parentRoomIndex, ref List<Vector2Int> roomsAlreadySeen, System.Func<Vector2Int, bool> FunctionToRunUponReacingARoom)
    {
        roomsAlreadySeen.Add(roomWhoseChildrenNeedToBeTraversed);
        FunctionToRunUponReacingARoom(roomWhoseChildrenNeedToBeTraversed);

        Room curRoom = roomIndexAndRoom[roomWhoseChildrenNeedToBeTraversed];
        if (curRoom.connectedRooms.Count <= 0)
        {
            return;
        }
        else
        {
            for (int i = 0; i < curRoom.connectedRooms.Count; i++)
            {
                Vector2Int curConnectedRoom = curRoom.connectedRooms[i];

                if (!roomsAlreadySeen.Contains(curConnectedRoom))
                {
                    TraverseUniqueRoomsThroughConnections(curConnectedRoom, roomWhoseChildrenNeedToBeTraversed, ref roomsAlreadySeen, FunctionToRunUponReacingARoom);
                }
            }
        }
    }

    public void SetGroundTileToSand(Vector3Int tilePositionOnTilesMap)
    {
        groundTileMap.SetTile(tilePositionOnTilesMap, groundTileDrySand);
    }

    public bool IsTileSand(Vector3Int tilePositionOnTileMap)
    {
        return groundTileMap.GetTile(tilePositionOnTileMap) == groundTileDrySand;
    }

    public bool IsTileGrass(Vector3Int tilePositionOnTileMap)
    {
        return groundTileMap.GetTile(tilePositionOnTileMap) == groundTileGrassMM;
    }

    private void DecideCastleSpawnRoom()
    {
        if (roomIndexAndRoom.ContainsKey(castleSpawnRoom))
        {
            return;
        }

        bool roomFound = false;
        for (int y = mapSizeInRooms.y - 1; y >= 0; y--)
        {
            for (int x = mapSizeInRooms.x; x >= 0; x--)
            {
                Vector2Int checkRoom = new Vector2Int(x, y);
                if (roomIndexAndRoom.ContainsKey(checkRoom))
                {
                    castleSpawnRoom = checkRoom;
                    break;
                }
            }
            if (roomFound)
            {
                break;
            }
        }
    }

    private bool CreateResourcesInRooms(Vector2Int currentRoomIndex)
    {
        Room curRoom = roomIndexAndRoom[currentRoomIndex];
        Vector3 roomOffset = new Vector3(currentRoomIndex.x * numTilesInRooms.x, currentRoomIndex.y * numTilesInRooms.y, 0.0f);

        if (currentRoomIndex == castleSpawnRoom)
        {
            for (int i = 0; i < goblinTowerSpawnerPositions.Count; i++)
            {
                GameObject goblinSpawnTower = Instantiate(goblinSpawnTowerPrefab, roomOffset + new Vector3(goblinTowerSpawnerPositions[i].x, goblinTowerSpawnerPositions[i].y, 0.0f), Quaternion.identity, roomObjectDictionary[currentRoomIndex].transform);
                goblinTowers.Add(goblinSpawnTower.transform);
            }

            return true;
        }

        for (int i = 0; i < curRoom.sections.Count; i++)
        {
            if (curRoom.sections[i].size.x <= dimsForRecourceRooms.x || curRoom.sections[i].size.y <= dimsForRecourceRooms.y)
            {
                if (curRoom.sections[i].size.x * curRoom.sections[i].size.y <= maxNumTilesForResourceRooms)
                {
                    Transform goldMinesParentGameObjectTransform;
                    if (roomObjectDictionary[currentRoomIndex].transform.childCount == 1)
                    {
                        goldMinesParentGameObjectTransform = Instantiate(goldMinesParentPrefab, roomObjectDictionary[currentRoomIndex].transform).transform;
                    }
                    else
                    {
                        goldMinesParentGameObjectTransform = roomObjectDictionary[currentRoomIndex].transform.GetChild(1);
                    }

                    curRoom.sections[i].mineSection = true;
                    Vector3 curSectionCentre = curRoom.sections[i].bottomLeft + curRoom.sections[i].size * 0.5f;
                    Vector3 minePos = curSectionCentre + roomOffset;
                    GameObject mineObject = Instantiate(minesObjectPrefab, minePos, Quaternion.identity, goldMinesParentGameObjectTransform);
                    //PaintSectionToSand(currentRoomIndex, curRoom.sections[i]);

                    mineTilePositions.Add(minePos);
                }
            }
        }

        List<Vector2> occupiedPositions = new List<Vector2>();

        Vector2 dimsForTreesAndBushesRooms = dimsForRecourceRooms + Vector2.one;
        for (int i = 0; i < curRoom.sections.Count; i++)
        {
            occupiedPositions.Clear();
            if (curRoom.sections[i].size.x >= dimsForTreesAndBushesRooms.x && curRoom.sections[i].size.y >= dimsForTreesAndBushesRooms.y)
            {
                if (curRoom.sections[i].size.x * curRoom.sections[i].size.y > minNumberOfTilesForTreesAndBushes)
                {
                    Transform structuresInRoomParentGameObjectTransform;
                    if (roomObjectDictionary[currentRoomIndex].transform.childCount == 1)
                    {
                        structuresInRoomParentGameObjectTransform = Instantiate(goldMinesParentPrefab, roomObjectDictionary[currentRoomIndex].transform).transform;
                    }
                    else
                    {
                        structuresInRoomParentGameObjectTransform = roomObjectDictionary[currentRoomIndex].transform.GetChild(1);
                    }

                    int maxNumBushesInSection = (int)curRoom.sections[i].size.x * (int)curRoom.sections[i].size.y / 50;
                    int maxNumTreesInSection = (int)curRoom.sections[i].size.x * (int)curRoom.sections[i].size.y / 20;

                    int insetPositionsInsideSectionBy = 3;
                    //for (int bush = 0; bush < Mathf.Min(generateBushesInThisSection, maxNumBushesInSection); bush++)
                    for (int bush = 0; bush < maxNumBushesInSection; bush++)
                    {
                        Vector2 curBushPosition = ReturnRandomTileInSectionPadded(currentRoomIndex, i, insetPositionsInsideSectionBy);
                        if (!occupiedPositions.Contains(curBushPosition))
                        {
                            GameObject bushObject = Instantiate(bushesObjectPrefab, curBushPosition, Quaternion.identity, structuresInRoomParentGameObjectTransform);
                            bushObject.GetComponent<SpriteRenderer>().sortingOrder = (int)bushObject.transform.position.y * -1;
                            occupiedPositions.Add(curBushPosition);
                        }
                    }

                    //for (int tree = 0; tree < Mathf.Min(generateTreeInThisSection, maxNumTreesInSection); tree++)
                    for (int tree = 0; tree < maxNumTreesInSection; tree++)
                    {
                        Vector2 curTreePosition = ReturnRandomTileInSectionPadded(currentRoomIndex, i, insetPositionsInsideSectionBy);
                        if (!occupiedPositions.Contains(curTreePosition))
                        {
                            GameObject treeObject = Instantiate(treesObjectPrefab, curTreePosition, Quaternion.identity, structuresInRoomParentGameObjectTransform);
                            treeObject.GetComponent<SpriteRenderer>().sortingOrder = (int)treeObject.transform.position.y * -1;
                            occupiedPositions.Add(curTreePosition);
                        }
                    }

                    //GameObject treeObject = Instantiate(treesObjectPrefab, curSectionCentre + roomOffset, Quaternion.identity, structuresInRoomParentGameObjectTransform);
                    //PaintSectionToSand(currentRoomIndex, curRoom.sections[i]);
                }
            }
        }

        return true;
    }

    private bool FormConnectionsBetweenSectionsInRoom(Vector2Int curRoomIndex)
    {
        if (curRoomIndex == castleSpawnRoom)
        {
            return true;
        }

        Room curRoom = roomIndexAndRoom[curRoomIndex];
        Vector3 roomOffset = new Vector3(curRoomIndex.x * numTilesInRooms.x, curRoomIndex.y * numTilesInRooms.y, 0.0f);

        // Init section connections count to 0.
        List<int> numConnectionsPerSection = new List<int>();
        for (int i = 0; i < curRoom.sections.Count; i++)
        {
            numConnectionsPerSection.Add(0);
        }

        for (int i = 0; i < curRoom.sections.Count; i++)
        {
            //if ((curRoom.entryExitSectionIndex.Contains(i)) || (Random.Range(0, 2) == 0) || (numConnectionsPerSection[i] <= maxNumConnectionsPerNormalSection) || true)
            {
                for (int j = i + 1; j < curRoom.sections.Count; j++)
                {
                    List<ConnectionTiles> connectionTiles = new List<ConnectionTiles>();
                    if (CanSectionsBeConnected(curRoom.sections[i], curRoom.sections[j], new Vector3Int((int)roomOffset.x, (int)roomOffset.y, 0), ref connectionTiles) && connectionTiles.Count > 0)
                    {
                        int randomTileToMakeConnection = Random.Range(0, connectionTiles.Count);

                        if (drawConnnectionsInSand)
                        {
                            groundTileMap.SetTile(connectionTiles[randomTileToMakeConnection].worldTileA, groundTileDrySand);
                            groundTileMap.SetTile(connectionTiles[randomTileToMakeConnection].worldTileB, groundTileDrySand);
                        }
                        else
                        {
                            groundTileMap.SetTile(connectionTiles[randomTileToMakeConnection].worldTileA, groundTileGrassMM);
                            groundTileMap.SetTile(connectionTiles[randomTileToMakeConnection].worldTileB, groundTileGrassMM);
                        }


                        wallsTileMap.SetTile(connectionTiles[randomTileToMakeConnection].worldTileA, null);
                        wallsTileMap.SetTile(connectionTiles[randomTileToMakeConnection].worldTileB, null);

                        curRoom.sections[i].connectedSectionsInThisRoom.Add(j);
                        curRoom.sections[j].connectedSectionsInThisRoom.Add(i);

                        numConnectionsPerSection[i]++;
                        numConnectionsPerSection[j]++;
                    }

                    //for (int k = 0; k < connectionTiles.Count; k++)
                    //{
                    //    groundTileMap.SetTile(connectionTiles[k].worldTileA, groundTileDrySand);
                    //    groundTileMap.SetTile(connectionTiles[k].worldTileB, groundTileDrySand);
                    //}
                }
            }
        }

        return true;
    }

    private bool DrawAllPossibleConnectionsBetweenSectionsInRoom(Vector2Int curRoomIndex)
    {
        Room curRoom = roomIndexAndRoom[curRoomIndex];
        Vector3 roomOffset = new Vector3(curRoomIndex.x * numTilesInRooms.x, curRoomIndex.y * numTilesInRooms.y, 0.0f);

        for (int i = 0; i < curRoom.sections.Count; i++)
        {
            for (int j = i; j < curRoom.sections.Count; j++)
            {
                List<ConnectionTiles> connectionTiles = new List<ConnectionTiles>();
                if (CanSectionsBeConnected(curRoom.sections[i], curRoom.sections[j], new Vector3Int((int)roomOffset.x, (int)roomOffset.y, 0), ref connectionTiles))
                {
                    Vector3 sectionACentred = new Vector3(curRoom.sections[i].bottomLeft.x + curRoom.sections[i].size.x / 2, curRoom.sections[i].bottomLeft.y + curRoom.sections[i].size.y / 2, 0.0f);
                    Vector3 sectionBCentred = new Vector3(curRoom.sections[j].bottomLeft.x + curRoom.sections[j].size.x / 2, curRoom.sections[j].bottomLeft.y + curRoom.sections[j].size.y / 2, 0.0f);

                    sectionACentred += roomOffset;
                    sectionBCentred += roomOffset;

                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(sectionACentred, sectionBCentred);
                }
            }
        }

        return true;
    }

    private bool CanSectionsBeConnected(Section a, Section b, Vector3Int roomOffset, ref List<ConnectionTiles> connectionTiles)
    {
        return (SectionsShareAHorizontalBorder(a, b, roomOffset, ref connectionTiles) > 0) || (SectionsShareAVerticalBorder(a, b, roomOffset, ref connectionTiles) > 0);
    }

    private int SectionsShareAVerticalBorder(Section a, Section b, Vector3Int roomOffset, ref List<ConnectionTiles> connectionTiles)
    {
        Vector2 bottomLeftA = a.bottomLeft;
        Vector2 topLeftA = a.bottomLeft + Vector2.up * a.size.y + Vector2.down;
        Vector2 bottomRightA = a.bottomLeft + Vector2.right * a.size.x + Vector2.left;
        Vector2 topRightA = a.bottomLeft + a.size + Vector2.one * -1.0f;

        Vector2 bottomLeftB = b.bottomLeft;
        Vector2 topLeftB = b.bottomLeft + Vector2.up * b.size.y + Vector2.down;
        Vector2 bottomRightB = b.bottomLeft + Vector2.right * b.size.x + Vector2.left;
        Vector2 topRightB = b.bottomLeft + b.size + Vector2.down * -1.0f;


        if (Mathf.Abs(bottomRightA.x - bottomLeftB.x) == 1)
        {
            if((bottomRightA.y >= bottomLeftB.y) && (bottomRightA.y <= topLeftB.y)
                || (topRightA.y >= bottomLeftB.y) && (topRightA.y <= topLeftB.y))
            {
                NeighbouringTilesForSectionsVerticalBorder(topRightA, bottomRightA, topLeftB, bottomLeftB, a, b, roomOffset, ref connectionTiles);
                return 1;
            }
            else
            {
                return -1;
            }
        }

        if (Mathf.Abs(bottomLeftA.x - bottomRightB.x) == 1)
        {
            if ((bottomLeftA.y >= bottomRightB.y) && (bottomLeftA.y <= topRightB.y)
                || (topLeftA.y >= bottomRightB.y) && (topLeftA.y <= topRightB.y))
            {
                NeighbouringTilesForSectionsVerticalBorder(topLeftA, bottomLeftA, topRightB, bottomRightB, a, b, roomOffset, ref connectionTiles);
                return 2;
            }
            else
            {
                return -2;
            }
        }

        return 0;
    }

    private int SectionsShareAHorizontalBorder(Section a, Section b, Vector3Int roomOffset, ref List<ConnectionTiles> connectionTiles)
    {
        Vector2 bottomLeftA = a.bottomLeft;
        Vector2 topLeftA = a.bottomLeft + Vector2.up * a.size.y + Vector2.down;
        Vector2 bottomRightA = a.bottomLeft + Vector2.right * a.size.x + Vector2.left;
        Vector2 topRightA = a.bottomLeft + a.size + Vector2.one * -1.0f;

        Vector2 bottomLeftB = b.bottomLeft;
        Vector2 topLeftB = b.bottomLeft + Vector2.up * b.size.y + Vector2.down;
        Vector2 bottomRightB = b.bottomLeft + Vector2.right * b.size.x + Vector2.left;
        Vector2 topRightB = b.bottomLeft + b.size + Vector2.down * -1.0f;

        if (Mathf.Abs(topLeftA.y - bottomLeftB.y) == 1)
        {
            if ((topRightA.x >= bottomLeftB.x) && (topRightA.x <= bottomRightB.x)
                || (topLeftA.x >= bottomLeftB.x) && (topLeftA.x <= bottomRightB.x))
            {
                NeighbouringTilesForSectionsHorizontalBorder(topLeftA, topRightA, bottomLeftB, bottomRightB, a, b, roomOffset, ref connectionTiles);
                return 1;
            }
            else
            {
                return -1;
            }

        }

        if (Mathf.Abs(bottomLeftA.y - topLeftB.y) == 1)
        {
            if ((bottomRightA.x >= topLeftB.x) && (bottomRightA.x <= topRightB.x)
                || (bottomLeftA.x >= topLeftB.x) && (bottomLeftA.x <= topRightB.x))
            {
                NeighbouringTilesForSectionsHorizontalBorder(bottomLeftA, bottomRightA, topLeftB, topRightA, a, b, roomOffset, ref connectionTiles);
                return 2;
            }
            else
            {
                return -2;
            }

        }

        return 0;
    }

    public bool SectionContainsPointPadded(Vector2Int currentRoomIndex, int sectionIndex, Vector2 point)
    {
        Vector2 roomOffset = new Vector2(currentRoomIndex.x * numTilesInRooms.x, currentRoomIndex.y * numTilesInRooms.y);
        Room curRoom = roomIndexAndRoom[currentRoomIndex];
        Section currentSection = curRoom.sections[sectionIndex];

        Vector2 bottomLeft = currentSection.bottomLeft + Vector2.one;
        //Vector2 topLeft = currentSection.bottomLeft + Vector2.up * currentSection.size.y + Vector2.down;
        //Vector2 bottomRight = currentSection.bottomLeft + Vector2.right * currentSection.size.x + Vector2.left;
        Vector2 topRight = currentSection.bottomLeft + currentSection.size + Vector2.one * -1.0f;

        bottomLeft += roomOffset;
        topRight += roomOffset;

        return point.x >= bottomLeft.x && point.x <= topRight.x && point.y >= bottomLeft.y && point.y <= topRight.y;
    }

    //public Vector2Int ReturnRandomTileInSectionPaddedAwayFrom

    public Vector2Int ReturnRandomTileInSectionPadded(Vector2Int currentRoomIndex, int sectionIndex)
    {
        Vector2 roomOffset = new Vector2(currentRoomIndex.x * numTilesInRooms.x, currentRoomIndex.y * numTilesInRooms.y);
        Room curRoom = roomIndexAndRoom[currentRoomIndex];
        Section currentSection = curRoom.sections[sectionIndex];

        Vector2 bottomLeft = currentSection.bottomLeft + Vector2.one * 2.0f;
        //Vector2 topLeft = currentSection.bottomLeft + Vector2.up * currentSection.size.y + Vector2.down;
        //Vector2 bottomRight = currentSection.bottomLeft + Vector2.right * currentSection.size.x + Vector2.left;
        Vector2 topRight = currentSection.bottomLeft + currentSection.size + Vector2.one * -2.0f;

        bottomLeft += roomOffset;
        topRight += roomOffset;

        return new Vector2Int(Random.Range((int)bottomLeft.x, (int)topRight.x), Random.Range((int)bottomLeft.y, (int)topRight.y));
    }

    public Vector2Int ReturnRandomTileInSectionPadded(Vector2Int currentRoomIndex, int sectionIndex, int padding)
    {
        Vector2 roomOffset = new Vector2(currentRoomIndex.x * numTilesInRooms.x, currentRoomIndex.y * numTilesInRooms.y);
        Room curRoom = roomIndexAndRoom[currentRoomIndex];
        Section currentSection = curRoom.sections[sectionIndex];

        Vector2 bottomLeft = currentSection.bottomLeft + Vector2.one * padding;
        //Vector2 topLeft = currentSection.bottomLeft + Vector2.up * currentSection.size.y + Vector2.down;
        //Vector2 bottomRight = currentSection.bottomLeft + Vector2.right * currentSection.size.x + Vector2.left;
        Vector2 topRight = currentSection.bottomLeft + currentSection.size + Vector2.one * -padding;

        bottomLeft += roomOffset;
        topRight += roomOffset;

        return new Vector2Int(Random.Range((int)bottomLeft.x, (int)topRight.x), Random.Range((int)bottomLeft.y, (int)topRight.y));
    }


    private bool IsTileACornerTileInSection(Section currentSection, Vector3Int tileToCheck)
    {
        Vector2 tile = new Vector2(tileToCheck.x, tileToCheck.y);

        Vector2 bottomLeft = currentSection.bottomLeft;
        Vector2 topLeft = currentSection.bottomLeft + Vector2.up * currentSection.size.y + Vector2.down;
        Vector2 bottomRight = currentSection.bottomLeft + Vector2.right * currentSection.size.x + Vector2.left;
        Vector2 topRight = currentSection.bottomLeft + currentSection.size + Vector2.one * -1.0f;

        return (tile == bottomLeft) || (tile == topLeft) || (tile == bottomRight) || (tile == topRight); 
    }

    private void NeighbouringTilesForSectionsVerticalBorder(Vector2 topA, Vector2 bottomA, Vector2 topB, Vector2 bottomB, Section a, Section b, Vector3Int roomOffset, ref List<ConnectionTiles> tilesThatAllowConnection)
    {
        int minimumTopY = (int)Mathf.Min(topA.y, topB.y);
        int maximumBottomY = (int)Mathf.Max(bottomA.y, bottomB.y);

        for (int i = maximumBottomY; i <= minimumTopY; i++)
        {
            Vector3Int aTileIndex = new Vector3Int((int)bottomA.x, i, 0);
            Vector3Int bTileIndex = new Vector3Int((int)bottomB.x, i, 0);

            //if ((!IsTileACornerTileInSection(a, aTileIndex) && !IsTileACornerTileInSection(b, bTileIndex)) || true)
            if (!IsTileACornerTileInSection(a, aTileIndex) && !IsTileACornerTileInSection(b, bTileIndex))
            {
                ConnectionTiles curConnectionTile = new ConnectionTiles();
                curConnectionTile.worldTileA = aTileIndex + roomOffset;
                curConnectionTile.worldTileB = bTileIndex + roomOffset;

                tilesThatAllowConnection.Add(curConnectionTile);
                //groundTileMap.SetTile(new Vector3Int((int)bottomA.x, i, 0) + roomAOffset, groundTileDrySand);
                //groundTileMap.SetTile(new Vector3Int((int)bottomB.x, i, 0) + roomBOffset, groundTileDrySand);
            }
        }
    }

    private void NeighbouringTilesForSectionsHorizontalBorder(Vector2 topLeftA, Vector2 topRightA, Vector2 bottomLeftB, Vector2 bottomRightB, Section a, Section b, Vector3Int roomOffset, ref List<ConnectionTiles> tilesThatAllowConnection)
    {
        int minimumRightX = (int)Mathf.Min(topRightA.x, bottomRightB.x);
        int maximumLeftX = (int)Mathf.Max(topLeftA.x, bottomLeftB.x);

        for (int i = maximumLeftX; i <= minimumRightX; i++)
        {
            Vector3Int aTileIndex = new Vector3Int(i, (int)topLeftA.y, 0);
            Vector3Int bTileIndex = new Vector3Int(i, (int)bottomLeftB.y, 0);

            //if ((!IsTileACornerTileInSection(a, aTileIndex) && !IsTileACornerTileInSection(b, bTileIndex)) || true)
            if (!IsTileACornerTileInSection(a, aTileIndex) && !IsTileACornerTileInSection(b, bTileIndex))
            {
                ConnectionTiles curConnectionTile = new ConnectionTiles();
                curConnectionTile.worldTileA = aTileIndex + roomOffset;
                curConnectionTile.worldTileB = bTileIndex + roomOffset;

                tilesThatAllowConnection.Add(curConnectionTile);
                //groundTileMap.SetTile(new Vector3Int(i, (int)topLeftA.y, 0) + roomAOffset, groundTileDrySand);
                //groundTileMap.SetTile(new Vector3Int(i, (int)bottomLeftB.y, 0) + roomBOffset, groundTileDrySand);
            }

        }
    }

    private bool MakeConnectionsBetweenRoomsVisibleOnMap(Vector2Int currentRoomIndex)
    {
        Room curRoom = roomIndexAndRoom[currentRoomIndex];

        Vector3Int curRoomOffsetInTiles = new Vector3Int(currentRoomIndex.x * numTilesInRooms.x, currentRoomIndex.y * numTilesInRooms.y, 0);
        for (int i = 0; i < curRoom.connectedRooms.Count; i++)
        {
            int curConnectedRoomIsLeftRightUp = ConnectedRoomIsLeftRightUp(currentRoomIndex, curRoom.connectedRooms[i]);

            TilesAvailableForConnectingRooms(currentRoomIndex, curRoom.connectedRooms[i], curConnectedRoomIsLeftRightUp);





            //Vector3Int curConnectedRoomOffsetInTiles = new Vector3Int(curRoom.connectedRooms[i].x * numTilesInRooms.x, curRoom.connectedRooms[i].y * numTilesInRooms.y, 0);
            //foreach (KeyValuePair<Vector2Int, Vector2Int> connectedTiles in tilesThatCanConnect)
            //{
            //    groundTileMap.SetTile(new Vector3Int(connectedTiles.Key.x, connectedTiles.Key.y, 0) + curRoomOffsetInTiles, groundTileDrySand);
            //    groundTileMap.SetTile(new Vector3Int(connectedTiles.Value.x, connectedTiles.Value.y, 0) + curConnectedRoomOffsetInTiles, groundTileDrySand);
            //}
        }

        return true;
    }

    private void TilesAvailableForConnectingRooms(Vector2Int roomAIndex, Vector2Int roomBIndex, int curConnectedRoomIsLeftRightUp)
    {
        if(curConnectedRoomIsLeftRightUp == left)
        {
            List<int> sectionsOfRoomAFacingB = SectionsWithRoomWallOfSide(roomAIndex, SectionBorderingWallType.Left);
            List<int> sectionsOfRoomBFacingA = SectionsWithRoomWallOfSide(roomBIndex, SectionBorderingWallType.Right);

            //for (int i = 0; i < sectionsOfRoomAFacingB.Count; i++)
            //{
            //    PaintSectionToSand(roomAIndex, roomIndexAndRoom[roomAIndex].sections[sectionsOfRoomAFacingB[i]]);
            //}

            //for (int i = 0; i < sectionsOfRoomBFacingA.Count; i++)
            //{
            //    PaintSectionToSand(roomBIndex, roomIndexAndRoom[roomBIndex].sections[sectionsOfRoomBFacingA[i]]);
            //}

            TilesThatConnectInSectionLists(roomAIndex, roomBIndex, curConnectedRoomIsLeftRightUp, sectionsOfRoomAFacingB, sectionsOfRoomBFacingA);
        }
        else if(curConnectedRoomIsLeftRightUp == right)
        {
            List<int> sectionsOfRoomAFacingB = SectionsWithRoomWallOfSide(roomAIndex, SectionBorderingWallType.Right);
            List<int> sectionsOfRoomBFacingA = SectionsWithRoomWallOfSide(roomBIndex, SectionBorderingWallType.Left);

            //for (int i = 0; i < sectionsOfRoomAFacingB.Count; i++)
            //{
            //    PaintSectionToSand(roomAIndex, roomIndexAndRoom[roomAIndex].sections[sectionsOfRoomAFacingB[i]]);
            //}

            //for (int i = 0; i < sectionsOfRoomBFacingA.Count; i++)
            //{
            //    PaintSectionToSand(roomBIndex, roomIndexAndRoom[roomBIndex].sections[sectionsOfRoomBFacingA[i]]);
            //}

            TilesThatConnectInSectionLists(roomAIndex, roomBIndex, curConnectedRoomIsLeftRightUp, sectionsOfRoomAFacingB, sectionsOfRoomBFacingA);
        }
        else if(curConnectedRoomIsLeftRightUp == up)
        {
            List<int> sectionsOfRoomAFacingB = SectionsWithRoomWallOfSide(roomAIndex, SectionBorderingWallType.Top);
            List<int> sectionsOfRoomBFacingA = SectionsWithRoomWallOfSide(roomBIndex, SectionBorderingWallType.Bottom);

            TilesThatConnectInSectionLists(roomAIndex, roomBIndex, curConnectedRoomIsLeftRightUp, sectionsOfRoomAFacingB, sectionsOfRoomBFacingA);
        }
    }

    private void PaintSectionToSand(Vector2Int curRoomIndex, Section curSection)
    {
        Vector3Int bottomLeft = new Vector3Int((int)curSection.bottomLeft.x, (int)curSection.bottomLeft.y, 0);
        Vector3Int roomIndexOffset = new Vector3Int(curRoomIndex.x * numTilesInRooms.x, curRoomIndex.y * numTilesInRooms.y, 0);

        for (int x = bottomLeft.x; x < bottomLeft.x + curSection.size.x; x++)
        {
            for (int y = bottomLeft.y; y < bottomLeft.y + curSection.size.y; y++)
            {
                groundTileMap.SetTile(new Vector3Int(x, y, 0) + roomIndexOffset, groundTileDrySand);
            }
        }
    }

    private bool PaintRoomEntryExitSections(Vector2Int currentRoomIndex)
    {
        Room currentRoom = roomIndexAndRoom[currentRoomIndex];
        for (int i = 0; i < currentRoom.entryExitSectionIndex.Count; i++)
        {
            PaintSectionToSand(currentRoomIndex, currentRoom.sections[currentRoom.entryExitSectionIndex[i]]);
        }
        return true;
    }

    private void TilesThatConnectInSectionLists(Vector2Int roomAIndex, Vector2Int roomBIndex, int curConnectedRoomIsLeftRightUp, List<int> sectionsATouchingB, List<int> sectionsBTouchingA)
    {
        Room roomA = roomIndexAndRoom[roomAIndex];
        Room roomB = roomIndexAndRoom[roomBIndex];

        Vector3Int roomAOffset = new Vector3Int(roomAIndex.x * numTilesInRooms.x, roomAIndex.y * numTilesInRooms.y, 0);
        Vector3Int roomBOffset = new Vector3Int(roomBIndex.x * numTilesInRooms.x, roomBIndex.y * numTilesInRooms.y, 0);

        List<ConnectionTiles> tilesThatAllowConnection = new List<ConnectionTiles>();

        for (int i = 0; i < sectionsATouchingB.Count; i++)
        {
            Section curSectionA = roomA.sections[sectionsATouchingB[i]];

            for (int j = 0; j < sectionsBTouchingA.Count; j++)
            {
                Section curSectionB = roomB.sections[sectionsBTouchingA[j]];
                ConnectingTilesOfTwoSections(curSectionA, curSectionB, roomAOffset, roomBOffset, curConnectedRoomIsLeftRightUp, ref tilesThatAllowConnection);
            }
        }

        int randConnectionTile = Random.Range(0, tilesThatAllowConnection.Count);

        Vector3Int tileA = tilesThatAllowConnection[randConnectionTile].worldTileA;
        Vector3Int tileB = tilesThatAllowConnection[randConnectionTile].worldTileB;

        if (drawConnnectionsInSand)
        {
            groundTileMap.SetTile(tileA, groundTileDrySand);
            groundTileMap.SetTile(tileB, groundTileDrySand);
        }
        else
        {
            groundTileMap.SetTile(tileA, groundTileGrassMM);
            groundTileMap.SetTile(tileB, groundTileGrassMM);
        }

        wallsTileMap.SetTile(tileA, null);
        wallsTileMap.SetTile(tileB, null);

        for (int i = 0; i < sectionsATouchingB.Count; i++)
        {
            Section curSectionA = roomA.sections[sectionsATouchingB[i]];
            if (TileBelongsToSection(tileA, curSectionA, roomAOffset))
            {
                roomA.entryExitSectionIndex.Add(sectionsATouchingB[i]);
            }
        }

        for (int i = 0; i < sectionsBTouchingA.Count; i++)
        {
            Section curSectionB = roomB.sections[sectionsBTouchingA[i]];
            if (TileBelongsToSection(tileB, curSectionB, roomBOffset))
            {
                roomB.entryExitSectionIndex.Add(sectionsBTouchingA[i]);
            }
        }

        //groundTileMap.SetTile(tilesThatAllowConnection[randConnectionTile].worldTileA, groundTileGrassMM);
        //groundTileMap.SetTile(tilesThatAllowConnection[randConnectionTile].worldTileB, groundTileGrassMM);
    }

    private bool TileBelongsToSection(Vector3Int currentTileIndexWorld, Section curSection, Vector3Int currentRoomOffset)
    {
        Vector3Int currentSectionBottomLeftWorldPos = new Vector3Int((int)curSection.bottomLeft.x + currentRoomOffset.x, (int)curSection.bottomLeft.y + currentRoomOffset.y, 0);

        return (currentTileIndexWorld.x >= currentSectionBottomLeftWorldPos.x
            && currentTileIndexWorld.x <= currentSectionBottomLeftWorldPos.x + curSection.size.x
            && currentTileIndexWorld.y >= currentSectionBottomLeftWorldPos.y
            && currentTileIndexWorld.y <= currentSectionBottomLeftWorldPos.y + curSection.size.y);
    }

    private void ConnectingTilesOfTwoSections(Section a, Section b, Vector3Int roomAOffset, Vector3Int roomBOffset, int curConnectedRoomIsLeftRightUp, ref List<ConnectionTiles> tilesThatAllowConnection)
    {
        // Why adding an offset of Vector2.left to the code below works? Hell if I know. But it does. So I won't touch it. Ah...its the tile that becomes a wall and the fafct that size is one greater than that?

        if (curConnectedRoomIsLeftRightUp == left)
        {
            Vector3 bottomLeftA = a.bottomLeft + Vector2.up + Vector2.left;
            Vector3 topLeftA = a.bottomLeft + new Vector2(0.0f, a.size.y) + Vector2.down + Vector2.down + Vector2.left;

            Vector3 bottomRightB = b.bottomLeft + new Vector2(b.size.x, 0.0f) + Vector2.up;
            Vector3 topRightB = b.bottomLeft + b.size + Vector2.down + Vector2.down;

            NeighbouringTilesLR(topLeftA, bottomLeftA, topRightB, bottomRightB, roomAOffset, roomBOffset, curConnectedRoomIsLeftRightUp, ref tilesThatAllowConnection);
        }
        else if (curConnectedRoomIsLeftRightUp == right)
        {
            Vector3 bottomRightA = a.bottomLeft + new Vector2(a.size.x, 0.0f) + Vector2.up + Vector2.left;
            Vector3 topRightA = a.bottomLeft + a.size + Vector2.down + Vector2.down + Vector2.left;

            Vector3 bottomLeftB = b.bottomLeft + Vector2.up;
            Vector3 topLeftB = b.bottomLeft + new Vector2(0.0f, b.size.y) + Vector2.down + Vector2.down;

            NeighbouringTilesLR(topRightA, bottomRightA, topLeftB, bottomLeftB, roomAOffset, roomBOffset, curConnectedRoomIsLeftRightUp, ref tilesThatAllowConnection);
        }
        else if (curConnectedRoomIsLeftRightUp == up)
        {
            Vector3 topLeftA = a.bottomLeft + new Vector2(0.0f, a.size.y) + Vector2.down + Vector2.right;
            Vector3 topRightA = a.bottomLeft + a.size + Vector2.down + Vector2.left + Vector2.left;

            Vector3 bottomLeftB = b.bottomLeft + Vector2.right;
            Vector3 bottomRightB = b.bottomLeft + new Vector2(b.size.x, 0.0f) + Vector2.left + Vector2.left;

            NeighbouringTilesU(topLeftA, topRightA, bottomLeftB, bottomRightB, roomAOffset, roomBOffset, curConnectedRoomIsLeftRightUp, ref tilesThatAllowConnection);
        }
    }

    private void NeighbouringTilesLR(Vector3 topA, Vector3 bottomA, Vector3 topB, Vector3 bottomB, Vector3Int roomAOffset, Vector3Int roomBOffset, int curConnectedRoomIsLeftRightUp, ref List<ConnectionTiles> tilesThatAllowConnection)
    {
        if(curConnectedRoomIsLeftRightUp == left || curConnectedRoomIsLeftRightUp == right)
        {
            int minimumTopY = (int)Mathf.Min(topA.y, topB.y);
            int maximumBottomY = (int)Mathf.Max(bottomA.y, bottomB.y);

            for (int i = maximumBottomY; i <= minimumTopY; i++)
            {
                ConnectionTiles curConnectionTile = new ConnectionTiles();
                curConnectionTile.worldTileA = new Vector3Int((int)bottomA.x, i, 0) + roomAOffset;
                curConnectionTile.worldTileB = new Vector3Int((int)bottomB.x, i, 0) + roomBOffset;

                tilesThatAllowConnection.Add(curConnectionTile);
                //groundTileMap.SetTile(new Vector3Int((int)bottomA.x, i, 0) + roomAOffset, groundTileDrySand);
                //groundTileMap.SetTile(new Vector3Int((int)bottomB.x, i, 0) + roomBOffset, groundTileDrySand);
            }
        }
    }

    private void NeighbouringTilesU(Vector3 topLeftA, Vector3 topRightA, Vector3 bottomLeftB, Vector3 bottomRightB, Vector3Int roomAOffset, Vector3Int roomBOffset, int curConnectedRoomIsLeftRightUp, ref List<ConnectionTiles> tilesThatAllowConnection)
    {
        if (curConnectedRoomIsLeftRightUp == up)
        {
            int minimumRightX = (int)Mathf.Min(topRightA.x, bottomRightB.x);
            int maximumLeftX = (int)Mathf.Max(topLeftA.x, bottomLeftB.x);

            for (int i = maximumLeftX; i <= minimumRightX; i++)
            {
                ConnectionTiles curConnectionTile = new ConnectionTiles();
                curConnectionTile.worldTileA = new Vector3Int(i, (int)topLeftA.y, 0) + roomAOffset;
                curConnectionTile.worldTileB = new Vector3Int(i, (int)bottomLeftB.y, 0) + roomBOffset;

                tilesThatAllowConnection.Add(curConnectionTile);
                //groundTileMap.SetTile(new Vector3Int(i, (int)topLeftA.y, 0) + roomAOffset, groundTileDrySand);
                //groundTileMap.SetTile(new Vector3Int(i, (int)bottomLeftB.y, 0) + roomBOffset, groundTileDrySand);
            }
        }
    }

    private List<int> SectionsWithRoomWallOfSide(Vector2Int currentRoomIndex, SectionBorderingWallType borderWallType)
    {
        List<int> sectionsThatShareBorderWallOfType = new List<int>();
        Room curRoom = roomIndexAndRoom[currentRoomIndex];
        for (int i = 0; i < curRoom.sections.Count; i++)
        {
            if (curRoom.sections[i].borderingWallTypes.Contains(borderWallType))
            {
                sectionsThatShareBorderWallOfType.Add(i);
            }
        }
        return sectionsThatShareBorderWallOfType;
    }

    private int ConnectedRoomIsLeftRightUp(Vector2Int currentRoomIndex, Vector2Int connectedRoomIndex)
    {
        if(currentRoomIndex.y == connectedRoomIndex.y)
        {
            if(currentRoomIndex.x < connectedRoomIndex.x)
            {
                return right;
            }
            else
            {
                return left;
            }
        }
        else
        {
            return up;
        }
    }

    private bool GenerateRoomGameObject(Vector2Int currentRoomIndex)
    {
        GameObject room = Instantiate(emptyRoomPrefab, roomsParent);
        room.name = "Room " + "(" + currentRoomIndex.x + ", " + currentRoomIndex.y + ")";
        room.transform.position = new Vector3(currentRoomIndex.x * numTilesInRooms.x, currentRoomIndex.y * numTilesInRooms.y, 0);
        room.transform.position += new Vector3(numTilesInRooms.x / 2, numTilesInRooms.y / 2, 0);
        room.transform.position += new Vector3(0.0f, 0.5f, 0.0f);
        roomObjectDictionary.Add(currentRoomIndex, room);

        GameObject curRoomBoundaryObject = Instantiate(currentRoomBoundaryObject, roomObjectDictionary[currentRoomIndex].transform);
        CameraTargetManager cameraTargetManager = curRoomBoundaryObject.GetComponent<CameraTargetManager>();
        cameraTargetManager.cineCamera = cineCamera;

        BoxCollider2D roomBoundaryCollider = curRoomBoundaryObject.GetComponent<BoxCollider2D>();
        roomBoundaryCollider.size = numTilesInRooms;
        return true;
    }

    private bool GenerateEnemiesForRoom(Vector2Int currentRoomIndex)
    {
        GameObject enemiesParentGameObject = Instantiate(enemiesParentPrefab, roomObjectDictionary[currentRoomIndex].transform);

        Room curRoom = roomIndexAndRoom[currentRoomIndex];

        Transform currentRoomBoundaryTransform = roomObjectDictionary[currentRoomIndex].transform.GetChild(0);
        CameraTargetManager curCameraTargetManager = currentRoomBoundaryTransform.GetComponent<CameraTargetManager>();
        curCameraTargetManager.enemiesParent = enemiesParentGameObject.transform;

        for (int i = 0; i < curRoom.sections.Count; i++)
        {
            Section curSection = curRoom.sections[i];

            GenerateEnemiesForSection(currentRoomIndex, i, curSection, curCameraTargetManager, ref enemiesParentGameObject);
            //Debug.Log("Spawned enemies for section.");
        }

        return true;
    }

    public void CreateCastleRoomEnemy(GameObject goblinToSpawn, Vector3 spawnPosition, Transform enemiesParentTransform, Vector2Int curRoomIndex, int sectionIndex, CameraTargetManager curCameraTargetManager)
    {
        GameObject enemyGameobject = Instantiate(goblinToSpawn, spawnPosition, Quaternion.identity, enemiesParentTransform);

        Enemy_Movement enemyMovementComponent = enemyGameobject.GetComponent<Enemy_Movement>();
        EnemyHealth enemyHealthComponent = enemyGameobject.GetComponent<EnemyHealth>();

        EnemyProperties s_EnemyProperties = enemyGameobject.GetComponent<EnemyProperties>();
        s_EnemyProperties.sectionIndex = sectionIndex;
        s_EnemyProperties.roomIndex = curRoomIndex;
        s_EnemyProperties.castleRoomEnemies = true;

        s_EnemyProperties.dynamiteShadowsParentTransform = curCameraTargetManager.dynamiteShadowsParent;

        enemyGameobject.SetActive(false);

        Section curSection = roomIndexAndRoom[curRoomIndex].sections[sectionIndex];
        curSection.enemiesPropertyComponentList.Add(s_EnemyProperties);
        curSection.enemiesMovementComponentList.Add(enemyMovementComponent);
        curSection.enemiesHealthComponentList.Add(enemyHealthComponent);
    }

    private void GenerateEnemiesForSection(Vector2Int curRoomIndex, int sectionIndex, Section curSection, CameraTargetManager curCameraTargetManager, ref GameObject enemiesParentGameObject)
    {
        //Vector2Int roomOffset = new Vector2Int(curRoomIndex.x * numTilesInRooms.x, curRoomIndex.y * numTilesInRooms.y);

        if (SectionContainsPointPadded(curRoomIndex, sectionIndex, (playerSpawnRoom * numTilesInRooms) + playerSpawnTile))
        {
            return;
        }

        if(curRoomIndex == castleSpawnRoom)
        {
            SpawnCastleEnemies s_SpawnCastleEnemies = GameObject.FindGameObjectWithTag("CastleSpawner").GetComponent<SpawnCastleEnemies>();
            s_SpawnCastleEnemies.enemiesParentTransform = enemiesParentGameObject.transform;
            s_SpawnCastleEnemies.cameraTargetManager = curCameraTargetManager;

            s_SpawnCastleEnemies.enemiesSpawnPoints.Clear();
            s_SpawnCastleEnemies.enemiesSpawnPoints = goblinTowers;

            for (int i = 0; i < s_SpawnCastleEnemies.enemiesSpawnPoints.Count; i++)
            {
                s_SpawnCastleEnemies.spawnPointHealths.Add(s_SpawnCastleEnemies.enemiesSpawnPoints[i].GetComponent<StructureHealth>());
            }

            //int numTorchGoblinsInCastleSpawnRoom = 30;
            //int numTNTBarrelGoblinsInCastleSpawnRoom = 15;
            //int numBombGoblinsInCastleSpawnRoom = 10;

            //for (int i = 0; i < numTorchGoblinsInCastleSpawnRoom; i++)
            //{
            //    CreateCastleRoomEnemy(torchGoblinEnemy, enemiesParentGameObject.transform, curRoomIndex, sectionIndex, curCameraTargetManager);
            //}
            //for (int i = 0; i < numTNTBarrelGoblinsInCastleSpawnRoom; i++)
            //{
            //    CreateCastleRoomEnemy(tntGoblinEnemy, enemiesParentGameObject.transform, curRoomIndex, sectionIndex, curCameraTargetManager);
            //}
            //for (int i = 0; i < numBombGoblinsInCastleSpawnRoom; i++)
            //{
            //    CreateCastleRoomEnemy(bombGoblinEnemy, enemiesParentGameObject.transform, curRoomIndex, sectionIndex, curCameraTargetManager);
            //}

            return;
        }

        int patrolStartPointIndex = 1;
        if(curSection.size.x <= 6.0f || curSection.size.y <= 6.0f)
        {
            patrolStartPointIndex = 0;
        }

        int numEnemiesInThisSection = 0;
        List<EnemyType> enemyTypesInThisSection = new List<EnemyType>();
        for (int i = 0; CanSpawnMoreEnemiesInThisSection(numEnemiesInThisSection, curSection.size, curRoomIndex.y) && i < curSection.patrolPoints.Count; i++)
        {
            //Debug.Log("Spawned enemies in section at corner.");

            int currentWayPointIndex = patrolStartPointIndex + i * 2;

            Vector3 curEnemyPos = new Vector3(curSection.patrolPoints[currentWayPointIndex].x, curSection.patrolPoints[currentWayPointIndex].y, 0.0f);
            curEnemyPos.x += 0.5f;
            curEnemyPos.y += 0.5f;

            //enemyPositions.Add(curEnemyPos);
            if (debugEnemy)
            {
                GameObject enemyGameobject = Instantiate(randomPrefabEnemy, curEnemyPos, Quaternion.identity, enemiesParentGameObject.transform);
                numEnemiesSpawned++;
                numEnemiesInThisSection++;
            }
            else
            {

                EnemyType curEnemyType = GetEnemyTypeForThisRoom(curRoomIndex);

                if (curEnemyType == EnemyType.TorchGoblin)
                {
                    goblinToSpawn = torchGoblinEnemy;
                }
                if (curEnemyType == EnemyType.BombGoblin)
                {
                    goblinToSpawn = bombGoblinEnemy;
                }
                if (curEnemyType == EnemyType.TNTBarrelGoblin)
                {
                    goblinToSpawn = tntGoblinEnemy;
                }

                GameObject enemyGameobject = Instantiate(goblinToSpawn, curEnemyPos, Quaternion.identity, enemiesParentGameObject.transform);
                numEnemiesSpawned++;
                //GameObject enemyGameobject = Instantiate(torchGoblinEnemy, curEnemyPos, Quaternion.identity, gameObject.transform);

                enemyGameobject.GetComponent<SpriteRenderer>().sortingOrder = 4;

                Enemy_Movement enemyMovementComponent = enemyGameobject.GetComponent<Enemy_Movement>();
                enemyMovementComponent.currentWayPointIndex = currentWayPointIndex;

                EnemyProperties s_EnemyProperties = enemyGameobject.GetComponent<EnemyProperties>();
                s_EnemyProperties.sectionIndex = sectionIndex;
                s_EnemyProperties.roomIndex = curRoomIndex;

                s_EnemyProperties.dynamiteShadowsParentTransform = curCameraTargetManager.dynamiteShadowsParent;

                if (curSection.mineSection)
                {
                    s_EnemyProperties.mineGuard = true;
                }
                else
                {
                    for (int k = 0; k < curSection.connectedSectionsInThisRoom.Count; k++)
                    {
                        if (roomIndexAndRoom[curRoomIndex].sections[curSection.connectedSectionsInThisRoom[k]].mineSection)
                        {
                            s_EnemyProperties.connectedToMineGuardSection = true;
                        }
                    }
                }

                enemyGameobject.SetActive(false);

                curSection.enemiesPropertyComponentList.Add(s_EnemyProperties);
                curSection.enemiesMovementComponentList.Add(enemyMovementComponent);

                EnemyHealth enemyHealthComponent = enemyGameobject.GetComponent<EnemyHealth>();
                curSection.enemiesHealthComponentList.Add(enemyHealthComponent);


                enemyTypesInThisSection.Add(s_EnemyProperties.enemyType);

                numEnemiesInThisSection++;
            }
        }
    }

    private EnemyType GetEnemyTypeForThisRoom(Vector2Int currentRoomIndex)
    {
        float randFloat = Random.Range(0.0f, 1.0f);

        foreach (float probability in spawnTypesBasedOnRoomY[currentRoomIndex.y].enemySpawnTypeAndProbabilityRanges.Keys)
        {
            if(probability > randFloat)
            {
                return spawnTypesBasedOnRoomY[currentRoomIndex.y].enemySpawnTypeAndProbabilityRanges[probability];
            }
        }

        Debug.Log("Failed to get an enemy type, defaulting to torch goblin.");
        return EnemyType.TorchGoblin;
    }

    public bool IsCurrentWayPointEmpty(Vector2Int currentRoomIndex, int currentSectionIndex, int wayPointIndex)
    {
        return roomIndexAndRoom[currentRoomIndex].sections[currentSectionIndex].patrolPointsOccupied[wayPointIndex];
    }

    public bool EmptyApproachingWayPoint(Vector2Int currentRoomIndex, int currentSectionIndex, int wayPointIndex, bool usingAllWayPoints, bool usingCorners)
    {
        usingCorners = usingAllWayPoints ? true : usingCorners;

        Room currentRoom = roomIndexAndRoom[currentRoomIndex];
        Section currentSection = currentRoom.sections[currentSectionIndex];

        int numPatrolPointsOccupied = 0;
        int offset = usingCorners ? 0 : 1;
        int iteration = usingAllWayPoints ? 1 : 2;
        for (int i = offset; i < currentSection.patrolPointsOccupied.Count; i += iteration)
        {
            if (currentSection.patrolPointsOccupied[i])
            {
                numPatrolPointsOccupied++;
            }
        }

        int totalNumberOfPatrolPoints = usingAllWayPoints ? currentSection.patrolPointsOccupied.Count : currentSection.patrolPointsOccupied.Count / 2;
        if(numPatrolPointsOccupied > totalNumberOfPatrolPoints / 2)
        {
            return false;
        }

        for (int i = 0; i < currentSection.enemiesMovementComponentList.Count; i++)
        {
            if (currentSection.enemiesMovementComponentList[i].currentWayPointIndex == wayPointIndex)
            {
                currentSection.enemiesMovementComponentList[i].SwitchToNextWayPointImmediately();
            }
        }

        return true;
    }

    private bool DrawSectionsToTileMaps(Vector2Int currentRoomIndex)
    {
        Room curRoom = roomIndexAndRoom[currentRoomIndex];
        for (int i = 0; i < curRoom.sections.Count; i++)
        {
            Section curSection = curRoom.sections[i];

            Vector3Int currentSectionBottomLeftTilesIndex = new Vector3Int(currentRoomIndex.x * numTilesInRooms.x + (int)curSection.bottomLeft.x, currentRoomIndex.y * numTilesInRooms.y + (int)curSection.bottomLeft.y, 0);

            for (int y = 0; y < curSection.size.y; y++)
            {
                for (int x = 0; x < curSection.size.x; x++)
                {
                    Vector3Int curTilePosInTileMapGrid = new Vector3Int(currentSectionBottomLeftTilesIndex.x + x, currentSectionBottomLeftTilesIndex.y + y, 0);

                    bool borderTile = false;
                    if (y == 0)
                    {
                        //Bottom border of the section.
                        //groundTileMap.SetTile(curTilePosInTileMapGrid, stoneWallTile);
                        wallsTileMap.SetTile(curTilePosInTileMapGrid, stoneWallTile);
                        borderTile = true;
                    }
                    else if (y == curSection.size.y - borderInset)
                    {
                        //Top border of the section.
                        //groundTileMap.SetTile(curTilePosInTileMapGrid, stoneWallTile);
                        wallsTileMap.SetTile(curTilePosInTileMapGrid, stoneWallTile);
                        borderTile = true;
                    }

                    if (x == 0)
                    {
                        //Left border of the section.
                        //groundTileMap.SetTile(curTilePosInTileMapGrid, stoneWallTile);
                        wallsTileMap.SetTile(curTilePosInTileMapGrid, stoneWallTile);
                        borderTile = true;
                    }
                    else if (x == curSection.size.x - borderInset)
                    {
                        //Right border of the section.
                        //groundTileMap.SetTile(curTilePosInTileMapGrid, stoneWallTile);
                        wallsTileMap.SetTile(curTilePosInTileMapGrid, stoneWallTile);
                        borderTile = true;
                    }

                    if (!borderTile)
                    {
                        //Non border of the section.
                        groundTileMap.SetTile(curTilePosInTileMapGrid, groundTileGrassMM);
                    }
                }
            }
        }

        return true;
    }

    private bool DrawRoomConnectionsGizmos(Vector2Int curRoomIndexCoords)
    {
        Room curRoom = roomIndexAndRoom[curRoomIndexCoords];

        Vector2 roomCenteringOffset = new Vector2(numTilesInRooms.x / 2, numTilesInRooms.y / 2);
        //roomCenteringOffset += new Vector2(0.5f, -0.5f);

        for (int connectedRoomListIndex = 0; connectedRoomListIndex < curRoom.connectedRooms.Count; connectedRoomListIndex++)
        {
            Vector2Int connectedRoomIndexCoord = curRoom.connectedRooms[connectedRoomListIndex];

            Vector3 startPos = new Vector3(curRoomIndexCoords.x * numTilesInRooms.x + roomCenteringOffset.x, curRoomIndexCoords.y * numTilesInRooms.y + roomCenteringOffset.y, 0.0f);
            Vector3 endPos = new Vector3(connectedRoomIndexCoord.x * numTilesInRooms.x + roomCenteringOffset.x, connectedRoomIndexCoord.y * numTilesInRooms.y + roomCenteringOffset.y, 0.0f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(startPos, endPos);
        }

        return true;
    }

    private bool DrawSectionConnectionToRoomCentreGizmos(Vector2Int curRoomIndexCoords)
    {
        Room curRoom = roomIndexAndRoom[curRoomIndexCoords];
        bool drawNotASectionOnly = false;

        for (int i = 0; i < curRoom.sections.Count; i++)
        {
            if (drawNotASectionOnly)
            {
                if (curRoom.sections[i].size.x <= 1 || curRoom.sections[i].size.y <= 1)
                {
                    DrawSectionConnectionToRoomCentre(curRoomIndexCoords, i);
                }
            }
            else
            {
                if (curRoom.sections[i].size.x > 1 && curRoom.sections[i].size.y > 1)
                {
                    DrawSectionConnectionToRoomCentre(curRoomIndexCoords, i);
                }
            }
        }

        return true;
    }

    private bool DrawSectionsGizmos(Vector2Int curRoomIndexCoords)
    {
        Room curRoom = roomIndexAndRoom[curRoomIndexCoords];
        bool drawNotASectionOnly = false;

        for (int i = 0; i < curRoom.sections.Count; i++)
        {
            if (drawNotASectionOnly)
            {
                if (curRoom.sections[i].size.x <= 1 || curRoom.sections[i].size.y <= 1)
                {
                    DrawSectionGizmos(curRoomIndexCoords, i);
                }
            }
            else
            {
                if (curRoom.sections[i].size.x > 1 && curRoom.sections[i].size.y > 1)
                {
                    DrawSectionGizmos(curRoomIndexCoords, i);
                }
            }
        }

        return true;
    }

    private void DrawSectionGizmos(Vector2Int curRoomIndexCoords, int curSectionIndex)
    {
        Room curRoom = roomIndexAndRoom[curRoomIndexCoords];

        Section curSection = curRoom.sections[curSectionIndex];

        for (int i = 0; i < curSection.borderingWallTypes.Count; i++)
        {
            GizmosDrawSectionRect(curRoomIndexCoords, Vector2.up * 0.1f, curSection);
        }

    }

    private void GizmosDrawSectionRect(Vector2Int roomIndex, Vector2 offset, Section section)
    {
        Vector3 roomWorldOffset = new Vector3(roomIndex.x * numTilesInRooms.x, roomIndex.y * numTilesInRooms.y, 0.0f);
        Vector3 providedOffset = offset;

        Vector3 bottomLeftWorldPosition     = new Vector3(section.bottomLeft.x, section.bottomLeft.y, 0.0f);
        Vector3 bottomRightWorldPosition    = bottomLeftWorldPosition + new Vector3(section.size.x, 0.0f, 0.0f);
        Vector3 topLeftWorldPosition        = bottomLeftWorldPosition + new Vector3(0.0f, section.size.y, 0.0f);
        Vector3 topRightWorldPosition       = topLeftWorldPosition + new Vector3(section.size.x, 0.0f, 0.0f);

        bottomLeftWorldPosition     += roomWorldOffset + providedOffset;
        bottomRightWorldPosition    += roomWorldOffset + providedOffset;
        topLeftWorldPosition        += roomWorldOffset + providedOffset;
        topRightWorldPosition       += roomWorldOffset + providedOffset;


        bool hasBottomBoundary = false;
        bool hasTopBoundary = false;
        bool hasLeftBoundary = false;
        bool hasRightBoundary = false;
        for (int i = 0; i < section.borderingWallTypes.Count; i++)
        {
            if(section.borderingWallTypes[i] == SectionBorderingWallType.Bottom)
            {
                hasBottomBoundary = true;
            }
            if (section.borderingWallTypes[i] == SectionBorderingWallType.Top)
            {
                hasTopBoundary = true;
            }
            if (section.borderingWallTypes[i] == SectionBorderingWallType.Left)
            {
                hasLeftBoundary = true;
            }
            if (section.borderingWallTypes[i] == SectionBorderingWallType.Right)
            {
                hasRightBoundary = true;
            }
        }

        if (hasBottomBoundary)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(bottomLeftWorldPosition, bottomRightWorldPosition);
        }

        if (hasRightBoundary)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(bottomRightWorldPosition, topRightWorldPosition);
        }

        if (hasTopBoundary)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(topRightWorldPosition, topLeftWorldPosition);
        }

        if (hasLeftBoundary)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(topLeftWorldPosition, bottomLeftWorldPosition);
        }
    }

    private void DrawSectionConnectionToRoomCentre(Vector2Int curRoomIndexCoords, int curSectionIndex)
    {
        Room curRoom = roomIndexAndRoom[curRoomIndexCoords];

        Section curSection = curRoom.sections[curSectionIndex];
        Vector2 sectionCenteringOffset = new Vector2(curSection.size.x / 2, curSection.size.y / 2);

        Vector3 uncenteredRoomCoords = new Vector3(curRoomIndexCoords.x * numTilesInRooms.x, curRoomIndexCoords.y * numTilesInRooms.y, 0.0f);
        Vector3 roomCenteringOffset = new Vector3(numTilesInRooms.x / 2, numTilesInRooms.y / 2, 0.0f);
        roomCenteringOffset += new Vector3(0.5f, -0.5f, 0.0f);
        Vector3 centeredRoomCoords = uncenteredRoomCoords + roomCenteringOffset;

        Vector3 curSectionCoords = uncenteredRoomCoords + new Vector3(curSection.bottomLeft.x, curSection.bottomLeft.y);
        Vector3 curSectionCoordsOffset = curSectionCoords + new Vector3(sectionCenteringOffset.x, sectionCenteringOffset.y, 0.0f);

        Vector3 startPos = centeredRoomCoords;
        //Vector3 endPos = curSectionCoords;
        Vector3 endPos = curSectionCoordsOffset;

        for (int i = 0; i < curSection.borderingWallTypes.Count; i++)
        {
            Vector3 endOffsetBasedOnType = new Vector3(0.1f, 0.1f, 0.0f);

            if (curSection.borderingWallTypes[i] == SectionBorderingWallType.Bottom)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(startPos, endPos + endOffsetBasedOnType);
            }
            if (curSection.borderingWallTypes[i] == SectionBorderingWallType.Left)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(startPos, endPos - endOffsetBasedOnType);
            }
            if (curSection.borderingWallTypes[i] == SectionBorderingWallType.Right)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(startPos + endOffsetBasedOnType, endPos + endOffsetBasedOnType);
            }
            if (curSection.borderingWallTypes[i] == SectionBorderingWallType.Top)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(startPos - endOffsetBasedOnType, endPos - endOffsetBasedOnType);
            }
        }
    }

    private bool RoomsAreConnectedWithEachOther(Vector2Int a, Vector2Int b)
    {
        return roomIndexAndRoom[a].connectedRooms.Contains(b) || roomIndexAndRoom[b].connectedRooms.Contains(a);
    }

    private bool CreateConnectionsBetweenRooms(Vector2Int currentRoomIndex)
    {
        Room curRoom = roomIndexAndRoom[currentRoomIndex];
        curRoom.connectedRooms.Clear();

        List<int> eliminatedDirections = new List<int>();

        int randNumConnections = Random.Range(1, 4);

        while (eliminatedDirections.Count < randNumConnections)
        {
            Vector2Int leftRoomIndex = currentRoomIndex + Vector2Int.left;
            Vector2Int rightRoomIndex = currentRoomIndex + Vector2Int.right;
            Vector2Int upRoomIndex = currentRoomIndex + Vector2Int.up;

            bool leftPossible = currentRoomIndex.x > 0 && !eliminatedDirections.Contains(left) && !RoomsAreConnectedWithEachOther(leftRoomIndex, currentRoomIndex);
            bool rightPossible = currentRoomIndex.x < mapSizeInRooms.x - 1 && !eliminatedDirections.Contains(right) && !RoomsAreConnectedWithEachOther(rightRoomIndex, currentRoomIndex);
            bool upPossible = currentRoomIndex.y < mapSizeInRooms.y - 1 && !eliminatedDirections.Contains(up) && !RoomsAreConnectedWithEachOther(upRoomIndex, currentRoomIndex);

            // This room has all the connections it can possiblly make.
            if (!leftPossible && !rightPossible && !upPossible)
            {
                break;
            }

            int randInt = Random.Range(0, 3);

            if (randInt == up && upPossible)
            {
                //Debug.Log("Created connection, up room := " + currentRoomIndex.ToString() + " <-> " + upRoomIndex.ToString());
                curRoom.connectedRooms.Add(upRoomIndex);
                eliminatedDirections.Add(up);
            }

            if (randInt == right && rightPossible)
            {
                //Debug.Log("Created connection, right room := " + currentRoomIndex.ToString() + " <-> " + rightRoomIndex.ToString());
                curRoom.connectedRooms.Add(rightRoomIndex);
                eliminatedDirections.Add(right);
            }

            if (randInt == left && leftPossible)
            {
                //Debug.Log("Created connection, left room := " + currentRoomIndex.ToString() + " <-> " + leftRoomIndex.ToString());
                curRoom.connectedRooms.Add(leftRoomIndex);
                eliminatedDirections.Add(left);
            }
        }

        return true;

    }

    private bool CreateSectionsInCurrentRoom(Vector2Int currentRoomIndex)
    {
        Room currentRoom = roomIndexAndRoom[currentRoomIndex];
        currentRoom.sections.Clear();

        if (currentRoomIndex == castleSpawnRoom)
        {
            Section curSection = new Section();

            Vector2Int randomSectionSize = new Vector2Int(numTilesInRooms.x, numTilesInRooms.y);

            curSection.bottomLeft = new Vector2Int(0, 0);
            curSection.size = randomSectionSize;

            curSection.borderingWallTypes.Add(SectionBorderingWallType.Left);
            curSection.borderingWallTypes.Add(SectionBorderingWallType.Bottom);
            curSection.borderingWallTypes.Add(SectionBorderingWallType.Right);
            curSection.borderingWallTypes.Add(SectionBorderingWallType.Top);

            currentRoom.sections.Add(curSection);

            return true;
        }

        bool[,] marked = new bool[numTilesInRooms.x, numTilesInRooms.y];
        Vector2Int _minimumSectionSize = minimumSectionSize;

        if (currentRoomIndex.y >= minYForResourceRooms)
        {
            _minimumSectionSize = dimsForRecourceRooms;
        }

        for (int y = 0; y < numTilesInRooms.y; y++)
        {
            for (int x = 0; x < numTilesInRooms.x; x++)
            {
                if (!marked[x, y])
                {
                    int widthRemainingTiles = numTilesInRooms.x - x;
                    int heightRemainingTiles = numTilesInRooms.y - y;

                    for (int i = 0; i < widthRemainingTiles; i++)
                    {
                        if (marked[i + x, y])
                        {
                            widthRemainingTiles = i;
                            break;
                        }
                    }

                    int maxWidth = Mathf.Min(widthRemainingTiles, maximumSectionSize.x);
                    int maxHeight = Mathf.Min(heightRemainingTiles, maximumSectionSize.y);

                    int randomSizeX = Random.Range(_minimumSectionSize.x, maxWidth);
                    int randomSizeY = Random.Range(_minimumSectionSize.y, maxHeight);
                    Vector2Int randomSectionSize = new Vector2Int(randomSizeX, randomSizeY);

                    // Check to create a resource or tel room.
                    if(Random.Range(0.0f, 1.0f) <= probabilityOfSpecialSection)
                    {
                        randomSectionSize = new Vector2Int(_minimumSectionSize.x, _minimumSectionSize.y);
                    }

                    int remainingTilesX = widthRemainingTiles - randomSectionSize.x;
                    if (remainingTilesX <= _minimumSectionSize.x)
                    {
                        randomSectionSize.x += remainingTilesX;
                    }

                    int remainingTilesY = heightRemainingTiles - randomSectionSize.y;
                    if (remainingTilesY <= _minimumSectionSize.y)
                    {
                        randomSectionSize.y += remainingTilesY;
                    }

                    Section curSection = new Section();

                    curSection.bottomLeft = new Vector2Int(x, y);
                    curSection.size = randomSectionSize;

                    if (x == 0)
                    {
                        curSection.borderingWallTypes.Add(SectionBorderingWallType.Left);
                    }

                    if (y == 0)
                    {
                        curSection.borderingWallTypes.Add(SectionBorderingWallType.Bottom);
                    }

                    if (x + randomSectionSize.x >= numTilesInRooms.x)
                    {
                        curSection.borderingWallTypes.Add(SectionBorderingWallType.Right);
                    }

                    if (y + randomSectionSize.y >= numTilesInRooms.y)
                    {
                        curSection.borderingWallTypes.Add(SectionBorderingWallType.Top);
                    }

                    FillSectionWithPatrollingWayPoints(currentRoomIndex, curSection);

                    currentRoom.sections.Add(curSection);

                    for (int j = y; j < (y + randomSectionSize.y); j++)
                    {
                        for (int i = x; i < (x + randomSectionSize.x); i++)
                        {
                            marked[i, j] = true;
                        }
                    }
                }
            }
        }


        return true;
    }

    private bool CanSpawnMoreEnemiesInThisSection(int numberOfEnemiesSpawned, Vector2 randomSectionSize, int roomY)
    {
        int maxNumberOfEnemiesThatCanBeSpawnedInASection = 4;
        int numTilesInSection = ((int)randomSectionSize.x - 1) * ((int)randomSectionSize.y - 1);

        int maxNumberOfEnemiesFor16Tiles = 1;
        int maxNumberOfEnemiesFor25Tiles = 2;
        int maxNumberOfEnemiesForXTiles = 3;

        int spawnDueToHeightBasedDifficulty = roomY > mapSizeInRooms.y / 4 ? 4 : 0;
        bool heightBasedDifficultyChance = Random.Range(0, 2) == 0 ? false : true;

        if (heightBasedDifficultyChance)
        {
            maxNumberOfEnemiesFor16Tiles++;
            maxNumberOfEnemiesFor25Tiles++;
            maxNumberOfEnemiesForXTiles++;

            maxNumberOfEnemiesFor16Tiles = Mathf.Clamp(maxNumberOfEnemiesFor16Tiles, 1, 4);
            maxNumberOfEnemiesFor25Tiles = Mathf.Clamp(maxNumberOfEnemiesFor25Tiles, 2, 4);
            maxNumberOfEnemiesForXTiles = Mathf.Clamp(maxNumberOfEnemiesForXTiles, 3, 4);
        }

        if(numberOfEnemiesSpawned >= maxNumberOfEnemiesThatCanBeSpawnedInASection)
        {
            return false;
        }

        if(numTilesInSection <= 16)
        {
            return numberOfEnemiesSpawned < maxNumberOfEnemiesFor16Tiles;
        }

        if(numTilesInSection <= 25)
        {
            return numberOfEnemiesSpawned < maxNumberOfEnemiesFor25Tiles;
        }

        return numberOfEnemiesSpawned < maxNumberOfEnemiesForXTiles;
    }

    private void FillSectionWithPatrollingWayPoints(Vector2 currentRoomIndex, Section currentSection)
    {
        Vector2 roomOffset = new Vector2(currentRoomIndex.x * numTilesInRooms.x, currentRoomIndex.y * numTilesInRooms.y);

        List<Vector2> corners = new List<Vector2>
        {
            new Vector2(currentSection.bottomLeft.x + 1, currentSection.bottomLeft.y + 1),                                                          // bottom left
            new Vector2(currentSection.bottomLeft.x + 1, currentSection.bottomLeft.y + currentSection.size.y - 1 - 1),                              // top left
            new Vector2(currentSection.bottomLeft.x + currentSection.size.x - 1 - 1, currentSection.bottomLeft.y + currentSection.size.y - 1 - 1),  // top right
            new Vector2(currentSection.bottomLeft.x + currentSection.size.x - 1 - 1, currentSection.bottomLeft.y + 1)                               // bottom right
        };

        List<Vector2> midPoints = new List<Vector2>
        {
            (corners[0] + corners[1]) / 2,
            (corners[1] + corners[2]) / 2,
            (corners[2] + corners[3]) / 2,
            (corners[3] + corners[0]) / 2
        };

        //currentSection.patrolPoints.Add(roomOffset + corners[0] + new Vector2(0.5f, 0.5f));
        //currentSection.patrolPoints.Add(roomOffset + corners[1] + new Vector2(0.5f, 0.5f));
        //currentSection.patrolPoints.Add(roomOffset + corners[2] + new Vector2(0.5f, 0.5f));
        //currentSection.patrolPoints.Add(roomOffset + corners[3] + new Vector2(0.5f, 0.5f));

        currentSection.patrolPoints.Add(roomOffset + corners[0] + new Vector2(0.5f, 0.5f));
            currentSection.patrolPoints.Add(roomOffset + midPoints[0] + new Vector2(0.5f, 0.5f));
        currentSection.patrolPoints.Add(roomOffset + corners[1] + new Vector2(0.5f, 0.5f));
            currentSection.patrolPoints.Add(roomOffset + midPoints[1] + new Vector2(0.5f, 0.5f));
        currentSection.patrolPoints.Add(roomOffset + corners[2] + new Vector2(0.5f, 0.5f));
            currentSection.patrolPoints.Add(roomOffset + midPoints[2] + new Vector2(0.5f, 0.5f));
        currentSection.patrolPoints.Add(roomOffset + corners[3] + new Vector2(0.5f, 0.5f));
            currentSection.patrolPoints.Add(roomOffset + midPoints[3] + new Vector2(0.5f, 0.5f));


        currentSection.patrolPointsOccupied.Add(false);
        currentSection.patrolPointsOccupied.Add(false);
        currentSection.patrolPointsOccupied.Add(false);
        currentSection.patrolPointsOccupied.Add(false);
        currentSection.patrolPointsOccupied.Add(false);
        currentSection.patrolPointsOccupied.Add(false);
        currentSection.patrolPointsOccupied.Add(false);
        currentSection.patrolPointsOccupied.Add(false);
    }

    private bool AnotherEnemyInSectionHasSameWayPointIndex(Vector2Int currentRoomIndex, int currentSectionIndex, int currentWayPointIndex)
    {
        Room currentRoom = roomIndexAndRoom[currentRoomIndex];
        Section currentSection = currentRoom.sections[currentSectionIndex];

        for (int i = 0; i < currentSection.enemiesMovementComponentList.Count; i++)
        {
            if (currentSection.enemiesMovementComponentList[i].currentWayPointIndex == currentWayPointIndex)
            {
                return true;
            }
        }

        return false;
    }

    public Vector2 GetNextPatrolWayPointPosition(Vector2Int currentRoomIndex, int currentSectionIndex, ref int currentWayPointIndex, bool corners, bool useAllPatrolPoints)
    {
        Room currentRoom = roomIndexAndRoom[currentRoomIndex];
        Section currentSection = currentRoom.sections[currentSectionIndex];

        int offset = useAllPatrolPoints ? 1 : 2;
        corners = useAllPatrolPoints ? true : corners;
        int starting = corners ? 0 : 1;

        int curWPIndex = currentWayPointIndex;

        //while (currentSection.patrolPointsOccupied[currentWayPointIndex] && )
        if (curWPIndex + offset < currentSection.patrolPoints.Count)
        {
            curWPIndex += offset;
        }
        else
        {
            curWPIndex = starting;
        }

        if (AnotherEnemyInSectionHasSameWayPointIndex(currentRoomIndex, currentSectionIndex, curWPIndex))
        {
            if(currentWayPointIndex + 1 < currentSection.patrolPoints.Count)
            {
                return currentSection.patrolPoints[currentWayPointIndex + 1];
            }
            else
            {
                return currentSection.patrolPoints[0];
            }
        }
        else
        {
            currentWayPointIndex = curWPIndex;
            return currentSection.patrolPoints[currentWayPointIndex];
        }

    }

    public void SetPatrolPointOccupancyInSection(Vector2Int currentRoomIndex, int currentSectionIndex, bool valueToSetTo, int currentWayPointIndex)
    {
        Room currentRoom = roomIndexAndRoom[currentRoomIndex];
        Section currentSection = currentRoom.sections[currentSectionIndex];

        currentSection.patrolPointsOccupied[currentWayPointIndex] = valueToSetTo;
    }

}