using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileBorderType
{
    TOP_MIDDLE,
    BOTTOM_MIDDLE,
    RIGHT_MIDDLE,
    LEFT_MIDDLE
}

public class MapGenerator : MonoBehaviour
{
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

    [SerializeField] private GameObject torchGoblinEnemy;

    [SerializeField] private Vector2Int mapSizeInRooms;
    [SerializeField] private Vector2Int numTilesInRooms;

    [SerializeField] private Vector2Int minimumSectionSize = new Vector2Int(3, 3);
    //[SerializeField] private Vector2Int maximumSectionSize = new Vector2Int(5, 5);

    private Vector2Int mapSizeInTiles;

    private List<Vector3> enemyPositions = new List<Vector3>();

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

        //GenerateMap();

        //FillRoomWithMaze(new Vector2Int(2 * numTilesInRooms.x, 1 * numTilesInRooms.y));
        FillMapWithMaze();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //FillRoomWithMaze(new Vector2Int(2 * numTilesInRooms.x, 1 * numTilesInRooms.y));
            FillMapWithMaze();
        }
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < enemyPositions.Count; i++)
        {
            Gizmos.DrawWireSphere(enemyPositions[i], 0.5f);
        }
    }

    private void GenerateMap()
    {
        //for (int y = 0; y < mapSizeInTiles.y / 2; y++)
        //{
        //    for (int x = 0; x < mapSizeInTiles.x; x++)
        //    {
        //        groundTileMap.SetTile(new Vector3Int(x, y, 0), groundTileGrassMM);
        //    }
        //}

        //for (int y = mapSizeInTiles.y / 2; y < mapSizeInTiles.y; y++)
        //{
        //    for (int x = 0; x < mapSizeInTiles.x; x++)
        //    {
        //        groundTileMap.SetTile(new Vector3Int(x, y, 0), groundTileDrySand);
        //    }
        //}

        for (int y = 0; y < mapSizeInTiles.y / 2; y += numTilesInRooms.y)
        {
            for (int x = 0; x < mapSizeInTiles.x; x++)
            {
                groundTileMap.SetTile(new Vector3Int(x, y, 0), groundTileGrassTM);
            }
        }

        for (int y = numTilesInRooms.y - 1; y < mapSizeInTiles.y / 2; y += numTilesInRooms.y)
        {
            for (int x = 0; x < mapSizeInTiles.x; x++)
            {
                groundTileMap.SetTile(new Vector3Int(x, y, 0), groundTileGrassBM);
            }
        }

        for (int y = 0; y < mapSizeInTiles.y / 2; y++)
        {
            for (int x = 0; x < mapSizeInTiles.x; x += numTilesInRooms.x)
            {
                groundTileMap.SetTile(new Vector3Int(x, y, 0), groundTileGrassML);
            }
        }

        for (int y = 0; y < mapSizeInTiles.y / 2; y++)
        {
            for (int x = numTilesInRooms.x - 1; x < mapSizeInTiles.x; x += numTilesInRooms.x)
            {
                groundTileMap.SetTile(new Vector3Int(x, y, 0), groundTileGrassMR);
            }
        }


    }

    private void FillMapWithMaze()
    {
        enemyPositions.Clear();
        for (int j = 0; j < mapSizeInRooms.y; j++)
        {
            for (int i = 0; i < mapSizeInRooms.x; i++)
            {
                FillRoomWithMaze(new Vector2Int(i, j));
            }
        }
    }

    private void FillRoomWithMaze(Vector2Int roomIndex)
    {
        Vector2Int offsetInTiles = new Vector2Int(roomIndex.x * numTilesInRooms.x, roomIndex.y * numTilesInRooms.y);
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

                    int numEnemiesInThisSection = 0;
                    for (int j = y; j < (y + randomSectionSize.y); j++)
                    {
                        for (int i = x; i < (x + randomSectionSize.x); i++)
                        {
                            if (!marked[i, j])
                            {
                                int enemyTilePosIndex = IsEnemyGenerationTile(new Vector2(i, j), ref enemySpawnCornerPos);
                                if (enemyTilePosIndex != -1 && CanSpawnMoreEnemiesInThisSection(numEnemiesInThisSection, randomSectionSize, roomIndex.y))
                                {
                                    //Debug.Log("Enemy generation tile pos.");
                                    //Debug.Log(i + ", " + j);
                                    Vector3 curEnemyPos = new Vector3(offsetInTiles.x + i, offsetInTiles.y + j, 0);
                                    curEnemyPos.x += 0.5f;
                                    curEnemyPos.y += 0.5f;
                                    //Gizmos.DrawWireSphere(curEnemyPos, 0.5f);
                                    enemyPositions.Add(curEnemyPos);
                                    //if(enemyPositions.Count % 1 == 0)
                                    //{
                                    //    Instantiate(torchGoblinEnemy, curEnemyPos, Quaternion.identity);
                                    //}
                                    GameObject enemyGameobject = Instantiate(torchGoblinEnemy, curEnemyPos, Quaternion.identity);
                                    List<Vector2> patrolWaypoints = new List<Vector2>();
                                    patrolWaypoints = PatrolingWayPointsListGenerator(enemyTilePosIndex, offsetInTiles, ref enemySpawnCornerPos);
                                    Enemy_Movement enemyMovementComponent = enemyGameobject.GetComponent<Enemy_Movement>();
                                    enemyMovementComponent.patrollingWayPoints = patrolWaypoints;

                                    numEnemiesInThisSection++;
                                }

                                marked[i, j] = true;

                                Vector3Int curTilePosInTileMapGrid = new Vector3Int(offsetInTiles.x + i, offsetInTiles.y + j, 0);
                                bool borderTile = false;
                                if (j == y)
                                {
                                    //Bottom border of the section.
                                    groundTileMap.SetTile(curTilePosInTileMapGrid, stoneWallTile);
                                    borderTile = true;
                                }
                                else if (j == (y + randomSectionSize.y - 1))
                                {
                                    //Top border of the section.
                                    marked[i, j] = false;
                                    borderTile = true;
                                }

                                if (i == x)
                                {
                                    //Left border of the section.
                                    groundTileMap.SetTile(curTilePosInTileMapGrid, stoneWallTile);
                                    borderTile = true;
                                }
                                else if (i == (x + randomSectionSize.x - 1))
                                {
                                    //Right border of the section.
                                    marked[i, j] = false;
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
                }
            }
        }
    }

    void GenerateEnemy(Vector3 position)
    {
        GameObject instantiatedEnemy = Instantiate(torchGoblinEnemy);
        instantiatedEnemy.transform.position = new Vector3(position.x, position.y, 0.0f);
    }

    private bool CanSpawnMoreEnemiesInThisSection(int numberOfEnemiesSpawned, Vector2 randomSectionSize, int roomY)
    {
        int difficultyBuffer = 2;
        int minimumOneDimensionOffsetFromMinSectionSize = 3;
        if((randomSectionSize.x > minimumSectionSize.x + minimumOneDimensionOffsetFromMinSectionSize && randomSectionSize.y > minimumSectionSize.y) ||
            (randomSectionSize.x > minimumSectionSize.x && randomSectionSize.y > minimumSectionSize.y + minimumOneDimensionOffsetFromMinSectionSize))
        {
            return Random.Range(0, 10) >= mapSizeInRooms.y - roomY - difficultyBuffer;
        }
        else if(randomSectionSize.x == minimumSectionSize.x || randomSectionSize.y == minimumSectionSize.y)
        {
            return numberOfEnemiesSpawned < 2 && Random.Range(0, 10) >= mapSizeInRooms.y - roomY - difficultyBuffer;
        }

        return false;
    }

    private int IsEnemyGenerationTile(Vector2 tilePosIndex, ref List<Vector2> enemyGenerationTileIndices)
    {
        for (int i = 0; i < enemyGenerationTileIndices.Count; i++)
        {
            if (enemyGenerationTileIndices[i].x == tilePosIndex.x && enemyGenerationTileIndices[i].y == tilePosIndex.y)
            {
                return i;
            }
        }

        return -1;
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