using System;
using Server.Controllers;
using Utls;

namespace _GameClient.Models
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

        public LostState(Dizi dizi, long startTime, DiziActivityLog lastActivityLog)
        {
            StartTime = startTime;
            LastUpdate = startTime;
            Props = new DiziDynamicProps(dizi);
            LastActivityLog = lastActivityLog;
        }
    }
}