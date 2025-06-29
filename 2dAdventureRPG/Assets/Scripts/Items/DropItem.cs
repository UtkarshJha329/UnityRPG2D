using UnityEngine;


public enum DropType
{
    DamageDrop,
    HealthDrop,
    SpeedDrop,
    PlayerDeath
}


//[RequireComponent(typeof(SpriteRenderer))]
//[RequireComponent(typeof(CircleCollider2D))]
public class DropItem : MonoBehaviour
{
    public DropType dropItemType;

    public SpriteRenderer dropSpriteRenderer;
    public Sprite damageDropSprite;
    public Sprite healthDropSprite;
    public Sprite speedDropSprite;

    //public CircleCollider2D dropPlayerPickupCollider;

    private PlayerHealth s_PlayerHealthManager;
    private PlayerProperties s_PlayerProperties;

    public float pickupDistance = 2.5f;

    public float bobUpDownDistance = 0.75f;
    public float bobSpeed = 1.0f;

    private Vector3 topBobPosition = Vector3.zero;
    private Vector3 bottomBobPosition = Vector3.zero;

    private Vector3 currentBobToPosition = Vector3.zero;

    private void Awake()
    {
        //dropSpriteRenderer = GetComponent<SpriteRenderer>();
        //dropPlayerPickupCollider = GetComponent<CircleCollider2D>();

        GameObject playerGameObject = GameObject.FindGameObjectWithTag("Player");
        s_PlayerProperties = playerGameObject.GetComponent<PlayerProperties>();
        s_PlayerHealthManager = playerGameObject.GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        //if(Vector3.Distance(transform.position, s_PlayerProperties.transform.position) <= 2.5f)
        //{
        //    transform.position += (s_PlayerProperties.transform.position - transform.position).normalized * 0.15f;
        //}

        if(transform.position.y < s_PlayerHealthManager.transform.position.y)
        {
            dropSpriteRenderer.sortingOrder = 500;
        }
        else
        {
            dropSpriteRenderer.sortingOrder = 0;
        }

        if (Vector3.Distance(transform.position, s_PlayerProperties.transform.position) <= pickupDistance)
        {
            if (dropItemType == DropType.DamageDrop)
            {
                Debug.Log("Increased player attack damage.");

                s_PlayerProperties.attackDamageValue--;
            }
            else if (dropItemType == DropType.SpeedDrop)
            {
                Debug.Log("Increased player speed.");

                s_PlayerProperties.speed++;
            }
            else if (dropItemType == DropType.HealthDrop)
            {
                Debug.Log("Increased player health.");

                s_PlayerHealthManager.IncreaseMaxHealth(1);
                s_PlayerHealthManager.ChangeHealth(1);
            }
            else if (dropItemType == DropType.PlayerDeath)
            {
                //s_PlayerProperties.attackDamageValue++;
            }

            Destroy(gameObject);
        }

        if(Vector3.Distance(dropSpriteRenderer.transform.position, currentBobToPosition) <= 0.2f)
        {
            currentBobToPosition = currentBobToPosition == topBobPosition ? bottomBobPosition : topBobPosition;
        }

        dropSpriteRenderer.transform.position += (currentBobToPosition - dropSpriteRenderer.transform.position).normalized * bobSpeed * Time.deltaTime;
    }

    public void SetDropTypeAndInitDrop(DropType dropType)
    {
        dropItemType = dropType;

        if (dropItemType == DropType.DamageDrop)
        {
            dropSpriteRenderer.sprite = damageDropSprite;
            transform.localScale = Vector3.one * 1.15f;
        }
        else if (dropItemType == DropType.HealthDrop)
        {
            dropSpriteRenderer.sprite = healthDropSprite;
            transform.localScale = Vector3.one * 2.0f;
        }
        else if (dropItemType == DropType.SpeedDrop)
        {
            dropSpriteRenderer.sprite = speedDropSprite;
            transform.localScale = Vector3.one * 1.25f;
        }

        //transform.localScale = Vector3.one * 2.0f;

        topBobPosition = transform.position + Vector3.up * bobUpDownDistance;
        bottomBobPosition = transform.position;

        currentBobToPosition = topBobPosition;
    }
}
