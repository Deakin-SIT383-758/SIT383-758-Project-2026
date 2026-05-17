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

    // Returns the hazard total for the active runway.
    public int GetHazardCount()
    {
        hazards = Object.FindObjectsByType<Hazard>(FindObjectsSortMode.None);
        return hazards.Length;
    }

    // Instantiates hazards for the specified runway and sets their color based on severity
    public void LoadHazards(string runwayID, Transform runwayTransform)
    {
        Debug.Log("Loading hazards for: " + runwayID);

        // Get ALL hazard components that are children of this runway ONLY
        Hazard[] hazards = runwayTransform.GetComponentsInChildren<Hazard>();

        foreach (Hazard h in hazards)
        {
            if (h == null) continue;

            // Add visual prefab to each hazard point
            Transform hazardParent = runwayTransform.Find("HazardContainer");

            if (hazardParent == null)
            {
                Debug.LogError("HazardContainer not found!");
                return;
            }

            GameObject obj = Instantiate(hazardPrefab, hazardParent);

            obj.transform.SetParent(h.transform); 
            obj.transform.localPosition = Vector3.zero;

            HazardObject hazardObj = obj.GetComponent<HazardObject>();

            if (hazardObj != null)
            {
                hazardObj.Initialise(h); 
            }
            else
            {
                Debug.LogWarning("HazardObject component missing!");
            }
        }
    }
}
           



