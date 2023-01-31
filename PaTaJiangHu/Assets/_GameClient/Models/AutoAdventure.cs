using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core;
using Server.Configs.Adventures;
using Server.Controllers;
using Systems.Coroutines;
using UnityEngine;
using Utls;

namespace _GameClient.Models
{
    /// <summary>
    /// (自动)挂机历练模型
    /// </summary>
    public class AutoAdventure : IRewardReceiver
    {
        public enum States
        {
            Progress,
            Recall,
            End,
        }
        private enum Modes
        {
            Polling,
            Story
        }
        /// <summary>
        /// 回程秒数
        /// </summary>
        public int JourneyReturnSec { get; }
        /// <summary>
        /// 当前状态
        /// </summary>
        public States State { get; private set; }
        private Modes Mode { get; set; }//内部使用的状态模式
        /// <summary>
        /// 开始时间
        /// </summary>
        public long StartTime { get; }
        /// <summary>
        /// 上次更新时间
        /// </summary>
        public long LastUpdate { get; private set; }
        /// <summary>
        /// 当前里数
        /// </summary>
        public int LastMile { get; private set; }

        private int MessageSecs { get; } //文本展示间隔(秒)
        private ICoroutineInstance ServiceCo { get; set; }
        private Dizi Dizi { get; }

        private DiziAdvController AdvController => Game.Controllers.Get<DiziAdvController>();
        public IEnumerable<IStacking<IGameItem>> GetItems() => Rewards.SelectMany(i => i.AllItems);

        public Equipment Equipment { get; }
        public IReadOnlyList<IGameReward> Rewards => _rewards;
        private readonly List<IGameReward> _rewards = new List<IGameReward>();
        private List<string> _storyLog = new List<string>();
        public IReadOnlyList<string> StoryLog => _storyLog;
        private Queue<DiziAdvLog> Stories { get; set; } = new Queue<DiziAdvLog>();

        public AutoAdventure(long startTime, int journeyReturnSec, int messageSecs, Dizi dizi)
        {
            MessageSecs = messageSecs;
            JourneyReturnSec = journeyReturnSec;
            StartTime = startTime;
            LastUpdate = startTime;
            Dizi = dizi;
            State = States.Progress;
            StartService();
        }

        private string CoName => ServiceCo?.GetInstanceID() + "历练." + State + ".";
        private void SetServiceName(string serviceName) => ServiceCo.name = CoName + serviceName;
        private void StartService()
        {
            ServiceCo = Game.CoService.RunCo(UpdateEverySecond(), () => ServiceCo = null, Dizi.Name);
            IEnumerator UpdateEverySecond()
            {
                yield return new WaitForEndOfFrame();
                while (State is not States.End)
                {
                    SetServiceName(Mode.ToString());
                    switch (Mode)
                    {
                        case Modes.Polling:
                        {
                            //每秒轮询是否触发故事
                            AdvController.CheckMile(Dizi.Guid, (totalMile, isAdvEnd) =>
                            {
                                LastMile = totalMile;
                                if (isAdvEnd)
                                    AdvController.AdventureRecall(Dizi.Guid);
                            });
                            yield return new WaitForSeconds(1);
                            break;
                        }
                        case Modes.Story:
                        {
                            //当进入故事模式
                            //循环播放直到播放队列空
                            SetServiceName("Story");
                            while (Stories.Count > 0)
                            {
                                var st = Stories.Dequeue();
                                var messages = new Queue<string>(st.Messages);
                                while (messages.Count > 0)
                                {
                                    var message = messages.Dequeue();
                                    var isStoryEnd = messages.Count == 0;
                                    _storyLog.Add(message);
                                    Game.MessagingManager.SendParams(
                                        EventString.Dizi_Adv_EventMessage, Dizi.Guid,
                                        message, isStoryEnd);
                                    yield return new WaitForSeconds(MessageSecs);
                                }
                            }
                            Mode = Modes.Polling;//循环结束自动回到轮询故事
                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        //更新冒险位置
        private void UpdateTime(long updatedTicks, int updatedMiles)
        {
            LastUpdate = updatedTicks;
            LastMile = updatedMiles;
        }

        internal void RegStory(DiziAdvLog story)
        {
            Mode = Modes.Story;
            UpdateTime(story.NowTicks, story.LastMiles);//更新发生地点 & 最新里数
            Stories.Enqueue(story);
        }

        internal void Recall(long now,int lastMile)
        {
            const string RecallText = "回程中";
            UpdateTime(now,lastMile);
            State = States.Recall;
            SetServiceName(RecallText);
            Game.CoService.RunCo(ReturnFromAdventure(), null, Dizi.Name).name = RecallText;

            IEnumerator ReturnFromAdventure()
            {
                yield return new WaitUntil(() => Mode == Modes.Polling);
                yield return new WaitForSeconds(JourneyReturnSec);
                SetServiceName("已回到山门.");
                RegStory(new DiziAdvLog(new[] { $"{Dizi.Name}已回到山门!" }, Dizi.Guid, SysTime.UnixNow, LastMile));
                yield return new WaitForSeconds(1);
                State = States.End;
                Game.MessagingManager.Send(EventString.Dizi_Adv_End, Dizi.Guid);
            }
        }

        void IRewardReceiver.SetReward(IGameReward reward) => _rewards.Add(reward);
    }

    public class Equipment
    {

    }
    public class DiziBag
    {
        private IStacking<IGameItem>[] _items;
        public IStacking<IGameItem>[] Items => _items;

        public DiziBag(int length)
        {
            _items = new IStacking<IGameItem>[length];
        }

        public DiziBag(IStacking<IGameItem>[] items)
        {
            _items = items;
        }

    }
}