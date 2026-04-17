using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class HUDManager : MonoBehaviour
{
    public TextMeshProUGUI hazardDisplay;
    public TextMeshProUGUI runwayName;
    public List<string> runwayNames;
    MetadataManager metaManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Updates the HUD with the current RunwayID and the number of Hazards on that runway.
    public void UpdateHUD(string runwayID, int hazardCount)
    {
        foreach (RunwayData data in metaManager.runwayDatabase)
        {
            if (data.runwayID == runwayID)
            {
                runwayName.text = "Current Runway is: " + runwayID;
                hazardDisplay.text = "Hazards: " + hazardCount;
            }
        }
    }
}
