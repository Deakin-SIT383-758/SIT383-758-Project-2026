using UnityEngine;

public class HazardRaycaster : MonoBehaviour
{
    public float maxDistance = 10f;
    private HazardObject currentTarget;

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
        }
    }
}
