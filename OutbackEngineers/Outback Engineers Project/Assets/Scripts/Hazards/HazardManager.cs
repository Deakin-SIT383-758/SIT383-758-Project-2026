using UnityEngine;
using System.Collections.Generic;

public class HazardManager : MonoBehaviour
{
    //Accessing the Meta Data Manager
    MetadataManager metaManager;

    // Prefab used to visually represent a hazard in the scene
    public GameObject hazardPrefab;

    // List of hazards for the associated runway
    public List<Hazard> runwayAHazards;
    public List<Hazard> runwayBHazards;

    //Array to dynamically store all the hazards in the scene
    Hazard[] hazards;

    // Instantiates hazards for the specified runway and sets their color based on severity
    public void LoadHazards(string runwayID)
    {
        // Selects appropriate hazard list based on runway ID
        List<Hazard> selectedHazards = new List<Hazard>();

        if (runwayID == "Runway_A")
            selectedHazards = runwayAHazards;
        else if (runwayID == "Runway_B")
            selectedHazards = runwayBHazards;

        // Instantiate each hazard and set its color according to severity
        foreach (Hazard h in selectedHazards)
        {
            // Create a hazard GameObject at the specified position
            GameObject obj = Instantiate(hazardPrefab, h.position, Quaternion.identity);

            // Get the Renderer to change the hazard's color
            Renderer rend = obj.GetComponent<Renderer>();

            // Get the Renderer to change the hazard's color, else green
            if (h.severity == 3)
                rend.material.color = Color.red;
            else if (h.severity == 2)
                rend.material.color = Color.yellow;
            else
                rend.material.color = Color.green;
        }
    }

    // Returns the hazard total for each runway
    public int GetHazardCount(string runwayID)
    {
        foreach (RunwayData data in metaManager.runwayDatabase)
        {
            if (data.runwayID == runwayID)
            {
                hazards = Object.FindObjectsByType<Hazard>(FindObjectsSortMode.None);                
            }
        }
        
        //Returns the count of hazards for the specified runway
        return hazards.Length;
    }
}