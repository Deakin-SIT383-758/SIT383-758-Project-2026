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
    public string datestamp;
}


// Stores and displays metadata about the runway, such as location and time of last update
public class MetadataManager : MonoBehaviour
{
    public TextMeshProUGUI runwayText;
    public TextMeshProUGUI locationText;
    public TextMeshProUGUI timeText; // Use this for both date and time

    public List<RunwayData> runwayDatabase;

    public void DisplayMetadata(string runwayID, int instance)
    {
        foreach (RunwayData data in runwayDatabase)
        {
            if (data.runwayID == runwayID && data.RunwayInstance == instance)
            {
                runwayText.text = "Runway: " + data.runwayID;
                locationText.text = "Lat/Lon: " + data.latitude + ", " + data.longitude;

                // Try to parse and format date/time for better readability
                if (System.DateTime.TryParse($"{data.datestamp} {data.timestamp}", out var dt))
                {
                    // Example: "Updated: 14 May 2026, 15:30"
                    timeText.text = $"Updated: {dt:dd MMM yyyy, HH:mm}";
                }
                else
                {
                    // Fallback if parsing fails
                    timeText.text = $"Updated: {data.datestamp} {data.timestamp}";
                }
            }
        }
    }
}
