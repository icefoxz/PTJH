using System.Collections.Generic;
using Server.Configs.Adventures;
using Server.Controllers;

/// <summary>
/// 弟子活动信息
/// </summary>
public class ActivityLogWrapper
{
    public Queue<string> Messages { get; set; }
    public string DiziGuid { get; set; }
    public long NowTicks { get; set; }
    public int LastMiles { get; set; }
    public Queue<string> AdjustEvents { get; set; }
    public IGameReward Reward { get; set; }
    public int PlayCount { get; }

    public ActivityLogWrapper(DiziActivityLog diziActivityLog)
    {
        Messages = diziActivityLog.Messages == null
            ? new Queue<string>()
            : new Queue<string>(diziActivityLog.Messages);
        AdjustEvents = diziActivityLog.AdjustEvents == null
            ? new Queue<string>()
            : new Queue<string>(diziActivityLog.AdjustEvents);
        DiziGuid = diziActivityLog.DiziGuid;
        NowTicks = diziActivityLog.NowTicks;
        LastMiles = diziActivityLog.LastMiles;
        Reward = diziActivityLog.Reward;
        PlayCount = ActivityFragment.GetPlayCount(diziActivityLog);
    }
}

/// <summary>
/// 弟子活动碎片信息
/// </summary>
public class ActivityFragment
{
    public string Message { get; }
    public IGameReward Reward { get; }

    public ActivityFragment(string message, IGameReward reward)
    {
        Message = message;
        Reward = reward;
    }
    public static int GetPlayCount(DiziActivityLog log) =>
        log.Messages.Length + log.AdjustEvents.Count + (log.Reward == null ? 0 : 1);
    public static ActivityFragment[] GetFragments(DiziActivityLog log)
    {
        var fragments = new ActivityFragment[GetPlayCount(log)];
        var msgQ = new Queue<string>(log.Messages);
        var adjQ = new Queue<string>(log.AdjustEvents);
        for (int i = 0; i < fragments.Length; i++)
        {
            if(msgQ.Count>0)
            {
                fragments[i] = InstanceFragment(msgQ.Dequeue());
                continue;
            }
            if(adjQ.Count>0)
            {
                fragments[i] = InstanceFragment(adjQ.Dequeue());
                continue;
            }

            fragments[i] = InstanceFragment(log.Reward);
        }
        return fragments;
    }

    public static ActivityFragment InstanceFragment(string message) => new ActivityFragment(message, null);
    public static ActivityFragment InstanceFragment(IGameReward reward) => new ActivityFragment(null, reward);
}