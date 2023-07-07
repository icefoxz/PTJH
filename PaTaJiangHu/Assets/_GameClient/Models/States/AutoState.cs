using System;
using System.Collections;
using AOT._AOT.Utls;
using GameClient.System;
using UnityEngine;

namespace GameClient.Models.States
{
    /// <summary>
    /// 自动状态,所有弟子的基本状态. 一般会自动转成<see cref="IdleState"/>
    /// </summary>
    public class AutoState : IDiziState
    {
        public long StartTime { get; }
        public long LastUpdate { get; }
        public string ShortTitle => "闲";
        public string Description => "闲置中...";
        public string CurrentOccasion => "山门中";
        public string CurrentMapName => "宗门";
        public string StateLabel => "闲置中";
        public TimeSpan CurrentProgressTime => TimeSpan.FromSeconds(SysTime.UnixNow - LastUpdate);
        public DiziStateHandler Handler { get;  }

        public AutoState(DiziStateHandler handler)
        {
            var now = SysTime.UnixNow;
            StartTime = now;
            LastUpdate = now;
            Handler = handler;
            Game.CoService.RunCo(WaitForTenSecs(), null, handler.DiziName, nameof(AutoState));
        }

        private IEnumerator WaitForTenSecs()
        {
            yield return new WaitForSeconds(10);
            if (Handler.DiziState == this) Handler.StartIdle(SysTime.UnixNow);
        }
    }
}