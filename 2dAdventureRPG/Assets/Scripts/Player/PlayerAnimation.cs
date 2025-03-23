using System.Collections;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;


[RequireComponent(typeof(CharacterStates))]
[RequireComponent(typeof(PlayerProperties))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerAnimation : MonoBehaviour
{

    private Rigidbody2D rb2d;
    private Animator animator;

    private CharacterStates characterStates;
    private PlayerProperties s_PlayerProperties;

    private bool waitingToResetKnockback = false;

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        characterStates = GetComponent<CharacterStates>();
        s_PlayerProperties = GetComponent<PlayerProperties>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!characterStates.IsAttacking())
        {
            if (Mathf.Abs(s_PlayerProperties.currentMovementInput.magnitude) > 0)
            {
                characterStates.isIdling = false;
                characterStates.isMoving = true;
            }
            else
            {
                characterStates.isIdling = true;
                characterStates.isMoving = false;
            }
        }
    }

    private void LateUpdate()
    {
        animator.SetBool("isIdling", characterStates.isIdling);
        animator.SetBool("isMoving", characterStates.isMoving);
        animator.SetBool("isAttackingSide", characterStates.isAttackingSide);
        animator.SetBool("isAttackingDown", characterStates.isAttackingDown);
        animator.SetBool("isAttackingUp", characterStates.isAttackingUp);

        if (characterStates.IsAttacking())
        {
            int numLoops = (int)animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            float fractionalPart = animator.GetCurrentAnimatorStateInfo(0).normalizedTime - numLoops;
            if (fractionalPart >= 0.9f)
            {
                characterStates.ResetCharacterStatesToFalse();
                characterStates.isIdling = true;
            }
        }

        if (characterStates.isKnockbacked && !waitingToResetKnockback)
        {
            StartCoroutine(KnockbackResetTime());
            waitingToResetKnockback = true;
        }
    }

    IEnumerator KnockbackResetTime()
    {
        yield return new WaitForSeconds(s_PlayerProperties.knockbackTime);

        rb2d.linearVelocity = Vector2.zero;
        characterStates.isKnockbacked = false;
        waitingToResetKnockback = false;
    }
}
