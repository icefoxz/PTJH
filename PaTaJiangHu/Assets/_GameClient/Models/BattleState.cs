using System;
using Utls;

namespace _GameClient.Models
{
    /// <summary>
    /// 战斗事件
    /// </summary>
    public class BattleState : IDiziState
    {
        public long StartTime { get; }
        public long LastUpdate { get; }
        public string ShortTitle => "战";
        public string Description => "战斗中...";
        public string CurrentOccasion { get; }
        public string CurrentMapName { get; }
        public string StateLabel { get; }
        public TimeSpan CurrentProgressTime => SysTime.CompareUnixNow(LastUpdate);

        public BattleState(long startTime, string currentOccasion, string currentMapName)
        {
            StartTime = startTime;
            LastUpdate = startTime;
            CurrentOccasion = currentOccasion;
            CurrentMapName = currentMapName;
            StateLabel = currentOccasion;
        }
    }
}