using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GolfBall : MonoBehaviour
{
    private Rigidbody2D rb2D;
    private AngleIndicator angleIndicator;
    private PowerIndicator powerIndicator;

    private ParticleSystem golfBallTrail;

    private Vector2 startingPos;
    private Vector2 endPos;
    private Vector2 currentDirection;
    private Vector2 currentDistance;

    public Action OnLanding = null;

    public float bestDistance;

    private void Awake()
    {
        golfBallTrail = FindAnyObjectByType<ParticleSystem>();
    }
    private void Start()
    {
        GameManager.instance.ShootActivate -= OnShoot;
        GameManager.instance.ShootActivate += OnShoot;

        OnLanding -= WhenLanding;
        OnLanding += WhenLanding;

        rb2D = GetComponent<Rigidbody2D>();

        angleIndicator = GetComponent<AngleIndicator>();
        powerIndicator = GetComponent<PowerIndicator>();

        if (angleIndicator == null || powerIndicator == null)
        {
            Debug.LogError("AngleIndicator or PowerIndicator is not assigned!");
        }

        startingPos = transform.position;
    }

    private void Update()
    {
    }

    private void OnShoot()
    {
        if (angleIndicator == null || powerIndicator == null)
        {
            Debug.LogError("Cannot shoot: Missing references.");
            return;
        }

        Debug.Log($"Shooting with Direction: {angleIndicator.direction}, Power: {powerIndicator.powerValue}");
        rb2D.AddForce(angleIndicator.direction * powerIndicator.powerValue, ForceMode2D.Impulse);
        golfBallTrail.Play();

    }

    private void WhenLanding()
    {
        Distance();
        golfBallTrail.Stop();
        GameManager.instance.WhenLanding();

    }

    public float Distance()
    {
        float previousDistance = currentDistance.magnitude;
        endPos = transform.position;

        currentDistance = new Vector2(endPos.x - startingPos.x, 0);

        if (currentDistance.magnitude > previousDistance)
        {
            bestDistance = currentDistance.magnitude;
            return bestDistance;

        }
        else
        {
            return currentDistance.magnitude;
        }
    }

    public void ResetPlayer()
    {
        transform.position = startingPos;

        GameManager.instance.ShootActivate -= OnShoot;
        GameManager.instance.ShootActivate += OnShoot;

        GameManager.instance.ResetPlayer -= ResetPlayer;
        GameManager.instance.ResetPlayer += ResetPlayer;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") && GameManager.instance.matchState == MatchState.GolfBallInAir)
        {
            OnLanding?.Invoke();
        }
    }
}
