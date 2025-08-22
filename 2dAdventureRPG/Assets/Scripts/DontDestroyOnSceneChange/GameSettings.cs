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
}
