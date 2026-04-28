using UnityEngine;
using TMPro;
using TMPro.Examples;
using System.Collections.Generic;

[System.Serializable]
public class RunwayData
{
    public string runwayID;
    public string runwayName;
    public float latitude;
    public float longitude;
    public string timestamp;
    public int RunwayInstance;
}


// Stores and displays metadata about the runway, such as location and time of last update
public class MetadataManager : MonoBehaviour
{
    public TextMeshProUGUI runwayText;
    public TextMeshProUGUI locationText;
    public TextMeshProUGUI timeText;

    public List<RunwayData> runwayDatabase;

    public void DisplayMetadata(string runwayID)
    {
        foreach (RunwayData data in runwayDatabase)
        {
            if (data.runwayID == runwayID)
            {
                runwayText.text = "Runway: " + data.runwayID;
                locationText.text = "Lat/Lon: " + data.latitude + ", " + data.longitude;
                timeText.text = "Updated: " + data.timestamp;
            }
        }
    }
}