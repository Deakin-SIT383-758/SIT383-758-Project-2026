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

    //Reference to the MetadataManager to access runway Metadata
    public MetadataManager metaManager;

    //A set of arrays that contain all prefabs each Runway has had over time
    public GameObject[] RunwayA_Timeline;
    public GameObject[] RunwayB_Timeline;

    //A dictionary that uses the RunwayID as the reference and the arrays containing the timeline of each runway as the definition
    Dictionary<string, GameObject[]> Timelines;

    void Awake()
    {
        Timelines = new Dictionary<string, GameObject[]>()
        {
            { "Runway_A", RunwayA_Timeline },
            { "Runway_B", RunwayB_Timeline }
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
}
