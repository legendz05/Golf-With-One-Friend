using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldInitializasing : MonoBehaviour
{
    public float xOffset;

    private bool hasSpawnedNextSection = false;
    [SerializeField] private GameObject groundPrefab;


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
            Instantiate(groundPrefab, new Vector2(groundPrefab.transform.position.x + xOffset, groundPrefab.transform.position.y), Quaternion.identity);
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
