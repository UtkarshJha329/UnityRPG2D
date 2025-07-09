using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

[System.Serializable]
public class Track
{
    public List<int> trackPatterns = new List<int>();
    public float volume = 1.0f;
    public bool loopTrack = true;

    public bool muteTrack = false;

    [HideInInspector] public int trackIndex = -1;

    private int currentPlayingPatternIndex = -1;
    private List<bool> patternPlayingStatus = new List<bool>();
    
    public void InitTrack(int trackIndex)
    {
        for (int i = 0; i < trackPatterns.Count; i++)
        {
            patternPlayingStatus.Add(false);
        }

        this.trackIndex = trackIndex;
    }

    public int PatternToPlayNowFromTrack()
    {
        //if (muteTrack)
        //{
        //    return -2;
        //}

        if (currentPlayingPatternIndex == -1 || !patternPlayingStatus[currentPlayingPatternIndex])
        {
            currentPlayingPatternIndex++;

            if (currentPlayingPatternIndex >= patternPlayingStatus.Count)
            {
                //if(trackIndex == 0)
                //{
                //    Debug.Log("Current Playing Pattern Index := " + currentPlayingPatternIndex + ", patternPlayingStatus.Count" + patternPlayingStatus.Count);
                //}
                if (loopTrack)
                {
                    //if(trackIndex == 0)
                    //{
                    //    Debug.Log("Finished playing track (" + trackIndex + ") at := " + Time.time + ", now looping.");
                    //}
                    Debug.Log("Finished playing track (" + trackIndex + ") at := " + Time.time + ", now looping.");

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
                //if(currentPlayingPatternIndex == 0)
                //{
                //    Debug.Log("Played 0 pattern.");
                //}
                //Debug.Log("Playing new pattern (" + currentPlayingPatternIndex + ") at := " + Time.time);
                //if (trackIndex == 0)
                //{
                //    Debug.Log("Playing next := " + trackPatterns[currentPlayingPatternIndex]);
                //}
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
            themeTracks[i].InitTrack(i);
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
    [SerializeField] private const int skipPatternIndex1Bars140bpm = -3;
    [SerializeField] private const float skipPatternIndex4Bars140bpmTimeInSec = 6.86f;
    [SerializeField] private const float skipPatternIndex1Bars140bpmTimeInSec = 6.86f / 4.0f;
    //[SerializeField] private const float skipPatternIndex1Bars140bpmTimeInSec = 1.72f;

    private bool once = true;

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
        //if (once)
        //{
        //    //Debug.Log("Started playing track at := " + Time.time);
        //    once = false;
        //}
        //PlayMusic(mainMenuMusicIndex);
        PlayMusic(combatMusicIndex);
    }

    private void PlayMusic(int musicIndex)
    {
        for (int i = 0; i < themeMusics[musicIndex].themeTracks.Count; i++)
        {
            int nextPatternToPlayFromTrack = themeMusics[musicIndex].themeTracks[i].PatternToPlayNowFromTrack();
            if(nextPatternToPlayFromTrack != -1)
            {
                if(nextPatternToPlayFromTrack != skipPatternIndex8Bars140bpm && nextPatternToPlayFromTrack != skipPatternIndex1Bars140bpm)
                {
                    //Debug.Log("Playing pattern number := " + nextPatternToPlayFromTrack);
                    if (!themeMusics[musicIndex].themeTracks[i].muteTrack)
                    {
                        themeMusicSource.PlayOneShot(themesMusicPatterns[nextPatternToPlayFromTrack], themeMusics[musicIndex].themeTracks[i].volume);
                    }
                    StartCoroutine(SetCurrentTrackPatternPlayingToFalse(musicIndex, i, themesMusicPatterns[nextPatternToPlayFromTrack].length));
                    //StartCoroutine(SetCurrentTrackPatternPlayingToFalse(musicIndex, i, skipPatternIndex8Bars140bpmTimeInSec));
                }
                else if(nextPatternToPlayFromTrack == skipPatternIndex8Bars140bpm)
                {
                    StartCoroutine(SetCurrentTrackPatternPlayingToFalse(musicIndex, i, skipPatternIndex4Bars140bpmTimeInSec));
                }
                else if(nextPatternToPlayFromTrack == skipPatternIndex1Bars140bpm)
                {
                    //if (!themeMusics[musicIndex].themeTracks[i].muteTrack)
                    //{
                    //    Debug.Log("Loading reset after := " + skipPatternIndex1Bars140bpmTimeInSec + " seconds.");
                    //}

                    StartCoroutine(SetCurrentTrackPatternPlayingToFalse(musicIndex, i, skipPatternIndex1Bars140bpmTimeInSec));
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

        //if (trackIndex == 0)
        //{
        //    Debug.Log("Resetting after := " + durationOfPatternPlaying + " seconds.");
        //}

        themeMusics[musicIndex].themeTracks[trackIndex].SetCurrentPlayingPatternToFinished();
    }
}
