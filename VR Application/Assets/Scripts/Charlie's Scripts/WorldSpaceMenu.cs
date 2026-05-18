using UnityEngine;

public class WorldSpaceMenu : MonoBehaviour
{
    [Header("Follow Target")]
    public Transform cameraTransform; // Drag your Main Camera here

    [Header("Positioning")]
    public Vector3 offset = new Vector3(0f, -0.2f, 0.6f); // Relative to camera
    public float followSpeed = 5f;       // Smoothing speed
    public float rotationSpeed = 5f;

    [Header("Deadzone (prevents jitter)")]
    public float positionDeadzone = 0.05f;

    private Vector3 _targetPosition;
    private Quaternion _targetRotation;

    void Start()
    {
        // Auto-find camera if not assigned
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        // Snap to position immediately on start
        transform.position = GetTargetPosition();
        transform.rotation = GetTargetRotation();
    }

    void Update()
    {
        _targetPosition = GetTargetPosition();
        _targetRotation = GetTargetRotation();

        // Only move if outside deadzone (avoids jitter when standing still)
        if (Vector3.Distance(transform.position, _targetPosition) > positionDeadzone)
        {
            transform.position = Vector3.Lerp(
                transform.position, _targetPosition, Time.deltaTime * followSpeed);
        }

        transform.rotation = Quaternion.Slerp(
            transform.rotation, _targetRotation, Time.deltaTime * rotationSpeed);
    }

    Vector3 GetTargetPosition()
    {
        return cameraTransform.TransformPoint(offset);
    }

    Quaternion GetTargetRotation()
    {
        // Always face the camera
        Vector3 directionToCamera = transform.position - cameraTransform.position;
        if (directionToCamera == Vector3.zero) return transform.rotation;
        return Quaternion.LookRotation(directionToCamera);
    }
}