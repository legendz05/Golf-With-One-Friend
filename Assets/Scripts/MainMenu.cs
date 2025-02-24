using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private TMP_Text highscoreText;

    private FirebaseAuth auth;
    private DatabaseReference dbReference;

    public GameObject statsObject;

    void OnEnable()
    {
        FirebaseTest.instance.LoadUsernameFromFirebase((username) =>
        {
            Debug.Log("Username in New Scene: " + username);
            usernameText.text = username.ToString();

        });

        auth = FirebaseAuth.DefaultInstance;
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void Stats()
    {
        GetComponent<Lobbies>().menuObject.SetActive(false);
        GetComponent<Lobbies>().lobbyObject.SetActive(false);
        statsObject.SetActive(true);


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

            highscoreText.text = storedBestScore.ToString();
        });
    }

    // Update is called once per frame
    void Update()
    {

    }

}
