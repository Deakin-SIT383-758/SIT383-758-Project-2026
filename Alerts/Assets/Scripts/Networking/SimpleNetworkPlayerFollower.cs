using Fusion;
using UnityEngine;

public class SimpleNetworkPlayerFollower : NetworkBehaviour
{
    private Transform cameraTransform;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            FindLocalCamera();
        }
    }

    private void Update()
    {
        if (!Object.HasStateAuthority)
            return;

        if (cameraTransform == null)
        {
            FindLocalCamera();
            return;
        }

        Vector3 cameraPosition = cameraTransform.position;
        Quaternion cameraRotation = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0f);

        transform.SetPositionAndRotation(cameraPosition, cameraRotation);
    }

    private void FindLocalCamera()
    {
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }
}