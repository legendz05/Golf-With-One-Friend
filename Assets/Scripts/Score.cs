using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

public class Score : MonoBehaviour
{
    private GolfBall golfBall;
    private float bestDistance;
    private FirebaseAuth auth;
    private DatabaseReference dbReference;

    void Awake()
    {
        golfBall = FindAnyObjectByType<GolfBall>();

        auth = FirebaseAuth.DefaultInstance;
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    void Update()
    {
        bestDistance = golfBall.bestDistance;
    }

    public void SaveBestScore()
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No user is signed in!");
            return;
        }

        string userId = auth.CurrentUser.UserId;

        dbReference.Child("users").Child(userId).Child("bestScore").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError("Failed to load best score: " + task.Exception);
                return;
            }

            float storedBestScore = 0;

            if (task.Result.Exists)
            {
                storedBestScore = float.Parse(task.Result.Value.ToString());
            }

            if (bestDistance > storedBestScore)
            {
                dbReference.Child("users").Child(userId).Child("bestScore").SetValueAsync(bestDistance).ContinueWithOnMainThread(saveTask =>
                {
                    if (saveTask.Exception != null)
                    {
                        Debug.LogError("Failed to save best score: " + saveTask.Exception);
                    }
                    else
                    {
                        Debug.Log("New best score saved: " + bestDistance);
                    }
                });
            }
            else
            {
                Debug.Log("Current score is not higher than saved best score.");
            }
        });
    }
}
