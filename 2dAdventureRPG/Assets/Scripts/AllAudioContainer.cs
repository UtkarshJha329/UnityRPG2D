using System.Collections.Generic;
using System.Xml;
using UnityEngine;

[System.Serializable]
public class EnemyAudio
{
    public EnemyType enemyType;
    public AudioClip audioClip;
}

public enum GameObjectType
{
    Enemy,
    Player
}

public enum EnvironmentType
{
    Grass,
    Sand
}

public enum FoliageType
{
    Bush,
    Tree
}

[System.Serializable]
public class GameObjectBasedAudio
{
    public GameObjectType gameObjectType;
    public List<AudioClip> audioClip;
}

[System.Serializable]
public class EnvironmentBasedAudio
{
    public EnvironmentType environmentType;
    public List<AudioClip> audioClips;
}

[System.Serializable]
public class FoliageMovementAudio
{
    public FoliageType foliageType;
    public AudioClip audioClip;
}

public class AllAudioContainer : MonoBehaviour
{
    [Header("Goblin Specific")]
    [SerializeField] private List<EnemyAudio> enemyHurtSfx = new List<EnemyAudio>();
    [SerializeField] private List<EnemyAudio> enemyDeadSfx = new List<EnemyAudio>();

    [Header("Attacking")]
    [SerializeField] private List<GameObjectBasedAudio> swishSfx = new List<GameObjectBasedAudio>();

    [Header("Walking")]
    [SerializeField] private List<EnvironmentBasedAudio> environmentWalkSfx = new List<EnvironmentBasedAudio>();

    [Header("Foliage")]
    [SerializeField] private List<FoliageMovementAudio> foliageMovementClips = new List<FoliageMovementAudio>();

    [Header("Knight Specific")]
    [SerializeField] private List<AudioClip> knightArmourWalkingClips = new List<AudioClip>();

    public static Dictionary<EnemyType, AudioClip> enemyHurtBasedOnEnemyType = new Dictionary<EnemyType, AudioClip>();
    public static Dictionary<EnemyType, AudioClip> enemyDeadBasedOnEnemyType = new Dictionary<EnemyType, AudioClip>();
    public static Dictionary<GameObjectType, List<AudioClip>> swishSoundEffects = new Dictionary<GameObjectType, List<AudioClip>>();
    public static Dictionary<EnvironmentType, List<AudioClip>> environmentWalkSoundEffects = new Dictionary<EnvironmentType, List<AudioClip>>();
    public static Dictionary<FoliageType, AudioClip> foliageMovementSoundEffects = new Dictionary<FoliageType, AudioClip>();
    public static List<AudioClip> knightWalkingInArmourSoundEffects = new List<AudioClip>();
    //public static List<AudioClip> dynamiteExplodingSoundEffects = new List<AudioClip>();

    private void OnValidate()
    {
        enemyHurtBasedOnEnemyType.Clear();
        for (int i = 0; i < enemyHurtSfx.Count; i++)
        {
            enemyHurtBasedOnEnemyType.Add(enemyHurtSfx[i].enemyType, enemyHurtSfx[i].audioClip);
        }

        enemyDeadBasedOnEnemyType.Clear();
        for (int i = 0; i < enemyDeadSfx.Count; i++)
        {
            enemyDeadBasedOnEnemyType.Add(enemyDeadSfx[i].enemyType, enemyDeadSfx[i].audioClip);
        }

        swishSoundEffects.Clear();
        for (int i = 0; i < swishSfx.Count; i++)
        {
            swishSoundEffects.Add(swishSfx[i].gameObjectType, swishSfx[i].audioClip);
        }

        environmentWalkSoundEffects.Clear();
        for (int i = 0; i < environmentWalkSfx.Count; i++)
        {
            environmentWalkSoundEffects.Add(environmentWalkSfx[i].environmentType, environmentWalkSfx[i].audioClips);
        }

        knightWalkingInArmourSoundEffects.Clear();
        for (int i = 0; i < knightArmourWalkingClips.Count; i++)
        {
            knightWalkingInArmourSoundEffects.Add(knightArmourWalkingClips[i]);
        }

        foliageMovementSoundEffects.Clear();
        for (int i = 0; i < foliageMovementClips.Count; i++)
        {
            foliageMovementSoundEffects.Add(foliageMovementClips[i].foliageType, foliageMovementClips[i].audioClip);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
