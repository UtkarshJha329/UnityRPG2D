using UnityEngine;

public class GameStats : MonoBehaviour
{
    public bool fighting = false;
    public bool finishedCurrentSection = true;

    public int currentSectionIndex = -1;
    public int previousSectionIndex = -1;

    public bool WasPreviousSectionCompleted()
    {
        // return number of enemies in last section or simply store the state of the number of enemies alive in the last section when you leave it?.

        return false;
    }

    public bool WasSectionCompletedInTheLastSomeTime()
    {
        // return some arbiritary decay timer value is greater than zero that resets each time the player kills all the enemies in a section.

        return false;
    }
}
