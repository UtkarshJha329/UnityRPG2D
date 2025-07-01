using DG.Tweening;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;


[RequireComponent(typeof(CharacterStates))]
[RequireComponent(typeof(PlayerProperties))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAnimation : MonoBehaviour
{
    private Rigidbody2D rb2d;
    private SpriteRenderer currentPlayerSpriteRenderer;
    private Animator animator;

    private CharacterStates characterStates;
    private PlayerProperties s_PlayerProperties;

    private bool waitingToResetKnockback = false;

    private float nextFlashAtSecond = 0.0f;
    private int numTimesDamageFlashed = 6;
    private float minGapBetweenDamageFlashes = 0.0f;

    private Color originalSpriteColor;

    private PlayerHealth playerHealth;

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        currentPlayerSpriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        characterStates = GetComponent<CharacterStates>();
        s_PlayerProperties = GetComponent<PlayerProperties>();
        playerHealth = GetComponent<PlayerHealth>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        minGapBetweenDamageFlashes = s_PlayerProperties.damageFlashTime / s_PlayerProperties.numDamageFlashLoops;
        originalSpriteColor = currentPlayerSpriteRenderer.color;
    }

    // Update is called once per frame
    void Update()
    {
        currentPlayerSpriteRenderer.sortingOrder = ((int)transform.position.y * -1);

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
            if(playerHealth.GetCurrentPlayerHealth() <= 0)
            {
                s_PlayerProperties.playerHitStopManager.StopTimeFor(5.0f, 0.1f);
            }
            else
            {
                s_PlayerProperties.playerHitStopManager.StopTimeFor(s_PlayerProperties.knockedBackTimeStopTime, s_PlayerProperties.knockedBackTimeStopTimeScale);      // Hit stop while taking a hit.
            }

            numTimesDamageFlashed = 0;
            s_PlayerProperties.impulseSourceForScreenShake.GenerateImpulseWithVelocity(s_PlayerProperties.knockBackDirection.normalized * 0.2f);
            StartCoroutine(KnockbackResetTime());
            waitingToResetKnockback = true;
        }

        if (numTimesDamageFlashed < s_PlayerProperties.numDamageFlashLoops)
        {
            CustomDamageFlash();
        }
        else
        {
            currentPlayerSpriteRenderer.color = originalSpriteColor;
        }
    }

    IEnumerator KnockbackResetTime()
    {
        yield return new WaitForSeconds(s_PlayerProperties.knockbackTime);

        rb2d.linearVelocity = Vector2.zero;
        characterStates.isKnockbacked = false;
        waitingToResetKnockback = false;
    }
    private void DamageFlash()
    {

        Sequence damageFlashSequence = DOTween.Sequence();
        damageFlashSequence.Append(currentPlayerSpriteRenderer.DOColor(Color.red, s_PlayerProperties.personalDamageFlashTime).SetEase(Ease.OutElastic));
        damageFlashSequence.Append(currentPlayerSpriteRenderer.DOColor(originalSpriteColor, s_PlayerProperties.personalDamageFlashTime).SetEase(Ease.OutElastic));

        damageFlashSequence.SetLoops(s_PlayerProperties.numDamageFlashLoops);
    }

    private void CustomDamageFlash()
    {
        if (nextFlashAtSecond <= Time.time)
        {
            //Debug.Log("Flashing from damage.");
            currentPlayerSpriteRenderer.color = currentPlayerSpriteRenderer.color == originalSpriteColor ? Color.red : originalSpriteColor;

            numTimesDamageFlashed++;
            nextFlashAtSecond = Time.time + minGapBetweenDamageFlashes;
        }
    }

}
