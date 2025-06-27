using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent (typeof(Rigidbody2D))]
[RequireComponent (typeof(CircleCollider2D))]
[RequireComponent (typeof(CharacterStates))]
[RequireComponent (typeof(EnemyProperties))]
public class Enemy_Movement : MonoBehaviour
{
    public int currentWayPointIndex = -1;

    private Rigidbody2D rb2d;
    private CircleCollider2D detectorCircle;
    private CharacterStates characterStates;
    private EnemyProperties s_EnemyProperties;

    private Transform playerTransform;

    private bool playerWasDetected = false;

    private bool switchingToNextWayPoint = false;

    private MapGenerator mapGenerator;

    private Vector2 currentPatrolWayPointPosition = Vector2.zero;
    private Vector2 pointCloseToCurrentPatrolWayPointPosition = Vector2.zero;

    private float wanderRadius = 0.75f;
    private bool wander = true;

    private bool alreadySeenPlayer = false;

    private bool useCornersForMovingAround = true;
    private bool useAllPatrolPoints = true;

    private float emptyApproachingWayPointDistance = 1.5f;

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        detectorCircle = GetComponent<CircleCollider2D>();
        characterStates = GetComponent<CharacterStates>();
        s_EnemyProperties = GetComponent<EnemyProperties>();

        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        mapGenerator = GameObject.FindGameObjectWithTag("MapTileGrid").GetComponent<MapGenerator>();

        detectorCircle.offset = new Vector2(s_EnemyProperties.detectorCircleOffset, detectorCircle.offset.y);
        detectorCircle.radius = s_EnemyProperties.detectorCircleRadius;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (mapGenerator.roomIndexAndRoom[s_EnemyProperties.roomIndex].sections[s_EnemyProperties.sectionIndex].size.x <= 6.0f
                        || mapGenerator.roomIndexAndRoom[s_EnemyProperties.roomIndex].sections[s_EnemyProperties.sectionIndex].size.y <= 6.0f)
        {
            //wanderRadius = 0.5f;
            wander = false;
            useAllPatrolPoints = false;
        }

        currentPatrolWayPointPosition = mapGenerator.GetNextPatrolWayPointPosition(s_EnemyProperties.roomIndex, s_EnemyProperties.sectionIndex, ref currentWayPointIndex, useCornersForMovingAround, useAllPatrolPoints);
        pointCloseToCurrentPatrolWayPointPosition = PointCloseToCurrentWayPointPosition(wanderRadius);
    }

    private void OnEnable()
    {
        alreadySeenPlayer = false;
        switchingToNextWayPoint = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!characterStates.isKnockbacked)
        {
            s_EnemyProperties.facingDirection = new Vector3(rb2d.linearVelocity.x != 0.0f ? Mathf.Sign(rb2d.linearVelocity.x) : s_EnemyProperties.facingDirection.x, 1.0f, 0.0f);
            transform.localScale = new Vector3(s_EnemyProperties.facingDirection.x, s_EnemyProperties.facingDirection.y, 0.0f);
        }
    }

    private void FixedUpdate()
    {
        if (!characterStates.isKnockbacked && !characterStates.isDead && currentWayPointIndex >= 0)
        {
            if (!alreadySeenPlayer)
            {
                alreadySeenPlayer = detectorCircle.OverlapPoint(playerTransform.position);
            }

            if (alreadySeenPlayer && PlayerIsWithinTheSameSection(playerTransform.position))
            {
                //Debug.Log("Detected player.");
                playerWasDetected = true;

                if (!characterStates.IsAttacking())
                {
                    Vector2 directionToPlayer = playerTransform.position - transform.position;
                    float distanceToPlayer = directionToPlayer.magnitude;

                    if (distanceToPlayer > s_EnemyProperties.attackRange)
                    {
                        directionToPlayer = directionToPlayer.normalized;
                        rb2d.linearVelocity = directionToPlayer * s_EnemyProperties.speed;

                        //Debug.Log("Moving to attack player!");

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

                if(s_EnemyProperties.mineGuard || s_EnemyProperties.connectedToMineGuardSection || !wander)
                {
                    Vector2 displacementBetweenCurWayPointAndEnemy = (currentPatrolWayPointPosition) - new Vector2(transform.position.x, transform.position.y);
                    float distanceToWaypoint = displacementBetweenCurWayPointAndEnemy.magnitude;
                    if (distanceToWaypoint > s_EnemyProperties.patrolingWayPointStoppingDistance)
                    {
                        rb2d.linearVelocity = displacementBetweenCurWayPointAndEnemy.normalized * s_EnemyProperties.speed;

                        characterStates.isMoving = true;
                        characterStates.isIdling = false;

                        //if (distanceToWaypoint < emptyApproachingWayPointDistance && !mapGenerator.IsCurrentWayPointEmpty(s_EnemyProperties.roomIndex, s_EnemyProperties.sectionIndex, currentWayPointIndex))
                        //{
                        //    mapGenerator.EmptyApproachingWayPoint(s_EnemyProperties.roomIndex, s_EnemyProperties.sectionIndex, currentWayPointIndex, useAllPatrolPoints, useCornersForMovingAround);
                        //}
                    }
                    else
                    {
                        rb2d.linearVelocity = Vector2.zero;

                        characterStates.isMoving = false;
                        characterStates.isIdling = true;

                        if (!switchingToNextWayPoint)
                        {
                            switchingToNextWayPoint = true;
                            mapGenerator.SetPatrolPointOccupancyInSection(s_EnemyProperties.roomIndex, s_EnemyProperties.sectionIndex, true, currentWayPointIndex);
                            StartCoroutine(SwitchToNextWayPointAfterWaiting());
                        }
                    }
                }
                else
                {
                    //Debug.Log("Random wandering.");

                    Vector2 displaceBetweenCurRandomPointAndEnemy = (pointCloseToCurrentPatrolWayPointPosition) - new Vector2(transform.position.x, transform.position.y);

                    float distanceToWaypoint = displaceBetweenCurRandomPointAndEnemy.magnitude;
                    if (distanceToWaypoint > s_EnemyProperties.wanderWayPointStoppingDistance)
                    {
                        //Debug.Log("Am trying to move as a wanderlin.");
                        rb2d.linearVelocity = displaceBetweenCurRandomPointAndEnemy.normalized * s_EnemyProperties.speed;

                        characterStates.isMoving = true;
                        characterStates.isIdling = false;

                        //if (distanceToWaypoint < emptyApproachingWayPointDistance && !mapGenerator.IsCurrentWayPointEmpty(s_EnemyProperties.roomIndex, s_EnemyProperties.sectionIndex, currentWayPointIndex))
                        //{
                        //    mapGenerator.EmptyApproachingWayPoint(s_EnemyProperties.roomIndex, s_EnemyProperties.sectionIndex, currentWayPointIndex, useAllPatrolPoints, useCornersForMovingAround);
                        //}
                    }
                    else
                    {
                        //Debug.Log("THE WANDERLIN HAS REACHED IT'S DESTINATION!!!");

                        rb2d.linearVelocity = Vector2.zero;

                        characterStates.isMoving = false;
                        characterStates.isIdling = true;

                        if (!switchingToNextWayPoint)
                        {
                            //Debug.Log("Wanderling has ordered a new destination.");
                            switchingToNextWayPoint = true;
                            mapGenerator.SetPatrolPointOccupancyInSection(s_EnemyProperties.roomIndex, s_EnemyProperties.sectionIndex, true, currentWayPointIndex);
                            StartCoroutine(SwitchToNextWayPointAfterWaiting());
                        }
                        else
                        {
                            //Debug.Log("Wanderling has already ordered a new destination.");
                        }
                    }
                }
            }
        }
    }

    private Vector2 PointCloseToCurrentWayPointPosition(float radiusAroundPoint)
    {
        Vector2 pointCloseToCurrentWayPointPosition = currentPatrolWayPointPosition + Random.insideUnitCircle * radiusAroundPoint;
        while (!mapGenerator.SectionContainsPointPadded(s_EnemyProperties.roomIndex, s_EnemyProperties.sectionIndex, pointCloseToCurrentWayPointPosition))
        {
            pointCloseToCurrentWayPointPosition = currentPatrolWayPointPosition + Random.insideUnitCircle * radiusAroundPoint;
        }
        return pointCloseToCurrentWayPointPosition;
    }

    public bool PlayerIsWithinTheSameSection(Vector2 playerPosition)
    {
        //Debug.Log("Player is within the same section!");
        //Debug.Log("Enemy in room index := " + s_EnemyProperties.roomIndex.ToString() + " and section : " + s_EnemyProperties.sectionIndex + " checking if player at := " + playerPosition.ToString() + " is in the same section.");
        return mapGenerator.SectionContainsPointPadded(s_EnemyProperties.roomIndex, s_EnemyProperties.sectionIndex, playerPosition);
    }

    public void KnockbackEnemy(float knockbackForceValue, Vector3 knockbackDirection)
    {
        characterStates.isKnockbacked = true;

        Vector3 knockbackForce = knockbackDirection * knockbackForceValue;
        rb2d.AddForce(knockbackForce, ForceMode2D.Impulse);
    }

    IEnumerator SwitchToNextWayPointAfterWaiting()
    {
        yield return new WaitForSeconds(5.0f);

        SwitchToNextWayPointImmediately();
    }

    public void SwitchToNextWayPointImmediately()
    {
        mapGenerator.SetPatrolPointOccupancyInSection(s_EnemyProperties.roomIndex, s_EnemyProperties.sectionIndex, false, currentWayPointIndex);

        currentPatrolWayPointPosition = mapGenerator.GetNextPatrolWayPointPosition(s_EnemyProperties.roomIndex, s_EnemyProperties.sectionIndex, ref currentWayPointIndex, useCornersForMovingAround, useAllPatrolPoints);

        pointCloseToCurrentPatrolWayPointPosition = PointCloseToCurrentWayPointPosition(wanderRadius);

        switchingToNextWayPoint = false;

        StopAllCoroutines();
    }

}
