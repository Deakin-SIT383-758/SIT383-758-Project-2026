using Fusion;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Hands;

public class LocalAvatarSync : NetworkBehaviour
{
    public Transform headVisual;
    public Transform leftHandVisual;
    public Transform rightHandVisual;

    private Transform headSource;
    private Transform leftControllerSource;
    private Transform rightControllerSource;
    private Transform leftHandSource;
    private Transform rightHandSource;

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
        if (Object.HasInputAuthority)
        {
            // Hide renderers only, keep objects active for NetworkTransform to work
            headVisual.GetComponent<Renderer>().enabled = false;
            leftHandVisual.GetComponent<Renderer>().enabled = false;
            rightHandVisual.GetComponent<Renderer>().enabled = false;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasInputAuthority) return;

        if (headSource != null)
        {
            headVisual.position = headSource.position;
            headVisual.rotation = headSource.rotation;

            transform.position = new Vector3(headSource.position.x, 0f, headSource.position.z);
        }

        // Left hand
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

        // Right hand
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