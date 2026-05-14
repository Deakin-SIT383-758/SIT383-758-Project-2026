using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRDial : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
{
    [Header("Dial Settings")]
    public Transform dialVisual;

    public float rotationSpeed = 1f;

    public float minAngle = -90f;
    public float maxAngle = 90f;

    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor grabbingHand;

    private float currentAngle;

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        grabbingHand = args.interactorObject as UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor;
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        grabbingHand = null;
    }

    private void Update()
    {
        if (grabbingHand == null)
            return;

        Transform handTransform = grabbingHand.transform;

        // Get hand local direction relative to dial
        Vector3 localHandPos = transform.InverseTransformPoint(handTransform.position);

        // Calculate angle around Y axis
        float targetAngle = Mathf.Atan2(localHandPos.x, localHandPos.z) * Mathf.Rad2Deg;

        // Clamp angle
        targetAngle = Mathf.Clamp(targetAngle, minAngle, maxAngle);

        currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * 10f);

        dialVisual.localRotation = Quaternion.Euler(0f, currentAngle, 0f);
    }
}