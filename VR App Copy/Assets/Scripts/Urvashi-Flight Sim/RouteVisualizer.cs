using UnityEngine;

/// <summary>
/// Draws a dotted LineRenderer along the selected plane's route waypoints.
/// Hides the line when no plane is selected.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class RouteVisualizer : MonoBehaviour
{
    [SerializeField] private PlaneManager planeManager;
    [SerializeField] private LineRenderer line;

    [Tooltip("Vertical offset added to each waypoint so the line draws clearly above terrain.")]
    [SerializeField] private float yOffset = 0f;

    private void Awake()
    {
        planeManager ??= PlaneManager.Instance;
        if (line == null) line = GetComponent<LineRenderer>();
    }

    private void OnEnable()
    {
        if (planeManager == null) return;
        planeManager.OnPlaneSelected += HandleSelected;
        HandleSelected(planeManager.SelectedPlaneId);
    }

    private void OnDisable()
    {
        if (planeManager != null)
            planeManager.OnPlaneSelected -= HandleSelected;
    }

    private void HandleSelected(string hex)
    {
        if (line == null) return;

        if (string.IsNullOrEmpty(hex) || planeManager?.Provider == null)
        {
            line.positionCount = 0;
            return;
        }

        Vector3[] route = planeManager.Provider.GetRoute(hex);
        if (route == null || route.Length < 2)
        {
            line.positionCount = 0;
            return;
        }

        if (yOffset != 0f)
        {
            for (int i = 0; i < route.Length; i++)
                route[i] += new Vector3(0f, yOffset, 0f);
        }

        line.positionCount = route.Length;
        line.SetPositions(route);
    }
}
