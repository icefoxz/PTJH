using System;
using MyBox;
using UnityEngine;

namespace GameClient.SoScripts.Adventures
{
    [CreateAssetMenu(fileName = "id_战斗事件名", menuName = "状态玩法/副本/战斗事件")]
    internal class BattleEventSo : AdvEventSoBase
    {
        public enum Result
        {
            Win,
            Lose
        }
        public enum Finalized
        {
            Escaped,
            Exhausted
        }
        private enum Types
        {
            [InspectorName("试探")]Test,
            [InspectorName("决斗")]Duel,
        }

        [SerializeField] private string 事件名 = "战斗";
        [SerializeField] private Types 类型;
        [SerializeField] private AdvEventSoBase 胜利;
        [SerializeField] private AdvEventSoBase 战败;
        [ConditionalField(nameof(类型), false, Types.Test)] [SerializeField] private AdvEventSoBase 击杀;
        [ConditionalField(nameof(类型), false, Types.Test)] [SerializeField] private AdvEventSoBase 逃脱;

        public override string Name => 事件名;

        public override void EventInvoke(IAdvEventArg arg)
        {
            var nextEvent = arg.InteractionResult switch
            {
                0 => NextEvent(Result.Win, Finalized.Exhausted),
                1 => NextEvent(Result.Lose, Finalized.Exhausted),
                2 => NextEvent(Result.Win, Finalized.Escaped),
                3 => NextEvent(Result.Lose, Finalized.Escaped),
                _ => throw new ArgumentOutOfRangeException($"{nameof(arg.InteractionResult)}", arg.InteractionResult.ToString())
            };
            OnNextEvent?.Invoke(nextEvent);
        }

        public override event Action<IAdvEvent> OnNextEvent;

        /// <summary>
        /// [0].Win<br/>[1].Lose<br/>[2].Kill<br/>[3].Escape
        /// </summary>
        public override IAdvEvent[] AllEvents => Type switch
        {
            Types.Test => new[] { Win, Lose, Kill, Escaped },
            Types.Duel => new[] { Win, Lose },
            _ => throw new ArgumentOutOfRangeException()
        };
        public override AdvTypes AdvType => AdvTypes.Battle;
        public override event Action<string[]> OnLogsTrigger;

        private IAdvEvent NextEvent(Result result, Finalized finalize)
        {
            var next = result switch
            {
                Result.Win when
                    Type == Types.Test => finalize == Finalized.Escaped ? Win : Kill,
                Result.Win => Win,
                Result.Lose when
                    Type == Types.Test => finalize == Finalized.Escaped ? Escaped : Lose,
                Result.Lose => Lose,
                _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
            };
            return next;
        }
        private Types Type => 类型;
        private IAdvEvent Win => 胜利;
        private IAdvEvent Lose => 战败;
        private IAdvEvent Kill => 击杀;
        private IAdvEvent Escaped => 逃脱;
    }
}