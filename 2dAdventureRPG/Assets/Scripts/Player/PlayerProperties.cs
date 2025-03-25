using UnityEngine;


public class PlayerProperties : MonoBehaviour
{
    public float speed = 5.0f;

    public float attackDamageValue = -1.0f;
    public float canAttackInTime = 0.5f;

    public float knockbackEnemyWithForce = 5.0f;
    public float knockbackTime = 5.0f;
    public float personalDamageFlashTime = 0.25f * 0.25f;
    public int numDamageFlashLoops = 5;

    public float damageTextTime = 7.5f;

    public Vector2Int facingDirection;
    public Vector2 currentMovementInput = Vector2.zero;
    public Vector2 lastMovementInput = Vector2.zero;

}
