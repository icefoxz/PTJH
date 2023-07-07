using System;
using System.Collections.Generic;
using GameClient.Args;

namespace GameClient.Modules.BattleM
{
    public interface IEffectBuffConfig
    {
        /// <summary>
        /// 触发模式
        /// </summary>
        RoundEffectBuff.EffectModes Mode { get; }
        /// <summary>
        /// 回合触发值
        /// </summary>
        int RoundCount { get; }
        /// <summary>
        /// 持续回合数, 如果-1则不自动移除buff
        /// </summary>
        int Lasting { get; }

        /// <summary>
        /// 获取叠加值
        /// </summary>
        /// <param name="event"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        float GetStackingValue(string @event, DiziCombatUnit target);
        /// <summary>
        /// 是否允许叠加, 一般在生成的时候决定是否允许同类的buff重复赋buff
        /// </summary>
        bool IsAllowStacking { get; }
        /// <summary>
        /// 触发时执行的方法
        /// </summary>
        /// <param name="target"></param>
        /// <param name="caster"></param>
        /// <param name="args"></param>
        /// <param name="buffManager"></param>
        /// <param name="units"></param>
        void OnTriggeredFunction(DiziCombatUnit target, DiziCombatUnit caster, CombatArgs args,
            BuffManager<DiziCombatUnit> buffManager, List<DiziCombatUnit> units);
    }

    /// <summary>
    /// 基础触发buff, 不提供方法, 仅提供属性, 子类实现其它扩展功能
    /// </summary>
    public class BuffRoundEffectByMode : RoundEffectBuff
    {
        public Combat.RoundTriggers RoundTriggers { get; } 
        protected BuffManager<DiziCombatUnit> BuffManager { get; }

        public BuffRoundEffectByMode(string name, DiziCombatUnit caster, BuffManager<DiziCombatUnit> buffManager,
            CombatArgs args, IEffectBuffConfig config, Combat.RoundTriggers roundTriggers)
            : base(name, caster, buffManager, args, config)
        {
            BuffManager = buffManager;
            RoundTriggers = roundTriggers;
        }

        public override void OnRoundStartTrigger(List<DiziCombatUnit> units)
        {
            if(RoundTriggers == Combat.RoundTriggers.RoundStart) TriggerCountDown(() => OnTriggered(units));
        }
        public override void OnRoundEndTrigger(List<DiziCombatUnit> units)
        {
            if(RoundTriggers == Combat.RoundTriggers.RoundEnd) TriggerCountDown(() => OnTriggered(units));
            base.OnRoundEndTrigger(units);
        }
        public override void OnCombatStartTrigger(List<DiziCombatUnit> units)
        {
            if (RoundTriggers == Combat.RoundTriggers.CombatStart) TriggerCountDown(() => OnTriggered(units));
        }

        public override float GetEffectValue(string eventName) => IsTriggered ? base.GetEffectValue(eventName) : 0;

        private void OnTriggered(List<DiziCombatUnit> units)
        {
            Config.OnTriggeredFunction(Target, Caster, Args, BuffManager, units);
        }

        public override string ToString()
        {
            var roundText = RoundTriggers switch
            {
                Combat.RoundTriggers.RoundStart => "回合开始",
                Combat.RoundTriggers.RoundEnd => "回合结束",
                Combat.RoundTriggers.CombatStart => "战斗开始",
                _ => "未知"
            };
            var effectText = EffectMode switch
            {
                EffectModes.EveryRound => "每回合",
                EffectModes.SpecificRound => $"回合_{RoundCount}",
                EffectModes.SkipRound => $"跳{RoundCount}回合",
                EffectModes.CooldownRound => $"冷却{RoundCount}回合",
                _ => throw new ArgumentOutOfRangeException()
            };
            return $"效果Buff_{roundText}_{effectText}";
        }

    }
}