using UnityEngine;

public class PlayerHaloManager : MonoBehaviour
{
    public GameObject knightHalo;

    public bool haloIsVisible = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        knightHalo.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameStats.playerReachedCutSceneTile)
        {
            knightHalo.SetActive(haloIsVisible);
        }
    }

}
