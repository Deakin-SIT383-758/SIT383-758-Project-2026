using UnityEngine;
using UnityEngine.XR.Hands;
using System.Collections.Generic;
using System.Collections;
using System;

public class SpawnHighlight : MonoBehaviour
{
    [SerializeField] private DetectGesture gestureDetector;
    [SerializeField] private float maxRayDistance = 10.0f;
    [SerializeField] private string mapTag = "Map";

    // These events are picked up by NetworkedGesture to sync things over the network
    public event Action<Vector3, Vector3, Handedness> OnLaserUpdated;
    public event Action<Handedness> OnLaserStopped;
    public event Action<Vector3> OnHighlightRequested;

    // Determines which hand this script reads from
    public Handedness handedness = Handedness.Right;

    private XRHandSubsystem handSubsystem;
    private LineRenderer lineRenderer;
    private bool isPointing;
    private Vector3 lastHitPoint;
    private bool hasValidHit;

    private void OnEnable()
    {
        lineRenderer = GetComponent<LineRenderer>();

        // Subscribe to gesture events from DetectGesture
        if (gestureDetector != null)
        {
            gestureDetector.OnPointingStarted += HandlePointingStarted;
            gestureDetector.OnPointingStopped += HandlePointingStopped;
        }

        // Hand subsystem isn't immediately available so wait for it in a coroutine
        StartCoroutine(WaitForHandSubsystem());
    }

    private void OnDisable()
    {
        // Unsubscribe when disabled to avoid errors
        if (gestureDetector != null)
        {
            gestureDetector.OnPointingStarted -= HandlePointingStarted;
            gestureDetector.OnPointingStopped -= HandlePointingStopped;
        }
    }

    private void HandlePointingStarted() => isPointing = true;

    private void HandlePointingStopped()
    {
        isPointing = false;
        if (lineRenderer != null) lineRenderer.enabled = false;

        // Tell NetworkedGesture the laser stopped
        OnLaserStopped?.Invoke(handedness);

        // If pointing at the map when the gesture ended, request a highlight spawn
        if (hasValidHit)
        {
            OnHighlightRequested?.Invoke(lastHitPoint);
            hasValidHit = false;
        }
    }

    // Waits until the XR hand subsystem is running before trying to read hand data
    IEnumerator WaitForHandSubsystem()
    {
        List<XRHandSubsystem> subsystems = new();

        while (handSubsystem == null)
        {
            SubsystemManager.GetSubsystems(subsystems);

            foreach (var sub in subsystems)
            {
                if (sub != null && sub.running)
                {
                    handSubsystem = sub;
                    break;
                }
            }

            yield return null;
        }
    }

    void Update()
    {
        if (!isPointing || handSubsystem == null)
        {
            lineRenderer.enabled = false;
            return;
        }

        XRHand hand = (handedness == Handedness.Left) ? handSubsystem.leftHand : handSubsystem.rightHand;
        if (!hand.isTracked) return;

        XRHandJoint indexTip = hand.GetJoint(XRHandJointID.IndexTip);
        XRHandJoint indexIntermediate = hand.GetJoint(XRHandJointID.IndexIntermediate);

        if (!indexTip.TryGetPose(out Pose tipPose)) return;
        if (!indexIntermediate.TryGetPose(out Pose intermediatePose)) return;
        if (lineRenderer == null) return;

        // XR hand joint positions are in local tracking space so we need to convert
        // them to world space using the XR Origin's transform
        Transform xrOrigin = Camera.main.transform.root;

        Vector3 rayOrigin = xrOrigin != null
            ? xrOrigin.TransformPoint(tipPose.position)
            : tipPose.position;

        Vector3 intermediateWorld = xrOrigin != null
            ? xrOrigin.TransformPoint(intermediatePose.position)
            : intermediatePose.position;

        // Ray shoots from the tip away from the intermediate joint, along the finger
        Vector3 rayDirection = (rayOrigin - intermediateWorld).normalized;

        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, maxRayDistance))
        {
            lineRenderer.SetPosition(0, rayOrigin);
            lineRenderer.SetPosition(1, hit.point);
            lineRenderer.enabled = true;

            if (hit.collider.CompareTag(mapTag))
            {
                // Pointing at the map, store the hit point for when the gesture ends
                lastHitPoint = hit.point;
                hasValidHit = true;
                OnLaserUpdated?.Invoke(rayOrigin, hit.point, handedness);
            }
            else
            {
                hasValidHit = false;
                OnLaserUpdated?.Invoke(rayOrigin, rayOrigin + rayDirection * maxRayDistance, handedness);
            }
        }
        else
        {
            // Nothing hit, draw the laser to max distance
            lineRenderer.SetPosition(0, rayOrigin);
            lineRenderer.SetPosition(1, rayOrigin + rayDirection * maxRayDistance);
            lineRenderer.enabled = true;
            hasValidHit = false;
            OnLaserUpdated?.Invoke(rayOrigin, rayOrigin + rayDirection * maxRayDistance, handedness);
        }
    }
}