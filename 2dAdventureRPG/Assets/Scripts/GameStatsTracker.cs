using UnityEngine;

[RequireComponent(typeof(GameStats))]
public class GameStatsTracker : MonoBehaviour
{
    [SerializeField] private float killStreakCooldownTime = 5.0f;


    private int lastEnemyKillCount = 0;

    private bool killStreakActive = false;
    private float killStreakCooldownTimeAt = 0.0f;

    private GameStats s_GameStats;

    private void Awake()
    {
        s_GameStats = GetComponent<GameStats>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleEnemyKilled();
        HandleResetKillStreak();
    }

    private void HandleEnemyKilled()
    {
        if(lastEnemyKillCount != s_GameStats.NumEnemiesKilled())
        {
            Debug.Log("Activated Killstreak.");

            killStreakActive = true;
            killStreakCooldownTimeAt = Time.time + killStreakCooldownTime;
            lastEnemyKillCount = s_GameStats.NumEnemiesKilled();

            s_GameStats.currentKillStreak++;
        }
    }

    private void HandleResetKillStreak()
    {
        if(killStreakActive && killStreakCooldownTimeAt <= Time.time)
        {
            Debug.Log("Deactivated Killstreak.");

            s_GameStats.currentKillStreak = 0;
            killStreakActive = false;
        }
    }
}
