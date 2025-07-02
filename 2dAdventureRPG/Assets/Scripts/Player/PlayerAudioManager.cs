using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerAudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource environmentWalkAudioSource;
    [SerializeField] private AudioSource knightArmourWalkAudioSource;

    private CharacterStates characterStates;
    private AudioSource audioSource;

    private MapGenerator mapGenerator;

    [SerializeField] private float grassWalkVolume = 0.06f;
    [SerializeField] private float sandWalkVolume = 0.04f;
    [SerializeField] private float knightArmourWalkVolume = 0.06f;

    private void Awake()
    {
        characterStates = GetComponent<CharacterStates>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        mapGenerator = GameObject.FindGameObjectWithTag("MapTileGrid").GetComponent<MapGenerator>();

    }

    private void Update()
    {
        knightArmourWalkAudioSource.volume = knightArmourWalkVolume;

        if (characterStates.isMoving && !environmentWalkAudioSource.isPlaying && !knightArmourWalkAudioSource.isPlaying)
        {
            Vector3Int playerTilePos = new Vector3Int((int)transform.position.x, (int)transform.position.y, 0);
            PlayWalkingSfx(!mapGenerator.IsTileSand(playerTilePos));
        }
    }

    public void PlaySwordSwishSfx()
    {
        audioSource.pitch = Random.Range(0.75f, 1.25f);
        audioSource.PlayOneShot(AllAudioContainer.swishSoundEffects[GameObjectType.Player][Random.Range(0, AllAudioContainer.swishSoundEffects[GameObjectType.Player].Count)]);
    }

    public void PlayWalkingSfx(bool grass)
    {
        environmentWalkAudioSource.pitch = Random.Range(0.75f, 1.25f);

        if (grass)
        {
            environmentWalkAudioSource.volume = grassWalkVolume;
            environmentWalkAudioSource.PlayOneShot(AllAudioContainer.environmentWalkSoundEffects[EnvironmentType.Grass][Random.Range(0, AllAudioContainer.environmentWalkSoundEffects[EnvironmentType.Grass].Count)]);
        }
        else
        {
            environmentWalkAudioSource.volume = sandWalkVolume;
            environmentWalkAudioSource.PlayOneShot(AllAudioContainer.environmentWalkSoundEffects[EnvironmentType.Sand][Random.Range(0, AllAudioContainer.environmentWalkSoundEffects[EnvironmentType.Sand].Count)]);
        }

        knightArmourWalkAudioSource.PlayOneShot(AllAudioContainer.knightWalkingInArmourSoundEffects[Random.Range(0, AllAudioContainer.knightWalkingInArmourSoundEffects.Count)]);
    }
}
