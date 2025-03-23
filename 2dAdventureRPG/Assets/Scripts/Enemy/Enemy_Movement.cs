using Unity.VisualScripting;
using UnityEngine;

[RequireComponent (typeof(Rigidbody2D))]
[RequireComponent (typeof(CircleCollider2D))]
[RequireComponent (typeof(CharacterStates))]
[RequireComponent (typeof(EnemyProperties))]
public class Enemy_Movement : MonoBehaviour
{

    private Rigidbody2D rb2d;
    private CircleCollider2D detectorCircle;
    private CharacterStates characterStates;
    private EnemyProperties s_EnemyProperties;

    private Transform playerTransform;

    private bool playerWasDetected = false;


    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        detectorCircle = GetComponent<CircleCollider2D>();
        characterStates = GetComponent<CharacterStates>();
        s_EnemyProperties = GetComponent<EnemyProperties>();

        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        detectorCircle.offset = new Vector2(s_EnemyProperties.detectorCircleOffset, detectorCircle.offset.y);
        detectorCircle.radius = s_EnemyProperties.detectorCircleRadius;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        s_EnemyProperties.facingDirection = new Vector3(rb2d.linearVelocity.x != 0.0f ? Mathf.Sign(rb2d.linearVelocity.x) : s_EnemyProperties.facingDirection.x, 1.0f, 0.0f);
        transform.localScale = new Vector3(s_EnemyProperties.facingDirection.x, s_EnemyProperties.facingDirection.y, 0.0f);
    }

    private void FixedUpdate()
    {
        if (!characterStates.isKnockbacked && !characterStates.isDead)
        {
            if (detectorCircle.OverlapPoint(playerTransform.position))
            {
                playerWasDetected = true;

                if (!characterStates.IsAttacking())
                {
                    Vector2 directionToPlayer = playerTransform.position - transform.position;
                    float distanceToPlayer = directionToPlayer.magnitude;

                    if (distanceToPlayer > s_EnemyProperties.attackRange)
                    {
                        directionToPlayer = directionToPlayer.normalized;
                        rb2d.linearVelocity = directionToPlayer * s_EnemyProperties.speed;

                        characterStates.isMoving = true;
                        characterStates.isIdling = false;
                    }
                    else
                    {
                        rb2d.linearVelocity = Vector2.zero;

                        characterStates.isMoving = false;
                        characterStates.isIdling = true;
                    }
                }
                else
                {
                    rb2d.linearVelocity = Vector2.zero;

                    characterStates.isMoving = false;
                    characterStates.isIdling = false;
                }
            }
            else
            {
                playerWasDetected = false;
                rb2d.linearVelocity = Vector2.zero;

                characterStates.isMoving = false;
                characterStates.isIdling = true;
            }
        }
    }

    public void KnockbackEnemy(float knockbackForceValue, Vector3 knockbackDirection)
    {
        characterStates.isKnockbacked = true;

        Vector3 knockbackForce = knockbackDirection * knockbackForceValue;
        rb2d.AddForce(knockbackForce, ForceMode2D.Impulse);
    }

}
