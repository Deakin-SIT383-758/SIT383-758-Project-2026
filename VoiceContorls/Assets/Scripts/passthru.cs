using UnityEngine;
using Meta.XR.MRUtilityKit;

public class passthru : MonoBehaviour
{
    [SerializeField] private OVRPassthroughLayer passthroughLayer;
    private bool isPassthrough = true;

    public Transform rayStartpoint;
    public float rayLength = 5;
    public MRUKAnchor.SceneLabels lableFilter;
    public GameObject Stuckup;

    void Start()
    {
        if (passthroughLayer != null)
        {
            passthroughLayer.enabled = true;
            isPassthrough = true;
        }
    }

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.RawButton.A))
        {
            TogglePassthrough();
        }
        if (OVRInput.GetDown(OVRInput.RawButton.B))
        {
            Ray ray = new Ray(rayStartpoint.position, rayStartpoint.forward);
            MRUKRoom room = MRUK.Instance.GetCurrentRoom();

            bool hasHit = room.Raycast(ray, rayLength, LabelFilter.FromEnum(lableFilter), out RaycastHit hit, out MRUKAnchor anchor);

            if (hasHit)
            {
                Vector3 hitpoint = hit.point;
                Vector3 hitNormal = hit.normal;

                Stuckup.transform.position = hitpoint;
                Stuckup.transform.rotation = Quaternion.LookRotation(hitNormal);
            }
        }
    }
    private void TogglePassthrough()
    {
        isPassthrough = !isPassthrough;
        passthroughLayer.enabled = isPassthrough;
    }
}
