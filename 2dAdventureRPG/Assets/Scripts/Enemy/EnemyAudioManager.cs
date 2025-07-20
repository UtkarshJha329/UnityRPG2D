using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class EnemyAudioManager : MonoBehaviour
{
    private EnemyProperties s_EnemyProperties;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        s_EnemyProperties = GetComponent<EnemyProperties>();

        s_EnemyProperties.audioSourcePitch = Random.Range(0.75f, 1.25f);
        audioSource.pitch = s_EnemyProperties.audioSourcePitch;
    }

    public void PlayHurtAudio()
    {
        audioSource.PlayOneShot(AllAudioContainer.enemyHurtBasedOnEnemyType[s_EnemyProperties.enemyType]);
        audioSource.PlayOneShot(AllAudioContainer.hitBySwordAudioClips[Random.Range(0, AllAudioContainer.hitBySwordAudioClips.Count)], Random.Range(4.0f, 8.0f));
    }
}
