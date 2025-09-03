using DG.Tweening.Core.Easing;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance { get; private set; }
    public float MASTER_VOLUME { get; set; } = 1.0f;
    public string GAME_SEED { get; set; } = "Seed...";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void SetSeed()
    {
        if (GameSettings.Instance.GAME_SEED.Length == 0)
        {
            GameSettings.Instance.GAME_SEED = Random.Range(197, 1283128).ToString();
        }
        Random.InitState(GameSettings.Instance.GAME_SEED.GetHashCode());
    }
}
