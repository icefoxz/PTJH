using System;
using System.Collections.Generic;
using _GameClient.Models;
using Server.Configs.Adventures;
using Utls;

namespace Server.Controllers
{
    public class DiziAdvController : IGameController
    {
        private Config.Adventure Config { get; }
        private int EventLogSecs => Config.AdventureCfg.EventLogSecs;
        private int JourneyReturnSec => Config.AdventureCfg.AdvMap.JourneyReturnSec;
        private int MinuteInMile => Config.AdventureCfg.MinuteInMile;
        private AdventureMapSo Map { get; }

        private Faction Faction => Game.World.Faction;
        internal DiziAdvController(Config.Adventure config)
        {
            Config = config;
            Map = config.AdventureCfg.AdvMap;
        }

        public void AdventureStart(string guid)
        {
            var dizi = Faction.DiziMap[guid];
            dizi.StartAdventure(SysTime.UnixNow, JourneyReturnSec, EventLogSecs);
            Game.MessagingManager.Send(EventString.Dizi_Adv_Start, guid);
        }

        public void AdventureRecall(string guid)
        {
            var now = SysTime.UnixNow;
            var dizi = Faction.DiziMap[guid];
            var lastMile = CheckMile(dizi.Guid);
            dizi.QuitAdventure(now, lastMile);
        }

        public int CheckMile(string diziGuid)
        {
            var dizi = Faction.DiziMap[diziGuid];
            var lastMile = dizi.Adventure.LastMile;
            var lastUpdate = dizi.Adventure.LastUpdate;
            var now = SysTime.UnixNow;
            var miles = GetMiles(now, lastUpdate);
            var totalMiles = lastMile + miles;
            var messages = GetStoryMessages(lastMile, totalMiles, dizi);
            dizi.Adventure.OnMessageLog(now, totalMiles, messages);
            return totalMiles;
        }

        //获取故事信息
        private IReadOnlyList<string> GetStoryMessages(int lastMile, int totalMiles, Dizi dizi)
        {
            var places = Map.PickMajorPlace(lastMile, totalMiles);//根据当前路段找出故事地点
            var messages = new List<string>();
            for (int i = 0; i < places.Length; i++)//为每一个故事地点获取一个故事
            {
                var place = places[i];
                var story = place.WeighPickStory();//根据权重随机故事
                var player = new StoryPlayer(story);
                messages.AddRange(player.Start(dizi));//获取当前的故事信息
            }
            return messages;
        }

        //计算出间隔里数
        private int GetMiles(long now, long lastUpdate)
        {
            var millisecondInterval = now - lastUpdate;
            if (millisecondInterval < 0)
                throw new InvalidOperationException(
                    $"Interval ={millisecondInterval}! last = {lastUpdate}, now = {now},");
            //如果有马匹改变里数, 先计算马匹并叠加+里数和减去-时间. 剩余时间才与实际里数时间相乘
            //由于使用中物品目前只有马匹, 但使用中状态剩余多少百分比还是有必要记录, 并且是一并记录使用那个一物品
            var timeSpan = SysTime.MillisecondsToTimeSpan(millisecondInterval);
            var miles = (int)(MinuteInMile * timeSpan.TotalMinutes);
            return miles;
        }

        internal record StoryPlayer
        {
            private const int RecursiveLimit = 9999;
            private int _recursiveIndex = 0;
            private List<string> _messages;
            public IReadOnlyCollection<string> Messages => _messages;

            private IAdvStory Story { get; }
            private IAdvEvent CurrentEvent { get; set; }

            public StoryPlayer(IAdvStory story)
            {
                Story = story;
            }

            public string[] Start(Dizi dizi)
            {
                while (CurrentEvent!=null && CurrentEvent.AdvType != AdvTypes.Quit)
                {
                    ProcessEvent(Story.StartAdvEvent, dizi);
                    _recursiveIndex++;
                    if (_recursiveIndex >= RecursiveLimit)
                        throw new StackOverflowException($"故事{Story.Name} 死循环!检查其中事件{CurrentEvent.name}");
                }
                return _messages.ToArray();
            }

            private void ProcessEvent(IAdvEvent advEvent,Dizi dizi)
            {
                advEvent.OnNextEvent += OnNextEvent;
                advEvent.OnLogsTrigger += OnLogsTrigger;
                //advEvent.EventInvoke(dizi);
            }

            private void OnLogsTrigger(string[] logs)
            {
                CurrentEvent.OnLogsTrigger -= OnLogsTrigger;
                _messages.AddRange(logs);
            }

            private void OnNextEvent(IAdvEvent advEvent)
            {
                CurrentEvent.OnNextEvent -= OnNextEvent;
                CurrentEvent = advEvent;
            }
        }
    }
}