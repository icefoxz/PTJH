using System.Collections.Generic;
using AOT.Core;
using GameClient.Controllers;
using GameClient.Modules.Adventure;
using GameClient.System;

namespace GameClient.Models
{
    /// <summary>
    /// 弟子历练, 用于管理历练相关信息
    /// </summary>
    public class Adventure_ActivityManager : DiziActivityModel<DiziAdventureDelayedPoller>
    {
        protected override string LogPrefix { get; } = "历练";
        private DiziAdvController AdventureController => Game.Controllers.Get<DiziAdvController>();

        public void ActivityStart(AdventureActivity activity)
        {
            var poller = new DiziAdventureDelayedPoller(this, activity);
            poller.OnRecallAction += OnRecall;
            Add(activity.Dizi.Guid, poller);
            SendEvent(EventString.Dizi_Adv_Start, activity.Dizi.Guid); 
            SendEvent(EventString.Dizi_State_Update, activity.Dizi.Guid); 
        }

        public void ActivityRemove(string guid)
        {
            var poller = GetPoller(guid);
            poller.StopService();
            Remove(guid);
            SendEvent(EventString.Dizi_Adv_Finalize, guid);
        }

        public void ActivityUpdate(DiziActivityLog log)
        {
            var poller = GetPoller(log.DiziGuid);
            poller.ActivityLogUpdate(log);
            SendEvent(EventString.Dizi_Adv_EventMessage, log.DiziGuid);
            SendEvent(EventString.Dizi_State_Update, log.DiziGuid); 
        }

        public void SetWaiting(Dizi dizi)
        {
            SendEvent(EventString.Dizi_Adv_Waiting, dizi.Guid);
            SendEvent(EventString.Dizi_State_Update, dizi.Guid); 
        }

        public AdventureActivity GetActivity(string guid)=> GetPoller(guid).Activity;
        public IReadOnlyList<ActivityFragment> GetFragments(string guid) => GetPoller(guid).Fragments;

        public void ActivityRecall(string guid)
        {
            var poller = GetPoller(guid);
            OnRecall(guid);
            poller.ManualActivityRecall();
        }
        private void OnRecall(string guid)
        {
            var poller = GetPoller(guid);
            poller.OnRecallAction -= OnRecall;
            poller.OnWaitingAction += OnWaiting;
            SendEvent(EventString.Dizi_Adv_Recall, guid);
            SendEvent(EventString.Dizi_State_Update, guid); 
        }

        private void OnWaiting(string guid)
        {
            var poller = GetPoller(guid);
            poller.OnWaitingAction -= OnWaiting;
            AdventureController.AdventureReturn(guid);
        }

        public void ActivityReturn(string guid)
        {
            SendEvent(EventString.Dizi_Adv_Waiting, guid);
            SendEvent(EventString.Dizi_State_Update, guid); 
        }
    }
}