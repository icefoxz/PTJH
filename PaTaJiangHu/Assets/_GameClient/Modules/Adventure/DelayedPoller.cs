using System.Collections.Generic;
using AOT.Core;
using GameClient.Models;
using GameClient.Modules.DiziM;
using GameClient.System;

namespace GameClient.Modules.Adventure
{
    /// <summary>
    /// 活动处理器, 主要是记录弟子活动和实现延迟记录
    /// </summary>
    public abstract class DelayedPoller
    {
        private ActivityDelayedPlayer DelayedPlayer { get; } // 延迟记录器
        public IReadOnlyList<ActivityFragment> Fragments => DelayedPlayer.ActivityHistory; // 历史记录
        private Dizi Dizi { get; }
        protected DelayedPoller(Dizi dizi)
        {
            Dizi = dizi;
            DelayedPlayer = new ActivityDelayedPlayer(
                coName: "活动记录器", 
                coParent: dizi.Name,
                messageFunc: ActivityFragment.InstanceFragment,
                rewardFunc: ActivityFragment.InstanceFragment); //记录历练信息
            DelayedPlayer.OnRewardAction += Recorder_OnRewardAction;
            DelayedPlayer.OnMessageAction += Recorder_OnMessageAction;
            DelayedPlayer.OnAdjustmentAction += Recorder_OnAdjustmentAction;
        }

        protected void Recorder_Update(DiziActivityLog log) => DelayedPlayer.Reg(log);//注册新的log, 然后根据时间转化成为信息碎片

        private void Recorder_OnAdjustmentAction(string obj) =>
            Game.MessagingManager.SendParams(EventString.Dizi_Activity_Adjust, Dizi.Guid, obj);

        private void Recorder_OnMessageAction(string obj) =>
            Game.MessagingManager.SendParams(EventString.Dizi_Activity_Message, Dizi.Guid, obj);

        private void Recorder_OnRewardAction(IGameReward obj) =>
            Game.MessagingManager.SendParams(EventString.Dizi_Activity_Reward, Dizi.Guid);

        protected void Recorder_Stop()
        {
            DelayedPlayer.OnRewardAction -= Recorder_OnRewardAction;
            DelayedPlayer.OnMessageAction -= Recorder_OnMessageAction;
            DelayedPlayer.OnAdjustmentAction -= Recorder_OnAdjustmentAction;
            DelayedPlayer.Stop();
        }
    }
}