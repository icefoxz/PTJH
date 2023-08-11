using System.Collections.Generic;
using GameClient.Modules.DiziM;

namespace GameClient.Modules.Adventure
{
    /// <summary>
    /// 弟子活动碎片信息, 基于<see cref="DiziActivityLog"/>转化成文本碎片信息
    /// </summary>
    public record ActivityFragment
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
}