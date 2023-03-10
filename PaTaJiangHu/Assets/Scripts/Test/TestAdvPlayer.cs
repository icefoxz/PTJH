using Server.Configs.Adventures;
using Server.Controllers;
using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;
using Utls;

public class TestAdvPlayer : MonoBehaviour
{
    private DiziActivityLogHandler logHandler;
    private Queue<DiziActivityLog> Logs;
    [SerializeField]private DiziActivityPlayer player;

    void Start()
    {
        Init();
        InstanceLogs();
    }

    private void Init()
    {
        player.PlayMessage += msg => XDebug.Log(msg);
        player.PlayAdjustment += msg => XDebug.Log($"Adjustment: {msg}");
        player.PlayReward += RewardMessage;
    }

    public void SubScribeLog()
    {
        player.Reg(Logs.Dequeue());
    }

    public void FastForward() => player.FastForward(3);

    private void RewardMessage(IGameReward reward)
    {
        var r = reward as GameReward;
        XDebug.Log($"Reward: Coins = {r.Coins}, Gems = {r.Gems}");
    }

    public void InstanceLogs()
    {
        var story1 = new DiziActivityLog("", 123, 123)
        {
            Messages =
                new string[]
                    { "事件1", "Hello, world!", "Welcome to our game!", "Are you ready to start?", "Good luck!" },
            AdjustEvents = new List<string> { "Adjust event: unlock level 2", "Adjust event: unlock level 3" },
            Reward = new GameReward { Coins = 200, Gems = 50 }
        };
        var story2 = new DiziActivityLog("", 123, 123)
        {
            AdjustEvents = new List<string> { "事件2", "Adjust event: unlock level 2", "Adjust event: unlock level 3" },
            Reward = new GameReward { Coins = 100, Gems = 20 }
        };
        var story3 = new DiziActivityLog("", 123, 123)
        {
            AdjustEvents = new List<string> { "事件3", "Adjust event: unlock level 2" },
            Reward = new GameReward { Coins = 20, Gems = 5 }
        };
        var story4 = new DiziActivityLog("", 123, 123)
        {
            Messages = new string[] { "事件4", "Welcome to our game!", "Are you ready to start?", "Good luck!" },
            Reward = new GameReward { Coins = 50, Gems = 10 }
        };
        var story5 = new DiziActivityLog("", 123, 123)
        {
            Messages = new string[] { "事件5", "Hello, world!" },
            Reward = new GameReward { Coins = 10, Gems = 5 }
        };
        Logs = new Queue<DiziActivityLog>(new[] { story1, story2, story3, story4, story5 });
    }

    private class GameReward : IGameReward
    {
        public IAdvPackage[] Packages { get; }
        public IStacking<IGameItem>[] AllItems { get; }
        public int Coins { get; set; }
        public int Gems { get; set; }
    }
}
