using Fusion;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class ServerManager : NetworkBehaviour
{
    public static ServerManager Instance;

    public DataBoard leaderboardText;
    [Serializable]
    public struct TimeData : INetworkStruct
    {
        public int id;
        public NetworkString<_32> name;
        public int hours;
        public int minutes;
        public int used;
    }

    [Networked, Capacity(10)]
    public NetworkArray<TimeData> PracticeTimeList { get; }
    public override void Spawned()
    {
        Instance = this;
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_RequestEveryoneResubmit()
    {
        PlayerSubmitTime localSubmitter = FindFirstObjectByType<PlayerSubmitTime>();
        if (localSubmitter != null && localSubmitter.Object.HasInputAuthority)
        {
            localSubmitter.SubmitMyTime();
        }
    }
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SubmitTime(int id, string name, int hours, int minutes)
    {
        for (int i = 0; i < PracticeTimeList.Length; i++)
        {
            TimeData entry = PracticeTimeList.Get(i);
            if (entry.used == 1 && entry.id == id)
            {
                entry.name = name;
                entry.hours = hours;
                entry.minutes = minutes;

                PracticeTimeList.Set(i, entry);
                return;
            }
        }
        for (int i = 0; i < PracticeTimeList.Length; i++)
        {
            TimeData entry = PracticeTimeList.Get(i);

            if (entry.used == 0)
            {
                entry.id = id;
                entry.name = name;
                entry.hours = hours;
                entry.minutes = minutes;
                entry.used = 1;

                PracticeTimeList.Set(i, entry);
                return;
            }
        }
    }
    private void UpdateLeaderboardText()
    {
        if (leaderboardText == null)
        {
            leaderboardText = FindFirstObjectByType<DataBoard>();
        }
        string display = "Hours In Sim\n";
        for (int i = 0; i < PracticeTimeList.Length; i++)
        {
            TimeData entry = PracticeTimeList.Get(i);

            if (entry.used == 0) continue;

            display += entry.name.ToString() + " - " +
                       entry.hours + "H " +
                       entry.minutes + "M\n";
        }
        leaderboardText.text.text = display;
    }
    private void Update()
    {
        UpdateLeaderboardText();
    }

}
