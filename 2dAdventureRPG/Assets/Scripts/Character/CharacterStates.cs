using Unity.VisualScripting;
using UnityEngine;

public class CharacterStates : MonoBehaviour
{
    public bool isIdling = true;
    public bool isMoving = false;
    public bool isAttackingSide = false;
    public bool isAttackingDown = false;
    public bool isAttackingUp = false;
    public bool isKnockbacked = false;

    public bool isDead = false;

    public bool attackEvent = false;

    public bool IsAttacking()
    {
        return isAttackingDown || isAttackingUp || isAttackingSide;
    }

    public void ResetCharacterStatesToFalse()
    {
        isIdling = false;
        isMoving = false;
        isAttackingDown = false;
        isAttackingUp = false;
        isAttackingSide = false;

        attackEvent = false;
    }
}
