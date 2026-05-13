using System;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.Gestures;
using UnityEngine.XR.Hands.Samples.GestureSample;

public class DetectGesture : MonoBehaviour
{
    [SerializeField] private XRHandTrackingEvents handTrackingEvents;
    [SerializeField] private XRHandShape[] handShapes;
    [SerializeField] private float gestureDetectionInterval = 0.1f;
    [SerializeField] private float minimumDetectionThreshold = 0.9f;
    [SerializeField] private HandShapeCompletenessCalculator handShapeCompletenessCalculator;

    public event Action OnPointingStarted;
    public event Action OnPointingStopped;

    public bool IsPointing {  get; private set; }

    private float timeOfLastConditionCheck;
    private bool wasPointing;

    void OnEnable() => handTrackingEvents.jointsUpdated.AddListener(OnJointsUpdated);

    private void OnDisable() => handTrackingEvents.jointsUpdated.RemoveListener(OnJointsUpdated);

    void OnJointsUpdated(XRHandJointsUpdatedEventArgs eventArgs)
    {
        if (Time.time - timeOfLastConditionCheck < gestureDetectionInterval) return;

        bool detected = false;

        foreach (var handShape in handShapes)
        {
            handShapeCompletenessCalculator.TryCalculateHandShapeCompletenessScore(eventArgs.hand, handShape, out float completenessScore);

            if (handTrackingEvents.handIsTracked && completenessScore >= minimumDetectionThreshold)
            {
                detected = true;
                break;
            }
        }

        IsPointing = detected;

        if (IsPointing && !wasPointing)
        {
            Debug.Log("Pointing Started");
            OnPointingStarted?.Invoke();
        }
        else if (!IsPointing && wasPointing)
        {
            Debug.Log("Pointing Stopped");
            OnPointingStopped?.Invoke();
        }

        wasPointing = IsPointing;
        timeOfLastConditionCheck = Time.time;
    }
}
