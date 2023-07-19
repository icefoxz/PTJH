using System;
using AOT.Utls;
using GameClient.Controllers;

namespace GameClient.Models.States
{
    /// <summary>
    /// 失踪状态
    /// </summary>
    public class LostState : IDiziState
    {
        public long StartTime { get; }
        public long LastUpdate { get; private set; }
        public DiziDynamicProps Props { get; }
        public DiziActivityLog LastActivityLog { get; }
        public string ShortTitle => "失";
        public string Description => "失踪了...";
        public string CurrentOccasion => "未知";
        public string CurrentMapName => "未知";
        public string StateLabel => "失踪";
        public TimeSpan CurrentProgressTime => SysTime.CompareUnixNow(LastUpdate);
        public DiziStateHandler Handler { get; }

        public LostState(Dizi dizi, long startTime, DiziActivityLog lastActivityLog, DiziStateHandler handler)
        {
            StartTime = startTime;
            LastUpdate = startTime;
            Props = new DiziDynamicProps(dizi);
            LastActivityLog = lastActivityLog;
            Handler = handler;
        }
    }
}