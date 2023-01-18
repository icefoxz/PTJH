using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using Server.Controllers;
using UnityEngine;

namespace _GameClient.Models
{
    /// <summary>
    /// (自动)挂机历练模型
    /// </summary>
    public class AutoAdventure
    {
        public enum States
        {
            None,
            Progress,
            Recall,
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
        //private int AdventureCoId { get; set; } //历练CoroutineId
        //private int MessageCoId { get; set; } //信息CoroutineId
        private int ServiceCoId { get; set; }
        private Dizi Dizi { get; }
        private DiziBag Bag { get; }
        public IStacking<IGameItem>[] BagItems => Bag.Items;
        public Equipment Equipment { get; }
        private DiziAdvController AdvController => Game.Controllers.Get<DiziAdvController>();
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

        private void StartService()
        {
            ServiceCoId = Game.CoService.RunCo(UpdateEverySecond(), () => ServiceCoId = 0);
            IEnumerator UpdateEverySecond()
            {
                yield return new WaitForEndOfFrame();
                while (State == States.Progress)
                {
                    switch (Mode)
                    {
                        case Modes.Polling:
                        {
                            //每秒轮询是否触发故事
                            AdvController.CheckMile(Dizi.Guid);
                            yield return new WaitForSeconds(1);
                            break;
                        }
                        case Modes.Story:
                        {
                            //当进入故事模式
                            //循环播放直到播放队列空
                            while (Stories.Count > 0)
                            {
                                var st = Stories.Dequeue();
                                var messages = new Queue<string>(st.Messages);
                                while (messages.Count > 0)
                                {
                                    var message = messages.Dequeue();
                                    var isStoryEnd = messages.Count == 0;
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

        internal void Quit(long now,int lastMile)
        {
            UpdateTime(now,lastMile);
            State = States.Recall;
            Game.CoService.RunCo(ReturnFromAdventure());

            IEnumerator ReturnFromAdventure()
            {
                yield return new WaitUntil(() => Mode == Modes.Polling);
                if (ServiceCoId != 0) Game.CoService.StopCo(ServiceCoId);
                Game.MessagingManager.SendParams(EventString.Dizi_Adv_EventMessage, Dizi.Guid, $"{Dizi.Name}回程中...",
                    false);
                yield return new WaitForSeconds(JourneyReturnSec);
                State = States.None;
                Game.MessagingManager.SendParams(EventString.Dizi_Adv_EventMessage, Dizi.Guid, $"{Dizi.Name}已回到山门!",
                    true);
                Game.MessagingManager.Send(EventString.Dizi_Adv_End, Dizi.Guid);
            }
        }
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