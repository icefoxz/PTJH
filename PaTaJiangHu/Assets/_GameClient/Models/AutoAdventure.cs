using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core;
using Server.Configs.Adventures;
using Server.Controllers;
using UnityEngine;
using UnityEngine.Events;
using Utls;

namespace _GameClient.Models
{
    /// <summary>
    /// (自动)挂机历练模型
    /// </summary>
    public class AutoAdventure : AdvPollingHandler, IRewardHandler
    {
        public enum States
        {
            Progress,
            Recall,
            End,
        }

        /// <summary>
        /// 回程秒数
        /// </summary>
        public int JourneyReturnSec => Map.JourneyReturnSec;
        /// <summary>
        /// 当前状态
        /// </summary>
        public States State { get; private set; }
        /// <summary>
        /// 当前里数
        /// </summary>
        public int LastMile { get; private set; }

        private int MessageSecs { get; } //文本展示间隔(秒)
        //private ICoroutineInstance ServiceCo { get; set; }
        private Dizi Dizi { get; }
        public IAutoAdvMap Map { get; }
        private DiziAdvController AdvController => Game.Controllers.Get<DiziAdvController>();
        public IEnumerable<IStacking<IGameItem>> GetItems() => Rewards.SelectMany(i => i.AllItems);

        public IReadOnlyList<IGameReward> Rewards => _rewards;
        private readonly List<IGameReward> _rewards = new List<IGameReward>();
        private List<string> _storyLog = new List<string>();
        public IReadOnlyList<string> StoryLog => _storyLog;

        private IReadOnlyList<DiziAdvLog> Stories => _stories;

        private Queue<string> MessageQueue { get; set; }
        private DateTime messageUpdate;
        private List<DiziAdvLog> _stories = new List<DiziAdvLog>();
        private int _storyIndex;

        public AutoAdventure(IAutoAdvMap map, long startTime, int messageSecs, Dizi dizi)
            : base(startTime,dizi.Name)
        {
            Map = map;
            MessageSecs = messageSecs;
            Dizi = dizi;
            State = States.Progress;
        }

        public UnityEvent UpdateStoryService { get; } = new UnityEvent();

        protected override string CoName => "历练." + State + ".";

        protected override void PollingUpdate()
        {
            if (State is States.End)
            {
                StopService();
                return;
            }

            UpdateServiceName();
            //每秒轮询是否触发故事
            AdvController.CheckMile(Map.Id, Dizi.Guid, (totalMile, isAdvEnd) =>
            {
                LastMile = totalMile;
                if (isAdvEnd)
                    AdvController.AdventureRecall(Dizi.Guid);
            });
            UpdateStoryService?.Invoke();
        }

        protected override void StoryUpdate()
        {
            UpdateServiceName();
            //确保信息是根据配置的秒数播放
            var elapsedSecs = (SysTime.Now - messageUpdate).TotalSeconds;
            var times = elapsedSecs / MessageSecs;
            //如果当前过了一定程度的秒数,将一次更新更多历练信息(因为玩家不会死盯着一个弟子看历练)
            for (int i = 0; i < times; i++)
            {
                //如果没有故事, 返回轮询模式
                if (Stories.Count - 1 == _storyIndex && MessageQueue?.Count == 0)
                {
                    Mode = Modes.Polling; //循环结束自动回到轮询故事
                    break;
                }

                //如果信息为空,尝从故事中获取信息
                if (MessageQueue == null || MessageQueue.Count == 0)
                {
                    var st = Stories[_storyIndex];
                    _storyIndex++;
                    MessageQueue = new Queue<string>(st.Messages);
                }

                if (!MessageQueue.Any())
                {
                    Mode = Modes.Polling;
                    break;
                }

                UpdateStoryLog(false);
            }
            messageUpdate = SysTime.Now;
            UpdateStoryService?.Invoke();
        }

        //更新故事信息
        private void UpdateStoryLog(bool clearStory)
        {
            if (MessageQueue == null || MessageQueue.Count == 0) return;
            do UpdateLog();
            while (clearStory && MessageQueue.Count > 0);

            void UpdateLog()
            {
                var lastMessage = MessageQueue.Dequeue();
                var isStoryEnd = MessageQueue.Count == 0; //是否当前的故事结束
                _storyLog.Add(lastMessage);
                Game.MessagingManager.SendParams(EventString.Dizi_Adv_EventMessage,
                    Dizi.Guid, lastMessage, isStoryEnd);
            }
        }

        //更新冒险位置
        private void UpdateTime(long updatedTicks, int updatedMiles)
        {
            UpdateTime(updatedTicks);
            LastMile = updatedMiles;
        }

        //注册故事,准备展示
        internal override void RegStory(DiziAdvLog story)
        {
            if (Mode == Modes.Story)
            {
                //如果还有故事未播放完毕,清除故事
                UpdateStoryLog(true);
            }
            Mode = Modes.Story;
            UpdateTime(story.NowTicks, story.LastMiles);//更新发生地点 & 最新里数
            _stories.Add(story);
        }

        internal void Recall(long now, int lastMile, Action recallAction)
        {
            const string RecallText = "回程中";
            if (Mode == Modes.Story)
            {
                //如果还有故事未播放完毕,清除故事
                UpdateStoryLog(true);
            }
            UpdateTime(now, lastMile);
            State = States.Recall;
            UpdateServiceName(RecallText);
            Game.CoService.RunCo(ReturnFromAdventure(), null, Dizi.Name).name = RecallText;

            IEnumerator ReturnFromAdventure()
            {
                yield return new WaitUntil(() => Mode == Modes.Polling);
                var returnTime = SysTime.Now;
                yield return new WaitUntil(() => (SysTime.Now - returnTime).TotalSeconds >= JourneyReturnSec);
                UpdateServiceName("已回到山门.");
                var advLog = new DiziAdvLog(Dizi.Guid, SysTime.UnixNow, LastMile);
                advLog.SetMessages(new[] { $"{Dizi.Name}已回到山门!" });
                RegStory(advLog);
                yield return new WaitForSeconds(1);
                recallAction?.Invoke();
                State = States.End;
                Game.MessagingManager.Send(EventString.Dizi_Adv_End, Dizi.Guid);
            }
        }

        void IRewardHandler.SetReward(IGameReward reward)
        {
            _rewards.Add(reward);
        }
    }
}