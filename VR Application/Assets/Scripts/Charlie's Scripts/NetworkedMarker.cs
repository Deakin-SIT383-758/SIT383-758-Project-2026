using Fusion;
using UnityEngine;

public class NetworkedMarker : NetworkBehaviour
{
    [SerializeField] private float lifetime = 10f;
    private LineRenderer lineRenderer;
    private float timer;

    [Networked] private Vector3 NetworkedHitPoint { get; set; }
    [Networked] private Vector3 NetworkedRayOrigin { get; set; }

    public override void Spawned()
    {
        timer = lifetime;
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void Initialise(Vector3 hitPoint, Vector3 rayOrigin)
    {
        // Only the spawning client sets these
        NetworkedHitPoint = hitPoint;
        NetworkedRayOrigin = rayOrigin;

        transform.position = hitPoint;
        UpdateLineRenderer();
    }

    public override void FixedUpdateNetwork()
    {
        // Sync position on all clients
        transform.position = NetworkedHitPoint;
        UpdateLineRenderer();

        // Only the State Authority counts down and despawns
        if (!HasStateAuthority) return;

        timer -= Runner.DeltaTime;
        if (timer <= 0f)
            Runner.Despawn(Object);
    }

    private void UpdateLineRenderer()
    {
        if (lineRenderer == null) return;
        lineRenderer.SetPosition(0, NetworkedRayOrigin);
        lineRenderer.SetPosition(1, NetworkedHitPoint);
    }
}