using System;
using System.Collections;
using UnityEngine;

public class AngleIndicator : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Transform shootingOrigin;
    public float lineLength;
    public Vector2 direction;

    [SerializeField] private float currentAngle;

    private bool decreaseAngle = false;

    private void Start()
    {
        GameManager.instance.ResetPlayer -= ResetAngle;
        GameManager.instance.ResetPlayer += ResetAngle;

        currentAngle = 0f;
    }
    void Update()
    {
        CheckAngleStateInMatch();
    }

    public void CheckAngleStateInMatch()
    {
        if (GameManager.instance.matchState == MatchState.GolfBallAngle)
        {
            AngleSequence();

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    Debug.Log("Touch detected in AngleIndicator");
                    GameManager.instance.SetAngle?.Invoke();
                }
            }
        }

    }

    public void AngleSequence()
    {
        if (!decreaseAngle)
        {
            currentAngle += 1f; // Increase angle

            if (currentAngle >= 90)
            {
                decreaseAngle = !decreaseAngle;
            }
        }
        else if (decreaseAngle)
        {
            currentAngle -= 1f; // Decrease angle

            if (currentAngle <= 0)
            {
                decreaseAngle = !decreaseAngle;
            }
        }

        currentAngle = Mathf.Clamp(currentAngle, 0f, 90f);

        direction = new Vector2(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad));

        Vector3 startPoint = shootingOrigin.position;
        Vector3 endPoint = startPoint + new Vector3(direction.x, direction.y, 0) * lineLength;

        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);
    }

    public void ResetAngle()
    {
        currentAngle = 0f;

        GameManager.instance.ResetPlayer -= ResetAngle;
        GameManager.instance.ResetPlayer += ResetAngle;
    }
}
