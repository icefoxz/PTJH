using System;
using System.Linq;
using System.Text;
using BattleM;
using Core;
using MyBox;
using Server.Configs.Adventures;
using UnityEngine;

namespace Server.Controllers
{
    public interface ITestAdventureController
    {
        void OnEventInvoke(int mapId, int index);
        void OnStartMapEvent(int mapId);
    }

    public class TestAdventureController : ITestAdventureController
    {
        private AdvConfig Config { get; }
        internal TestAdventureController(AdvConfig config)
        {
            Config = config;
        }

        public void StartAdventureMaps()
        {
            var list = Config.Maps.Select(InstanceMapData).ToList();
            Game.MessagingManager.Send(EventString.Test_AdventureMap, list);

            Story InstanceMapData(AdvStorySo storySo)
            {
                var allEvents = storySo.AllEvents.Select(e => ((AdvEventSoBase)e).name).ToArray();
                return new Story
                {
                    Id = storySo.Id,
                    Name = storySo.Name,
                    StartEventName = storySo.StartAdvEvent.ToString(),
                    AllEvents = allEvents
                };
            }

        }
        public void OnEventInvoke(int mapId, int index)
        {
            var map = Config.Maps.First(m => m.Id == mapId);
            var advEvent = map.AllAdvEvents[index];
            var eventData = InstanceEventData(map, advEvent);
            Game.MessagingManager.Send(EventString.Test_AdvEventInvoke, eventData);
        }

        public void OnStartMapEvent(int mapId)
        {
            var map = Config.Maps.First(m => m.Id == mapId);
            var eventData = InstanceEventData(map, map.StartAdvEvent);
            Game.MessagingManager.Send(EventString.Test_AdvEventInvoke, eventData);
        }

        public class Story
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string StartEventName { get; set; }
            public string[] AllEvents { get; set; }
        }
        public class AdvEvent
        {
            public string Name { get; set; }
            public int StoryId { get; set; }
            public AdvTypes AdvType { get; set; }
            public int[] NextIndexes { get; set; }

            internal AdvEvent(AdvStorySo storySo, AdvEventSoBase so)
            {
                Name = so.name;
                StoryId = storySo.Id;
                AdvType = so.AdvType;
                NextIndexes = so.AllEvents.Select(e => storySo.AllEvents.IndexOfItem(e)).ToArray();
            }

            public AdvEvent()
            {
                
            }
        }
        public class OptionEvent : AdvEvent
        {
            public string[] Options { get; set; }
            public string Story { get; set; }

            internal OptionEvent(AdvStorySo storySo, OptionEventSo so) : base(storySo, so)
            {
                Options = so.GetOptions;
                Story = so.Story;
            }
            internal OptionEvent(AdvStorySo storySo, PoolEventSo so) : base(storySo, so)
            {
                Options = so.AllEvents.Cast<AdvEventSoBase>().Select(e=>e.name).ToArray();
                Story = $"权重事件：{so.name}";
            }
            internal OptionEvent(AdvStorySo storySo, TermEventSo so,ITerm term) : base(storySo, so)
            {
                Options = so.AllEvents.Cast<AdvEventSoBase>().Select(e=>e.name).ToArray();
                var sb = new StringBuilder($"状态：{term}\n");
                sb = sb.Append("符合条件事件：\n");
                foreach (var (title, advEvent) in so.GetInTermEventsWithTitle(term))
                    sb.Append($"{title}-{((AdvEventSoBase)advEvent).name}\n");
                Story = sb.ToString();
            }

            public OptionEvent()
            {
                
            }
        }
        public class BriefEvent : AdvEvent
        {
            public string Text { get; set; }

            internal BriefEvent(AdvStorySo storySo, AdvTextEventSo so) : base(storySo, so)
            {
                Text = so.Text;
            }

            public BriefEvent()
            {

            }
        }

        public class DialogEvent : AdvEvent
        {
            //public int[] Ids { get; set; }
            public string[] Names { get; set; }
            public string[] Messages { get; set; }

            internal DialogEvent(AdvStorySo storySo, AdvDialogEventSo so) : base(storySo, so)
            {
                var array = so.GetDialogue.Select(d => (d.name, d.message)).ToArray();
                //Ids = new int[array.Length];
                Names = new string[array.Length];
                Messages = new string[array.Length];
                for (var i = 0; i < array.Length; i++)
                {
                    var (name, message) = array[i];
                    //Ids[i] = id;
                    Names[i] = name;
                    Messages[i] = message;
                }
            }

            public DialogEvent()
            {

            }
        }

        public class BattleEvent : AdvEvent
        {
            /// <summary>
            /// [0].Win<br/>[1].Lose<br/>[2].Kill<br/>[3].Escape
            /// </summary>
            public string[] ResultEvents { get; set; }
            internal BattleEvent(AdvStorySo storySo, BattleEventSo so) : base(storySo, so)
            {
                ResultEvents = so.AllEvents.Cast<AdvEventSoBase>().Select(c => c.name).ToArray();
            }

            public BattleEvent()
            {
                
            }
        }
        public class RewardEvent : AdvEvent
        {
            public string[] Rewards { get; set; }
            internal RewardEvent(AdvStorySo storySo, RewardEventSo so) : base(storySo, so)
            {
                var r = so.Reward;
                Rewards = r.AllItems.Select(i => $"{i.Name} x{i.Amount}").ToArray();
            }

            public RewardEvent()
            {
                
            }
        }

        private AdvEvent InstanceEventData(AdvStorySo storySo,IAdvEvent advEvent)
        {
            return GetEventData(storySo, advEvent);

            AdvEvent GetEventData(AdvStorySo mSo, IAdvEvent aEvent)
            {
                AdvEvent advEventData = aEvent.AdvType switch
                {
                    AdvTypes.Quit => new AdvEvent(mSo, aEvent as AdvQuitEventSo),
                    AdvTypes.Story => new BriefEvent(mSo, aEvent as AdvTextEventSo),
                    AdvTypes.Dialog => new DialogEvent(mSo, aEvent as AdvDialogEventSo),
                    AdvTypes.Pool => new OptionEvent(mSo, advEvent as PoolEventSo),
                    AdvTypes.Option => new OptionEvent(mSo, aEvent as OptionEventSo),
                    AdvTypes.Battle => new BattleEvent(mSo, aEvent as BattleEventSo),
                    AdvTypes.Term => new OptionEvent(mSo, advEvent as TermEventSo, new TestTerm()),
                    AdvTypes.Reward => new RewardEvent(mSo, aEvent as RewardEventSo),
                    _ => throw new ArgumentOutOfRangeException($"未支持事件={advEvent.AdvType}!")
                };
                return advEventData;
            }
        }
        private class TestTerm : ITerm
        {
            public override string ToString()=> Stamina.ToString();
            public IConditionValue Stamina => new ConValue(100);
            public IConditionValue Silver { get; }
            public IConditionValue Food { get; }
            public IConditionValue Emotion { get; }
            public IConditionValue Injury { get; }
            public IConditionValue Inner { get; }
            public IWeapon Weapon { get; }
            public IArmor Armor { get; }
            public int Level { get; }
            public int Power { get; }
            public IGameItem[] Items { get; }
        }

        [Serializable] internal class AdvConfig
        {
            [SerializeField] private AdvStorySo[] 地图;
            public AdvStorySo[] Maps => 地图;
        }
    }

    public class UnitStatus
    {
        public ConValue Hp { get; set; }
        public ConValue Mp { get; set; }

        public UnitStatus()
        {

        }

        public UnitStatus(int hp, int tp, int mp)
        {
            Hp = new ConValue(hp);
            Mp = new ConValue(mp);
        }

        public ICombatStatus GetCombatStatus() => CombatStatus.Instance(
            Hp.Value, Hp.Max, Hp.Fix,
            Mp.Value, Mp.Max, Mp.Fix);

        public void Clone(ICombatStatus c)
        {
            Hp.Clone(c.Hp);
            Mp.Clone(c.Mp);
        }
        public override string ToString() => $"Hp{Hp},Mp{Mp}";
    }
}
