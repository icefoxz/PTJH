using System.Collections.Generic;
using GameClient.Modules.DiziM;

namespace GameClient.Modules.Adventure
{
    /// <summary>
    /// 弟子活动信息, 记录当前弟子活动的细节
    /// </summary>
    public record DiziActivityLog : IRewardHandler
    {
        public string[] Messages { get; set; }
        public string DiziGuid { get; set; }
        public string Occasion { get; set; }
        public long OccurredTime { get; set; }
        public int LastMiles { get; set; }
        public List<string> AdjustEvents { get; set; }
        public IGameReward Reward { get; set; }

        public DiziActivityLog(string diziGuid, long occurredTime, int lastMiles, string occasion)
        {
            DiziGuid = diziGuid;
            OccurredTime = occurredTime;
            LastMiles = lastMiles;
            Occasion = occasion;
            AdjustEvents = new List<string>();
        }

        public void SetMessages(string[] messages)
        {
            Messages = messages;
        }

        public void SetReward(IGameReward reward)
        {
            Reward = reward;
        }

        public void AddAdjustmentInfo(string[] adjustMessages)
        {
            AdjustEvents.AddRange(adjustMessages);
        }
    }
}