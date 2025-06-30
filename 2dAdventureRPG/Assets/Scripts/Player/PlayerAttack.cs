using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterStates))]
[RequireComponent(typeof(PlayerProperties))]
public class PlayerAttack : MonoBehaviour
{
    public Collider2D[] attackPoints; // 0 => right side attack, 1 => left side attack, 2 => down attack, 3 => up attack

    private CharacterStates characterStates;
    private PlayerProperties s_PlayerProperties;

    private int[][] directionalAttackCollisionPointPairs = new int[4][] { new int[2] { 0, 3 }, new int[2] { 1, 3 }, new int[2] { 2, 1 }, new int[] { 3, 0, 1 } };
    private List<List<Collider2D>> colliderPairs = new List<List<Collider2D>>();

    private float canAttackAtNextTime = 0.0f;

    private void Awake()
    {
        characterStates = GetComponent<CharacterStates>();
        s_PlayerProperties = GetComponent<PlayerProperties>();

    }
    private void Start()
    {
        for (int i = 0; i < directionalAttackCollisionPointPairs.Length; i++)
        {
            List<Collider2D> colliders2D = new List<Collider2D>();
            for (int j = 0; j < directionalAttackCollisionPointPairs[i].Length; j++)
            {
                //Debug.Log(attackPoints[directionalAttackCollisionPointPairs[i][j]].transform.parent.name);
                colliders2D.Add(attackPoints[directionalAttackCollisionPointPairs[i][j]]);
            }
            colliderPairs.Add(colliders2D);
        }

    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && canAttackAtNextTime <= Time.time)
        {
            if(s_PlayerProperties.lastMovementInput.x != 0.0f)
            {
                characterStates.isAttackingSide = true;

                characterStates.isAttackingUp = false;
                characterStates.isAttackingDown = false;
                characterStates.isIdling = false;
                characterStates.isMoving = false;
            }
            else if(s_PlayerProperties.facingDirection.y > 0.0f)
            {
                characterStates.isAttackingUp = true;

                characterStates.isAttackingSide = false;
                characterStates.isAttackingDown = false;
                characterStates.isIdling = false;
                characterStates.isMoving = false;
            }
            else if(s_PlayerProperties.facingDirection.y < 0.0f)
            {
                characterStates.isAttackingDown = true;

                characterStates.isAttackingSide = false;
                characterStates.isAttackingUp = false;
                characterStates.isIdling = false;
                characterStates.isMoving = false;
            }

            canAttackAtNextTime = Time.time + s_PlayerProperties.canAttackInTime;
        }

        if (characterStates.attackEvent)
        {
            Attack();
            characterStates.attackEvent = false;
        }
    }

    private void Attack()
    {
        if (characterStates.isAttackingSide)
        {
            if (s_PlayerProperties.facingDirection.x > 0.0f)
            {
                AttackUsingColliders(colliderPairs[0]);
            }
            else if (s_PlayerProperties.facingDirection.x < 0.0f)
            {
                AttackUsingColliders(colliderPairs[0]);
            }
        }
        else if (characterStates.isAttackingDown)
        {
            AttackUsingColliders(colliderPairs[2]);
        }
        else if (characterStates.isAttackingUp)
        {
            AttackUsingColliders(colliderPairs[3]);
        }

    }

    private void AttackUsingColliders(List<Collider2D> colliderPairsToCheck)
    {
        foreach (Collider2D collider2D in colliderPairsToCheck)
        {
            List<Collider2D> overlapResults = new List<Collider2D>();
            collider2D.Overlap(overlapResults);

            foreach (Collider2D collidedColliders in overlapResults)
            {
                if (!collidedColliders.isTrigger && collidedColliders.CompareTag("Enemy"))
                {
                    //Debug.Log(collider2D.name + " collided with an enemy! " + collidedColliders.gameObject.name);
                    EnemyHealth enemyHealthManager = collidedColliders.gameObject.GetComponent<EnemyHealth>();
                    Enemy_Movement enemyMovemnetManager = collidedColliders.gameObject.GetComponent<Enemy_Movement>();

                    enemyHealthManager.ChangeHealth(s_PlayerProperties.attackDamageValue);

                    Vector3 knockbackDirection = enemyHealthManager.transform.position - transform.position;
                    enemyMovemnetManager.KnockbackEnemy(s_PlayerProperties.knockbackEnemyWithForce, knockbackDirection);

                    s_PlayerProperties.playerHitStopManager.StopTimeFor(s_PlayerProperties.enemyHitTimeStopTime, s_PlayerProperties.enemyHitTimeStopTimeScale);
                    s_PlayerProperties.impulseSourceForScreenShake.GenerateImpulseWithVelocity(knockbackDirection.normalized * 0.05f);
                }
                else if (!collidedColliders.isTrigger && collidedColliders.CompareTag("Structure"))
                {
                    StructureHealth structureHealth = collidedColliders.gameObject.GetComponent<StructureHealth>();
                    structureHealth.DamageStructure(-1);

                    Vector3 knockbackDirection = structureHealth.transform.position - transform.position;
                    s_PlayerProperties.playerHitStopManager.StopTimeFor(s_PlayerProperties.enemyHitTimeStopTime, s_PlayerProperties.enemyHitTimeStopTimeScale);
                    s_PlayerProperties.impulseSourceForScreenShake.GenerateImpulseWithVelocity(knockbackDirection.normalized * 0.05f);
                }
            }
        }
    }

    //Set attack event to true from animation events so that enemy can check for player and deal damage to him.
    public void AttackEvent()
    {
        //Debug.Log("Attacking the player from animation event.");
        characterStates.attackEvent = true;
    }


}
