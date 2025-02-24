using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Lobbies : MonoBehaviour
{
    [SerializeField] public GameObject lobbyObject;
    [SerializeField] public GameObject menuObject;

    private FirebaseAuth auth;
    private DatabaseReference dbReference;

    private Lobbies instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        lobbyObject.SetActive(false);
        menuObject.SetActive(true);
    }

    public void LobbyCanvasEnabler()
    {
        lobbyObject.SetActive(true);
        menuObject.SetActive(false);
    }

    public void CreateLobby()
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No user signed in. Cannot create a lobby.");
            return;
        }

        string userID = auth.CurrentUser.UserId;

        dbReference.Child("users").Child(userID).Child("userName").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError("Failed to get username: " + task.Exception);
                return;
            }

            if (!task.Result.Exists)
            {
                Debug.LogError("Username not found in database.");
                return;
            }

            string username = task.Result.Value.ToString();
            string lobbyID = GenerateLobbyID();

            dbReference.Child("lobbies").Child(lobbyID).GetValueAsync().ContinueWithOnMainThread(task2 =>
            {
                if (task2.Exception != null)
                {
                    Debug.LogError("Error checking existing lobby ID: " + task2.Exception);
                    return;
                }

                if (task2.Result.Exists)
                {
                    Debug.LogWarning("Lobby ID already exists! Generating a new one...");
                    CreateLobby();
                    return;
                }

                dbReference.Child("lobbies").Child(lobbyID).Child("Host").SetValueAsync(username).ContinueWithOnMainThread(task3 =>
                {
                    if (task3.Exception != null)
                    {
                        Debug.LogError("Failed to create lobby: " + task3.Exception);
                        return;
                    }

                    dbReference.Child("users").Child(userID).Child("lobbyCode").SetValueAsync(lobbyID).ContinueWithOnMainThread(task4 =>
                    {
                        if (task4.Exception != null)
                        {
                            Debug.LogError("Failed to save lobby code to user data: " + task4.Exception);
                            return;
                        }

                        Debug.Log($"Lobby created successfully with ID: {lobbyID} by {username}");
                        SceneManager.LoadScene("Lobby");
                    });
                });
            });
        });
    }

    public string GenerateLobbyID()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        System.Random random = new System.Random();
        return new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public void JoinLobby(string lobbyCode)
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No user signed in. Cannot join a lobby.");
            return;
        }

        string userID = auth.CurrentUser.UserId;

        dbReference.Child("users").Child(userID).Child("userName").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError("Failed to get username: " + task.Exception);
                return;
            }

            if (!task.Result.Exists)
            {
                Debug.LogError("Username not found in database.");
                return;
            }

            string username = task.Result.Value.ToString();

            dbReference.Child("lobbies").Child(lobbyCode).GetValueAsync().ContinueWithOnMainThread(task2 =>
            {
                if (task2.Exception != null)
                {
                    Debug.LogError("Error checking lobby: " + task2.Exception);
                    return;
                }

                if (!task2.Result.Exists)
                {
                    Debug.LogWarning("Lobby not found! Check the code and try again.");
                    return;
                }

                dbReference.Child("lobbies").Child(lobbyCode).Child("Guest").SetValueAsync(username).ContinueWithOnMainThread(task3 =>
                {
                    if (task3.Exception != null)
                    {
                        Debug.LogError("Failed to join lobby: " + task3.Exception);
                        return;
                    }

                    dbReference.Child("users").Child(userID).Child("lobbyCode").SetValueAsync(lobbyCode).ContinueWithOnMainThread(task4 =>
                    {
                        if (task4.Exception != null)
                        {
                            Debug.LogError("Failed to save lobby code to user data: " + task4.Exception);
                            return;
                        }

                        Debug.Log($"Joined lobby {lobbyCode}. Host: {task2.Result.Child("Host").Value}");
                        SceneManager.LoadScene("Lobby");
                    });
                });
            });
        });
    }
}
