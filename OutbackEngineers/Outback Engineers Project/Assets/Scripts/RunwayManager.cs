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
        metadataManager.DisplayMetadata(currentRunwayID);

        // Update HUD continuously
        hazardCount = hazardManager.GetHazardCount();
        hudManager.UpdateHUD(currentRunwayID, hazardCount);

        // Get active runway timeline instance
        foreach (RunwayData data in metadataManager.runwayDatabase)
        {
            if (data.runwayID == currentRunwayID)
            {
                activeInstance = data.RunwayInstance;
            }
        }

        // Timeline interaction
        if ((int)hudManager.timeline.value != activeInstance)
        {
            hudManager.GetRunwayInstance(currentTimeline);

            // Future retro runway loading support
            // runwaySpawner.LoadRetroRunway(currentTimeline[(int)hudManager.timeline.value]);
        }
    }

    void LoadRunwaySystems()
    {
        DetectRunway();

        int runwayIndex = GetRunwayIndex(currentRunwayID);

        runwaySpawner.SpawnRunway(runwayIndex);

        GameObject runwayObj = runwaySpawner.GetCurrentRunway();

        if (runwayObj != null)
        {
            hazardManager.LoadHazards(currentRunwayID, runwayObj.transform);
        }

        metadataManager.DisplayMetadata(currentRunwayID);

        hazardCount = hazardManager.GetHazardCount();

        hudManager.UpdateHUD(currentRunwayID, hazardCount);

        currentTimeline = hudManager.SetRunwayTimeline(currentRunwayID);
    }

    int GetRunwayIndex(string runwayID)
    {
        switch (runwayID)
        {
            case "City_Runway":
                return 0;

            case "DryLand_Runway":
                return 1;

            case "Grass_Runway":
                return 2;

            case "Marsh_Runway":
                return 3;

            case "RedSand_Runway":
                return 4;

            default:
                Debug.LogWarning("Unknown runway ID: " + runwayID);
                return 0;
        }
    }

    public string GetRunwayID()
    {
        return currentRunwayID;
    }
}
