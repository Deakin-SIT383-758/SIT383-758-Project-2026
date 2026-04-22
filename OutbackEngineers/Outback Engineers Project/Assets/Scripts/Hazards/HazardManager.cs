using UnityEngine;
using System.Collections.Generic;

public class HazardManager : MonoBehaviour
{
    //Accessing the Meta Data Manager
    public MetadataManager metaManager;

    // Prefab used to visually represent a hazard in the scene
    public GameObject hazardPrefab;

    // List of hazards for the associated runway
    public List<Hazard> runwayAHazards;
    public List<Hazard> runwayBHazards;

    //Array to dynamically store all the hazards in the scene
    Hazard[] hazards;

    // Instantiates hazards for the specified runway and sets their color based on severity
    public void LoadHazards(string runwayID, Transform runwayTransform)
    {
        // Debug logs to verify that the correct runway ID is being passed and that the hazard prefab and runway transform are not null
        Debug.Log("HazardPrefab: " + hazardPrefab);
        Debug.Log("RunwayTransform: " + runwayTransform);

        // Clear existing hazards before spawning new ones
        GameObject[] existingHazards = GameObject.FindGameObjectsWithTag("Hazard");

        foreach (GameObject obj in existingHazards)
        {
            Destroy(obj);
        }

        List<Hazard> selectedHazards = new List<Hazard>();

        if (runwayID == "Runway_A")
            selectedHazards = runwayAHazards;
        else if (runwayID == "Runway_B")
            selectedHazards = runwayBHazards;

        foreach (Hazard h in selectedHazards)
        {
            if (h == null) continue; // Skip null hazards to prevent errors

            Vector3 worldPos = runwayTransform.TransformPoint(h.position);

            GameObject obj = Instantiate(hazardPrefab, worldPos, Quaternion.identity);
            obj.transform.SetParent(runwayTransform);

            HazardObject hazardObj = obj.GetComponent<HazardObject>(); // Phase 2: Get the HazardObject component to set the severity and update the visual representation based on severity

            if (hazardObj != null)
            {
                hazardObj.SetSeverity(h.severity);
            }
            else
            {
                Debug.LogWarning("HazardObject component missing!");
            }

            Debug.Log("Spawning hazard at: " + worldPos); // Debug log to verify hazard positions are being calculated and hazards are spawning correctly
        }

    }

    // Returns the hazard total for each runway
    public int GetHazardCount(string runwayID)
    {
        if (runwayID == "Runway_A")
            return runwayAHazards.Count;
        else if (runwayID == "Runway_B")
            return runwayBHazards.Count;

        return 0;
    }

}
