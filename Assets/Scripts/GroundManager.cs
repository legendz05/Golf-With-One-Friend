using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundManager : MonoBehaviour
{
    public static List<GameObject> currentRoundObjects = new List<GameObject>();

    public void ResetWorld()
    {
        Debug.Log("Reset World");
        foreach (var item in currentRoundObjects)
        {
            Destroy(item);
        }
        currentRoundObjects.Clear();
    }
}
