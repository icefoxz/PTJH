using System;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using UnityEngine;

namespace Server.Configs.Adventures
{
    [CreateAssetMenu(fileName = "id_调整事件名", menuName = "事件/调整事件")]
    internal class AdjustEventSo : AdvEventSoBase
    {
        [SerializeField] private string 事件名;
        [SerializeField] private AdvEventSoBase 下个事件;
        [TextArea][SerializeField] private string 事件描述;
        [SerializeField] private Adjustment 调整;
        [SerializeField] private ConAdjustment[] 状态;

        public override string Name => 事件名;
        private string Brief => 事件描述;
        private AdvEventSoBase NextEvent => 下个事件;
        private Adjustment Adjust => 调整;
        private ConAdjustment[] ConFields => 状态;

        public override void EventInvoke(IAdvEventArg arg)
        {
            var nextEvent = NextEvent;
            var list = new List<string>
            {
                Brief,
                Adjust.Set(arg.Adjustment)
            };
            var adjustments = ConFields.Select(a => a.Set(arg.Adjustment, this)).ToArray();
            InvokeAdjustmentEvent(adjustments);
            list.AddRange(adjustments);
            var messages = list.Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => string.Format(s, arg.DiziName))
                .ToArray();
            if (messages.Any()) OnLogsTrigger?.Invoke(messages);
            OnNextEvent?.Invoke(nextEvent);
        }

        public override event Action<IAdvEvent> OnNextEvent;
        public override IAdvEvent[] AllEvents => new IAdvEvent[] { NextEvent };
        public override AdvTypes AdvType => AdvTypes.Adjust;
        public override event Action<string[]> OnLogsTrigger;


        [Serializable] private class ConAdjustment
        {
            private const string SilverString = "俸禄";
            private const string FoodString = "食物";
            private const string ConditionString = "精神";
            private const string InjuryString = "外伤";
            private const string InnerString = "内伤";

            private string GetString(Kinds kind) => kind switch
            {
                Kinds.Silver => SilverString,
                Kinds.Food => FoodString,
                Kinds.Condition => ConditionString,
                Kinds.Injury => InjuryString,
                Kinds.Inner => InnerString,
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };

            private bool RenameElement()
            {
                switch (Kind)
                {
                    case Kinds.Silver:
                    case Kinds.Food:
                    case Kinds.Condition:
                    case Kinds.Injury:
                    case Kinds.Inner:
                        break;
                    default:
                        _name = string.Empty;
                        return true;
                }
                _name = $"{GetString(Kind)}:{Value}{(Percentage ? "%" : string.Empty)}";
                return true;
            }

            private enum Kinds
            {
                [InspectorName(SilverString)] Silver = 1,
                [InspectorName(FoodString)] Food = 2,
                [InspectorName(ConditionString)] Condition = 3,
                [InspectorName(InjuryString)] Injury = 4,
                [InspectorName(InnerString)] Inner = 5,
            }

            [ConditionalField(true, nameof(RenameElement))] [ReadOnly] [SerializeField] private string _name;

            [SerializeField] private Kinds 类型;
            [SerializeField] private int 值;
            [SerializeField] private bool 百分比;

            private Kinds Kind => 类型;
            private int Value => 值;
            private bool Percentage => 百分比;

            public string Set(IAdjustment adj, AdjustEventSo so)
            {
                var adjType = Kind switch
                {
                    Kinds.Silver => IAdjustment.Types.Silver,
                    Kinds.Food => IAdjustment.Types.Food,
                    Kinds.Condition => IAdjustment.Types.Emotion,
                    Kinds.Injury => IAdjustment.Types.Injury,
                    Kinds.Inner => IAdjustment.Types.Inner,
                    _ => throw new ArgumentOutOfRangeException($"{so.Name} 存在未知类型 = {Kind}, 请检查配置!")
                };
                return adj.Set(adjType, Value, Percentage);
            }
        }

        [Serializable] public class Adjustment
        {
            [SerializeField] private bool 调整弟子体力;
            [ConditionalField(nameof(调整弟子体力))][SerializeField] private int 弟子体力;
            [ConditionalField(nameof(调整弟子体力))][SerializeField] private bool 百分比;

            private bool AdjustStamina => 调整弟子体力;

            private int Stamina => 弟子体力;
            private bool Percentage => 百分比;
            public string Set(IAdjustment adj) => AdjustStamina 
                ? adj.Set(IAdjustment.Types.Stamina, Stamina, Percentage) 
                : string.Empty;
        }
    }
}