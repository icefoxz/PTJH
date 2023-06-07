using System;
using System.Collections.Generic;
using Server.Configs.Battles;
using UnityEngine;

// 战斗buff的基类
public abstract class CombatBuff: Buff<DiziCombatUnit> 
{
    /// <summary>
    /// 回合当前数据, 一般用于静态数据参考. 如果要用当前数据可以直接<see cref="CombatBuff.Caster"/>和<see cref="CombatBuff.Target"/>,<br/>
    /// 但是这俩个数据有可能会是null值
    /// </summary>
    protected CombatArgs Args { get; }
    // 是否是回合结束时消耗
    private bool IsRoundEndDepletion { get; }
    protected CombatBuff(DiziCombatUnit caster, IBuffHandler<DiziCombatUnit> buffHandler, CombatArgs args, int lasting = 1) : base(caster,
        buffHandler, lasting)
    {
        Args = args;
        IsRoundEndDepletion = lasting > 0;
    }

    protected override void OnApplyBuff() => Target.AddBuff(this);
    protected override void OnRemoveBuff() => Target.RemoveBuff(this);

    /// <summary>
    /// buff对值的直接增益或减益性的功能
    /// </summary>
    /// <param name="eventName"></param>
    /// <returns></returns>
    public abstract float GetEffectValue(string eventName);

    /// <summary>
    /// 回合结束时执行并根据<see cref="IsRoundEndDepletion"/>来决定是否自动递减回合持久数
    /// </summary>
    /// <param name="units"></param>
    public override void OnRoundEndTrigger(List<DiziCombatUnit> units)
    {
        if (IsRoundEndDepletion)
            SubLasting();
    }
}

/// <summary>
/// 基础的效果Buff,一般用于增加属性,并且不实现自动移除buff的方法 
/// </summary>
public class EffectBuff : CombatBuff
{
    protected IEffectBuffConfig Config { get; }
    public override string TypeId { get; }

    public EffectBuff(string name, DiziCombatUnit caster, IBuffHandler<DiziCombatUnit> buffHandler,
        CombatArgs args, IEffectBuffConfig config) : base(caster, buffHandler, args, config.Lasting)
    {
        Config = config;
        TypeId = name;
    }



    public override void OnRoundStartTrigger(List<DiziCombatUnit> units)
    {
    }
    public override void OnCombatStartTrigger(List<DiziCombatUnit> units)
    {
    }

    public override float GetEffectValue(string eventName) => Config.GetStackingValue(eventName, Target);
    public override string ToString() => "效果Buff";
}

// 回合触发buff
public abstract class RoundEffectBuff : EffectBuff
{
    /// <summary>
    /// 触发模式
    /// </summary>
    public enum EffectModes
    {
        /// <summary>
        /// 每一回合
        /// </summary>
        [InspectorName("每回合")] EveryRound,

        /// <summary>
        /// 指定单个回合
        /// </summary>
        [InspectorName("指定回合")] SpecificRound,

        /// <summary>
        /// 跳过回合后执行
        /// </summary>
        [InspectorName("每跳过n回合")] SkipRound,

        /// <summary>
        /// 先执行后冷却
        /// </summary>
        [InspectorName("回合冷却")] CooldownRound
    }

    protected EffectModes EffectMode { get; }
    public bool IsTriggered => EffectMode switch
    {
        EffectModes.EveryRound => true,
        EffectModes.SpecificRound when _roundCount == 1 => true,
        EffectModes.SkipRound when _roundCount == 1 => true,
        EffectModes.CooldownRound when _roundCount == 0 => true,
        _ => false
    };
    protected int RoundCount { get; set; }
    private int _roundCount;

    protected RoundEffectBuff(string name, DiziCombatUnit caster, IBuffHandler<DiziCombatUnit> buffHandler,
        CombatArgs args, IEffectBuffConfig config) : base(name, caster, buffHandler, args, config)
    {
        EffectMode = config.Mode;
        RoundCount = config.RoundCount;
        switch (EffectMode)
        {
            case EffectModes.EveryRound:
                break;
            case EffectModes.SpecificRound:
            case EffectModes.SkipRound:
                _roundCount = RoundCount;
                break;
            case EffectModes.CooldownRound:
                _roundCount = 0;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// 根据触发模式实现触发
    /// </summary>
    /// <param name="onTriggered"></param>
    protected void TriggerCountDown(Action onTriggered)
    {
        if (IsTriggered)
        {
            if (EffectMode == EffectModes.SpecificRound)// 指定回合触发后就不会再触发了
            {
                onTriggered();
                _roundCount--;
                return;
            }
            TriggerActionAndResetCount();// 其他模式触发后都会重置回合数
        }
        else _roundCount--;

        if (_roundCount < -1) _roundCount = -1;

        void TriggerActionAndResetCount()
        {
            onTriggered();
            _roundCount = RoundCount;
        }
    }
}