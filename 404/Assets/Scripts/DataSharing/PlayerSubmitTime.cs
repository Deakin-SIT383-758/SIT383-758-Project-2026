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
        if (!PlayerPrefs.HasKey("PlayerID") || PlayerPrefs.GetInt("PlayerID",0) == 0)
        {
            PlayerPrefs.SetInt("PlayerID", Random.Range(100000, 999999));
            PlayerPrefs.Save();
        }
        return PlayerPrefs.GetInt("PlayerID");
    }
}
