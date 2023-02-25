using System;
using System.Collections.Generic;
using Server.Controllers;
using Utls;

namespace _GameClient.Models
{
    public class IdleState : AdvPollingHandler
    {
        private Dizi Dizi { get; }
        
        public DiziAdvLog CurrentStory { get; private set; }
        public int MessageIndex { get; private set; }

        private readonly List<string> _messages = new List<string>();
        public IReadOnlyList<string> Messages => _messages;

        private DiziIdleController IdleController => Game.Controllers.Get<DiziIdleController>();
        private int MessageUpdateSecs => Game.Config.Idle.MessageUpdateSecs;

        public IdleState(Dizi dizi,long startTime) : base(startTime,dizi.Name)
        {
            Dizi = dizi;
        }

        protected override void PollingUpdate()
        {
            //向闲置控制器轮询事件
            IdleController.QueryIdleStory(Dizi.Guid);
        }

        internal override void RegStory(DiziAdvLog story)
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
    }
}