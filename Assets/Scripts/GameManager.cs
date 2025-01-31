using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    public MatchState matchState;

    public Action SetAngle = null;
    public Action SetPower = null;
    public Action ShootActivate = null;
    public Action GameOver = null;
    public Action ResetPlayer = null;

    public UnityEvent ResetRound;

    private GolfBall golfBall;
    public int currentRound;


    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI roundCountDownText;
    [SerializeField] private TextMeshProUGUI bestDistance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        golfBall = FindAnyObjectByType<GolfBall>();
        if (golfBall != null) { }
    }

    private void Start()
    {
        currentRound = 1;

        StartCoroutine(RoundBegin());
    }

    private IEnumerator RoundBegin()
    {
        SetAngle -= OnAngleSet;
        SetAngle += OnAngleSet;

        SetPower -= OnPowerSet;
        SetPower += OnPowerSet;

        matchState = MatchState.Intermission;
        distanceText.text = "";

        yield return new WaitForSeconds(0.1f);
        roundCountDownText.text = "3";
        yield return new WaitForSeconds(1);
        roundCountDownText.text = "2";
        yield return new WaitForSeconds(1);
        roundCountDownText.text = "1";
        yield return new WaitForSeconds(1);
        roundCountDownText.text = "ROUND BEGIN";
        yield return new WaitForSeconds(1);

        roundCountDownText.text = "";
        matchState = MatchState.GolfBallAngle;

        yield return null;
    }

    void Update()
    {
        //Debug.Log($"Current MatchState: {matchState}");
    }


    public void OnAngleSet()
    {
        Debug.Log("OnAngleSet called: Changing state to GolfBallPower");
        StartCoroutine(TransitionToState(MatchState.GolfBallPower));

    }

    public void OnPowerSet()
    {
        Debug.Log("OnPowerSet called: Changing state to GolfBallInAir");
        StartCoroutine(TransitionToState(MatchState.GolfBallInAir, invokeShoot: true));

    }

    public void WhenLanding()
    {
        Debug.Log("WhenLanding called: Changing state to GolfBallGrounded");
        StartCoroutine(TransitionToState(MatchState.GolfBallGrounded));
        UpdateScore();
        Invoke(nameof(RoundOver), 5);
    }

    private void UpdateScore()
    {
        distanceText.text = $"Distance: {golfBall.Distance()}m";
    }

    private IEnumerator TransitionToState(MatchState newState, bool invokeShoot = false)
    {
        yield return new WaitForSeconds(0.1f);
        matchState = newState;

        if (invokeShoot)
        {
            Debug.Log("Invoking Shoot action...");
            ShootActivate?.Invoke();
            StartCoroutine(PowerUpManager.instance.PowerUpTimer());
        }
    }

    private void RoundOver()
    {
        currentRound += 1;

        if (currentRound < 4)
        {
            ResetPlayer?.Invoke();
            ResetRound?.Invoke();
            StartCoroutine(RoundBegin());
        }
        else
        {
            distanceText.text = "";
            bestDistance.text = $"Best Distance: {golfBall.bestDistance}m";
            Invoke(nameof(LoadMain), 3);

        }
    }

    void LoadMain()
    {
        SceneManager.LoadScene("MainMenu");
    }
}

public enum MatchState
{
    Intermission,
    GolfBallAngle,
    GolfBallPower,
    GolfBallInAir,
    GolfBallGrounded
}
