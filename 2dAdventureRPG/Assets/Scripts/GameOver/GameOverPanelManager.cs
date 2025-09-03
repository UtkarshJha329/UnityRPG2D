using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverPanelManager : MonoBehaviour
{
    public string mainMenuSceneName = "MainMenuScene";

    public GameObject youDiedTextGameObject;
    public GameObject youWinTextGameObject;

    public GameObject gameOverPanelContents;

    public void Start()
    {
        gameOverPanelContents.SetActive(false);
        youDiedTextGameObject.SetActive(false);
        youWinTextGameObject.SetActive(false);
    }

    private void Update()
    {
        if(GameStats.playerFinishedFinalCutscene && GameStats.finalRoomConvertedIntoGrassFully)
        {
            if (GameStats.gameOverState == 1)
            {
                youWinTextGameObject.SetActive(true);
                gameOverPanelContents.SetActive(true);
            }
        }
        else if (GameStats.gameOverState == -1)
        {
            youDiedTextGameObject.SetActive(true);
            gameOverPanelContents.SetActive(true);
        }
    }

    public void ShowGameOverPanel(bool youWin)
    {
        if (youWin)
        {
            youDiedTextGameObject.SetActive(false);
        }
        else
        {
            youDiedTextGameObject.SetActive(true);
        }

        gameOverPanelContents.SetActive(true);
    }

    public void OnClickLevelRestartButton()
    {
        ResetGameStats();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnClickReturnToMainMenuButton()
    {
        ResetGameStats();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void ResetGameStats()
    {
        Time.timeScale = 1.0f;

        GameStats.gameOverState = 0;
        GameStats.finalStructuresHaveBeenDestroyed = false;
        GameStats.playerReachedCutSceneTile = false;
        GameStats.playerFinishedFinalCutscene = false;
        GameStats.finalRoomConvertedIntoGrassFully = false;

        Debug.Log("Reset game stats.");
    }
}
