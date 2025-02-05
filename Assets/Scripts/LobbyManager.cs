using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    public DatabaseReference dbReference;
    public TextMeshProUGUI hostPlayer;
    public TextMeshProUGUI guestPlayer;

    private FirebaseAuth auth;
    private string currentUser;

    private void OnEnable()
    {
        auth = FirebaseAuth.DefaultInstance;
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        GetCurrentUser();
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
            GetPlayersInLobby();
        });
    }

    void GetPlayersInLobby()
    {
        dbReference.Child("lobbies").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to load lobby data: " + task.Exception);
                return;
            }

            if (!task.Result.Exists)
            {
                Debug.LogError("No lobbies found!");
                return;
            }

            foreach (var lobby in task.Result.Children)
            {
                string lobbyID = lobby.Key;
                string host = lobby.Child("Host").Value.ToString();
                string guest = lobby.Child("Guest").Value != null ? lobby.Child("Guest").Value.ToString() : "Waiting for player...";

                if (host == currentUser || guest == currentUser)
                {
                    Debug.Log($"Found lobby: {lobbyID}");
                    hostPlayer.text = $"Host: {host}";
                    guestPlayer.text = $"Guest: {guest}";
                    return;
                }
            }

            Debug.LogError("User is not in any lobby.");
        });
    }
}
