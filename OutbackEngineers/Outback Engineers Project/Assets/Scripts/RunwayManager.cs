using UnityEngine;

public class RunwayManager : MonoBehaviour
{
    public string currentRunwayID;


    void Start()
    {
        DetectRunway();   
    }

    void DetectRunway()
    {
        // Phase 1: simulate runway detection with placeholder data

        int random = Random.Range(0, 2);

        if (random == 0)
            currentRunwayID = "Runway_A";
        else
            currentRunwayID = "Runway_B";

        Debug.Log("Current Runway Detected: " + currentRunwayID); // Ensure Runway Detection is working correctly, should switch between Runway_A and Runway_B randomly each time the game starts
    }

    public string GetRunwayID()
    {
        return currentRunwayID;
    }



}
