using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterStates))]
[RequireComponent(typeof(PlayerProperties))]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb2d;
    private CharacterStates characterStates;
    private PlayerProperties s_PlayerProperties;

    private Vector2 velocity;

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        characterStates = GetComponent<CharacterStates>();
        s_PlayerProperties = GetComponent<PlayerProperties>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        s_PlayerProperties.facingDirection = new Vector2Int(1, 1);
        //s_PlayerProperties.lastMovementInput = new Vector2(1.0f, 1.0f);
    }

    // Update is called once per frame
    private void Update()
    {
        //Debug.Log(characterStates.isKnockbacked);
        if (!characterStates.isKnockbacked && !characterStates.IsAttacking() && !characterStates.isDead)
        {
            if (!s_PlayerProperties.isPlayingCutscene)
            {
                s_PlayerProperties.currentMovementInput.x = Input.GetAxisRaw("Horizontal");
                s_PlayerProperties.currentMovementInput.y = Input.GetAxisRaw("Vertical");
            }
            else
            {
                if (Vector3.Distance(transform.position, new Vector3(s_PlayerProperties.currentCutsceneMovementTarget.x, s_PlayerProperties.currentCutsceneMovementTarget.y, 0.0f)) > 0.1f)
                {
                    s_PlayerProperties.currentMovementInput = (s_PlayerProperties.currentCutsceneMovementTarget - new Vector2(transform.position.x, transform.position.y)).normalized;
                }
                else
                {
                    s_PlayerProperties.currentMovementInput = Vector2.zero;
                }
            }

            if (s_PlayerProperties.currentMovementInput.y != 0.0f)
            {
                s_PlayerProperties.lastMovementInput.y = s_PlayerProperties.currentMovementInput.y;
                s_PlayerProperties.lastMovementInput.x = 0.0f;
            }
            if (s_PlayerProperties.currentMovementInput.x != 0.0f)
            {
                s_PlayerProperties.lastMovementInput.x = s_PlayerProperties.currentMovementInput.x;
            }

            s_PlayerProperties.facingDirection = new Vector2Int(s_PlayerProperties.currentMovementInput.x != 0.0f ? (int)Mathf.Sign(s_PlayerProperties.currentMovementInput.x) : s_PlayerProperties.facingDirection.x
                                                                , s_PlayerProperties.currentMovementInput.y != 0.0f ? (int)Mathf.Sign(s_PlayerProperties.currentMovementInput.y) : s_PlayerProperties.facingDirection.y);

            velocity = new Vector2(s_PlayerProperties.currentMovementInput.x, s_PlayerProperties.currentMovementInput.y);
            velocity = velocity.normalized * s_PlayerProperties.speed;

            transform.localScale = new Vector3(s_PlayerProperties.facingDirection.x, transform.localScale.y, transform.localScale.z);
        }
        else
        {
            s_PlayerProperties.currentMovementInput = Vector2.zero;
        }
    }

    private void FixedUpdate()
    {
        if (characterStates.IsAttacking())
        {
            rb2d.linearVelocity = Vector2.zero;
        }

        if (!characterStates.isKnockbacked && !characterStates.IsAttacking())
        {
            rb2d.linearVelocity = velocity;
        }
    }

    public void KnockbackPlayer(float knockbackForceValue, Vector3 knockbackDirection)
    {
        characterStates.isKnockbacked = true;

        s_PlayerProperties.knockBackDirection = knockbackDirection;
        Vector3 knockbackForce = knockbackDirection * knockbackForceValue;
        rb2d.AddForce(knockbackForce, ForceMode2D.Impulse);

    }
}
