using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ExplosionSoundEffectHandler : MonoBehaviour
{
    public float explosionVolume;
    public AudioClip explosionSfxClip;

    private AudioSource explosionAudioSource;

    private bool playedExplosion = false;

    private void Awake()
    {
        explosionAudioSource = GetComponent<AudioSource>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!playedExplosion)
        {
            explosionAudioSource.PlayOneShot(explosionSfxClip, explosionVolume);
            playedExplosion = true;
        }
    }
}
