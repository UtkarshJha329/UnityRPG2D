using System.Collections.Generic;
using UnityEngine;

public class TurnGrassToSand : MonoBehaviour
{
    [SerializeField] private float turnGrassIntoSandEveryXSeconds = 5.0f;

    private MapGenerator mapGenerator;

    private float nextTurnGrassIntoSandSeconds = 0.0f;

    private bool initTilesList = true;
    private Queue<Vector3Int> tilesToTurnIntoSand = new Queue<Vector3Int>();

    [SerializeField] private int maxNumTilesToConvertEachFrame = 40;
    private bool notFinishedPreviousConversionsToSand = false;
    private int numTilesToConvertInCommingFrames = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mapGenerator = GameObject.FindGameObjectWithTag("MapTileGrid").GetComponent<MapGenerator>();
    }

    // Update is called once per frame
    void Update()
    {
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
        if(nextTurnGrassIntoSandSeconds <= Time.time /* || notFinishedPreviousConversionsToSand*/)
        {
            int numTilesConvertedThisRound = 0;
            while (numTilesConvertedThisRound < maxNumTilesToConvertEachFrame && tilesToTurnIntoSand.Count > 0)
            {
                numTilesConvertedThisRound++;
 
                Vector3Int currentTilePosition = tilesToTurnIntoSand.Dequeue();

                if (mapGenerator.IsTileGrass(currentTilePosition))
                {
                    mapGenerator.SetGroundTileToSand(currentTilePosition);

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

                    Debug.Log("Converted tile to sand.");
                }

                numTilesToConvertInCommingFrames--;
                numTilesToConvertInCommingFrames = Mathf.Max(numTilesToConvertInCommingFrames, 0);
            }

            if (numTilesToConvertInCommingFrames == 0)
            {
                notFinishedPreviousConversionsToSand = false;
            }

            if(numTilesConvertedThisRound >= maxNumTilesToConvertEachFrame && !notFinishedPreviousConversionsToSand)
            {
                notFinishedPreviousConversionsToSand = true;
                numTilesToConvertInCommingFrames = tilesToTurnIntoSand.Count;
            }

            nextTurnGrassIntoSandSeconds = Time.time + turnGrassIntoSandEveryXSeconds;
        }
    }

}
