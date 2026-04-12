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

        currentRunwayID = "Runway_A"; // Will update based of company data specifications

        Debug.Log("Current Runway Detected: " + currentRunwayID); // Ensure Runway Detection is working correctly
    }

    public string GetRunwayID()
    {
        return currentRunwayID;
    }



}
