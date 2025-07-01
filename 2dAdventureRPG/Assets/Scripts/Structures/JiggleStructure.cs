using UnityEngine;

public class JiggleStructure : MonoBehaviour
{
    [SerializeField] float jiggleDistance = 0.5f;

    [SerializeField] private float jiggleForTime = 0.25f;
    [SerializeField] private float jiggleSpeed = 4.0f;

    private Vector3 jigglePositionA = Vector3.zero;
    private Vector3 jigglePositionB = Vector3.zero;


    public bool jiggle = false;

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

        transform.position += (currentJiggleToPosition - transform.position).normalized * jiggleSpeed * Time.deltaTime;

        if (Time.time >= jiggleStopTime)
        {
            //Debug.Log("Stopped jiggling.");
            jiggle = false;
        }
    }
}
