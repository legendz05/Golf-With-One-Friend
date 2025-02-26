using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

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
    [SerializeField] private GameObject endGameCanvas;

    private bool isRoundOver = false;
    private float hostBestScore = 0;
    private float guestBestScore = 0;
    private string lobbyCode;

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

        endGameCanvas.SetActive(false);

    }

    void Start()
    {
        currentRound = 1;

        string userID = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        FirebaseDatabase.DefaultInstance
            .GetReference("users")
            .Child(userID)
            .Child("lobbyCode")
            .GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && task.Result.Exists)
                {
                    lobbyCode = task.Result.Value.ToString();
                    Debug.Log($"Lobby Code: {lobbyCode}");
                }
            });

        StartCoroutine(RoundBegin());
    }


    private IEnumerator RoundBegin()
    {
        SetAngle -= OnAngleSet;
        SetAngle += OnAngleSet;

        SetPower -= OnPowerSet;
        SetPower += OnPowerSet;

        StartCoroutine(TransitionToState(MatchState.Intermission));
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
        StartCoroutine(TransitionToState(MatchState.GolfBallAngle));

        yield return null;
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
        if (isRoundOver) return;
        Debug.Log("WhenLanding called: Changing state to GolfBallGrounded");
        StartCoroutine(TransitionToState(MatchState.GolfBallGrounded));
        UpdateScore();
        Invoke(nameof(RoundOver), 5);
    }


    private void UpdateScore()
    {
        float currentDistance = golfBall.Distance();
        distanceText.text = $"Distance: {currentDistance}m";

        string userID = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

        if (IsHost())
        {
            if (currentDistance > hostBestScore)
            {
                hostBestScore = currentDistance;
                FirebaseDatabase.DefaultInstance
                    .GetReference("lobbies")
                    .Child(lobbyCode)
                    .Child("Scores")
                    .Child("Host")
                    .SetValueAsync(hostBestScore);
            }
        }
        else
        {
            if (currentDistance > guestBestScore)
            {
                guestBestScore = currentDistance;
                FirebaseDatabase.DefaultInstance
                    .GetReference("lobbies")
                    .Child(lobbyCode)
                    .Child("Scores")
                    .Child("Guest")
                    .SetValueAsync(guestBestScore);
            }
        }
    }

    private bool IsHost()
    {
        string userID = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        bool isHost = false;

        FirebaseDatabase.DefaultInstance
            .GetReference("users")
            .Child(userID)
            .Child("lobbyCode")
            .GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && task.Result.Exists)
                {
                    lobbyCode = task.Result.Value.ToString();

                    FirebaseDatabase.DefaultInstance
                        .GetReference("lobbies")
                        .Child(lobbyCode)
                        .Child("Host")
                        .GetValueAsync().ContinueWithOnMainThread(task2 =>
                        {
                            if (task2.IsCompleted && task2.Result.Exists)
                            {
                                isHost = (task2.Result.Value.ToString() == userID);
                            }
                        });
                }
            });

        return isHost;
    }

    private IEnumerator TransitionToState(MatchState newState, bool invokeShoot = false)
    {
        yield return new WaitForSeconds(0.1f);
        matchState = newState;

        if (invokeShoot)
        {
            GameObject golfClub = GameObject.FindWithTag("Golfclub");
            Animator golfClubAnimator = golfClub.GetComponent<Animator>();
            AnimatorStateInfo info = golfClubAnimator.GetCurrentAnimatorStateInfo(0);
            golfClubAnimator.Play("GolfSwing");

            while (!info.IsName("stop"))
            {
                Debug.Log("While!");
                info = golfClubAnimator.GetCurrentAnimatorStateInfo(0);
                yield return new WaitForEndOfFrame();
            }

            Debug.Log("Invoking Shoot action...");
            ShootActivate?.Invoke();
            StartCoroutine(PowerUpManager.instance.PowerUpTimer());
        }
    }

    private void RoundOver()
    {
        if (isRoundOver) return;
        isRoundOver = true;

        currentRound += 1;
        Debug.Log($"RoundOver called. Current Round: {currentRound}");

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
            CheckBestScore();
            endGameCanvas.SetActive(true);
            Invoke(nameof(LoadMain), 5);
        }

        isRoundOver = false;
    }


    void LoadMain()
    {
        if (!string.IsNullOrEmpty(lobbyCode))
        {
            FirebaseDatabase.DefaultInstance
                .GetReference("lobbies")
                .Child(lobbyCode)
                .RemoveValueAsync().ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompleted)
                    {
                        Debug.Log($"Lobby {lobbyCode} deleted successfully.");
                    }
                    else
                    {
                        Debug.LogError($"Failed to delete lobby: {task.Exception}");
                    }
                });
        }

        SceneManager.LoadScene("MainMenu");
    }


    private void CheckBestScore()
    {
        FirebaseDatabase.DefaultInstance
            .GetReference("lobbies")
            .Child(lobbyCode)
            .Child("Scores")
            .GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && task.Result.Exists)
                {
                    DataSnapshot scores = task.Result;
                    int hostScore = int.Parse(scores.Child("Host").Value.ToString());
                    int guestScore = int.Parse(scores.Child("Guest").Value.ToString());

                    if (hostScore > guestScore)
                    {
                        bestDistance.text = $"Winner: Host with {hostScore}m";
                    }
                    else if (guestScore > hostScore)
                    {
                        bestDistance.text = $"Winner: Guest with {guestScore}m";
                    }
                    else
                    {
                        bestDistance.text = "It's a Tie!";
                    }
                }
            });
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
