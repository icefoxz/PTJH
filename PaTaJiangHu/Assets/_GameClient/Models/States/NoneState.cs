using System;
using System.Collections;
using AOT.Utls;
using GameClient.System;
using UnityEngine;

namespace GameClient.Models.States
{
    /// <summary>
    /// 无状态, 所有弟子的基本状态
    /// </summary>
    public class NoneState : IDiziState
    {
        public long StartTime { get; }
        public long LastUpdate { get; }
        public string ShortTitle => "无";
        public string Description => "等待中...";
        public string CurrentOccasion => "待着";
        public string CurrentMapName => "原地";
        public string StateLabel => "待候着";
        public TimeSpan CurrentProgressTime => TimeSpan.FromSeconds(SysTime.UnixNow - LastUpdate);
        public DiziStateHandler Handler { get;  }

        public NoneState(DiziStateHandler handler)
        {
            var now = SysTime.UnixNow;
            StartTime = now;
            LastUpdate = now;
            Handler = handler;
        }
    }
}