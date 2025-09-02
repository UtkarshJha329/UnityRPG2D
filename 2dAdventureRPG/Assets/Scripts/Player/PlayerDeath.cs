using UnityEditor.Tilemaps;
using UnityEngine;

[RequireComponent(typeof(CharacterStates))]
public class PlayerDeath : MonoBehaviour
{
    private CharacterStates characterStates;

    private void Awake()
    {
        characterStates = GetComponent<CharacterStates>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void LateUpdate()
    {
        if (characterStates.isDead)
        {
            GameStats.gameOverState = -1;
            gameObject.SetActive(false);
        }
    }
}
