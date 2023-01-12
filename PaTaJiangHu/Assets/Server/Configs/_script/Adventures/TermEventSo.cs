using System;
using System.Collections.Generic;
using System.Linq;
using BattleM;
using Core;
using UnityEngine;
using Utls;

namespace Server.Configs.Adventures
{
    /// <summary>
    /// 条款
    /// </summary>
    public interface IIClause<in T>
    {
        bool IsInTerm(ITerm term);
    }

    /// <summary>
    /// 条件
    /// </summary>
    public interface ITerm
    {
        IConditionValue Stamina { get; }
        IConditionValue Silver { get; }
        IConditionValue Food { get; }
        IConditionValue Emotion { get; }
        IConditionValue Injury { get; }
        IConditionValue Inner { get; }
        IWeapon Weapon { get; }
        IArmor Armor { get; }
        int Level { get; }
        int Power { get; }
        IGameItem[] Items { get; }
    }

    [CreateAssetMenu(fileName = "id_条件事件名", menuName = "事件/条件事件")]
    internal class TermEventSo : AdvEventSoBase
    {
        [SerializeField] private string 事件名;
        [SerializeField] private TermField.Modes _mode;
        [SerializeField] private NoTermField 无条件事件;
        [SerializeField] private TermField[] 条件;

        public override string Name => 事件名;
        public override void EventInvoke(IAdvEventArg arg)
        {
            var nextEvent = _mode switch
            {
                TermField.Modes.First => GetFirstInTermEvent(arg.Term),
                TermField.Modes.Random => GetInTermEvents(arg.Term).RandomPick(),
                _ => throw new ArgumentOutOfRangeException()
            };
            OnNextEvent?.Invoke(nextEvent);
        }

        public override event Action<IAdvEvent> OnNextEvent;

        /// <summary>
        /// NoTerm = 0 index
        /// </summary>
        public override IAdvEvent[] AllEvents
        {
            get
            {
                var list = new List<IAdvEvent>(TermFields.Select(t => t.Event));
                list.Insert(0, NoTermEvent.Event);
                return list.ToArray();
            }
        }
        public override AdvTypes AdvType => AdvTypes.Term;
        public override event Action<string[]> OnLogsTrigger;//条件事件不触发
        private TermField[] TermFields => 条件;
        private NoTermField NoTermEvent => 无条件事件;
        public (string title, IAdvEvent advEvent)[] GetInTermEventsWithTitle(ITerm term)
        {
            var inTermEvents = TermFields
                .Where(s => s.InTerm(term))
                .Select(c => (c.Title, c.Event)).ToList();
            inTermEvents.Add((NoTermEvent.Title, NoTermEvent.Event));
            return inTermEvents.ToArray();
        }

        public IAdvEvent GetFirstInTermEvent(ITerm term)
        {
            var advEvent = TermFields.FirstOrDefault(s => s.InTerm(term));
            return advEvent == null ? NoTermEvent.Event : advEvent.Event;
        }

        public IAdvEvent[] GetInTermEvents(ITerm term)
        {
            var inTermEvents = TermFields
                .Where(s => s.InTerm(term))
                .Select(c => c.Event).ToList();
            inTermEvents.Add(NoTermEvent.Event);
            return inTermEvents.ToArray();
        }
    }
}