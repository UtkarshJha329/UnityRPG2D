using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveData
{
    public int numTorchEnemies = 5;
    public int numBarrelEnemies = 5;
    public int numBombEnemies = 5;
}

public class SpawnCastleEnemies : MonoBehaviour
{
    public List<Transform> enemiesSpawnPoints = new List<Transform>();
    public List<StructureHealth> spawnPointHealths = new List<StructureHealth>();
    public GameObject torchEnemyGameObject;
    public GameObject barrelEnemyGameObject;
    public GameObject bombEnemyGameObject;

    public List<WaveData> wavesData = new List<WaveData>();

    public Transform enemiesParentTransform;
    public CameraTargetManager cameraTargetManager;

    private MapGenerator mapGenerator;

    private int currentWaveIndex = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mapGenerator = GameObject.FindGameObjectWithTag("MapTileGrid").GetComponent<MapGenerator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (NumEnemiesAliveCrossedRespawnThreshold(4) && currentWaveIndex < wavesData.Count)
        {
            SpawnWave(currentWaveIndex);
            currentWaveIndex++;
        }
    }

    private void SpawnWave(int waveIndex)
    {
        List<Transform> validSpawnPoints = new List<Transform>();
        for (int i = 0; i < spawnPointHealths.Count; i++)
        {
            if (spawnPointHealths[i].structureCurrentHealth > 0)
            {
                validSpawnPoints.Add(enemiesSpawnPoints[i]);
            }
        }


        float randomSpawnRadius = 2.0f;
        GameObject curRoom = mapGenerator.roomObjectDictionary[mapGenerator.castleSpawnRoom];
        for (int i = 0; i < wavesData[waveIndex].numTorchEnemies; i++)
        {
            Vector3 randomOffsetFromSpawn = (Random.insideUnitCircle * randomSpawnRadius);
            Vector3 spawnPosition =  randomOffsetFromSpawn + validSpawnPoints[Random.Range(0, validSpawnPoints.Count)].position;
            mapGenerator.CreateCastleRoomEnemy(torchEnemyGameObject, spawnPosition, enemiesParentTransform, mapGenerator.castleSpawnRoom, 0, cameraTargetManager);
        }

        randomSpawnRadius = 3.0f;
        for (int i = 0; i < wavesData[waveIndex].numBarrelEnemies; i++)
        {
            Vector3 randomOffsetFromSpawn = (Random.insideUnitCircle * randomSpawnRadius);
            Vector3 spawnPosition = randomOffsetFromSpawn + validSpawnPoints[Random.Range(0, validSpawnPoints.Count)].position;
            mapGenerator.CreateCastleRoomEnemy(barrelEnemyGameObject, spawnPosition, enemiesParentTransform, mapGenerator.castleSpawnRoom, 0, cameraTargetManager);
        }

        randomSpawnRadius = 5.0f;
        for (int i = 0; i < wavesData[waveIndex].numBombEnemies; i++)
        {
            Vector3 randomOffsetFromSpawn = (Random.insideUnitCircle * randomSpawnRadius);
            Vector3 spawnPosition = randomOffsetFromSpawn + validSpawnPoints[Random.Range(0, validSpawnPoints.Count)].position;
            mapGenerator.CreateCastleRoomEnemy(bombEnemyGameObject, spawnPosition, enemiesParentTransform, mapGenerator.castleSpawnRoom, 0, cameraTargetManager);
        }
    }

    private bool NumEnemiesAliveCrossedRespawnThreshold(int threshold)
    {
        Section curSection = mapGenerator.roomIndexAndRoom[mapGenerator.castleSpawnRoom].sections[0];

        int numEnemiesAlive = 0;
        for (int i = 0; i < curSection.enemiesHealthComponentList.Count; i++)
        {
            if (curSection.enemiesHealthComponentList[i].GetCurrentHealth() > 0)
            {
                numEnemiesAlive++;
            }
        }

        return numEnemiesAlive < threshold;
    }
}
