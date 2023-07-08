﻿using System;
using System.Collections.Generic;
using AOT._AOT.Core;
using AOT._AOT.Utls;
using GameClient.Controllers;
using GameClient.Modules.Adventure;
using GameClient.Modules.DiziM;
using GameClient.System;

namespace GameClient.Models.States
{
    /// <summary>
    /// 闲置状态
    /// </summary>
    public class IdleState : AdvEventPollingHandler, IRewardHandler,IDiziState
    {
        private Dizi Dizi { get; }
        public string ShortTitle => "闲";
        public string Description => "闲置中...";
        public string CurrentMapName => "宗门";
        public string CurrentOccasion => "山门内";
        public string StateLabel => "闲置";
        public TimeSpan CurrentProgressTime => SysTime.CompareUnixNow(LastUpdate);
        public DiziStateHandler Handler { get; }

        public DiziActivityLog CurrentStory { get; private set; }
        public int MessageIndex { get; private set; }
        /// <summary>
        /// 状态是否还在活动,一般停止状态会返回false
        /// </summary>
        public bool IsActive { get; private set; }

        protected override string CoName => "闲置";
        private readonly List<string> _messages = new List<string>();
        public IReadOnlyList<string> Messages => _messages;

        private DiziIdleController IdleController => Game.Controllers.Get<DiziIdleController>();
        private int MessageUpdateSecs => Game.Config.Idle.MessageUpdateSecs;

        public IdleState(Dizi dizi, long startTime, DiziActivityPlayer activityPlayer, DiziStateHandler handler) : base(startTime, dizi.Name,
            activityPlayer)
        {
            Dizi = dizi;
            Handler = handler;
            IsActive = true;
        }

        protected override void PollingUpdate()
        {
            //向闲置控制器轮询事件
            IdleController.QueryIdleStory(Dizi.Guid);
        }

        protected override void OnRegStory(DiziActivityLog story)
        {
            //当有故事注册的时候
            Mode = Modes.Story;
            CurrentStory = story;
            _messageUpdateTime = SysTime.Now;
            UpdateTime(SysTime.UnixNow);
        }

        internal void StopIdleState() => StopService();
        protected override void StopService()
        {
            IsActive = false;
            base.StopService();
            if (CurrentStory == null) return;
            UpdateCurrentStoryMessage(CurrentStory.Messages.Length);
            MessageIndex = 0;
        }

        private void UpdateCurrentStoryMessage(int loop)
        {
            for (var i = 0; i < loop; i++)
            {
                if (MessageIndex >= CurrentStory.Messages.Length) break;
                var message = CurrentStory.Messages[MessageIndex];
                _messages.Add(message);
                Game.MessagingManager.SendParams(EventString.Dizi_Idle_EventMessage, Dizi.Guid, message);
                MessageIndex++;
            }
        }

        private DateTime _messageUpdateTime;

        protected override void StoryUpdate()
        {
            if (CurrentStory == null) //如果没有故事
            {
                MessageIndex = 0; //重置索引
                Mode = Modes.Polling; //返回轮询模式
                return;
            }

            var ts = SysTime.Now - _messageUpdateTime;
            var loops = (int)(ts.TotalSeconds / MessageUpdateSecs);
            UpdateCurrentStoryMessage(loops);

            if (loops > 0) _messageUpdateTime = SysTime.Now;

            if (MessageIndex >= CurrentStory.Messages.Length)
            {
                CurrentStory = null;//故事结束
            }
        }

        public void SetReward(IGameReward reward)
        {
            var con = Game.Controllers.Get<RewardController>();
            //不显示奖励品
            con.SetReward(reward, false);
        }
    }
}