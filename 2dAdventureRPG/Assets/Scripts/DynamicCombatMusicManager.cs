using System.Collections.Generic;
using UnityEngine;

public enum CombatState
{
    Idling,
    Winning,
    Losing
}

[System.Serializable]
public class ThemeGroupsTracksData
{
    public List<int> tracksIncluded = new List<int>();
}

public class DynamicCombatMusicManager : MonoBehaviour
{
    public CombatState combatState;

    public ThemeGroupsTracksData winningCombatThemeGroupsTracksData;
    public ThemeGroupsTracksData idlingCombatSlowThemeGroupsTracksData;
    public ThemeGroupsTracksData losingCombatThemeGroupsTracksData;
    public ThemeGroupsTracksData idlingCombatUpbeatThemeGroupsTracksData;

    private const int winningTrackSplitIndex = 0;
    private const int losingTrackSplitIndex = 1;

    private AllThemeMusicContainer themeMusicHandler;

    private int combatMusicIndex = 1;
    private int currentTrackListIndex = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        themeMusicHandler = GetComponent<AllThemeMusicContainer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetThemeGroup(idlingCombatSlowThemeGroupsTracksData, 1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetThemeGroup(winningCombatThemeGroupsTracksData, 1);       // Ah Ah Ah Ah Ah Ah Ah Ah Ah Ah Ah Ah Ahhhhhhhhhhh Ah Ah
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetThemeGroup(idlingCombatUpbeatThemeGroupsTracksData, 1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SetThemeGroup(losingCombatThemeGroupsTracksData, 1);        // Oh Oh Oh Oh Oh Oh Oh Oh Oh Oh Oh Oh Ohhhhhhhhhhhh Oh Oh Oh
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            themeMusicHandler.interruptWithOtherTrackSplit = true;
        }

        if (!themeMusicHandler.interruptWithOtherTrackSplit)
        {
            themeMusicHandler.PlayMusic(combatMusicIndex);
        }
        else
        {
            int trackListIndexToInterruptWith = currentTrackListIndex == 0 ? 1 : 0;
            if(themeMusicHandler.InterruptMusicToChangeTrackLists(combatMusicIndex, trackListIndexToInterruptWith))
            {
                currentTrackListIndex = trackListIndexToInterruptWith;
            }
        }
    }

    private void SetThemeGroup(ThemeGroupsTracksData themeGroupToPlay, int themeTrackIndex)
    {
        for (int i = 0; i < themeMusicHandler.themeMusics[combatMusicIndex].themeTracks.Count; i++)
        {
            if (themeGroupToPlay.tracksIncluded.Contains(i))
            {
                themeMusicHandler.UnMuteAudioSourceForTrack(combatMusicIndex, i);
            }
            else
            {
                themeMusicHandler.MuteAudioSourceForTrack(combatMusicIndex, i);
            }
        }
    }
}
