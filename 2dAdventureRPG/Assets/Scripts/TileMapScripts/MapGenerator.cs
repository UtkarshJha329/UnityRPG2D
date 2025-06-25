using System.Collections.Generic;
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

    public List<SectionBorderingWallType> borderingWallTypes = new List<SectionBorderingWallType>();
}

[System.Serializable]
public class Room
{
    public List<Section> sections = new List<Section>();

    public List<Vector2Int> connectedRooms = new List<Vector2Int>();
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

public class MapGenerator : MonoBehaviour
{
    [Header("Enemy Instantiating Data")]
    [SerializeField] private GameObject torchGoblinEnemy;
    [SerializeField] private GameObject randomPrefabEnemy;

    [Header("Map Generator Data")]
    [SerializeField] private Vector2Int mapSizeInRooms;
    [SerializeField] private Vector2Int numTilesInRooms;
    [SerializeField] private Vector2Int minimumSectionSize = new Vector2Int(3, 3);
    [SerializeField] private Vector2Int playerSpawnRoom;
    [SerializeField] private Vector2Int playerSpawnTile;
    [SerializeField] private Vector2Int castleSpawnRoom;

    //[SerializeField] private Vector2Int maximumSectionSize = new Vector2Int(5, 5);

    [Header("Map Generator Tiles Data")]
    [SerializeField] private Tilemap groundTileMap;

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

    [SerializeField] private GameObject emptyRoomPrefab;
    [SerializeField] private GameObject playerPrefab;

    [Header("Room Camera Generator Data")]
    [SerializeField] private CinemachineCamera cineCamera;
    [SerializeField] private GameObject currentRoomBoundaryObject;

    private Vector2Int mapSizeInTiles;

    private List<Vector3> enemyPositions = new List<Vector3>();

    private Dictionary<Vector2Int, Room> roomIndexAndRoom = new Dictionary<Vector2Int, Room>();

    [SerializeField] string debugStringVisitedRooms = "";
    [SerializeField] string debugStringUntouchedRooms = "";

    [SerializeField] List<DebugRoom> roomsThatHaveBeenCreated;

    private int numEnemiesSpawned = 0;

    private int left = 0;
    private int right = 1;
    private int up = 2;

    private int borderInset = 1;

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
        mapSizeInTiles = mapSizeInRooms * numTilesInRooms;

        playerSpawnRoom = new Vector2Int(mapSizeInRooms.x / 2, 0);
        playerSpawnTile = new Vector2Int(numTilesInRooms.x / 2, 1);
        castleSpawnRoom = new Vector2Int(mapSizeInRooms.x / 2, mapSizeInRooms.y - 1);


        GameObject player = Instantiate(playerPrefab);
        player.transform.position = new Vector3(playerSpawnRoom.x * numTilesInRooms.x + playerSpawnTile.x, playerSpawnRoom.y * numTilesInRooms.y + playerSpawnTile.y, 0.0f);


        for (int y = 0; y < mapSizeInRooms.y; y++)
        {
            for (int x = 0; x < mapSizeInRooms.x; x++)
            {
                Room curRoom = new Room();
                roomIndexAndRoom.Add(new Vector2Int(x, y), curRoom);
            }
        }

        MakeMap();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            MakeMap();
        }
    }

    private void MakeMap()
    {
        groundTileMap.ClearAllTiles();
        for (int i = 0; i < roomsParent.childCount; i++)
        {
            GameObject.Destroy(roomsParent.GetChild(i).gameObject);
        }

        List<Vector2Int> roomsSeen = new List<Vector2Int>();
        TraverseUniqueRoomsThroughConnections(playerSpawnRoom, Vector2Int.down + Vector2Int.left, ref roomsSeen, CreateConnections);

        roomsSeen.Clear();
        TraverseUniqueRoomsThroughConnections(playerSpawnRoom, Vector2Int.down + Vector2Int.left, ref roomsSeen, CreateSectionsInCurrentRoom);

        roomsSeen.Clear();
        TraverseUniqueRoomsThroughConnections(playerSpawnRoom, Vector2Int.down + Vector2Int.left, ref roomsSeen, DrawSectionsToTileMaps);

        roomsSeen.Clear();
        TraverseUniqueRoomsThroughConnections(playerSpawnRoom, Vector2Int.down + Vector2Int.left, ref roomsSeen, GenerateEnemiesForRoom);

        roomsSeen.Clear();
        TraverseUniqueRoomsThroughConnections(playerSpawnRoom, Vector2Int.down + Vector2Int.left, ref roomsSeen, MakeConnectionsForRoomVisibleOnMap);


        roomsThatHaveBeenCreated.Clear();
        foreach(KeyValuePair<Vector2Int, Room> roomData in roomIndexAndRoom)
        {
            DebugRoom dRoom = new DebugRoom();
            dRoom.roomIndex = roomData.Key;
            dRoom.roomData = roomData.Value;
            roomsThatHaveBeenCreated.Add(dRoom);
        }
    }

    private void OnDrawGizmos()
    {
        if(roomIndexAndRoom.Count > 0)
        {
            List<Vector2Int> roomsSeen = new List<Vector2Int>();
            TraverseUniqueRoomsThroughConnections(playerSpawnRoom, Vector2Int.down + Vector2Int.left, ref roomsSeen, DrawRoomConnectionsGizmos);
            //TraverseRoomsThroughConnections(playerSpawnRoom, Vector2Int.down + Vector2Int.left, DrawSectionConnectionToRoomCentreGizmos);
            roomsSeen.Clear();
            TraverseUniqueRoomsThroughConnections(playerSpawnRoom, Vector2Int.down + Vector2Int.left, ref roomsSeen, DrawSectionsGizmos);
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

    private bool MakeConnectionsForRoomVisibleOnMap(Vector2Int currentRoomIndex)
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

    private void TilesThatConnectInSectionLists(Vector2Int roomAIndex, Vector2Int roomBIndex, int curConnectedRoomIsLeftRightUp, List<int> sectionsATouchingB, List<int> sectionsBTouchingA)
    {
        Room roomA = roomIndexAndRoom[roomAIndex];
        Room roomB = roomIndexAndRoom[roomBIndex];

        Vector3Int roomAOffset = new Vector3Int(roomAIndex.x * numTilesInRooms.x, roomAIndex.y * numTilesInRooms.y, 0);
        Vector3Int roomBOffset = new Vector3Int(roomBIndex.x * numTilesInRooms.x, roomBIndex.y * numTilesInRooms.y, 0);

        for (int i = 0; i < sectionsATouchingB.Count; i++)
        {
            Section curSectionA = roomA.sections[sectionsATouchingB[i]];

            for (int j = 0; j < sectionsBTouchingA.Count; j++)
            {
                Section curSectionB = roomB.sections[sectionsBTouchingA[j]];
                ConnectingTilesOfTwoSections(curSectionA, curSectionB, roomAOffset, roomBOffset, curConnectedRoomIsLeftRightUp);
            }
        }
    }

    private void ConnectingTilesOfTwoSections(Section a, Section b, Vector3Int roomAOffset, Vector3Int roomBOffset, int curConnectedRoomIsLeftRightUp)
    {
        // Why adding an offset of Vector2.left to the code below works? Hell if I know. But it does. So I won't touch it.

        if (curConnectedRoomIsLeftRightUp == left)
        {
            Vector3 bottomLeftA = a.bottomLeft + Vector2.up + Vector2.left;                                                            // Trim bottom most row since it can't be connected as it is entirely stone
            Vector3 topLeftA = a.bottomLeft + new Vector2(0.0f, a.size.y) + Vector2.down + Vector2.down + Vector2.left;                               // Trim top most row since it can't be connected as it is entirely stone

            Vector3 bottomRightB = b.bottomLeft + new Vector2(b.size.x, 0.0f) + Vector2.up;                             // Trim bottom most row since it can't be connected as it is entirely stone
            Vector3 topRightB = b.bottomLeft + b.size + Vector2.down + Vector2.down;                                                   // Trim top most row since it can't be connected as it is entirely stone

            NeighbouringTiles(topLeftA, bottomLeftA, topRightB, bottomRightB, roomAOffset, roomBOffset, curConnectedRoomIsLeftRightUp);
        }
        else if (curConnectedRoomIsLeftRightUp == right)
        {
            Vector3 bottomRightA = a.bottomLeft + new Vector2(a.size.x, 0.0f) + Vector2.up + Vector2.left;                             // Trim bottom most row since it can't be connected as it is entirely stone
            Vector3 topRightA = a.bottomLeft + a.size + Vector2.down + Vector2.down + Vector2.left;                                                   // Trim top most row since it can't be connected as it is entirely stone

            Vector3 bottomLeftB = b.bottomLeft + Vector2.up;                                                            // Trim bottom most row since it can't be connected as it is entirely stone
            Vector3 topLeftB = b.bottomLeft + new Vector2(0.0f, b.size.y) + Vector2.down + Vector2.down;                               // Trim top most row since it can't be connected as it is entirely stone

            NeighbouringTiles(topRightA, bottomRightA, topLeftB, bottomLeftB, roomAOffset, roomBOffset, curConnectedRoomIsLeftRightUp);
        }
        else if (curConnectedRoomIsLeftRightUp == up)
        {

        }
    }

    private void NeighbouringTiles(Vector3 topA, Vector3 bottomA, Vector3 topB, Vector3 bottomB, Vector3Int roomAOffset, Vector3Int roomBOffset, int curConnectedRoomIsLeftRightUp)
    {
        if(curConnectedRoomIsLeftRightUp == left || curConnectedRoomIsLeftRightUp == right)
        {
            int minimumTopY = (int)Mathf.Min(topA.y, topB.y);
            int maximumBottomY = (int)Mathf.Max(bottomA.y, bottomB.y);

            for (int i = maximumBottomY; i <= minimumTopY; i++)
            {
                groundTileMap.SetTile(new Vector3Int((int)bottomA.x, i, 0) + roomAOffset, groundTileDrySand);
                groundTileMap.SetTile(new Vector3Int((int)bottomB.x, i, 0) + roomBOffset, groundTileDrySand);
            }
        }
        else if(curConnectedRoomIsLeftRightUp == up)
        {

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

    private bool GenerateEnemiesForRoom(Vector2Int curRoomIndex)
    {
        GameObject room = Instantiate(emptyRoomPrefab, roomsParent);
        room.name = "Room " + "(" + curRoomIndex.x + ", " + curRoomIndex.y + ")";
        room.transform.position = new Vector3(curRoomIndex.x * numTilesInRooms.x, curRoomIndex.y * numTilesInRooms.y, 0);
        room.transform.position += new Vector3(numTilesInRooms.x / 2, numTilesInRooms.y / 2, 0);

        GameObject enemiesParentGameObject = Instantiate(enemiesParentPrefab, room.transform);

        GameObject curRoomBoundaryObject = Instantiate(currentRoomBoundaryObject, room.transform);
        CameraTargetManager cameraTargetManager = curRoomBoundaryObject.GetComponent<CameraTargetManager>();
        cameraTargetManager.cineCamera = cineCamera;
        cameraTargetManager.enemiesParent = enemiesParentGameObject.transform;


        Room curRoom = roomIndexAndRoom[curRoomIndex];
        for (int i = 0; i < curRoom.sections.Count; i++)
        {
            Section curSection = curRoom.sections[i];

            GenerateEnemiesForSection(curRoomIndex, curSection, ref enemiesParentGameObject);
            //Debug.Log("Spawned enemies for section.");
        }

        return true;
    }

    private void GenerateEnemiesForSection(Vector2Int curRoomIndex, Section curSection, ref GameObject enemiesParentGameObject)
    {
        Vector3 curRoomPos = new Vector3(curRoomIndex.x * numTilesInRooms.x, curRoomIndex.y * numTilesInRooms.y, 0.0f);
        List<Vector2> enemySpawnCornerPos = new List<Vector2>
        {
            new Vector2(curSection.bottomLeft.x + 1, curSection.bottomLeft.y + 1),                                                      // Bottom Left
            new Vector2(curSection.bottomLeft.x + curSection.size.x - borderInset - 1, curSection.bottomLeft.y + 1),                                  // Bottom Right
            new Vector2(curSection.bottomLeft.x + 1, curSection.bottomLeft.y + curSection.size.y - borderInset - 1),                                  // Top Left
            new Vector2(curSection.bottomLeft.x + curSection.size.x - borderInset - 1, curSection.bottomLeft.y + curSection.size.y - borderInset - 1)               // Top Right
        };

        int numEnemiesInThisSection = 0;
        for (int i = 0; CanSpawnMoreEnemiesInThisSection(numEnemiesInThisSection, curSection.size, curRoomIndex.y) && i < enemySpawnCornerPos.Count; i++)
        {
            //Debug.Log("Spawned enemies in section at corner.");

            Vector3 curEnemyPos = new Vector3(enemySpawnCornerPos[i].x, enemySpawnCornerPos[i].y, 0.0f) + curRoomPos;
            curEnemyPos.x += 0.5f;
            curEnemyPos.y += 0.5f;

            //enemyPositions.Add(curEnemyPos);
            bool debug = true;
            if (debug)
            {
                GameObject enemyGameobject = Instantiate(randomPrefabEnemy, curEnemyPos, Quaternion.identity, enemiesParentGameObject.transform);
                numEnemiesSpawned++;
                numEnemiesInThisSection++;
            }
            else
            {
                GameObject enemyGameobject = Instantiate(torchGoblinEnemy, curEnemyPos, Quaternion.identity, enemiesParentGameObject.transform);
                numEnemiesSpawned++;
                //GameObject enemyGameobject = Instantiate(torchGoblinEnemy, curEnemyPos, Quaternion.identity, gameObject.transform);

                List<Vector2> patrolWaypoints = new List<Vector2>();
                patrolWaypoints = PatrolingWayPointsListGenerator(i, curRoomPos, ref enemySpawnCornerPos);
                Enemy_Movement enemyMovementComponent = enemyGameobject.GetComponent<Enemy_Movement>();
                enemyMovementComponent.patrollingWayPoints = patrolWaypoints;

                numEnemiesInThisSection++;
            }
        }
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
                        groundTileMap.SetTile(curTilePosInTileMapGrid, stoneWallTile);
                        borderTile = true;
                    }
                    else if (y == curSection.size.y - borderInset)
                    {
                        //Top border of the section.
                        groundTileMap.SetTile(curTilePosInTileMapGrid, stoneWallTile);
                        borderTile = true;
                    }

                    if (x == 0)
                    {
                        //Left border of the section.
                        groundTileMap.SetTile(curTilePosInTileMapGrid, stoneWallTile);
                        borderTile = true;
                    }
                    else if (x == curSection.size.x - borderInset)
                    {
                        //Right border of the section.
                        groundTileMap.SetTile(curTilePosInTileMapGrid, stoneWallTile);
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

    private bool CreateConnections(Vector2Int currentRoomIndex)
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

        bool[,] marked = new bool[numTilesInRooms.x, numTilesInRooms.y];

        for (int y = 0; y < numTilesInRooms.y; y++)
        {
            for (int x = 0; x < numTilesInRooms.x; x++)
            {
                if (!marked[x, y])
                {
                    int maxWidth = numTilesInRooms.x - x;
                    int maxHeight = numTilesInRooms.y - y;

                    for (int i = 0; i < maxWidth; i++)
                    {
                        if (marked[i + x, y])
                        {
                            maxWidth = i + 1;
                            break;
                        }
                    }

                    int randomSizeX = Random.Range(minimumSectionSize.x, maxWidth);
                    int randomSizeY = Random.Range(minimumSectionSize.y, maxHeight);
                    Vector2Int randomSectionSize = new Vector2Int(randomSizeX, randomSizeY);

                    int remainingTilesX = maxWidth - randomSectionSize.x;
                    //Vector2Int added = new Vector2Int(0, 0);
                    if (remainingTilesX <= minimumSectionSize.x)
                    {
                        //added.x = remainingTilesX;
                        randomSectionSize.x += remainingTilesX;
                    }

                    int remainingTilesY = numTilesInRooms.y - y - randomSectionSize.y;
                    if (remainingTilesY <= minimumSectionSize.y)
                    {
                        //added.y = remainingTilesY;
                        randomSectionSize.y += remainingTilesY;
                    }

                    List<Vector2> enemySpawnCornerPos = new List<Vector2>
                    {
                        new Vector2(x + 1, y + 1),                                                      // Bottom Left
                        new Vector2(x + randomSectionSize.x - 2, y + 1),                                // Bottom Right
                        new Vector2(x + 1, y + randomSectionSize.y - 2),                                // Top Left
                        new Vector2(x + randomSectionSize.x - 2, y + randomSectionSize.y - 2)           // Top Right
                    };

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

    private List<Vector2> PatrolingWayPointsListGenerator(int curCornerIndex, Vector2 offsetInTiles, ref List<Vector2> corners)
    {
        List<Vector2> patrolingWayPointsList = new List<Vector2>();

        if(curCornerIndex == 0)
        {
            patrolingWayPointsList.Add(offsetInTiles + corners[0] + new Vector2(0.5f, 0.5f));
            patrolingWayPointsList.Add(offsetInTiles + corners[2] + new Vector2(0.5f, 0.5f));
        }
        else if(curCornerIndex == 1)
        {
            patrolingWayPointsList.Add(offsetInTiles + corners[0] + new Vector2(0.5f, 0.5f));
            patrolingWayPointsList.Add(offsetInTiles + corners[1] + new Vector2(0.5f, 0.5f));
        }
        else if(curCornerIndex == 2)
        {
            patrolingWayPointsList.Add(offsetInTiles + corners[2] + new Vector2(0.5f, 0.5f));
            patrolingWayPointsList.Add(offsetInTiles + corners[3] + new Vector2(0.5f, 0.5f));
        }
        else if(curCornerIndex == 3)
        {
            patrolingWayPointsList.Add(offsetInTiles + corners[3] + new Vector2(0.5f, 0.5f));
            patrolingWayPointsList.Add(offsetInTiles + corners[1] + new Vector2(0.5f, 0.5f));
        }

        return patrolingWayPointsList;
    }
}