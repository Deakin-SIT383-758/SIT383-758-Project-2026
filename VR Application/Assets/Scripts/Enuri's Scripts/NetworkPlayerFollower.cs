using Fusion;
using UnityEngine;

public class NetworkPlayerFollower : NetworkBehaviour
{
    private Transform xrCamera;

    public override void Spawned()
    {
        // Only the local player's own network object should follow their headset.
        if (Object.HasStateAuthority)
        {
            Camera mainCamera = Camera.main;

            if (mainCamera != null)
            {
                xrCamera = mainCamera.transform;
            }
            else
            {
                Debug.LogWarning("No Main Camera found for NetworkPlayerFollower.");
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority || xrCamera == null)
            return;

        transform.position = xrCamera.position;

        // Keep body facing the same horizontal direction as the headset.
        Vector3 euler = xrCamera.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(0f, euler.y, 0f);
    }
}