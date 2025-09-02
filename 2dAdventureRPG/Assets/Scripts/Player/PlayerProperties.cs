using System.Collections;
using Unity.Cinemachine;
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
    public float damageFlashTime = 1.0f;

    public float damageTextTime = 7.5f;

    public Vector2Int facingDirection;
    public Vector2 currentMovementInput = Vector2.zero;
    public Vector2 lastMovementInput = Vector2.zero;

    public TimeStop playerHitStopManager;

    public float knockedBackTimeStopTime = 1.0f;
    public float knockedBackTimeStopTimeScale = 0.0f;
    public float enemyHitTimeStopTime = 2.0f;
    public float enemyHitTimeStopTimeScale = 0.0f;

    private float damageColourFlashForSeconds = 5.0f;

    public Vector2 knockBackDirection = Vector2.zero;

    public CinemachineImpulseSource impulseSourceForScreenShake;

    public bool sandDamage = false;

    public bool isPlayingCutscene = false;
    public Vector2 currentCutsceneMovementTarget = Vector2.zero;

    private void Awake()
    {
        playerHitStopManager = GameObject.FindGameObjectWithTag("HitStopManager").GetComponent<TimeStop>();
        if(playerHitStopManager == null)
        {
            Debug.LogError("A reference for playerHitStopManager has not been found in the scene in the PlayerProperties script!");
        }

        impulseSourceForScreenShake = GetComponent<CinemachineImpulseSource>();

    }
}
