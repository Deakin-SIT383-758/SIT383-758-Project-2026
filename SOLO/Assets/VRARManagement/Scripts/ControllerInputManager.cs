using UnityEngine;

public class ControllerInputManager : MonoBehaviour
{
    [SerializeField] private OVRPassthroughLayer passthroughLayer;

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
    }

}
