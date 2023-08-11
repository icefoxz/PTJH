using System;
using System.Collections.Generic;
using System.Linq;
using AOT.Core;
using AOT.Core.Dizi;
using AOT.Utls;
using GameClient.Modules.DiziM;
using UnityEngine;
using UnityEngine.Analytics;

namespace GameClient.SoScripts.Adventures
{
    /// <summary>
    /// 条款
    /// </summary>
    public interface IIClause
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
        ///// <summary>
        /// 当前武功
        /// </summary>
        //ICombatSkill CombatSkill { get; }
        ///// <summary>
        /// 当前内功
        /// </summary>
        //IForceSkill ForceSkill { get; }
        ///// <summary>
        /// 当前轻功
        /// </summary>
        //IDodgeSkill DodgeSkill { get; }
        IDiziEquipment Equipment { get; }
        Gender Gender { get; }
        int Level { get; }
        /// <summary>
        /// 战力
        /// </summary>
        int Power { get; }
        /// <summary>
        /// 当前力量
        /// </summary>
        int Strength { get; }
        /// <summary>
        /// 当前敏捷
        /// </summary>
        int Agility { get; }
        /// <summary>
        /// 当前血量
        /// </summary>
        int Hp { get; }
        /// <summary>
        /// 当前内力
        /// </summary>
        int Mp { get; }
        /// <summary>
        /// 品阶
        /// </summary>
        int Grade { get; }
        IEnumerable<IGameItem> Items { get; }
    }

    [CreateAssetMenu(fileName = "id_条件事件名", menuName = "状态玩法/事件/条件事件")]
    internal class TermEventSo : AdvEventSoBase
    {
        [SerializeField] private string 事件名;
        [SerializeField] private TermField.Modes _mode;
        [SerializeField] private NoTermField 无条件事件;
        [SerializeField] private TermField[] 条件;

        public override string Name => 事件名;

        protected override IAdvEvent OnEventInvoke(IAdvEventArg arg)
        {
            var nextEvent = _mode switch
            {
                TermField.Modes.First => GetFirstInTermEvent(arg.Term),
                TermField.Modes.Random => GetInTermEvents(arg.Term).RandomPick(),
                _ => throw new ArgumentOutOfRangeException()
            };
            return nextEvent;
        }

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