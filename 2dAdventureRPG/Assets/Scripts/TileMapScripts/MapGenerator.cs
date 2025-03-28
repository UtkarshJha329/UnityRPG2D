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

    [SerializeField] private Vector2Int mapSizeInRooms;
    [SerializeField] private Vector2Int numTilesInRooms;

    [SerializeField] private Vector2Int minimumSectionSize = new Vector2Int(3, 3);
    //[SerializeField] private Vector2Int maximumSectionSize = new Vector2Int(5, 5);

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
        for (int j = 0; j < mapSizeInRooms.y; j++)
        {
            for (int i = 0; i < mapSizeInRooms.x; i++)
            {
                FillRoomWithMaze(new Vector2Int(numTilesInRooms.x * i, numTilesInRooms.y * j));
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
                    int maxWidth = numTilesInRooms.x - x;
                    int maxHeight = numTilesInRooms.y - y;

                    //maxWidth = maxWidth > maximumSectionSize.x ? maximumSectionSize.x : maxWidth;
                    //maxHeight = maxHeight > maximumSectionSize.y ? maximumSectionSize.y : maxHeight;

                    for (int i = 0; i < maxWidth; i++)
                    {
                        if (marked[i + x, y])
                        {
                            //Debug.Log("Extended Max Width from : " + maxWidth + ", to : " + i);
                            maxWidth = i + 1;
                            break;
                        }
                    }

                    int randomSizeX = Random.Range(minimumSectionSize.x, maxWidth);
                    int randomSizeY = Random.Range(minimumSectionSize.y, maxHeight);
                    Vector2Int randomSectionSize = new Vector2Int(randomSizeX, randomSizeY);

                    //int remainingTilesX = numTilesInRooms.x - x - randomSectionSize.x;
                    int remainingTilesX = maxWidth - randomSectionSize.x;
                    Vector2Int added = new Vector2Int(0, 0);
                    if (remainingTilesX <= minimumSectionSize.x)
                    {
                        added.x = remainingTilesX;
                        randomSectionSize.x += remainingTilesX;
                    }                    

                    int remainingTilesY = numTilesInRooms.y - y - randomSectionSize.y;
                    if (remainingTilesY <= minimumSectionSize.y)
                    {
                        added.y = remainingTilesY;
                        randomSectionSize.y += remainingTilesY;
                    }

                    //Debug.Log("Random Section Size : " + randomSectionSize.x + ", " + randomSectionSize.y + " From : " + x + ", " + y);
                    //Debug.Log("Added " + added.x + ", " + added.y);

                    for (int j = y; j < (y + randomSectionSize.y); j++)
                    {
                        for (int i = x; i < (x + randomSectionSize.x); i++)
                        {
                            if (!marked[i, j])
                            {
                                //Debug.Log("Before : " + i + ", " + j);
                                marked[i, j] = true;
                                //Debug.Log("After : " + i + ", " + j);

                                Vector3Int curTilePosInTileMapGrid = new Vector3Int(offsetInTiles.x + i, offsetInTiles.y + j, 0);
                                bool borderTile = false;
                                if (j == y)
                                {
                                    //Bottom border of the section.
                                    //groundTileMap.SetTile(curTilePosInTileMapGrid, groundTileGrassBM);
                                    groundTileMap.SetTile(curTilePosInTileMapGrid, stoneWallTile);
                                    //groundTileMap.SetTile(curTilePosInTileMapGrid, groundTileGrassMM);
                                    borderTile = true;
                                }
                                else if (j == (y + randomSectionSize.y - 1))
                                {
                                    //Top border of the section.
                                    //groundTileMap.SetTile(curTilePosInTileMapGrid, groundTileGrassTM);
                                    //groundTileMap.SetTile(curTilePosInTileMapGrid, groundTileGrassMM);
                                    //groundTileMap.SetTile(curTilePosInTileMapGrid, groundTileDrySand);
                                    marked[i, j] = false;
                                    borderTile = true;
                                }

                                if (i == x)
                                {
                                    //Left border of the section.
                                    //groundTileMap.SetTile(curTilePosInTileMapGrid, groundTileGrassML);
                                    groundTileMap.SetTile(curTilePosInTileMapGrid, stoneWallTile);
                                    //groundTileMap.SetTile(curTilePosInTileMapGrid, groundTileGrassMM);
                                    borderTile = true;
                                }
                                else if (i == (x + randomSectionSize.x - 1))
                                {
                                    //Right border of the section.
                                    //groundTileMap.SetTile(curTilePosInTileMapGrid, groundTileGrassMR);
                                    //groundTileMap.SetTile(curTilePosInTileMapGrid, groundTileGrassMM);
                                    //groundTileMap.SetTile(curTilePosInTileMapGrid, groundTileDrySand);
                                    //marked[i, j] = false;
                                    marked[i, j] = false;
                                    borderTile = true;
                                }

                                if (!borderTile)
                                {
                                    //Non border of the section.
                                    groundTileMap.SetTile(curTilePosInTileMapGrid, groundTileGrassMM);
                                    //groundTileMap.SetTile(curTilePosInTileMapGrid, groundTileDrySand);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}