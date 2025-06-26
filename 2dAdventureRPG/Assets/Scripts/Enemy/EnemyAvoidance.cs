using System.Collections.Generic;
using UnityEngine;

public class EnemyAvoidance : MonoBehaviour
{
    public Rigidbody2D rb2d;
    public CircleCollider2D detectionCollider2D;

    [Header("Enemy Detection Variables")]
    public float nearbyEnemyDetectionRange = 5.0f;
    public float forceAmountToPushWhenEnemyNearby = 10.0f;
    
    private List<GameObject> enemiesNearby = new List<GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(rb2d == null)
        {
            Debug.LogError("rb2d in Enemy Avoidance not attributed!");
        }
        if(detectionCollider2D == null)
        {
            Debug.LogError("detectionCollider2D in Enemy Avoidance not attributed!");
        }
        detectionCollider2D.radius = nearbyEnemyDetectionRange;
    }

    // Update is called once per frame
    void Update()
    {
        AvoidDetectedNearByEnemies();
    }

    private void AvoidDetectedNearByEnemies()
    {
        for (int i = 0; i < enemiesNearby.Count; i++)
        {
            float distanceToCurEnemy = Vector3.Distance(transform.position, enemiesNearby[i].transform.position);

            float forceAmountToApply = distanceToCurEnemy / nearbyEnemyDetectionRange;
            Vector3 forceDirection = Vector3.Reflect(enemiesNearby[i].transform.position - transform.position, transform.forward).normalized;

            rb2d.AddForce(forceDirection * forceAmountToApply);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && collision.transform != transform.parent)
        {
            if(Vector3.Distance(transform.position, collision.gameObject.transform.position) < nearbyEnemyDetectionRange){
                enemiesNearby.Add(collision.gameObject);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            if(Vector3.Distance(transform.position, collision.gameObject.transform.position) > nearbyEnemyDetectionRange)
            {
                enemiesNearby.Remove(collision.gameObject);
            }
        }
    }
}
