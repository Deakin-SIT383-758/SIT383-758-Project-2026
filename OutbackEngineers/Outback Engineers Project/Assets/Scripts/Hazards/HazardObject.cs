using UnityEngine;

public class HazardObject : MonoBehaviour // Phase 2: HazardObject class created to represent hazards in the scene, with visual representation based on severity
{                                         // No longer just a data class, now also handles visual representation in the scene
    public int severity;

    private Renderer rend;

    void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
        UpdateVisual();
    }

    public void SetSeverity(int value)
    {
        severity = value;
        UpdateVisual();
    }

    void UpdateVisual()
    {
        if (rend == null) return;

        if (severity == 3)
            rend.material.color = Color.red;
        else if (severity == 2)
            rend.material.color = Color.yellow;
        else
            rend.material.color = Color.green;
    }
}