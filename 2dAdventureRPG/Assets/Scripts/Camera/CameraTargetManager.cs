using Unity.Cinemachine;
using UnityEngine;

public class CameraTargetManager : MonoBehaviour
{
    public CinemachineCamera cineCamera;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            cineCamera.Follow = transform;
        }
    }
}
