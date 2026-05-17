using UnityEngine;

public class RunwayManager : MonoBehaviour
{
    public string currentRunwayID;

    int activeInstance;
    int hazardCount;

    GameObject[] currentTimeline;

    public HazardManager hazardManager;
    public MetadataManager metadataManager;
    public HUDManager hudManager;
    public RunwaySpawner runwaySpawner;

    private bool runwayLoaded = false;

    void Start()
    {
        // Safety checks
        if (hazardManager == null)
            hazardManager = Object.FindAnyObjectByType<HazardManager>();

        if (metadataManager == null)
            metadataManager = Object.FindAnyObjectByType<MetadataManager>();

        if (hudManager == null)
            hudManager = Object.FindAnyObjectByType<HUDManager>();

        if (runwaySpawner == null)
            runwaySpawner = Object.FindAnyObjectByType<RunwaySpawner>();

        LoadRunwaySystems();
    }

    void DetectRunway()
    {
        if (PersistanceScript.Instance == null)
        {
            Debug.LogError("PersistenceScript instance is NULL!");
            return;
        }

        currentRunwayID = PersistanceScript.Instance.selectedRunway;

        Debug.Log("Detected runway ID: " + currentRunwayID);
    }

    void Update()
    {
        // Prevent update logic before runway fully loads
        if (!runwayLoaded)
            return;

        // Update metadata continuously
        metadataManager.DisplayMetadata(currentRunwayID, activeInstance);

        // Update HUD continuously
        hazardCount = hazardManager.GetHazardCount();
        hudManager.UpdateHUD(currentRunwayID, hazardCount, activeInstance);
    }

    //Loads all the Runway Systems
    void LoadRunwaySystems()
    {
        DetectRunway();

        currentTimeline = hudManager.SetRunwayTimeline(currentRunwayID);

        activeInstance = currentTimeline.Length-1;

        runwaySpawner.LoadRunway(currentTimeline[activeInstance]);

        GameObject runwayObj = runwaySpawner.GetCurrentRunway();

        if (runwayObj != null)
        {
            hazardManager.LoadHazards(currentRunwayID, runwayObj.transform);
        }

        metadataManager.DisplayMetadata(currentRunwayID, activeInstance);

        hazardCount = hazardManager.GetHazardCount();

        hudManager.UpdateHUD(currentRunwayID, hazardCount, activeInstance);
        
        hudManager.timeline.value = activeInstance;
    }

    //Handles when the value of the timeline slider is changed
    public void HandleSliderValueChanged(float value)
    {
        // Future retro runway loading support
        Debug.Log("RetroRunway Loaded");

        activeInstance = (int)value;

        runwaySpawner.LoadRunway(currentTimeline[activeInstance]);

        GameObject runwayObj = runwaySpawner.GetCurrentRunway();

        if (runwayObj != null)
        {
            hazardManager.LoadHazards(currentRunwayID, runwayObj.transform);
        }

        metadataManager.DisplayMetadata(currentRunwayID, activeInstance);

        hazardCount = hazardManager.GetHazardCount();

        hudManager.UpdateHUD(currentRunwayID, hazardCount, activeInstance);
        
        hudManager.timeline.value = activeInstance;
    }
}
