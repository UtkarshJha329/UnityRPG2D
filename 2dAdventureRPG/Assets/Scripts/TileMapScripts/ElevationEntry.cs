using UnityEngine;

public class ElevationEntry : MonoBehaviour
{
    [SerializeField] private Collider2D[] mountainColliders;
    [SerializeField] private Collider2D[] boundaryColliders;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            foreach (Collider2D mountainCollider in mountainColliders) { mountainCollider.enabled = false; }
            foreach (Collider2D boundaryCollider in boundaryColliders) { boundaryCollider.enabled = true; }

            collision.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 15;
        }
    }
}
