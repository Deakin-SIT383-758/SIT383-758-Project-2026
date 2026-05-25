using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns and updates PlaneVisual instances for every active plane.
/// Subscribes to the data provider's update/remove events.
/// </summary>
public class FlightOverlayManager : MonoBehaviour
{
    [SerializeField] private PlaneManager planeManager;
    [SerializeField] private PlaneVisual planePrefab;
    [Tooltip("Parent transform that all plane visuals are spawned under (e.g. WorldMap root).")]
    [SerializeField] private Transform mapRoot;

    private readonly Dictionary<string, PlaneVisual> _visuals = new();
    private IPlaneDataProvider _provider;

    private void Awake()
    {
        planeManager ??= PlaneManager.Instance;
        if (mapRoot == null) mapRoot = transform;
    }

    private void OnEnable()
    {
        if (planeManager == null) return;

        _provider = planeManager.Provider;
        if (_provider != null)
        {
            _provider.OnPlaneUpdated += HandlePlaneUpdated;
            _provider.OnPlaneRemoved += HandlePlaneRemoved;

            foreach (var kvp in _provider.ActivePlanes)
                Spawn(kvp.Value);
        }
    }

    private void OnDisable()
    {
        if (_provider != null)
        {
            _provider.OnPlaneUpdated -= HandlePlaneUpdated;
            _provider.OnPlaneRemoved -= HandlePlaneRemoved;
        }
    }

    private void HandlePlaneUpdated(PlaneData plane)
    {
        if (!_visuals.TryGetValue(plane.hex, out var visual))
            visual = Spawn(plane);
        visual.ApplyData(plane);
    }

    private void HandlePlaneRemoved(string hex)
    {
        if (!_visuals.TryGetValue(hex, out var visual)) return;
        _visuals.Remove(hex);
        if (visual != null) Destroy(visual.gameObject);
    }

    private PlaneVisual Spawn(PlaneData plane)
    {
        if (planePrefab == null)
        {
            Debug.LogError("[FlightOverlayManager] Plane prefab not assigned.");
            return null;
        }

        var visual = Instantiate(planePrefab, mapRoot);
        visual.name = $"Plane_{plane.flight}_{plane.hex}";
        visual.Bind(plane.hex);
        visual.ApplyData(plane);
        _visuals[plane.hex] = visual;
        return visual;
    }
}
