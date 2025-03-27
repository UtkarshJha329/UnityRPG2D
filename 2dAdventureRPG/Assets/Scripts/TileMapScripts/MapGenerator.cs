using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Tilemaps;

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

    [SerializeField] private Tile groundTileDrySand;

    [SerializeField] private Vector2Int mapSizeInRooms;
    [SerializeField] private Vector2Int numTilesInRooms;

    private Vector2Int mapSizeInTiles;

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

        FillRoomWithMaze(new Vector2Int(2 * numTilesInRooms.x, 1 * numTilesInRooms.y));
    }

    // Update is called once per frame
    void Update()
    {
        
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

    private void FillRoomWithMaze(Vector2Int offsetInTiles)
    {
        bool[,] marked = new bool[numTilesInRooms.x, numTilesInRooms.y];

        for (int y = 0; y < numTilesInRooms.y; y++)
        {
            for (int x = 0; x < numTilesInRooms.x; x++)
            {
                if (!marked[x, y])
                {
                    int randomSizeX = Random.Range(3, numTilesInRooms.x - x);
                    int randomSizeY = Random.Range(3, numTilesInRooms.y - y);
                    Vector2Int randomSectionSize = new Vector2Int(randomSizeX, randomSizeY);

                    int remainingTilesX = numTilesInRooms.x - x - randomSizeX;
                    Vector2Int added = new Vector2Int(0, 0);
                    if (remainingTilesX < 4)
                    {
                        added.x = remainingTilesX;
                        randomSectionSize.x += remainingTilesX;
                    }

                    int remainingTilesY = numTilesInRooms.y - y - randomSizeY;
                    if (remainingTilesY < 4)
                    {
                        added.y = remainingTilesY;
                        randomSectionSize.y += remainingTilesY;
                    }

                    Debug.Log("Random Section Size : " + randomSectionSize.x + ", " + randomSectionSize.y + " From : " + x + ", " + y);
                    Debug.Log("Added " + added.x + ", " + added.y);

                    for (int j = y; j < (y + randomSectionSize.y); j++)
                    {
                        for (int i = x; i < (x + randomSectionSize.x); i++)
                        {
                            //Debug.Log("Before : " + i + ", " + j);
                            marked[i, j] = true;
                            //Debug.Log("After : " + i + ", " + j);

                            if (j == y)
                            {
                                //Bottom border of the section.
                                groundTileMap.SetTile(new Vector3Int(offsetInTiles.x + i, offsetInTiles.y + j, 0), groundTileGrassBM);
                            }
                            else if (j == (y + randomSectionSize.y - 1))
                            {
                                //Top border of the section.
                                groundTileMap.SetTile(new Vector3Int(offsetInTiles.x + i, offsetInTiles.y + j, 0), groundTileGrassTM);
                            }
                            else if (i == x)
                            {
                                //Left border of the section.
                                groundTileMap.SetTile(new Vector3Int(offsetInTiles.x + i, offsetInTiles.y + j, 0), groundTileGrassML);
                            }
                            else if (i == (x + randomSectionSize.x - 1))
                            {
                                //Right border of the section.
                                groundTileMap.SetTile(new Vector3Int(offsetInTiles.x + i, offsetInTiles.y + j, 0), groundTileGrassMR);
                            }
                            else
                            {
                                //Non border of the section.
                                groundTileMap.SetTile(new Vector3Int(offsetInTiles.x + i, offsetInTiles.y + j, 0), groundTileGrassMM);
                            }
                        }
                    }
                }
            }
        }
    }
}
