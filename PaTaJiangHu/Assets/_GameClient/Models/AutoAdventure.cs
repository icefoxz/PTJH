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
            Process,
            Return,
        }
        private List<string> _messageLog = new List<string>();
        private Queue<string> _printQueue;

        /// <summary>
        /// 回程秒数
        /// </summary>
        public int JourneyReturnSec { get; }
        public States State { get; private set; }
        //public TimeSpan TotalTimeSpan => Recorder.TotalTimeSpan();
        //public float[] MileMap => Recorder.MileMap.ToArray();
        //开始时间
        public long StartTime { get; }

        //上次更新时间
        public long LastUpdate { get; private set; }

        //当前里数
        public int LastMile { get; private set; }

        private IReadOnlyList<string> MessageLog => _messageLog;
        private DiziAdvController AdvController => Game.Controllers.Get<DiziAdvController>();
        private Dizi Dizi { get; }
        private DiziBag Bag { get; }
        private int MessageSecs { get; } //文本展示间隔(秒)
        private int AdventureCoId { get; set; } = -1; //历练CoroutineId
        private int MessageCoId { get; set; } = -1;//信息CoroutineId
        private int ReturnCoId { get; set; } = -1;//回归CoroutineId
        public Equipment Equipment { get; }

        public AutoAdventure(long startTime, int journeyReturnSec, int messageSecs, Dizi dizi)
        {
            MessageSecs = messageSecs;
            JourneyReturnSec = journeyReturnSec;
            StartTime = startTime;
            LastUpdate = startTime;
            Dizi = dizi;
            State = States.Process;
            PollingStoryPerSec();
        }

        //轮询控制器是否有故事发生
        private void PollingStoryPerSec()
        {
            AdventureCoId = Game.CoService.RunCo(RequestEverySec());

            IEnumerator RequestEverySec()
            {
                yield return new WaitForSeconds(1);
                AdvController.CheckMile(Dizi.Guid);
            }
        }

        //停止轮询
        private void StopPollingStory()
        {
            Game.CoService.StopCo(AdventureCoId);
            AdventureCoId = -1;
        }

        internal void StartStory(long now, int lasMile)
        {
            if (AdventureCoId < 0)
                throw new NotImplementedException($"AdvCoId = {AdventureCoId}");
            StopPollingStory();
            UpdateTime(now, lasMile);
        }

        private void UpdateTime(long now, int lasMile)
        {
            LastUpdate = now;
            LastMile = lasMile;
        }

        internal void OnMessageLog(long lasTick, int totalMiles, IReadOnlyList<string> messages)
        {
            UpdateTime(lasTick, totalMiles);
            if (MessageCoId > 0)//如果新故事进来,但旧故事未完成播放
            {
                _messageLog.AddRange(_printQueue);//直接加入信息列
                _printQueue = new Queue<string>(messages);//播放信息队列放入新故事
                Game.MessagingManager.SendParams(EventString.Dizi_Adv_MessagesUpdate, Dizi.Guid);
                return;
            }

            //如果播放队列已经空了
            _printQueue = new Queue<string>(messages);//播放信息队列放入新故事
            MessageCoId = Game.CoService.RunCo(PrintAdvLogMessage());

            IEnumerator PrintAdvLogMessage()
            {
                //循环播放直到播放队列空
                while (_printQueue.Count > 0)
                {
                    var message = _printQueue.Dequeue();
                    Game.MessagingManager.SendParams(EventString.Dizi_Adv_EventMessage, Dizi.Guid, message);
                    yield return new WaitForSeconds(MessageSecs);
                }
                Game.CoService.StopCo(MessageCoId);
                MessageCoId = -1;
            }
        }

        internal void Quit(long now,int lastMile)
        {
            UpdateTime(now,lastMile);
            State = States.Return;
            ReturnCoId = Game.CoService.RunCo(ReturnFromAdventure());

            IEnumerator ReturnFromAdventure()
            {
                yield return new WaitForSeconds(JourneyReturnSec);
                State = States.None;
                Game.CoService.StopCo(ReturnCoId);
            }
        }
    }


    public class Equipment
    {

    }
    public class DiziBag
    {
        private IGameItem[] _items;
        public IGameItem[] Items => _items;

        public DiziBag(int length)
        {
            _items = new IGameItem[length];
        }

        public DiziBag(IGameItem[] items)
        {
            _items = items;
        }

    }
}