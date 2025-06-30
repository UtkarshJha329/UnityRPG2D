using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class JiggleFoliage : MonoBehaviour
{
    [SerializeField] float jiggleDistance = 0.15f;

    [SerializeField] private float jiggleForTime = 0.25f;
    [SerializeField] private float jiggleSpeed = 4.0f;

    private Vector3 jigglePositionA = Vector3.zero;
    private Vector3 jigglePositionB = Vector3.zero;

    public bool jiggle = false;

    private Vector3 currentJiggleToPosition = Vector3.zero;
    private float jiggleStopTime = 0.0f;

    private Rigidbody2D rb2d;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        jigglePositionA = transform.position + Vector3.left * jiggleDistance;
        jigglePositionB = transform.position + Vector3.right * jiggleDistance;

        currentJiggleToPosition = jigglePositionA;

        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (jiggle)
        {
            Jiggle();
        }
        else
        {
            jiggleStopTime = Time.time + jiggleForTime;
        }
    }

    private void Jiggle()
    {
        if (Vector3.Distance(transform.position, currentJiggleToPosition) < 0.1f)
        {
            currentJiggleToPosition = currentJiggleToPosition == jigglePositionA ? jigglePositionB : jigglePositionA;
        }

        Vector3 newPosition = transform.position + (currentJiggleToPosition - transform.position).normalized * jiggleSpeed * Time.deltaTime;
        rb2d.MovePosition(newPosition);

        if (Time.time >= jiggleStopTime)
        {
            //Debug.Log("Stopped jiggling.");
            jiggle = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        jiggle = true;
        jiggleStopTime = Time.time + jiggleForTime;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        jiggle = true;
        jiggleStopTime = Time.time + jiggleForTime;
    }
}
