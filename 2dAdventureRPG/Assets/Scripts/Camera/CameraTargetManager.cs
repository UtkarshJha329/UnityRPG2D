using Unity.Cinemachine;
using UnityEngine;

public class CameraTargetManager : MonoBehaviour
{
    public CinemachineCamera cineCamera;
    public Transform enemiesParent;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            cineCamera.Follow = transform;

            enemiesParent.gameObject.SetActive(true);
            enemiesParent.GetComponent<ActivateAllChildrenEnemy>().SetAllChildrenEnemies(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            enemiesParent.gameObject.SetActive(false);
            enemiesParent.GetComponent<ActivateAllChildrenEnemy>().SetAllChildrenEnemies(false);
        }
    }
}
