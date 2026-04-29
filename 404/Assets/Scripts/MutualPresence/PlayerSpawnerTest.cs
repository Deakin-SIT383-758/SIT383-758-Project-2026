using Fusion;
using UnityEngine;
//This is for testing that connections are working, this will be heavily modified for the prototype as the prototype progresses.
public class PlayerSpawnerTest : SimulationBehaviour, IPlayerJoined
{
    //creates the test object on join
    public GameObject ConnectionTestObject;
    void IPlayerJoined.PlayerJoined(Fusion.PlayerRef player)
    {
        if (Runner.LocalPlayer ==player)
        {
            Runner.Spawn(ConnectionTestObject, new Vector3(0,1,-5),Quaternion.identity);
        }
    }
}
