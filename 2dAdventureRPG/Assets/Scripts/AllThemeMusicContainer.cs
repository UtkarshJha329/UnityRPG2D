using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Track
{
    public List<int> trackPatterns = new List<int>();
    public float volume = 1.0f;
    public bool loopTrack = true;

    private int currentPlayingPatternIndex = 0;
    private List<bool> patternPlayingStatus = new List<bool>();
    
    public void InitTrack()
    {
        for (int i = 0; i < trackPatterns.Count; i++)
        {
            patternPlayingStatus.Add(false);
        }
    }

    public int PatternToPlayNowFromTrack()
    {
        if (currentPlayingPatternIndex == -1 || !patternPlayingStatus[currentPlayingPatternIndex])
        {
            currentPlayingPatternIndex++;

            if (currentPlayingPatternIndex >= patternPlayingStatus.Count)
            {
                if (loopTrack)
                {
                    currentPlayingPatternIndex = 0;

                    patternPlayingStatus[currentPlayingPatternIndex] = true;
                    return trackPatterns[currentPlayingPatternIndex];
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                //Debug.Log("From the track, playing new pattern number := " + currentPlayingPatternIndex);

                patternPlayingStatus[currentPlayingPatternIndex] = true;
                return trackPatterns[currentPlayingPatternIndex];
            }
        }
        else
        {
            //Debug.Log("From the track, already playing pattern number := " + currentPlayingPatternIndex);
            return -1;
        }

    }

    public void SetCurrentPlayingPatternToFinished()
    {
        patternPlayingStatus[currentPlayingPatternIndex] = false;
    }
}

[System.Serializable]
public class Theme
{
    public List<Track> themeTracks = new List<Track>();

    public void InitTheme()
    {
        for (int i = 0; i < themeTracks.Count; i++)
        {
            themeTracks[i].InitTrack();
        }
    }
}

public class AllThemeMusicContainer : MonoBehaviour
{
    [SerializeField] private List<AudioClip> themesMusicPatterns = new List<AudioClip>();
    [SerializeField] private AudioSource themeMusicSource;

    [SerializeField] private List<Theme> themeMusics = new List<Theme>();

    [SerializeField] private int mainMenuMusicIndex = 0;
    [SerializeField] private int combatMusicIndex = 1;

    [SerializeField] private const int skipPatternIndex8Bars140bpm = -2;
    [SerializeField] private const float skipPatternIndex8Bars140bpmTimeInSec = 6.86f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < themeMusics.Count; i++)
        {
            themeMusics[i].InitTheme();
        }
    }

    // Update is called once per frame
    void Update()
    {
        PlayMusic(mainMenuMusicIndex);
    }

    private void PlayMusic(int musicIndex)
    {
        for (int i = 0; i < themeMusics[musicIndex].themeTracks.Count; i++)
        {
            int nextPatternToPlayFromTrack = themeMusics[musicIndex].themeTracks[i].PatternToPlayNowFromTrack();
            if(nextPatternToPlayFromTrack != -1)
            {
                if(nextPatternToPlayFromTrack != skipPatternIndex8Bars140bpm)
                {
                    //Debug.Log("Playing pattern number := " + nextPatternToPlayFromTrack);
                    themeMusicSource.PlayOneShot(themesMusicPatterns[nextPatternToPlayFromTrack], themeMusics[musicIndex].themeTracks[i].volume);
                    StartCoroutine(SetCurrentTrackPatternPlayingToFalse(musicIndex, i, themesMusicPatterns[nextPatternToPlayFromTrack].length));
                }
                else
                {
                    StartCoroutine(SetCurrentTrackPatternPlayingToFalse(musicIndex, i, skipPatternIndex8Bars140bpmTimeInSec));
                }
            }
            else
            {
                //Debug.Log("Failed to get a new pattern to play.");
            }
        }
    }

    IEnumerator SetCurrentTrackPatternPlayingToFalse(int musicIndex, int trackIndex, float durationOfPatternPlaying)
    {
        yield return new WaitForSeconds(durationOfPatternPlaying);

        themeMusics[musicIndex].themeTracks[trackIndex].SetCurrentPlayingPatternToFinished();
    }
}
