using Fusion;
using UnityEngine;

public class LocalAvatarSync : NetworkBehaviour
{
    public Transform headVisual;
    public Transform leftHandVisual;
    public Transform rightHandVisual;

    // These are set at spawn time by PlayerSpawner, pointing to the XR Origin objects
    private Transform headSource;
    private Transform leftControllerSource;
    private Transform rightControllerSource;
    private Transform leftHandSource;
    private Transform rightHandSource;

    // Called by PlayerSpawner after the avatar spawns to give the XR tracking references
    public void SetSources(Transform head, Transform leftController, Transform rightController, Transform leftHand, Transform rightHand)
    {
        headSource = head;
        leftControllerSource = leftController;
        rightControllerSource = rightController;
        leftHandSource = leftHand;
        rightHandSource = rightHand;
    }

    public override void Spawned()
    {
        // Hide your own avatar visuals as you don't need to see yourself
        // Disable the renderer rather than the object so NetworkTransform keeps working
        if (Object.HasInputAuthority)
        {
            headVisual.GetComponent<Renderer>().enabled = false;
            leftHandVisual.GetComponent<Renderer>().enabled = false;
            rightHandVisual.GetComponent<Renderer>().enabled = false;
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Only the local player should be moving their own avatar
        if (!Object.HasInputAuthority) return;

        // Move the head visual to match the headset position
        if (headSource != null)
        {
            headVisual.position = headSource.position;
            headVisual.rotation = headSource.rotation;

            // Keep the avatar root on the floor, following the head horizontally
            transform.position = new Vector3(headSource.position.x, 0f, headSource.position.z);
        }

        // Left hand, use controller if active, otherwise fall back to hand tracking (L_Wrist)
        bool leftControllerActive = leftControllerSource != null && leftControllerSource.gameObject.activeInHierarchy;

        if (leftControllerActive)
        {
            leftHandVisual.position = leftControllerSource.position;
            leftHandVisual.rotation = leftControllerSource.rotation;
        }
        else if (leftHandSource != null && leftHandSource.position != Vector3.zero)
        {
            leftHandVisual.position = leftHandSource.position;
            leftHandVisual.rotation = leftHandSource.rotation;
        }

        // Right hand, same logic as left
        bool rightControllerActive = rightControllerSource != null && rightControllerSource.gameObject.activeInHierarchy;

        if (rightControllerActive)
        {
            rightHandVisual.position = rightControllerSource.position;
            rightHandVisual.rotation = rightControllerSource.rotation;
        }
        else if (rightHandSource != null && rightHandSource.position != Vector3.zero)
        {
            rightHandVisual.position = rightHandSource.position;
            rightHandVisual.rotation = rightHandSource.rotation;
        }
    }
}