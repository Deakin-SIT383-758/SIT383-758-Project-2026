using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(CharacterController))]
public class QuestJoystickMove : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("Vertical Movement")]
    public float verticalSpeed = 2f;
    public float minHeight = 0.5f;
    public float maxHeight = 5f;

    private CharacterController characterController;

    void Start()
    {
        characterController =
            GetComponent<CharacterController>();
    }

    void Update()
    {
        InputDevice leftController =
            InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        InputDevice rightController =
            InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        InputDevice headset =
            InputDevices.GetDeviceAtXRNode(XRNode.Head);

        Vector3 movement = Vector3.zero;

        // HEAD-RELATIVE MOVEMENT
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
            Vector3 forward =
                headRotation * Vector3.forward;

            Vector3 right =
                headRotation * Vector3.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            movement +=
                (forward * leftJoystick.y +
                 right * leftJoystick.x)
                * moveSpeed;
        }

        // VERTICAL MOVEMENT
        if (
            rightController.TryGetFeatureValue(
                CommonUsages.primary2DAxis,
                out Vector2 rightJoystick)
        )
        {
            movement.y =
                rightJoystick.y * verticalSpeed;
        }

        // MOVE USING PHYSICS COLLISION
        characterController.Move(
            movement * Time.deltaTime);

        // CLAMP HEIGHT
        Vector3 position = transform.position;

        position.y =
            Mathf.Clamp(
                position.y,
                minHeight,
                maxHeight);

        transform.position = position;
    }
}