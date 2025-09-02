using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms.Impl;

public class ScoreManager : MonoBehaviour
{
    public TMP_Text scoreTextComponent;
    public AudioSource scoreAudioSource;

    public List<AudioClip> scoreIncreaseAudioClips = new List<AudioClip>();

    public AudioClip playerDeathAudioClip;

    public int torchGoblinScore = 10;
    public int barrelGoblinScore = 50;
    public int TNTGoblinScore = 100;

    public int minesDestroyedScore = 10000;

    public int gameWinScore = 1000000;

    public float waitForSecondsBetweenEachScoreUpdate = 0.05f;

    private GameStats s_GameStats;

    private bool startedDelayedScore = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        s_GameStats = GameObject.FindGameObjectWithTag("GameStatsManager").GetComponent<GameStats>();
        scoreTextComponent.text = "0";
    }

    // Update is called once per frame
    void Update()
    {
        if(GameStats.gameOverState != 0)
        {
            //scoreTextComponent.text = GetRunScore().ToString();
            if (!startedDelayedScore)
            {
                //FakeScore(10, 10, 10);
                scoreAudioSource.PlayOneShot(playerDeathAudioClip);

                StartCoroutine(DelayedSetScore());
                startedDelayedScore = true;
            }
        }
    }

    private void FakeScore(int numTorchGoblinsKilled, int numBarrelGoblinsKilled, int numTNTGoblinsKilled)
    {
        for (int i = 0; i < numTorchGoblinsKilled; i++)
        {
            s_GameStats.KilledTorchGoblin();
        }
        for (int i = 0; i < numBarrelGoblinsKilled; i++)
        {
            s_GameStats.KilledBarrelGoblin();
        }
        for (int i = 0; i < numTNTGoblinsKilled; i++)
        {
            s_GameStats.KilledTNTGoblin();
        }
    }

    IEnumerator DelayedSetScore()
    {
        int score = 0;
        int numTorchGoblinsKilled = s_GameStats.NumTorchGoblinsKilled();
        for (int i = 0; i < numTorchGoblinsKilled; i++)
        {
            //score += (i * torchGoblinScore);
            //scoreTextComponent.text = score.ToString();

            IncreaseScore(ref score, i * torchGoblinScore);

            yield return new WaitForSecondsRealtime(waitForSecondsBetweenEachScoreUpdate);
        }

        int numBarrelGoblinsKilled = s_GameStats.NumBarrelGoblinsKilled();
        for (int i = 0; i < numBarrelGoblinsKilled; i++)
        {
            //score += (i * barrelGoblinScore);
            //scoreTextComponent.text = score.ToString();

            IncreaseScore(ref score, i * barrelGoblinScore);

            yield return new WaitForSecondsRealtime(waitForSecondsBetweenEachScoreUpdate);
        }

        int numTNTGoblinsKilled = s_GameStats.NumTNTGoblinsKilled();
        for (int i = 0; i < numTNTGoblinsKilled; i++)
        {
            //score += (i * TNTGoblinScore);
            //scoreTextComponent.text = score.ToString();

            IncreaseScore(ref score, i * TNTGoblinScore);

            yield return new WaitForSecondsRealtime(waitForSecondsBetweenEachScoreUpdate);
        }

        int numMinesDestroyed = s_GameStats.NumMinesDestroyed();
        for (int i = 0; i < numMinesDestroyed; i++)
        {
            //score += i * minesDestroyedScore;
            //scoreTextComponent.text = score.ToString();

            IncreaseScore(ref score, i * minesDestroyedScore);

            yield return new WaitForSecondsRealtime(waitForSecondsBetweenEachScoreUpdate);
        }

        if (GameStats.gameOverState == 1)
        {
            //score += gameWinScore;
            //scoreTextComponent.text = score.ToString();

            IncreaseScore(ref score, gameWinScore);

            yield return new WaitForSecondsRealtime(waitForSecondsBetweenEachScoreUpdate);
        }
    }

    private void IncreaseScore(ref int currentScore, int increaseScoreBy)
    {
        currentScore = currentScore + increaseScoreBy;
        scoreTextComponent.text = currentScore.ToString();

        //scoreAudioSource.pitch = Random.Range(0.75f, 0.85f);
        scoreAudioSource.PlayOneShot(scoreIncreaseAudioClips[Random.Range(0, scoreIncreaseAudioClips.Count)], Random.Range(0.25f, 0.35f));
    }

    private int GetRunScore()
    {
        int score = 0;

        score += s_GameStats.NumTorchGoblinsKilled() * torchGoblinScore;
        score += s_GameStats.NumBarrelGoblinsKilled() * barrelGoblinScore;
        score += s_GameStats.NumTNTGoblinsKilled() * TNTGoblinScore;

        return score;
    }
}
