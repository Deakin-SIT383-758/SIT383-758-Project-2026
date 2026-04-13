using System;
using Unity.VisualScripting;
using UnityEngine;

public class FloatingObjectFollow : MonoBehaviour
{
    [SerializeField] private Transform hmd;
    [SerializeField] private Vector3 posOffset = Vector3.zero;
    public float speed = 0.5f; // adjust speed of movement to target position

    public GameObject targetPrefab; // prefab to show target position
    private GameObject target; // prefab instantiated into scene

    void Start()
    {
        target = Instantiate(targetPrefab, this.transform.position, Quaternion.identity); // instantiate target location marker
    }

    void LateUpdate()
    {
        float frameSpeed = speed * Vector3.Distance(transform.position, target.transform.position);
        Debug.Log("Frame speed: " + frameSpeed);
        target.transform.rotation = hmd.rotation; // match target marker to HMD rotation
        Vector3 newPos = hmd.TransformPoint(hmd.localPosition + posOffset); // apply posOffset relative to HMD's transform space
        target.transform.position = newPos; // set new position of target marker

        transform.rotation = Quaternion.Lerp(transform.rotation, target.transform.rotation, frameSpeed); //set rotation to intermediate between current and target rotations
        transform.position = Vector3.Lerp(transform.position, target.transform.position, frameSpeed); // set position to intermediate between current and target position
    }
}
