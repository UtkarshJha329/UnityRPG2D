using UnityEngine;

public class JiggleFoliage : MonoBehaviour
{
    [SerializeField] float jiggleDistance = 0.5f;

    private Vector3 jigglePositionA = Vector3.zero;
    private Vector3 jigglePositionB = Vector3.zero;

    private float jiggleForTime = 0.2f;
    private float jiggleSpeed = 4.0f;

    private bool jiggle = false;

    private Vector3 currentJiggleToPosition = Vector3.zero;
    private float jiggleStopTime = 0.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        jigglePositionA = transform.position + Vector3.left * jiggleDistance;
        jigglePositionB = transform.position + Vector3.right * jiggleDistance;

        currentJiggleToPosition = jigglePositionA;
    }

    // Update is called once per frame
    void Update()
    {
        if (jiggle)
        {
            if (Vector3.Distance(transform.position, currentJiggleToPosition) < 0.1f)
            {
                currentJiggleToPosition = currentJiggleToPosition == jigglePositionA ? jigglePositionB : jigglePositionA;
            }

            Jiggle();

            if(Time.time >= jiggleStopTime)
            {
                jiggle = false;
            }
        }
    }

    private void Jiggle()
    {
        transform.position += (currentJiggleToPosition - transform.position).normalized * jiggleSpeed * Time.deltaTime;
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
