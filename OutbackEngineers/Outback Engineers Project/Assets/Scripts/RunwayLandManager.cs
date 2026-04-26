using UnityEngine;
using System.Collections.Generic;

public class RunwayLandManager : MonoBehaviour
{
    [Header("Runway Prefabs")]
    public GameObject RunwayA;
    public GameObject RunwayB;

    [Header("Spawn Settings")]
    public float spawnDistance = 10f;
    public string defaultRunwayID = "Runway_A";

    public Transform playerTransform;
    private GameObject currentRunway;
    private Dictionary<string, GameObject> RunwayDict;

    void Awake()
    {
        Transform cam = playerTransform;

        RunwayDict = new Dictionary<string, GameObject>()
        {
            { "Runway_A", RunwayA },
            { "Runway_B", RunwayB }
        };

        //if (!string.IsNullOrEmpty(defaultRunwayID))
        //{
          //  LoadRunway(defaultRunwayID);
        //}
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            LoadRunway("Runway_A");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            LoadRunway("Runway_B");
        }
    }

    //Loads the latest version of a runway
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

    //Positions a loaded runway at the correct position
    void PositionRunway(GameObject runway)
    {
        if (playerTransform == null)
        {
            Debug.LogWarning("Player Transform not assigned.");
            return;
        }

        Transform cam = playerTransform;

        Vector3 forward = cam.forward;
        forward.y = 0f;

        if (forward == Vector3.zero)
        {
            forward = Vector3.forward; // fallback safety
        }

        forward.Normalize();

        Vector3 spawnPosition = cam.position + forward * spawnDistance;
        spawnPosition.y = Mathf.Max(0f, cam.position.y - 1.5f); //Keep runway near "ground level" relative to player height

        runway.transform.position = spawnPosition;
        runway.transform.rotation = Quaternion.LookRotation(forward);
    }

    public GameObject GetCurrentRunway()
    {
        return currentRunway;
    }

    //Loads an older version of a runway
    public void LoadRetroRunway(GameObject RetroRunway)
    {
        if (currentRunway != null)
        {
            Destroy(currentRunway);
        }
        currentRunway = Instantiate(RetroRunway);
        PositionRunway(currentRunway);
    }
}