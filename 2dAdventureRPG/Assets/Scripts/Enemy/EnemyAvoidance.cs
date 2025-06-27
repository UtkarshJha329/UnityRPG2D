using System.Collections.Generic;
using UnityEngine;

public class EnemyAvoidance : MonoBehaviour
{
    public Rigidbody2D rb2d;
    public CircleCollider2D detectionCollider2D;

    public EnemyProperties s_EnemyProperties;
    public Enemy_Movement s_EnemyMovement;

    [Header("Enemy Detection Variables")]
    public float nearbyEnemyDetectionRange = 2.5f;
    public float avoidanceSpeed = 5.0f;
    
    //public List<GameObject> enemiesNearbyAvoidanceObject = new List<GameObject>();
    public List<GameObject> enemiesNearby = new List<GameObject>();
    public List<EnemyProperties> enemiesNearbyEnemyProperties = new List<EnemyProperties>();
    public List<Rigidbody2D> enemiesNearbyRigidbody2D = new List<Rigidbody2D>();

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
        if(s_EnemyProperties == null)
        {
            Debug.LogError("Reference to s_EnemyProperties component has not been assigned!");
        }
        if(s_EnemyMovement == null)
        {
            Debug.LogError("Reference to s_EnemyMovement component has not been assigned!");
        }
        detectionCollider2D.radius = nearbyEnemyDetectionRange;
    }

    // Update is called once per frame
    void Update()
    {
        detectionCollider2D.radius = nearbyEnemyDetectionRange;
        //AvoidDetectedNearByEnemies();
    }

    private void FixedUpdate()
    {
        AvoidDetectedNearByEnemies();
    }

    private void AvoidDetectedNearByEnemies()
    {
        Vector2 avoidanceOffsetThisFrame = Vector2.zero;
        for (int i = 0; i < enemiesNearby.Count; i++)
        {
            //float distanceToCurEnemy = Vector3.Distance(transform.position, enemiesNearby[i].transform.position);

            //float forceAmountToApply = distanceToCurEnemy / nearbyEnemyDetectionRange;
            //Vector3 forceDirection = Vector3.Reflect(enemiesNearby[i].transform.position - transform.position, transform.forward).normalized;

            //rb2d.AddForce(forceDirection * forceAmountToApply);

            //if (Vector3.Distance(transform.position, enemiesNearby[i].transform.position) < 1.5f)
            {

                Vector2 enemyVelocity = enemiesNearbyRigidbody2D[i].linearVelocity;
                Vector2 curVelocity = rb2d.linearVelocity;

                //if (Vector2.Dot(enemyVelocity, curVelocity) < 0.0f)
                {
                    Vector2 directionToCheck = enemiesNearby[i].transform.position - transform.position;
                    Vector2 directionToCheckForEnemy = directionToCheck * -1.0f;

                    float speedInDirectionToCheck = Vector2.Dot(directionToCheck, curVelocity) / directionToCheck.magnitude;
                    float speedInDirectionToCheckForEnemy = Vector2.Dot(directionToCheckForEnemy, enemyVelocity) / directionToCheckForEnemy.magnitude;

                    //speedInDirectionToCheck += Mathf.Sign(speedInDirectionToCheck) * 0.1f;
                    //speedInDirectionToCheckForEnemy += Mathf.Sign(speedInDirectionToCheckForEnemy) * 0.1f;

                    directionToCheck = directionToCheck.normalized * speedInDirectionToCheck;
                    directionToCheckForEnemy = directionToCheckForEnemy.normalized * speedInDirectionToCheckForEnemy;

                    //Vector2 directionToCheck = curVelocity;
                    //Vector2 directionToCheckForEnemy = enemyVelocity;
                    //Vector2 directionToCheck = curVelocity.magnitude == 0.0f ? s_EnemyProperties.facingDirection * 0.1f : curVelocity;
                    //Vector2 directionToCheckForEnemy = enemyVelocity.magnitude == 0.0f ? enemiesNearbyEnemyProperties[i].facingDirection * 0.1f : enemyVelocity;

                    Vector2 a = transform.position;
                    Vector2 b = new Vector2(transform.position.x, transform.position.y) + directionToCheck;
                    Vector2 c = enemiesNearby[i].transform.position;
                    Vector2 d = new Vector2(enemiesNearby[i].transform.position.x, enemiesNearby[i].transform.position.y) + directionToCheckForEnemy;

                    float bottom = ((d.y - c.y) * (b.x - a.x)) - ((d.x - c.x) * (b.y - a.y));

                    if (bottom != 0.0f)
                    {
                        float top = ((d.x - c.x) * (a.y - c.y)) - ((d.y - c.y) * (a.x - c.x));

                        float t = top / bottom;

                        if (t >= 0.0f && t <= 1.0f)
                        {
                            //Vector2 intersectionPoint = new Vector2(Mathf.Lerp(a.x, b.x, t), Mathf.Lerp(a.y, b.y, t));

                            Vector2 dirToOtherEnemy = (enemiesNearby[i].transform.position - transform.position).normalized;
                            float distToOtherEnemy = Vector3.Magnitude(dirToOtherEnemy);
                            dirToOtherEnemy = dirToOtherEnemy.normalized;
                            Vector2 right = Vector3.Cross(Vector3.forward, dirToOtherEnemy).normalized;

                            Debug.DrawLine(transform.position, enemiesNearby[i].transform.position, Color.red);
                            Debug.DrawLine(transform.position, transform.position + new Vector3(right.x, right.y, 0.0f), Color.green);

                            avoidanceOffsetThisFrame += (distToOtherEnemy / nearbyEnemyDetectionRange) * right;

                        }
                    }
                }
            }
        }

        //s_EnemyMovement.avoidanceOffset = avoidanceOffsetThisFrame * avoidanceSpeed;
        rb2d.linearVelocity += avoidanceOffsetThisFrame * avoidanceSpeed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("EnemyAvoidance"))
        {
            //enemiesNearbyAvoidanceObject.Add(collision.gameObject);
            if(collision.transform.parent.GetComponent<EnemyProperties>().sectionIndex == s_EnemyProperties.sectionIndex)
            {
                enemiesNearby.Add(collision.transform.parent.gameObject);
                enemiesNearbyEnemyProperties.Add(collision.transform.parent.GetComponent<EnemyProperties>());
                enemiesNearbyRigidbody2D.Add(collision.transform.parent.GetComponent<Rigidbody2D>());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("EnemyAvoidance"))
        {
            if (collision.transform.parent.GetComponent<EnemyProperties>().sectionIndex == s_EnemyProperties.sectionIndex)
            {
                enemiesNearby.Remove(collision.transform.parent.gameObject);
                enemiesNearbyEnemyProperties.Remove(collision.transform.parent.GetComponent<EnemyProperties>());
                enemiesNearbyRigidbody2D.Remove(collision.transform.parent.GetComponent<Rigidbody2D>());
            }
        }
    }
}
