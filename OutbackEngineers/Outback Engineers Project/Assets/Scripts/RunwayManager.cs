using UnityEngine;

public class RunwayManager : MonoBehaviour
{
    public string currentRunwayID;
    int activeInstance;
    GameObject[] currentTimeline;

    public HazardManager hazardManager; // Reference to the HazardManager to load hazards for the detected runway
    public MetadataManager metadataManager; // Reference to the MetadataManager to display runway metadata
    public RunwayLandManager runwaylandManager; // Reference to the RunwayLandManager to load runway terrain and objects for the detected runway
    public HUDManager hudManager; // Reference to the HUDManager to update the HUD with hazard count and runway information



    void Start()
    {
        DetectRunway();

        runwaylandManager.LoadRunway(currentRunwayID); // Load runway terrain and objects for the detected runway

        GameObject runwayObj = runwaylandManager.GetCurrentRunway(); // Load hazards for the detected runway
            if (runwayObj != null)
            {
                hazardManager.LoadHazards(currentRunwayID, runwayObj.transform);
            }
            else
            {
                Debug.LogError("Runway object is NULL � cannot spawn hazards!");
            }

        metadataManager.DisplayMetadata(currentRunwayID); // Display metadata for the detected runway

        int hazardCount = hazardManager.GetHazardCount();
        hudManager.UpdateHUD(currentRunwayID, hazardCount); //Displays hazard count and other data to the HUD
        currentTimeline = hudManager.SetRunwayTimeline(currentRunwayID); //Updates the timeline slider with the values of the timeline of the current runway and stores that array
    }

    // Update is called once per frame
    void Update()
    {
        metadataManager.DisplayMetadata(currentRunwayID); //Updates metadata Display for current runway

        int hazardCount = hazardManager.GetHazardCount(); 
        hudManager.UpdateHUD(currentRunwayID, hazardCount); //Updates the display of current hazard count and other data
        
        foreach (RunwayData data in metadataManager.runwayDatabase)
        {
            if (data.runwayID == currentRunwayID)
            {
                activeInstance = data.RunwayInstance;
            }
        }
        if ((int)hudManager.timeline.value != activeInstance)
        {
            hudManager.GetRunwayInstance(currentTimeline);
            runwaylandManager.LoadRetroRunway(currentTimeline[(int)hudManager.timeline.value]);
        }
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
