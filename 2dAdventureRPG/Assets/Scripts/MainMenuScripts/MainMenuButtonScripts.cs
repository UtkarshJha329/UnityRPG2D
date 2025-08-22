using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuButtonScripts : MonoBehaviour
{
    public TMP_InputField inputField;

    public GameObject loadingScreenPanel;
    public Slider progressBar;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        loadingScreenPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartButtonOnClickFunction()
    {
        string gameSceneName = "GameBalanceScene";
        //SceneManager.LoadScene(gameSceneName);

        loadingScreenPanel.SetActive(true);
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
            progressBar.value = progress;

            // When loading is almost complete, allow scene activation
            if (operation.progress >= 0.9f)
            {
                if(GameSettings.Instance.GAME_SEED.Length == 0)
                {
                    GameSettings.Instance.GAME_SEED = Random.Range(197, 1283128).ToString();
                }
                Random.InitState(GameSettings.Instance.GAME_SEED.GetHashCode());
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
}
