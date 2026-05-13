using System;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.Gestures;
using UnityEngine.XR.Hands.Samples.GestureSample;

public class DetectGesture : MonoBehaviour
{
    [SerializeField] private XRHandTrackingEvents handTrackingEvents;

    // Hand shapes are created in the XR Hands visualiser and assigned here in the inspector
    [SerializeField] private XRHandShape[] handShapes;

    // How often to check for the gesture, checking every frame is unnecessary and expensive
    [SerializeField] private float gestureDetectionInterval = 0.1f;

    // How closely the hand needs to match the shape to count as detected (0-1)
    [SerializeField] private float minimumDetectionThreshold = 0.9f;

    [SerializeField] private HandShapeCompletenessCalculator handShapeCompletenessCalculator;

    public event Action OnPointingStarted;
    public event Action OnPointingStopped;

    public bool IsPointing { get; private set; }

    private float timeOfLastConditionCheck;
    private bool wasPointing;

    // Subscribe and unsubscribe to hand joint updates
    void OnEnable() => handTrackingEvents.jointsUpdated.AddListener(OnJointsUpdated);
    private void OnDisable() => handTrackingEvents.jointsUpdated.RemoveListener(OnJointsUpdated);

    void OnJointsUpdated(XRHandJointsUpdatedEventArgs eventArgs)
    {
        // Only check at the set interval rather than every joint update
        if (Time.time - timeOfLastConditionCheck < gestureDetectionInterval) return;

        bool detected = false;

        // Check each hand shape, if any of them match well enough, the gesture counts
        foreach (var handShape in handShapes)
        {
            handShapeCompletenessCalculator.TryCalculateHandShapeCompletenessScore(
                eventArgs.hand, handShape, out float completenessScore);

            if (handTrackingEvents.handIsTracked && completenessScore >= minimumDetectionThreshold)
            {
                detected = true;
                break;
            }
        }

        IsPointing = detected;

        // Only fire events when the state actually changes, not every interval
        if (IsPointing && !wasPointing)
            OnPointingStarted?.Invoke();
        else if (!IsPointing && wasPointing)
            OnPointingStopped?.Invoke();

        wasPointing = IsPointing;
        timeOfLastConditionCheck = Time.time;
    }
}