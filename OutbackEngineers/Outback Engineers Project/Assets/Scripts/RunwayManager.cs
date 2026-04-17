using UnityEngine;

public class RunwayManager : MonoBehaviour
{
    public string currentRunwayID;

    public HazardManager hazardManager; // Reference to the HazardManager to load hazards for the detected runway
    public MetadataManager metadataManager; // Reference to the MetadataManager to display runway metadata
    //RunwayLoader runwayLoader - Teshan
    //HUDManager hudManager - Lochlan 


    void Start()
    {
        DetectRunway();
        //runwayLoader.LoadRunway(currentRunwayID); - teshan contribution

        hazardManager.LoadHazards(currentRunwayID); // Load hazards for the detected runway
        metadataManager.DisplayMetadata(currentRunwayID); // Display metadata for the detected runway

        //int hazardCount = hazardManager.GetHazardCount(currentRunwayID); Must implement this line for hudManager to update 
        //hudManager.UpdateHUD(currentRunwayID, hazardCount); - lochlan contribution
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
