using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MinesHealth : MonoBehaviour
{
    public int minesHealth = 40;
    public Sprite closedMineSprite;

    private int currentHealth = 40;
    private SpriteRenderer mineSpriteRenderer;



    private void Awake()
    {
        if(closedMineSprite == null)
        {
            Debug.LogError("closedMineSprite variable has not been assigned in the inspector on the object, " + gameObject.name);
        }

        mineSpriteRenderer = GetComponent<SpriteRenderer>();
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = minesHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if(currentHealth <= 0)
        {
            mineSpriteRenderer.sprite = closedMineSprite;
            // Add smoke and sound effects during the change.
        }
    }
}
