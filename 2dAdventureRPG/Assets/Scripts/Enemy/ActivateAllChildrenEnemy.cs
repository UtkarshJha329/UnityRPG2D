using UnityEngine;

public class ActivateAllChildrenEnemy : MonoBehaviour
{
    public void SetAllChildrenEnemies(bool activeState)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(activeState);
        }
    }
}
