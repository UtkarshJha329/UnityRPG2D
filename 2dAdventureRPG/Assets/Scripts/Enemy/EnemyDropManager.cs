using UnityEngine;
using UnityEngine.UIElements;

public class EnemyDropManager : MonoBehaviour
{
    public GameObject dropPrefab;

    public void Awake()
    {
        if(dropPrefab == null)
        {
            Debug.LogError("dropPrefab not assigned in EnemyDropManager script attached to " + gameObject.name);
        }
    }

    public void DropAppropriateItemAtLocation(Vector3 location, Vector3 directionToOffsetDrop, DropType dropType, Transform dropParent)
    {
        DropItem dropItem = Instantiate(dropPrefab, location + directionToOffsetDrop * 0.75f, Quaternion.identity, dropParent).GetComponent<DropItem>();
        dropItem.SetDropTypeAndInitDrop(dropType);
    }
}
