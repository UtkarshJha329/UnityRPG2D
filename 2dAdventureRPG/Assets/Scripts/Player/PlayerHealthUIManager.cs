using DG.Tweening;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerHealth))]
public class PlayerHealthUIManager : MonoBehaviour
{
    [SerializeField] private GameObject playerCanvas;
    [SerializeField] private GameObject playerHealthUIContainerPrefab;
    [SerializeField] private GameObject playerHealthUIPrefab;
    [SerializeField] private Sprite[] playerHealthUISprites;

    private HorizontalLayoutGroup c_playerHealthUIHorizontalLayoutGroup;
    private PlayerHealth s_playerHealth;

    private int totalNumberOfHeartContainers = 0;
    private int numberOfPartsPerHeart = 4;

    public bool updatedHealth = true;

    private void Awake()
    {
        s_playerHealth = GetComponent<PlayerHealth>();

        playerCanvas = GameObject.FindGameObjectWithTag("PlayerCanvas");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject playerHealthUIContainerGameObject = Instantiate(playerHealthUIContainerPrefab, playerCanvas.transform);
        c_playerHealthUIHorizontalLayoutGroup = playerHealthUIContainerGameObject.GetComponent<HorizontalLayoutGroup>();

        UpdatePlayerHealthUIHearts();
    }

    // Update is called once per frame
    void Update()
    {
        if (updatedHealth)
        {
            UpdatePlayerHealthUIHearts();
            updatedHealth = false;
        }
    }

    private void UpdatePlayerHealthUIHearts()
    {
        int totalPlayerHealth = (int)s_playerHealth.GetMaxPlayerHealth();
        totalNumberOfHeartContainers = totalPlayerHealth / numberOfPartsPerHeart;

        int currentPlayerHealth = (int)s_playerHealth.GetCurrentPlayerHealth();
        int fullyFilledNumberOfHearthContainers = currentPlayerHealth / numberOfPartsPerHeart;

        int partiallyFilledContainerLevel = numberOfPartsPerHeart - (int)(((s_playerHealth.GetCurrentPlayerHealth() / (float)numberOfPartsPerHeart) - (currentPlayerHealth / numberOfPartsPerHeart)) * numberOfPartsPerHeart);

        c_playerHealthUIHorizontalLayoutGroup.spacing = totalNumberOfHeartContainers;

        if(totalNumberOfHeartContainers != c_playerHealthUIHorizontalLayoutGroup.gameObject.transform.childCount)
        {
            for (int i = 0; i < c_playerHealthUIHorizontalLayoutGroup.gameObject.transform.childCount; i++)
            {
                Destroy(c_playerHealthUIHorizontalLayoutGroup.gameObject.transform.transform.GetChild(i));
            }

            for (int i = 0; i < totalNumberOfHeartContainers; i++)
            {
                //Debug.Log("Player Heart " + i + " spawned.");
                GameObject heart = Instantiate(playerHealthUIPrefab, c_playerHealthUIHorizontalLayoutGroup.gameObject.transform);
                heart.name = "Player Heart (" + i + ")";
                //heart.transform.DOLocalJump(heart.transform.position, 10.0f, 1, 0.75f);
                //heart.transform.DOShakeScale(0.5f);
            }
        }

        for (int i = 0; i < totalNumberOfHeartContainers; i++)
        {

            GameObject heart = c_playerHealthUIHorizontalLayoutGroup.gameObject.transform.GetChild(i).gameObject;

            if(i < fullyFilledNumberOfHearthContainers)
            {
                heart.GetComponent<Image>().sprite = playerHealthUISprites[0];
            }
            else if(i == fullyFilledNumberOfHearthContainers)
            {
                heart.GetComponent<Image>().sprite = playerHealthUISprites[partiallyFilledContainerLevel];
                //heart.transform.DOLocalJump(heart.transform.position, 10.0f, 1, 0.75f);
                heart.transform.DOComplete();
                heart.transform.DOShakeScale(0.5f);
                //heart.transform.DOPunchScale(heart.transform.localScale * 1.05f, 0.25f);
                //heart.transform.DOPunchPosition(heart.transform.position + new Vector3(0.0f, 1.0f, 0.0f), 0.5f);
                //Debug.Log("Partial heart.");
            }
            else if(i > fullyFilledNumberOfHearthContainers)
            {
                heart.GetComponent<Image>().sprite = playerHealthUISprites[4];

                heart.transform.DOComplete();
                heart.transform.DOPunchScale(heart.transform.localScale * 0.5f, 0.25f);
            }
        }

    }
}
