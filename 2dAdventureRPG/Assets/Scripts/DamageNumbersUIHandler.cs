using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

public class DamageNumbersUIHandler : MonoBehaviour
{

    [SerializeField] private GameObject damageTextObject;

    private static Transform curGameCanvasTransform;

    private void Awake()
    {
        //curGameCanvasTransform = GameObject.FindGameObjectWithTag("PlayerCanvas").transform;
        curGameCanvasTransform = transform.GetChild(0);
    }

    public void ShowDamageText(Transform spawnFromTransform, int damageAmount, float duration)
    {
        GameObject damageText = Instantiate(damageTextObject, curGameCanvasTransform);
        damageText.transform.position = spawnFromTransform.position + (Vector3.up * 1.5f);

        TextMeshProUGUI textMeshProComponent = damageText.GetComponent<TextMeshProUGUI>();
        textMeshProComponent.color = Color.red;

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
