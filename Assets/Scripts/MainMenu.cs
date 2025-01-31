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

    void OnLevelWasLoaded()
    {
        FirebaseTest.instance.LoadUsernameFromFirebase((username) =>
        {
            Debug.Log("Username in New Scene: " + username);
            usernameText.text = username.ToString();

        });

    }

    // Update is called once per frame
    void Update()
    {

    }

}
