using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class FloatingObjectFollow : MonoBehaviour
{
    [SerializeField] private Transform hmd;
    [SerializeField] private Vector3 posOffset = Vector3.zero;
    public float speed = 0.5f; // adjust speed of movement to target position

    public GameObject targetPrefab; // prefab to show target position
    private GameObject target; // prefab instantiated into scene

    private bool locked = false;

    void Start()
    {
        target = Instantiate(targetPrefab, this.transform.position, Quaternion.identity); // instantiate target location marker
    }

    void LateUpdate()
    {
        if (locked) return; // don't update position if locked

        float frameSpeed = speed * SpeedFunction(Vector3.Distance(transform.position, target.transform.position));
        Debug.Log("Frame speed: " + frameSpeed);
        target.transform.rotation = hmd.rotation; // match target marker to HMD rotation
        Vector3 newPos = hmd.TransformPoint(posOffset); // apply posOffset relative to HMD's transform space
        target.transform.position = newPos; // set new position of target marker

        transform.rotation = Quaternion.Lerp(transform.rotation, target.transform.rotation, frameSpeed); //set rotation to intermediate between current and target rotations
        transform.position = Vector3.Lerp(transform.position, target.transform.position, frameSpeed); // set position to intermediate between current and target position
    }

    float SpeedFunction(float distance)
    {
        speed = Mathf.Pow((0.1f * distance), 2) + 0.2f;
        Debug.Log("Speed: " + speed);
        return speed;
    }

    [ContextMenu("New Target Position")]
    public void NewTargetPosition()
    {
        posOffset = hmd.InverseTransformPoint(transform.position); // set offset to current position in HMD's transform space
    }

    public void ToggleLocked()
    {
        Debug.Log("Lock toggled");
        locked = !locked; // invert current state of locked
        GetComponent<XRGrabInteractable>().enabled = !locked; // disable grabbing while locked
    }
}
