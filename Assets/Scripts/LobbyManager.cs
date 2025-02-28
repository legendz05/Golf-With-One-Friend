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
    private string lobbyCode;
    private bool isHost = false;
    private bool isGuest = false;


    private void OnEnable()
    {
        auth = FirebaseAuth.DefaultInstance;
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        Debug.Log("Fetching current user...");
        GetCurrentUser();
    }

    public void Update()
    {

    }

    public void ToggleReadyStatus()
    {
        if (string.IsNullOrEmpty(lobbyCode))
        {
            Debug.LogError("Lobby code is not set. Cannot update ready status.");
            return;
        }

        if (isHost)
        {
            dbReference.Child("lobbies").Child(lobbyCode).Child("Ready").Child("Host").SetValueAsync(true);
        }
        else if (isGuest)
        {
            dbReference.Child("lobbies").Child(lobbyCode).Child("Ready").Child("Guest").SetValueAsync(true);
        }
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

    void ListenForReadyStatus()
    {
        dbReference.Child("lobbies").Child(lobbyCode).Child("Ready").ValueChanged += HandleReadyStatusChanged;
    }

    private void HandleReadyStatusChanged(object sender, ValueChangedEventArgs e)
    {
        if (e.DatabaseError != null)
        {
            Debug.LogError($"Firebase error: {e.DatabaseError.Message}");
            return;
        }

        if (e.Snapshot.Exists)
        {
            bool hostReady = e.Snapshot.Child("Host").Value != null && (bool)e.Snapshot.Child("Host").Value;
            bool guestReady = e.Snapshot.Child("Guest").Value != null && (bool)e.Snapshot.Child("Guest").Value;

            readyPlayersText.text = $"Ready Players: {(hostReady ? 1 : 0) + (guestReady ? 1 : 0)}/2";

            if (hostReady && guestReady)
            {
                Debug.Log("Both players are ready. Starting the game...");
                SceneManager.LoadScene("GameScene");
            }
        }
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

            lobbyCode = task.Result.Value.ToString();
            Debug.Log($"Lobby code retrieved: {lobbyCode}");

            dbReference.Child("lobbies").Child(lobbyCode).GetValueAsync().ContinueWithOnMainThread(task2 =>
            {
                if (task2.Exception != null)
                {
                    Debug.LogError("Failed to get lobby details: " + task2.Exception);
                    return;
                }

                if (task2.Result.Exists)
                {
                    string host = task2.Result.Child("Host").Value?.ToString();
                    string guest = task2.Result.Child("Guest").Value?.ToString();

                    if (currentUser == host)
                    {
                        isHost = true;
                        Debug.Log("Current user is the Host.");
                    }
                    else if (currentUser == guest)
                    {
                        isGuest = true;
                        Debug.Log("Current user is the Guest.");
                    }

                    GetPlayersInLobby(lobbyCode);
                    lobbyCodeText.text = lobbyCode;

                    ListenForReadyStatus();
                }
            });
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

    void OnDisable()
    {
        if (!string.IsNullOrEmpty(lobbyCode))
        {
            if (isHost)
            {
                dbReference.Child("lobbies").Child(lobbyCode).Child("Ready").Child("Host").SetValueAsync(false);
            }
            else if (isGuest)
            {
                dbReference.Child("lobbies").Child(lobbyCode).Child("Ready").Child("Guest").SetValueAsync(false);
            }
        }
    }

}
