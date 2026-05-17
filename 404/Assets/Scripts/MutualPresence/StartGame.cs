using Fusion;
using UnityEngine;

public class StartGame : MonoBehaviour
{

    public NetworkRunner runner;

    private void Start()
    {
        string roomName = PlayerPrefs.GetString("RoomName", "DefaultRoom");

        var bootstrap = GetComponent<FusionBootstrap>();

        if (bootstrap != null)
        {
            bootstrap.DefaultRoomName = roomName;
            Debug.Log("Room name set before Fusion starts: " + roomName);
        }
    }
}

