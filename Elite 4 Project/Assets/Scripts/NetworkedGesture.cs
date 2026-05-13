using UnityEngine;
using Fusion;
using System.Data.Common;

public class NetworkedGesture : NetworkBehaviour
{
    [SerializeField] private NetworkPrefabRef highlightPrefab;
    [SerializeField] private LineRenderer networkedLaser;
    [Networked] private bool LaserActive { get; set; }
    [Networked] private Vector3 LaserStart { get; set; }
    [Networked] private Vector3 LaserEnd { get; set; }

    private LineRenderer lineRenderer;
    private SpawnHighlight spawnHighlight;

    public override void Spawned()
    {
        GameObject laserObj = GameObject.Find("NetworkedLaser");
        if (laserObj != null)
        {
            lineRenderer = laserObj.GetComponent<LineRenderer>();
        }
        else
        {
            Debug.LogWarning("NetworkedGesture: No NetworkedLaser object found in scene");
        }

        if (!Object.HasInputAuthority) return;

        spawnHighlight = FindFirstObjectByType<SpawnHighlight>();
        if (spawnHighlight != null)
        {
            spawnHighlight.OnLaserUpdated += HandleLaserUpdated;
            spawnHighlight.OnLaserStopped += HandleLaserStopped;
            spawnHighlight.OnHighlightRequested += HandleHighlightRequested;
            Debug.Log("NetworkedGestureSync: Hooked into SpawnHighlight");
        }
        else
        {
            Debug.LogWarning("NetworkedGestureSync: No SpawnHighlight found in scene");
        }
    }

    void OnDestroy()
    {
        if (spawnHighlight != null)
        {
            spawnHighlight.OnLaserUpdated -= HandleLaserUpdated;
            spawnHighlight.OnLaserStopped -= HandleLaserStopped;
            spawnHighlight.OnHighlightRequested -= HandleHighlightRequested;
        }
    }

    private void HandleLaserUpdated(Vector3 start, Vector3 end)
    {
        LaserActive = true;
        LaserStart = start;
        LaserEnd = end;
    }

    private void HandleLaserStopped()
    {
        LaserActive = false;
    }

    private void HandleHighlightRequested(Vector3 position)
    {
        Debug.Log($"Spawning highlight at {position}, prefab valid: {highlightPrefab.IsValid}");
        Runner.Spawn(highlightPrefab, position + Vector3.up * 0.01f, Quaternion.identity);
    }

    public override void Render()
    {
        if (lineRenderer == null) return;

        // Local player uses their own line renderer on DetectGestureRightHand
        if (Object.HasInputAuthority)
        {
            lineRenderer.enabled = false;
            return;
        }

        lineRenderer.enabled = LaserActive;
        if (LaserActive)
        {
            lineRenderer.SetPosition(0, LaserStart);
            lineRenderer.SetPosition(1, LaserEnd);
        }
    }
}