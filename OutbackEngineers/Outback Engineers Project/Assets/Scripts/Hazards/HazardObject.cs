using UnityEngine;
using TMPro;


public class HazardObject : MonoBehaviour // Phase 3: Implementing Raycasting and Visual Feedback for Hazards for enhanced interactivity
{                                         // No longer just a data class, now also handles visual representation in the scene
    public int severity;

    public string hazardType;

    private Renderer[] renderers;

    private Vector3 originalScale;

    public GameObject overlayPrefab;

    private GameObject overlayInstance;


    public GameObject crackMesh; // Phase 3: Adding Distinct Visual Representations for Different Hazard Types to improve player recognition and immersion
    public GameObject waterMesh;
    public GameObject debrisMesh;

    void Awake()
    {
        CreateOverlay();
     
    }

    void Start()
    {
        
    }

    public void Highlight()
    {
        foreach (Renderer r in renderers)
        {
            r.material.color = Color.cyan;
        }

        transform.localScale = originalScale * 1.2f;
    }

    public void Unhighlight()
    {
        transform.localScale = originalScale;

        UpdateVisual(); // restore correct colours
    }

    public void Initialise(Hazard data)
    {
        hazardType = data.type;
        severity = data.severity;

        SetMeshByType();

        
        renderers = GetComponentsInChildren<Renderer>();

        originalScale = transform.localScale;

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
        foreach (Renderer r in renderers)
        {
            if (severity == 3)
                r.material.color = Color.red;
            else if (severity == 2)
                r.material.color = Color.yellow;
            else
                r.material.color = Color.green;
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

    void SetMeshByType() // Activates the appropriate mesh based on the hazard type
    {
        if (crackMesh == null || waterMesh == null || debrisMesh == null)
        {
            Debug.LogError("Mesh references not assigned in HazardObject!");
            return;
        }

        // Disable all first
        crackMesh.SetActive(false);
        waterMesh.SetActive(false);
        debrisMesh.SetActive(false);

        switch (hazardType)
        {
            case "Crack":
                crackMesh.SetActive(true);
                break;

            case "Water":
                waterMesh.SetActive(true);
                break;

            case "Debris":
                debrisMesh.SetActive(true);
                break;

            default:
                crackMesh.SetActive(true);
                break;
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