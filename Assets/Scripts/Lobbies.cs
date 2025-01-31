using UnityEngine;

public class Lobbies : MonoBehaviour
{

    [SerializeField] private GameObject lobbyObject;
    [SerializeField] private GameObject menuObject;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LobbyCanvasEnabler()
    {
        lobbyObject.SetActive(true);
        menuObject.SetActive(false);
    }
}
