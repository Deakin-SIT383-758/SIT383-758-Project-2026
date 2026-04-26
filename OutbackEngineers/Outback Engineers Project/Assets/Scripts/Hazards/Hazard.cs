using UnityEngine;

[System.Serializable]
public class Hazard : MonoBehaviour 
{
    public int severity; // 1 = low, 2 = medium, 3 = high
    public string type; // e.g., "Bird Strike", "Foreign Object Debris", "Weather Hazard", etc.
}
