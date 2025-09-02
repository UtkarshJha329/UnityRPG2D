using System.Collections;
using System.ComponentModel;
using UnityEngine;

public class TimeStop : MonoBehaviour
{
    public static bool waiting = false;

    public void StopTimeFor(float duration, float scale)
    {
        if (!waiting)
        {
            //Debug.Log("Starting time stop for " + duration + " seconds with scale " + scale + ".");
            waiting = true;
            Time.timeScale = scale;
            StartCoroutine(WaitForSecondsBeforeResettingTimeScale(duration));
        }
    }

    IEnumerator WaitForSecondsBeforeResettingTimeScale(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1.0f;
        waiting = false;
    }

}
