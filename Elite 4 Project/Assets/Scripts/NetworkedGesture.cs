using UnityEngine;
using Fusion;
using UnityEngine.XR.Hands;

public class NetworkedGesture : NetworkBehaviour
{
    [SerializeField] private NetworkPrefabRef highlightPrefab;

    // [Networked] means Fusion will automatically sync these values to all players
    [Networked] private bool LaserActiveRight { get; set; }
    [Networked] private Vector3 LaserStartRight { get; set; }
    [Networked] private Vector3 LaserEndRight { get; set; }
    [Networked] private bool LaserActiveLeft { get; set; }
    [Networked] private Vector3 LaserStartLeft { get; set; }
    [Networked] private Vector3 LaserEndLeft { get; set; }

    private LineRenderer lineRendererRight;
    private LineRenderer lineRendererLeft;
    private SpawnHighlight rightSpawnHighlight;
    private SpawnHighlight leftSpawnHighlight;

    public override void Spawned()
    {
        // Grab the line renderers from the scene objects that are set up for the lasers
        GameObject rightLaserObj = GameObject.Find("NetworkedLaserRight");
        if (rightLaserObj != null)
            lineRendererRight = rightLaserObj.GetComponent<LineRenderer>();

        GameObject leftLaserObj = GameObject.Find("NetworkedLaserLeft");
        if (leftLaserObj != null)
            lineRendererLeft = leftLaserObj.GetComponent<LineRenderer>();

        // Only the local player needs to listen to SpawnHighlight events
        // Remote players just read the networked values in Render()
        if (!Object.HasInputAuthority) return;

        // Find both SpawnHighlight scripts in the scene (one per hand) and hook into their events
        SpawnHighlight[] spawnHighlights = FindObjectsByType<SpawnHighlight>(FindObjectsSortMode.None);
        foreach (var s in spawnHighlights)
        {
            if (s.handedness == Handedness.Right)
                rightSpawnHighlight = s;
            else if (s.handedness == Handedness.Left)
                leftSpawnHighlight = s;
        }

        if (rightSpawnHighlight != null)
        {
            rightSpawnHighlight.OnLaserUpdated += HandleLaserUpdated;
            rightSpawnHighlight.OnLaserStopped += HandleLaserStopped;
            rightSpawnHighlight.OnHighlightRequested += HandleHighlightRequested;
        }

        if (leftSpawnHighlight != null)
        {
            leftSpawnHighlight.OnLaserUpdated += HandleLaserUpdated;
            leftSpawnHighlight.OnLaserStopped += HandleLaserStopped;
            leftSpawnHighlight.OnHighlightRequested += HandleHighlightRequested;
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events when the object is destroyed to avoid memory leaks
        if (rightSpawnHighlight != null)
        {
            rightSpawnHighlight.OnLaserUpdated -= HandleLaserUpdated;
            rightSpawnHighlight.OnLaserStopped -= HandleLaserStopped;
            rightSpawnHighlight.OnHighlightRequested -= HandleHighlightRequested;
        }

        if (leftSpawnHighlight != null)
        {
            leftSpawnHighlight.OnLaserUpdated -= HandleLaserUpdated;
            leftSpawnHighlight.OnLaserStopped -= HandleLaserStopped;
            leftSpawnHighlight.OnHighlightRequested -= HandleHighlightRequested;
        }

        // Turn off the lasers when the player leaves rather than deleting the scene objects
        if (lineRendererRight != null) lineRendererRight.enabled = false;
        if (lineRendererLeft != null) lineRendererLeft.enabled = false;
    }

    // SpawnHighlight calls this every frame while pointing, passing the laser start/end positions
    private void HandleLaserUpdated(Vector3 start, Vector3 end, Handedness hand)
    {
        if (hand == Handedness.Right)
        {
            LaserActiveRight = true;
            LaserStartRight = start;
            LaserEndRight = end;
        }
        else
        {
            LaserActiveLeft = true;
            LaserStartLeft = start;
            LaserEndLeft = end;
        }
    }

    // Called when the gesture stops, just turns the laser off
    private void HandleLaserStopped(Handedness hand)
    {
        if (hand == Handedness.Right)
            LaserActiveRight = false;
        else
            LaserActiveLeft = false;
    }

    // Called when gesture ends on a valid map hit, uses Fusion to spawn the marker for all players
    private void HandleHighlightRequested(Vector3 position)
    {
        Runner.Spawn(highlightPrefab, position + Vector3.up * 0.01f, Quaternion.identity);
    }

    // Render() runs every frame after Fusion has finished its network updates
    // This is used to draw the laser for remote players based on the synced values above
    public override void Render()
    {
        // Local player already has their own laser on DetectGestureRightHand/Left, skip them
        if (Object.HasInputAuthority) return;

        if (lineRendererRight != null)
        {
            lineRendererRight.enabled = LaserActiveRight;
            if (LaserActiveRight)
            {
                lineRendererRight.SetPosition(0, LaserStartRight);
                lineRendererRight.SetPosition(1, LaserEndRight);
            }
        }

        if (lineRendererLeft != null)
        {
            lineRendererLeft.enabled = LaserActiveLeft;
            if (LaserActiveLeft)
            {
                lineRendererLeft.SetPosition(0, LaserStartLeft);
                lineRendererLeft.SetPosition(1, LaserEndLeft);
            }
        }
    }
}