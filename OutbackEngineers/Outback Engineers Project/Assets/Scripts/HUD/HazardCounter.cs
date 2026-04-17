using UnityEngine;
using TMPro;

public class HazardCounter : MonoBehaviour
{
    public TextMeshProUGUI hazardCounter;
    Hazard[] hazards;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        hazards = Object.FindObjectsByType<Hazard>(FindObjectsSortMode.None);
        hazardCounter.text = "Hazards: " + hazards.Length;
    }
}
