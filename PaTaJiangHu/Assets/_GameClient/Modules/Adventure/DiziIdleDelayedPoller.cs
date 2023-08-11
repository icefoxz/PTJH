using System.Collections;
using AOT.Core.Systems.Coroutines;
using GameClient.Controllers;
using GameClient.Models;
using GameClient.System;
using UnityEngine;

namespace GameClient.Modules.Adventure
{
    public class DiziIdleDelayedPoller : DelayedPoller
    {
        public IdleActivity Activity { get; } // 实时活动信息
        private ICoroutineInstance Co { get; }
        private DiziIdleController IdleController => Game.Controllers.Get<DiziIdleController>();
        public DiziIdleDelayedPoller(IdleActivity activity) : base(activity.Dizi)
        {
            Activity = activity;
            Co = Game.CoService.RunCo(ActivityPollingUpdate(), Destroy, "Activity", activity.Dizi.Name);
        }

        private IEnumerator ActivityPollingUpdate()
        {
            while (Activity.State is IdleActivity.States.Active)
            {
                yield return new WaitForSeconds(1);
                if (Activity.State is IdleActivity.States.End) break;
                IdleController.QueryIdleStory(Activity.Dizi.Guid);
            }
        }

        private void Destroy()
        {
            Co.StopCo();
            Co.Destroy();
        }

        public void SetEnd(long endTime)
        {
            Activity.SetEnd(endTime);
            Recorder_Stop();
        }

        public void Update(DiziActivityLog log)
        {
            Activity.AddLog(log.OccurredTime, log);
            Recorder_Update(log);
        }
    }
}