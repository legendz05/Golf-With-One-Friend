using UnityEngine;

public class Background : MonoBehaviour
{
    public GameObject backgroundPrefab;
    public GameObject backgroundSpawn;

    void Start()
    {
        InvokeRepeating(nameof(SpawnBackgroundElement), 1, 10);
    }

    // Update is called once per frame


    void SpawnBackgroundElement()
    {
        Instantiate(backgroundPrefab, backgroundSpawn.transform.position, Quaternion.identity);
    }
}
