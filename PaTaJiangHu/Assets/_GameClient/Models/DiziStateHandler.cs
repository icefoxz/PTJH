using System;
using Server.Configs.Adventures;
using Server.Controllers;
using Utls;

namespace _GameClient.Models
{
    /// <summary>
    /// 弟子状态处理器
    /// </summary>
    public class DiziStateHandler
    {
        public enum States
        {
            Lost,
            Idle,
            Adventure
        }
        
        public States Current
        {
            get
            {
                var result = CheckNull(LostState) + CheckNull(Idle) + CheckNull(Adventure);
                if (result > 1)
                    XDebug.LogError($"状态异常!活跃状态: Lost:{LostState}, Idle:{Idle}, Adventure:{Adventure}");
                if (LostState != null) return States.Lost;
                if (Idle != null) return States.Idle;
                if (Adventure != null) return States.Adventure;
                throw new NotImplementedException();
                int CheckNull(object obj) => obj != null ? 1 : 0;
            }
        }
        public IdleState Idle { get; private set; }
        public LostState LostState { get; private set; }
        public AutoAdventure Adventure { get; private set; }

        public string ShortTitle { get; private set; } = "闲";
        public string Description { get; private set; }
        public long LastUpdate { get; private set; }
        private Dizi Dizi { get; }

        public DiziStateHandler(Dizi dizi)
        {
            Dizi = dizi;
        }

        // 设定弟子状态短文本
        private void Set(string shortTitle, string description, long lastUpdate)
        {
            ShortTitle = shortTitle;
            Description = description;
            LastUpdate = lastUpdate;
        }

        public override string ToString() => string.Join('|', ShortTitle, Description, LastUpdate);

        public void StartIdle(long startTime)
        {
            Idle = new IdleState(Dizi, startTime);
            Set("闲", "闲置中...", startTime);
        }

        public void StartLost(Dizi dizi, long startTime, DiziActivityLog lastActivityLog)
        {
            LostState = new LostState(dizi, startTime, lastActivityLog);
            Set("失", "失踪了...", startTime);
        }

        public void RestoreFromLost()
        {
            LostState = null;
            Set("归", "失踪回归...", SysTime.UnixNow);
        }

        public void RecallFromAdventure(long now, int lastMile)
        {
            Set("回", "回程中...", now);
            Adventure.Recall(now, lastMile, () => Set("待", "宗门外等待", 0));
        }

        public void StartAdventure(IAutoAdvMap map, long startTime, int messageSecs)
        {
            Adventure = new AutoAdventure(map, startTime, messageSecs, Dizi);
            Adventure.UpdateStoryService.AddListener(() => Set("历", "历练中...", startTime));
        }

        public void FinalizeAdventure()
        {
            Adventure.UpdateStoryService.RemoveAllListeners();
            Set("归", "历练回归...", SysTime.UnixNow);
            Adventure = null;
        }

        public void Terminate(long terminateTime,int lastMile)
        {
            Adventure.UpdateStoryService.RemoveAllListeners();
            Set("断", "历练中断...", terminateTime);
            Adventure.Terminate(terminateTime, lastMile);
        }
    }
}