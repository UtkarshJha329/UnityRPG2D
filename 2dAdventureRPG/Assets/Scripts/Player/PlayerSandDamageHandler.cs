using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerSandDamageHandler : MonoBehaviour
{
    [SerializeField] private float timeBetweenSandDamage = 5.0f;
    [SerializeField] private float knockbackForce = 0.5f;
    [SerializeField] private float sandDamageAmount = -1.0f;

    private PlayerProperties s_PlayerProperties;
    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;

    private MapGenerator mapGenerator;

    private float nextSandDamageTime = 0.0f;

    private void Awake()
    {
        s_PlayerProperties = GetComponent<PlayerProperties>();

        playerHealth = GetComponent<PlayerHealth>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mapGenerator = GameObject.FindGameObjectWithTag("MapTileGrid").GetComponent<MapGenerator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (nextSandDamageTime <= Time.time)
        {
            Vector3Int playerTilePos = new Vector3Int((int)transform.position.x, (int)transform.position.y, 0);
            if (mapGenerator.IsTileSand(playerTilePos))
            {
                playerHealth.ChangeHealth(sandDamageAmount);
                playerMovement.KnockbackPlayer(knockbackForce, Vector3.right * transform.localScale.x);

                s_PlayerProperties.sandDamage = true;

                nextSandDamageTime = Time.time + timeBetweenSandDamage;
            }
        }
    }
}
