using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public DatabaseReference dbReference;
    public TextMeshProUGUI hostPlayer;
    public TextMeshProUGUI guestPlayer;
    public TextMeshProUGUI lobbyCodeText;
    public TextMeshProUGUI readyPlayersText;

    private FirebaseAuth auth;
    private string currentUser;
    private int readyPlayers = 0;

    private void OnEnable()
    {
        auth = FirebaseAuth.DefaultInstance;
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        Debug.Log("Fetching current user...");
        GetCurrentUser();
    }

    public void Update()
    {
        if (readyPlayers == 2)
        {
            SceneManager.LoadScene("GameScene");
            readyPlayers = 0;
        }
    }

    public void numOfPlayers()
    {
        readyPlayers++;
        readyPlayersText.text = $"Ready Players: {readyPlayers}/2";
    }

    void GetCurrentUser()
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No user is logged in.");
            return;
        }

        string userID = auth.CurrentUser.UserId;

        dbReference.Child("users").Child(userID).Child("userName").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError("Failed to get current username: " + task.Exception);
                return;
            }

            if (!task.Result.Exists)
            {
                Debug.LogError("Current user has no username in database.");
                return;
            }

            currentUser = task.Result.Value.ToString();
            Debug.Log($"Current user: {currentUser}");

            GetLobbyCode();
        });
    }

    void GetLobbyCode()
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("User is not authenticated. Cannot access lobby data.");
            return;
        }

        string userID = auth.CurrentUser.UserId;

        dbReference.Child("users").Child(userID).Child("lobbyCode").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError("Failed to get lobby code: " + task.Exception);
                return;
            }

            if (!task.Result.Exists || string.IsNullOrEmpty(task.Result.Value?.ToString()))
            {
                Debug.LogError("No lobby code found for user.");
                return;
            }

            string lobbyCode = task.Result.Value.ToString();
            Debug.Log($"Lobby code retrieved: {lobbyCode}");

            GetPlayersInLobby(lobbyCode);
            lobbyCodeText.text = lobbyCode;
        });
    }

    void GetPlayersInLobby(string lobbyCode)
    {
        dbReference.Child("lobbies").Child(lobbyCode).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError("Failed to load lobby data: " + task.Exception);
                return;
            }

            if (!task.Result.Exists)
            {
                Debug.LogError("Lobby not found!");
                return;
            }

            string host = task.Result.Child("Host").Value?.ToString() ?? "Unknown Host";
            string guest = task.Result.Child("Guest").Value?.ToString() ?? "No Guest";

            Debug.Log($"Lobby found: {lobbyCode}");

            if (hostPlayer != null)
                hostPlayer.text = $"Host: {host}";
            else
                Debug.LogError("hostPlayer UI reference is missing!");

            if (guestPlayer != null)
                guestPlayer.text = $"Guest: {guest}";
            else
                Debug.LogError("guestPlayer UI reference is missing!");
        });
    }
}
