using UnityEngine;
using Fusion;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{    
    public NetworkPrefabRef PlayerPrefab;
    public Transform[] SpawnPoints;

    private static int spawnIndex = 0;

    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            Transform point = SpawnPoints[spawnIndex % SpawnPoints.Length];
            spawnIndex++;

            NetworkObject spawnedAvatar = Runner.Spawn(PlayerPrefab, point.position, point.rotation, player);

            Transform head = Camera.main?.transform;

            // Find XR Origin first
            GameObject xrOrigin = GameObject.Find("XR Origin Hands (XR Rig)");
            if (xrOrigin == null)
            {
                Debug.LogError("PlayerSpawner: Could not find XR Origin Hands (XR Rig)");
                return;
            }

            // Use FindInChildren helper which searches inactive objects too
            Transform leftController = FindInChildren(xrOrigin.transform, "Left Controller");
            Transform rightController = FindInChildren(xrOrigin.transform, "Right Controller");

            Transform leftHand = FindInChildren(xrOrigin.transform, "L_Wrist");
            Transform rightHand = FindInChildren(xrOrigin.transform, "R_Wrist");

            Debug.Log($"Head: {head != null}, LeftController: {leftController != null}, RightController: {rightController != null}, LeftHand: {leftHand != null}, RightHand: {rightHand != null}");

            LocalAvatarSync sync = spawnedAvatar.GetComponent<LocalAvatarSync>();
            if (sync != null)
            {
                sync.SetSources(head, leftController, rightController, leftHand, rightHand);
            }
            else
            {
                Debug.LogWarning("PlayerSpawner: No LocalAvatarSync found on spawned avatar.");
            }
        }
    }

    // Searches all children including inactive ones
    private Transform FindInChildren(Transform parent, string name)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name) return child;
        }
        return null;
    }
}
