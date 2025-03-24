using Unity.Cinemachine;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cineCamera;
    [SerializeField] private GameObject currentRoomBoundaryObject;

    [SerializeField] private Vector2Int numRooms;
    [SerializeField] private Vector2Int roomDims;
    [SerializeField] private Vector2 roomOffsets;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int y = 0; y < numRooms.y; y++)
        {
            for (int x = 0; x < numRooms.x; x++)
            {
                GameObject curRoom = Instantiate(currentRoomBoundaryObject, transform);
                curRoom.transform.position = new Vector3((x * roomDims.x) + roomOffsets.x, (y * roomDims.y) + roomOffsets.y, 0.0f);

                curRoom.GetComponent<CameraTargetManager>().cineCamera = cineCamera;
            }
        }
    }
}
