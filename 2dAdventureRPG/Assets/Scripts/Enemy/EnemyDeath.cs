using UnityEngine;

[RequireComponent(typeof(CharacterStates))]
public class EnemyDeath : MonoBehaviour
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
            gameObject.SetActive(false);
        }
    }
}
