using UnityEngine;

public enum EnemyType
{
    TorchGoblin,
    BombGoblin,
    TNTBarrelGoblin
}

public class EnemyProperties : MonoBehaviour
{
    public float speed = 3.0f;

    public float attackRange = 2.0f;
    public float detectorCircleOffset = 2.5f;
    public float detectorCircleRadius = 4.0f;

    public float damage = -1.0f;
    public float delayTimeToAttackAfterPlayerDetection = 1.5f;
    public float delayTimeToAttackAfterPreviousAttack = 0.75f;

    public Vector2 attakcingDownOffset = new Vector2(1.5f, -0.75f);
    public Vector2 attakcingUpOffset = new Vector2(1.5f, 0.75f);

    public Vector3 facingDirection = Vector3.one;

    public float knockbackForce = 10.0f;

    public float personalKnockbackTime = 0.25f;
    public float personalDamageFlashTime = 0.25f * 0.25f;
    public float personalDamageTextTime = 7.5f;
    public int numDamageFlashLoops = 5;

    public float patrolingWayPointStoppingDistance = 0.1f;
    public float wanderWayPointStoppingDistance = 0.5f;

    public bool mineGuard = false;
    public bool connectedToMineGuardSection = false;

    public int sectionIndex = -1;
    public Vector2Int roomIndex = new Vector2Int(-1, -1);

    public EnemyType enemyType = EnemyType.TorchGoblin;

    public Vector3 dynamiteSpawnPosition = Vector3.zero;
    public GameObject dynamiteSpawnObject;

    public bool canAttack = false;

    public Transform dynamiteShadowsParentTransform;

    public DropType dropType;

    public bool castleRoomEnemies = false;

    public float audioSourcePitch = 1.2f;

    public float dynamiteExplosionVolume = 0.0f;
}
