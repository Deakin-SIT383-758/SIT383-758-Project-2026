using UnityEngine;
using Meta.XR.MRUtilityKit;
using static OVRAnchor;

public class TackerManager : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    public void OnTrackableAdded(MRUKTrackable trackable)
    {
        if (trackable.TrackableType != TrackableType.QRCode)
        {
            return;
        }

        GameObject go = Instantiate(prefab, trackable.transform);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
    }
}
