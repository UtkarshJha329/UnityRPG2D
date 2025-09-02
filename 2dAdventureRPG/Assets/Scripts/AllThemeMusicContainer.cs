using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VolumePatterns
{
    FadeIn,
    Constant,
    FadeOut
}

[System.Serializable]
public class TrackSplitMixingRules
{
    public bool ignoreVolumePatternsForInBetweenPlays = false;

    public VolumePatterns introVolumePattern;
    public VolumePatterns outroVolumePattern;

    // Add a variable to check if track split should end with track playing or at bar time.
}

[System.Serializable]
public class TrackSplit
{
    public int trackSplitPatternIndexStart;
    public int trackSplitPatternIndexEnd;

    public TrackSplitMixingRules mixingRules;
}

[System.Serializable]
public class Track
{
    public List<int> trackPatterns = new List<int>();
    public float volume = 1.0f;
    public bool loopTrack = true;

    public List<TrackSplit> trackSplits = new List<TrackSplit>();

    public bool muteTrack = false;

    [HideInInspector] public int trackIndex = -1;

    private int currentPlayingTrackSplitIndex = 0;  // TODO expose better for more control <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    private int currentPlayingPatternIndex = -1;
    private List<bool> patternPlayingStatus = new List<bool>();

    public void SetCurrentPlayingTrackSplitIndex(int value)
    {
        currentPlayingTrackSplitIndex = value;
    }

    public void SetCurrentPlayingPatternIndexToTrackSplitIndexStart(int trackSplitIndex)
    {
        currentPlayingPatternIndex = trackSplits[trackSplitIndex].trackSplitPatternIndexStart;
    }

    public void SetCurrentTrackSplitIndexAndPatternIndexToTracksStart(int trackSplitIndex)
    {
        SetCurrentPlayingTrackSplitIndex(trackSplitIndex);
        SetCurrentPlayingPatternIndexToTrackSplitIndexStart(trackSplitIndex);
    }

    public void InitTrack(int trackIndex)
    {
        for (int i = 0; i < trackPatterns.Count; i++)
        {
            patternPlayingStatus.Add(false);
        }

        this.trackIndex = trackIndex;
    }

    public void InterruptWithNewTrackSplitIndex(int newTrackSplitIndex)
    {
        SetCurrentTrackSplitIndexAndPatternIndexToTracksStart(newTrackSplitIndex);
        currentPlayingPatternIndex--;
    }

    public bool IsTrackPlaying()
    {
        return patternPlayingStatus[currentPlayingPatternIndex];
    }

    public int PatternToPlayNowFromTrack(ref VolumePatterns volumePatternToUseToPlayThisPattern)
    {
        if(currentPlayingTrackSplitIndex < trackSplits.Count)
        {
            //if (currentPlayingTrackSplitIndex == -1 || currentPlayingPatternIndex > trackSplits[currentPlayingTrackSplitIndex].trackSplitPatternIndexEnd)
            //{
            //    currentPlayingTrackSplitIndex++;
            //    Debug.Log("Increased track (" + trackIndex + ") split index to := " + currentPlayingTrackSplitIndex);
            //}

            if (currentPlayingPatternIndex == -1 || !patternPlayingStatus[currentPlayingPatternIndex])
            {
                if (currentPlayingPatternIndex == -1)
                {
                    //currentPlayingPatternIndex = trackSplits[currentPlayingTrackSplitIndex].trackSplitPatternIndexStart;
                    SetCurrentPlayingPatternIndexToTrackSplitIndexStart(currentPlayingTrackSplitIndex);
                    currentPlayingPatternIndex--;
                }

                bool previousPatternWasEmpty = true;
                if(currentPlayingPatternIndex != -1 && currentPlayingPatternIndex < patternPlayingStatus.Count)
                {
                    previousPatternWasEmpty = trackPatterns[currentPlayingPatternIndex] < 0;
                }

                currentPlayingPatternIndex++;

                //if (currentPlayingPatternIndex >= patternPlayingStatus.Count)
                if (currentPlayingPatternIndex > trackSplits[currentPlayingTrackSplitIndex].trackSplitPatternIndexEnd)
                {
                    if (loopTrack)
                    {
                        currentPlayingPatternIndex = trackSplits[currentPlayingTrackSplitIndex].trackSplitPatternIndexStart;

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
                    bool currentPatternIsNotEmpty = trackPatterns[currentPlayingPatternIndex] >= 0;

                    if(currentPatternIsNotEmpty && previousPatternWasEmpty)
                    {
                        //Debug.Log("On track := " + trackIndex + " on pattern := " + currentPlayingPatternIndex + " on split index := " + currentPlayingTrackSplitIndex + " set volumePatternToUseToPlayThisPattern to := " + trackSplits[currentPlayingTrackSplitIndex].mixingRules.introVolumePattern);
                        volumePatternToUseToPlayThisPattern = trackSplits[currentPlayingTrackSplitIndex].mixingRules.introVolumePattern;
                    }
                    //if(trackIndex == 3 && currentPlayingPatternIndex >= 8)
                    //{
                    //    Debug.Log("Playing oohs.");
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
        else
        {
            //Debug.Log("From the track, already playing pattern number := " + currentPlayingPatternIndex);
            currentPlayingTrackSplitIndex = 0;
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

public class TracksFadingProperties
{
    public int themeMusicIndex = -1;
    public int trackIndex = -1;
    public float fadingInTrackStartTime = 0.0f;

    public TracksFadingProperties(int themeMusicIndex, int trackIndex, float fadingInTrackStartTime)
    {
        this.themeMusicIndex = themeMusicIndex;
        this.trackIndex = trackIndex;
        this.fadingInTrackStartTime = fadingInTrackStartTime;
    }
}

public class AllThemeMusicContainer : MonoBehaviour
{
    [SerializeField] private List<AudioClip> themesMusicPatterns = new List<AudioClip>();
    [SerializeField] private AudioSource themeMusicSource;

    [SerializeField] public List<Theme> themeMusics = new List<Theme>();

    [SerializeField] private int mainMenuMusicIndex = 0;
    [SerializeField] private int combatMusicIndex = 1;

    [SerializeField] private const int skipPatternIndex8Bars140bpm = -2;
    [SerializeField] private const int skipPatternIndex1Bars140bpm = -3;
    [SerializeField] private const float skipPatternIndex4Bars140bpmTimeInSec = 6.86f;
    [SerializeField] private const float skipPatternIndex1Bars140bpmTimeInSec = 6.86f / 4.0f;
    //[SerializeField] private const float skipPatternIndex1Bars140bpmTimeInSec = 1.72f;

    [SerializeField] private GameObject trackAudioSourceGameObject;

    private bool once = true;

    private List<TracksFadingProperties> tracksToFadeIn = new List<TracksFadingProperties>();
    private float fadeInBeginVolume = 0.1f;
    private float fadeInFinalVolume = 1.0f;
    private float fadeInTime = 0.5f;

    private List<List<AudioSource>> audioSourcePerThemePerTrack = new List<List<AudioSource>>();

    public bool interruptWithOtherTrackSplit = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (trackAudioSourceGameObject == null)
        {
            Debug.LogError("trackAudioSourceGameObect reference in " + gameObject.name + " AllThemeMusicContainer component is missing.");
        }

        for (int i = 0; i < themeMusics.Count; i++)
        {
            themeMusics[i].InitTheme();
        }

        for (int i = 0; i < themeMusics.Count; i++)
        {
            audioSourcePerThemePerTrack.Add(new List<AudioSource>());
            for (int j = 0; j < themeMusics[i].themeTracks.Count; j++)
            {
                GameObject instantiatedTrackAudioSourceGameObject = Instantiate(trackAudioSourceGameObject, transform);
                instantiatedTrackAudioSourceGameObject.name = "Theme : (" + i + ") track : (" + j + ")";
                audioSourcePerThemePerTrack[i].Add(instantiatedTrackAudioSourceGameObject.GetComponent<AudioSource>());
            }
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
    }

    public bool InterruptMusicToChangeTrackLists(int musicIndex, int trackListIndexToInterruptWith)
    {
        //bool someTrackHasBeenPlayingFromBeforeInterruption = false;
        for (int i = 0; i < themeMusics[musicIndex].themeTracks.Count; i++)
        {
            //if (audioSourcePerThemePerTrack[musicIndex][i].isPlaying)
            if (themeMusics[musicIndex].themeTracks[i].IsTrackPlaying())
            {
                //someTrackHasBeenPlayingFromBeforeInterruption = true;
                return false;
            }
        }

        for (int i = 0; i < themeMusics[musicIndex].themeTracks.Count; i++)
        {
            themeMusics[musicIndex].themeTracks[i].InterruptWithNewTrackSplitIndex(trackListIndexToInterruptWith);
        }

        interruptWithOtherTrackSplit = false;

        return true;
    }

    public void MuteAudioSourceForTrack(int musicIndex, int trackIndex)
    {
        audioSourcePerThemePerTrack[musicIndex][trackIndex].mute = true;
    }

    public bool IsAudioSourceForTrackMuted(int musicIndex, int trackIndex)
    {
        return audioSourcePerThemePerTrack[musicIndex][trackIndex].mute;
    }

    public void UnMuteAudioSourceForTrack(int musicIndex, int trackIndex)
    {
        audioSourcePerThemePerTrack[musicIndex][trackIndex].mute = false;
    }

    public void ApplyFadeInToTrack(int musicIndex, int trackIndex)
    {
        tracksToFadeIn.Add(new TracksFadingProperties(musicIndex, trackIndex, Time.time));
        audioSourcePerThemePerTrack[musicIndex][trackIndex].volume = fadeInBeginVolume;
    }

    public void PlayMusic(int musicIndex)
    {
        for (int i = 0; i < themeMusics[musicIndex].themeTracks.Count; i++)
        {
            VolumePatterns volumePatternToUseToPlayThisPattern = VolumePatterns.Constant;
            int nextPatternToPlayFromTrack = themeMusics[musicIndex].themeTracks[i].PatternToPlayNowFromTrack(ref volumePatternToUseToPlayThisPattern);
            if(nextPatternToPlayFromTrack != -1)
            {
                if(nextPatternToPlayFromTrack != skipPatternIndex8Bars140bpm && nextPatternToPlayFromTrack != skipPatternIndex1Bars140bpm)
                {
                    //Debug.Log("Playing pattern number := " + nextPatternToPlayFromTrack);
                    if (!themeMusics[musicIndex].themeTracks[i].muteTrack)
                    {
                        //themeMusicSource.PlayOneShot(themesMusicPatterns[nextPatternToPlayFromTrack], themeMusics[musicIndex].themeTracks[i].volume);
                        audioSourcePerThemePerTrack[musicIndex][i].clip = themesMusicPatterns[nextPatternToPlayFromTrack];
                        audioSourcePerThemePerTrack[musicIndex][i].volume = themeMusics[musicIndex].themeTracks[i].volume;
                        audioSourcePerThemePerTrack[musicIndex][i].Play();

                        if (volumePatternToUseToPlayThisPattern == VolumePatterns.FadeIn)
                        {
                            tracksToFadeIn.Add(new TracksFadingProperties(musicIndex, i, Time.time));
                            audioSourcePerThemePerTrack[musicIndex][i].volume = fadeInBeginVolume;
                        }
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

        int removedCount = 0;
        for (int i = 0; i < tracksToFadeIn.Count - removedCount; i++)
        {
            float volumeIncreaseByPercentGivenFadeInTime = fadeInFinalVolume - fadeInBeginVolume / fadeInTime;
            audioSourcePerThemePerTrack[tracksToFadeIn[i].themeMusicIndex][tracksToFadeIn[i].trackIndex].volume += volumeIncreaseByPercentGivenFadeInTime * Time.deltaTime;

            //float finalFadeTime = tracksToFadeIn[i].fadingInTrackStartTime + fadeInTime;
            //float fadedInByPercent = (finalFadeTime - Time.time) / fadeInTime;

            //if (fadedInByPercent <= 1.0f)
            //{
            //    Debug.Log("Faded in by percent := " + fadedInByPercent);
            //    float currentFadedVolume = Mathf.Lerp(fadeInBeginVolume, themeMusics[musicIndex].themeTracks[tracksToFadeIn[i].trackIndex].volume, fadedInByPercent);
            //    //themeMusicSource.volume = currentFadedVolume;
            //    audioSourcePerThemePerTrack[tracksToFadeIn[i].themeMusicIndex][tracksToFadeIn[i].trackIndex].volume = currentFadedVolume;
            //}
            //else
            //{
            //    tracksToFadeIn.RemoveAt(i);
            //    removedCount++;
            //    i--;
            //}

            if (audioSourcePerThemePerTrack[tracksToFadeIn[i].themeMusicIndex][tracksToFadeIn[i].trackIndex].volume >= 1.0f)
            {
                tracksToFadeIn.RemoveAt(i);
                removedCount++;
                i--;
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
