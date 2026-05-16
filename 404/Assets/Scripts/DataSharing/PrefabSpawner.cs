using Fusion;
using UnityEngine;
using static Unity.Collections.Unicode;

public class PrefabSpawner : SimulationBehaviour, IPlayerJoined
{
    public NetworkObject serverManagerPrefab;
    //creates the test object on join
    public GameObject playerPrefab;
    void IPlayerJoined.PlayerJoined(Fusion.PlayerRef player)
    {
        if (Runner.IsSharedModeMasterClient && ServerManager.Instance == null)
        {
            Runner.Spawn(serverManagerPrefab, Vector3.zero, Quaternion.identity);
        }

        if (player == Runner.LocalPlayer)
        {
            Runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, player);
        }
    }
}
