using Fusion;
using UnityEngine;

public class SimplePlayerSpawner : MonoBehaviour
{
    [Header("Player Prefab")]
    public NetworkObject playerPrefab;

    [Header("Spawn Settings")]
    public Transform spawnPoint;

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        // In Shared Mode, each client should spawn only their own player marker.
        if (runner.LocalPlayer != player)
            return;

        Vector3 spawnPosition = spawnPoint != null
            ? spawnPoint.position
            : new Vector3(Random.Range(-1f, 1f), 1.6f, Random.Range(-1f, 1f));

        Quaternion spawnRotation = Quaternion.identity;

        runner.Spawn(playerPrefab, spawnPosition, spawnRotation, player);

        Debug.Log("Spawned simple network player for: " + player);
    }
}