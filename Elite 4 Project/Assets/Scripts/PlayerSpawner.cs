using UnityEngine;
using Fusion;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    public NetworkPrefabRef PlayerPrefab;
    public Transform[] SpawnPoints;

    // Static so it persists across spawns and each player gets a different spawn point
    private static int spawnIndex = 0;

    public void PlayerJoined(PlayerRef player)
    {
        // Only spawn an avatar for the local player, not for other players joining
        if (player == Runner.LocalPlayer)
        {
            // Cycle through spawn points so players don't all spawn on top of each other
            Transform point = SpawnPoints[spawnIndex % SpawnPoints.Length];
            spawnIndex++;

            NetworkObject spawnedAvatar = Runner.Spawn(PlayerPrefab, point.position, point.rotation, player);

            Transform head = Camera.main?.transform;

            // Search from the XR Origin rather than using GameObject.Find for each object
            // because FindInChildren can find inactive objects too (hand objects start inactive)
            GameObject xrOrigin = GameObject.Find("XR Origin Hands (XR Rig)");
            if (xrOrigin == null)
            {
                Debug.LogError("PlayerSpawner: Could not find XR Origin Hands (XR Rig)");
                return;
            }

            Transform leftController = FindInChildren(xrOrigin.transform, "Left Controller");
            Transform rightController = FindInChildren(xrOrigin.transform, "Right Controller");

            // L_Wrist and R_Wrist are the joints that actually update during hand tracking
            Transform leftHand = FindInChildren(xrOrigin.transform, "L_Wrist");
            Transform rightHand = FindInChildren(xrOrigin.transform, "R_Wrist");

            // Pass all the tracking references to LocalAvatarSync so it knows what to follow
            LocalAvatarSync sync = spawnedAvatar.GetComponent<LocalAvatarSync>();
            if (sync != null)
                sync.SetSources(head, leftController, rightController, leftHand, rightHand);
            else
                Debug.LogWarning("PlayerSpawner: No LocalAvatarSync found on spawned avatar.");
        }
    }

    // Regular GetComponentsInChildren misses inactive objects, so use this instead
    private Transform FindInChildren(Transform parent, string name)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name) return child;
        }
        return null;
    }
}