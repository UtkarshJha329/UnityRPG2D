using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageNumbersUIHandler : MonoBehaviour
{
    [SerializeField] private GameObject damageTextObject;

    private static Transform curGameCanvasTransform;

    private Dictionary<Transform, GameObject> damagedEnemyHealthAndEnemyTransform = new Dictionary<Transform, GameObject>();

    private void Awake()
    {
        //curGameCanvasTransform = GameObject.FindGameObjectWithTag("PlayerCanvas").transform;
        curGameCanvasTransform = transform.GetChild(0);
    }

    private void Update()
    {
        foreach (KeyValuePair<Transform, GameObject> keyValuePair in damagedEnemyHealthAndEnemyTransform)
        {
            if (keyValuePair.Key.gameObject.activeInHierarchy)
            {
                keyValuePair.Value.SetActive(true);
                Vector3 enemyHealthTextOffset = ((Vector3.up + Vector3.left * keyValuePair.Key.localScale.x) * 0.75f);
                keyValuePair.Value.transform.position = keyValuePair.Key.position + enemyHealthTextOffset;
            }
            else
            {
                keyValuePair.Value.SetActive(false);
            }
        }
    }

    public void RemoveEnemyFromUIHealthList(Transform enemyTransform)
    {
        //Debug.Log("Deleted enemy health text object!");
        Destroy(damagedEnemyHealthAndEnemyTransform[enemyTransform]);
        damagedEnemyHealthAndEnemyTransform.Remove(enemyTransform);
    }

    public TextMeshProUGUI ShowEnemyHealth(Transform followTransform, int health)
    {
        Vector3 enemyHealthTextOffset = ((Vector3.up + Vector3.left * followTransform.localScale.x) * 0.75f);

        GameObject healthText = Instantiate(damageTextObject, curGameCanvasTransform);
        healthText.transform.position = followTransform.position + enemyHealthTextOffset;

        healthText.GetComponent<RectTransform>().sizeDelta = new Vector2(1.0f, healthText.GetComponent<RectTransform>().sizeDelta.y);

        TextMeshProUGUI textMeshProComponent = healthText.GetComponent<TextMeshProUGUI>();
        //textMeshProComponent.color = Color.red;
        textMeshProComponent.color = Color.black;
        textMeshProComponent.fontSize = 0.55f;

        textMeshProComponent.text = health.ToString();

        damagedEnemyHealthAndEnemyTransform.Add(followTransform, healthText);

        return textMeshProComponent;
    }

    public void ShowDamageText(Transform spawnFromTransform, int damageAmount, float duration)
    {
        GameObject damageText = Instantiate(damageTextObject, curGameCanvasTransform);
        damageText.transform.position = spawnFromTransform.position + (Vector3.up * 1.5f);

        TextMeshProUGUI textMeshProComponent = damageText.GetComponent<TextMeshProUGUI>();
        textMeshProComponent.color = Color.red;
        textMeshProComponent.fontSize = 0.5f;

        textMeshProComponent.text = damageAmount.ToString();

        damageText.transform.DOLocalMove(damageText.transform.position + (Vector3.up * 2.5f), duration);
        textMeshProComponent.DOFade(0.0f, duration);

        StartCoroutine(DestroyDamageTextAfterTweeningIsDone(damageText, duration));
    }

    IEnumerator DestroyDamageTextAfterTweeningIsDone(GameObject damageTextObject, float duration)
    {
        yield return new WaitForSeconds(duration + 0.1f);

        Destroy(damageTextObject);
    }
}
