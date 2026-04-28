using UnityEngine;
using System.Collections.Generic;

public class HazardManager : MonoBehaviour
{
    //Accessing the Meta Data Manager
    public MetadataManager metaManager;

    // Prefab used to visually represent a hazard in the scene
    public GameObject hazardPrefab;

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

        //Locates each of the hazard empties stored in the runway prefab, and stores them in an array
        hazards = Object.FindObjectsByType<Hazard>(FindObjectsSortMode.None);

        foreach (Hazard h in hazards)
        {
            if (h == null) continue; // Skip null hazards to prevent errors

            Vector3 worldPos = runwayTransform.TransformPoint(h.position);

            GameObject obj = Instantiate(hazardPrefab, worldPos, Quaternion.identity);
            obj.transform.SetParent(runwayTransform);

            Renderer rend = obj.GetComponentInChildren<Renderer>();

            if (rend != null)
            {
                if (h.severity == 3)
                    rend.material.color = Color.red;
                else if (h.severity == 2)
                    rend.material.color = Color.yellow;
                else
                    rend.material.color = Color.green;
            }
            else
            {
                Debug.LogWarning("No Renderer found on hazard prefab!");
            }

            Debug.Log("Spawning hazard at: " + worldPos); // Debug log to verify hazard positions are being calculated and hazards are spawning correctly
        }

    }

    // Returns the hazard total for the active runway.
    public int GetHazardCount()
    {
        hazards = Object.FindObjectsByType<Hazard>(FindObjectsSortMode.None);
        return hazards.Length;
    }

}
