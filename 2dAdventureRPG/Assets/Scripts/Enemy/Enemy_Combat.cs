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

    private void Awake()
    {
        characterStates = GetComponent<CharacterStates>();
        s_EnemyProperties = GetComponent<EnemyProperties>();

        animator = gameObject.GetComponent<Animator>();

        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        playerHealthManager = playerTransform.GetComponent<PlayerHealth>();
        s_PlayerMovement = playerTransform.GetComponent<PlayerMovement>();
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
        if((playerTransform.position - transform.position).magnitude <= s_EnemyProperties.attackRange)
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

        if (characterStates.attackEvent)
        {
            AttackPlayer();
            characterStates.attackEvent = false;
        }
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
        if (characterStates.isAttackingSide)
        {
            if(s_EnemyProperties.facingDirection.x > 0.0f)
            {
                AttackPlayerUsingColliders(colliderPairs[0]);
            }
            else if(s_EnemyProperties.facingDirection.x < 0.0f)
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
