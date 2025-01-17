using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    private GolfBall golfBall;
    private GameObject spawnedPowerUp;
    private Rigidbody2D rb2D;
    private int randomPowerUp;
    [SerializeField] private List<Animator> powerupTints = new List<Animator>();

    public List<GameObject> powerUps = new List<GameObject>();

    public static PowerUpManager instance = null;

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
        golfBall = FindAnyObjectByType<GolfBall>();
        rb2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.matchState == MatchState.GolfBallInAir)
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    Vector3 touchPos = Camera.main.ScreenToWorldPoint(touch.position);

                    if (touchPos.magnitude - spawnedPowerUp.transform.position.magnitude < 1 && spawnedPowerUp != null)
                    {
                        Debug.Log("Touch detected in PowerUpManager");
                        ForcePowerUp();
                        Destroy(spawnedPowerUp);
                    }
                }
            }
        }
    }

    public void ForcePowerUp()
    {
        if (golfBall != null)
        {
            Debug.Log("Force POWERUP");
            golfBall.GetComponent<Rigidbody2D>().AddForce(new Vector2(15, 40), ForceMode2D.Impulse);
            powerupTints[0].Play("RedTint");
        }
        else
        {
            Debug.Log("null is golfBall");
        }
    }

    public IEnumerator PowerUpTimer()
    {
        Camera mainCamera = Camera.main;

        while (GameManager.instance.matchState == MatchState.GolfBallInAir)
        {
            yield return new WaitForSeconds(3);

            Vector3 cameraPosition = mainCamera.transform.position;
            float cameraHeight = mainCamera.orthographicSize * 2f;
            float cameraWidth = cameraHeight * mainCamera.aspect;

            float minX = cameraPosition.x - cameraWidth / 2;
            float maxX = cameraPosition.x + cameraWidth / 2;
            float minY = cameraPosition.y - cameraHeight / 2;
            float maxY = cameraPosition.y + cameraHeight / 2;

            float spawnX = Random.Range(minX, maxX);
            float spawnY = Random.Range(minY, maxY);

            randomPowerUp = Random.Range(0, powerUps.Count);

            spawnedPowerUp = Instantiate(powerUps[randomPowerUp], new Vector2(spawnX, spawnY), Quaternion.identity);
            spawnedPowerUp.transform.SetParent(mainCamera.transform);

            yield return new WaitForSeconds(1f);
            Destroy(spawnedPowerUp);
        }
    }



}
