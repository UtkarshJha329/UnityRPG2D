using UnityEditor.Rendering;
using UnityEngine;

public class DynamiteHandling : MonoBehaviour
{

    public Vector3 moveDirection = Vector3.zero;
    public float moveSpeed = 13.0f;

    public int damage = 10;
    public float knockbackForce = 10.0f;

    public PlayerHealth playerHealthManager;
    public PlayerMovement s_PlayerMovement;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (playerHealthManager.ChangeHealth(damage))
            {
                Vector3 knockbackDirection = collision.transform.position - transform.position;
                s_PlayerMovement.KnockbackPlayer(knockbackForce, knockbackDirection);
            }

            // SPAWN AN EXPLOSION BEFORE DESTROYING DYNAMITE!

            Destroy(gameObject);
        }
    }
}
