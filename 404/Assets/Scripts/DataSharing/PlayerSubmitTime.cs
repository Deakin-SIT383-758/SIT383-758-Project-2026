using Fusion;
using System.Collections;
using UnityEngine;

public class PlayerSubmitTime : NetworkBehaviour
{
    public override void Spawned()
    {
        if (!Object.HasInputAuthority) return;

        StartCoroutine(WaitThenSubmit());
    }

    private IEnumerator WaitThenSubmit()
    {
        while (ServerManager.Instance == null)
        {
            yield return null;
        }

        SubmitMyTime();

        ServerManager.Instance.RPC_RequestEveryoneResubmit();
    }
    public void SubmitMyTime()
    {
        int minutes = PlayerPrefs.GetInt("Minutes", 0);
        int hours = PlayerPrefs.GetInt("Hours", 0);
        string name = PlayerPrefs.GetString("Name", "user");
        int id = GetOrCreatePlayerId();

        ServerManager.Instance.RPC_SubmitTime(id, name, hours, minutes);
    }
    private int GetOrCreatePlayerId()
    {
        if (!PlayerPrefs.HasKey("PlayerID"))
        {
            int newId = Random.Range(100000, 999999);
            PlayerPrefs.SetInt("PlayerID", newId);
            PlayerPrefs.Save();

            Debug.Log("Created new PlayerID: " + newId);
        }

        return PlayerPrefs.GetInt("PlayerID");
    }
}
