using System.Collections;
using UnityEngine;

public class MainMenuMusicHandler : MonoBehaviour
{
    public AudioClip mainMenuMusic;
    public AudioSource mainMenuAudioSource;

    public float mainMenuMusicFadeInOutTime = 1.5f;
    public float mainMenuMusicFadeInOutSampleRate = 0.016f;

    private float perInvoke_mainMenuMusicFadeInOutDeltaVolume = 0.1f;

    private float oldMasterVolume = -1.0f;

    private void Awake()
    {
        mainMenuAudioSource = GetComponent<AudioSource>();
        mainMenuAudioSource.clip = mainMenuMusic;
    }

    private void Start()
    {
        perInvoke_mainMenuMusicFadeInOutDeltaVolume = mainMenuMusicFadeInOutSampleRate / mainMenuMusicFadeInOutTime;
    }

    private void Update()
    {
        if (GameSettings.Instance.MASTER_VOLUME != oldMasterVolume)
        {
            if (GameSettings.Instance.MASTER_VOLUME == 1.0f)
            {
                mainMenuAudioSource.Play();
                mainMenuAudioSource.volume = 0.0f;
                mainMenuAudioSource.loop = true;
                InvokeRepeating("SlowlyIncreaseMainMenuAudioSourceVolume", 0.0f, mainMenuMusicFadeInOutSampleRate);
                Invoke("StopSlowlyIncreaseMainMenuAudioSourceVolume", mainMenuMusicFadeInOutTime);
            }
            else
            {
                mainMenuAudioSource.Stop();
            }

            oldMasterVolume = GameSettings.Instance.MASTER_VOLUME;
        }
    }

    private void SlowlyIncreaseMainMenuAudioSourceVolume()
    {
        mainMenuAudioSource.volume += perInvoke_mainMenuMusicFadeInOutDeltaVolume;

        mainMenuAudioSource.volume = Mathf.Clamp01(mainMenuAudioSource.volume);
    }

    private void StopSlowlyIncreaseMainMenuAudioSourceVolume()
    {
        CancelInvoke("SlowlyIncreaseMainMenuAudioSourceVolume");
    }
}
