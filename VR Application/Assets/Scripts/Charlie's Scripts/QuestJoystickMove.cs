using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(CharacterController))]
public class QuestJoystickMove : MonoBehaviour
{
    public Transform head;

    public float moveSpeed = 2f;
    public float verticalSpeed = 2f;

    public float minHeight = 0.5f;
    public float maxHeight = 5f;

    private CharacterController cc;

    void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        UpdateCharacterController();

        Vector2 leftInput = Vector2.zero;
        Vector2 rightInput = Vector2.zero;

        InputDevice leftController =
            InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        InputDevice rightController =
            InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        leftController.TryGetFeatureValue(
            CommonUsages.primary2DAxis,
            out leftInput);

        rightController.TryGetFeatureValue(
            CommonUsages.primary2DAxis,
            out rightInput);

        Vector3 forward = head.forward;
        Vector3 right = head.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 move =
            (forward * leftInput.y + right * leftInput.x)
            * moveSpeed;

        move.y = rightInput.y * verticalSpeed;

        cc.Move(move * Time.deltaTime);

        ClampHeight();
    }

    void UpdateCharacterController()
    {
        cc.height = Mathf.Clamp(head.localPosition.y, 1f, 2f);

        Vector3 center = head.localPosition;
        center.y = cc.height / 2f;

        cc.center = center;
    }

    void ClampHeight()
    {
        Vector3 pos = transform.position;

        pos.y = Mathf.Clamp(pos.y, minHeight, maxHeight);

        transform.position = pos;
    }
}