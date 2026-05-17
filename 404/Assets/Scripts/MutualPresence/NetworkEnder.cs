using Fusion;
using UnityEngine;

public class NetworkEnder : MonoBehaviour
{
    public NetworkRunner runner;
    public MainMenuUIHandler menu;

    public void Update()
    {
        if (menu.shutdown)
        {
            shutItDown();
            
        }
    }
    public async void shutItDown()
    {
        await runner.Shutdown();
    }
}
