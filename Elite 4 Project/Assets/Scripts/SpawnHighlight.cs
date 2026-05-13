using UnityEngine;
using UnityEngine.XR.Hands;
using System.Collections.Generic;
using System.Collections;
using System;

public class SpawnHighlight : MonoBehaviour
{
    [SerializeField] private DetectGesture gestureDetector;
    [SerializeField] private Handedness handedness = Handedness.Right;
    [SerializeField] private float maxRayDistance = 10.0f;
    [SerializeField] private string mapTag = "Map";

    public event Action<Vector3, Vector3> OnLaserUpdated;
    public event Action OnLaserStopped;
    public event Action<Vector3> OnHighlightRequested;

    private XRHandSubsystem handSubsystem;
    private LineRenderer lineRenderer;
    private bool isPointing;
    private Vector3 lastHitPoint;
    private bool hasValidHit;

    private void OnEnable()
    {
        lineRenderer = GetComponent<LineRenderer>();
        Debug.Log($"SpawnHighlight: LineRenderer found: {lineRenderer != null}");

        if (gestureDetector != null)
        {
            gestureDetector.OnPointingStarted += HandlePointingStarted;
            gestureDetector.OnPointingStopped += HandlePointingStopped;
        }

        StartCoroutine(WaitForHandSubsystem());
    }

    private void OnDisable()
    {
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
        OnLaserStopped?.Invoke();

        if (hasValidHit)
        {
            OnHighlightRequested?.Invoke(lastHitPoint);
            hasValidHit = false;
        }
    }

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

        Debug.Log("SpawnHighlight: Hand subsystem found and running");
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

        Transform xrOrigin = Camera.main.transform.root;

        // Transform tip position from local tracking space to world space
        Vector3 rayOrigin = xrOrigin != null
            ? xrOrigin.TransformPoint(tipPose.position)
            : tipPose.position;

        Vector3 intermediateWorld = xrOrigin != null
            ? xrOrigin.TransformPoint(intermediatePose.position)
            : intermediatePose.position;

        Vector3 rayDirection = (rayOrigin - intermediateWorld).normalized;

        Debug.Log($"RayOrigin: {rayOrigin}, Direction: {rayDirection}");
        Debug.Log($"SpawnHighlight: Setting lineRenderer enabled, lineRenderer null: {lineRenderer == null}");

        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, maxRayDistance))
        {
            lineRenderer.SetPosition(0, rayOrigin);
            lineRenderer.SetPosition(1, hit.point);
            lineRenderer.enabled = true;
            Debug.Log($"pos0: {lineRenderer.GetPosition(0)}, pos1: {lineRenderer.GetPosition(1)}");

            if (hit.collider.CompareTag(mapTag))
            {
                lastHitPoint = hit.point;
                hasValidHit = true;
                OnLaserUpdated?.Invoke(rayOrigin, hit.point);
            }
            else
            {
                hasValidHit = false;
                OnLaserUpdated?.Invoke(rayOrigin, rayOrigin + rayDirection * maxRayDistance);
            }
        }
        else
        {
            lineRenderer.SetPosition(0, rayOrigin);
            lineRenderer.SetPosition(1, rayOrigin + rayDirection * maxRayDistance);
            lineRenderer.enabled = true;
            hasValidHit = false;
            OnLaserUpdated?.Invoke(rayOrigin, rayOrigin + rayDirection * maxRayDistance);
        }
    }
}
