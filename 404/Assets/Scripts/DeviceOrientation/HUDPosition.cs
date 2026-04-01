using UnityEngine;

public class HUDPosition : MonoBehaviour
{
    [SerializeField] private Transform hmd;
    [SerializeField] private Vector3 posOffset = Vector3.zero;

    void LateUpdate()
    {
        transform.rotation = hmd.rotation; // match HMD rotation
    }
}
