using UnityEngine;

public class GameStats : MonoBehaviour
{
    private int numEnemiesKilled = 0;

    private int numTorchGoblinsKilled = 0;
    private int numBarrelGoblinsKilled = 0;
    private int numTNTGoblinsKilled = 0;

    private int numMinesDestroyed = 0;

    public int currentKillStreak = 0;
    public float timeSinceLastHitEnemy = 0.0f;

    public static int gameOverState = 0;

    public static bool finalStructuresHaveBeenDestroyed = false;
    public static bool playerReachedCutSceneTile = false;
    public static bool playerFinishedFinalCutscene = false;
    public static bool finalRoomConvertedIntoGrassFully = false;

    private void Awake()
    {
        currentKillStreak = 0;

        finalStructuresHaveBeenDestroyed = false;
        playerReachedCutSceneTile = false;
        playerFinishedFinalCutscene = false;
        finalRoomConvertedIntoGrassFully = false;
    }

    public int NumEnemiesKilled()
    {
        return numEnemiesKilled;
    }

    public int NumMinesDestroyed()
    {
        return numMinesDestroyed;
    }

    public int NumTorchGoblinsKilled()
    {
        return numTorchGoblinsKilled;
    }

    public int NumBarrelGoblinsKilled()
    {
        return numBarrelGoblinsKilled;
    }

    public int NumTNTGoblinsKilled()
    {
        return numTNTGoblinsKilled;
    }

    public void DestroyedMine()
    {
        numMinesDestroyed++;
    }

    public void KilledTorchGoblin()
    {
        numTorchGoblinsKilled++;
        numEnemiesKilled++;
    }

    public void KilledBarrelGoblin()
    {
        numBarrelGoblinsKilled++;
        numEnemiesKilled++;
    }

    public void KilledTNTGoblin()
    {
        numTNTGoblinsKilled++;
        numEnemiesKilled++;
    }
}
