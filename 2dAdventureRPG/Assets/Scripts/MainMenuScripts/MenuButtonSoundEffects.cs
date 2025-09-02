using UnityEngine;

public class MenuButtonSoundEffects : MonoBehaviour
{
    public AudioClip lowCumbleAudioClip;
    public AudioClip loudCumbleAudioClip;

    public void PlayLowCrumbleSoundEffectOnHover(GameObject buttonGameObject)
    {
        buttonGameObject.GetComponent<AudioSource>().pitch = Random.Range(0.45f, 0.65f);
        buttonGameObject.GetComponent<AudioSource>().PlayOneShot(lowCumbleAudioClip, Random.Range(0.65f, 0.85f));
    }

    public void PlayLoudCrumbleSoundEffectOnHover(GameObject buttonGameObject)
    {
        buttonGameObject.GetComponent<AudioSource>().pitch = Random.Range(0.65f, 0.75f);
        buttonGameObject.GetComponent<AudioSource>().PlayOneShot(loudCumbleAudioClip, Random.Range(0.65f, 0.85f));
    }
}
