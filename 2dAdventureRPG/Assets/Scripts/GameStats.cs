using UnityEngine;

public class GameStats : MonoBehaviour
{
    private int numEnemiesKilled = 0;

    private int numTorchGoblinsKilled = 0;
    private int numBarrelGoblinsKilled = 0;
    private int numTNTGoblinsKilled = 0;

    public int currentKillStreak = 0;

    private void Awake()
    {
        currentKillStreak = 0;
    }

    public int NumEnemiesKilled()
    {
        return numEnemiesKilled;
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
