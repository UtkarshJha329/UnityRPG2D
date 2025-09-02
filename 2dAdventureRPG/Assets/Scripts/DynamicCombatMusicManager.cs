using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

    private const int combatMusicIndex = 1;
    private int currentTrackListIndex = 0;

    private PlayerHealth s_PlayerHealth;
    private GameStats s_GameStats;

    private const int angelChoirKillstreakCount = 10;
    private int playerHealthForHarshChoir = -1;      // set in update
    private int playerHealthForTenseMusic = -1;      // set in update

    private bool changedTrackList = false;

    private const int angelTracklist = 0;
    private const int harshTracklist = 1;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        themeMusicHandler = GetComponent<AllThemeMusicContainer>();

        s_GameStats = GameObject.FindGameObjectWithTag("GameStatsManager").GetComponent<GameStats>();

    }

    // Update is called once per frame
    void Update()
    {
        if(s_PlayerHealth == null)
        {
            s_PlayerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();

            playerHealthForTenseMusic = (int)s_PlayerHealth.GetMaxPlayerHealth() / 2;
            playerHealthForHarshChoir = (int)s_PlayerHealth.GetMaxPlayerHealth() / 4;
        }

        SetCorrectThemeGroup();
    }

    private void SetCorrectThemeGroup()
    {
        // Low kill streak simple music
        // High kill streak angle music
        // health falls low without high kill streak tense music
        // health falls very low while high kill streak harsh music

        // kill streak takes priority over health (so angel choir takes priority over harsh choir) ? Or some combination?

        // high kill streak switch to angel track spit
        // low health switch to harsh track split

        int changeToTrackList = -1;
        if (s_GameStats.currentKillStreak >= angelChoirKillstreakCount)
        {
            SetThemeGroup(winningCombatThemeGroupsTracksData, 1);       // Ah Ah Ah Ah Ah Ah Ah Ah Ah Ah Ah Ah Ahhhhhhhhhhh Ah Ah
            changeToTrackList = angelTracklist;
        }
        else if(s_GameStats.currentKillStreak > 0 && s_PlayerHealth.GetCurrentPlayerHealth() > playerHealthForTenseMusic)
        {
            SetThemeGroup(idlingCombatSlowThemeGroupsTracksData, 1);    // slow
            changeToTrackList = angelTracklist;
        }
        else if(s_PlayerHealth.GetCurrentPlayerHealth() <= playerHealthForHarshChoir)
        {
            SetThemeGroup(losingCombatThemeGroupsTracksData, 1);        // Oh Oh Oh Oh Oh Oh Oh Oh Oh Oh Oh Oh Ohhhhhhhhhhhh Oh Oh Oh
            changeToTrackList = harshTracklist;
        }
        else if(s_PlayerHealth.GetCurrentPlayerHealth() <= playerHealthForTenseMusic)
        {
            SetThemeGroup(idlingCombatUpbeatThemeGroupsTracksData, 1);  // upbeat
            changeToTrackList = harshTracklist;
        }
        else
        {
            SetThemeGroup(winningCombatThemeGroupsTracksData, 1);       // Ah Ah Ah Ah Ah Ah Ah Ah Ah Ah Ah Ah Ahhhhhhhhhhh Ah Ah
            changeToTrackList = angelTracklist;
        }

        if (currentTrackListIndex != changeToTrackList)
        {
            themeMusicHandler.interruptWithOtherTrackSplit = true;
        }

        if (!themeMusicHandler.interruptWithOtherTrackSplit)
        {
            themeMusicHandler.PlayMusic(combatMusicIndex);
        }
        else
        {
            if (themeMusicHandler.InterruptMusicToChangeTrackLists(combatMusicIndex, changeToTrackList))
            {
                currentTrackListIndex = changeToTrackList;
                //Debug.Log("Interrupted with new track list : " + currentTrackListIndex);
            }
        }

    }

    private void SetThemeGroup(ThemeGroupsTracksData themeGroupToPlay, int themeTrackIndex)
    {
        for (int i = 0; i < themeMusicHandler.themeMusics[combatMusicIndex].themeTracks.Count; i++)
        {
            if (themeGroupToPlay.tracksIncluded.Contains(i))
            {
                if(themeMusicHandler.IsAudioSourceForTrackMuted(combatMusicIndex, i))
                {
                    themeMusicHandler.ApplyFadeInToTrack(combatMusicIndex, i);
                }
                themeMusicHandler.UnMuteAudioSourceForTrack(combatMusicIndex, i);
            }
            else
            {
                themeMusicHandler.MuteAudioSourceForTrack(combatMusicIndex, i);
            }
        }
    }
}
