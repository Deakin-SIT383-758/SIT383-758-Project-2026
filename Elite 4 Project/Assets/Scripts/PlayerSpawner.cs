using UnityEngine;
using Fusion;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    //public GameObject PlayerPrefab;
    public NetworkPrefabRef PlayerPrefab;
    public Transform[] SpawnPoints;

    private static int spawnIndex = 0;

    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            Transform point = SpawnPoints[spawnIndex % SpawnPoints.Length];
            spawnIndex++;
            Runner.Spawn(PlayerPrefab, point.position, point.rotation, player);
        }
    }
}
