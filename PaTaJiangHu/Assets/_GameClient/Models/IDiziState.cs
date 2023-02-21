using System;
using System.Collections.Generic;
using Server.Controllers;
using Utls;

namespace _GameClient.Models
{
    public class DiziIdleState : AdvPollingHandler
    {
        private Dizi Dizi { get; }
        protected override string DiziName => Dizi.Name;
        public Queue<DiziAdvLog> Stories { get; private set; } = new Queue<DiziAdvLog>();
        public DiziAdvLog CurrentStory { get; private set; }
        public int MessageIndex { get; private set; }

        private readonly List<string> _messages = new List<string>();
        public IReadOnlyList<string> Messages => _messages;

        private DiziIdleController IdleController => Game.Controllers.Get<DiziIdleController>();
        private int MessageUpdateSecs => IdleController.IdleCfg.MessageUpdateSecs;

        public DiziIdleState(Dizi dizi,long startTime) : base(startTime)
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
            Stories.Enqueue(story);
        }

        protected override void StopService()
        {
            base.StopService();
            UpdateCurrentStoryMessage(CurrentStory.Messages.Length, null);

            MessageIndex = 0;
            //while (Stories.Count>0)
            //{
            //    var s
            //}
        }

        private void UpdateCurrentStoryMessage(int loop, Action<string> onMessageUpdate)
        {
            for (var i = 0; i < loop; i++)
            {
                if (MessageIndex >= CurrentStory.Messages.Length) break;
                var message = CurrentStory.Messages[MessageIndex];
                _messages.Add(message);
                onMessageUpdate?.Invoke(message);
                MessageIndex++;
            }
        }

        private DateTime _messageUpdateTime;

        protected override void StoryUpdate()
        {
            if (CurrentStory == null)//如果没有故事
            {
                if (Stories.Count == 0)//如果当前所有故事已清空
                {
                    Mode = Modes.Polling;//返回轮询模式
                    return;
                }
                CurrentStory = Stories.Dequeue();//获取故事
                MessageIndex = 0;//重置索引
                _messageUpdateTime = SysTime.Now;
            }
            if (_messageUpdateTime == default) return;
            var ts = SysTime.Now - _messageUpdateTime;
            var loops = (int)(ts.TotalSeconds / MessageUpdateSecs);
            UpdateCurrentStoryMessage(loops,
                msg => Game.MessagingManager.SendParams(EventString.Dizi_Idle_EventMessage, Dizi.Guid, msg));

            if (MessageIndex >= CurrentStory.Messages.Length) return;
            _messageUpdateTime = SysTime.Now;
        }
    }
}