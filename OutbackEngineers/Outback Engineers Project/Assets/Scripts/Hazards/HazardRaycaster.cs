using UnityEngine;

public class HazardRaycaster : MonoBehaviour
{
    public float maxDistance = 10f;
    private HazardObject currentTarget;

    public HUDManager hudManager; //Phase 3: Reference to the HUDManager to display hazard information on the HUD

    void Update()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward); // Will later be updated to playerTransform for VR compatibility!

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance)) // Only consider hits within maxDistance
        {
            HazardObject hazard = hit.collider.GetComponent<HazardObject>();

            if (hazard != null) // Only consider hits on objects with a HazardObject component
            {
                if (currentTarget != hazard)
                {
                    if (currentTarget != null)
                        currentTarget.Unhighlight();

                    currentTarget = hazard;
                    currentTarget.Highlight();

                    if (hudManager != null) // Phase 3: Display hazard information on the HUD when a new hazard is targeted
                        hudManager.ShowHazardInfo(hazard.hazardType, hazard.severity);
                }
            }
        }
        else
        {
            if (currentTarget != null)
            {
                currentTarget.Unhighlight();
                currentTarget = null;
            }

            if (hudManager != null) // Phase 3: Clear hazard information from the HUD when no hazard is targeted
                hudManager.ClearHazardInfo();
        }
    }
}
