using UnityEngine;
using System.Collections.Generic;

public class RunwayManager : MonoBehaviour
{
    [Header("Runway Prefabs")]
    public GameObject RunwayA;
    public GameObject RunwayB;

    [Header("Spawn Settings")]
    public float spawnDistance = 10f;
    public string defaultRunwayID = "A";

    private GameObject currentRunway;
    private Dictionary<string, GameObject> RunwayDict;

    void Start()
    {
        RunwayDict = new Dictionary<string, GameObject>()
        {
            { "A", RunwayA },
            { "B", RunwayB }
        };

        if (!string.IsNullOrEmpty(defaultRunwayID))
        {
            LoadRunway(defaultRunwayID);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            LoadRunway("A");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            LoadRunway("B");
        }
    }
    public void LoadRunway(string runwayID)
    {
        if (currentRunway != null)
        {
            Destroy(currentRunway);
        }

        if (RunwayDict.ContainsKey(runwayID) && RunwayDict[runwayID] != null)
        {
            currentRunway = Instantiate(RunwayDict[runwayID]);
            PositionRunway(currentRunway);
            Debug.Log("Loaded runway: " + runwayID);
        }
        else
        {
            Debug.LogWarning("Runway not found or prefab missing for ID: " + runwayID);
        }
    }

    void PositionRunway(GameObject runway)
    {
        if (Camera.main == null)
        {
            Debug.LogWarning("No Main Camera found in the scene.");
            return;
        }

        Transform cam = Camera.main.transform;

        Vector3 forward = cam.forward;
        forward.y = 0f;
        forward.Normalize();

        runway.transform.position = cam.position + forward * spawnDistance;
        runway.transform.rotation = Quaternion.LookRotation(forward);
    }
}