using UnityEngine;

public class HUDPosition : MonoBehaviour
{
    [SerializeField] private Transform hmd;
    [SerializeField] private Vector3 posOffset = Vector3.zero;

    void LateUpdate()
    {
        transform.rotation = hmd.rotation; // match HMD rotation
        Vector3 newPos = hmd.TransformPoint(hmd.localPosition + posOffset); // apply posOffset relative to HMD's transform space
        transform.position = newPos; // set new position
    }
}
