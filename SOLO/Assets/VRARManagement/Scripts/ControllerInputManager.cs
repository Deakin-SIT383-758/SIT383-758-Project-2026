using UnityEngine;

public class ControllerInputManager : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private OVRPassthroughLayer passthroughLayer;
    [SerializeField] private float verticalOffset = 50f;
    private bool inVR = false;

    private void Start()
    {
        SetupCameraLayers();
        inVR = true;
        ToggleVR();
    }

    private void Update()
    {
        bool triggered = (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger) || OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger));
        if (triggered)
            ToggleVR();
    }

    [Range(1, 10)]
    public float godViewScale = 1.1f;
    void ToggleVR()
    {
        inVR = !inVR;
        float scale = inVR ? godViewScale : 1;
        
        passthroughLayer.textureOpacity = inVR ? 0 : 1;
        float offset = inVR ? verticalOffset : -verticalOffset;
        Vector3 targetPos = new Vector3(
            playerTransform.position.x,
            playerTransform.position.y + offset,
            playerTransform.position.z);

        playerTransform.position = targetPos;
        playerScaler.transform.localScale = Vector3.one * scale;

        foreach (var go in vrOnlyGOs)
        {
            go.SetActive(inVR);
        }
        foreach (var go in arOnlyGOs)
        {
            go.SetActive(!inVR);
        }

        UpdateCameraClips();

        UpdateCameraLayer(inVR);
    }
    public Transform playerScaler;
    public GameObject[] vrOnlyGOs;
    public GameObject[] arOnlyGOs;

    float nearClipCentreAR = 0.3f, farClipCentreAR = 80000, nearClipCentreVR = 5f, farClipCentreVR = 150000;
    public Camera centreEye;

    private void UpdateCameraClips()
    {
        centreEye.nearClipPlane = inVR ? nearClipCentreVR : nearClipCentreAR;
        centreEye.farClipPlane = inVR ? farClipCentreVR : farClipCentreAR;
    }

    private LayerMask initialLayerMask;
    private LayerMask VRLayerMask;
    [SerializeField] private LayerMask terrainLayer;
    private void SetupCameraLayers()
    {
        initialLayerMask = centreEye.cullingMask;
        VRLayerMask = initialLayerMask - terrainLayer;
    }
    private void UpdateCameraLayer(bool inVR)
    {
        centreEye.cullingMask = inVR ? initialLayerMask : VRLayerMask;
    }
}
