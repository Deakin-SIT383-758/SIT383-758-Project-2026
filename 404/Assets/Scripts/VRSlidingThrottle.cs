using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRSlidingThrottle : MonoBehaviour
{
    public enum SlideAxis
    {
        LocalX,
        LocalY,
        LocalZ
    }

    [Header("Throttle Movement")]
    public SlideAxis controllerAxis = SlideAxis.LocalZ;
    public SlideAxis throttleAxis = SlideAxis.LocalX;
    public float minPosition = 0f;
    public float maxPosition = 0.3f;
    public float controllerMovementMultiplier = 2f;
    public bool invertDirection = false;

    [Range(0, 1)]
    public float throttleValue;

    public string controlName = "Throttle";
    public TMP_Text text;

    [Header("References")]
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    private bool isGrabbed = false;
    private Transform handTransform;

    private Vector3 startLocalPos;
    private Quaternion startLocalRotation;
    private Vector3 startLocalScale;

    private float grabStartHandPosition;
    private float grabStartThrottlePosition;
    private float targetPosition;

    private void Start()
    {
        startLocalPos = transform.localPosition;
        startLocalRotation = transform.localRotation;
        startLocalScale = transform.localScale;
        targetPosition = Mathf.Clamp(GetAxisValue(startLocalPos, throttleAxis), minPosition, maxPosition);
        ApplyThrottlePosition(targetPosition);

        if (grabInteractable == null)
            grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        if (grabInteractable != null)
        {
            grabInteractable.trackPosition = false;
            grabInteractable.trackRotation = false;
            grabInteractable.trackScale = false;
            grabInteractable.predictedVisualsTransform = null;
            grabInteractable.throwOnDetach = false;

            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
        }

        if (TryGetComponent(out Rigidbody throttleRigidbody))
        {
            throttleRigidbody.isKinematic = true;
            throttleRigidbody.useGravity = false;
        }

        Application.onBeforeRender += ApplyCurrentThrottlePosition;
    }

    private void OnDestroy()
    {
        Application.onBeforeRender -= ApplyCurrentThrottlePosition;

        if (grabInteractable == null)
            return;

        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        isGrabbed = true;

        handTransform = args.interactorObject.transform;

        Vector3 localHandPos = GetHandPositionInRailSpace();

        grabStartHandPosition = GetAxisValue(localHandPos, controllerAxis);
        grabStartThrottlePosition = targetPosition;
        ApplyThrottlePosition(targetPosition);
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
        handTransform = null;

        ChecklistManager.Instance.ControlUpdate(controlName, throttleValue);
        text.text = Mathf.Round(throttleValue * 100.0f).ToString() + "%";
    }

    private void LateUpdate()
    {
        if (!isGrabbed || handTransform == null)
        {
            ApplyThrottlePosition(targetPosition);
            return;
        }

        Vector3 localHandPos = GetHandPositionInRailSpace();

        float handDelta = GetAxisValue(localHandPos, controllerAxis) - grabStartHandPosition;

        if (invertDirection)
            handDelta = -handDelta;

        handDelta *= controllerMovementMultiplier;

        targetPosition = grabStartThrottlePosition + handDelta;
        targetPosition = Mathf.Clamp(targetPosition, minPosition, maxPosition);

        ApplyThrottlePosition(targetPosition);
    }

    private void ApplyCurrentThrottlePosition()
    {
        ApplyThrottlePosition(targetPosition);
    }

    private void ApplyThrottlePosition(float axisPosition)
    {
        Vector3 targetPos = startLocalPos;
        SetAxisValue(ref targetPos, throttleAxis, axisPosition);

        transform.localPosition = targetPos;
        transform.localRotation = startLocalRotation;
        transform.localScale = startLocalScale;

        throttleValue = Mathf.InverseLerp(minPosition, maxPosition, axisPosition);
    }

    private Vector3 GetHandPositionInRailSpace()
    {
        if (transform.parent != null)
            return transform.parent.InverseTransformPoint(handTransform.position);

        return handTransform.position;
    }

    private float GetAxisValue(Vector3 position, SlideAxis axis)
    {
        switch (axis)
        {
            case SlideAxis.LocalX:
                return position.x;
            case SlideAxis.LocalY:
                return position.y;
            default:
                return position.z;
        }
    }

    private void SetAxisValue(ref Vector3 position, SlideAxis axis, float value)
    {
        switch (axis)
        {
            case SlideAxis.LocalX:
                position.x = value;
                break;
            case SlideAxis.LocalY:
                position.y = value;
                break;
            default:
                position.z = value;
                break;
        }
    }
}
