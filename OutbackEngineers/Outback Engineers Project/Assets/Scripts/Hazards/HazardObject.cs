using UnityEngine;
using TMPro;


public class HazardObject : MonoBehaviour // Phase 3: Implementing Raycasting and Visual Feedback for Hazards for enhanced interactivity
{                                         // No longer just a data class, now also handles visual representation in the scene
    public int severity;

    public string hazardType;

    private Renderer rend;

    public GameObject overlayPrefab;

    private GameObject overlayInstance;

    private Color originalColor;

    void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
        CreateOverlay();
     
    }

    void Start()
    {
        if (rend != null)
            originalColor = rend.material.color;
    }

    public void Highlight() //Change hazards color to cyan when highlighted, providing visual feedback to the player
    {
        if (rend != null)
            rend.material.color = Color.cyan;
    }

    public void Unhighlight() 
    {
        UpdateVisual(); // restore correct color
    }


    public void Initialise(Hazard data) // Initializes the hazard object with data from the Hazard class, setting up its type and severity, and then updates its visual representation accordingly
    {
        hazardType = data.type;
        severity = data.severity;

        UpdateVisual();

        Debug.Log("Hazard Type: " + hazardType + " | Severity: " + severity);
    }

    public void SetSeverity(int value)
    {
        severity = value;
        UpdateVisual();
    }

    void CreateOverlay() // Creates a visual overlay (e.g., "!") above the hazard to indicate its severity level
    {
        if (overlayPrefab == null)
        {
            Debug.LogWarning("Overlay prefab not assigned!");
            return;
        }

        overlayInstance = Instantiate(overlayPrefab, transform);

        // Position above hazard
        overlayInstance.transform.localPosition = new Vector3(0, 1.5f, 0);
    }

    void UpdateVisual()
    {
        if (rend != null)
        {
            if (severity == 3)
                rend.material.color = Color.red;
            else if (severity == 2)
                rend.material.color = Color.yellow;
            else
                rend.material.color = Color.green;
        }

        if (overlayInstance != null)
        {
            TextMeshProUGUI text = overlayInstance.GetComponentInChildren<TextMeshProUGUI>();

            if (text != null)
            {
                // TYPE → SYMBOL
                switch (hazardType)
                {
                    case "Crack":
                        text.text = "#";
                        break;
                    case "Debris":
                        text.text = "X";
                        break;
                    case "Water":
                        text.text = "~";
                        break;
                    default:
                        text.text = "!";
                        break;
                }

                // SEVERITY → COLOUR
                if (severity == 3)
                    text.color = Color.red;
                else if (severity == 2)
                    text.color = Color.yellow;
                else
                    text.color = Color.green;
            }
        }
    }

    void LateUpdate() // Ensures the overlay always faces the camera, improving visibility regardless of player position or orientation
    {
        if (overlayInstance == null) return;

        Transform cam = Camera.main.transform; // Will later be updated to playerTransform for VR compatibility!

        overlayInstance.transform.LookAt(cam);

        overlayInstance.transform.Rotate(0, 180, 0);
    }
}