using Server.Configs.Battles;
using System;
using System.Collections.Generic;

/// <summary>
/// 战斗基本接口
/// </summary>
public interface ICombatSet
{
    /// <summary>
    /// 重击触发率
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    float GetHardRate(CombatArgs arg);
    /// <summary>
    /// 重击倍率
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    float GetHardDamageRatioAddOn(CombatArgs arg);
    /// <summary>
    /// 会心触发率
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    float GetCriticalRate(CombatArgs arg);
    /// <summary>
    /// 会心倍率
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    float GetCriticalDamageRatioAddOn(CombatArgs arg);
    /// <summary>
    /// 内力伤害
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    float GetMpUses(CombatArgs arg);
    /// <summary>
    /// 内力伤害比率加成
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    float GetMpDamageConvertRateAddOn(CombatArgs arg);
    /// <summary>
    /// 内力护甲的占比, 正数为值, 负数为百分比
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    float GetMpArmorRate(CombatArgs arg);
    /// <summary>
    /// 内力护甲转化率加成
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    float GetMpArmorConvertRateAddOn(CombatArgs arg);
    /// <summary>
    /// 闪避率
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    float GetDodgeRate(CombatArgs arg);
    IEnumerable<CombatBuff> GetSelfBuffs(DiziCombatUnit caster, int round, BuffManager<DiziCombatUnit> buffManager);
    IEnumerable<CombatBuff> GetTargetBuffs(DiziCombatUnit target, DiziCombatUnit caster,CombatArgs args, int round, BuffManager<DiziCombatUnit> buffManager);
}

public static class CombatExtension
{
    /// <summary>
    /// 综合所有战斗设定的结果
    /// </summary>
    /// <param name="combats"></param>
    /// <returns></returns>
    public static ICombatSet Combine(this IEnumerable<ICombatSet> combats)
    {
        var hardRate = new List<Func<CombatArgs, float>>();
        var hardDamageRatio = new List<Func<CombatArgs, float>>();
        var criticalRate = new List<Func<CombatArgs, float>>();
        var criticalMultiplier = new List<Func<CombatArgs, float>>();
        var mpUses = new List<Func<CombatArgs, float>>();
        var mpDamageCovertRateAddOn = new List<Func<CombatArgs, float>>();
        var mpArmorRate = new List<Func<CombatArgs,float>>();
        var mpArmorConvertRateAddon = new List<Func<CombatArgs, float>>();
        var dodgeRate = new List<Func<CombatArgs, float>>();
        var selfBuffs = new List<Func<DiziCombatUnit, int, BuffManager<DiziCombatUnit>, IEnumerable<CombatBuff>>>();
        var targetBuffs = new List<Func<DiziCombatUnit, DiziCombatUnit, CombatArgs, int, BuffManager<DiziCombatUnit>, IEnumerable<CombatBuff>>>();
        foreach (var field in combats)
        {
            hardRate.Add(field.GetHardRate);
            hardDamageRatio.Add(field.GetHardDamageRatioAddOn);
            criticalRate.Add(field.GetCriticalRate);
            criticalMultiplier.Add(field.GetCriticalDamageRatioAddOn);
            mpUses.Add(field.GetMpUses);
            mpDamageCovertRateAddOn.Add(field.GetMpDamageConvertRateAddOn);
            mpArmorRate.Add(field.GetMpArmorRate);
            mpArmorConvertRateAddon.Add(field.GetMpArmorConvertRateAddOn);
            dodgeRate.Add(field.GetDodgeRate);
            selfBuffs.Add(field.GetSelfBuffs);
            targetBuffs.Add(field.GetTargetBuffs);
        }
        return new CombatSet(
            hardRate: hardRate, 
            hardDamageRatio: hardDamageRatio, 
            criticalRate: criticalRate,
            criticalDamageRatio: criticalMultiplier,
            mpUses: mpUses, 
            mpDamageCovertRateAddOn: mpDamageCovertRateAddOn,
            mpArmorRate: mpArmorRate, 
            mpArmorConvertRateAddOn: mpArmorConvertRateAddon,
            dodgeRate: dodgeRate,
            selfBuffs: selfBuffs,
            targetBuffs: targetBuffs);
    }
}