using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    //UI Elements used in the script
    public TextMeshProUGUI hazardDisplay;
    public TextMeshProUGUI runwayName;
    public TextMeshProUGUI dateUpdated;
    public Slider timeline;
    public TextMeshProUGUI hazardInfo; // Phase 3: Adding Detailed Hazard Information to the HUD for enhanced player awareness

    //Reference to the MetadataManager to access runway Metadata
    public MetadataManager metaManager;

    //A set of arrays that contain all prefabs each Runway has had over time
    public GameObject[] cityTimeline;
    public GameObject[] dryLandTimeline;
    public GameObject[] grassTimeline;
    public GameObject[] marshTimeline;
    public GameObject[] redSandTimeline;

    //A dictionary that uses the RunwayID as the reference and the arrays containing the timeline of each runway as the definition
    Dictionary<string, GameObject[]> Timelines;

    void Awake()
    {
        Timelines = new Dictionary<string, GameObject[]>()
        {
            { "City_Runway", cityTimeline },
            { "DryLand_Runway", dryLandTimeline },
            { "Grass_Runway", grassTimeline },
            { "Marsh_Runway", marshTimeline },
            { "RedSand_Runway", redSandTimeline }
        };
    }

    //Updates the HUD with the current RunwayID and the number of Hazards on that runway.
    public void UpdateHUD(string runwayID, int hazardCount)
    {
        foreach (RunwayData data in metaManager.runwayDatabase)
        {
            if (data.runwayID == runwayID)
            {
                runwayName.text = data.runwayName + " Runway";
                hazardDisplay.text = "Hazards: " + hazardCount;
                dateUpdated.text = "Version: " + data.RunwayInstance;
            }
        }
    }

    //Sets the timeline slider with the values for the timeline of the active runway
    public GameObject[] SetRunwayTimeline(string runwayID)
    {
        GameObject[] currentTimeline = Timelines[runwayID];
        timeline.maxValue = currentTimeline.Length;
        return currentTimeline;
    }

    //Gets the current value of the slider and returns the chosen instance
    public GameObject GetRunwayInstance(GameObject[] ChosenTimeline)
    {
        int newRunwayInstance = (int)timeline.value;
        return ChosenTimeline[newRunwayInstance];
    }

    // Displays detailed information about specific hazards
    public void ShowHazardInfo(string type, int severity)
    {
        hazardInfo.text = type + " | Severity: " + severity;
    }

    public void ClearHazardInfo() // Clears the hazard information from the HUD when no hazard is targeted.
    {
        hazardInfo.text = "";
    }
}
