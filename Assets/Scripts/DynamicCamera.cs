using UnityEngine;

public class DynamicCamera : MonoBehaviour
{
    private Transform golfBallPos;
    private Vector3 offset = new Vector3(0, 0, -10);
    private Vector3 startPos;
    public float smoothSpeed = 5f;

    void Start()
    {
        startPos = transform.position;

        if (golfBallPos == null)
        {
            GolfBall golfBall = FindObjectOfType<GolfBall>();
            if (golfBall != null)
            {
                golfBallPos = golfBall.transform;
            }
            else
            {
                Debug.LogError("GolfBall not found! Assign it in the Inspector.");
            }
        }
    }

    void Update()
    {
        if (golfBallPos != null && GameManager.instance.matchState == MatchState.GolfBallInAir)
        {
            offset = new Vector3(0, 0, -10);
            Vector3 targetPosition = golfBallPos.position + offset;

            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);
            smoothSpeed = 20f;
        }
        else if (golfBallPos != null && GameManager.instance.matchState == MatchState.GolfBallGrounded)
        {
            offset = new Vector3(0, 3.9f, -10);
            Vector3 targetPosition = golfBallPos.position + offset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);

        }
        else if (golfBallPos != null && GameManager.instance.matchState == MatchState.Intermission)
        {
            Vector3 targetPosition = startPos;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);
        }
    }
}
