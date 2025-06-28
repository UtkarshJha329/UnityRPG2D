using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent (typeof(EnemyProperties))]
[RequireComponent (typeof(CharacterStates))]
[RequireComponent (typeof(Animator))]
public class Enemy_Combat : MonoBehaviour
{
    public Collider2D[] attackPoints; // 0 => right side attack, 1 => left side attack, 2 => down attack, 3 => up attack

    private float nextTimeToAttack = 0.0f;

    private CharacterStates characterStates;
    private EnemyProperties s_EnemyProperties;

    private Animator animator;

    private Transform playerTransform;
    private PlayerHealth playerHealthManager;
    private PlayerMovement s_PlayerMovement;

    private int[][] directionalAttackCollisionPointPairs = new int[4][] { new int[2] {0, 3 }, new int[2] {1, 3 }, new int[2] {2, 1 }, new int[] {3, 0, 1 } };
    private List<List<Collider2D>> colliderPairs = new List<List<Collider2D>>();

    private MapGenerator mapGenerator;

    private bool alreadyExploded = false;

    private void Awake()
    {
        characterStates = GetComponent<CharacterStates>();
        s_EnemyProperties = GetComponent<EnemyProperties>();

        animator = gameObject.GetComponent<Animator>();

        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        playerHealthManager = playerTransform.GetComponent<PlayerHealth>();
        s_PlayerMovement = playerTransform.GetComponent<PlayerMovement>();

        if(s_EnemyProperties.dynamiteSpawnObject == null)
        {
            Debug.LogError("Dynamite spawn object is missing or not assigned!!!");
        }

        mapGenerator = GameObject.FindGameObjectWithTag("MapTileGrid").GetComponent<MapGenerator>();
    }

    private void Start()
    {
        for (int i = 0; i < directionalAttackCollisionPointPairs.Length; i++)
        {
            List<Collider2D> colliders2D = new List<Collider2D>();
            for (int j = 0; j < directionalAttackCollisionPointPairs[i].Length; j++)
            {
                colliders2D.Add(attackPoints[directionalAttackCollisionPointPairs[i][j]]);
            }
            colliderPairs.Add(colliders2D);
        }

    }

    private void Update()
    {
        if (s_EnemyProperties.enemyType == EnemyType.TorchGoblin)
        {
            if ((playerTransform.position - transform.position).magnitude <= s_EnemyProperties.attackRange)
            {
                if (nextTimeToAttack <= Time.time)
                {
                    //Debug.Log("Attacking Player.");
                    float horizontalDistanceToPlayer = playerTransform.position.x - transform.position.x;
                    float verticalDistanceToPlayer = (playerTransform.position.y - transform.position.y);
                    if (Mathf.Abs(horizontalDistanceToPlayer) < s_EnemyProperties.attakcingDownOffset.x && verticalDistanceToPlayer < s_EnemyProperties.attakcingDownOffset.y)
                    {
                        characterStates.isAttackingDown = true;
                    }
                    else if (Mathf.Abs(horizontalDistanceToPlayer) < s_EnemyProperties.attakcingUpOffset.x && verticalDistanceToPlayer > s_EnemyProperties.attakcingUpOffset.y)
                    {
                        characterStates.isAttackingUp = true;
                    }
                    else
                    {
                        characterStates.isAttackingSide = true;
                    }

                    characterStates.isMoving = false;
                    characterStates.isIdling = false;
                    nextTimeToAttack = Time.time + s_EnemyProperties.delayTimeToAttackAfterPreviousAttack;
                }
            }
            else
            {
                nextTimeToAttack = Time.time + s_EnemyProperties.delayTimeToAttackAfterPlayerDetection;
            }
        }
        else if(s_EnemyProperties.enemyType == EnemyType.BombGoblin)
        {
            if (PlayerIsWithinTheSameSection(playerTransform.position) && Vector3.Distance(playerTransform.position, transform.position) <= s_EnemyProperties.attackRange)
            {
                //Debug.Log("Player is within attacking range!!!!");
                if (nextTimeToAttack <= Time.time)
                {
                    //Debug.Log("Attacking side!");

                    //characterStates.isAttackingDown = true;
                    //characterStates.isAttackingUp = true;
                    characterStates.isAttackingSide = true;

                    characterStates.isMoving = false;
                    characterStates.isIdling = false;
                    nextTimeToAttack = Time.time + s_EnemyProperties.delayTimeToAttackAfterPreviousAttack;
                }
            }
        }
        else if (s_EnemyProperties.enemyType == EnemyType.TNTBarrelGoblin)
        {
            if ((playerTransform.position - transform.position).magnitude <= s_EnemyProperties.attackRange)
            {
                if (nextTimeToAttack <= Time.time)
                {
                    characterStates.isAttackingSide = true;

                    characterStates.isMoving = false;
                    characterStates.isIdling = false;

                    nextTimeToAttack = Time.time + s_EnemyProperties.delayTimeToAttackAfterPreviousAttack;
                }
            }
            else
            {
                nextTimeToAttack = Time.time + s_EnemyProperties.delayTimeToAttackAfterPlayerDetection;
            }
        }

        if (characterStates.attackEvent)
        {
            if(s_EnemyProperties.enemyType != EnemyType.TNTBarrelGoblin)
            {
                AttackPlayer();
                characterStates.attackEvent = false;
            }
            else
            {
                if (!alreadyExploded)
                {
                    AttackPlayer();
                    characterStates.attackEvent = false;
                    alreadyExploded = true;
                }
            }
        }
    }

    public bool PlayerIsWithinTheSameSection(Vector2 playerPosition)
    {
        //Debug.Log("Player is within the same section!");
        //Debug.Log("Enemy in room index := " + s_EnemyProperties.roomIndex.ToString() + " and section : " + s_EnemyProperties.sectionIndex + " checking if player at := " + playerPosition.ToString() + " is in the same section.");
        return mapGenerator.SectionContainsPointPadded(s_EnemyProperties.roomIndex, s_EnemyProperties.sectionIndex, playerPosition);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            //collision.gameObject.GetComponent<PlayerHealth>().ChangeHealth(s_EnemyProperties.damage);
            playerHealthManager.ChangeHealth(s_EnemyProperties.damage);
        }
    }

    private void AttackPlayer()
    {
        if(s_EnemyProperties.enemyType == EnemyType.TorchGoblin)
        {
            AttackMelee();
        }
        else if(s_EnemyProperties.enemyType == EnemyType.BombGoblin)
        {
            AttackByThrowingBomb();
        }
        else if(s_EnemyProperties.enemyType == EnemyType.TNTBarrelGoblin)
        {
            AttackPlayerByExploding();
        }
    }

    private void AttackPlayerByExploding()
    {
        GameObject dynamiteGameObject = Instantiate(s_EnemyProperties.dynamiteSpawnObject, transform.position, Quaternion.identity);

        DynamiteHandling dynamiteHandling = dynamiteGameObject.GetComponent<DynamiteHandling>();

        dynamiteHandling.tntGoblinEnemyParent = transform;
        dynamiteHandling.showDynamiteSprite = false;

        dynamiteHandling.timeInWhichDynamiteDetonates = 0.75f;
        dynamiteHandling.explosionRangeCollider.radius = 5.0f;
        dynamiteHandling.explosionAtTime = dynamiteHandling.timeInWhichDynamiteDetonates; // buffer time to explosion after bomb lands.
        dynamiteHandling.explosionAtTime += Time.time;

        dynamiteHandling.dynamiteShadowsParentTransform = s_EnemyProperties.dynamiteShadowsParentTransform;
        dynamiteHandling.s_PlayerMovement = s_PlayerMovement;
        dynamiteHandling.playerHealthManager = playerHealthManager;

        //Debug.Log(gameObject.name + " ;=; Exploding and destroying myself in front of player.");

        Destroy(gameObject, 0.8f);
    }

    private void AttackByThrowingBomb()
    {
        if (characterStates.isAttackingSide || characterStates.isAttackingUp || characterStates.isAttackingDown)
        {
            AttackPlayerByThrowingBomb();
        }
    }

    private void AttackMelee()
    {
        if (characterStates.isAttackingSide)
        {
            if (s_EnemyProperties.facingDirection.x > 0.0f)
            {
                AttackPlayerUsingColliders(colliderPairs[0]);
            }
            else if (s_EnemyProperties.facingDirection.x < 0.0f)
            {
                AttackPlayerUsingColliders(colliderPairs[0]);
            }
        }
        else if (characterStates.isAttackingDown)
        {
            AttackPlayerUsingColliders(colliderPairs[2]);
        }
        else if (characterStates.isAttackingUp)
        {
            AttackPlayerUsingColliders(colliderPairs[3]);
        }
    }

    private void AttackPlayerByThrowingBomb()
    {
        Vector3 directionTowardsPlayer = playerTransform.position - transform.position;

        GameObject dynamiteGameObject = Instantiate(s_EnemyProperties.dynamiteSpawnObject, transform.position + s_EnemyProperties.dynamiteSpawnPosition, Quaternion.identity);

        DynamiteHandling dynamiteHandling = dynamiteGameObject.GetComponent<DynamiteHandling>();
        dynamiteHandling.moveDirection = directionTowardsPlayer.normalized;

        dynamiteHandling.timeToReachTarget = Vector3.Distance(playerTransform.position, transform.position) / dynamiteHandling.moveSpeedDirect;
        dynamiteHandling.explosionAtTime = dynamiteHandling.timeInWhichDynamiteDetonates; // buffer time to explosion after bomb lands.
        dynamiteHandling.explosionAtTime += (dynamiteHandling.timeToReachTarget + Time.time);

        dynamiteHandling.verticalSpeed = (2.0f * dynamiteHandling.fakeHeightToReach) / dynamiteHandling.timeToReachTarget;


        dynamiteHandling.dynamiteShadowsParentTransform = s_EnemyProperties.dynamiteShadowsParentTransform;
        dynamiteHandling.s_PlayerMovement = s_PlayerMovement;
        dynamiteHandling.playerHealthManager = playerHealthManager;
    }

    private void AttackPlayerUsingColliders(List<Collider2D> colliderPairsToCheck)
    {

        foreach (Collider2D collider2D in colliderPairsToCheck)
        {
            List<Collider2D> overlapResults = new List<Collider2D>();
            collider2D.Overlap(overlapResults);

            bool damagedPlayerSoBreak = false;
            foreach (Collider2D collidedColliders in overlapResults)
            {
                if (collidedColliders.CompareTag("Player"))
                {
                    if (playerHealthManager.ChangeHealth(s_EnemyProperties.damage))
                    {
                        Vector3 knockbackDirection = playerTransform.position - transform.position;
                        s_PlayerMovement.KnockbackPlayer(s_EnemyProperties.knockbackForce, knockbackDirection);
                    }
                    damagedPlayerSoBreak = true;
                    break;
                }
            }
            if(damagedPlayerSoBreak) { break; }
        }
    }

    //Set attack event to true from animation events so that enemy can check for player and deal damage to him.
    public void AttackEvent()
    {
        //Debug.Log("Attacking the player from animation event.");
        characterStates.attackEvent = true;
    }
}
