using UnityEngine;

public class VRMenuSmoothFollow : MonoBehaviour
{
    public Transform head;

    [Header("Placement")]
    public float distance = 2f;
    public float verticalOffset = -0.2f;

    [Header("Smooth Follow")]
    public float positionSmoothTime = 0.15f;
    public float rotationSmoothSpeed = 8f;

    [Header("Dead Zones")]
    public float positionDeadZone = 0.15f;
    public float rotationDeadZone = 10f;

    private Vector3 positionVelocity;

    private Vector3 currentTargetPosition;
    private Quaternion currentTargetRotation;

    private void OnEnable()
    {
        SnapToHead();
    }

    void LateUpdate()
    {
        if (head == null)
            return;

        Vector3 desiredPosition =
            head.position +
            head.forward * distance +
            Vector3.up * verticalOffset;

        float distanceFromTarget =
            Vector3.Distance(currentTargetPosition, desiredPosition);

        if (distanceFromTarget > positionDeadZone)
        {
            currentTargetPosition = desiredPosition;
        }

        transform.position = Vector3.SmoothDamp(
            transform.position,
            currentTargetPosition,
            ref positionVelocity,
            positionSmoothTime
        );

        Vector3 lookDirection =
            head.position - transform.position;

        Quaternion desiredRotation =
            Quaternion.LookRotation(-lookDirection);

        float angleDifference =
            Quaternion.Angle(currentTargetRotation, desiredRotation);

        if (angleDifference > rotationDeadZone)
        {
            currentTargetRotation = desiredRotation;
        }

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            currentTargetRotation,
            rotationSmoothSpeed * Time.deltaTime
        );
    }

    private void SnapToHead()
    {
        if (head == null)
            return;

        positionVelocity = Vector3.zero;

        currentTargetPosition =
            head.position +
            head.forward * distance +
            Vector3.up * verticalOffset;

        transform.position = currentTargetPosition;

        Vector3 lookDirection =
            head.position - transform.position;

        currentTargetRotation =
            Quaternion.LookRotation(-lookDirection);

        transform.rotation = currentTargetRotation;
    }
}