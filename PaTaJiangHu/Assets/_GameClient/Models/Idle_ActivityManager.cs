using AOT.Utls;
using GameClient.Modules.Adventure;
using GameClient.Modules.DiziM;
using GameClient.SoScripts.Adventures;
using System.Collections.Generic;
using System;
using System.Linq;
using AOT.Core;

namespace GameClient.Models
{
    /// <summary>
    /// 弟子闲置状态管理器
    /// </summary>
    public class Idle_ActivityManager : DiziActivityModel<DiziIdleDelayedPoller>
    {
        protected override string LogPrefix { get; } = "闲置";

        internal void ActivityStart(Dizi dizi, IdleMapSo map, long startTime)
        {
            var idle = new IdleActivity(map, startTime, dizi);
            var poller = new DiziIdleDelayedPoller(idle);
            dizi.SetState(idle);
            Add(dizi.Guid, poller);
            SendEvent(EventString.Dizi_Idle_Start, dizi.Guid);
            SendEvent(EventString.Dizi_State_Update, dizi.Guid);

        }
        internal void ActivityUpdate(Dizi dizi, DiziActivityLog activityLog)
        {
            var poller = GetPoller(dizi.Guid);
            poller.Update(activityLog);
            SendEvent(EventString.Dizi_Idle_EventMessage, dizi.Guid);
        }
        internal void ActivityEnd(Dizi dizi, long endTime)
        {
            var poller = GetPoller(dizi.Guid);
            poller.SetEnd(endTime);
            Remove(dizi.Guid);
            SendEvent(EventString.Dizi_Idle_Stop, dizi.Guid);
            SendEvent(EventString.Dizi_State_Update, dizi.Guid);
        }

        public IdleActivity GetActivity(string guid) => GetPoller(guid).Activity;

        public IReadOnlyList<ActivityFragment> GetFragments(string guid)
        {
            var poller = GetPoller(guid);
            return poller?.Fragments;
        }
    }
    public record IdleActivity : IDiziState
    {
        public enum States
        {
            Active,
            End
        }
        #region IDiziState
        public DiziActivities Activity { get; } = DiziActivities.Idle;
        public long StartTime { get; private set; }
        public long LastUpdate { get; private set; }
        public string ShortTitle => "闲";
        public string Description => "闲置中...";
        public string CurrentOccasion => "山门中";
        public string StateLabel => "闲置";
        public string CurrentMapName => Map.Name;
        public TimeSpan CurrentProgressTime => SysTime.CompareUnixNow(LastUpdate);
        #endregion
        public long EndTime { get; private set; }
        public int LastMiles { get; private set; } = 0;// 发呆状态不会有里程
        public States State => EndTime > 0 && LastUpdate >= EndTime ? States.End : States.Active;
        internal IdleMapSo Map { get; private set; }
        public Dizi Dizi { get; private set; }
        public IReadOnlyList<DiziActivityLog> Logs => _logs;
        public IEnumerable<IGameReward> Rewards => Logs.Select(l => l.Reward);

        private List<DiziActivityLog> _logs = new List<DiziActivityLog>();

       internal IdleActivity(IdleMapSo map, long startTime, Dizi dizi)
       {
            Map = map;
            StartTime = startTime;
            LastUpdate = startTime;
            Dizi = dizi;
       }

       public void AddLog(long lastUpdate, DiziActivityLog log)
       {
           LastUpdate = lastUpdate;
           _logs.Add(log);
       }
       public void SetEnd(long endTime)
       {
           LastUpdate = endTime;
           EndTime = endTime;
       }
    }
}