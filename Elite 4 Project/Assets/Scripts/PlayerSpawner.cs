using UnityEngine;
using Fusion;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    public GameObject PlayerPrefab;

    public void PlayerJoined(PlayerRef player)
    {
        Runner.Spawn(PlayerPrefab, new Vector3(0, 1, 3), Quaternion.identity);
    }
}
