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
            if (h == null) continue;

            Transform hazardParent = runwayTransform.Find("HazardContainer"); // Ensure there is a child object named "HazardContainer" under the runway in the hierarchy to serve as the parent for hazards
            GameObject obj = Instantiate(hazardPrefab, hazardParent);

            // Use LOCAL position (relative to runway)
            obj.transform.localPosition = h.position;

            obj.transform.localScale = Vector3.one;

            // Apply data to component
            HazardObject hazardObj = obj.GetComponent<HazardObject>();

            if (hazardObj != null)
            {
                hazardObj.SetSeverity(h.severity);
            }
            else
            {
                Debug.LogWarning("HazardObject component missing!");
            }

            Debug.Log("Spawning hazard at LOCAL position: " + h.position);
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
