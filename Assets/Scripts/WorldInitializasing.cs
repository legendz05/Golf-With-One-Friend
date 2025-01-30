using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldInitializasing : MonoBehaviour
{
    public float xOffset;

    private bool hasSpawnedNextSection = false;
    [SerializeField] private GameObject groundPrefab;

    private int rndXOffset;

    [SerializeField] private List<GameObject> obstacles = new List<GameObject>();

    void Awake()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !hasSpawnedNextSection)
        {
            hasSpawnedNextSection = true;
            GroundManager.currentRoundObjects.Add(Instantiate(groundPrefab, new Vector2(groundPrefab.transform.position.x + xOffset, groundPrefab.transform.position.y), Quaternion.identity));


            for (int i = 0; i < 3; i++)
            {
                rndXOffset = Random.Range(-100, 100);
                GroundManager.currentRoundObjects.Add(Instantiate(obstacles[Random.Range(0, obstacles.Count)], new Vector2(groundPrefab.transform.position.x + rndXOffset, groundPrefab.transform.position.y + groundPrefab.transform.localScale.y / 2), Quaternion.identity));
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            hasSpawnedNextSection = false;
        }
    }
}
