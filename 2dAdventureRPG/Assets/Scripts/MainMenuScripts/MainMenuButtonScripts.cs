using JetBrains.Annotations;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuButtonScripts : MonoBehaviour
{
    public TMP_InputField inputField;

    public GameObject rulesPanel;

    private CanvasGroup rulesMenuCanvasGroup;

    private AudioSource mainMenuAudioSource;
    public AudioClip pageTurningAudioClip;

    private void Awake()
    {
        mainMenuAudioSource = GetComponent<AudioSource>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rulesPanel.SetActive(false);

        rulesMenuCanvasGroup = rulesPanel.GetComponent<CanvasGroup>();
        rulesMenuCanvasGroup.alpha = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartButtonOnClickFunction()
    {
        string gameSceneName = "GameBalanceScene";
        //SceneManager.LoadScene(gameSceneName);

        StartCoroutine(LoadSceneAsync(gameSceneName));

    }

    public void OnSeedTextSubmit()
    {
        GameSettings.Instance.GAME_SEED = inputField.text;
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false; // Prevent scene from activating immediately

        while (!operation.isDone)
        {
            // Progress is typically from 0 to 0.9, representing loading progress
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            //progressBar.value = progress;

            // When loading is almost complete, allow scene activation
            if (operation.progress >= 0.9f)
            {
                GameSettings.SetSeed();
                // Optionally, you can wait for a key press or a delay here
                operation.allowSceneActivation = true;
                //loadingScreenPanel.SetActive(false); // Hide the loading screen after activation
            }

            yield return null; // Wait for the next frame
        }
    }

    public void ToggleGameSoundsOnClickFunction()
    {
        GameSettings.Instance.MASTER_VOLUME = GameSettings.Instance.MASTER_VOLUME == 0.0f ? 1.0f : 0.0f;
        AudioListener.volume = GameSettings.Instance.MASTER_VOLUME;
    }

    public void OpenRulesPanel()
    {
        rulesPanel.SetActive(true);
        rulesMenuCanvasGroup.alpha = 0.0f;

        InvokeRepeating("IncreaseRulesPanelCanvasGroupAlpha", 0.0f, 0.1f);
        Invoke("PlayPageTurningSoundEffect", 0.25f);
        Invoke("StopInvokingRulesPanelCanvasGroupAplhaChangingMethodForOpening", 1.5f);
    }

    private void IncreaseRulesPanelCanvasGroupAlpha()
    {
        rulesMenuCanvasGroup.alpha += 0.1f;
    }

    private void StopInvokingRulesPanelCanvasGroupAplhaChangingMethodForOpening()
    {
        CancelInvoke("IncreaseRulesPanelCanvasGroupAlpha");
    }
    
    private void PlayPageTurningSoundEffect()
    {
        mainMenuAudioSource.PlayOneShot(pageTurningAudioClip);
    }

    public void CloseRulesPanel()
    {
        InvokeRepeating("DecreaseRulesPanelCanvasGroupAlpha", 0.0f, 0.1f);
        Invoke("PlayPageTurningSoundEffect", 0.15f);
        Invoke("StopInvokingRulesPanelCanvasGroupAplhaChangingMethodForClosing", 1.5f);
    }

    private void DecreaseRulesPanelCanvasGroupAlpha()
    {
        rulesMenuCanvasGroup.alpha -= 0.1f;
    }

    private void StopInvokingRulesPanelCanvasGroupAplhaChangingMethodForClosing()
    {
        CancelInvoke("DecreaseRulesPanelCanvasGroupAlpha");
        rulesPanel.SetActive(false);
    }
}
