using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

public class FirebaseTest : MonoBehaviour
{
    FirebaseAuth auth;

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
                Debug.LogError(task.Exception);

            auth = FirebaseAuth.DefaultInstance;

            FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(false);

        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
            AnonymousSignIn();

        if (Input.GetKeyDown(KeyCode.D))
            DataTest(auth.CurrentUser.UserId, Random.Range(0, 100).ToString());

        if (Input.GetKeyDown(KeyCode.R))
            RegisterNewUser("isac@test.test", "password");

        if (Input.GetKeyDown(KeyCode.S))
            SignIn("isac@test.test", "password");

        if (Input.GetKeyDown(KeyCode.Q))
            SignOut();
    }

    private void AnonymousSignIn()
    {
        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogWarning(task.Exception);
            }
            else
            {
                FirebaseUser newUser = task.Result.User;
                Debug.LogFormat("User signed in successfully: {0} ({1})",
                    newUser.DisplayName, newUser.UserId);
            }
        });
    }

    private void DataTest(string userID, string data)
    {
        Debug.Log("Trying to write data...");
        var db = FirebaseDatabase.DefaultInstance;
        db.RootReference.Child("users").Child(userID).SetValueAsync(data).ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
                Debug.LogWarning(task.Exception);
            else
                Debug.Log("DataTestWrite: Complete");
        });
    }

    private void RegisterNewUser(string email, string password)
    {
        Debug.Log("Starting Registration");
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogWarning(task.Exception);
            }
            else
            {
                FirebaseUser newUser = task.Result.User;
                Debug.LogFormat("User Registerd: {0} ({1})",
                  newUser.DisplayName, newUser.UserId);
            }
        });
    }

    private void SignIn(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogWarning(task.Exception);
            }
            else
            {
                FirebaseUser newUser = task.Result.User;
                Debug.LogFormat("User signed in successfully: {0} ({1})",
                  newUser.DisplayName, newUser.UserId);
            }
        });
    }

    private void SignOut()
    {
        auth.SignOut();
        Debug.Log("User signed out");
    }

    private void SaveToFirebase(string data)
    {
        var db = FirebaseDatabase.DefaultInstance;
        var userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        //puts the JSON data in the "users/userId" part of the database.
        db.RootReference.Child("users").Child(userId).SetRawJsonValueAsync(data);
    }

    private void LoadFromFirebase()
    {
        var db = FirebaseDatabase.DefaultInstance;
        var userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        db.RootReference.Child("users").Child(userId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError(task.Exception);
            }

            //here we get the result from our database.
            DataSnapshot snap = task.Result;

            //And send the JSON data to a function that can update our game.
            //LoadState(snap.GetRawJsonValue());
        });
    }
}