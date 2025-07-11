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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
