using DG.Tweening;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CharacterStates))]
[RequireComponent(typeof(EnemyProperties))]
public class EnemyStateAnimationApplier : MonoBehaviour
{
    private SpriteRenderer currentEnemySprite;
    private Animator animator;
    private Rigidbody2D rb2d;
    private CharacterStates characterStates;
    private EnemyProperties s_EnemyProperties;

    private bool waitingToResetKnockback;

    private void Awake()
    {
        currentEnemySprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        rb2d = GetComponent<Rigidbody2D>();
        characterStates = GetComponent<CharacterStates>();
        s_EnemyProperties = GetComponent<EnemyProperties>();

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool("isIdling", characterStates.isIdling);
        animator.SetBool("isMoving", characterStates.isMoving);
        animator.SetBool("isAttackingSide", characterStates.isAttackingSide);
        animator.SetBool("isAttackingDown", characterStates.isAttackingDown);
        animator.SetBool("isAttackingUp", characterStates.isAttackingUp);

        currentEnemySprite.sortingOrder = ((int)transform.position.y * -1);
    }

    private void LateUpdate()
    {
        if(characterStates.IsAttacking() && s_EnemyProperties.enemyType != EnemyType.TNTBarrelGoblin)
        {
            int numLoops = (int)animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            float fractionalPart = animator.GetCurrentAnimatorStateInfo(0).normalizedTime - numLoops;
            if(fractionalPart >= 0.9f)
            {
                characterStates.ResetCharacterStatesToFalse();
                characterStates.isIdling = true;
                //Debug.Log("Reset states to false.");
            }
        }

        if (characterStates.isKnockbacked && !waitingToResetKnockback)
        {
            DamageFlash();
            StartCoroutine(KnockbackResetTime());
            waitingToResetKnockback = true;
        }
    }
    IEnumerator KnockbackResetTime()
    {
        yield return new WaitForSeconds(s_EnemyProperties.personalKnockbackTime);

        rb2d.linearVelocity = Vector2.zero;
        characterStates.isKnockbacked = false;
        waitingToResetKnockback = false;
    }

    private void DamageFlash()
    {
        Color originalSpriteColor = currentEnemySprite.color;

        Sequence damageFlashSequence = DOTween.Sequence();
        damageFlashSequence.Append(currentEnemySprite.DOColor(Color.red, s_EnemyProperties.personalDamageFlashTime).SetEase(Ease.OutElastic));
        damageFlashSequence.Append(currentEnemySprite.DOColor(originalSpriteColor, s_EnemyProperties.personalDamageFlashTime).SetEase(Ease.OutElastic));

        damageFlashSequence.SetLoops(s_EnemyProperties.numDamageFlashLoops);
    }

}
