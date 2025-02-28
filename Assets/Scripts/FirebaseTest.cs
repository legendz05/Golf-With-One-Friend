using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using NUnit.Framework;
using System.Text.RegularExpressions;
using System;

public class FirebaseTest : MonoBehaviour
{
    public static FirebaseTest instance;

    private FirebaseAuth auth;
    private DatabaseReference db;

    private string emailVar, passwordVar, usernameVar;

    [SerializeField] private TMP_InputField emailText;
    [SerializeField] private TMP_InputField passwordText;
    [SerializeField] private TMP_InputField usernameText;

    [SerializeField] private Button registerButton;
    [SerializeField] private Button signInButton;
    [SerializeField] private Button signOutButton;

    [SerializeField] private TextMeshProUGUI errorMessage;

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

        usernameText.gameObject.SetActive(false);
    }

    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError(task.Exception);
                return;
            }

            auth = FirebaseAuth.DefaultInstance;
            db = FirebaseDatabase.DefaultInstance.RootReference;
        });
    }

    // ------------------------ User Authentication ------------------------

    public void RegisterUser()
    {
        if (string.IsNullOrEmpty(emailVar) || string.IsNullOrEmpty(passwordVar))
        {
            Debug.LogWarning("Email or password is empty.");
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(emailVar, passwordVar).ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                GetErrorMessage(task.Exception);
                return;
            }

            FirebaseUser newUser = task.Result.User;
            Debug.Log($"User Registered: {newUser.Email} ({newUser.UserId})");

        });
    }

    public void SignInUser()
    {
        if (string.IsNullOrEmpty(emailVar) || string.IsNullOrEmpty(passwordVar))
        {
            Debug.LogWarning("Email or password is empty.");
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(emailVar, passwordVar).ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                GetErrorMessage(task.Exception);
                return;
            }

            FirebaseUser newUser = task.Result.User;
            Debug.Log($"User signed in: {newUser.Email} ({newUser.UserId})");

            LoadUsernameFromFirebase(username =>
            {
                if (!string.IsNullOrEmpty(username) && username != "Guest")
                {
                    LoadMainMenu();
                }
                else
                {
                    usernameText.gameObject.SetActive(true);
                    emailText.gameObject.SetActive(false);
                    passwordText.gameObject.SetActive(false);

                    registerButton.gameObject.SetActive(false);
                    signInButton.gameObject.SetActive(false);
                    signOutButton.gameObject.SetActive(false);
                }
            });
        });
    }

    public void SignOutUser()
    {
        auth.SignOut();
        Debug.Log("User signed out.");
    }

    // ------------------------ Username Management ------------------------

    public void SaveUsername()
    {
        if (string.IsNullOrEmpty(usernameVar))
        {
            Debug.LogWarning("Username cannot be empty.");
            return;
        }

        string userID = auth.CurrentUser?.UserId;
        if (userID == null)
        {
            Debug.LogError("No user signed in.");
            return;
        }

        db.Child("usernames").Child(usernameVar).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result.Exists)
            {
                Debug.LogWarning("Username already taken. Choose another.");
                return;
            }

            db.Child("users").Child(userID).Child("userName").SetValueAsync(usernameVar).ContinueWithOnMainThread(task1 =>
            {
                if (task1.Exception != null)
                {
                    Debug.LogWarning(task1.Exception);
                    return;
                }

                Debug.Log("Username saved under user ID.");

                db.Child("usernames").Child(usernameVar).SetValueAsync(userID).ContinueWithOnMainThread(task2 =>
                {
                    if (task2.Exception == null)
                    {
                        Debug.Log("Username stored globally.");
                        LoadMainMenu();
                    }
                    else
                    {
                        Debug.LogWarning(task2.Exception);
                    }
                });
            });
        });
    }

    public void LoadUsernameFromFirebase(System.Action<string> callback)
    {
        string userID = auth.CurrentUser?.UserId;
        if (userID == null)
        {
            Debug.LogError("No user signed in.");
            callback?.Invoke("Guest");
            return;
        }

        db.Child("users").Child(userID).Child("userName").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result.Exists)
            {
                string username = task.Result.Value.ToString();
                usernameVar = username;
                Debug.Log($"Loaded Username: {username}");
                callback?.Invoke(username);
            }
            else
            {
                Debug.LogWarning("Username not found in database.");
                callback?.Invoke("Guest");
            }
        });
    }

    //------------------------- Error Messages  ------------------------

    public void GetErrorMessage(Exception exception)
    {
        Debug.Log($"Exception Type: {exception.GetType()} - Message: {exception.Message}");

        Firebase.FirebaseException firebaseEx = null;

        if (exception is AggregateException aggregateException)
        {
            foreach (var innerException in aggregateException.InnerExceptions)
            {
                if (innerException is Firebase.FirebaseException innerFirebaseEx)
                {
                    firebaseEx = innerFirebaseEx;
                    break; // Exit loop once we find the Firebase exception
                }
            }
        }
        else if (exception is Firebase.FirebaseException directFirebaseEx)
        {
            firebaseEx = directFirebaseEx;
        }

        string message = "An unknown error occurred.";

        if (firebaseEx != null)
        {
            var errorCode = (AuthError)firebaseEx.ErrorCode;
            message = GetErrorMessage(errorCode);
        }
        else
        {
            message = exception.Message;
        }

        Debug.LogError($"Final Error Message: {message}");

        if (errorMessage != null)
        {
            errorMessage.text = message;
            errorMessage.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("errorMessage is NULL! Make sure it is assigned in the Unity Inspector.");
        }
    }



    private string GetErrorMessage(AuthError errorCode)
    {
        var message = "";
        switch (errorCode)
        {
            case AuthError.AccountExistsWithDifferentCredentials:
                message = "Account exists!";
                break;
            case AuthError.MissingPassword:
                message = "Missing password!";
                break;
            case AuthError.WeakPassword:
                message = "Weak password!";
                break;
            case AuthError.WrongPassword:
                message = "Wrong password!";
                break;
            case AuthError.EmailAlreadyInUse:
                message = "Email already in use!";
                break;
            case AuthError.InvalidEmail:
                message = "Email invalid!";
                break;
            case AuthError.MissingEmail:
                message = "Email missing!";
                break;
            default:
                message = "Error!";
                break;
        }
        errorMessage.text = message;
        return message;
    }

    // ------------------------ UI & Scene Management ------------------------

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void UpdateEmail(string email) => emailVar = email;
    public void UpdatePassword(string password) => passwordVar = password;
    public void UpdateUsername(string username) => usernameVar = username;
}
