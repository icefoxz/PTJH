using System;
using System.Collections;
using System.Collections.Generic;
using AOT.Core.Systems.Coroutines;
using AOT.Utls;
using GameClient.Controllers;
using GameClient.Models;
using GameClient.System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameClient.Modules.Adventure
{
    /// <summary>
    /// 弟子活动的处理器, 用于处理弟子活动的轮询与播放
    /// </summary>
    public class DiziAdventureDelayedPoller : DelayedPoller
    {
        public AdventureActivity Activity { get; } // 实时活动信息
        private ICoroutineInstance Co { get; }
        private DiziAdvController AdvController => Game.Controllers.Get<DiziAdvController>();
        private Adventure_ActivityManager Manager { get; set; }
#if UNITY_EDITOR
        AdventureActivitySpector Spector { get; set; }
#endif
        public event Action<string> OnWaitingAction;
        public event Action<string> OnRecallAction;
        private bool _isReturning;

        public DiziAdventureDelayedPoller(Adventure_ActivityManager manager, AdventureActivity activity) : base(activity.Dizi)
        {
            Manager = manager;
            Activity = activity;
            Co = Game.CoService.RunCo(ActivityPollingUpdate(), Destroy, "Activity", activity.Dizi.Name);

#if UNITY_EDITOR
            Spector = Co.AddComponent<AdventureActivitySpector>();
            Spector.Act = Activity;
#endif
        }
        // 手动触发回程不会触发OnRecallAction
        public void ManualActivityRecall() => _isReturning = true;

        /// <summary>
        /// 轮询更新, 主要处理实时信息
        /// </summary>
        private IEnumerator ActivityPollingUpdate()
        {
            while (Activity.State is AdventureActivity.States.Progress)
            {
                //每秒轮询是否触发故事
                yield return new WaitForSeconds(1);
                AdvController.UpdateActivity(Activity);
            }

            //逻辑到这里, 说明活动已经结束, 但是还没有返回
            if (Activity.State == AdventureActivity.States.Returning && !_isReturning) // 第一次返回会触发
                OnRecallAction?.Invoke(Activity.Dizi.Guid);
            _isReturning = true;

            while (Activity.State is AdventureActivity.States.Returning)
            {
                Activity.SetLastUpdate(SysTime.UnixNow);
                yield return new WaitForSeconds(1);
            }
            OnWaitingAction?.Invoke(Activity.Dizi.Guid);
            StopService();
            Manager.SetWaiting(Activity.Dizi);
        }

        public void StopService()
        {
            Co?.StopCo();
        }
        private void Destroy()
        {
            Co.StopCo();
            Co.Destroy();
        }

        public void ActivityLogUpdate(DiziActivityLog log)
        {
            Activity.AddLog(log);
            Recorder_Update(log);
#if UNITY_EDITOR
            Spector.Stories.Push(log.Occasion);
#endif
        }

#if UNITY_EDITOR
        private class AdventureActivitySpector : MonoBehaviour, IDiziState
        {
            public AdventureActivity Act;
            public long StartTime => Act.StartTime;
            public long LastUpdate => Act.LastUpdate;
            [ShowInInspector] public DiziActivities Activity => Act.Activity;
            [ShowInInspector] public int LastMiles => Act.LastMiles;
            [ShowInInspector] public string ShortTitle => Act.ShortTitle;
            [ShowInInspector] public string Description => Act.Description;
            [ShowInInspector] public string CurrentOccasion => Act.CurrentOccasion;
            [ShowInInspector] public string CurrentMapName => Act.CurrentMapName;
            [ShowInInspector] public string StateLabel => Act.StateLabel;
            [ShowInInspector] public TimeSpan CurrentProgressTime => Act.CurrentProgressTime;
            [ShowInInspector] public string 开始 => SysTime.LocalFromUnixTicks(StartTime).ToString("T");
            [ShowInInspector] public string 最后更新 => SysTime.LocalFromUnixTicks(LastUpdate).ToString("T");
            [ShowInInspector] public string 弟子名 => Act.Dizi.Name;
            [ShowInInspector] public string 结束时间 => Act.EndTime == default ? "未知" : SysTime.LocalFromUnixTicks(Act.EndTime).ToString("T");
            [ShowInInspector] public string 当前时间 => SysTime.Now.ToString("T");
            [ShowInInspector] public Stack<string> 事件经过 => Stories;
            public Stack<string> Stories { get; set; }  = new Stack<string>();
        }
#endif
    }
}