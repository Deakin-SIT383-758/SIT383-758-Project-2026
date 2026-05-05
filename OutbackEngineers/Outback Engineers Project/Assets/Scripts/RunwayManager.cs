using UnityEngine;
using UnityEngine.SceneManagement;

public class RunwayManager : MonoBehaviour
{
    public string currentRunwayID;
    int activeInstance;
    GameObject[] currentTimeline;
    Scene scene;
    int hazardCount;

    public HazardManager hazardManager; // Reference to the HazardManager to load hazards for the detected runway
    public MetadataManager metadataManager; // Reference to the MetadataManager to display runway metadata
    public RunwayLandManager runwaylandManager; // Reference to the RunwayLandManager to load runway terrain and objects for the detected runway
    public HUDManager hudManager; // Reference to the HUDManager to update the HUD with hazard count and runway information
    public MenuManager menuManager; //Reference to the MenuManager to see when scenes are changed

    // Update is called once per frame
    void Update()
    {
        scene = SceneManager.GetActiveScene();
        if (scene.name == "MainScene" && menuManager.SceneChanged == true)
        {
            hazardManager = Object.FindAnyObjectByType<HazardManager>();
            metadataManager = Object.FindAnyObjectByType<MetadataManager>();
            runwaylandManager = Object.FindAnyObjectByType<RunwayLandManager>();
            hudManager = Object.FindAnyObjectByType<HUDManager>();

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

            hazardCount = hazardManager.GetHazardCount();
            hudManager.UpdateHUD(currentRunwayID, hazardCount); //Displays hazard count and other data to the HUD
            currentTimeline = hudManager.SetRunwayTimeline(currentRunwayID); //Updates the timeline slider with the values of the timeline of the current runway and stores that array
            
            menuManager.SceneChanged = false;
        }
        else if (scene.name == "MainScene")
        {
            metadataManager.DisplayMetadata(currentRunwayID); //Updates metadata Display for current runway

            hazardCount = hazardManager.GetHazardCount(); 
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
    }

    void DetectRunway()
    {
        currentRunwayID = menuManager.ChosenScene;
    }

    public string GetRunwayID()
    {
        return currentRunwayID;
    }


}
