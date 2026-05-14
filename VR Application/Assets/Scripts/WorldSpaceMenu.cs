using UnityEngine;

public class VRMenuSmoothFollow : MonoBehaviour
{
    public Transform head;

    [Header("Placement")]
    public float distance = 2f;

    // Negative = lower
    public float verticalOffset = -0.2f;

    [Header("Smooth Follow")]
    public float positionSmoothTime = 0.15f;
    public float rotationSmoothSpeed = 8f;

    private Vector3 positionVelocity;

    void LateUpdate()
    {
        if (head == null)
            return;

        Vector3 targetPosition =
            head.position +
            head.forward * distance +
            Vector3.up * verticalOffset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref positionVelocity,
            positionSmoothTime
        );

        Vector3 lookDirection =
            transform.position - head.position;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(lookDirection),
            rotationSmoothSpeed * Time.deltaTime
        );
    }
}