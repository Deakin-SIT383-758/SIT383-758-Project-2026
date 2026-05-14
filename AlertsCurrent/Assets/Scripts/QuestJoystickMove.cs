using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(CharacterController))]
public class QuestJoystickMove : MonoBehaviour
{
    [Header("Horizontal Movement")]
    public float moveSpeed = 2f;

    [Header("Vertical Movement")]
    public float verticalSpeed = 2f;
    public float minHeight = 0.5f;
    public float maxHeight = 5f;

    private CharacterController characterController;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        Vector3 movement = Vector3.zero;

        InputDevice leftController =
            InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        InputDevice rightController =
            InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        InputDevice headset =
            InputDevices.GetDeviceAtXRNode(XRNode.Head);

        if (
            leftController.TryGetFeatureValue(
                CommonUsages.primary2DAxis,
                out Vector2 leftJoystick)
            &&
            headset.TryGetFeatureValue(
                CommonUsages.deviceRotation,
                out Quaternion headRotation)
        )
        {
            Vector3 forward = headRotation * Vector3.forward;
            Vector3 right = headRotation * Vector3.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            movement +=
                (forward * leftJoystick.y + right * leftJoystick.x)
                * moveSpeed;
        }

        if (
            rightController.TryGetFeatureValue(
                CommonUsages.primary2DAxis,
                out Vector2 rightJoystick)
        )
        {
            movement.y = rightJoystick.y * verticalSpeed;
        }

        characterController.Move(movement * Time.deltaTime);

        float currentY = transform.position.y;
        float clampedY = Mathf.Clamp(currentY, minHeight, maxHeight);
        float correctionY = clampedY - currentY;

        if (Mathf.Abs(correctionY) > 0.001f)
        {
            characterController.Move(new Vector3(0f, correctionY, 0f));
        }
    }
}