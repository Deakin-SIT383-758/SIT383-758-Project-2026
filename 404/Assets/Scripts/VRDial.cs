using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRDial : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
{
    [Header("Dial Visual")]
    public Transform dialVisual;

    [Header("Rotation Settings")]
    public float startAngle = 90f;

    [Tooltip("Maximum rotation left/right from start angle")]
    public float rotationLimit = 45f;

    public float smoothSpeed = 15f;

    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor grabbingHand;

    private Vector3 lockedPosition;
    private Quaternion lockedRotation;

    private float currentAngle;

    private float grabStartHandAngle;
    private float grabStartDialAngle;

    protected override void Awake()
    {
        base.Awake();

        lockedPosition = transform.position;
        lockedRotation = transform.rotation;

        trackPosition = false;
        trackRotation = false;
        throwOnDetach = false;

        currentAngle = startAngle;

        UpdateDialVisual();
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        grabbingHand = args.interactorObject as UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor;

        grabStartHandAngle = GetHandAngle();
        grabStartDialAngle = currentAngle;
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        grabbingHand = null;
    }

    private void LateUpdate()
    {
        // Keep object fixed
        transform.position = lockedPosition;
        transform.rotation = lockedRotation;

        if (grabbingHand == null)
            return;

        float currentHandAngle = GetHandAngle();

        float angleOffset =
            Mathf.DeltaAngle(grabStartHandAngle, currentHandAngle);

        float targetAngle =
            grabStartDialAngle + angleOffset;

        // Clamp around start angle
        targetAngle = Mathf.Clamp(
            targetAngle,
            startAngle - rotationLimit,
            startAngle + rotationLimit
        );

        currentAngle = Mathf.Lerp(
            currentAngle,
            targetAngle,
            Time.deltaTime * smoothSpeed
        );

        UpdateDialVisual();
    }

    private float GetHandAngle()
    {
        Vector3 localHandPos =
            transform.InverseTransformPoint(grabbingHand.transform.position);

        return Mathf.Atan2(localHandPos.x, localHandPos.z) * Mathf.Rad2Deg;
    }

    private void UpdateDialVisual()
    {
        dialVisual.localRotation =
            Quaternion.Euler(0f, currentAngle, 0f);
    }
}