using UnityEngine;

public class ControllerInputManager : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private OVRPassthroughLayer passthroughLayer;
    [SerializeField] private float verticalOffset = 50f;
    private bool inVR = false;

    private void Start()
    {
        passthroughLayer.textureOpacity = inVR ? 1 : 0;
    }

    private void Update()
    {
        bool triggered = (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger) || OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger));
        if (triggered)
            ToggleVR();
    }

    void ToggleVR()
    {
        inVR = !inVR;
        passthroughLayer.textureOpacity = inVR ? 1 : 0;
        float offset = inVR ? verticalOffset : -verticalOffset;
        Vector3 targetPos = new Vector3(
            playerTransform.position.x,
            playerTransform.position.y + offset,
            playerTransform.position.z);

        playerTransform.position = targetPos;
    }

}
