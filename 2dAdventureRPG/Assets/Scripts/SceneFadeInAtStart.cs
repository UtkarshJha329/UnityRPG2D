using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class SceneFadeInAtStart : MonoBehaviour
{
    private CanvasGroup fadeInBlockingCanvasGroup;

    public float fadeOutTime = 1.5f;
    public float fadeOutAlphaPerInvoke = 0.016f;

    private float fadeOutInvokeRate = 0.1f;

    private void Start()
    {
        fadeInBlockingCanvasGroup = GetComponent<CanvasGroup>();
        fadeInBlockingCanvasGroup.alpha = 1.0f;

        fadeOutInvokeRate = fadeOutAlphaPerInvoke * fadeOutTime;
        StartFadeOutForCanvasGroup();
    }

    public void StartFadeOutForCanvasGroup()
    {
        InvokeRepeating("DecreaseCanvasGroupAlpha", 0.0f, fadeOutInvokeRate);
        Invoke("StopInvokingStartFadeOutForCanvasGroup", fadeOutTime);
    }

    private void DecreaseCanvasGroupAlpha()
    {
        fadeInBlockingCanvasGroup.alpha -= fadeOutAlphaPerInvoke;
    }

    private void StopInvokingStartFadeOutForCanvasGroup()
    {
        CancelInvoke("DecreaseRulesPanelCanvasGroupAlpha");
        fadeInBlockingCanvasGroup.gameObject.SetActive(false);
    }
}
